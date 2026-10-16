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
        public decimal Makstud { get; set; }          // tegelikult makstud summa
        public string Arve { get; set; }              // PDF-faili nimi kaustas Arved
    }
}