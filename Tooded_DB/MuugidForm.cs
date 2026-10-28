using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Tooded_DB
{
    // Müügiajalugu (oma idee): kõik tšekid ühes tabelis, topeltklõps avab PDF-i
    public class MuugidForm : Form
    {
        private DataGridView grid;
        private Label lblKokku;

        public MuugidForm()
        {
            Text = "Müügiajalugu";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1200, 560);
            MinimumSize = new Size(800, 400);

            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };
            grid.CellDoubleClick += grid_CellDoubleClick;

            lblKokku = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(10, 0, 0, 0) };
            var btnTagasta = new Button { Text = "Tagasta valitud ost", Dock = DockStyle.Right, Width = 200 };
            btnTagasta.Click += (s, e) => Tagasta();
            var alumine = new Panel { Dock = DockStyle.Bottom, Height = 46 };
            alumine.Controls.Add(lblKokku);
            alumine.Controls.Add(btnTagasta);

            Controls.Add(grid);
            Controls.Add(alumine);
            Teema.Rakenda(this);

            Load += (s, e) =>
            {
                if (AktiivneKasutaja.OnKlient)
                {
                    MessageBox.Show("Sellel vormil pole sul juurdepääsu.");
                    Close();
                    return;
                }
                Lae();
            };
        }

        private void Lae()
        {
            try
            {
                using var db = new ToodedContext();
                var andmed = db.Muugid.AsNoTracking()
                    .Include(m => m.Kasutaja)
                    .Include(m => m.Klient)
                    .OrderByDescending(m => m.Kuupaev)
                    .ToList()
                    .Select(m => new
                    {
                        Nr = m.Id,
                        Kuupäev = m.Kuupaev.ToString("dd.MM.yyyy HH:mm"),
                        Müüja = m.Kasutaja?.Kasutajanimi,
                        Klient = m.Klient?.Nimi,
                        Kokku = m.Kokku,
                        Boonus = m.KasutatudBoonus,
                        Sünnipäevasoodus = m.SynnipaevaSoodus,
                        Makseviis = m.Maksetuup,
                        Makstud = m.Makstud,
                        Arve = m.Arve,
                        Staatus = m.Tagastatud ? "Tagastatud" : "",
                        Tagastatud = m.Tagastatud
                    })
                    .ToList();

                grid.DataSource = andmed;

                // loetavad veerud: laius sisu järgi, päis murtakse mitmele reale, summad kahe komakohaga
                grid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
                grid.ColumnHeadersHeight = 44;
                grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                foreach (DataGridViewColumn c in grid.Columns)
                {
                    c.MinimumWidth = 70;
                    if (c.Name == "Kokku" || c.Name == "Boonus" || c.Name == "Sünnipäevasoodus" || c.Name == "Makstud")
                    {
                        c.DefaultCellStyle.Format = "0.00 €";
                        c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    }
                }
                if (grid.Columns["Tagastatud"] != null) grid.Columns["Tagastatud"].Visible = false;
                if (grid.Columns["Sünnipäevasoodus"] != null) grid.Columns["Sünnipäevasoodus"].HeaderText = "Sünnipäeva soodus";
                if (grid.Columns["Arve"] != null) grid.Columns["Arve"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                lblKokku.Text = $"{Keel.T("Müüke:")} {andmed.Count}   |   {Keel.T("Käive kokku:")} {andmed.Where(a => !a.Tagastatud).Sum(a => a.Makstud).ToString("0.00") + " €"}   |   {Keel.T("(topeltklõps reale avab tšeki PDF-i)")}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Andmebaasi viga: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }

        // Tagastus: kaubad lähevad tagasi lattu, selle ostuga kogutud boonus võetakse maha, kasutatud boonus tagastatakse
        private void Tagasta()
        {
            if (grid.CurrentRow == null) { MessageBox.Show("Vali tabelist ost."); return; }
            int id = (int)grid.CurrentRow.Cells["Nr"].Value;
            if (MessageBox.Show("Kas tagastada valitud ost? Kaubad lähevad tagasi lattu.", "Tagastus", MessageBoxButtons.YesNo) != DialogResult.Yes) return;

            try
            {
                using var db = new ToodedContext();
                using var tx = db.Database.BeginTransaction();
                var m = db.Muugid.Include(x => x.Read).Include(x => x.Klient).FirstOrDefault(x => x.Id == id);
                if (m == null) { MessageBox.Show("Ostu ei leitud."); return; }
                if (m.Tagastatud) { MessageBox.Show("See ost on juba tagastatud."); return; }
                if (m.Read.Count == 0) { MessageBox.Show("Selle vana ostu ridu pole salvestatud, tagastada ei saa."); return; }

                foreach (var r in m.Read)
                {
                    var t = db.Toodetabel.Find(r.ToodeId);
                    if (t != null) t.Kogus += r.Kogus;
                }
                if (m.Klient != null)
                    m.Klient.Boonus = Math.Max(0, m.Klient.Boonus - m.SaadudBoonus + m.KasutatudBoonus);
                m.Tagastatud = true;
                m.TagastatudAeg = DateTime.Now;
                db.SaveChanges();
                tx.Commit();
                Lae();
                MessageBox.Show("Ost on tagastatud.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Andmebaasi viga: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }

        private void grid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            string arve = grid.Rows[e.RowIndex].Cells["Arve"].Value?.ToString();
            if (string.IsNullOrEmpty(arve)) return;

            string tee = Path.Combine(Kaustad.Arved, arve);
            if (!File.Exists(tee))
            {
                MessageBox.Show("PDF-faili ei leitud kaustast Arved.");
                return;
            }
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(tee) { UseShellExecute = true }); }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}