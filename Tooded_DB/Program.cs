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

                bool jatka;
                do
                {
                    jatka = false;
                    using (var menu = new FormMain())
                    {
                        var tulemus = menu.ShowDialog();
                        if (tulemus == DialogResult.Yes) jatka = true;   // teema vahetati: ava menu uuesti
                        else if (tulemus != DialogResult.Retry) return;
                    }
                } while (jatka);
                AktiivneKasutaja.Tuhjenda();
            }
        }

        // Algandmed tühja andmebaasi jaoks: kategooriad ja kaks kasutajat
        // Algandmed tühja andmebaasi jaoks: kategooriad, tooted ja kaks kasutajat
        // kui sellist kategooriat veel pole, luuakse see (nii ei kuku programm vanade andmete peal)
        private static int LeiaVoiLooKategooria(ToodedContext db, string nimi)
        {
            var kat = db.Kategooriatabel.FirstOrDefault(k => k.Kategooria_nimetus == nimi);
            if (kat == null)
            {
                kat = new Kategooria { Kategooria_nimetus = nimi };
                db.Kategooriatabel.Add(kat);
                db.SaveChanges();
            }
            return kat.Id;
        }

        // Parandab vanad/vigased andmed (käib igal käivitusel, kahjutu kui parandada pole midagi)
        private static void ParandaAndmed(ToodedContext db)
        {
            // katkise tähega toode ("V\uFFFDi") kustutatakse
            var katki = db.Toodetabel.ToList().Where(t => t.Toodenimetus != null && t.Toodenimetus.Contains("\uFFFD")).ToList();
            if (katki.Count > 0) db.Toodetabel.RemoveRange(katki);

            var edami = db.Toodetabel.FirstOrDefault(t => t.Toodenimetus == "Edami juust");
            if (edami != null && !db.Toodetabel.Any(t => t.Toodenimetus == "Eesti juust")) edami.Toodenimetus = "Eesti juust";

            var oun = db.Toodetabel.FirstOrDefault(t => t.Toodenimetus == "Õun");
            if (oun != null)
            {
                oun.KategooriaId = LeiaVoiLooKategooria(db, "Puuviljad");
                oun.Pilt = "oun.jpg";
            }

            var piim = db.Toodetabel.FirstOrDefault(t => t.Toodenimetus == "Piim");
            if (piim != null) piim.KategooriaId = LeiaVoiLooKategooria(db, "Piimatooted");

            db.SaveChanges();

            // vanad nimed: tort ja joogid saavad mahu (iga maht on eraldi toode)
            foreach (var (vana, uus) in new[] { ("Pagarini tort", "Maasikatort"), ("Coca-Cola", "Coca-Cola 0,5 L"), ("Sprite", "Sprite 0,5 L"), ("Fanta", "Fanta 0,5 L"), ("Mineraalvesi", "Mineraalvesi gaseerimata 0,5 L"), ("Ice Tea", "Ice Tea 0,5 L"), ("Mahl", "Mahl 0,35 L") })
            {
                var t = db.Toodetabel.FirstOrDefault(x => x.Toodenimetus == vana);
                if (t != null && !db.Toodetabel.Any(x => x.Toodenimetus == uus)) t.Toodenimetus = uus;
            }

            // üks kord: kaalutavad tooted saavad ühikuks kg (tüki keskmine kaal võimaldab osta ka tükkidena)
            string lipp = Path.Combine(Kaustad.Juur, "uhikud_ok.flag");
            if (!File.Exists(lipp))
            {
                var kg = new (string nimi, decimal tuki)[]
                {
                    ("Banaan", 0.18m), ("Apelsin", 0.25m), ("\u00d5un", 0.18m), ("Pirn", 0.2m), ("Viinamarjad", 0m),
                    ("Kanafilee", 0m), ("Hakkliha", 0m), ("Seakaelakarbonaad", 0m), ("Sea liha", 0m), ("Vorstid", 0m)
                };
                foreach (var x in kg)
                {
                    var t = db.Toodetabel.FirstOrDefault(y => y.Toodenimetus == x.nimi);
                    if (t != null) { t.Uhik = "kg"; t.TukiKaal = x.tuki; }
                }
                Directory.CreateDirectory(Kaustad.Juur);
                File.WriteAllText(lipp, "ok");
            }

            foreach (var tt in db.Toodetabel.Where(x => x.Uhik == null || x.Uhik == ""))
                tt.Uhik = "tk";

            db.SaveChanges();

            // tühi kategooria "Toidutooted" kustutatakse
            // (ja vana kirjaveaga "Toidutooded", kui seal tooteid pole)
            foreach (var tyhi in new[] { "Toidutooded", "Toidutooted" })
            {
                var toidu = db.Kategooriatabel.FirstOrDefault(k => k.Kategooria_nimetus == tyhi);
                if (toidu != null && !db.Toodetabel.Any(t => t.KategooriaId == toidu.Id))
                {
                    db.Kategooriatabel.Remove(toidu);
                    db.SaveChanges();
                }
            }
        }

        // Joogid päris piltidega ja mahtudega (käib ühe korra: pärast seda saab pilte ja hindu vabalt muuta)
        private static void ParandaJoogid(ToodedContext db)
        {
            string lipp = Path.Combine(Kaustad.Juur, "joogid_v2.flag");
            if (File.Exists(lipp)) return;

            // vanad nimed saavad uue mahu (nii jäävad müügid ja pakkumised alles)
            var uuedHinnad = new HashSet<string>();
            foreach (var (vana, uus, hind) in new[]
            {
                ("Mahl 0,35 L", "Mahl 200 ml", 0.79f),
                ("Sprite 1 L", "Sprite 0,85 L", 2.19f),
                ("Ice Tea 1 L", "Ice Tea 1,5 L", 2.69f),
                ("Mineraalvesi gaseeritud 1 L", "Mineraalvesi gaseeritud 1,5 L", 1.19f),
                ("Mineraalvesi gaseerimata 1 L", "Mineraalvesi gaseerimata 1,5 L", 1.09f)
            })
            {
                var t = db.Toodetabel.FirstOrDefault(x => x.Toodenimetus == vana);
                if (t != null && !db.Toodetabel.Any(x => x.Toodenimetus == uus)) { t.Toodenimetus = uus; t.Hind = hind; }
            }
            db.SaveChanges();

            var joogid = new (string nimi, int kogus, float hind, string pilt)[]
            {
                ("Coca-Cola 0,5 L", 20, 1.80f, "coca_05.jpg"), ("Coca-Cola 1 L", 20, 2.49f, "coca_1.jpg"),
                ("Coca-Cola 1,5 L", 20, 2.99f, "coca_15.jpg"), ("Coca-Cola 2 L", 20, 3.49f, "coca_2.jpg"),
                ("Coca-Cola Zero 0,5 L", 20, 1.80f, "zero_05.jpg"), ("Coca-Cola Zero 1,5 L", 20, 2.99f, "zero_15.jpg"),
                ("Coca-Cola Zero 2 L", 20, 3.49f, "zero_2.jpg"),
                ("Sprite 0,5 L", 20, 1.79f, "sprite_05.jpg"), ("Sprite 0,85 L", 20, 2.19f, "sprite_085.jpg"),
                ("Sprite 1,5 L", 20, 2.79f, "sprite_15.jpg"), ("Sprite 2 L", 20, 3.29f, "sprite_2.jpg"),
                ("Ice Tea 0,5 L", 22, 1.49f, "icetea_05.jpg"), ("Ice Tea 1,5 L", 20, 2.69f, "icetea_15.jpg"),
                ("Mahl 2 L", 20, 3.39f, "mahl_2.jpg"), ("Mahl 200 ml", 30, 0.79f, "mahl_200ml.jpg"),
                ("Mahl 1 L", 20, 2.59f, "mahl_1.jpg"),
                ("Mineraalvesi gaseeritud 1,5 L", 30, 1.19f, "varska_gaas_15.jpg"), ("Mineraalvesi gaseeritud 0,5 L", 40, 0.69f, "varska_gaas_05.jpg"),
                ("Mineraalvesi gaseerimata 0,5 L", 40, 0.69f, "saaremaa_05.jpg"), ("Mineraalvesi gaseerimata 1,5 L", 30, 1.09f, "saaremaa_15.jpg")
            };
            int kat = LeiaVoiLooKategooria(db, "Joogid");
            foreach (var j in joogid)
            {
                var t = db.Toodetabel.FirstOrDefault(x => x.Toodenimetus == j.nimi);
                if (t != null) t.Pilt = j.pilt;
                else db.Toodetabel.Add(new Toode { Toodenimetus = j.nimi, Kogus = j.kogus, Hind = j.hind, Pilt = j.pilt, KategooriaId = kat });
            }
            db.SaveChanges();

            Directory.CreateDirectory(Kaustad.Juur);
            File.WriteAllText(lipp, "ok");
        }

        // Näidiskomplektid (lisatakse ainult siis, kui sellist veel pole)
        private static void LooNaidisPakkumised(ToodedContext db)
        {
            DateTime a = DateTime.Today, k = DateTime.Today.AddDays(14);
            int? Id(string nimi) => db.Toodetabel.Where(t => t.Toodenimetus == nimi).Select(t => (int?)t.Id).FirstOrDefault();

            void Lisa(string tuup, string nimi, int kogus, int tasuta, decimal hind, string nimi2 = null, int protsent = 0)
            {
                int? id = Id(nimi);
                if (id == null) return;
                int? id2 = nimi2 == null ? (int?)null : Id(nimi2);
                if (nimi2 != null && id2 == null) return;
                if (db.Pakkumised.Any(p => p.Tuup == tuup && p.ToodeId == id.Value && p.Toode2Id == id2)) return;
                db.Pakkumised.Add(new Pakkumine
                {
                    Tuup = tuup, ToodeId = id.Value, Kogus = kogus, Tasuta = tasuta, KomplektHind = hind,
                    Toode2Id = id2, Protsent = protsent, Alates = a, Kuni = k
                });
            }

            Lisa(PakkumiseTuubid.KomplektHind, "Piim", 5, 0, 3.00m);
            Lisa(PakkumiseTuubid.KomplektHind, "Mineraalvesi gaseerimata 0,5 L", 6, 0, 3.00m);
            Lisa(PakkumiseTuubid.KomplektHind, "Red Bull", 3, 0, 5.00m);
            Lisa(PakkumiseTuubid.OstaSaaTasuta, "Jogurt", 2, 1, 0);
            Lisa(PakkumiseTuubid.OstaSaaTasuta, "Coca-Cola 0,5 L", 3, 1, 0);
            Lisa(PakkumiseTuubid.TeineProtsent, "Jogurt", 1, 0, 0, "Kohupiim", 50);
            db.SaveChanges();
        }

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
                int KatId(string nimi) => LeiaVoiLooKategooria(db, nimi);

                db.Toodetabel.AddRange(
                    new Toode { Toodenimetus = "Piim", Kogus = 12, Hind = 1.0f, Pilt = "piim.jpg", KategooriaId = KatId("Piimatooted") },
                    new Toode { Toodenimetus = "Sea liha", Kogus = 2, Hind = 2.0f, Pilt = "Sealiha.jpg", KategooriaId = KatId("Liha") },
                    new Toode { Toodenimetus = "Õun", Kogus = 8, Hind = 1.0f, Pilt = "oun.jpg", KategooriaId = KatId("Puuviljad") },
                    new Toode { Toodenimetus = "Mahl 0,35 L", Kogus = 5, Hind = 2.0f, Pilt = "mahl.jpg", KategooriaId = KatId("Joogid") },
                    new Toode { Toodenimetus = "Coca-Cola 0,5 L", Kogus = 6, Hind = 1.8f, Pilt = "Coca-Cola.jpg", KategooriaId = KatId("Joogid") },
                    new Toode { Toodenimetus = "Monster", Kogus = 4, Hind = 2.5f, Pilt = "monster.jpg", KategooriaId = KatId("Energiajoogid") });
            }

            ParandaAndmed(db);

            // 20 lisatoodet (4 igasse kategooriasse); lisatakse nime järgi, kui veel puudub
            {
                int KatId2(string nimi) => LeiaVoiLooKategooria(db, nimi);
                var lisa = new (string nimi, int kogus, float hind, string pilt, string kat)[]
                {
                    ("Jogurt", 20, 0.99f, "jogurt.jpg", "Piimatooted"),
                    ("Eesti juust", 12, 3.49f, "juust.jpg", "Piimatooted"),
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
                    ("Mineraalvesi gaseerimata 0,5 L", 40, 0.69f, "mineraalvesi.jpg", "Joogid"),
                    ("Mineraalvesi gaseerimata 1 L", 30, 0.99f, "mineraalvesi.jpg", "Joogid"),
                    ("Mineraalvesi gaseeritud 0,5 L", 40, 0.69f, "mineraalvesi.jpg", "Joogid"),
                    ("Mineraalvesi gaseeritud 1 L", 30, 0.99f, "mineraalvesi.jpg", "Joogid"),
                    ("Mahl 1 L", 20, 2.59f, "mahl.jpg", "Joogid"),
                    ("Ice Tea 1 L", 20, 2.19f, "icetea.jpg", "Joogid"),
                    ("Ice Tea 0,5 L", 22, 1.49f, "icetea.jpg", "Joogid"),
                    ("Sprite 0,5 L", 24, 1.79f, "sprite.jpg", "Joogid"),
                    ("Fanta 0,5 L", 24, 1.79f, "fanta.jpg", "Joogid"),
                    ("Coca-Cola 1 L", 20, 2.49f, "Coca-Cola.jpg", "Joogid"),
                    ("Coca-Cola 1,5 L", 20, 2.99f, "Coca-Cola.jpg", "Joogid"),
                    ("Coca-Cola 2 L", 20, 3.49f, "Coca-Cola.jpg", "Joogid"),
                    ("Sprite 1 L", 20, 2.29f, "sprite.jpg", "Joogid"),
                    ("Sprite 1,5 L", 20, 2.79f, "sprite.jpg", "Joogid"),
                    ("Sprite 2 L", 20, 3.29f, "sprite.jpg", "Joogid"),
                    ("Fanta 1 L", 20, 2.29f, "fanta.jpg", "Joogid"),
                    ("Fanta 1,5 L", 20, 2.79f, "fanta.jpg", "Joogid"),
                    ("Fanta 2 L", 20, 3.29f, "fanta.jpg", "Joogid"),
                    ("Red Bull", 20, 1.99f, "redbull.jpg", "Energiajoogid"),
                    ("Burn", 18, 1.69f, "burn.jpg", "Energiajoogid"),
                    ("Hell", 18, 1.39f, "hell.jpg", "Energiajoogid"),
                    ("Nocco", 15, 2.29f, "nocco.jpg", "Energiajoogid"),
                    ("Maasikatort", 6, 12.99f, "pagarini_tort.jpg", "Kondiitritooted"),
                    ("Dubai tort", 5, 18.99f, "dubai_tort.jpg", "Kondiitritooted"),
                    ("Šokolaaditort", 6, 14.99f, "sokolaadi_tort.jpg", "Kondiitritooted"),
                    ("Marjatort", 6, 15.49f, "marja_tort.jpg", "Kondiitritooted"),
                    ("Mango juustukook", 5, 16.99f, "mango_juustukook.jpg", "Kondiitritooted"),
                    ("Champagne tort", 4, 17.49f, "champagne_tort.jpg", "Kondiitritooted"),
                    ("Juustukook", 7, 13.99f, "juustukook.jpg", "Kondiitritooted"),
                    ("Sai", 25, 0.99f, "sai.jpg", "Toidutooted"),
                    ("Rukkileib", 20, 1.59f, "rukkileib.jpg", "Toidutooted"),
                    ("Sepik", 20, 1.19f, "sepik.jpg", "Toidutooted"),
                    ("Croissant", 18, 0.89f, "croissant.jpg", "Toidutooted"),
                    ("Kaerasai", 18, 1.09f, "kaerasai.jpg", "Toidutooted"),
                    ("Munad 10 tk", 30, 2.89f, "munad.jpg", "Toidutooted"),
                    ("Herned", 25, 1.29f, "herned.jpg", "Toidutooted"),
                    ("Mais", 25, 1.19f, "mais.jpg", "Toidutooted"),
                    ("Tuunikala", 22, 2.49f, "tuunikala.jpg", "Toidutooted"),
                    ("Ketšup", 20, 1.89f, "ketsup.jpg", "Toidutooted"),
                    ("Makaronid", 30, 1.39f, "makaronid.jpg", "Toidutooted"),
                    ("Riis", 30, 1.99f, "riis.jpg", "Toidutooted"),
                    ("Tatar", 25, 1.79f, "tatar.jpg", "Toidutooted"),
                    ("Jahu", 25, 1.09f, "jahu.jpg", "Toidutooted"),
                    ("Suhkur", 25, 1.29f, "suhkur.jpg", "Toidutooted"),
                    ("Šokolaad", 40, 1.99f, "sokolaad.jpg", "Maiustused ja snäkid"),
                    ("Küpsised", 35, 1.79f, "kupsised.jpg", "Maiustused ja snäkid"),
                    ("Kommid", 35, 2.49f, "kommid.jpg", "Maiustused ja snäkid"),
                    ("Kartulikrõpsud", 30, 2.19f, "kropsud.jpg", "Maiustused ja snäkid"),
                    ("Pähklid", 25, 3.49f, "pahklid.jpg", "Maiustused ja snäkid"),
                };
                foreach (var x in lisa)
                {
                    if (!db.Toodetabel.Any(t => t.Toodenimetus == x.nimi))
                        db.Toodetabel.Add(new Toode { Toodenimetus = x.nimi, Kogus = x.kogus, Hind = x.hind, Pilt = x.pilt, KategooriaId = KatId2(x.kat) });
                }
            }

            db.SaveChanges();
            LooNaidisPakkumised(db);
            ParandaJoogid(db);

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