using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Memory;
using NHotkey;
using NHotkey.Wpf;

namespace CactusPie.Palworld.DisableBuildingRestrictions.Modules.Base;

public abstract class ModuleBase : IModule
{
    private readonly Window _mainWindow;

    protected Mem? GameMemory { get; set; }

    public abstract Key Hotkey { get; }

    public abstract string Name { get; }

    public bool IsEnabled { get; protected set; }

    public event EventHandler? StateChanged;

    private bool _isRunningHotkey = false;

    public ModuleBase(Window mainWindow)
    {
        _mainWindow = mainWindow;
    }

    public abstract Task<bool> TryInitialize(Mem gameMemory);

    public void OnGameProcessExited()
    {
        IsEnabled = false;
        _mainWindow.Dispatcher.Invoke(() =>
            {
                HotkeyManager.Current.Remove(Name);
            });
    }

    protected abstract void Toggle();

    protected void OnStateChanged()
    {
        EventHandler? eventHandler = StateChanged;
        eventHandler?.Invoke(this, EventArgs.Empty);
    }

    protected void EnableHotkey()
    {
        _mainWindow.Dispatcher.Invoke(() =>
        {
            HotkeyManager.Current.AddOrReplace(Name, Hotkey, ModifierKeys.None, OnHotkeyPressed);
        });
    }

    private void OnHotkeyPressed(object? sender, HotkeyEventArgs e)
    {
        if (_isRunningHotkey || GameMemory == null)
        {
            return;
        }

        _isRunningHotkey = true;

        if (GameMemory.MProc.Process.HasExited)
        {
            IsEnabled = false;
            HotkeyManager.Current.Remove(Name);
            return;
        }

        Toggle();
        _isRunningHotkey = false;
    }
}