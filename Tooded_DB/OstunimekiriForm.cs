using System.ComponentModel;
using Microsoft.EntityFrameworkCore;

namespace Tooded_DB
{
    public class NimekiriRida
    {
        [Browsable(false)] public int ToodeId { get; set; }
        public string Toode { get; set; }
        public int Kogus { get; set; }
        public decimal Hind { get; set; }
        public decimal Summa => Hind * Kogus;
    }

    // Kliendi ostunimekiri (shopping list): mida on vaja osta. Salvestatakse kasutaja kausta, nii et see jääb alles.
    public class OstunimekiriForm : Form
    {
        private DataGridView gridTooted, gridNimekiri;
        private NumericUpDown numKogus;
        private Label lblKokku;
        private readonly BindingList<NimekiriRida> _nimekiri = new BindingList<NimekiriRida>();
        private readonly string _fail = Path.Combine(Kaustad.Juur, "Nimekiri_" + AktiivneKasutaja.Nimi + ".txt");

        public OstunimekiriForm()
        {
            Text = "Ostunimekiri";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1000, 560);
            MinimumSize = new Size(800, 450);

            var tabel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(8) };
            tabel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tabel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tabel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // vasak: tooted
            var vasak = new Panel { Dock = DockStyle.Fill };
            gridTooted = LooGrid();
            gridTooted.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) Lisa(); };
            var vasakAlt = new Panel { Dock = DockStyle.Bottom, Height = 50 };
            var lKogus = new Label { Text = "Kogus:", Location = new Point(5, 15), AutoSize = true };
            numKogus = new NumericUpDown { Location = new Point(65, 11), Width = 60, Minimum = 1, Maximum = 99, Value = 1 };
            var btnLisa = new Button { Text = "Lisa nimekirja →", Location = new Point(140, 8), Size = new Size(180, 34) };
            btnLisa.Click += (s, e) => Lisa();
            vasakAlt.Controls.AddRange(new Control[] { lKogus, numKogus, btnLisa });
            var lVasak = new Label { Text = "Tooted", Dock = DockStyle.Top, Height = 30, Font = new Font("Segoe UI", 12, FontStyle.Bold) };
            vasak.Controls.Add(gridTooted);
            vasak.Controls.Add(vasakAlt);
            vasak.Controls.Add(lVasak);

            // parem: nimekiri
            var parem = new Panel { Dock = DockStyle.Fill };
            gridNimekiri = LooGrid();
            gridNimekiri.DataSource = _nimekiri;
            var paremAlt = new Panel { Dock = DockStyle.Bottom, Height = 90 };
            var btnPlus = new Button { Text = "+", Location = new Point(5, 8), Size = new Size(50, 34) };
            btnPlus.Click += (s, e) => Muuda(1);
            var btnMiinus = new Button { Text = "−", Location = new Point(60, 8), Size = new Size(50, 34) };
            btnMiinus.Click += (s, e) => Muuda(-1);
            var btnEemalda = new Button { Text = "Eemalda", Location = new Point(115, 8), Size = new Size(100, 34) };
            btnEemalda.Click += (s, e) => { var r = Valitud(); if (r != null) { _nimekiri.Remove(r); Salvesta(); Uuenda(); } };
            var btnTuhjenda = new Button { Text = "Tühjenda", Location = new Point(220, 8), Size = new Size(100, 34) };
            btnTuhjenda.Click += (s, e) => { _nimekiri.Clear(); Salvesta(); Uuenda(); };
            lblKokku = new Label { Location = new Point(5, 54), Size = new Size(400, 30), Font = new Font("Segoe UI", 13, FontStyle.Bold) };
            paremAlt.Controls.AddRange(new Control[] { btnPlus, btnMiinus, btnEemalda, btnTuhjenda, lblKokku });
            var lParem = new Label { Text = "Minu ostunimekiri", Dock = DockStyle.Top, Height = 30, Font = new Font("Segoe UI", 12, FontStyle.Bold) };
            parem.Controls.Add(gridNimekiri);
            parem.Controls.Add(paremAlt);
            parem.Controls.Add(lParem);

            tabel.Controls.Add(vasak, 0, 0);
            tabel.Controls.Add(parem, 1, 0);
            Controls.Add(tabel);

            Load += (s, e) => { LaeTooted(); LaeNimekiri(); Uuenda(); };
            Teema.Rakenda(this);
        }

        private DataGridView LooGrid() => new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

        private void LaeTooted()
        {
            try
            {
                using var db = new ToodedContext();
                var tooted = db.Toodetabel.AsNoTracking().Include(t => t.Kategooria).OrderBy(t => t.Toodenimetus).ToList();
                gridTooted.DataSource = tooted.Select(t => new
                {
                    t.Id,
                    Toode = t.Toodenimetus,
                    Kategooria = t.Kategooria?.Kategooria_nimetus,
                    Hind = Allahindlus.HindPraegu(t).ToString("0.00") + " €" + (Allahindlus.SoodusKehtib(t) ? "  (−" + t.Soodus + "%)" : "")
                }).ToList();
                gridTooted.Columns["Id"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Andmebaasi viga: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }

        private void Lisa()
        {
            if (gridTooted.CurrentRow == null) { MessageBox.Show("Vali toode."); return; }
            int id = (int)gridTooted.CurrentRow.Cells["Id"].Value;
            try
            {
                using var db = new ToodedContext();
                var t = db.Toodetabel.Find(id);
                if (t == null) return;
                var rida = _nimekiri.FirstOrDefault(r => r.ToodeId == id);
                if (rida == null)
                    _nimekiri.Add(new NimekiriRida { ToodeId = id, Toode = t.Toodenimetus, Kogus = (int)numKogus.Value, Hind = Allahindlus.HindPraegu(t) });
                else
                    rida.Kogus += (int)numKogus.Value;
                Salvesta();
                Uuenda();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Andmebaasi viga: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }

        private NimekiriRida Valitud() => gridNimekiri.CurrentRow?.DataBoundItem as NimekiriRida;

        private void Muuda(int muutus)
        {
            var r = Valitud();
            if (r == null) return;
            r.Kogus += muutus;
            if (r.Kogus <= 0) _nimekiri.Remove(r);
            Salvesta();
            Uuenda();
        }

        private void Uuenda()
        {
            _nimekiri.ResetBindings();
            lblKokku.Text = "Kokku: " + _nimekiri.Sum(r => r.Summa).ToString("0.00") + " €";
        }

        // fail: iga rida "toodeId;kogus"
        private void Salvesta()
        {
            try
            {
                Directory.CreateDirectory(Kaustad.Juur);
                File.WriteAllLines(_fail, _nimekiri.Select(r => r.ToodeId + ";" + r.Kogus));
            }
            catch (Exception ex) { MessageBox.Show("Nimekirja salvestamine ebaõnnestus: " + ex.Message); }
        }

        private void LaeNimekiri()
        {
            if (!File.Exists(_fail)) return;
            try
            {
                using var db = new ToodedContext();
                foreach (var rida in File.ReadAllLines(_fail))
                {
                    var osad = rida.Split(';');
                    if (osad.Length != 2 || !int.TryParse(osad[0], out int id) || !int.TryParse(osad[1], out int kogus) || kogus <= 0) continue;
                    var t = db.Toodetabel.Find(id);
                    if (t == null) continue;   // toode on vahepeal kustutatud
                    _nimekiri.Add(new NimekiriRida { ToodeId = id, Toode = t.Toodenimetus, Kogus = kogus, Hind = Allahindlus.HindPraegu(t) });
                }
            }
            catch (Exception ex) { MessageBox.Show("Nimekirja laadimine ebaõnnestus: " + ex.Message); }
        }
    }
}
