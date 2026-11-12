using System;
using System.Collections.Generic;
using System.Text;

namespace Tooded_DB
{
    // Üks sooritatud müük (tšekk)
    public class Muuk
    {
        public int Id { get; set; }
        public DateTime Kuupaev { get; set; }
        public int? KasutajaId { get; set; }
        public Kasutaja Kasutaja { get; set; }
        public int? KlientId { get; set; }
        public Klient Klient { get; set; }
        public decimal Kokku { get; set; }            // ostukorvi summa
        public decimal KasutatudBoonus { get; set; }  // kui palju boonust kasutati
        public decimal SynnipaevaSoodus { get; set; } // sünnipäevasoodus eurodes
        public string Maksetuup { get; set; }         // kuidas maksti (sularahas, kaardiga, arvega...)
        public decimal Makstud { get; set; }          // tegelikult makstud summa
        public string Arve { get; set; }              // PDF-faili nimi kaustas Arved
        public decimal SaadudBoonus { get; set; }     // selle ostuga kogutud boonus (tagastamisel võetakse tagasi)
        public bool Tagastatud { get; set; }          // true = ost on tagastatud
        public DateTime? TagastatudAeg { get; set; }
        public List<MuugiRida> Read { get; set; } = new List<MuugiRida>();
    }

    // Müügi rida (mida osteti), vajalik tagastamiseks
    public class MuugiRida
    {
        public int Id { get; set; }
        public int MuukId { get; set; }
        public Muuk Muuk { get; set; }
        public int ToodeId { get; set; }
        public string Toode { get; set; }
        public decimal Kogus { get; set; }
        public decimal Summa { get; set; }   // rea summa pärast soodusi
    }
}
