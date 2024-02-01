using System.Windows;
using System.Windows.Input;
using CactusPie.Palworld.DisableBuildingRestrictions.Modules.Base;

namespace CactusPie.Palworld.DisableBuildingRestrictions.Modules;

public sealed class BuildingInMidAirModule : ModuleBase
{
    private const string DefaultBuildingInMidAirAobs = "0F 84 8A 00 00 00 48 8D 8D";

    private const string EnabledBuildingInMidAirAobs = "90 90 90 90 90 90 48 8D 8D E0 00 00 00";

    public override Key Hotkey => Key.F10;

    public override string Name => "Mid Air Building";

    public BuildingInMidAirModule(Window mainWindow) : base(mainWindow, DefaultBuildingInMidAirAobs, EnabledBuildingInMidAirAobs)
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
            GameMemory.WriteBytes(Address, new byte[]{ 0x0F, 0x84, 0x8A, 0x00, 0x00, 0x00, });
        }

        OnStateChanged();
    }
}