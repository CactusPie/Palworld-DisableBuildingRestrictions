using System.Windows;
using System.Windows.Input;
using CactusPie.Palworld.DisableBuildingRestrictions.Modules.Base;

namespace CactusPie.Palworld.DisableBuildingRestrictions.Modules;

public class BuildingCloseToPalboxModule : ModuleBase
{
    public override Key Hotkey => Key.F11;

    public override string Name => "Building Close To Palbox";

    public BuildingCloseToPalboxModule(Window mainWindow) : base(mainWindow, PalworldAobs.DefaultBuildingCloseToPalboxAobs, PalworldAobs.EnabledBuildingCloseToPalboxAobs)
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