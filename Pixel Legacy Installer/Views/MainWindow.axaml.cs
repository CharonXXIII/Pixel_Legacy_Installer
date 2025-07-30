using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.Collections;
using System;
using System.Linq;
using Pixel_Legacy_Installer.ViewModels;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using System.Diagnostics;
using Avalonia.Layout;
using System.IO;

namespace Pixel_Legacy_Installer.Views
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _progressBarTimer;
        public MainWindow()
        {
            InitializeComponent();
            MinWidth = 600;
            MinHeight = 400;

            Opened += (_, _) =>
            {
                this.Width = 800;
                this.Height = 650;
            };

            _progressBarTimer = new DispatcherTimer();
            _progressBarTimer.Interval = TimeSpan.FromMilliseconds(500);
            _progressBarTimer.Tick += (sender, e) => SetProgressBarVisibility();
            _progressBarTimer.Start();

            DataContext = new MainWindowViewModel();
        }

        private async void DiscordButton_Click(object? sender, RoutedEventArgs e)
        {
            var url = "https://discord.gg/3H69pHzPS5";
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch
            {
                await MainWindowViewModel.ShowMessageAsync(this, $"Unable to open Discord link. Please copy and paste the URL into your browser.\n{url}", "Error");
            }
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            CleanupUpdateFiles();
            this.Width = 800;
            this.Height = 650;
        }

        private void SetProgressBarVisibility()
        {
            if (DownloadProgressBar != null)
            {
                DownloadProgressBar.IsVisible = DownloadProgressBar.Value != 0 && DownloadProgressBar.Value != 100;
            }
        }

        private void CleanupUpdateFiles()
        {
            try
            {
                var currentExe = Process.GetCurrentProcess().MainModule?.FileName!;
                var appDir = Path.GetDirectoryName(currentExe)!;

                var updateZip = Path.Combine(appDir, "update.zip");
                var updateBat = Path.Combine(appDir, "update.bat");
                var cleanupBat = Path.Combine(appDir, "cleanup.bat");

                if (File.Exists(updateZip))
                    File.Delete(updateZip);

                if (File.Exists(updateBat))
                    File.Delete(updateBat);

                if (File.Exists(cleanupBat))
                    File.Delete(cleanupBat);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CleanupUpdateFiles error: {ex.Message}");
            }
        }

    }
}
