﻿using System.Windows;
using System.Windows.Input;

namespace CactusPie.Palworld.DisableBuildingRestrictions.Modules;

public class WaterBuildingModule : ModuleBase
{
    public override Key Hotkey => Key.F10;

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