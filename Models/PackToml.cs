using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomlyn.Model;
using Tomlyn;
using System.ComponentModel;

namespace PackwizModpackManager.Models
{
    public class PackToml : INotifyPropertyChanged
    {
        private string name;
        private string version;
        private string author;
        private string packFormat; // Cambia el tipo de int a string
        private string forgeVersion;
        public string NeoForgeVersion { get; set; }
        public string FabricVersion { get; set; }
        public string QuiltVersion { get; set; }

        private string minecraftVersion;

        public string IndexFile { get; set; }
        public string HashFormat { get; set; }
        public string Hash { get; set; }

        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public string Version
        {
            get => version;
            set
            {
                if (version != value)
                {
                    version = value;
                    OnPropertyChanged(nameof(Version));
                }
            }
        }

        public string Author
        {
            get => author;
            set
            {
                if (author != value)
                {
                    author = value;
                    OnPropertyChanged(nameof(Author));
                }
            }
        }

        public string PackFormat // Ahora es string
        {
            get => packFormat;
            private set
            {
                if (packFormat != value)
                {
                    packFormat = value;
                    OnPropertyChanged(nameof(PackFormat));
                }
            }
        }

        public string ForgeVersion
        {
            get => forgeVersion;
            set
            {
                if (forgeVersion != value)
                {
                    forgeVersion = value;
                    OnPropertyChanged(nameof(ForgeVersion));
                }
            }
        }

        public string MinecraftVersion
        {
            get => minecraftVersion;
            set
            {
                if (minecraftVersion != value)
                {
                    minecraftVersion = value;
                    OnPropertyChanged(nameof(MinecraftVersion));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static PackToml FromToml(string tomlContent)
        {
            var tomlTable = Toml.Parse(tomlContent).ToModel();
            if (tomlTable is TomlTable table)
            {
                var packToml = new PackToml
                {
                    Name = table.TryGetValue("name", out var name) ? name.ToString() : "",
                    Version = table.TryGetValue("version", out var version) ? version.ToString() : "",
                    Author = table.TryGetValue("author", out var author) ? author.ToString() : "",
                    PackFormat = table.TryGetValue("pack-format", out var packFormat) ? packFormat.ToString() : ""
                };

                // Sección [index] (si es necesario capturar)
                if (table.TryGetValue("index", out var index) && index is TomlTable indexTable)
                {
                    packToml.IndexFile = indexTable.TryGetValue("file", out var indexFile) ? indexFile.ToString() : "";
                    packToml.HashFormat = indexTable.TryGetValue("hash-format", out var hashFormat) ? hashFormat.ToString() : "";
                    packToml.Hash = indexTable.TryGetValue("hash", out var hash) ? hash.ToString() : "";
                }

                // Sección [versions]
                if (table.TryGetValue("versions", out var versions) && versions is TomlTable versionsTable)
                {
                    packToml.ForgeVersion = versionsTable.TryGetValue("forge", out var forgeVersion) ? forgeVersion.ToString() : "";
                    packToml.NeoForgeVersion = versionsTable.TryGetValue("neoforge", out var neoForgeVersion) ? neoForgeVersion.ToString() : "";
                    packToml.FabricVersion = versionsTable.TryGetValue("fabric", out var fabricVersion) ? fabricVersion.ToString() : "";
                    packToml.QuiltVersion = versionsTable.TryGetValue("quilt", out var quiltVersion) ? quiltVersion.ToString() : "";
                    packToml.MinecraftVersion = versionsTable.TryGetValue("minecraft", out var minecraftVersion) ? minecraftVersion.ToString() : "";
                }

                return packToml;
            }
            return null;
        }

        public string ToToml()
        {
            var table = new TomlTable
            {
                ["name"] = Name,
                ["version"] = Version,
                ["author"] = Author,
                ["pack-format"] = PackFormat,
            };

            // Añadir sección [index] si es necesario
            var indexTable = new TomlTable
            {
                ["file"] = IndexFile,
                ["hash-format"] = HashFormat,
                ["hash"] = Hash
            };
            table["index"] = indexTable;

            // Añadir sección [versions]
            var versionsTable = new TomlTable();

            if (!string.IsNullOrEmpty(ForgeVersion))
            {
                versionsTable["forge"] = ForgeVersion;
            }

            if (!string.IsNullOrEmpty(NeoForgeVersion))
            {
                versionsTable["neoforge"] = NeoForgeVersion;
            }

            if (!string.IsNullOrEmpty(FabricVersion))
            {
                versionsTable["fabric"] = FabricVersion;
            }

            if (!string.IsNullOrEmpty(QuiltVersion))
            {
                versionsTable["quilt"] = QuiltVersion;
            }

            if (!string.IsNullOrEmpty(MinecraftVersion))
            {
                versionsTable["minecraft"] = MinecraftVersion;
            }

            table["versions"] = versionsTable;
            

            return Toml.FromModel(table);
        }
    }
}
