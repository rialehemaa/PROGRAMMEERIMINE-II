using System;
using System.Collections.Generic;
using System.Text;
namespace Tooded_DB
{
    // Pildid ja arved hoitakse kasutaja kaustas AppData\Local\ToodedApp
    public static class Kaustad
    {
        public static readonly string Juur = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ToodedApp");
        public static readonly string Pildid = Path.Combine(Juur, "Images");
        public static readonly string Arved = Path.Combine(Juur, "Arved");

        public static void Loo()
        {
            Directory.CreateDirectory(Pildid);
            Directory.CreateDirectory(Arved);

            // rakendusega kaasa tulnud pildid kopeeritakse kasutaja kausta (kui neid seal veel pole
            // või kui fail on teistsugune, nt uuendatud pilt)
            string algsed = Path.Combine(AppContext.BaseDirectory, "Images");
            if (!Directory.Exists(algsed)) return;
            foreach (var fail in Directory.GetFiles(algsed))
            {
                string siht = Path.Combine(Pildid, Path.GetFileName(fail));
                if (!File.Exists(siht) || new FileInfo(siht).Length != new FileInfo(fail).Length)
                    File.Copy(fail, siht, true);
            }
        }
    }
}