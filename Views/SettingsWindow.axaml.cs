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
using Semi.Avalonia;
using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Media;

namespace PackwizModpackManager.Views
{
    public partial class SettingsWindow : Window
    {
        private const string ConfigFilePath = "config.json";
        private const string DefaultPackwizPath = "packwiz.exe";
        private const string DefaultProjectsFolder = "Proyectos";

        public SettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            if (File.Exists(ConfigFilePath))
            {
                var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigFilePath));
                PackwizPathTextBox.Text = config.PackwizPath;
                ProjectsFolderTextBox.Text = config.ProjectsFolder ?? DefaultProjectsFolder;
            }
            else
            {
                PackwizPathTextBox.Text = DefaultPackwizPath;
                ProjectsFolderTextBox.Text = DefaultProjectsFolder;
            }
        }

        private async void OnBuscarClick(object sender, RoutedEventArgs e)
        {
            var result = await new OpenFileDialog()
            {
                Title = "Seleccionar ejecutable de Packwiz",
                Filters = new List<FileDialogFilter>
                {
                    new FileDialogFilter { Name = "Ejecutables", Extensions = new List<string> { "exe", "bin", "sh" } },
                    new FileDialogFilter { Name = "Todos los archivos", Extensions = new List<string> { "*" } }
                }
            }.ShowAsync(this);

            if (result != null && result.Length > 0)
            {
                PackwizPathTextBox.Text = result[0];
            }
        }

        private async void OnSeleccionarCarpetaClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Seleccionar carpeta de proyectos"
            };

            var result = await dialog.ShowAsync(this);
            if (!string.IsNullOrEmpty(result))
            {
                ProjectsFolderTextBox.Text = result;
            }
        }

        private void OnGuardarClick(object sender, RoutedEventArgs e)
        {
            var config = new Config
            {
                PackwizPath = PackwizPathTextBox.Text,
                ProjectsFolder = ProjectsFolderTextBox.Text
            };

            File.WriteAllText(ConfigFilePath, JsonConvert.SerializeObject(config));
            Close();
        }

        private void OnThemeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string themeTag = selectedItem.Tag.ToString();
                ApplyTheme(themeTag);
            }
        }

        private void ApplyTheme(string themeTag)
        {
            Styles styles = Application.Current.Styles;
            styles.Clear();

            switch (themeTag)
            {
                case "Material":
                    styles.Add(Application.Current.LocateMaterialTheme<MaterialTheme>());
                    break;
                case "Semi":
                    styles.Add(new SemiTheme());
                    break;
                case "Fluent":
                    styles.Add(new Avalonia.Themes.Fluent.FluentTheme());
                    break;
            }
        }

        private class Config
        {
            public string PackwizPath { get; set; }
            public string ProjectsFolder { get; set; }
        }
    }
}