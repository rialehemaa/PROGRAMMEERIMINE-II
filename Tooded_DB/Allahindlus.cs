using System;
using System.Collections.Generic;
using System.Linq;

namespace Tooded_DB
{
    // Tulemus sünnipäevasoodustusest
    public class SynnipaevaTulemus
    {
        public decimal Uldine { get; set; }   // soodus kogu ostukorvile
        public decimal Tort { get; set; }     // lisasoodus tortidele
        public string Tekst { get; set; } = "";
        public decimal Kokku => Uldine + Tort;
    }

    // Allahindluste arvutused: toote soodus, komplektid ja kliendi sünnipäevasoodus
    public static class Allahindlus
    {
        public const string TortideKategooria = "Kondiitritooted";

        public const decimal SynnipaevaPaevaMaar = 0.15m;   // sünnipäeval -15% kogu ostukorvile
        public const decimal SynnipaevaTortMaar = 0.05m;    // + -5% tortidele
        public const decimal SynnipaevaNadalaMaar = 0.10m;  // 3 päeva enne ja pärast -10%
        public const int SynnipaevaPaevi = 3;

        // ---- toote protsentsoodus (alates ... kuni) ----
        public static bool SoodusKehtib(Toode t) =>
            t.Soodus > 0
            && (t.SoodusAlates == null || t.SoodusAlates.Value.Date <= DateTime.Today)
            && (t.SoodusKuni == null || t.SoodusKuni.Value.Date >= DateTime.Today);

        public static decimal HindPraegu(Toode t)
        {
            decimal hind = Math.Round((decimal)t.Hind, 2);
            if (SoodusKehtib(t)) hind = Math.Round(hind * (100 - t.Soodus) / 100m, 2);
            return hind;
        }

        // ---- ostukorvi read: parim soodus (protsent VÕI komplekt, need ei liitu) ----
        public static void Arvuta(IList<OstukorviRida> read, IEnumerable<Pakkumine> pakkumised)
        {
            var aktiivsed = pakkumised.Where(p => p.Kehtib).ToList();

            foreach (var r in read)
            {
                int tk = (int)Math.Floor(r.Kogus);
                decimal protsentSoodus = Math.Round((r.Hind - r.HindSoodus) * r.Kogus, 2);
                r.SpTort = 0;
                decimal parimKomplekt = 0;
                string komplektTekst = "";

                foreach (var p in aktiivsed)
                {
                    decimal d = 0;
                    string tekst = "";

                    if (p.Tuup == PakkumiseTuubid.OstaSaaTasuta && p.ToodeId == r.ToodeId && p.Kogus > 0 && p.Tasuta > 0)
                    {
                        int grupp = p.Kogus + p.Tasuta;
                        int tasuta = (tk / grupp) * p.Tasuta;
                        d = tasuta * r.Hind;
                        tekst = $"{p.Kogus}+{p.Tasuta}: {tasuta} {Keel.T("tasuta")}";
                    }
                    else if (p.Tuup == PakkumiseTuubid.KomplektHind && p.ToodeId == r.ToodeId && p.Kogus > 0)
                    {
                        int komplekte = tk / p.Kogus;
                        d = komplekte * (p.Kogus * r.Hind - p.KomplektHind);
                        tekst = $"{p.Kogus} {Keel.T("tk")} {p.KomplektHind:0.00} €";
                    }
                    else if (p.Tuup == PakkumiseTuubid.TeineProtsent && p.Toode2Id == r.ToodeId)
                    {
                        int a = (int)Math.Floor(read.Where(x => x.ToodeId == p.ToodeId).Sum(x => x.Kogus));
                        int paare = Math.Min(a, tk);
                        d = paare * r.Hind * p.Protsent / 100m;
                        tekst = $"{Keel.T(p.Toode?.Toodenimetus)} + {Keel.T(r.Toode)} −{p.Protsent} %";
                    }

                    d = Math.Round(d, 2);
                    if (d > parimKomplekt) { parimKomplekt = d; komplektTekst = tekst; }
                }

                if (parimKomplekt > protsentSoodus)
                {
                    r.Soodus = parimKomplekt;
                    r.Selgitus = komplektTekst;
                }
                else
                {
                    r.Soodus = protsentSoodus;
                    r.Selgitus = protsentSoodus > 0 ? $"−{Math.Round((1 - r.HindSoodus / r.Hind) * 100):0} %" : "";
                }
            }
        }

        // ---- sünnipäev ----
        private static DateTime SynniKuupaev(DateTime sp, int aasta)
        {
            int paev = Math.Min(sp.Day, DateTime.DaysInMonth(aasta, sp.Month));
            return new DateTime(aasta, sp.Month, paev);
        }

        public static bool OnSynnipaevaPaev(Klient k)
        {
            if (k?.Synnipaev == null) return false;
            return SynniKuupaev(k.Synnipaev.Value, DateTime.Today.Year) == DateTime.Today;
        }

        public static bool OnSynnipaevaAeg(Klient k)
        {
            if (k?.Synnipaev == null) return false;
            var sp = k.Synnipaev.Value;
            var tana = DateTime.Today;
            for (int a = -1; a <= 1; a++)
                if (Math.Abs((SynniKuupaev(sp, tana.Year + a) - tana).TotalDays) <= SynnipaevaPaevi) return true;
            return false;
        }

        public static SynnipaevaTulemus Synnipaev(Klient k, decimal vahesumma, decimal tortSumma)
        {
            var t = new SynnipaevaTulemus();
            if (OnSynnipaevaPaev(k))
            {
                t.Uldine = Math.Round(vahesumma * SynnipaevaPaevaMaar, 2);
                t.Tort = Math.Round(tortSumma * SynnipaevaTortMaar, 2);
                t.Tekst = Keel.T("Sünnipäev") + " −15 %" + (t.Tort > 0 ? " " + Keel.T("ja") + " " + Keel.T("torti") + " −5 %" : "");
            }
            else if (OnSynnipaevaAeg(k))
            {
                t.Uldine = Math.Round(vahesumma * SynnipaevaNadalaMaar, 2);
                t.Tekst = Keel.T("Sünnipäevanädal") + " −10 %";
            }
            return t;
        }
    }
}
