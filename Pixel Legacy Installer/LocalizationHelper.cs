using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using Pixel_Legacy_Installer.Resources;
using System;
using System.Linq;

namespace Pixel_Legacy_Installer.LocalizationHelper
{
    public class HelperLocalization
    {
        public void SetLanguage(string languageCode)
        {
            var app = Application.Current ?? throw new InvalidOperationException("Application.Current is null");

            IResourceDictionary? resourceDictionary = languageCode.ToLower() switch
            {
                "en" => new Strings_en(),
                "pl" => new Strings_pl(),
                "fi" => new Strings_fi(),
                "id" => new Strings_id(),
                "tr" => new Strings_tr(),
                _ => throw new NotSupportedException($"Language '{languageCode}' not supported")
            };

            app.Resources.MergedDictionaries.Clear();
            app.Resources.MergedDictionaries.Add(resourceDictionary);
        }

        public string GetCurrentLanguageCode()
        {
            var app = Application.Current ?? throw new InvalidOperationException("Application.Current is null");

            var dict = app.Resources.MergedDictionaries.FirstOrDefault();
            if (dict == null)
                return "en"; // Default or fallback

            return dict switch
            {
                Strings_en => "en",
                Strings_pl => "pl",
                Strings_fi => "fi",
                Strings_id => "id",
                Strings_tr => "tr",
                _ => "unknown"
            };
        }

    }
}