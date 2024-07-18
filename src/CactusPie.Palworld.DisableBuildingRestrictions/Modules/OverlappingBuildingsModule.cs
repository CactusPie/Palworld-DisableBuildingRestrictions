using System.Windows;
using System.Windows.Input;
using CactusPie.Palworld.DisableBuildingRestrictions.Modules.Base;

namespace CactusPie.Palworld.DisableBuildingRestrictions.Modules;

public sealed class OverlappingBuildingsModule : SingleAddressModuleBase
{
    private const string DefaultOverlappingBuildingsAobs = "0F 84 40 01 00 00 48 8B CF E8 45";

    private const string EnabledOverlappingBuildingsAobs = "E9 41 01 00 00 90 48 8B CF E8 48";

    public override Key Hotkey => Key.F8;

    public override string Name => "Overlapping Buildings";

    public OverlappingBuildingsModule(Window mainWindow) : base(mainWindow, DefaultOverlappingBuildingsAobs, EnabledOverlappingBuildingsAobs)
    {
    }

    protected override void Toggle()
    {
        IsEnabled = !IsEnabled;

        if (IsEnabled)
        {
            GameMemory.WriteBytes(Address, new byte[]{ 0xE9, 0x41, 0x01, 0x00, 0x00, 0x90 });
        }
        else
        {
            GameMemory.WriteBytes(Address, new byte[]{ 0x0F, 0x84, 0x40, 0x01, 0x00, 0x00 });
        }

        OnStateChanged();
    }
}