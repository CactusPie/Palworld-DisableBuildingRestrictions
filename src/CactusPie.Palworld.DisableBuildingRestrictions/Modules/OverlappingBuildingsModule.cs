using System.Windows;
using System.Windows.Input;

namespace CactusPie.Palworld.DisableBuildingRestrictions.Modules;

public class OverlappingBuildingsModule : ModuleBase
{
    public override Key Hotkey => Key.F8;

    public override string Name => "Overlapping Buildings";

    public OverlappingBuildingsModule(Window mainWindow) : base(mainWindow, PalworldAobs.DefaultOverlappingBuildingsAobs, PalworldAobs.EnabledOverlappingBuildingsAobs)
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