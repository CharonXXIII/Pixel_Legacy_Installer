using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Pixel_Legacy_Installer.ViewModels;
using Pixel_Legacy_Installer.Views;
using System.Threading.Tasks;
using Pixel_Legacy_Installer.Auto_Updater;

namespace Pixel_Legacy_Installer
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static Window? MainWindow { get; private set; }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                DisableAvaloniaDataAnnotationValidation();

                var mainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };

                desktop.MainWindow = mainWindow;
                MainWindow = mainWindow;

                // Run auto-update check after MainWindow is initialized
                mainWindow.Opened += async (_, _) =>
                {
                    await UpdateChecker.CheckForUpdateAsync(mainWindow);
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void DisableAvaloniaDataAnnotationValidation()
        {
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }
    }
}