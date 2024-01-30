using System.Windows;
using System.Windows.Input;
using CactusPie.Palworld.DisableBuildingRestrictions.Modules.Base;

namespace CactusPie.Palworld.DisableBuildingRestrictions.Modules;

public class WaterBuildingModule : ModuleBase
{
    public override Key Hotkey => Key.F9;

    public override string Name => "Water Building";

    public WaterBuildingModule(Window mainWindow) : base(mainWindow, PalworldAobs.DefaultWaterBuildingAobs, PalworldAobs.EnabledWaterBuildingAobs)
    {
    }

    protected override void Toggle()
    {
        IsEnabled = !IsEnabled;

        if (IsEnabled)
        {
            GameMemory.WriteBytes(Address, new byte[]{ 0xEB, 0x0E });
        }
        else
        {
            GameMemory.WriteBytes(Address, new byte[]{ 0x74, 0x0E });
        }

        OnStateChanged();
    }
}