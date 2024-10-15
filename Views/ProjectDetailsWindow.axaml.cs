using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Jeek.Avalonia.Localization;
using MsBox.Avalonia;
using Newtonsoft.Json;
using PackwizModpackManager.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Tomlyn;
using Tomlyn.Model;

namespace PackwizModpackManager.Views
{
    public partial class ProjectDetailsWindow : Window
    {
        private string projectPath;
        private string selectedDirectory;
        public PackToml PackToml { get; set; }


        public ProjectDetailsWindow()
        {
            InitializeComponent();
        }

        public ProjectDetailsWindow(string projectPath) : this()
        {
            InitializeComponent();
            //Ensure to load before any mod the project folder to get the mods
            this.projectPath = projectPath;

            //Update GUI here with mods and pack.toml
            LoadMods();
            LoadPackToml(); // Asegúrate de que esta función se esté llamando
        }

        private void LoadMods()
        {
            ModsListBox.Items.Clear();
            var mods = new List<Mod>();
            string modsDirectory = Path.Combine(projectPath, "mods");

            if (Directory.Exists(modsDirectory))
            {
                var modFiles = Directory.GetFiles(modsDirectory, "*.pw.toml");
                foreach (var modFile in modFiles)
                {
                    var tomlContent = File.ReadAllText(modFile);
                    var tomlTable = Toml.Parse(tomlContent).ToModel();
                    if (tomlTable is TomlTable table && table.TryGetValue("name", out var modName))
                    {
                        var side = table.TryGetValue("side", out var modSide) ? modSide.ToString() : "both";
                        ModsListBox.Items.Add(new Mod { Name = modName.ToString(), Side = side, FilePath = modFile });
                    }
                }
            }
        }

        private async void LoadPackToml()
        {
            string packTomlPath = Path.Combine(projectPath, "pack.toml");
            if (File.Exists(packTomlPath))
            {
                var tomlContent = File.ReadAllText(packTomlPath);
                PackToml = PackToml.FromToml(tomlContent); // Cargar el TOML en la propiedad

                if (PackToml != null)
                {
                    // Establecer el DataContext del Window a sí mismo para exponer la propiedad PackToml
                    UpdateModLoaderUI(PackToml);
                    this.DataContext = PackToml;
                }
            }
            else
            {
                var messageBox = MessageBoxManager.GetMessageBoxStandard(Localizer.Get("Error"), Localizer.Get("DetailsPackNotExist"));

                // Obtener la ventana principal de la aplicación
                var mainWindow = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

                // Usar 'this' si está visible; de lo contrario, usar la ventana principal
                var ownerWindow = this.IsVisible ? this : mainWindow;

                if (ownerWindow != null)
                {
                    await messageBox.ShowWindowDialogAsync(ownerWindow);
                }
                else
                {
                    // Si no hay una ventana propietaria disponible, muestra el mensaje sin propietario
                    await messageBox.ShowAsync();
                }
            }
        }

        private async void OnAddModClick(object sender, RoutedEventArgs e)
        {
            string modSource = (ModSourceComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            string modId = ModIdTextBox.Text;
            bool skipConfirmation = SkipConfirmationCheckBox.IsChecked ?? false;

            if (string.IsNullOrEmpty(modSource) || string.IsNullOrEmpty(modId))
            {
                var messageBox = MessageBoxManager.GetMessageBoxStandard(Localizer.Get("Error"), Localizer.Get("DetailsAddModCompleteFields"));
                await messageBox.ShowWindowDialogAsync(this);
                return;
            }

            // Construir los argumentos del comando para añadir el mod
            string arguments = modSource.ToLower() switch
            {
                "curseforge" => $"curseforge add {modId}",
                "modrinth" => $"modrinth add {modId}",
                _ => throw new InvalidOperationException("Fuente de mod no soportada")
            };

            if (skipConfirmation)
            {
                arguments += " -y";
            }

            // Ejecutar el comando de Packwiz para añadir el mod
            await ExecutePackwizCommand(arguments);
            LoadMods(); // Actualizar la lista de mods
            ModIdTextBox.Text = ""; // Limpiar el campo de ID del mod
        }

        private async void OnUpdateSelectedModClick(object sender, RoutedEventArgs e)
        {
            if (ModsListBox.SelectedItem is Mod selectedMod)
            {
                string modFileName = Path.GetFileNameWithoutExtension(selectedMod.FilePath);

                int lastDotIndex = modFileName.LastIndexOf('.');
                if (lastDotIndex > 0)
                {
                    modFileName = modFileName.Substring(0, lastDotIndex);
                }

                string modFilePath = Path.Combine(projectPath, "mods", modFileName + ".pw.toml");

                if (File.Exists(modFilePath))
                {
                    // Lógica para actualizar el mod
                    UpdateMod(modFileName);
                    LoadMods(); // Recargar la lista de mods después de actualizar
                }
                else
                {
                    var messageBox = MessageBoxManager.GetMessageBoxStandard("Error",$"El archivo {modFileName}.pw.toml no existe.");
                    await messageBox.ShowWindowDialogAsync(this);
                    return;
                }
            }
            else
            {
                var messageBox = MessageBoxManager.GetMessageBoxStandard("Error", "Por favor, seleccione un mod.");
                await messageBox.ShowWindowDialogAsync(this);
                return;
            }
        }

        private async void UpdateMod(string modFileName)
        {
            // Implementa la lógica para actualizar el mod aquí
            // Por ejemplo, podrías descargar la última versión del mod y reemplazar el archivo existente
            string arguments = $"update " + modFileName;
            await ExecutePackwizCommand(arguments);
        }

        private async void OnRefreshModpackClick(object sender, RoutedEventArgs e)
        {
            // Ejecutar el comando de Packwiz para refrescar los hashes
            await ExecutePackwizCommand("refresh");

            var messageBox = MessageBoxManager.GetMessageBoxStandard("Éxito", "Modpack refrescado exitosamente.");
            await messageBox.ShowWindowDialogAsync(this);
        }

        private async void OnUpdateAllModsClick(object sender, RoutedEventArgs e)
        {
            // Construir los argumentos del comando para actualizar todos los mods
            string arguments = "update --all --yes";

            // Ejecutar el comando de Packwiz para actualizar todos los mods
            await ExecutePackwizCommand(arguments);
        }

        private async void OnDeleteSelectedModClick(object sender, RoutedEventArgs e)
        {
            var selectedMod = ModsListBox.SelectedItem as Mod;
            if (selectedMod == null)
            {
                var messageBox = MessageBoxManager.GetMessageBoxStandard("Error", "Por favor, seleccione un mod.");
                await messageBox.ShowWindowDialogAsync(this);
                return;
            }

            // Eliminar el archivo del mod
            if (File.Exists(selectedMod.FilePath))
            {
                File.Delete(selectedMod.FilePath);
            }

            // Ejecutar el comando de Packwiz para refrescar los hashes
            await ExecutePackwizCommand("refresh");

            // Actualizar la lista de mods
            LoadMods();
        }

        private async void OnSaveChangesClick(object sender, RoutedEventArgs e)
        {
            foreach (var item in ModsListBox.Items)
            {
                if (item is Mod mod)
                {
                    var tomlContent = File.ReadAllText(mod.FilePath);
                    var tomlTable = Toml.Parse(tomlContent).ToModel();

                    if (tomlTable is TomlTable table)
                    {
                        table["side"] = mod.Side;
                        var updatedTomlContent = Toml.FromModel(table);
                        File.WriteAllText(mod.FilePath, updatedTomlContent);
                    }
                }
            }

            // Ejecutar el comando de Packwiz para refrescar los hashes
            await ExecutePackwizCommand("refresh");

            var messageBox = MessageBoxManager.GetMessageBoxStandard("Éxito", "Cambios guardados exitosamente.");
            await messageBox.ShowWindowDialogAsync(this);
        }

        private async void OnModSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedMod = ModsListBox.SelectedItem as Mod;
            if (selectedMod != null)
            {
                // Aquí puedes añadir lógica adicional si necesitas hacer algo cuando se selecciona un mod
                Console.WriteLine($"Mod seleccionado: {selectedMod.Name}, Side: {selectedMod.Side}");
            }
        }

        private async void OnRefreshModsClick(object sender, RoutedEventArgs e)
        {
            LoadMods();
        }

        private async void OnSelectDirectoryClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();
            selectedDirectory = await dialog.ShowAsync(this);
        }

        private async void OnCopyModpackClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(selectedDirectory))
            {
                var messageBox = MessageBoxManager.GetMessageBoxStandard("Advertencia", "Por favor, selecciona un directorio.");
                await messageBox.ShowWindowDialogAsync(this);
                return;
            }

            if (Directory.Exists(projectPath))
            {
                await Task.Run(() => DirectoryCopy(projectPath, selectedDirectory));

                // Mostrar mensaje de éxito
                var messageBox = MessageBoxManager.GetMessageBoxStandard("Éxito", "Modpack copiado exitosamente.");
                await messageBox.ShowWindowDialogAsync(this);
            }
            else
            {
                // Mostrar mensaje de éxito
                var messageBox = MessageBoxManager.GetMessageBoxStandard("Error", "El directorio de mods no existe.");
                await messageBox.ShowWindowDialogAsync(this);
            }
        }

        private void DirectoryCopy(string sourceDirName, string destDirName)
        {
            // Obtiene la información del directorio
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            // Verifica que el directorio de origen exista
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException($"El directorio fuente no existe: {sourceDirName}");
            }

            // Crea el directorio de destino si no existe
            Directory.CreateDirectory(destDirName);

            // Copia todos los archivos en el directorio actual
            foreach (FileInfo file in dir.GetFiles())
            {
                string destFile = Path.Combine(destDirName, file.Name);
                file.CopyTo(destFile, true); // Sobreescribe archivos si ya existen
            }

            // Copia todos los subdirectorios recursivamente
            foreach (DirectoryInfo subdir in dir.GetDirectories())
            {
                string destSubDir = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, destSubDir); // Llamada recursiva
            }
        }

        private async void OnUploadModpackClick(object sender, RoutedEventArgs e)
        {
            string ftpServer = FtpServerTextBox.Text;
            string ftpUsername = FtpUsernameTextBox.Text;
            string ftpPassword = FtpPasswordBox.Text;

            if (string.IsNullOrEmpty(ftpServer) || string.IsNullOrEmpty(ftpUsername) || string.IsNullOrEmpty(ftpPassword))
            {
                var messageBox = MessageBoxManager.GetMessageBoxStandard("Advertencia", "Por favor, completa todos los campos de FTP.");
                await messageBox.ShowWindowDialogAsync(this);
                return;
            }

            string modsDirectory = Path.Combine(projectPath, "mods");
            if (Directory.Exists(modsDirectory))
            {
                foreach (var file in Directory.GetFiles(modsDirectory))
                {
                    string fileName = Path.GetFileName(file);
                    string ftpFilePath = $"ftp://{ftpServer}/{fileName}";

                    using (WebClient client = new WebClient())
                    {
                        client.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
                        await client.UploadFileTaskAsync(new Uri(ftpFilePath), WebRequestMethods.Ftp.UploadFile, file);
                    }
                }
                var messageBox = MessageBoxManager.GetMessageBoxStandard("Éxito", "Modpack subido exitosamente.");
                await messageBox.ShowWindowDialogAsync(this);
            }
            else
            {
                var messageBox = MessageBoxManager.GetMessageBoxStandard("Error", "El directorio de mods no existe.");
                await messageBox.ShowWindowDialogAsync(this);
            }
        }

        private async void OnExportModpackClick(object sender, RoutedEventArgs e)
        {
            if (ExportTypeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string exportType = selectedItem.Tag.ToString();
                string command = $"{exportType} export";

                await ExecutePackwizCommand(command);

                if (OpenFolderCheckBox.IsChecked == true)
                {
                    OpenProjectFolder();
                }
            }
            else
            {
                var messageBox = MessageBoxManager.GetMessageBoxStandard("Advertencia", "Por favor, selecciona un tipo de exportación.");
                await messageBox.ShowWindowDialogAsync(this);
            }
        }

        private async void OnSavePackTomlClick(object sender, RoutedEventArgs e)
        {
            if (PackToml != null)
            {
                PackToml.ForgeVersion = ForgeVersionTextBox.Text;
                PackToml.MinecraftVersion = MinecraftVersionTextBox.Text;

                string packTomlPath = Path.Combine(projectPath, "pack.toml");
                var updatedTomlContent = PackToml.ToToml();
                File.WriteAllText(packTomlPath, updatedTomlContent);

                var messageBox = MessageBoxManager.GetMessageBoxStandard("Éxito", "Cambios guardados exitosamente.");
                await messageBox.ShowWindowDialogAsync(this);
            }

            await ExecutePackwizCommand("refresh");
        }

        private void UpdateModLoaderUI(PackToml packToml)
        {
            // Mostrar/ocultar TextBox en función de si hay versión para ese modloader

            //Forge 
            ForgeVersionText.IsVisible = string.IsNullOrEmpty(packToml.ForgeVersion) ? IsVisible = false : IsVisible = true;
            ForgeVersionTextBox.IsVisible = string.IsNullOrEmpty(packToml.ForgeVersion) ? IsVisible = false : IsVisible = true;

            //NeoForge
            NeoForgeVersionText.IsVisible = string.IsNullOrEmpty(packToml.NeoForgeVersion) ? IsVisible = false : IsVisible = true;
            NeoForgeVersionTextBox.IsVisible = string.IsNullOrEmpty(packToml.NeoForgeVersion) ? IsVisible = false : IsVisible = true;

            //Fabric
            FabricVersionText.IsVisible = string.IsNullOrEmpty(packToml.FabricVersion) ? IsVisible = false : IsVisible = true;
            FabricVersionTextBox.IsVisible = string.IsNullOrEmpty(packToml.FabricVersion) ? IsVisible = false : IsVisible = true;

            //Quilt
            QuiltVersionText.IsVisible = string.IsNullOrEmpty(packToml.QuiltVersion) ? IsVisible = false : IsVisible = true;
            QuiltVersionTextBox.IsVisible = string.IsNullOrEmpty(packToml.QuiltVersion) ? IsVisible = false : IsVisible = true;
        }

        private async Task ExecutePackwizCommand(string arguments)
        {
            string packwizPath = GetPackwizPath();

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
                        var messageBox = MessageBoxManager.GetMessageBoxStandard("Éxito", "Operación completada exitosamente:\n" + output);
                        await messageBox.ShowWindowDialogAsync(this);
                    }
                    else
                    {
                        var messageBox = MessageBoxManager.GetMessageBoxStandard("Error", "Error en la operación:\n" + error);
                        await messageBox.ShowWindowDialogAsync(this);
                    }
                }
            }
            catch (Exception ex)
            {
                var messageBox = MessageBoxManager.GetMessageBoxStandard("Error", "Ocurrió un error:\n" + ex.Message);
                await Clipboard.SetTextAsync(ex.Message);
                await messageBox.ShowWindowDialogAsync(this);
            }
        }

        private string GetPackwizPath()
        {
            const string ConfigFilePath = "config.json";
            const string DefaultPackwizPath = "packwiz.exe";

            if (File.Exists(ConfigFilePath))
            {
                var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigFilePath));
                return config.PackwizPath ?? DefaultPackwizPath;
            }
            return DefaultPackwizPath;
        }

        private void OpenProjectFolder()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                Arguments = projectPath,
                FileName = "explorer.exe"
            };
            Process.Start(startInfo);
        }

        private class Config
        {
            public string PackwizPath { get; set; }
        }
    }
}

