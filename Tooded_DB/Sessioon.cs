using System;
using System.Collections.Generic;
using System.Text;

namespace Tooded_DB
{
    public static class Rollid
    {
        public const string Muuja = "Müüja";
        public const string Omanik = "Omanik";
        public const string Klient = "Klient";   // näeb ainult nädala pakkumisi
    }

    // Hetkel sisse logitud kasutaja
    public static class AktiivneKasutaja
    {
        public static int Id { get; set; }
        public static string Nimi { get; set; }
        public static string Roll { get; set; }

        public static bool OnOmanik => Roll == Rollid.Omanik;
        public static bool OnKlient => Roll == Rollid.Klient;

        public static void Tuhjenda()
        {
            Id = 0;
            Nimi = null;
            Roll = null;
        }
    }

    public static class PasswordHelper
    {
        public static string Hash(string parool) => BCrypt.Net.BCrypt.HashPassword(parool);

        public static bool Verify(string parool, string hash)
        {
            try { return BCrypt.Net.BCrypt.Verify(parool, hash); }
            catch { return false; }
        }
    }
}