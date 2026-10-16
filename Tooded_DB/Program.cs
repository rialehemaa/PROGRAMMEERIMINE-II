using System;
using System.Collections.Generic;
using System.Text;

namespace Tooded_DB
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // sisselogimine -> peamenüü; "Logi välja" viib tagasi sisselogimisele
            while (true)
            {
                using (var login = new LoginForm())
                {
                    if (login.ShowDialog() != DialogResult.OK) return;
                }

                using (var menu = new FormMain())
                {
                    if (menu.ShowDialog() != DialogResult.Retry) return;
                }
                AktiivneKasutaja.Tuhjenda();
            }
        }
    }
}