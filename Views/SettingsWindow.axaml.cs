using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using DynamicData;
using Material.Colors;
using Material.Styles.Themes;
using Material.Styles.Themes.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Media;
using Jeek.Avalonia.Localization;
using Avalonia.Data;
using PackwizModpackManager.Themes;

namespace PackwizModpackManager.Views
{
    public partial class SettingsWindow : Window
    {
        private const string ConfigFilePath = "config.json";
        private const string DefaultPackwizPath = "packwiz.exe";
        private const string DefaultProjectsFolder = "Proyectos";
        private List<ThemeInfo> themes = new List<ThemeInfo>();

        public SettingsWindow()
        {
            InitializeComponent();
            LoadThemeComboBox();
            LoadSettings();
        }

        private void LoadSettings()
        {
            if (File.Exists(ConfigFilePath))
            {
                var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigFilePath));
                PackwizPathTextBox.Text = config.PackwizPath;
                ProjectsFolderTextBox.Text = config.ProjectsFolder ?? DefaultProjectsFolder;
                LanguagesComboBox.SelectedItem = config.Language ?? "English";
                SetLanguageComboBox(config.Language ?? "English");
                SetThemeComboBox(config.SelectedTheme ?? "Dark Blue");
            }
            else
            {
                PackwizPathTextBox.Text = DefaultPackwizPath;
                ProjectsFolderTextBox.Text = DefaultProjectsFolder;
                LanguagesComboBox.SelectedItem = "English";
                SetLanguageComboBox("English");
            }
        }

        private void LoadThemeComboBox()
        {

            themes.Add(new ThemeInfo { Name = "Dark Blue", BaseTheme = BaseThemeMode.Dark, PrimaryColor = PrimaryColor.Blue, SecondaryColor = SecondaryColor.Pink });
            themes.Add(new ThemeInfo { Name = "Dark Green", BaseTheme = BaseThemeMode.Dark, PrimaryColor = PrimaryColor.Green, SecondaryColor = SecondaryColor.Pink });
            themes.Add(new ThemeInfo { Name = "Dark Red", BaseTheme = BaseThemeMode.Dark, PrimaryColor = PrimaryColor.Red, SecondaryColor = SecondaryColor.Pink });
            themes.Add(new ThemeInfo { Name = "Light Blue", BaseTheme = BaseThemeMode.Light, PrimaryColor = PrimaryColor.Blue, SecondaryColor = SecondaryColor.Cyan });
            themes.Add(new ThemeInfo { Name = "Light Green", BaseTheme = BaseThemeMode.Light, PrimaryColor = PrimaryColor.Green, SecondaryColor = SecondaryColor.Cyan });
            themes.Add(new ThemeInfo { Name = "Light Red", BaseTheme = BaseThemeMode.Light, PrimaryColor = PrimaryColor.Red, SecondaryColor = SecondaryColor.Cyan });

            ThemeSelectorCombobox.Items.Clear();
            ThemeSelectorCombobox.ItemsSource = themes;
            ThemeSelectorCombobox.DisplayMemberBinding = new Binding("Name");
        }

        private void SetLanguageComboBox(string language)
        {
            switch (language)
            {
                case "English":
                    LanguagesComboBox.SelectedIndex = 0;
                    break;
                case "Español":
                    LanguagesComboBox.SelectedIndex = 1;
                    break;
                // Add more cases by supported languages
                default:
                    LanguagesComboBox.SelectedIndex = 0;
                    break;
            }
        }

        private void SetThemeComboBox(string theme)
        {
            ThemeSelectorCombobox.SelectedIndex = themes.FindIndex(t => t.Name == theme);
        }

        private async void OnBuscarClick(object sender, RoutedEventArgs e)
        {

            var options = new FilePickerOpenOptions
            {
                Title = Localizer.Get("SettingPackwizBinarySelectText"),
                AllowMultiple = false,
                FileTypeFilter = new List<FilePickerFileType>
                {
                    new FilePickerFileType(Localizer.Get("Executables"))
                    {
                        Patterns = new[] { "*.exe", "*.bin", "*.sh" }
                    },
                    new FilePickerFileType(Localizer.Get("AllFiles"))
                    {
                        Patterns = new[] { "*" }
                    }
                }
            };

            // Show file selector
            var result = await this.StorageProvider.OpenFilePickerAsync(options);

            if (result != null && result.Count > 0)
            {
                var file = result[0];

                // Access to file path
                PackwizPathTextBox.Text = file.Path.LocalPath;
            }
        }

        private async void OnSeleccionarCarpetaClick(object sender, RoutedEventArgs e)
        {

            var options = new FolderPickerOpenOptions
            {
                Title = Localizer.Get("SettingsPackwizProjectFolderSelectText"),
                AllowMultiple = false
            };

            // Show folder selector
            var result = await this.StorageProvider.OpenFolderPickerAsync(options);

            if (result != null && result.Count > 0)
            {
                var folder = result[0];

                // Access to folder path
                ProjectsFolderTextBox.Text = folder.Path.LocalPath;
            }
        }

        private void OnGuardarClick(object sender, RoutedEventArgs e)
        {
            var config = new Config
            {
                PackwizPath = PackwizPathTextBox.Text,
                ProjectsFolder = ProjectsFolderTextBox.Text,
                Language = LanguagesComboBox.SelectedItem.ToString()
            };

            File.WriteAllText(ConfigFilePath, JsonConvert.SerializeObject(config));
            Close();
        }

        private void OnGuardarIdiomaClick(object sender, RoutedEventArgs e)
        {
            var config = new Config
            {
                PackwizPath = PackwizPathTextBox.Text,
                ProjectsFolder = ProjectsFolderTextBox.Text,
                Language = (LanguagesComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() // Guardar el idioma seleccionado
            };

            File.WriteAllText(ConfigFilePath, JsonConvert.SerializeObject(config));
            ApplyLanguage(config.Language); // Aplicar el idioma seleccionado
            Close();
        }

        private void OnGuardarTemaClick(object sender, RoutedEventArgs e)
        {

            var config = new Config
            {
                PackwizPath = PackwizPathTextBox.Text,
                ProjectsFolder = ProjectsFolderTextBox.Text,
                Language = (LanguagesComboBox.SelectedItem as ComboBoxItem)?.Content.ToString(), // Guardar el idioma seleccionado
                SelectedTheme = (ThemeSelectorCombobox.SelectedItem as ThemeInfo)?.Name // Guardar el tema seleccionado
            };

            File.WriteAllText(ConfigFilePath, JsonConvert.SerializeObject(config));
            var app = Application.Current as App;

            if (app != null)
            {
                if (ThemeSelectorCombobox.SelectedItem is ThemeInfo selectedTheme)
                {
                    ApplyTheme(selectedTheme);
                }
            }
        }

        private void ThemeSelectorCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeSelectorCombobox.SelectedItem is ThemeInfo selectedTheme)
            {
                ApplyTheme(selectedTheme);
            }
        }

        private void ApplyTheme(ThemeInfo theme)
        {
            var app = Application.Current as App;
            if (app != null)
            {
                app.SetTheme(theme.BaseTheme, theme.PrimaryColor, theme.SecondaryColor);
            }
        }

        private void ApplyLanguage(string language)
        {
            switch (language)
            {
                case "English":
                    Localizer.Language = "en-US";
                    break;
                case "Español":
                    Localizer.Language = "es-ES";
                    break;
                    // Añade más casos según los idiomas soportados
            }
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