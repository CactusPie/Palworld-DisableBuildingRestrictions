using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Memory;

namespace CactusPie.Palworld.DisableBuildingRestrictions.Modules.Base;

public abstract class MultiAddressModuleBase : ModuleBase
{
    private readonly IReadOnlyList<string> _defaultAobs;

    private readonly IReadOnlyList<string> _alreadyEnabledAobs;

    private long _minAddress;

    private long _maxAddress;

    protected IReadOnlyList<string>? Addresses { get; private set; }

    public MultiAddressModuleBase(Window mainWindow, IReadOnlyList<string> defaultAobs, IReadOnlyList<string> alreadyEnabledAobs) : base(mainWindow)
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

        IReadOnlyList<string>? addresses = await GetDefaultAddresses().ConfigureAwait(false);

        if (addresses is null or { Count: 0 })
        {
            usingAlreadyEnabledAobs = true;
            addresses = await GetAlreadyEnabledAddresses().ConfigureAwait(false);
        }

        if (addresses is null or { Count: 0 })
        {
            return false;
        }

        Addresses = addresses;
        EnableHotkey();
        IsEnabled = usingAlreadyEnabledAobs;

        return true;
    }

    private async Task<IReadOnlyList<string>?> GetDefaultAddresses()
    {
        var addresses = new string[_defaultAobs.Count];

        for (var i = 0; i < _defaultAobs.Count; i++)
        {
            long? address = await GetAddress(_defaultAobs[i]).ConfigureAwait(false);

            if (address == null)
            {
                return null;
            }

            addresses[i] = address.Value.ToString("X");
        }

        return addresses;
    }

    private async Task<IReadOnlyList<string>?> GetAlreadyEnabledAddresses()
    {
        var addresses = new string[_alreadyEnabledAobs.Count];

        for (var i = 0; i < _alreadyEnabledAobs.Count; i++)
        {
            long? address = await GetAddress(_alreadyEnabledAobs[i]).ConfigureAwait(false);

            if (address == null)
            {
                return null;
            }

            addresses[i] = address.Value.ToString("X");
        }

        return addresses;
    }

    private async Task<long?> GetAddress(string aobs)
    {
        IEnumerable<long>? foundAddresses = await GameMemory
            .AoBScan(_minAddress, _maxAddress, aobs, false, true)
            .ConfigureAwait(false);

        long? address = foundAddresses?.FirstOrDefault();

        if (address is null or 0)
        {
            return null;
        }

        return address;
    }
}