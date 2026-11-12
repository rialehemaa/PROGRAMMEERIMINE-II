using System;
using System.Collections.Generic;
using System.Text;

namespace Tooded_DB
{
    // Poe klient kliendikaardiga
    public class Klient
    {
        public int Id { get; set; }
        public string Nimi { get; set; }
        public string Email { get; set; }
        public string Telefon { get; set; }
        public string Kliendikaart { get; set; }   // kaardi number, nt K123456
        public DateTime? Synnipaev { get; set; }   // sünnipäev (−10% 3 päeva enne ja pärast)
        public decimal Boonus { get; set; }        // kogutud boonus eurodes
    }
}
