using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;

namespace Tooded_DB
{
    // Lihtne tulpdiagramm GDI+ abil (valmis diagrammikomponenti .NET 10-s pole)
    public class Diagramm : Control
    {
        public List<(string Silt, decimal Vaartus)> Andmed = new List<(string, decimal)>();
        public string Pealkiri = "";

        public Diagramm()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
        }

        private static decimal SobivMax(decimal m)
        {
            double p = Math.Pow(10, Math.Floor(Math.Log10((double)m)));
            double n = (double)m / p;
            double r = n <= 1 ? 1 : n <= 2 ? 2 : n <= 5 ? 5 : 10;
            return (decimal)(r * p);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Teema.Kaart);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            using var fTitle = new Font("Segoe UI", 11f, FontStyle.Bold);
            using var fText = new Font("Segoe UI", 9f);
            using var bTitle = new SolidBrush(Teema.Pealkiri);
            using var bText = new SolidBrush(Teema.Tekst);
            g.DrawString(Keel.T(Pealkiri), fTitle, bTitle, 12, 8);

            if (Andmed.Count == 0)
            {
                var sf0 = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(Keel.T("Andmeid pole."), fTitle, bText, new RectangleF(0, 0, Width, Height), sf0);
                return;
            }

            var plot = new Rectangle(64, 44, Width - 64 - 16, Height - 44 - 48);
            if (plot.Width < 60 || plot.Height < 60) return;

            decimal max = Andmed.Max(a => a.Vaartus);
            if (max <= 0) max = 1;
            decimal ylem = SobivMax(max);

            using var penJoon = new Pen(Teema.Joon);
            var sfY = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
            for (int i = 0; i <= 4; i++)
            {
                float y = plot.Bottom - plot.Height * i / 4f;
                g.DrawLine(penJoon, plot.Left, y, plot.Right, y);
                g.DrawString((ylem * i / 4).ToString("0.##"), fText, bText, new RectangleF(0, y - 10, plot.Left - 6, 20), sfY);
            }

            float w = (float)plot.Width / Andmed.Count;
            float bw = Math.Min(w * 0.62f, 80f);
            int samm = (int)Math.Ceiling(Andmed.Count / Math.Max(1f, plot.Width / 64f));
            var sfX = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near, Trimming = StringTrimming.EllipsisCharacter };
            using var bTulp = new SolidBrush(Teema.Pealkiri);

            for (int i = 0; i < Andmed.Count; i++)
            {
                var (silt, v) = Andmed[i];
                float x = plot.Left + w * i + (w - bw) / 2f;
                float h = (float)(v / ylem) * plot.Height;
                if (h > 0) g.FillRectangle(bTulp, x, plot.Bottom - h, bw, h);

                if (w >= 40 && v > 0)
                {
                    var sfV = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far };
                    g.DrawString(v.ToString("0.##"), fText, bText, new RectangleF(x - 20, plot.Bottom - h - 20, bw + 40, 18), sfV);
                }
                if (i % samm == 0)
                    g.DrawString(silt, fText, bText, new RectangleF(plot.Left + w * i - 4, plot.Bottom + 6, Math.Max(w * samm, 60) + 8, 36), sfX);
            }
        }
    }

    // Omaniku statistika: müük päevade kaupa ja enimmüüdud tooted (tagastatud oste ei arvestata)
    public class StatistikaForm : Form
    {
        private ComboBox comboPeriood;
        private Label lblKokkuvote;
        private Diagramm diaPaevad, diaTooted;
        private DataGridView gridPaevad, gridTooted;

        public StatistikaForm()
        {
            Text = "Statistika";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1100, 640);
            MinimumSize = new Size(800, 480);

            var yla = new Panel { Dock = DockStyle.Top, Height = 50 };
            var lPeriood = new Label { Text = "Periood:", Location = new Point(12, 15), AutoSize = true };
            comboPeriood = new ComboBox { Location = new Point(90, 11), Width = 190, DropDownStyle = ComboBoxStyle.DropDownList };
            comboPeriood.Items.AddRange(new object[] { "Viimased 7 päeva", "Viimased 30 päeva", "Viimased 90 päeva", "Kogu aeg" });
            comboPeriood.SelectedIndex = 1;
            lblKokkuvote = new Label { Location = new Point(300, 15), AutoSize = true, Font = new Font("Segoe UI", 10f, FontStyle.Bold) };
            yla.Controls.Add(lPeriood);
            yla.Controls.Add(comboPeriood);
            yla.Controls.Add(lblKokkuvote);

            var tabs = new TabControl { Dock = DockStyle.Fill };
            var pPaevad = new TabPage("Müük päevade kaupa");
            var pTooted = new TabPage("Enimmüüdud tooted");

            diaPaevad = new Diagramm { Dock = DockStyle.Fill, Pealkiri = "Käive päevade kaupa (€)" };
            gridPaevad = LooGrid();
            pPaevad.Controls.Add(diaPaevad);
            pPaevad.Controls.Add(gridPaevad);

            diaTooted = new Diagramm { Dock = DockStyle.Fill, Pealkiri = "Enimmüüdud tooted (tulu €)" };
            gridTooted = LooGrid();
            pTooted.Controls.Add(diaTooted);
            pTooted.Controls.Add(gridTooted);

            tabs.TabPages.Add(pPaevad);
            tabs.TabPages.Add(pTooted);

            Controls.Add(tabs);
            Controls.Add(yla);
            Teema.Rakenda(this);

            comboPeriood.SelectedIndexChanged += (s, e) => Lae();
            Load += (s, e) =>
            {
                if (!AktiivneKasutaja.OnOmanik)
                {
                    MessageBox.Show("Sellel vormil pole sul juurdepääsu.");
                    Close();
                    return;
                }
                Lae();
            };
        }

        private static DataGridView LooGrid()
        {
            return new DataGridView
            {
                Dock = DockStyle.Bottom,
                Height = 190,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
        }

        private void Lae()
        {
            try
            {
                DateTime tana = DateTime.Today;
                int idx = comboPeriood.SelectedIndex;
                DateTime a = idx == 0 ? tana.AddDays(-6) : idx == 1 ? tana.AddDays(-29) : idx == 2 ? tana.AddDays(-89) : new DateTime(2000, 1, 1);

                using var db = new ToodedContext();

                // ---- müük päevade kaupa ----
                var muugid = db.Muugid.AsNoTracking()
                    .Where(m => !m.Tagastatud && m.Kuupaev >= a)
                    .Select(m => new { m.Kuupaev, m.Makstud })
                    .ToList();

                var paevaMap = muugid.GroupBy(m => m.Kuupaev.Date)
                    .ToDictionary(g => g.Key, g => (Muuke: g.Count(), Kaive: g.Sum(x => x.Makstud)));

                DateTime algus = idx == 3 ? (paevaMap.Count > 0 ? paevaMap.Keys.Min() : tana) : a;
                var paevad = new List<(DateTime Paev, int Muuke, decimal Kaive)>();
                if ((tana - algus).TotalDays <= 120)
                {
                    for (var d = algus; d <= tana; d = d.AddDays(1))
                        paevad.Add(paevaMap.TryGetValue(d, out var dz) ? (d, dz.Muuke, dz.Kaive) : (d, 0, 0m));
                }
                else
                {
                    foreach (var kv in paevaMap.OrderBy(k => k.Key)) paevad.Add((kv.Key, kv.Value.Muuke, kv.Value.Kaive));
                }

                diaPaevad.Andmed = paevad.Select(p => (p.Paev.ToString("dd.MM"), p.Kaive)).ToList();
                diaPaevad.Invalidate();

                gridPaevad.DataSource = paevad.OrderByDescending(p => p.Paev)
                    .Select(p => new { Kuupaev = p.Paev, Muuke = p.Muuke, Kaive = p.Kaive }).ToList();
                if (gridPaevad.Columns["Kuupaev"] != null)
                {
                    gridPaevad.Columns["Kuupaev"].HeaderText = "Kuupäev";
                    gridPaevad.Columns["Kuupaev"].DefaultCellStyle.Format = "dd.MM.yyyy";
                }
                if (gridPaevad.Columns["Muuke"] != null) gridPaevad.Columns["Muuke"].HeaderText = "Müüke";
                if (gridPaevad.Columns["Kaive"] != null)
                {
                    gridPaevad.Columns["Kaive"].HeaderText = "Käive (€)";
                    gridPaevad.Columns["Kaive"].DefaultCellStyle.Format = "0.00";
                }

                // ---- kokkuvõte ----
                int kokkuMuuke = muugid.Count;
                decimal kaive = muugid.Sum(m => m.Makstud);
                decimal keskmine = kokkuMuuke > 0 ? Math.Round(kaive / kokkuMuuke, 2) : 0;
                lblKokkuvote.Text = Keel.T("Müüke kokku:") + " " + kokkuMuuke
                    + "     " + Keel.T("Käive:") + " " + kaive.ToString("0.00") + " €"
                    + "     " + Keel.T("Keskmine ost:") + " " + keskmine.ToString("0.00") + " €"
                    + "     (" + Keel.T("Tagastatud oste ei arvestata.") + ")";

                // ---- enimmüüdud tooted ----
                var read = db.MuugiRead.AsNoTracking()
                    .Where(r => !r.Muuk.Tagastatud && r.Muuk.Kuupaev >= a)
                    .Select(r => new { r.Toode, r.Kogus, r.Summa })
                    .ToList();

                var top = read.GroupBy(r => r.Toode)
                    .Select(g => new { Toode = g.Key, Kogus = g.Sum(x => x.Kogus), Tulu = g.Sum(x => x.Summa) })
                    .OrderByDescending(x => x.Tulu)
                    .Take(10)
                    .ToList();

                diaTooted.Andmed = top.Select(t => (Keel.T(t.Toode), t.Tulu)).ToList();
                diaTooted.Invalidate();

                gridTooted.DataSource = top.Select(t => new { t.Toode, t.Kogus, t.Tulu }).ToList();
                if (gridTooted.Columns["Toode"] != null) gridTooted.Columns["Toode"].HeaderText = "Toode";
                if (gridTooted.Columns["Kogus"] != null)
                {
                    gridTooted.Columns["Kogus"].HeaderText = "Kogus";
                    gridTooted.Columns["Kogus"].DefaultCellStyle.Format = "0.###";
                }
                if (gridTooted.Columns["Tulu"] != null)
                {
                    gridTooted.Columns["Tulu"].HeaderText = "Tulu (€)";
                    gridTooted.Columns["Tulu"].DefaultCellStyle.Format = "0.00";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Andmebaasi viga: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }
    }
}
