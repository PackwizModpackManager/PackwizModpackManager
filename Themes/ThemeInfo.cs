using Material.Colors;
using Material.Styles.Themes.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackwizModpackManager.Themes
{
    public class ThemeInfo
    {
        public string Name { get; set; }
        public BaseThemeMode BaseTheme { get; set; }
        public PrimaryColor PrimaryColor { get; set; }
        public SecondaryColor SecondaryColor { get; set; }
    }
}
