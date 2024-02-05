using System.Windows;
using System.Windows.Input;
using CactusPie.Palworld.DisableBuildingRestrictions.Modules.Base;

namespace CactusPie.Palworld.DisableBuildingRestrictions.Modules;

public sealed class OverlappingBuildingsModule : SingleAddressModuleBase
{
    private const string DefaultOverlappingBuildingsAobs = "74 07 B0 14 E9 2F 01 00 00";

    private const string EnabledOverlappingBuildingsAobs = "EB 07 B0 14 E9 2F 01 00 00";

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
            GameMemory.WriteBytes(Address, new byte[]{ 0xEB, 0x07 });
        }
        else
        {
            GameMemory.WriteBytes(Address, new byte[]{ 0x74, 0x07 });
        }

        OnStateChanged();
    }
}