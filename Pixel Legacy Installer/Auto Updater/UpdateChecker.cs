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
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Pixel_Legacy_Installer.Auto_Updater
{
    internal class UpdateChecker
    {
        public static async Task CheckForUpdateAsync(Window? owner)
        {
            try
            {
                using var client = new HttpClient();

                // Required for GitHub raw and release URLs to work properly
                client.DefaultRequestHeaders.UserAgent.ParseAdd("PixelLegacyInstaller/1.0");

                var jsonUrl = "https://raw.githubusercontent.com/CharonXXIII/Pixel_Legacy_Installer/master/version.json";
                var json = await client.GetStringAsync(jsonUrl);

                var latest = JsonSerializer.Deserialize<UpdateInfo>(json);

                if (latest is null || string.IsNullOrEmpty(latest.Version))
                {
                    Debug.WriteLine("Version is newest (no version found)");
                    return;
                }

                if (latest.Version != VersionInfo.CurrentVersion)
                {
                    Debug.WriteLine("New version available");

                    var message = $"A new version ({latest.Version}) is available!\n\nChanges:\n{latest.Changelog}\n\nDo you want to update now?\n\nDO NOT TURN CLOSE THE APP IF YOU ARE UPDATING. IT WILL AUTO-RESTART ONCE COMPLETE!";
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

                        // Download update.zip
                        var response = await client.GetAsync(latest.Url);
                        response.EnsureSuccessStatusCode();

                        await using (var zipStream = await response.Content.ReadAsStreamAsync())
                        await using (var fileStream = File.Create(zipPath))
                        {
                            await zipStream.CopyToAsync(fileStream);
                        }

                        // Extract ZIP safely to temp folder
                        ZipFile.ExtractToDirectory(zipPath, tempFolder, overwriteFiles: true);

                        var currentExe = Process.GetCurrentProcess().MainModule?.FileName!;
                        var appDir = Path.GetDirectoryName(currentExe)!;

                        var updaterScript = Path.Combine(tempFolder, "update.bat");
                        var cleanupScript = Path.Combine(tempFolder, "cleanup.bat");

                        // cleanup.bat will run after update and remove the temp folder and files
                        var cleanupBatch = $@"
@echo off
timeout /t 3 > nul
if exist ""{zipPath}"" del /f /q ""{zipPath}""
if exist ""{updaterScript}"" del /f /q ""{updaterScript}""
if exist ""{cleanupScript}"" del /f /q ""{cleanupScript}""
rd /s /q ""{tempFolder}""
";

                        // update.bat performs the update and launches cleanup.bat
                        var updateBatch = $@"
@echo off
timeout /t 2 > nul

:: Copy update files
xcopy /E /Y /Q ""{tempFolder}"" ""{appDir}""

:: Restart launcher
start """" ""{currentExe}""

:: Run cleanup in background
start /min ""cleanup"" cmd /c ""\""{cleanupScript}\""""""
";

                        File.WriteAllText(updaterScript, updateBatch);
                        File.WriteAllText(cleanupScript, cleanupBatch);

                        Process.Start(new ProcessStartInfo
                        {
                            FileName = updaterScript,
                            CreateNoWindow = true,
                            UseShellExecute = false
                        });

                        Environment.Exit(0);
                    }
                }
                else
                {
                    await MainWindowViewModel.ShowMessageAsync(owner, $"Running Version: {VersionInfo.CurrentVersion} Of Pixel Legacy Launcher!");
                }
            }
            catch (Exception ex)
            {
                await MainWindowViewModel.ShowMessageAsync(owner, $"Failed to check for updates:\n{ex.Message}", "Error");
            }
        }


        public class UpdateInfo
        {
            [JsonPropertyName("version")]
            public string Version { get; set; }

            [JsonPropertyName("url")]
            public string Url { get; set; }

            [JsonPropertyName("changelog")]
            public string Changelog { get; set; }
        }

    }
}
