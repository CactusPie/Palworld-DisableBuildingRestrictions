using System;
using System.Threading.Tasks;
using System.Windows;
using CactusPie.Palworld.DisableBuildingRestrictions.Modules;
using Memory;

namespace CactusPie.Palworld.DisableBuildingRestrictions
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Mem GameMemory = new();

        private Task? _gameProcessSearchingTask;

        private Task? _gameProcessExitWaitingTask;

        private readonly ModuleControl[] _moduleControls;

        public MainWindow()
        {
            _moduleControls = new ModuleControl[]
            {
                new(new NoSnappingModule(this)),
                new(new OverlappingBasesModule(this)),
                new(new OverlappingBuildingsModule(this)),
                new(new WaterBuildingModule(this)),
                new(new BuildingInMidAirModule(this)),
                new(new BuildingCloseToPalboxModule(this)),
            };

            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            foreach (ModuleControl module in _moduleControls)
            {
                StackPanelModules.Children.Add(module);
            }

            _gameProcessSearchingTask = WaitForGameProcess();
        }

        private async Task WaitForGameProcess()
        {
            if (_gameProcessSearchingTask != null)
            {
                return;
            }

            while (true)
            {
                bool opened = GameMemory.OpenProcess("Palworld-Win64-Shipping.exe");

                if (!opened || GameMemory.MProc.Process.HasExited)
                {
                    await Task.Delay(1000).ConfigureAwait(false);
                    continue;
                }

                break;
            }

            Dispatcher.Invoke(() =>
            {
                LabelWaitingForProcess.Content = $"Initializing (0/{_moduleControls.Length})";
            });

            for (var index = 0; index < _moduleControls.Length; index++)
            {
                ModuleControl module = _moduleControls[index];
                await module.InitializeModule(GameMemory).ConfigureAwait(false);

                int indexTmp = index;
                Dispatcher.Invoke(() =>
                {
                    LabelWaitingForProcess.Content = $"Initializing ({indexTmp+1}/{_moduleControls.Length})";
                });
            }

            Dispatcher.Invoke(() =>
            {
                LabelWaitingForProcess.Visibility = Visibility.Collapsed;
                StackPanelModules.Visibility = Visibility.Visible;
            });

            _gameProcessSearchingTask = null;
            _gameProcessExitWaitingTask = WaitForGameProcessToExit();
        }

        private async Task WaitForGameProcessToExit()
        {
            if (_gameProcessExitWaitingTask != null)
            {
                return;
            }

            while (true)
            {
                if (GameMemory.MProc.Process.HasExited)
                {
                    break;
                }

                await Task.Delay(1000).ConfigureAwait(false);
            }

            Dispatcher.Invoke(() =>
            {
                LabelWaitingForProcess.Content = $"Waiting for the game to be running...";
            });

            Dispatcher.Invoke(() =>
                {
                    StackPanelModules.Visibility = Visibility.Collapsed;
                    LabelWaitingForProcess.Visibility = Visibility.Visible;
                });

            foreach (ModuleControl moduleControl in _moduleControls)
            {
                moduleControl.OnGameProcessExited();
            }

            _gameProcessExitWaitingTask = null;
            _gameProcessSearchingTask = WaitForGameProcess();
        }
    }
}