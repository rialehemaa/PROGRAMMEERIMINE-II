using System;
using System.Collections.Generic;
using System.Text;

namespace Tooded_DB
{
    public class LoginForm : Form
    {
        private TextBox txtNimi;
        private TextBox txtParool;
        private Label lblViga;
        private Button btnLogin;
        private Button btnRegistreeri;

        public LoginForm()
        {
            Text = "Sisselogimine";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(340, 280);

            var pealkiri = new Label
            {
                Text = "Pood – sisselogimine",
                Font = new Font(Font.FontFamily, 14, FontStyle.Bold),
                Location = new Point(20, 15),
                AutoSize = true
            };
            var l1 = new Label { Text = "Kasutajanimi", Location = new Point(20, 60), AutoSize = true };
            txtNimi = new TextBox { Location = new Point(20, 80), Width = 300 };
            var l2 = new Label { Text = "Parool", Location = new Point(20, 115), AutoSize = true };
            txtParool = new TextBox { Location = new Point(20, 135), Width = 300, PasswordChar = '*' };

            lblViga = new Label
            {
                Location = new Point(20, 170),
                Size = new Size(300, 40),
                ForeColor = Color.Firebrick
            };

            btnLogin = new Button { Text = "Logi sisse", Location = new Point(20, 215), Size = new Size(140, 34) };
            btnLogin.Click += btnLogin_Click;

            btnRegistreeri = new Button { Text = "Registreeri", Location = new Point(180, 215), Size = new Size(140, 34) };
            btnRegistreeri.Click += btnRegistreeri_Click;

            var keeleValik = Keel.Valik(this);
            keeleValik.Location = new Point(235, 14);
            keeleValik.Width = 85;
            pealkiri.Font = new Font(Font.FontFamily, 11, FontStyle.Bold);

            Controls.AddRange(new Control[] { pealkiri, keeleValik, l1, txtNimi, l2, txtParool, lblViga, btnLogin, btnRegistreeri });
            AcceptButton = btnLogin;
            Teema.Rakenda(this);
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            lblViga.ForeColor = Color.Firebrick;
            lblViga.Text = "";
            string nimi = txtNimi.Text.Trim();
            string parool = txtParool.Text;

            if (nimi == "" || parool == "")
            {
                lblViga.Text = Keel.T("Sisesta kasutajanimi ja parool!");
                return;
            }

            try
            {
                using var db = new ToodedContext();
                var kasutaja = db.Kasutajad.FirstOrDefault(k => k.Kasutajanimi == nimi);

                if (kasutaja == null)
                {
                    lblViga.Text = Keel.T("Sellist kasutajat pole!");
                    return;
                }
                if (!PasswordHelper.Verify(parool, kasutaja.ParoolHash))
                {
                    lblViga.Text = Keel.T("Vale parool!");
                    return;
                }

                AktiivneKasutaja.Id = kasutaja.Id;
                AktiivneKasutaja.Nimi = kasutaja.Kasutajanimi;
                AktiivneKasutaja.Roll = kasutaja.Roll;
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                lblViga.Text = "Andmebaasi viga: " + (ex.InnerException?.Message ?? ex.Message);
            }
        }

        private void btnRegistreeri_Click(object sender, EventArgs e)
        {
            using var f = new RegistreeriForm();
            if (f.ShowDialog(this) == DialogResult.OK)
            {
                txtNimi.Text = f.Kasutajanimi;
                txtParool.Clear();
                txtParool.Focus();
                lblViga.ForeColor = Color.SeaGreen;
                lblViga.Text = Keel.T("Kasutaja loodud. Logi sisse.");
            }
        }
    }
}