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
            ClientSize = new Size(340, 270);

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

            Controls.AddRange(new Control[] { pealkiri, l1, txtNimi, l2, txtParool, lblViga, btnLogin, btnRegistreeri });
            AcceptButton = btnLogin;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            lblViga.ForeColor = Color.Firebrick;
            lblViga.Text = "";
            string nimi = txtNimi.Text.Trim();
            string parool = txtParool.Text;

            if (nimi == "" || parool == "")
            {
                lblViga.Text = "Sisesta kasutajanimi ja parool!";
                return;
            }

            try
            {
                using var db = new ToodedContext();
                var kasutaja = db.Kasutajad.FirstOrDefault(k => k.Kasutajanimi == nimi);

                if (kasutaja == null)
                {
                    lblViga.Text = "Sellist kasutajat pole!";
                    return;
                }
                if (!PasswordHelper.Verify(parool, kasutaja.ParoolHash))
                {
                    lblViga.Text = "Vale parool!";
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
                lblViga.Text = "Kasutaja loodud. Logi sisse.";
            }
        }
    }
}