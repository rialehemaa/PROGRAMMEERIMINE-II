using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Tooded_DB
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Kaustad.Loo();

            // andmebaas luuakse automaatselt (sama mis Update-Database)
            try
            {
                using var db = new ToodedContext();
                db.Database.Migrate();
                LooAlgandmed(db);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Andmebaasi viga: " + (ex.InnerException?.Message ?? ex.Message));
                return;
            }

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

        // Algandmed tühja andmebaasi jaoks: kategooriad ja kaks kasutajat
        // Algandmed tühja andmebaasi jaoks: kategooriad, tooted ja kaks kasutajat
        private static void LooAlgandmed(ToodedContext db)
        {
            if (!db.Kategooriatabel.Any())
            {
                db.Kategooriatabel.AddRange(
                    new Kategooria { Kategooria_nimetus = "Piimatooted" },
                    new Kategooria { Kategooria_nimetus = "Liha" },
                    new Kategooria { Kategooria_nimetus = "Joogid" },
                    new Kategooria { Kategooria_nimetus = "Puuviljad" },
                    new Kategooria { Kategooria_nimetus = "Energiajoogid" });
                db.SaveChanges();   // et kategooriatel tekiksid Id-d
            }

            if (!db.Toodetabel.Any())
            {
                int KatId(string nimi) => db.Kategooriatabel.First(k => k.Kategooria_nimetus == nimi).Id;

                db.Toodetabel.AddRange(
                    new Toode { Toodenimetus = "Piim", Kogus = 12, Hind = 1.0f, Pilt = "piim.jpg", KategooriaId = KatId("Piimatooted") },
                    new Toode { Toodenimetus = "Sea liha", Kogus = 2, Hind = 2.0f, Pilt = "Sealiha.jpg", KategooriaId = KatId("Liha") },
                    new Toode { Toodenimetus = "Õun", Kogus = 8, Hind = 1.0f, Pilt = "oun.jpg", KategooriaId = KatId("Puuviljad") },
                    new Toode { Toodenimetus = "Mahl", Kogus = 5, Hind = 2.0f, Pilt = "mahl.jpg", KategooriaId = KatId("Joogid") },
                    new Toode { Toodenimetus = "Coca-Cola", Kogus = 6, Hind = 1.8f, Pilt = "Coca-Cola.jpg", KategooriaId = KatId("Joogid") },
                    new Toode { Toodenimetus = "Monster", Kogus = 4, Hind = 2.5f, Pilt = "monster.jpg", KategooriaId = KatId("Energiajoogid") });
            }

            if (!db.Kasutajad.Any())
            {
                db.Kasutajad.AddRange(
                    new Kasutaja { Kasutajanimi = "omanik", ParoolHash = PasswordHelper.Hash("omanik123"), Roll = Rollid.Omanik },
                    new Kasutaja { Kasutajanimi = "muuja", ParoolHash = PasswordHelper.Hash("muuja123"), Roll = Rollid.Muuja });
            }

            db.SaveChanges();
        }
    }
}