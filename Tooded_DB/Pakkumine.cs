using System;

namespace Tooded_DB
{
    // Komplekt / kampaania: nt "Osta 2, saa 1 tasuta", "3 tk hinnaga 5 €", "Osta A, B -50%"
    public static class PakkumiseTuubid
    {
        public const string OstaSaaTasuta = "Osta N, saa M tasuta";
        public const string KomplektHind = "N tk hinnaga X €";
        public const string TeineProtsent = "Osta toode, teine -% allahindlusega";
    }

    public class Pakkumine
    {
        public int Id { get; set; }
        public string Tuup { get; set; }          // üks PakkumiseTuubid väärtustest
        public int ToodeId { get; set; }
        public Toode Toode { get; set; }
        public int Kogus { get; set; }            // N
        public int Tasuta { get; set; }           // M (tüüp 1)
        public decimal KomplektHind { get; set; } // X (tüüp 2)
        public int? Toode2Id { get; set; }        // teine toode (tüüp 3)
        public Toode Toode2 { get; set; }
        public int Protsent { get; set; }         // teise toote allahindlus (tüüp 3)
        public DateTime Alates { get; set; }
        public DateTime Kuni { get; set; }

        public bool Kehtib => Alates.Date <= DateTime.Today && Kuni.Date >= DateTime.Today;

        public string Kirjeldus()
        {
            string nimi = Keel.T(Toode?.Toodenimetus ?? "toode");
            string uhik = " " + Keel.T(Toode?.Uhik == "kg" ? "kg" : "tk");
            switch (Tuup)
            {
                case PakkumiseTuubid.OstaSaaTasuta:
                    return $"{nimi}: {Keel.T("osta")} {Kogus}, {Keel.T("saa")} {Tasuta} {Keel.T("tasuta")}";
                case PakkumiseTuubid.KomplektHind:
                    return $"{Kogus}{uhik} {nimi} {Keel.T("hinnaga")} {KomplektHind:0.00} €";
                case PakkumiseTuubid.TeineProtsent:
                    return $"{Keel.T("Osta")} {nimi}, {Keel.T(Toode2?.Toodenimetus)} −{Protsent} %";
                default:
                    return Tuup;
            }
        }
    }
}
