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

            // 20 lisatoodet (4 igasse kategooriasse); lisatakse nime järgi, kui veel puudub
            {
                int KatId2(string nimi) => db.Kategooriatabel.First(k => k.Kategooria_nimetus == nimi).Id;
                var lisa = new (string nimi, int kogus, float hind, string pilt, string kat)[]
                {
                    ("Jogurt", 20, 0.99f, "jogurt.jpg", "Piimatooted"),
                    ("Edami juust", 12, 3.49f, "juust.jpg", "Piimatooted"),
                    ("Või", 15, 2.79f, "voi.jpg", "Piimatooted"),
                    ("Kohupiim", 14, 1.39f, "kohupiim.jpg", "Piimatooted"),
                    ("Kanafilee", 10, 6.99f, "kanafilee.jpg", "Liha"),
                    ("Hakkliha", 9, 4.99f, "hakkliha.jpg", "Liha"),
                    ("Vorstid", 16, 3.29f, "vorstid.jpg", "Liha"),
                    ("Seakaelakarbonaad", 8, 5.49f, "seakael.jpg", "Liha"),
                    ("Banaan", 30, 1.29f, "banaan.jpg", "Puuviljad"),
                    ("Apelsin", 25, 1.59f, "apelsin.jpg", "Puuviljad"),
                    ("Viinamarjad", 12, 2.99f, "viinamari.jpg", "Puuviljad"),
                    ("Pirn", 18, 1.89f, "pirn.jpg", "Puuviljad"),
                    ("Mineraalvesi", 40, 0.69f, "mineraalvesi.jpg", "Joogid"),
                    ("Ice Tea", 22, 1.49f, "icetea.jpg", "Joogid"),
                    ("Sprite", 24, 1.79f, "sprite.jpg", "Joogid"),
                    ("Fanta", 24, 1.79f, "fanta.jpg", "Joogid"),
                    ("Red Bull", 20, 1.99f, "redbull.jpg", "Energiajoogid"),
                    ("Burn", 18, 1.69f, "burn.jpg", "Energiajoogid"),
                    ("Hell", 18, 1.39f, "hell.jpg", "Energiajoogid"),
                    ("Nocco", 15, 2.29f, "nocco.jpg", "Energiajoogid"),
                };
                foreach (var x in lisa)
                {
                    if (!db.Toodetabel.Any(t => t.Toodenimetus == x.nimi))
                        db.Toodetabel.Add(new Toode { Toodenimetus = x.nimi, Kogus = x.kogus, Hind = x.hind, Pilt = x.pilt, KategooriaId = KatId2(x.kat) });
                }
            }

            // kasutaja, kes näeb ainult nädala pakkumisi
            if (!db.Kasutajad.Any(k => k.Kasutajanimi == "klient"))
                db.Kasutajad.Add(new Kasutaja { Kasutajanimi = "klient", ParoolHash = PasswordHelper.Hash("klient123"), Roll = Rollid.Klient });

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