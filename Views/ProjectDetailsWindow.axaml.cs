using Avalonia.Controls;
using Avalonia.Interactivity;
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
            this.Focus();
        }

        public ProjectDetailsWindow(string projectPath) : this()
        {
            this.projectPath = projectPath;
            LoadMods();
            LoadPackToml(); // Aseg�rate de que esta funci�n se est� llamando
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
                    // Establecer el DataContext del Window a s� mismo para exponer la propiedad PackToml
                    ForgeVersionTextBox.Text = PackToml.ForgeVersion;
                    MinecraftVersionTextBox.Text = PackToml.MinecraftVersion;
                    this.DataContext = PackToml;
                }
            }
            else
            {
                var messageBox = MessageBoxManager.GetMessageBoxStandard("Error", "El archivo pack.toml no existe.");
                await messageBox.ShowWindowDialogAsync(this);
            }
        }

        private async void OnAddModClick(object sender, RoutedEventArgs e)
        {
            string modSource = (ModSourceComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            string modId = ModIdTextBox.Text;
            bool skipConfirmation = SkipConfirmationCheckBox.IsChecked ?? false;

            if (string.IsNullOrEmpty(modSource) || string.IsNullOrEmpty(modId))
            {
                var messageBox = MessageBoxManager.GetMessageBoxStandard("Error", "Por favor, complete todos los campos.");
                await messageBox.ShowWindowDialogAsync(this);
                return;
            }

            // Construir los argumentos del comando para a�adir el mod
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

            // Ejecutar el comando de Packwiz para a�adir el mod
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
                    // L�gica para actualizar el mod
                    UpdateMod(modFileName);
                    LoadMods(); // Recargar la lista de mods despu�s de actualizar
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
            // Implementa la l�gica para actualizar el mod aqu�
            // Por ejemplo, podr�as descargar la �ltima versi�n del mod y reemplazar el archivo existente
            string arguments = $"update " + modFileName;
            await ExecutePackwizCommand(arguments);
        }

        private async void OnRefreshModpackClick(object sender, RoutedEventArgs e)
        {
            // Ejecutar el comando de Packwiz para refrescar los hashes
            await ExecutePackwizCommand("refresh");

            var messageBox = MessageBoxManager.GetMessageBoxStandard("�xito", "Modpack refrescado exitosamente.");
            await messageBox.ShowWindowDialogAsync(this);
        }

        private async void OnUpdateAllModsClick(object sender, RoutedEventArgs e)
        {
            // Construir los argumentos del comando para actualizar todos los mods
            string arguments = "update --all";

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

            var messageBox = MessageBoxManager.GetMessageBoxStandard("�xito", "Cambios guardados exitosamente.");
            await messageBox.ShowWindowDialogAsync(this);
        }

        private async void OnModSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedMod = ModsListBox.SelectedItem as Mod;
            if (selectedMod != null)
            {
                // Aqu� puedes a�adir l�gica adicional si necesitas hacer algo cuando se selecciona un mod
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

                // Mostrar mensaje de �xito
                var messageBox = MessageBoxManager.GetMessageBoxStandard("�xito", "Modpack copiado exitosamente.");
                await messageBox.ShowWindowDialogAsync(this);
            }
            else
            {
                // Mostrar mensaje de �xito
                var messageBox = MessageBoxManager.GetMessageBoxStandard("Error", "El directorio de mods no existe.");
                await messageBox.ShowWindowDialogAsync(this);
            }
        }

        private void DirectoryCopy(string sourceDirName, string destDirName)
        {
            // Obtiene la informaci�n del directorio
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
                var messageBox = MessageBoxManager.GetMessageBoxStandard("�xito", "Modpack subido exitosamente.");
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
                var messageBox = MessageBoxManager.GetMessageBoxStandard("Advertencia", "Por favor, selecciona un tipo de exportaci�n.");
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

                var messageBox = MessageBoxManager.GetMessageBoxStandard("�xito", "Cambios guardados exitosamente.");
                await messageBox.ShowWindowDialogAsync(this);
            }

            await ExecutePackwizCommand("refresh");
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
                        var messageBox = MessageBoxManager.GetMessageBoxStandard("�xito", "Operaci�n completada exitosamente:\n" + output);
                        await messageBox.ShowWindowDialogAsync(this);
                    }
                    else
                    {
                        var messageBox = MessageBoxManager.GetMessageBoxStandard("Error", "Error en la operaci�n:\n" + error);
                        await messageBox.ShowWindowDialogAsync(this);
                    }
                }
            }
            catch (Exception ex)
            {
                var messageBox = MessageBoxManager.GetMessageBoxStandard("Error", "Ocurri� un error:\n" + ex.Message);
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

