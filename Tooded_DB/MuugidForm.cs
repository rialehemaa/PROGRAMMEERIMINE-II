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
            ClientSize = new Size(960, 520);

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

            lblKokku = new Label { Dock = DockStyle.Bottom, Height = 32, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(10, 0, 0, 0) };

            Controls.Add(grid);
            Controls.Add(lblKokku);
            Teema.Rakenda(this);

            Load += (s, e) =>
            {
                if (!AktiivneKasutaja.OnOmanik)
                {
                    MessageBox.Show("Sellel vormil on juurdepääs ainult omanikul.");
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
                        Arve = m.Arve
                    })
                    .ToList();

                grid.DataSource = andmed;
                lblKokku.Text = $"Müüke: {andmed.Count}   |   Käive kokku: {andmed.Sum(a => a.Makstud).ToString("0.00") + " €"}   |   (topeltklõps reale avab tšeki PDF-i)";
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