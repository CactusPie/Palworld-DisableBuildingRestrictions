using System.Windows;
using System.Windows.Input;

namespace CactusPie.Palworld.DisableBuildingRestrictions.Modules;

public class BuildingInMidAirModule : ModuleBase
{
    public override Key Hotkey => Key.F11;

    public override string Name => "Mid Air Building";

    public BuildingInMidAirModule(Window mainWindow) : base(mainWindow, PalworldAobs.DefaultBuildingInMidAirAobs, PalworldAobs.EnabledBuildingInMidAirAobs)
    {
    }

    protected override void Toggle()
    {
        IsEnabled = !IsEnabled;

        if (IsEnabled)
        {
            GameMemory.WriteBytes(Address, new byte[]{ 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
        }
        else
        {
            GameMemory.WriteBytes(Address, new byte[]{ 0x0F, 0x84, 0x8C, 0x00, 0x00, 0x00, });
        }

        OnStateChanged();
    }
}