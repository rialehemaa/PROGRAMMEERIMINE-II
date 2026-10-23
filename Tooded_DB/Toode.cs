using System;
using System.Collections.Generic;
using System.Text;

namespace Tooded_DB
{
    public class Toode
    {
        public int Id { get; set; }
        public string Toodenimetus { get; set; }
        public int Kogus { get; set; }
        public float Hind { get; set; }
        public string Pilt { get; set; }
        public int Soodus { get; set; }              // allahindlus protsentides (0 = soodust pole)
        public DateTime? SoodusKuni { get; set; }    // soodus kehtib selle kuupäevani (kaasa arvatud)

        public int KategooriaId { get; set; }
        public Kategooria Kategooria { get; set; }
    }
}
