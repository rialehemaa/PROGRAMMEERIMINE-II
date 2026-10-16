using System.Globalization;

namespace Tooded_DB
{
    public static class Sisend
    {
        // Tagastab null, kui kõik on korras, muidu veateate
        public static string KontrolliKogus(string tekst, out int kogus)
        {
            kogus = 0;
            if (string.IsNullOrWhiteSpace(tekst)) return "Sisesta kogus!";
            if (!int.TryParse(tekst.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out kogus))
                return "Kogus peab olema täisarv!";
            if (kogus < 0) return "Kogus ei tohi olla negatiivne!";
            return null;
        }

        public static string KontrolliHind(string tekst, out float hind)
        {
            hind = 0;
            if (string.IsNullOrWhiteSpace(tekst)) return "Sisesta hind!";
            string t = tekst.Trim().Replace(',', '.');
            if (!float.TryParse(t, NumberStyles.Float, CultureInfo.InvariantCulture, out hind))
                return "Hind peab olema arv!";
            if (hind < 0) return "Hind ei tohi olla negatiivne!";
            return null;
        }
    }
}