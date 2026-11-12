using Microsoft.EntityFrameworkCore;

namespace Tooded_DB
{
    // Kliendi ostuajalugu: ostud, mida see klient on teinud (kliendikaardi järgi), ja tema boonus
    public class OstudForm : Form
    {
        private DataGridView gridOstud, gridRead;
        private Label lblInfo;
        private Klient _klient;

        public OstudForm()
        {
            Text = "Ostuajalugu";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(900, 640);
            MinimumSize = new Size(640, 460);

            lblInfo = new Label { Dock = DockStyle.Top, Height = 44, Padding = new Padding(12, 12, 0, 0), Font = new Font("Segoe UI", 11, FontStyle.Bold) };

            gridOstud = UusGrid();
            gridOstud.CellDoubleClick += (s, e) => AvaPdf(e.RowIndex);
            gridOstud.SelectionChanged += (s, e) => LaeRead();
            gridRead = UusGrid();

            var tabel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            tabel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tabel.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
            tabel.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            tabel.RowStyles.Add(new RowStyle(SizeType.Percent, 42));
            tabel.Controls.Add(gridOstud, 0, 0);
            tabel.Controls.Add(new Label { Text = "Ostu kaubad (topeltklõps ostule avab tšeki PDF-i)", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
            tabel.Controls.Add(gridRead, 0, 2);

            Controls.Add(tabel);
            Controls.Add(lblInfo);
            Teema.Rakenda(this);

            Load += (s, e) =>
            {
                _klient = KlientSeos.Leia(this);
                if (_klient == null) { Close(); return; }
                Lae();
            };
        }

        private static DataGridView UusGrid() => new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            RowHeadersVisible = false,
            MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White
        };

        private void Lae()
        {
            try
            {
                using var db = new ToodedContext();
                var k = db.Kliendid.AsNoTracking().FirstOrDefault(x => x.Id == _klient.Id);
                lblInfo.Text = $"{k?.Nimi}   |   kaart {k?.Kliendikaart}   |   boonus: {k?.Boonus:0.00} €";

                var ostud = db.Muugid.AsNoTracking().Where(m => m.KlientId == _klient.Id)
                    .OrderByDescending(m => m.Kuupaev).ToList();
                gridOstud.DataSource = ostud.Select(m => new
                {
                    Nr = m.Id,
                    Kuupäev = m.Kuupaev.ToString("dd.MM.yyyy HH:mm"),
                    Kokku = m.Kokku.ToString("0.00") + " €",
                    Boonus = m.KasutatudBoonus.ToString("0.00") + " €",
                    Makseviis = m.Maksetuup,
                    Makstud = m.Makstud.ToString("0.00") + " €",
                    Staatus = m.Tagastatud ? "Tagastatud" : "",
                    Arve = m.Arve
                }).ToList();
                if (gridOstud.Columns["Arve"] != null) gridOstud.Columns["Arve"].Visible = false;
                LaeRead();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Andmebaasi viga: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }

        private void LaeRead()
        {
            if (gridOstud.CurrentRow == null || gridOstud.Columns["Nr"] == null) { gridRead.DataSource = null; return; }
            try
            {
                int id = (int)gridOstud.CurrentRow.Cells["Nr"].Value;
                using var db = new ToodedContext();
                gridRead.DataSource = db.MuugiRead.AsNoTracking().Where(r => r.MuukId == id)
                    .ToList()
                    .Select(r => new { Toode = r.Toode, Kogus = r.Kogus.ToString("0.###"), Summa = r.Summa.ToString("0.00") + " €" })
                    .ToList();
            }
            catch { }
        }

        private void AvaPdf(int rida)
        {
            if (rida < 0) return;
            string arve = gridOstud.Rows[rida].Cells["Arve"].Value?.ToString();
            if (string.IsNullOrEmpty(arve)) return;
            string tee = Path.Combine(Kaustad.Arved, arve);
            if (!File.Exists(tee)) { MessageBox.Show("PDF-faili ei leitud."); return; }
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(tee) { UseShellExecute = true }); }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}
