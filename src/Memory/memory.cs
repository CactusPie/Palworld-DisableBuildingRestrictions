using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;
using static Memory.Imps;

namespace Memory
{
    /// <summary>
    /// Memory.dll class. Full documentation at https://github.com/erfg12/memory.dll/wiki
    /// Trimmed down to the absolute minimum in order to avoid detection by Antivirus software
    /// </summary>
    public sealed partial class Mem
    {
        public Proc MProc = new Proc();

        /// <summary>
        /// Open the PC game process with all security and access rights.
        /// </summary>
        /// <param name="pid">Use process name or process ID here.</param>
        /// <returns>Process opened successfully or failed.</returns>
        /// <param name="failReason">Show reason open process fails</param>
        private bool OpenProcess(int pid, out string failReason)
        {
            if (pid <= 0)
            {
                failReason = "OpenProcess given proc ID 0.";
                return false;
            }


            if (MProc.Process != null && MProc.Process.Id == pid)
            {
                failReason = "mProc.Process is null";
                return true;
            }

            try
            {
                MProc.Process = Process.GetProcessById(pid);

                if (MProc.Process != null && !MProc.Process.Responding)
                {
                    failReason = "Process is not responding or null.";
                    return false;
                }

                MProc.Handle = Imps.OpenProcess(0x1F0FFF, true, pid);

                try {
                    Process.EnterDebugMode();
                } catch (Win32Exception) {
                }

                if (MProc.Handle == IntPtr.Zero)
                {
                    var eCode = Marshal.GetLastWin32Error();
                    Process.LeaveDebugMode();
                    MProc = null;
                    failReason = "failed opening a handle to the target process(GetLastWin32ErrorCode: " + eCode + ")";
                    return false;
                }

                // Lets set the process to 64bit or not here (cuts down on api calls)
                MProc.Is64Bit = Environment.Is64BitOperatingSystem && (IsWow64Process(MProc.Handle, out bool retVal) && !retVal);

                MProc.MainModule = MProc.Process.MainModule;

                failReason = "";
                return true;
            }
            catch (Exception ex) {
                failReason = "OpenProcess has crashed. " + ex;
                return false;
            }
        }

        private UIntPtr VirtualQueryEx(IntPtr hProcess, UIntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer)
        {
            UIntPtr retVal;

            // TODO: Need to change this to only check once.
            if (MProc.Is64Bit || IntPtr.Size == 8)
            {
                // 64 bit
                var tmp64 = new MEMORY_BASIC_INFORMATION64();
                retVal = Native_VirtualQueryEx(hProcess, lpAddress, out tmp64, new UIntPtr((uint)Marshal.SizeOf(tmp64)));

                lpBuffer.BaseAddress = tmp64.BaseAddress;
                lpBuffer.AllocationBase = tmp64.AllocationBase;
                lpBuffer.AllocationProtect = tmp64.AllocationProtect;
                lpBuffer.RegionSize = (long)tmp64.RegionSize;
                lpBuffer.State = tmp64.State;
                lpBuffer.Protect = tmp64.Protect;
                lpBuffer.Type = tmp64.Type;

                return retVal;
            }
            var tmp32 = new MEMORY_BASIC_INFORMATION32();

            retVal = Native_VirtualQueryEx(hProcess, lpAddress, out tmp32, new UIntPtr((uint)Marshal.SizeOf(tmp32)));

            lpBuffer.BaseAddress = tmp32.BaseAddress;
            lpBuffer.AllocationBase = tmp32.AllocationBase;
            lpBuffer.AllocationProtect = tmp32.AllocationProtect;
            lpBuffer.RegionSize = tmp32.RegionSize;
            lpBuffer.State = tmp32.State;
            lpBuffer.Protect = tmp32.Protect;
            lpBuffer.Type = tmp32.Type;

            return retVal;
        }


        /// <summary>
        /// Open the PC game process with all security and access rights.
        /// </summary>
        /// <param name="proc">Use process name or process ID here.</param>
        /// <returns></returns>
        public bool OpenProcess(string proc)
        {
            return OpenProcess(GetProcIdFromName(proc), out string failReason);
        }

        /// <summary>
        /// Get the process ID number by process name.
        /// </summary>
        /// <param name="name">Example: "eqgame". Use task manager to find the name. Do not include .exe</param>
        /// <returns></returns>
        private static int GetProcIdFromName(string name) //new 1.0.2 function
        {
            Process[] processlist = Process.GetProcesses();

            if (name.ToLower().Contains(".exe"))
            {
                name = name.Replace(".exe", "");
            }

            if (name.ToLower().Contains(".bin")) // test
            {
                name = name.Replace(".bin", "");
            }

            foreach (Process theprocess in processlist)
            {
                if (theprocess.ProcessName.Equals(name, StringComparison.CurrentCultureIgnoreCase)) //find (name).exe in the process list (use task manager to find the name)
                {
                    return theprocess.Id;
                }
            }

            return 0; //if we fail to find it
        }

        /// <summary>
        /// Get code. If just the ini file name is given with no path, it will assume the file is next to the executable.
        /// </summary>
        /// <param name="name">label for address or code</param>
        /// <param name="iniFile">path and name of ini file</param>
        /// <returns></returns>
        private static string LoadCode(string name, string iniFile)
        {
            var returnCode = new StringBuilder(1024);
            uint readIniResult;

            if (!string.IsNullOrEmpty(iniFile))
            {
                if (File.Exists(iniFile))
                {
                    readIniResult = GetPrivateProfileString("codes", name, "", returnCode, (uint)returnCode.Capacity, iniFile);
                    //Debug.WriteLine("read_ini_result=" + read_ini_result); number of characters returned
                }
                else
                {
                    Debug.WriteLine("ERROR: ini file \"" + iniFile + "\" not found!");
                }
            }
            else
            {
                returnCode.Append(name);
            }

            return returnCode.ToString();
        }

        private static int LoadIntCode(string name, string path)
        {
            try
            {
                var intValue = Convert.ToInt32(LoadCode(name, path), 16);
                if (intValue >= 0)
                {
                    return intValue;
                }
                else
                {
                    return 0;
                }
            } catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Convert code from string to real address. If path is not blank, will pull from ini file.
        /// </summary>
        /// <param name="name">label in ini file or code</param>
        /// <param name="path">path to ini file (OPTIONAL)</param>
        /// <param name="size">size of address (default is 8)</param>
        /// <returns></returns>
        private UIntPtr GetCode(string name, string path = "", int size = 8)
        {
            var theCode = "";
            if (MProc == null)
            {
                return UIntPtr.Zero;
            }

            if (MProc.Is64Bit)
            {
                //Debug.WriteLine("Changing to 64bit code...");
                if (size == 8)
                {
                    size = 16; //change to 64bit
                }

                return Get64BitCode(name, path, size); //jump over to 64bit code grab
            }

            if (!string.IsNullOrEmpty(path))
            {
                theCode = LoadCode(name, path);
            }
            else
            {
                theCode = name;
            }

            if (string.IsNullOrEmpty(theCode))
            {
                return UIntPtr.Zero;
            }

            // remove spaces
            if (theCode.Contains(" "))
            {
                theCode = theCode.Replace(" ", string.Empty);
            }

            if (!theCode.Contains("+") && !theCode.Contains(","))
            {
                try
                {
                    return new UIntPtr(Convert.ToUInt32(theCode, 16));
                } catch
                {
                    Console.WriteLine("Error in GetCode(). Failed to read address " + theCode);
                    return UIntPtr.Zero;
                }
            }

            string newOffsets = theCode;

            if (theCode.Contains("+"))
            {
                newOffsets = theCode.Substring(theCode.IndexOf('+') + 1);
            }

            var memoryAddress = new byte[size];

            if (newOffsets.Contains(','))
            {
                var offsetsList = new List<int>();

                string[] newerOffsets = newOffsets.Split(',');
                foreach (string oldOffsets in newerOffsets)
                {
                    string test = oldOffsets;
                    if (oldOffsets.Contains("0x"))
                    {
                        test = oldOffsets.Replace("0x","");
                    }

                    var preParse = 0;
                    if (!oldOffsets.Contains("-"))
                    {
                        preParse = int.Parse(test, NumberStyles.AllowHexSpecifier);
                    }
                    else
                    {
                        test = test.Replace("-", "");
                        preParse = int.Parse(test, NumberStyles.AllowHexSpecifier);
                        preParse = preParse * -1;
                    }
                    offsetsList.Add(preParse);
                }
                int[] offsets = offsetsList.ToArray();

                if (theCode.Contains("base") || theCode.Contains("main"))
                {
                    ReadProcessMemory(MProc.Handle, (UIntPtr)((int)MProc.MainModule.BaseAddress + offsets[0]), memoryAddress, (UIntPtr)size, IntPtr.Zero);
                }
                else if (!theCode.Contains("base") && !theCode.Contains("main") && theCode.Contains("+"))
                {
                    string[] moduleName = theCode.Split('+');
                    IntPtr altModule = IntPtr.Zero;
                    if (!moduleName[0].ToLower().Contains(".dll") && !moduleName[0].ToLower().Contains(".exe") && !moduleName[0].ToLower().Contains(".bin"))
                    {
                        string theAddr = moduleName[0];
                        if (theAddr.Contains("0x"))
                        {
                            theAddr = theAddr.Replace("0x", "");
                        }

                        altModule = (IntPtr)int.Parse(theAddr, NumberStyles.HexNumber);
                    }
                    else
                    {
                        try
                        {
                            altModule = GetModuleAddressByName(moduleName[0]);
                        }
                        catch
                        {
                            Debug.WriteLine("Module " + moduleName[0] + " was not found in module list!");
                            //Debug.WriteLine("Modules: " + string.Join(",", mProc.Modules));
                        }
                    }
                    ReadProcessMemory(MProc.Handle, (UIntPtr)((int)altModule + offsets[0]), memoryAddress, (UIntPtr)size, IntPtr.Zero);
                }
                else
                {
                    ReadProcessMemory(MProc.Handle, (UIntPtr)(offsets[0]), memoryAddress, (UIntPtr)size, IntPtr.Zero);
                }

                var num1 = BitConverter.ToUInt32(memoryAddress, 0); //ToUInt64 causes arithmetic overflow.

                var base1 = (UIntPtr)0;

                for (var i = 1; i < offsets.Length; i++)
                {
                    base1 = new UIntPtr(Convert.ToUInt32(num1 + offsets[i]));
                    ReadProcessMemory(MProc.Handle, base1, memoryAddress, (UIntPtr)size, IntPtr.Zero);
                    num1 = BitConverter.ToUInt32(memoryAddress, 0); //ToUInt64 causes arithmetic overflow.
                }
                return base1;
            }
            else // no offsets
            {
                var trueCode = Convert.ToInt32(newOffsets, 16);
                IntPtr altModule = IntPtr.Zero;
                //Debug.WriteLine("newOffsets=" + newOffsets);
                if (theCode.ToLower().Contains("base") || theCode.ToLower().Contains("main"))
                {
                    altModule = MProc.MainModule.BaseAddress;
                }
                else if (!theCode.ToLower().Contains("base") && !theCode.ToLower().Contains("main") && theCode.Contains("+"))
                {
                    string[] moduleName = theCode.Split('+');
                    if (!moduleName[0].ToLower().Contains(".dll") && !moduleName[0].ToLower().Contains(".exe") && !moduleName[0].ToLower().Contains(".bin"))
                    {
                        string theAddr = moduleName[0];
                        if (theAddr.Contains("0x"))
                        {
                            theAddr = theAddr.Replace("0x", "");
                        }

                        altModule = (IntPtr)int.Parse(theAddr, NumberStyles.HexNumber);
                    }
                    else
                    {
                        try
                        {
                            altModule = GetModuleAddressByName(moduleName[0]);
                        }
                        catch
                        {
                        }
                    }
                }
                else
                {
                    altModule = GetModuleAddressByName(theCode.Split('+')[0]);
                }

                return (UIntPtr)((int)altModule + trueCode);
            }
        }

        /// <summary>
        /// Retrieve mProc.Process module baseaddress by name
        /// </summary>
        /// <param name="name">name of module</param>
        /// <returns></returns>
        private IntPtr GetModuleAddressByName (string name)
        {
            return MProc.Process.Modules.Cast<ProcessModule>().SingleOrDefault(m => string.Equals(m.ModuleName, name, StringComparison.OrdinalIgnoreCase)).BaseAddress;
        }

        /// <summary>
        /// Convert code from string to real address. If path is not blank, will pull from ini file.
        /// </summary>
        /// <param name="name">label in ini file OR code</param>
        /// <param name="path">path to ini file (OPTIONAL)</param>
        /// <param name="size">size of address (default is 16)</param>
        /// <returns></returns>
        private UIntPtr Get64BitCode(string name, string path = "", int size = 16)
        {
            var theCode = "";
            if (!string.IsNullOrEmpty(path))
            {
                theCode = LoadCode(name, path);
            }
            else
            {
                theCode = name;
            }

            if (string.IsNullOrEmpty(theCode))
            {
                return UIntPtr.Zero;
            }

            // remove spaces
            if (theCode.Contains(" "))
            {
                theCode.Replace(" ", string.Empty);
            }

            string newOffsets = theCode;
            if (theCode.Contains("+"))
            {
                newOffsets = theCode.Substring(theCode.IndexOf('+') + 1);
            }

            var memoryAddress = new byte[size];

            if (!theCode.Contains("+") && !theCode.Contains(","))
            {
                try {
                    return new UIntPtr(Convert.ToUInt64(theCode, 16));
                }
                catch
                {
                    Console.WriteLine("Error in GetCode(). Failed to read address " + theCode);
                    return UIntPtr.Zero;
                }
            }

            if (newOffsets.Contains(','))
            {
                var offsetsList = new List<long>();

                string[] newerOffsets = newOffsets.Split(',');
                foreach (string oldOffsets in newerOffsets)
                {
                    string test = oldOffsets;
                    if (oldOffsets.Contains("0x"))
                    {
                        test = oldOffsets.Replace("0x", "");
                    }

                    long preParse = 0;
                    if (!oldOffsets.Contains("-"))
                    {
                        preParse = long.Parse(test, NumberStyles.AllowHexSpecifier);
                    }
                    else
                    {
                        test = test.Replace("-", "");
                        preParse = long.Parse(test, NumberStyles.AllowHexSpecifier);
                        preParse = preParse * -1;
                    }
                    offsetsList.Add(preParse);
                }
                long[] offsets = offsetsList.ToArray();

                if (theCode.Contains("base") || theCode.Contains("main"))
                {
                    ReadProcessMemory(MProc.Handle, (UIntPtr)((long)MProc.MainModule.BaseAddress + offsets[0]), memoryAddress, (UIntPtr)size, IntPtr.Zero);
                }
                else if (!theCode.Contains("base") && !theCode.Contains("main") && theCode.Contains("+"))
                {
                    string[] moduleName = theCode.Split('+');
                    IntPtr altModule = IntPtr.Zero;
                    if (!moduleName[0].ToLower().Contains(".dll") && !moduleName[0].ToLower().Contains(".exe") && !moduleName[0].ToLower().Contains(".bin"))
                    {
                        altModule = (IntPtr)long.Parse(moduleName[0], System.Globalization.NumberStyles.HexNumber);
                    }
                    else
                    {
                        try
                        {
                            altModule = GetModuleAddressByName(moduleName[0]);
                        }
                        catch
                        {
                            Debug.WriteLine("Module " + moduleName[0] + " was not found in module list!");
                            //Debug.WriteLine("Modules: " + string.Join(",", mProc.Modules));
                        }
                    }
                    ReadProcessMemory(MProc.Handle, (UIntPtr)((long)altModule + offsets[0]), memoryAddress, (UIntPtr)size, IntPtr.Zero);
                }
                else // no offsets
                {
                    ReadProcessMemory(MProc.Handle, (UIntPtr)(offsets[0]), memoryAddress, (UIntPtr)size, IntPtr.Zero);
                }

                var num1 = BitConverter.ToInt64(memoryAddress, 0);

                var base1 = (UIntPtr)0;

                for (var i = 1; i < offsets.Length; i++)
                {
                    base1 = new UIntPtr(Convert.ToUInt64(num1 + offsets[i]));
                    ReadProcessMemory(MProc.Handle, base1, memoryAddress, (UIntPtr)size, IntPtr.Zero);
                    num1 = BitConverter.ToInt64(memoryAddress, 0);
                }
                return base1;
            }
            else
            {
                var trueCode = Convert.ToInt64(newOffsets, 16);
                IntPtr altModule = IntPtr.Zero;
                if (theCode.Contains("base") || theCode.Contains("main"))
                {
                    altModule = MProc.MainModule.BaseAddress;
                }
                else if (!theCode.Contains("base") && !theCode.Contains("main") && theCode.Contains("+"))
                {
                    string[] moduleName = theCode.Split('+');
                    if (!moduleName[0].ToLower().Contains(".dll") && !moduleName[0].ToLower().Contains(".exe") && !moduleName[0].ToLower().Contains(".bin"))
                    {
                        string theAddr = moduleName[0];
                        if (theAddr.Contains("0x"))
                        {
                            theAddr = theAddr.Replace("0x", "");
                        }

                        altModule = (IntPtr)long.Parse(theAddr, NumberStyles.HexNumber);
                    }
                    else
                    {
                        try
                        {
                            altModule = GetModuleAddressByName(moduleName[0]);
                        }
                        catch
                        {
                            Debug.WriteLine("Module " + moduleName[0] + " was not found in module list!");
                            //Debug.WriteLine("Modules: " + string.Join(",", mProc.Modules));
                        }
                    }
                }
                else
                {
                    altModule = GetModuleAddressByName(theCode.Split('+')[0]);
                }

                return (UIntPtr)((long)altModule + trueCode);
            }
        }

        public string MSize()
        {
            if (MProc.Is64Bit)
            {
                return ("x16");
            }
            else
            {
                return ("x8");
            }
        }
    }
}
