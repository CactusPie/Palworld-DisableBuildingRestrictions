using System.Windows;
using System.Windows.Input;
using CactusPie.Palworld.DisableBuildingRestrictions.Modules.Base;

namespace CactusPie.Palworld.DisableBuildingRestrictions.Modules;

public sealed class NoSnappingModule : MultiAddressModuleBase
{
    private static readonly string[] DefaultNoSnappingAobs = {
        "75 79 41 0F 28 CF",
        "0F 87 48 04 00 00 41",
    };

    private static readonly string[] EnabledNoSnappingAobs = {
        "EB 79 41 0F 28 CF",
        "E9 49 04 00 00 90",
    };

    public override Key Hotkey => Key.F6;

    public override string Name => "No Snapping";

    public NoSnappingModule(Window mainWindow) : base(mainWindow, DefaultNoSnappingAobs, EnabledNoSnappingAobs)
    {
    }

    protected override void Toggle()
    {
        IsEnabled = !IsEnabled;

        if (IsEnabled)
        {
            GameMemory.WriteBytes(Addresses[0], new byte[]{ 0xEB });
            GameMemory.WriteBytes(Addresses[1], new byte[]{ 0xE9, 0x49, 0x04, 0x00, 0x00, 0x90 });
        }
        else
        {
            GameMemory.WriteBytes(Addresses[0], new byte[]{ 0x75 });
            GameMemory.WriteBytes(Addresses[1], new byte[]{ 0x0F, 0x87, 0x48, 0x04, 0x00, 0x00 });
        }

        OnStateChanged();
    }
}