using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Jeek.Avalonia.Localization;
using Avalonia.Threading;

namespace PackwizModpackManager.Views;

public partial class CreateProjectWindow : Window
{
    private const string ConfigFilePath = "config.json";
    private const string DefaultPackwizPath = "packwiz.exe";
    private const string DefaultProjectsFolder = "Proyectos";
    private string projectsFolder;

    public CreateProjectWindow()
    {
        InitializeComponent();
        LoadSettings();
        InitializeComboBoxes();
    }

    private async void InitializeComboBoxes()
    {
        // URL del JSON de versiones de Minecraft
        string mcVersionsUrl = "https://launchermeta.mojang.com/mc/game/version_manifest.json";
        List<string> mcVersions = new List<string>();
        using (HttpClient client = new HttpClient())
        {
            try
            {
                string json = await client.GetStringAsync(mcVersionsUrl);
                var versionManifest = JObject.Parse(json);
                var versions = versionManifest["versions"];
                foreach (var version in versions)
                {
                    if (version["type"].ToString() == "release")
                    {
                        McVersionComboBox.Items.Add(version["id"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                var messageBox = MessageBoxManager.GetMessageBoxStandard(Localizer.Get("Error"), "Error al cargar las versiones de Minecraft:\n" + ex.Message);
                await messageBox.ShowWindowDialogAsync(this);
            }
        }

        // Configurar opciones de mod loader
        var modLoaders = new List<string> { "Forge", "Fabric", "Quilt", "NeoForge" };
        foreach (var loader in modLoaders)
        {
            ModLoaderComboBox.Items.Add(loader);
        }
    }
    private async void OnCrearProyectoClick(object sender, RoutedEventArgs e)
    {
        string projectName = ProjectNameTextBox.Text;
        string author = AuthorTextBox.Text;
        string version = VersionTextBox.Text;
        string mcVersion = McVersionComboBox.SelectedItem?.ToString();
        string modLoader = ModLoaderComboBox.SelectedItem?.ToString();

        if (string.IsNullOrEmpty(projectName) || string.IsNullOrEmpty(author) || string.IsNullOrEmpty(version) || string.IsNullOrEmpty(mcVersion) || string.IsNullOrEmpty(modLoader))
        {
            var messageBox = MessageBoxManager.GetMessageBoxStandard(Localizer.Get("Error"), "Por favor, complete todos los campos.");
            await messageBox.ShowWindowDialogAsync(this);
            return;
        }


        // Sanitize project name to avoid problematic characters
        projectName = SanitizeProjectName(projectName);

        string projectPath = Path.Combine(projectsFolder, projectName);

        // Crear la carpeta del proyecto
        Directory.CreateDirectory(projectPath);

        // Obtener la ruta del ejecutable de Packwiz desde la configuración
        string packwizPath = GetPackwizPath();
        if (string.IsNullOrEmpty(packwizPath))
        {
            var messageBox = MessageBoxManager.GetMessageBoxStandard(Localizer.Get("Error"), "La ruta del ejecutable de Packwiz no está configurada.");
            await messageBox.ShowWindowDialogAsync(this);
            return;
        }

        // Construir los argumentos del comando
        string latestFlag = modLoader.ToLower() switch
        {
            "forge" => "--forge-latest",
            "fabric" => "--fabric-latest",
            "quilt" => "--quilt-latest",
            "neoforge" => "--neoforge-latest",
            _ => throw new InvalidOperationException("Mod loader no soportado")
        };

        string arguments = $"init --name \"{projectName}\" --author \"{author}\" --version \"{version}\" --mc-version \"{mcVersion}\" --modloader \"{modLoader}\" {latestFlag}";

        // Ejecutar el comando de Packwiz para crear el proyecto
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = packwizPath,
            Arguments = arguments,
            WorkingDirectory = projectPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using (Process process = new Process { StartInfo = startInfo })
            {
                process.Start();

                // Leer la salida del proceso
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                process.WaitForExit();

                // Mostrar la salida o el error al usuario
                if (process.ExitCode == 0)
                {
                    var messageBox = MessageBoxManager.GetMessageBoxStandard(Localizer.Get("Success"), Localizer.Get("CreateProjectSuccess") + "\n" + output);
                    await messageBox.ShowWindowDialogAsync(this);

                    // Open the project manager window
                    var projectDetailsWindow = new ProjectDetailsWindow(projectPath);
                    projectDetailsWindow.Show();

                    // Put focus on the new created window
                    Dispatcher.UIThread.Post(() =>
                    {
                        projectDetailsWindow.Activate();
                    }, DispatcherPriority.Background);

                    this.Close();
                }
                else
                {
                    var messageBox = MessageBoxManager.GetMessageBoxStandard(Localizer.Get("Error"), Localizer.Get("CreateProjectError") + "\n" + output);
                    await messageBox.ShowWindowDialogAsync(this);
                }
            }
        }
        catch (Exception ex)
        {
            var messageBox = MessageBoxManager.GetMessageBoxStandard(Localizer.Get("Error"), "Ocurrió un error:\n" + ex.Message);
            await Clipboard.SetTextAsync(ex.Message);
            await messageBox.ShowWindowDialogAsync(this);
        }

        this.Close();
    }

    private string GetPackwizPath()
    {
        if (File.Exists(ConfigFilePath))
        {
            var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigFilePath));
            return config.PackwizPath;
        }
        return DefaultPackwizPath;
    }

    private void LoadSettings()
    {
        if (File.Exists(ConfigFilePath))
        {
            var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigFilePath));
            projectsFolder = config.ProjectsFolder ?? DefaultProjectsFolder;
        }
        else
        {
            projectsFolder = DefaultProjectsFolder;
        }
    }

    private class Config
    {
        public string PackwizPath { get; set; }
        public string ProjectsFolder { get; set; }
        public string Language { get; set; }
    }

    private string SanitizeProjectName(string projectName)
    {
        // Replace spaces with underscores and remove invalid characters
        return string.Join("_", projectName.Split(Path.GetInvalidFileNameChars()));
    }
}