using System.Windows;
using System.Windows.Input;
using CactusPie.Palworld.DisableBuildingRestrictions.Modules.Base;

namespace CactusPie.Palworld.DisableBuildingRestrictions.Modules;

public sealed class BuildingCloseToPalboxModule : SingleAddressModuleBase
{
    private const string DefaultBuildingCloseToPalboxAobs = "74 15 48 8B 9E A8 02 00 00 B2 02";

    private const string EnabledBuildingCloseToPalboxAobs = "EB 15 48 8B 9E A8 02 00 00 B2 02";

    public override Key Hotkey => Key.F11;

    public override string Name => "Building Close To Palbox";

    public BuildingCloseToPalboxModule(Window mainWindow) : base(mainWindow, DefaultBuildingCloseToPalboxAobs, EnabledBuildingCloseToPalboxAobs)
    {
    }

    protected override void Toggle()
    {
        IsEnabled = !IsEnabled;

        if (IsEnabled)
        {
            GameMemory.WriteBytes(Address, new byte[]{ 0xEB, 0x15 });
        }
        else
        {
            GameMemory.WriteBytes(Address, new byte[]{ 0x74, 0x15 });
        }

        OnStateChanged();
    }
}