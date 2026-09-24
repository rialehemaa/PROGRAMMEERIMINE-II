using System;
using System.IO;
using System.Windows.Forms;

namespace KolmRakendust
{
    // Ühine punktiarvestuse ja tulemuste ajaloo klass,
    // mida kasutavad nii matemaatiline mäng kui ka sarnaste piltide mäng
    public static class Mangija
    {
        public static string Kasutajanimi { get; set; } = "";
        public static int KogutudPunktid { get; private set; } = 0;

        private static string FailiTee
        {
            get { return Path.Combine(Application.StartupPath, "tulemused.txt"); }
        }

        public static void SalvestaTulemus(string mang, string kirjeldus, int punktid)
        {
            KogutudPunktid += punktid;

            string kasutaja = Kasutajanimi;
            if (string.IsNullOrEmpty(kasutaja))
            {
                kasutaja = "tundmatu";
            }

            string rida = DateTime.Now.ToString("dd.MM.yyyy HH:mm")
                + " | " + kasutaja
                + " | " + mang
                + " | " + kirjeldus
                + " | " + punktid + " punkti (kokku: " + KogutudPunktid + ")";

            try
            {
                File.AppendAllText(FailiTee, rida + Environment.NewLine);
            }
            catch
            {
                // faili kirjutamine ebaõnnestus - mäng jätkub ka ilma salvestamiseta
            }
        }

        public static string LoeAjalugu()
        {
            if (!File.Exists(FailiTee)) return "Tulemusi pole veel salvestatud.";
            try
            {
                return File.ReadAllText(FailiTee);
            }
            catch
            {
                return "Tulemuste lugemine ebaõnnestus.";
            }
        }
    }
}