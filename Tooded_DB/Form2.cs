using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;

namespace Tooded_DB
{
    public partial class Form2 : Form
    {
        private readonly ToodedContext _db;
        private string valitudPilt;
        private ComboBox comboUhik;

        public Form2()
        {
            InitializeComponent();
            Teema.Rakenda(this);
            Paigutus();
            _db = new ToodedContext();
            comboKategooria.DropDownStyle = ComboBoxStyle.DropDownList;   // ainult valik, teksti sisestada ei saa
            LaeKategooriad();
            LaeTooted();

            // kategooria nupud ühendatakse koodis (töötab ka siis, kui Click pole disaineris seotud)
            foreach (Control c in Controls)
            {
                if (c is not Button b) continue;
                if (b.Text == "Lisa kategooria")
                {
                    b.Click -= btnLisaKategooria_Click;
                    b.Click += btnLisaKategooria_Click;
                }
                else if (b.Text == "Kustuta kategooria")
                {
                    b.Click -= btnKustutaKategooria_Click;
                    b.Click += btnKustutaKategooria_Click;
                }
            }

            // piltu näidatakse kohe käivitamisel (esimese rea pilt)
            if (dataGridViewTooted.Rows.Count > 0)
                NaitaPilti(dataGridViewTooted.Rows[0].Cells["Pilt"].Value?.ToString());

            Keel.Rakenda(this);   // tõlgib ka hiljem lisatud väljad (vihjetekstid, ühiku valik)
        }

        // Suur tabel, mis venib aknaga kaasa; väljad paremal ja nupud all jäävad serva külge
        private void Paigutus()
        {
            Text = "Ladu (haldus)";
            ClientSize = new Size(1200, 680);
            MinimumSize = new Size(940, 520);
            int W = ClientSize.Width, H = ClientSize.Height;
            int paremX = W - 12 - 230;

            dataGridViewTooted.Location = new Point(12, 12);
            dataGridViewTooted.Size = new Size(paremX - 24, H - 12 - 12 - 52);
            dataGridViewTooted.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewTooted.ScrollBars = ScrollBars.Both;
            dataGridViewTooted.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            comboUhik = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            comboUhik.Items.AddRange(new object[] { "tk", "kg" });
            comboUhik.SelectedIndex = 0;
            Controls.Add(comboUhik);
            txtKogus.PlaceholderText = "Kogus (kg-toote puhul kilogrammides)";
            txtHind.PlaceholderText = "Hind € (tk või kg kohta)";

            var paremale = new Control[] { txtNimetus, txtKogus, txtHind, comboUhik, comboKategooria };
            int y = 12;
            foreach (var c in paremale)
            {
                c.Location = new Point(paremX, y);
                c.Size = new Size(230, c.Height);
                c.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                y += 40;
            }
            picturePilt.Location = new Point(paremX, y + 4);
            picturePilt.Size = new Size(230, 190);
            picturePilt.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            int nuppY = H - 12 - 38;
            btnLisa.Size = new Size(110, 38);
            btnUuenda.Size = new Size(110, 38);
            btnKustuta.Size = new Size(110, 38);
            btnKustuta.Location = new Point(W - 12 - 110, nuppY);
            btnUuenda.Location = new Point(btnKustuta.Left - 8 - 110, nuppY);
            btnLisa.Location = new Point(btnUuenda.Left - 8 - 110, nuppY);
            foreach (var b in new[] { btnLisa, btnUuenda, btnKustuta })
                b.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            button1.Size = new Size(140, 38);
            button3.Size = new Size(160, 38);
            btnValiPilt.Size = new Size(120, 38);
            button1.Location = new Point(12, nuppY);
            button3.Location = new Point(button1.Right + 8, nuppY);
            btnValiPilt.Location = new Point(button3.Right + 8, nuppY);
            foreach (var b in new[] { button1, button3, btnValiPilt })
                b.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        }

        // ---------- Abimeetodid ----------

        // Laeb pildi ilma faili lukustamata
        private Image LaePilt(string tee)
        {
            using var fs = new FileStream(tee, FileMode.Open, FileAccess.Read);
            using var tmp = Image.FromStream(fs);
            return new Bitmap(tmp);
        }

        // Kopeerib pildi Images kausta (kui see pole juba seal)
        private void KopeeriPilt(string allikas, string failinimi)
        {
            Directory.CreateDirectory(Kaustad.Pildid);
            string siht = Path.GetFullPath(Path.Combine(Kaustad.Pildid, failinimi));
            if (!string.Equals(Path.GetFullPath(allikas), siht, StringComparison.OrdinalIgnoreCase))
                File.Copy(allikas, siht, true);
        }

        // Näitab pildi PictureBoxis (failinimi kaustast Images)
        private void NaitaPilti(string failinimi)
        {
            var vana = picturePilt.Image;
            picturePilt.Image = null;
            vana?.Dispose();

            if (string.IsNullOrEmpty(failinimi)) return;
            string tee = Path.Combine(Kaustad.Pildid, failinimi);
            if (!File.Exists(tee)) return;

            picturePilt.Image = LaePilt(tee);
        }

        // Väike sisestusaken (asendab Microsoft.VisualBasic InputBoxi)
        private string Kysi(string pealkiri, string kysimus)
        {
            using var f = new Form
            {
                Text = pealkiri,
                ClientSize = new Size(360, 130),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };
            var l = new Label { Text = kysimus, Location = new Point(15, 15), AutoSize = true };
            var tb = new TextBox { Location = new Point(15, 42), Width = 330 };
            var ok = new Button { Text = "OK", Location = new Point(180, 85), DialogResult = DialogResult.OK };
            var cancel = new Button { Text = "Tühista", Location = new Point(265, 85), DialogResult = DialogResult.Cancel };
            f.Controls.AddRange(new Control[] { l, tb, ok, cancel });
            f.AcceptButton = ok;
            f.CancelButton = cancel;

            return f.ShowDialog(this) == DialogResult.OK ? tb.Text : null;
        }

        // ---------- Andmete laadimine ----------

        private void LaeKategooriad()
        {
            comboKategooria.DataSource = _db.Kategooriatabel.ToList();
            comboKategooria.DisplayMember = "Kategooria_nimetus";
            comboKategooria.ValueMember = "Id";
        }

        private void LaeTooted()
        {
            dataGridViewTooted.DataSource = _db.Toodetabel
                .Include(t => t.Kategooria)
                .Select(t => new
                {
                    t.Id,
                    t.Toodenimetus,
                    t.Kogus,
                    t.Uhik,
                    t.Hind,
                    Kategooria = t.Kategooria.Kategooria_nimetus,
                    t.Pilt
                })
                .ToList();
        }

        // ---------- Pilt ----------

        private void btnValiPilt_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog();
            ofd.Filter = "Pildid|*.jpg;*.png;*.jpeg";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                valitudPilt = ofd.FileName;

                var vana = picturePilt.Image;
                picturePilt.Image = LaePilt(valitudPilt);
                vana?.Dispose();
            }
        }

        // ---------- Lisa / Uuenda / Kustuta ----------

        private void btnLisa_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNimetus.Text))
            {
                MessageBox.Show("Sisesta toote nimetus!");
                return;
            }

            string viga = Sisend.KontrolliKogus(txtKogus.Text, out decimal kogus);
            if (viga != null) { MessageBox.Show(viga); return; }

            viga = Sisend.KontrolliHind(txtHind.Text, out float hind);
            if (viga != null) { MessageBox.Show(viga); return; }

            if (comboKategooria.SelectedValue == null)
            {
                MessageBox.Show("Vali kategooria!");
                return;
            }

            var uus = new Toode
            {
                Toodenimetus = txtNimetus.Text.Trim(),
                Kogus = kogus,
                Uhik = comboUhik.SelectedItem?.ToString() ?? "tk",
                Hind = hind,
                KategooriaId = (int)comboKategooria.SelectedValue,
                Pilt = string.IsNullOrEmpty(valitudPilt) ? null : Path.GetFileName(valitudPilt)
            };

            if (!string.IsNullOrEmpty(valitudPilt))
                KopeeriPilt(valitudPilt, uus.Pilt);

            _db.Toodetabel.Add(uus);
            _db.SaveChanges();
            LaeTooted();
            valitudPilt = null;
        }

        private void dataGridViewTooted_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var rida = dataGridViewTooted.Rows[e.RowIndex];
            txtNimetus.Text = rida.Cells["Toodenimetus"].Value?.ToString();
            txtKogus.Text = rida.Cells["Kogus"].Value?.ToString();
            txtHind.Text = rida.Cells["Hind"].Value?.ToString();
            comboUhik.SelectedItem = rida.Cells["Uhik"].Value?.ToString() == "kg" ? "kg" : "tk";

            // vali toote kategooria ripploendis
            string kat = rida.Cells["Kategooria"].Value?.ToString();
            int i = comboKategooria.FindStringExact(kat);
            if (i >= 0) comboKategooria.SelectedIndex = i;

            valitudPilt = null;   // et "Uuenda" ei võtaks pilti eelmisest valikust
            NaitaPilti(rida.Cells["Pilt"].Value?.ToString());
        }

        private void btnUuenda_Click(object sender, EventArgs e)
        {
            if (dataGridViewTooted.CurrentRow == null)
            {
                MessageBox.Show("Vali tabelist toode.");
                return;
            }

            string viga = Sisend.KontrolliKogus(txtKogus.Text, out decimal kogus);
            if (viga != null) { MessageBox.Show(viga); return; }

            viga = Sisend.KontrolliHind(txtHind.Text, out float hind);
            if (viga != null) { MessageBox.Show(viga); return; }

            if (comboKategooria.SelectedValue == null)
            {
                MessageBox.Show("Vali kategooria!");
                return;
            }

            int id = (int)dataGridViewTooted.CurrentRow.Cells["Id"].Value;
            var toode = _db.Toodetabel.Find(id);
            if (toode == null) { MessageBox.Show("Toodet ei leitud."); return; }

            toode.Toodenimetus = txtNimetus.Text.Trim();
            toode.Kogus = kogus;
            toode.Uhik = comboUhik.SelectedItem?.ToString() ?? "tk";
            toode.Hind = hind;
            toode.KategooriaId = (int)comboKategooria.SelectedValue;

            if (valitudPilt != null)
            {
                toode.Pilt = Path.GetFileName(valitudPilt);
                KopeeriPilt(valitudPilt, toode.Pilt);
            }

            _db.SaveChanges();
            LaeTooted();
            valitudPilt = null;
        }

        private void btnKustuta_Click(object sender, EventArgs e)
        {
            if (dataGridViewTooted.CurrentRow == null)
            {
                MessageBox.Show("Vali tabelist toode.");
                return;
            }

            int id = (int)dataGridViewTooted.CurrentRow.Cells["Id"].Value;

            var toode = _db.Toodetabel.Find(id);

            _db.Toodetabel.Remove(toode);
            _db.SaveChanges();
            LaeTooted();
        }

        // ---------- Kategooriad ----------

        private void btnLisaKategooria_Click(object sender, EventArgs e)
        {
            string nimi = Kysi("Lisa kategooria", "Sisesta uue kategooria nimi:");
            if (string.IsNullOrWhiteSpace(nimi)) return;

            if (_db.Kategooriatabel.Any(k => k.Kategooria_nimetus == nimi.Trim()))
            {
                MessageBox.Show("Selline kategooria on juba olemas!");
                return;
            }

            var uus = new Kategooria { Kategooria_nimetus = nimi.Trim(), Kirjeldus = "" };
            _db.Kategooriatabel.Add(uus);
            _db.SaveChanges();

            LaeKategooriad();
            comboKategooria.SelectedValue = uus.Id;   // vali kohe uus kategooria
        }

        private void btnKustutaKategooria_Click(object sender, EventArgs e)
        {
            if (comboKategooria.SelectedValue == null) return;
            int id = (int)comboKategooria.SelectedValue;

            if (_db.Toodetabel.Any(t => t.KategooriaId == id))
            {
                MessageBox.Show("Selles kategoorias on tooteid. Kustuta või liiguta tooted enne ära.");
                return;
            }

            if (MessageBox.Show("Kas kustutada valitud kategooria?", "Kinnitus", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            var kat = _db.Kategooriatabel.Find(id);
            _db.Kategooriatabel.Remove(kat);
            _db.SaveChanges();
            LaeKategooriad();
        }

        private void dataGridViewTooted_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }
    }
}