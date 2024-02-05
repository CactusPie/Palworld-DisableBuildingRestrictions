using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Memory;

namespace CactusPie.Palworld.DisableBuildingRestrictions.Modules.Base;

public abstract class SingleAddressModuleBase : ModuleBase
{
    private readonly string _defaultAobs;

    private readonly string _alreadyEnabledAobs;

    private long _minAddress;

    private long _maxAddress;

    protected string? Address { get; private set; }

    public SingleAddressModuleBase(Window mainWindow, string defaultAobs, string alreadyEnabledAobs) : base(mainWindow)
    {
        _defaultAobs = defaultAobs;
        _alreadyEnabledAobs = alreadyEnabledAobs;
    }

    public override async Task<bool> TryInitialize(Mem gameMemory)
    {
        GameMemory = gameMemory;

        _minAddress = GameMemory.MProc.MainModule.BaseAddress.ToInt64();
        _maxAddress = _minAddress + GameMemory.MProc.MainModule.ModuleMemorySize;

        bool usingAlreadyEnabledAobs = false;

        long? address = await GetDefaultAddress().ConfigureAwait(false);

        if (address is null or 0)
        {
            usingAlreadyEnabledAobs = true;
            address = await GetAddressForAlreadyModifiedCode().ConfigureAwait(false);
        }

        if (address is null or 0)
        {
            return false;
        }

        Address = address.Value.ToString("X");
        EnableHotkey();
        IsEnabled = usingAlreadyEnabledAobs;

        return true;
    }

    private async Task<long?> GetDefaultAddress()
    {
        IEnumerable<long>? addresses = await GameMemory
            .AoBScan(_minAddress, _maxAddress, _defaultAobs, false, true)
            .ConfigureAwait(false);

        long? address = addresses?.FirstOrDefault();
        return address;
    }

    private async Task<long?> GetAddressForAlreadyModifiedCode()
    {
        IEnumerable<long>? addresses = await GameMemory
            .AoBScan(_minAddress, _maxAddress, _alreadyEnabledAobs, false, true)
            .ConfigureAwait(false);

        long? address = addresses?.FirstOrDefault();
        return address;
    }
}