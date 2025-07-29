using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Security.Principal;
using System.Windows;
using System.Linq;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using Avalonia.Controls;
using System.Security.Cryptography.X509Certificates;
using System.Collections.ObjectModel;
using Pixel_Legacy_Installer.LocalizationHelper;

namespace Pixel_Legacy_Installer.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly HelperLocalization _localizationService;

        public MainWindowViewModel()
        {
            _localizationService = new HelperLocalization();

            Languages = new ObservableCollection<string> { "en", "fi", "pl", "id", "tr" };
            SelectedLanguage = "en";
        }

        public ObservableCollection<string> Languages { get; }

        private string _selectedLanguage;
        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (SetProperty(ref _selectedLanguage, value))
                {
                    _localizationService.SetLanguage(value);
                    StatusMessage = $"Language set to {GetLanguageFullName(value)}";
                }
            }
        }

        public IRelayCommand ChangeLanguageCommand { get; }

        private string _statusMessage = "";
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        private int _downloadProgress;
        public int DownloadProgress
        {
            get => _downloadProgress;
            set => SetProperty(ref _downloadProgress, value);
        }

        [RelayCommand]
        private async Task Play()
        {
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var zipPath = Path.Combine(desktopPath, "Pixel Worlds.zip");
            var extractPath = Path.Combine(desktopPath, "Pixel Worlds");
            var exePath = Path.Combine(extractPath + "\\Pixel Worlds", "PixelWorlds.exe");

            try
            {
                if (File.Exists(exePath))
                {
                    StatusMessage = "Launching Pixel Worlds...";
                    Process.Start(new ProcessStartInfo { FileName = exePath, UseShellExecute = true });
                    return;
                }

                // If zip or extracted folder missing, can't launch
                if (!File.Exists(zipPath) || !Directory.Exists(extractPath))
                {
                    StatusMessage = "Game not downloaded or installed. Please click Download.";
                    return;
                }

                StatusMessage = "Game installation incomplete or corrupted. Please click Download.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"An error occurred: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task Download()
        {
            var url = "https://www.dropbox.com/scl/fi/y8cr2t6p608vuadxi09mf/Pixel-Worlds.zip?rlkey=8xu26aewgd7o6y3kvgnnw1m4a&st=mgoyiicy&dl=1";
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var zipPath = Path.Combine(desktopPath, "Pixel Worlds.zip");
            var extractPath = Path.Combine(desktopPath, "Pixel Worlds");

            try
            {
                bool needDownload = false;

                if (!File.Exists(zipPath))
                {
                    needDownload = true;
                }
                else
                {
                    try
                    {
                        using (var archive = ZipFile.OpenRead(zipPath))
                        {
                            // ZIP is valid — delete anyway to redownload
                            StatusMessage = "Existing ZIP detected. Deleting to redownload...";
                        }
                        File.Delete(zipPath);
                        needDownload = true;
                    }
                    catch (InvalidDataException)
                    {
                        StatusMessage = "Corrupted ZIP detected. Deleting corrupted file...";
                        File.Delete(zipPath);
                        needDownload = true;
                    }
                }

                if (!Directory.Exists(extractPath))
                {
                    needDownload = true;
                }

                if (needDownload)
                {
                    if (Directory.Exists(extractPath))
                    {
                        StatusMessage = "Removing old installation...";
                        Directory.Delete(extractPath, true);
                    }

                    StatusMessage = "Downloading Pixel Worlds...";
                    await DownloadFileWithProgressAsync(url, zipPath);
                    StatusMessage = "Download completed.";

                    StatusMessage = "Extracting files...";
                    using (var archive = ZipFile.OpenRead(zipPath))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            string destinationPath = Path.Combine(extractPath, entry.FullName);

                            if (string.IsNullOrEmpty(entry.Name)) // It's a directory
                            {
                                Directory.CreateDirectory(destinationPath);
                                continue;
                            }

                            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
                            entry.ExtractToFile(destinationPath, overwrite: true);
                        }
                    }
                    StatusMessage = "Extraction complete.";
                }
                else
                {
                    StatusMessage = "Game already downloaded and installed. Skipping download.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Download or extraction failed: {ex.Message}";
                await ShowMessageAsync(App.MainWindow!, StatusMessage);
            }
        }

        [RelayCommand]
        private async Task DownloadDiscordRPCGame()
        {
            var url = "https://www.dropbox.com/scl/fi/pv1tuco7nxkqq3yze5i3e/Pixel-Worlds-Discord-RPC.zip?rlkey=t6z59dn6cibo87whiz1dv5yw4&st=1j8hy4ca&dl=1";
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var zipPath = Path.Combine(desktopPath, "Pixel Worlds Discord RPC.zip");
            var extractPath = Path.Combine(desktopPath, "Pixel Worlds");

            try
            {
                bool needDownload = false;

                if (!File.Exists(zipPath))
                {
                    needDownload = true;
                }
                else
                {
                    try
                    {
                        using (var archive = ZipFile.OpenRead(zipPath))
                        {
                            StatusMessage = "Existing ZIP detected. Deleting to redownload...";
                        }
                        File.Delete(zipPath);
                        needDownload = true;
                    }
                    catch (InvalidDataException)
                    {
                        StatusMessage = "Corrupted ZIP detected. Deleting corrupted file...";
                        File.Delete(zipPath);
                        needDownload = true;
                    }
                }

                if (!Directory.Exists(extractPath))
                {
                    needDownload = true;
                }

                if (needDownload)
                {
                    if (Directory.Exists(extractPath))
                    {
                        StatusMessage = "Removing old installation...";
                        Directory.Delete(extractPath, true);
                    }

                    StatusMessage = "Downloading Pixel Worlds RPC Edition...";
                    await DownloadFileWithProgressAsync(url, zipPath);
                    StatusMessage = "Download completed.";

                    StatusMessage = "Extracting files...";
                    using (var archive = ZipFile.OpenRead(zipPath))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            var destinationPath = Path.Combine(extractPath, entry.FullName);

                            if (string.IsNullOrEmpty(entry.Name))
                            {
                                Directory.CreateDirectory(destinationPath);
                                continue;
                            }

                            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
                            entry.ExtractToFile(destinationPath, overwrite: true);
                        }
                    }
                    StatusMessage = "Extraction complete.";
                }
                else
                {
                    StatusMessage = "Game already downloaded and installed. Skipping download.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Download or extraction failed: {ex.Message}";
                await ShowMessageAsync(App.MainWindow!, StatusMessage);
            }
        }


        [RelayCommand]
        private async Task Connect()
        {
            string hostsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"drivers\etc\hosts");

            string[] entries = new[]
            {
        "74.208.102.182 lambda.us-east-1.amazonaws.com",
        "74.208.102.182 ec2-34-237-73-93.compute-1.amazonaws.com",
        "74.208.102.182 cognito-identity.us-east-1.amazonaws.com",
        "74.208.102.182 prod.gamev92.portalworldsgame.com"
    };

            try
            {
                StatusMessage = "Checking current hosts file entries...";

                var lines = await File.ReadAllLinesAsync(hostsPath);

                bool allExist = entries.All(entry =>
                    lines.Any(line => string.Equals(line.Trim(), entry, StringComparison.OrdinalIgnoreCase)));

                if (allExist)
                {
                    StatusMessage = "All required hosts entries already exist.";

                    return;
                }

                StatusMessage = "Adding missing entries to the hosts file...";

                using (var writer = File.AppendText(hostsPath))
                {
                    await writer.WriteLineAsync(); // blank line for spacing

                    foreach (var entry in entries)
                    {
                        if (!lines.Any(line => string.Equals(line.Trim(), entry, StringComparison.OrdinalIgnoreCase)))
                        {
                            await writer.WriteLineAsync(entry);
                        }
                    }
                }

                StatusMessage = "Hosts file updated successfully.";
            }
            catch (UnauthorizedAccessException)
            {
                StatusMessage = "Access denied. Please run the app as Administrator.";
                await ShowMessageAsync(App.MainWindow!, StatusMessage);
            }
            catch (Exception ex)
            {
                StatusMessage = $"An error occurred: {ex.Message}";
                await ShowMessageAsync(App.MainWindow!, StatusMessage);
            }
        }

        [RelayCommand]
        private async Task Disconnect()
        {
            string hostsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"drivers\etc\hosts");

            string[] entriesToRemove = new[]
            {
        "74.208.102.182 lambda.us-east-1.amazonaws.com",
        "74.208.102.182 ec2-34-237-73-93.compute-1.amazonaws.com",
        "74.208.102.182 cognito-identity.us-east-1.amazonaws.com",
        "74.208.102.182 prod.gamev92.portalworldsgame.com"
    };

            try
            {
                StatusMessage = "Reading hosts file to remove entries...";

                var lines = await File.ReadAllLinesAsync(hostsPath);

                var newLines = lines.Where(line =>
                    !entriesToRemove.Any(entry => string.Equals(line.Trim(), entry, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                if (newLines.Count == lines.Length)
                {
                    StatusMessage = "No matching hosts entries found to remove.";
                    return;
                }

                await File.WriteAllLinesAsync(hostsPath, newLines);

                StatusMessage = "Hosts entries removed successfully.";
            }
            catch (UnauthorizedAccessException)
            {
                StatusMessage = "Access denied. Please run the app as Administrator.";
                await ShowMessageAsync(App.MainWindow!, StatusMessage);
            }
            catch (Exception ex)
            {
                StatusMessage = $"An error occurred: {ex.Message}";
                await ShowMessageAsync(App.MainWindow!, StatusMessage);
            }
        }

        [RelayCommand]
        private async Task InstallCerts()
        {
            string certUrl = "https://www.dropbox.com/scl/fi/5nrhj57rlpjoz0q8aca6t/rootCA.crt?rlkey=o5neraoolanxotnaqvwspyrk3&st=0sio74hb&dl=1";
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string certFileName = "rootCA.crt";
            string certPath = Path.Combine(desktopPath, certFileName);

            try
            {
                StatusMessage = "Starting certificate download...";

                using (var client = new HttpClient())
                using (var response = await client.GetAsync(certUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                    var canReportProgress = totalBytes != -1;

                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(certPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        var buffer = new byte[8192];
                        long totalRead = 0;
                        int bytesRead;
                        var lastReportedPercent = 0;

                        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            totalRead += bytesRead;

                            if (canReportProgress)
                            {
                                int progress = (int)((totalRead * 100L) / totalBytes);
                                if (progress != lastReportedPercent)
                                {
                                    StatusMessage = $"Downloading certificate... {progress}%";
                                    lastReportedPercent = progress;
                                }
                            }
                        }
                    }
                }

                StatusMessage = "Certificate Download completed.";

                var cert = new X509Certificate2(certPath);

                // Check if certificate already installed by thumbprint
                using (var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine))
                {
                    store.Open(OpenFlags.ReadOnly);

                    bool isInstalled = store.Certificates
                        .Cast<X509Certificate2>()
                        .Any(c => c.Thumbprint?.Equals(cert.Thumbprint, StringComparison.OrdinalIgnoreCase) == true);

                    store.Close();

                    if (isInstalled)
                    {
                        StatusMessage = "Certificate is already installed.";
                        await ShowMessageAsync(App.MainWindow!, StatusMessage);
                        if (File.Exists(certPath))
                            File.Delete(certPath);
                        return;
                    }
                }

                StatusMessage = "Installing certificate to Trusted Root Certification Authorities...";

                using (var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine))
                {
                    store.Open(OpenFlags.ReadWrite);
                    store.Add(cert);
                    store.Close();
                }

                StatusMessage = "Certificate installed successfully.";

                if (File.Exists(certPath))
                    File.Delete(certPath);
            }
            catch (UnauthorizedAccessException)
            {
                StatusMessage = "Access denied. Please run the app as Administrator.";
                await ShowMessageAsync(App.MainWindow!, StatusMessage);
            }
            catch (Exception ex)
            {
                StatusMessage = $"An error occurred: {ex.Message}";
                await ShowMessageAsync(App.MainWindow!, StatusMessage);
            }
        }

        [RelayCommand]
        private async Task UninstallCerts()
        {
            string certFileName = "rootCA.crt";
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string certPath = Path.Combine(desktopPath, certFileName);

            try
            {
                if (!File.Exists(certPath))
                {
                    StatusMessage = $"Certificate file '{certFileName}' not found on Desktop.";
                    await ShowMessageAsync(App.MainWindow!, StatusMessage);
                    return;
                }

                StatusMessage = "Loading certificate for uninstallation...";

                var cert = new X509Certificate2(certPath);

                using (var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine))
                {
                    store.Open(OpenFlags.ReadWrite);

                    var certToRemove = store.Certificates
                        .Cast<X509Certificate2>()
                        .FirstOrDefault(c => c.Thumbprint?.Equals(cert.Thumbprint, StringComparison.OrdinalIgnoreCase) == true);

                    if (certToRemove == null)
                    {
                        StatusMessage = "Certificate not found in Trusted Root Certification Authorities.";
                        await ShowMessageAsync(App.MainWindow!, StatusMessage);
                    }
                    else
                    {
                        store.Remove(certToRemove);
                        StatusMessage = "Certificate uninstalled successfully.";
                        await ShowMessageAsync(App.MainWindow!, StatusMessage);
                    }

                    store.Close();
                }

                // Delete the certificate file after uninstalling
                if (File.Exists(certPath))
                {
                    File.Delete(certPath);
                    StatusMessage = "Certificate file deleted from Desktop.";
                    await ShowMessageAsync(App.MainWindow!, StatusMessage);
                }
            }
            catch (UnauthorizedAccessException)
            {
                StatusMessage = "Access denied. Please run the app as Administrator.";
                await ShowMessageAsync(App.MainWindow!, StatusMessage);
            }
            catch (Exception ex)
            {
                StatusMessage = $"An error occurred: {ex.Message}";
                await ShowMessageAsync(App.MainWindow!, StatusMessage);
            }
        }

        [RelayCommand]
        private async Task ReadMe()
        {
            var app = Avalonia.Application.Current!;
            if (app.Resources.TryGetResource("ReadMeText", null, out var value))
            {
                string readMeText = value?.ToString() ?? "Read Me";
                ShowMessageAsync(App.MainWindow!, readMeText);
            }
            else
            {
                ShowMessageAsync(App.MainWindow!, "Read Me");
            }
        }

        public static async Task ShowMessageAsync(Window? owner, string message, string? title = "Information", string? okButtonText = "Ok")
        {
            var messageBoxParams = new MessageBoxStandardParams
            {
                ButtonDefinitions = ButtonEnum.Ok,
                ContentTitle = title,
                ContentMessage = message,
                Icon = Icon.Info,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };

            var msgBox = MessageBoxManager.GetMessageBoxStandard(messageBoxParams);
            await msgBox.ShowWindowDialogAsync(owner);
        }


        private async Task DownloadFileWithProgressAsync(string url, string destinationPath)
        {
            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var contentLength = response.Content.Headers.ContentLength;

            using var contentStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var totalRead = 0L;
            var buffer = new byte[8192];
            var isMoreToRead = true;

            do
            {
                var read = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                if (read == 0)
                {
                    isMoreToRead = false;
                    UpdateDownloadProgress(100);
                    continue;
                }

                await fileStream.WriteAsync(buffer, 0, read);

                totalRead += read;
                if (contentLength.HasValue)
                {
                    var progress = (int)((totalRead * 100) / contentLength.Value);
                    UpdateDownloadProgress(progress);
                }
            }
            while (isMoreToRead);
        }

        private void UpdateDownloadProgress(int progress)
        {
            DownloadProgress = progress;
            StatusMessage = $"Downloading Pixel Worlds... {progress}%";
        }

        private string GetLanguageFullName(string code)
        {
            switch (code)
            {
                case "en": return "English";
                case "fi": return "Finnish";
                case "pl": return "Polish";
                case "id": return "Indonesian";
                case "tr": return "Turkish";
                default: return code; // fallback to code if unknown
            }
        }
    }
}