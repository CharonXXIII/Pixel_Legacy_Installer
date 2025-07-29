using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Globalization;
using System.Threading;

namespace Pixel_Legacy_Installer.ViewModels
{
    public class ViewModelBase : ObservableObject
    {
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

        private string _selectedLanguage;
        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (_selectedLanguage != value)
                {
                    _selectedLanguage = value;
                    OnPropertyChanged();

                    // Apply the selected language
                    ApplyLanguage(value);
                }
            }
        }

        private void ApplyLanguage(string language)
        {
            CultureInfo culture = language switch
            {
                "Finnish" => new CultureInfo("fi-FI"),
                "Polish" => new CultureInfo("pl-PL"),
                "Turkish" => new CultureInfo("tr-TR"),
                _ => new CultureInfo("en-US")
            };

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            var app = Avalonia.Application.Current;
            app.Resources.MergedDictionaries.Clear();

            string langCode = culture.TwoLetterISOLanguageName;

            string resPath = $"avares://Pixel_Legacy_Installer/Resources/Strings.{langCode}.axaml";

            var dict = AvaloniaXamlLoader.Load(new Uri(resPath)) as IResourceDictionary;

            if (dict != null)
            {
                app.Resources.MergedDictionaries.Add(dict);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load language resource: {resPath}");
            }
        }



    }
}