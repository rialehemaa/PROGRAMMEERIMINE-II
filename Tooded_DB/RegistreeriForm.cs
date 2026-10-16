using System;
using System.Collections.Generic;
using System.Text;

namespace Tooded_DB
{
    public class RegistreeriForm : Form
    {
        private TextBox txtNimi;
        private TextBox txtParool;
        private TextBox txtParool2;
        private ComboBox comboRoll;
        private Label lblViga;

        public string Kasutajanimi { get; private set; }

        public RegistreeriForm()
        {
            Text = "Registreerimine";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(340, 380);

            var pealkiri = new Label
            {
                Text = "Uus kasutaja",
                Font = new Font(Font.FontFamily, 14, FontStyle.Bold),
                Location = new Point(20, 15),
                AutoSize = true
            };
            var l1 = new Label { Text = "Kasutajanimi", Location = new Point(20, 55), AutoSize = true };
            txtNimi = new TextBox { Location = new Point(20, 75), Width = 300 };
            var l2 = new Label { Text = "Parool (vähemalt 4 märki)", Location = new Point(20, 110), AutoSize = true };
            txtParool = new TextBox { Location = new Point(20, 130), Width = 300, PasswordChar = '*' };
            var l3 = new Label { Text = "Korda parooli", Location = new Point(20, 165), AutoSize = true };
            txtParool2 = new TextBox { Location = new Point(20, 185), Width = 300, PasswordChar = '*' };
            var l4 = new Label { Text = "Roll", Location = new Point(20, 220), AutoSize = true };
            comboRoll = new ComboBox { Location = new Point(20, 240), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            comboRoll.Items.AddRange(new object[] { Rollid.Muuja, Rollid.Omanik });
            comboRoll.SelectedIndex = 0;

            lblViga = new Label { Location = new Point(20, 275), Size = new Size(300, 40), ForeColor = Color.Firebrick };

            var btnOk = new Button { Text = "Registreeri", Location = new Point(20, 325), Size = new Size(140, 34) };
            btnOk.Click += btnOk_Click;
            var btnTuhista = new Button { Text = "Tühista", Location = new Point(180, 325), Size = new Size(140, 34), DialogResult = DialogResult.Cancel };

            Controls.AddRange(new Control[] { pealkiri, l1, txtNimi, l2, txtParool, l3, txtParool2, l4, comboRoll, lblViga, btnOk, btnTuhista });
            AcceptButton = btnOk;
            CancelButton = btnTuhista;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            string nimi = txtNimi.Text.Trim();

            if (nimi.Length < 3) { lblViga.Text = "Kasutajanimi peab olema vähemalt 3 märki!"; return; }
            if (txtParool.Text.Length < 4) { lblViga.Text = "Parool peab olema vähemalt 4 märki!"; return; }
            if (txtParool.Text != txtParool2.Text) { lblViga.Text = "Paroolid ei ole ühesugused!"; return; }

            try
            {
                using var db = new ToodedContext();
                if (db.Kasutajad.Any(k => k.Kasutajanimi == nimi))
                {
                    lblViga.Text = "Selline kasutajanimi on juba olemas!";
                    return;
                }

                db.Kasutajad.Add(new Kasutaja
                {
                    Kasutajanimi = nimi,
                    ParoolHash = PasswordHelper.Hash(txtParool.Text),
                    Roll = comboRoll.SelectedItem.ToString()
                });
                db.SaveChanges();

                Kasutajanimi = nimi;
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                lblViga.Text = "Andmebaasi viga: " + (ex.InnerException?.Message ?? ex.Message);
            }
        }
    }
}