using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Tooded_DB
{
    // "Pood" vaade: iga kategooria on eraldi TabPage, tooted kuvatakse piltidena
    public class Form3 : Form
    {
        private readonly TabControl tabPood;

        public Form3()
        {
            Text = "Pood";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(930, 630);

            tabPood = new TabControl { Dock = DockStyle.Fill };
            Controls.Add(tabPood);

            Load += (s, e) => LaeKategooriadJaPildid();
            FormClosed += (s, e) => VabastaPildid();
        }

        // Laeb pildi kaustast Images ilma faili lukustamata
        private static Image LaePilt(string failinimi)
        {
            if (string.IsNullOrEmpty(failinimi)) return null;
            string tee = Path.Combine(Kaustad.Pildid, failinimi);
            if (!File.Exists(tee)) return null;
            try
            {
                using var fs = new FileStream(tee, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var tmp = Image.FromStream(fs);
                return new Bitmap(tmp);
            }
            catch { return null; }
        }

        private void VabastaPildid()
        {
            foreach (TabPage page in tabPood.TabPages)
                foreach (Control flow in page.Controls)
                    foreach (Control p in flow.Controls)
                        foreach (Control c in p.Controls)
                            if (c is PictureBox pb) pb.Image?.Dispose();
        }

        private void LaeKategooriadJaPildid()
        {
            try
            {
                using var db = new ToodedContext();
                var kategooriad = db.Kategooriatabel.AsNoTracking().OrderBy(k => k.Kategooria_nimetus).ToList();
                var tooted = db.Toodetabel.AsNoTracking().OrderBy(t => t.Toodenimetus).ToList();

                tabPood.TabPages.Clear();

                foreach (var kat in kategooriad)
                {
                    var page = new TabPage(kat.Kategooria_nimetus);
                    var flow = new FlowLayoutPanel
                    {
                        Dock = DockStyle.Fill,
                        AutoScroll = true,
                        Padding = new Padding(8)
                    };
                    page.Controls.Add(flow);

                    foreach (var t in tooted.Where(x => x.KategooriaId == kat.Id))
                        flow.Controls.Add(LooKaart(t));

                    tabPood.TabPages.Add(page);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Andmebaasi viga: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }

        private Panel LooKaart(Toode t)
        {
            var p = new Panel
            {
                Size = new Size(160, 200),
                Margin = new Padding(6),
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Hand
            };

            var pb = new PictureBox
            {
                Location = new Point(8, 5),
                Size = new Size(142, 140),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = LaePilt(t.Pilt)
            };

            var nimi = new Label
            {
                Text = t.Toodenimetus,
                Location = new Point(4, 150),
                Size = new Size(150, 22),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(Font.FontFamily, 9.5f, FontStyle.Bold),
                AutoEllipsis = true
            };

            var hind = new Label
            {
                Text = Math.Round((decimal)t.Hind, 2).ToString("0.00") + " €",
                Location = new Point(4, 172),
                Size = new Size(150, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            p.Controls.AddRange(new Control[] { pb, nimi, hind });

            // klõps kaardile näitab toote infot
            EventHandler info = (s, e) =>
                MessageBox.Show($"Toode: {t.Toodenimetus}\nHind: {Math.Round((decimal)t.Hind, 2):0.00} €\nLaos: {t.Kogus} tk",
                    "Toote info");
            p.Click += info;
            foreach (Control c in p.Controls) c.Click += info;

            return p;
        }
    }
}