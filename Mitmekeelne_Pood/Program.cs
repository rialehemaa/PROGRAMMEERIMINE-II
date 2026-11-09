using System.Globalization;
using System.Threading;

namespace Pood_andmebaasiga;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        string lang = Properties.Settings.Default.UserLanguage;

        if (string.IsNullOrWhiteSpace(lang))
            lang = "et-EE"; // vaikimisi

        Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);
        Thread.CurrentThread.CurrentCulture = new CultureInfo(lang);

        ApplicationConfiguration.Initialize();
        Application.Run(new Pood());
    }
}
