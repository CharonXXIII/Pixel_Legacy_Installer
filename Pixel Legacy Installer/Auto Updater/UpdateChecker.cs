using Avalonia.Controls;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Pixel_Legacy_Installer.Auto_Updater.AppVersion;
using Pixel_Legacy_Installer.ViewModels;
using System.IO;

namespace Pixel_Legacy_Installer.Auto_Updater
{
    internal class UpdateChecker
    {
        public static async Task CheckForUpdateAsync(Window? owner)
        {
            try
            {
                using var client = new HttpClient();
                var json = await client.GetStringAsync("https://raw.githubusercontent.com/YourUsername/YourRepo/main/version.json");
                var latest = JsonSerializer.Deserialize<UpdateInfo>(json);

                if (latest is null || string.IsNullOrEmpty(latest.Version))
                    return;

                if (latest.Version != VersionInfo.CurrentVersion)
                {
                    var message = $"A new version ({latest.Version}) is available!\n\nChanges:\n{latest.Changelog}\n\nDo you want to update now?";
                    var result = await MessageBoxManager
                        .GetMessageBoxStandard(new MessageBoxStandardParams
                        {
                            ContentTitle = "Update Available",
                            ContentMessage = message,
                            ButtonDefinitions = ButtonEnum.YesNo,
                            Icon = Icon.Info,
                            WindowStartupLocation = WindowStartupLocation.CenterOwner
                        })
                        .ShowWindowDialogAsync(owner);

                    if (result == ButtonResult.Yes)
                    {
                        var tempFolder = Path.Combine(Path.GetTempPath(), $"PixelLegacy_Update_{Guid.NewGuid()}");
                        Directory.CreateDirectory(tempFolder);

                        var zipPath = Path.Combine(tempFolder, "update.zip");

                        await using (var zipStream = await client.GetStreamAsync(latest.Url))
                        await using (var fileStream = File.Create(zipPath))
                        {
                            await zipStream.CopyToAsync(fileStream);
                        }

                        // Extract ZIP safely
                        ZipFile.ExtractToDirectory(zipPath, tempFolder);

                        var currentExe = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName!;
                        var appDir = Path.GetDirectoryName(currentExe)!;

                        // Copy new files into place
                        foreach (var file in Directory.GetFiles(tempFolder, "*", SearchOption.AllDirectories))
                        {
                            var relativePath = Path.GetRelativePath(tempFolder, file);
                            var destinationPath = Path.Combine(appDir, relativePath);

                            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
                            File.Copy(file, destinationPath, overwrite: true);
                        }

                        await MainWindowViewModel.ShowMessageAsync(owner, "Update complete! The launcher will now restart.", "Updated");

                        System.Diagnostics.Process.Start(currentExe);
                        Environment.Exit(0);
                    }
                }
                else
                {
                    await MainWindowViewModel.ShowMessageAsync(owner, "You're already on the latest version.");
                }
            }
            catch (Exception ex)
            {
                await MainWindowViewModel.ShowMessageAsync(owner, $"Failed to check for updates:\n{ex.Message}", "Error");
            }
        }

        public class UpdateInfo
        {
            public string Version { get; set; }
            public string Url { get; set; }
            public string Changelog { get; set; }
        }

    }
}
