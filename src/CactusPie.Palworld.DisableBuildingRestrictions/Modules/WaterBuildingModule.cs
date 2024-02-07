using System.Windows;
using System.Windows.Input;
using CactusPie.Palworld.DisableBuildingRestrictions.Modules.Base;

namespace CactusPie.Palworld.DisableBuildingRestrictions.Modules;

public sealed class WaterBuildingModule : SingleAddressModuleBase
{
    private const string DefaultWaterBuildingAobs = "74 0E 0F B6 4E 30";

    private const string EnabledWaterBuildingAobs = "EB 0E 0F B6 4E 30";

    public override Key Hotkey => Key.F9;

    public override string Name => "Water Building";

    public WaterBuildingModule(Window mainWindow) : base(mainWindow, DefaultWaterBuildingAobs, EnabledWaterBuildingAobs)
    {
    }

    protected override void Toggle()
    {
        IsEnabled = !IsEnabled;

        if (IsEnabled)
        {
            GameMemory.WriteBytes(Address, new byte[]{ 0xEB });
        }
        else
        {
            GameMemory.WriteBytes(Address, new byte[]{ 0x74 });
        }

        OnStateChanged();
    }
}