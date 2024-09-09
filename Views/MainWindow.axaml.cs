using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MsBox.Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace PackwizModpackManager.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
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
    }
}