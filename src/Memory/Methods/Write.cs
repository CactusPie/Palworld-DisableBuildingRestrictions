using System;
using System.Collections.Concurrent;
using System.Threading;
using static Memory.Imps;

namespace Memory
{
    public sealed partial class Mem
    {
        readonly ConcurrentDictionary<string, CancellationTokenSource> _freezeTokenSrcs = new ConcurrentDictionary<string, CancellationTokenSource>();

        /// <summary>
        /// Write byte array to addresses.
        /// </summary>
        /// <param name="code">address to write to</param>
        /// <param name="write">byte array to write</param>
        /// <param name="file">path and name of ini file. (OPTIONAL)</param>
        public void WriteBytes(string code, byte[] write, string file = "")
        {
            UIntPtr theCode;
            theCode = GetCode(code, file);
            WriteProcessMemory(MProc.Handle, theCode, write, (UIntPtr)write.Length, IntPtr.Zero);
        }
    }
}
