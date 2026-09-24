using System;
using System.IO;
using System.Windows.Forms;

namespace KolmRakendust
{
    // Lihtne kasutajate register - hoiab kasutajanimesid ja paroole
    // tekstifailis (kasutajanimi;parool ühel real).
    // NB! Paroolid on siin lihtsuse mõttes tavalise tekstina -
    // päris rakenduses ei tohiks paroole kunagi niimoodi salvestada.
    public static class Kasutajad
    {
        private static string FailiTee
        {
            get { return Path.Combine(Application.StartupPath, "kasutajad.txt"); }
        }

        public static bool KasutajaOnOlemas(string kasutajanimi)
        {
            if (!File.Exists(FailiTee)) return false;

            foreach (string rida in File.ReadAllLines(FailiTee))
            {
                string[] osad = rida.Split(';');
                if (osad.Length >= 1 && osad[0] == kasutajanimi)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool ParoolOnOige(string kasutajanimi, string parool)
        {
            if (!File.Exists(FailiTee)) return false;

            foreach (string rida in File.ReadAllLines(FailiTee))
            {
                string[] osad = rida.Split(';');
                if (osad.Length >= 2 && osad[0] == kasutajanimi && osad[1] == parool)
                {
                    return true;
                }
            }
            return false;
        }

        public static void LisaKasutaja(string kasutajanimi, string parool)
        {
            try
            {
                File.AppendAllText(FailiTee, kasutajanimi + ";" + parool + Environment.NewLine);
            }
            catch
            {
                // faili kirjutamine ebaõnnestus - kasutajat ei lisatud
            }
        }
    }
}