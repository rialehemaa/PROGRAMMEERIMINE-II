using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Tooded_DB
{
    // Administraatori (omaniku) paneel klientide haldamiseks
    public class KliendidForm : Form
    {
        private DataGridView grid;
        private TextBox txtOtsi, txtNimi, txtEmail, txtTelefon, txtKaart, txtBoonus;
        private DateTimePicker dtSynnipaev;
        private int? _valitudId;

        public KliendidForm()
        {
            Text = "Kliendid – admin paneel";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1000, 540);

            EhitaUi();
            Teema.Rakenda(this);
            Load += (s, e) =>
            {
                if (!AktiivneKasutaja.OnOmanik)
                {
                    MessageBox.Show("Sellel vormil on juurdepääs ainult omanikul.");
                    Close();
                    return;
                }
                LaeKliendid();
            };
        }

        private void EhitaUi()
        {
            var lOtsi = new Label { Text = "Otsi (nimi / kaart):", Location = new Point(12, 15), AutoSize = true };
            txtOtsi = new TextBox { Location = new Point(135, 12), Width = 200 };
            txtOtsi.TextChanged += (s, e) => LaeKliendid();

            grid = new DataGridView
            {
                Location = new Point(12, 45),
                Size = new Size(640, 480),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom
            };
            grid.CellClick += grid_CellClick;

            int x = 675, y = 15;
            Label L(string t) { var l = new Label { Text = t, Location = new Point(x, y), AutoSize = true }; y += 20; return l; }
            TextBox T() { var t = new TextBox { Location = new Point(x, y), Width = 300 }; y += 38; return t; }

            var l1 = L("Nimi"); txtNimi = T();
            var l2 = L("E-post"); txtEmail = T();
            var l3 = L("Telefon"); txtTelefon = T();
            var l4 = L("Kliendikaart (tühi = loo automaatselt)"); txtKaart = T();
            var l6 = L("Sünnipäev (tühi = pole teada)");
            dtSynnipaev = new DateTimePicker { Location = new Point(x, y), Width = 300, Format = DateTimePickerFormat.Short, ShowCheckBox = true, MaxDate = DateTime.Today, Checked = false };
            y += 38;
            var l5 = L("Boonus (€)"); txtBoonus = T();
            txtBoonus.Text = "0";

            var btnLisa = new Button { Text = "Lisa", Location = new Point(x, y), Size = new Size(95, 32) };
            btnLisa.Click += btnLisa_Click;
            var btnUuenda = new Button { Text = "Uuenda", Location = new Point(x + 102, y), Size = new Size(95, 32) };
            btnUuenda.Click += btnUuenda_Click;
            var btnKustuta = new Button { Text = "Kustuta", Location = new Point(x + 204, y), Size = new Size(95, 32) };
            btnKustuta.Click += btnKustuta_Click;
            var btnTuhjenda = new Button { Text = "Tühjenda väljad", Location = new Point(x, y + 40), Size = new Size(299, 30) };
            btnTuhjenda.Click += (s, e) => TuhjendaValjad();

            Controls.AddRange(new Control[]
            {
                lOtsi, txtOtsi, grid, l1, txtNimi, l2, txtEmail, l3, txtTelefon, l4, txtKaart, l6, dtSynnipaev, l5, txtBoonus,
                btnLisa, btnUuenda, btnKustuta, btnTuhjenda
            });
        }

        private void LaeKliendid()
        {
            try
            {
                using var db = new ToodedContext();
                var q = db.Kliendid.AsNoTracking().AsQueryable();
                string otsi = txtOtsi.Text.Trim();
                if (otsi != "") q = q.Where(k => k.Nimi.Contains(otsi) || k.Kliendikaart.Contains(otsi));

                grid.DataSource = q.OrderBy(k => k.Nimi)
                    .Select(k => new { k.Id, k.Nimi, k.Email, k.Telefon, k.Kliendikaart, k.Synnipaev, k.Boonus })
                    .ToList();
                if (grid.Columns["Synnipaev"] != null) grid.Columns["Synnipaev"].DefaultCellStyle.Format = "dd.MM.yyyy";
                if (grid.Columns["Boonus"] != null) grid.Columns["Boonus"].DefaultCellStyle.Format = "0.00";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Andmebaasi viga: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }

        private void grid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var r = grid.Rows[e.RowIndex];
            _valitudId = (int)r.Cells["Id"].Value;
            txtNimi.Text = r.Cells["Nimi"].Value?.ToString();
            txtEmail.Text = r.Cells["Email"].Value?.ToString();
            txtTelefon.Text = r.Cells["Telefon"].Value?.ToString();
            txtKaart.Text = r.Cells["Kliendikaart"].Value?.ToString();
            var sp = r.Cells["Synnipaev"].Value as DateTime?;
            dtSynnipaev.Checked = sp != null;
            if (sp != null) dtSynnipaev.Value = sp.Value;
            txtBoonus.Text = Convert.ToDecimal(r.Cells["Boonus"].Value).ToString("0.00");
        }

        private void TuhjendaValjad()
        {
            _valitudId = null;
            txtNimi.Clear(); txtEmail.Clear(); txtTelefon.Clear(); txtKaart.Clear();
            dtSynnipaev.Checked = false;
            txtBoonus.Text = "0";
        }

        private static string UusKaart(ToodedContext db)
        {
            var rnd = new Random();
            string kaart;
            do { kaart = "K" + rnd.Next(100000, 999999); }
            while (db.Kliendid.Any(k => k.Kliendikaart == kaart));
            return kaart;
        }

        private bool KontrolliValjad(out decimal boonus)
        {
            boonus = 0;
            if (string.IsNullOrWhiteSpace(txtNimi.Text))
            {
                MessageBox.Show("Sisesta kliendi nimi!");
                return false;
            }
            string t = txtBoonus.Text.Trim().Replace(',', '.');
            if (!decimal.TryParse(t, System.Globalization.NumberStyles.Number,
                    System.Globalization.CultureInfo.InvariantCulture, out boonus))
            {
                MessageBox.Show("Boonus peab olema arv!");
                return false;
            }
            if (boonus < 0)
            {
                MessageBox.Show("Boonus ei tohi olla negatiivne!");
                return false;
            }
            boonus = Math.Round(boonus, 2);
            return true;
        }

        private void btnLisa_Click(object sender, EventArgs e)
        {
            if (!KontrolliValjad(out decimal boonus)) return;

            try
            {
                using var db = new ToodedContext();
                string kaart = txtKaart.Text.Trim();
                if (kaart == "") kaart = UusKaart(db);
                else if (db.Kliendid.Any(k => k.Kliendikaart == kaart))
                {
                    MessageBox.Show("Selline kliendikaart on juba olemas!");
                    return;
                }

                db.Kliendid.Add(new Klient
                {
                    Nimi = txtNimi.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Telefon = txtTelefon.Text.Trim(),
                    Kliendikaart = kaart,
                    Synnipaev = dtSynnipaev.Checked ? dtSynnipaev.Value.Date : (DateTime?)null,
                    Boonus = boonus
                });
                db.SaveChanges();
                TuhjendaValjad();
                LaeKliendid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Andmebaasi viga: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }

        private void btnUuenda_Click(object sender, EventArgs e)
        {
            if (_valitudId == null) { MessageBox.Show("Vali tabelist klient."); return; }
            if (!KontrolliValjad(out decimal boonus)) return;

            try
            {
                using var db = new ToodedContext();
                var k = db.Kliendid.Find(_valitudId.Value);
                if (k == null) { MessageBox.Show("Klienti ei leitud."); return; }

                string kaart = txtKaart.Text.Trim();
                if (kaart == "") kaart = k.Kliendikaart;
                if (db.Kliendid.Any(x => x.Kliendikaart == kaart && x.Id != k.Id))
                {
                    MessageBox.Show("Selline kliendikaart on juba olemas!");
                    return;
                }

                k.Nimi = txtNimi.Text.Trim();
                k.Email = txtEmail.Text.Trim();
                k.Telefon = txtTelefon.Text.Trim();
                k.Kliendikaart = kaart;
                k.Synnipaev = dtSynnipaev.Checked ? dtSynnipaev.Value.Date : (DateTime?)null;
                k.Boonus = boonus;
                db.SaveChanges();
                LaeKliendid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Andmebaasi viga: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }

        private void btnKustuta_Click(object sender, EventArgs e)
        {
            if (_valitudId == null) { MessageBox.Show("Vali tabelist klient."); return; }
            if (MessageBox.Show("Kas kustutada valitud klient?", "Kinnitus", MessageBoxButtons.YesNo) != DialogResult.Yes) return;

            try
            {
                using var db = new ToodedContext();
                var k = db.Kliendid.Find(_valitudId.Value);
                if (k != null)
                {
                    db.Kliendid.Remove(k);
                    db.SaveChanges();
                }
                TuhjendaValjad();
                LaeKliendid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Andmebaasi viga: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }
    }
}