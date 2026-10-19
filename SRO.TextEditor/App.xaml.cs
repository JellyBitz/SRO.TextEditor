using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows;

namespace SRO.TextEditor
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var asm = Assembly.GetEntryAssembly().GetCustomAttribute<NeutralResourcesLanguageAttribute>();
            // Set default thread culture to English or the one being set
            string languageCode = asm?.CultureName ?? "en";
            CultureInfo culture = new CultureInfo(languageCode);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            // Force HandyControl to use the localized resource dictionary
            HandyControl.Tools.ConfigHelper.Instance.SetLang(languageCode);
            HandyControl.Properties.Langs.Lang.Culture = culture;
        }
    }
}
