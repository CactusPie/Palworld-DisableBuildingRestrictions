using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using CactusPie.Palworld.DisableBuildingRestrictions.Modules;
using Memory;

namespace CactusPie.Palworld.DisableBuildingRestrictions;

public partial class ModuleControl : UserControl
{
    private readonly IModule _module;

    public ModuleControl(IModule module)
    {
        _module = module;
        InitializeComponent();
        _module.StateChanged += ModuleOnStateChanged;
    }

    public async Task InitializeModule(Mem gameMemory)
    {
        bool succeeded = await _module.TryInitialize(gameMemory).ConfigureAwait(false);

        if (!succeeded)
        {
            Dispatcher.Invoke(
                () =>
                {
                    MainLabel.Content = $"{_module.Name}: Failed to find the correct memory address";
                    MainLabel.Foreground = new SolidColorBrush(Colors.DarkRed);
                });

            return;
        }

        Dispatcher.Invoke(UpdateLabel);
    }

    public void OnGameProcessExited()
    {
        _module.OnGameProcessExited();
    }

    private void ModuleOnStateChanged(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(UpdateLabel);
    }

    private void UpdateLabel()
    {
        string state;
        Color color;

        if (_module.IsEnabled)
        {
            state = "ENABLED";
            color = Colors.DarkGreen;
        }
        else
        {
            state = "DISABLED";
            color = Colors.Black;
        }

        MainLabel.Content = $"{_module.Name}: {state} ({_module.Hotkey} to toggle)";
        MainLabel.Foreground = new SolidColorBrush(color);
    }
}