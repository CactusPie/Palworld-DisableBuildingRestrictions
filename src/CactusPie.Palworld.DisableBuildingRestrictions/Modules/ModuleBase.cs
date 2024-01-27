using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Memory;
using NHotkey;
using NHotkey.Wpf;

namespace CactusPie.Palworld.DisableBuildingRestrictions.Modules;

public abstract class ModuleBase : IModule
{
    private readonly Window _mainWindow;

    private readonly string _defaultAobs;

    private readonly string _alreadyEnabledAobs;

    protected Mem? GameMemory { get; private set; }

    protected string? Address { get; private set; }

    public abstract Key Hotkey { get; }

    public abstract string Name { get; }

    public bool IsEnabled { get; protected set; }

    public event EventHandler? StateChanged;

    public ModuleBase(Window mainWindow, string defaultAobs, string alreadyEnabledAobs)
    {
        _mainWindow = mainWindow;
        _defaultAobs = defaultAobs;
        _alreadyEnabledAobs = alreadyEnabledAobs;
    }

    public async Task<bool> TryInitialize(Mem gameMemory)
    {
        GameMemory = gameMemory;
        bool usingAlreadyEnabledAobs = true;

        long? address = await GetDefaultAddress().ConfigureAwait(false);

        if (address is null or 0)
        {
            usingAlreadyEnabledAobs = true;
            address = await GetAddressForAlreadyModifiedCode().ConfigureAwait(false);
        }

        if (address is null or 0)
        {
            return false;
        }

        Address = address.Value.ToString("X");

        _mainWindow.Dispatcher.Invoke(() =>
            {
                HotkeyManager.Current.AddOrReplace(Name, Hotkey, ModifierKeys.None, OnHotkeyPressed);
            });

        IsEnabled = usingAlreadyEnabledAobs;

        return true;
    }

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

    private void OnHotkeyPressed(object? sender, HotkeyEventArgs e)
    {
        if (GameMemory == null)
        {
            return;
        }

        if (GameMemory.mProc.Process.HasExited)
        {
            IsEnabled = false;
            HotkeyManager.Current.Remove(Name);
            return;
        }

        Toggle();
    }

    private async Task<long?> GetDefaultAddress()
    {
        IEnumerable<long>? addresses = await GameMemory
            .AoBScan(_defaultAobs, false, true)
            .ConfigureAwait(false);

        long? address = addresses?.FirstOrDefault();
        return address;
    }

    private async Task<long?> GetAddressForAlreadyModifiedCode()
    {
        IEnumerable<long>? addresses = await GameMemory
            .AoBScan(_alreadyEnabledAobs, false, true)
            .ConfigureAwait(false);

        long? address = addresses?.FirstOrDefault();
        return address;
    }
}