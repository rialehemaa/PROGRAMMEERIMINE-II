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

namespace Tooded_DB
{
    // Seob kliendi login (Kasutaja) tema kliendikaardiga; esimesel korral küsitakse kaarti või nime
    public static class KlientSeos
    {
        public static Klient Leia(IWin32Window omanik)
        {
            try
            {
                using var db = new ToodedContext();
                var kasutaja = db.Kasutajad.Find(AktiivneKasutaja.Id);
                if (kasutaja == null) return null;

                if (kasutaja.KlientId != null)
                {
                    var olemas = db.Kliendid.Find(kasutaja.KlientId.Value);
                    if (olemas != null) return olemas;
                }

                string tekst = Kysi(omanik);
                if (string.IsNullOrWhiteSpace(tekst)) return null;
                tekst = tekst.Trim();

                var k = db.Kliendid.FirstOrDefault(x => x.Kliendikaart == tekst);
                if (k == null)
                {
                    var sonad = tekst.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var leitud = db.Kliendid.ToList()
                        .Where(x => sonad.All(w => (x.Nimi ?? "").IndexOf(w, StringComparison.OrdinalIgnoreCase) >= 0)).ToList();
                    if (leitud.Count == 1) k = leitud[0];
                    else
                    {
                        MessageBox.Show(leitud.Count == 0 ? "Klienti ei leitud." : "Leiti mitu klienti, sisesta kliendikaardi number.");
                        return null;
                    }
                }
                kasutaja.KlientId = k.Id;
                db.SaveChanges();
                return k;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Andmebaasi viga: " + (ex.InnerException?.Message ?? ex.Message));
                return null;
            }
        }

        private static string Kysi(IWin32Window omanik)
        {
            using var f = new Form
            {
                Text = "Kliendikaart",
                ClientSize = new Size(380, 150),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };
            var l = new Label { Text = "Sisesta kliendikaardi number või nimi:", Location = new Point(15, 15), AutoSize = true };
            var tb = new TextBox { Location = new Point(15, 45), Width = 350 };
            var ok = new Button { Text = "OK", Location = new Point(195, 95), Size = new Size(80, 34), DialogResult = DialogResult.OK };
            var cancel = new Button { Text = "Tühista", Location = new Point(285, 95), Size = new Size(80, 34), DialogResult = DialogResult.Cancel };
            f.Controls.AddRange(new Control[] { l, tb, ok, cancel });
            f.AcceptButton = ok;
            f.CancelButton = cancel;
            Teema.Rakenda(f);
            return f.ShowDialog(omanik) == DialogResult.OK ? tb.Text : null;
        }
    }
}
