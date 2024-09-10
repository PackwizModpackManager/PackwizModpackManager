using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Jeek.Avalonia.Localization;
using MsBox.Avalonia;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace PackwizModpackManager.Views
{
    public partial class MainWindow : Window
    {

        private const string ConfigFilePath = "config.json";

        public MainWindow()
        {
            InitializeComponent();
            LoadSettings();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void LoadSettings()
        {
            if (File.Exists(ConfigFilePath))
            {
                var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigFilePath));
                ApplyLanguage(config.Language ?? "en-US"); // Ajustar el idioma según la configuración
            }
            else
            {
                ApplyLanguage("en-US"); // Idioma predeterminado
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
                default:
                    Localizer.Language = "en-US";
                    break;
            }
        }

        private void OnCrearProyectoClick(object sender, RoutedEventArgs e)
        {
            var createProjectWindow = new CreateProjectWindow();
            createProjectWindow.ShowDialog(this);
        }

        private void OnAbrirProyectoClick(object sender, RoutedEventArgs e)
        {
            var projectSelectionWindow = new ProjectSelectionWindow();
            projectSelectionWindow.ShowDialog(this);
        }

        private void OnEliminarProyectoClick(object sender, RoutedEventArgs e)
        {
            // Lógica para eliminar un proyecto
        }

        private void OnConfiguracionClick(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog(this);
        }

        private class Config
        {
            public string PackwizPath { get; set; }
            public string ProjectsFolder { get; set; }
            public string Language { get; set; } // Nueva propiedad para el idioma
        }
    }
}