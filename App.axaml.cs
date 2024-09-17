using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Material.Colors;
using Material.Styles.Themes;
using Material.Styles.Themes.Base;
using PackwizModpackManager.ViewModels;
using PackwizModpackManager.Views;
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
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
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
    }
}