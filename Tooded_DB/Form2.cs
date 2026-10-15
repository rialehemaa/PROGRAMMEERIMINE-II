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

        public Form2()
        {
            InitializeComponent();
            _db = new ToodedContext();
            LaeKategooriad();
            LaeTooted();
        }

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
            Directory.CreateDirectory("Images");
            string siht = Path.GetFullPath(Path.Combine("Images", failinimi));
            if (!string.Equals(Path.GetFullPath(allikas), siht, StringComparison.OrdinalIgnoreCase))
                File.Copy(allikas, siht, true);
        }

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
                    t.Hind,
                    Kategooria = t.Kategooria.Kategooria_nimetus,
                    t.Pilt
                })
                .ToList();
        }

        private void btnValiPilt_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog();
            ofd.Filter = "Pildid|*.jpg;*.png;*.jpeg";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                valitudPilt = ofd.FileName;
                picturePilt.Image = LaePilt(valitudPilt);
            }
        }

        private void btnLisa_Click(object sender, EventArgs e)
        {
            var uus = new Toode
            {
                Toodenimetus = txtNimetus.Text,
                Kogus = int.Parse(txtKogus.Text),
                Hind = float.Parse(txtHind.Text),
                KategooriaId = (int)comboKategooria.SelectedValue,
                Pilt = string.IsNullOrEmpty(valitudPilt) ? null : Path.GetFileName(valitudPilt)
            };

            if (!string.IsNullOrEmpty(valitudPilt))
                KopeeriPilt(valitudPilt, uus.Pilt);

            _db.Toodetabel.Add(uus);
            _db.SaveChanges();
            LaeTooted();
        }

        private void dataGridViewTooted_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            txtNimetus.Text = dataGridViewTooted.Rows[e.RowIndex].Cells["Toodenimetus"].Value.ToString();
            txtKogus.Text = dataGridViewTooted.Rows[e.RowIndex].Cells["Kogus"].Value.ToString();
            txtHind.Text = dataGridViewTooted.Rows[e.RowIndex].Cells["Hind"].Value.ToString();

            string pilt = dataGridViewTooted.Rows[e.RowIndex].Cells["Pilt"].Value?.ToString();

            if (!string.IsNullOrEmpty(pilt) && File.Exists(Path.Combine("Images", pilt)))
                picturePilt.Image = LaePilt(Path.Combine("Images", pilt));
            else
                picturePilt.Image = null;
        }

        private void btnUuenda_Click(object sender, EventArgs e)
        {
            int id = (int)dataGridViewTooted.SelectedRows[0].Cells["Id"].Value;

            var toode = _db.Toodetabel.Find(id);

            toode.Toodenimetus = txtNimetus.Text;
            toode.Kogus = int.Parse(txtKogus.Text);
            toode.Hind = float.Parse(txtHind.Text);
            toode.KategooriaId = (int)comboKategooria.SelectedValue;

            if (valitudPilt != null)
            {
                toode.Pilt = Path.GetFileName(valitudPilt);
                KopeeriPilt(valitudPilt, toode.Pilt);
            }

            _db.SaveChanges();
            LaeTooted();
        }

        private void btnKustuta_Click(object sender, EventArgs e)
        {
            int id = (int)dataGridViewTooted.SelectedRows[0].Cells["Id"].Value;

            var toode = _db.Toodetabel.Find(id);

            _db.Toodetabel.Remove(toode);
            _db.SaveChanges();
            LaeTooted();
        }

        private void dataGridViewTooted_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}