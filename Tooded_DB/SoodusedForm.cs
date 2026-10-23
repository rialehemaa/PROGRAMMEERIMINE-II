using Microsoft.EntityFrameworkCore;

namespace Tooded_DB
{
    // muuda = true: Müüja/Omanik seab soodusi; muuda = false: Klient näeb ainult nädala pakkumisi
    public class SoodusedForm : Form
    {
        private readonly bool _muuda;
        private DataGridView grid;
        private Label lblValitud;
        private NumericUpDown numSoodus;
        private DateTimePicker dtKuni;
        private int? _valitudId;

        public SoodusedForm(bool muuda)
        {
            _muuda = muuda;
            Text = muuda ? "Soodused (haldus)" : "Nädala pakkumised";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(820, muuda ? 560 : 480);

            var pealkiri = new Label
            {
                Text = muuda ? "Toodete soodused" : "Nädala pakkumised",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 45,
                Padding = new Padding(12, 10, 0, 0)
            };

            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            grid.CellClick += grid_CellClick;

            Controls.Add(grid);
            Controls.Add(pealkiri);

            if (muuda)
            {
                var paneel = new Panel { Dock = DockStyle.Bottom, Height = 90 };
                lblValitud = new Label { Text = "Vali tabelist toode", Location = new Point(12, 10), AutoSize = true };
                var l1 = new Label { Text = "Soodus (%)", Location = new Point(12, 42), AutoSize = true };
                numSoodus = new NumericUpDown { Location = new Point(100, 38), Width = 70, Minimum = 1, Maximum = 90, Value = 30 };
                var l2 = new Label { Text = "Kehtib kuni", Location = new Point(190, 42), AutoSize = true };
                dtKuni = new DateTimePicker { Location = new Point(280, 38), Width = 130, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(7) };
                var btnSea = new Button { Text = "Sea soodus", Location = new Point(430, 34), Size = new Size(130, 34) };
                btnSea.Click += btnSea_Click;
                var btnEemalda = new Button { Text = "Kustuta soodus", Location = new Point(570, 34), Size = new Size(140, 34) };
                btnEemalda.Click += btnEemalda_Click;
                paneel.Controls.AddRange(new Control[] { lblValitud, l1, numSoodus, l2, dtKuni, btnSea, btnEemalda });
                Controls.Add(paneel);
            }

            Load += (s, e) =>
            {
                if (_muuda && AktiivneKasutaja.OnKlient)
                {
                    MessageBox.Show("Soodusi saab seada ainult müüja või omanik.");
                    Close();
                    return;
                }
                Lae();
            };
            Teema.Rakenda(this);
        }

        private void Lae()
        {
            try
            {
                using var db = new ToodedContext();
                var tooted = db.Toodetabel.AsNoTracking().Include(t => t.Kategooria)
                    .OrderBy(t => t.Toodenimetus).ToList();

                if (!_muuda) tooted = tooted.Where(Allahindlus.SoodusKehtib).ToList();

                grid.DataSource = tooted.Select(t => new
                {
                    t.Id,
                    Toode = t.Toodenimetus,
                    Kategooria = t.Kategooria?.Kategooria_nimetus,
                    Tavahind = Math.Round((decimal)t.Hind, 2).ToString("0.00") + " €",
                    Soodus = Allahindlus.SoodusKehtib(t) ? "−" + t.Soodus + " %" : "",
                    Uushind = Allahindlus.SoodusKehtib(t) ? Allahindlus.HindPraegu(t).ToString("0.00") + " €" : "",
                    Kehtib_kuni = Allahindlus.SoodusKehtib(t) && t.SoodusKuni != null ? t.SoodusKuni.Value.ToString("dd.MM.yyyy") : ""
                }).ToList();
                if (grid.Columns["Id"] != null) grid.Columns["Id"].Visible = false;

                if (!_muuda && tooted.Count == 0)
                    MessageBox.Show("Sel nädalal pakkumisi pole.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Andmebaasi viga: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }

        private void grid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (!_muuda || e.RowIndex < 0) return;
            _valitudId = (int)grid.Rows[e.RowIndex].Cells["Id"].Value;
            lblValitud.Text = "Valitud: " + grid.Rows[e.RowIndex].Cells["Toode"].Value;
        }

        private void btnSea_Click(object sender, EventArgs e)
        {
            if (_valitudId == null) { MessageBox.Show("Vali tabelist toode."); return; }
            if (dtKuni.Value.Date < DateTime.Today) { MessageBox.Show("Kuupäev ei tohi olla minevikus."); return; }

            try
            {
                using var db = new ToodedContext();
                var t = db.Toodetabel.Find(_valitudId.Value);
                if (t == null) { MessageBox.Show("Toodet ei leitud."); return; }
                t.Soodus = (int)numSoodus.Value;
                t.SoodusKuni = dtKuni.Value.Date;
                db.SaveChanges();
                Lae();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Andmebaasi viga: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }

        private void btnEemalda_Click(object sender, EventArgs e)
        {
            if (_valitudId == null) { MessageBox.Show("Vali tabelist toode."); return; }

            try
            {
                using var db = new ToodedContext();
                var t = db.Toodetabel.Find(_valitudId.Value);
                if (t == null) { MessageBox.Show("Toodet ei leitud."); return; }
                t.Soodus = 0;
                t.SoodusKuni = null;
                db.SaveChanges();
                Lae();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Andmebaasi viga: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }
    }
}
