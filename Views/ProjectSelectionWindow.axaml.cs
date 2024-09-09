using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MsBox.Avalonia;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace PackwizModpackManager.Views
{
    public partial class ProjectSelectionWindow : Window
    {
        private const string ConfigFilePath = "config.json";
        private const string DefaultProjectsFolder = "Proyectos";
        private Dictionary<string, string> projectMap;

        public ProjectSelectionWindow()
        {
            InitializeComponent();
            LoadProjects();
        }

        private string GetProjectsFolder()
        {
            if (File.Exists(ConfigFilePath))
            {
                var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigFilePath));
                return config.ProjectsFolder ?? DefaultProjectsFolder;
            }
            return DefaultProjectsFolder;
        }

        private void LoadProjects()
        {
            projectMap = new Dictionary<string, string>();
            string projectsFolder = GetProjectsFolder();

            if (Directory.Exists(projectsFolder))
            {
                var projectDirectories = Directory.GetDirectories(projectsFolder);
                var projectNames = new List<string>();

                foreach (var dir in projectDirectories)
                {
                    string folderName = Path.GetFileName(dir);
                    string projectName = folderName.Replace("_", " ");
                    projectMap[projectName] = folderName;
                    ProjectListBox.Items.Add(projectName);
                }
            }
            else
            {
                ProjectListBox.Items.Clear();
                ProjectListBox.Items.Add("No se encontraron proyectos.");   
            }
        }

        private async void OnAbrirClick(object sender, RoutedEventArgs e)
        {
            var selectedProject = ProjectListBox.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedProject) && projectMap.ContainsKey(selectedProject))
            {
                string folderName = projectMap[selectedProject];
                string projectPath = Path.Combine(GetProjectsFolder(), folderName);

                // Abrir la ventana de detalles del proyecto
                var projectDetailsWindow = new ProjectDetailsWindow(projectPath);
                projectDetailsWindow.Show();
                this.Close();
            }
            else
            {
                var messageBox = MessageBoxManager.GetMessageBoxStandard("Error", "Por favor, seleccione un proyecto.");
                await messageBox.ShowWindowDialogAsync(this);
            }
        }

        private class Config
        {
            public string ProjectsFolder { get; set; }
        }
    }
}