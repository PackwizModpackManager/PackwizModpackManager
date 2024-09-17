using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Material.Colors;
using Material.Styles.Themes;
using Material.Styles.Themes.Base;
using Newtonsoft.Json;
using PackwizModpackManager.Themes;
using PackwizModpackManager.ViewModels;
using PackwizModpackManager.Views;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PackwizModpackManager
{
    public partial class App : Application
    {
        private MaterialTheme _materialTheme;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            _materialTheme = this.Styles.OfType<MaterialTheme>().FirstOrDefault();
        }

        public override void OnFrameworkInitializationCompleted()
        {
            Config config = LoadConfig();
            ApplyThemeFromConfig(config);

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        private Config LoadConfig()
        {
            string configFilePath = "config.json";
            if (File.Exists(configFilePath))
            {
                string json = File.ReadAllText(configFilePath);
                return JsonConvert.DeserializeObject<Config>(json);
            }
            else
            {
                // Return default config if file does not exist
                return new Config
                {
                    PackwizPath = "",
                    ProjectsFolder = "",
                    Language = "English",
                    SelectedTheme = "Dark Blue"
                };
            }
        }

        private void ApplyThemeFromConfig(Config config)
        {
            var themes = GetAvailableThemes();

            var selectedTheme = themes.FirstOrDefault(t => t.Name == config.SelectedTheme);

            if (selectedTheme != null)
            {
                SetTheme(selectedTheme.BaseTheme, selectedTheme.PrimaryColor, selectedTheme.SecondaryColor);
            }
            else
            {
                // Applies the default theme if the selected theme is not found
                SetTheme(BaseThemeMode.Dark, PrimaryColor.Blue, SecondaryColor.Cyan);
            }
        }

        // Método para cambiar el tema y los colores
        public void SetTheme(BaseThemeMode baseTheme, PrimaryColor primaryColor, SecondaryColor secondaryColor)
        {
            if (_materialTheme != null)
            {
                _materialTheme.BaseTheme = baseTheme;
                _materialTheme.PrimaryColor = primaryColor;
                _materialTheme.SecondaryColor = secondaryColor;
            }
        }

        private List<ThemeInfo> GetAvailableThemes()
        {
            return new List<ThemeInfo>
            {
               new ThemeInfo { Name = "Dark Blue", BaseTheme = BaseThemeMode.Dark, PrimaryColor = PrimaryColor.Blue, SecondaryColor = SecondaryColor.Pink },
               new ThemeInfo { Name = "Dark Green", BaseTheme = BaseThemeMode.Dark, PrimaryColor = PrimaryColor.Green, SecondaryColor = SecondaryColor.Pink },
               new ThemeInfo { Name = "Dark Red", BaseTheme = BaseThemeMode.Dark, PrimaryColor = PrimaryColor.Red, SecondaryColor = SecondaryColor.Pink },
               new ThemeInfo { Name = "Light Blue", BaseTheme = BaseThemeMode.Light, PrimaryColor = PrimaryColor.Blue, SecondaryColor = SecondaryColor.Cyan },
               new ThemeInfo { Name = "Light Green", BaseTheme = BaseThemeMode.Light, PrimaryColor = PrimaryColor.Green, SecondaryColor = SecondaryColor.Cyan },
               new ThemeInfo { Name = "Light Red", BaseTheme = BaseThemeMode.Light, PrimaryColor = PrimaryColor.Red, SecondaryColor = SecondaryColor.Cyan }
            };
        }

        private class Config
        {
            public string PackwizPath { get; set; }
            public string ProjectsFolder { get; set; }
            public string Language { get; set; }
            public string SelectedTheme { get; set; }
        }
    }
}