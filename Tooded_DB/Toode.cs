using System;
using System.Collections.Generic;
using System.Text;

namespace Tooded_DB
{
    public class Toode
    {
        public int Id { get; set; }
        public string Toodenimetus { get; set; }
        public decimal Kogus { get; set; }             // laoseis (kg-toodetel kilogrammides)
        public string Uhik { get; set; } = "tk";       // müügiühik: "tk" (ka pakk, pudel) või "kg"
        public decimal TukiKaal { get; set; }          // kg-toote ühe tüki keskmine kaal kg (0 = tükkidena ei müüda)
        public float Hind { get; set; }
        public string Pilt { get; set; }
        public DateTime? SoodusAlates { get; set; }   // soodus kehtib alates sellest kuupäevast
        public int Soodus { get; set; }              // allahindlus protsentides (0 = soodust pole)
        public DateTime? SoodusKuni { get; set; }    // soodus kehtib selle kuupäevani (kaasa arvatud)

        public int KategooriaId { get; set; }
        public Kategooria Kategooria { get; set; }
    }
}
