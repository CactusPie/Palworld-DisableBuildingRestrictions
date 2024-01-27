using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Memory;

namespace CactusPie.Palworld.DisableBuildingRestrictions.Modules;

public interface IModule
{
    Key Hotkey { get; }

    string Name { get; }

    bool IsEnabled { get; }

    Task<bool> TryInitialize(Mem gameMemory);

    void OnGameProcessExited();

    event EventHandler? StateChanged;
}