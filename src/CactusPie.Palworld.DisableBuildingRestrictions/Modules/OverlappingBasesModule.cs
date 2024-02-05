using System.Windows;
using System.Windows.Input;
using CactusPie.Palworld.DisableBuildingRestrictions.Modules.Base;

namespace CactusPie.Palworld.DisableBuildingRestrictions.Modules;

public sealed class OverlappingBasesModule : SingleAddressModuleBase
{
    private const string DefaultOverlappingBasesAobs = "75 53 48 8B 9F 00 01 00 00";

    private const string EnabledOverlappingBasesAobs = "90 90 48 8B 9F 00 01 00 00";

    public override Key Hotkey => Key.F7;

    public override string Name => "Overlapping Bases";

    public OverlappingBasesModule(Window mainWindow) : base(mainWindow, DefaultOverlappingBasesAobs, EnabledOverlappingBasesAobs)
    {
    }

    protected override void Toggle()
    {
        IsEnabled = !IsEnabled;

        if (IsEnabled)
        {
            GameMemory.WriteBytes(Address, new byte[]{ 0x90, 0x90 });
        }
        else
        {
            GameMemory.WriteBytes(Address, new byte[]{ 0x75, 0x53 });
        }

        OnStateChanged();
    }
}