using System;
using System.Collections.Generic;
using System.Text;

namespace Tooded_DB
{
    // Süsteemi kasutaja (müüja või omanik)
    public class Kasutaja
    {
        public int Id { get; set; }
        public string Kasutajanimi { get; set; }
        public string ParoolHash { get; set; }   // parool on salvestatud räsina, mitte tekstina
        public string Roll { get; set; }         // "Müüja" või "Omanik"
        public int? KlientId { get; set; }       // kliendi login seotakse tema kliendikaardiga (ostuajalugu, boonus)
    }
}
