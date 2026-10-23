using System;

namespace Tooded_DB
{
    // Allahindluste arvutused: toote soodus ja kliendi sünnipäevasoodus
    public static class Allahindlus
    {
        public const decimal SynnipaevaMaar = 0.10m;   // 10% sünnipäevasoodus
        public const int SynnipaevaPaevi = 3;          // 3 päeva enne ja 3 päeva pärast

        public static bool SoodusKehtib(Toode t) =>
            t.Soodus > 0 && (t.SoodusKuni == null || t.SoodusKuni.Value.Date >= DateTime.Today);

        // praegune müügihind (soodusega, kui soodus kehtib)
        public static decimal HindPraegu(Toode t)
        {
            decimal hind = Math.Round((decimal)t.Hind, 2);
            if (SoodusKehtib(t)) hind = Math.Round(hind * (100 - t.Soodus) / 100m, 2);
            return hind;
        }

        public static bool OnSynnipaevaAeg(Klient k)
        {
            if (k?.Synnipaev == null) return false;
            var sp = k.Synnipaev.Value;
            var tana = DateTime.Today;
            for (int a = -1; a <= 1; a++)   // kontrollime ka aastavahetust
            {
                int aasta = tana.Year + a;
                int paev = Math.Min(sp.Day, DateTime.DaysInMonth(aasta, sp.Month));
                var kuup = new DateTime(aasta, sp.Month, paev);
                if (Math.Abs((kuup - tana).TotalDays) <= SynnipaevaPaevi) return true;
            }
            return false;
        }

        public static decimal SynnipaevaSoodus(Klient k, decimal kokku) =>
            OnSynnipaevaAeg(k) ? Math.Round(kokku * SynnipaevaMaar, 2) : 0;
    }
}
