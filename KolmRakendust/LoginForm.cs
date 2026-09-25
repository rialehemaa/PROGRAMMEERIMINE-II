using System;
using System.Drawing;
using System.Windows.Forms;
namespace KolmRakendust
{
    public class LoginForm : Form
    {
        TextBox txtKasutajanimi, txtParool;
        Button btnLogiSisse, btnRegistreeru;
        Label lblViga;

        public LoginForm()
        {
            this.Text = "Sisselogimine";
            this.Width = 340;
            this.Height = 280;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            Teema.StiliseeriVorm(this);

            Label lblPealkiri = new Label();
            lblPealkiri.Text = "Kolm rakendust";
            lblPealkiri.Font = new Font("Arial", 16, FontStyle.Bold);
            lblPealkiri.ForeColor = Teema.TekstiVarv;
            lblPealkiri.AutoSize = true;
            lblPealkiri.Location = new Point(20, 15);

            Label lblKasutajanimi = new Label();
            lblKasutajanimi.Text = "Kasutajanimi:";
            lblKasutajanimi.ForeColor = Teema.TekstiVarv;
            lblKasutajanimi.AutoSize = true;
            lblKasutajanimi.Location = new Point(20, 60);

            txtKasutajanimi = new TextBox();
            txtKasutajanimi.Location = new Point(130, 57);
            txtKasutajanimi.Width = 170;

            Label lblParool = new Label();
            lblParool.Text = "Parool:";
            lblParool.ForeColor = Teema.TekstiVarv;
            lblParool.AutoSize = true;
            lblParool.Location = new Point(20, 95);

            txtParool = new TextBox();
            txtParool.Location = new Point(130, 92);
            txtParool.Width = 170;
            txtParool.PasswordChar = '*';

            lblViga = new Label();
            lblViga.ForeColor = Teema.PunaneVarv;
            lblViga.AutoSize = true;
            lblViga.MaximumSize = new Size(290, 0);
            lblViga.Location = new Point(20, 125);
            lblViga.Text = "";

            btnLogiSisse = new Button();
            btnLogiSisse.Text = "Logi sisse";
            btnLogiSisse.Size = new Size(130, 35);
            btnLogiSisse.Location = new Point(35, 185);
            btnLogiSisse.Click += BtnLogiSisse_Click;
            Teema.StiliseeriNupp(btnLogiSisse);

            btnRegistreeru = new Button();
            btnRegistreeru.Text = "Registreeru";
            btnRegistreeru.Size = new Size(130, 35);
            btnRegistreeru.Location = new Point(175, 185);
            btnRegistreeru.Click += BtnRegistreeru_Click;
            Teema.StiliseeriNupp(btnRegistreeru);

            this.Controls.Add(lblPealkiri);
            this.Controls.Add(lblKasutajanimi);
            this.Controls.Add(txtKasutajanimi);
            this.Controls.Add(lblParool);
            this.Controls.Add(txtParool);
            this.Controls.Add(lblViga);
            this.Controls.Add(btnLogiSisse);
            this.Controls.Add(btnRegistreeru);

            this.AcceptButton = btnLogiSisse;
        }

        private void BtnLogiSisse_Click(object sender, EventArgs e)//поля не должни бить пустие
        {
            if (string.IsNullOrWhiteSpace(txtKasutajanimi.Text) || string.IsNullOrWhiteSpace(txtParool.Text))
            {
                lblViga.Text = "Sisesta kasutajanimi ja parool.";
                return;
            }

            string kasutajanimi = txtKasutajanimi.Text.Trim();
            string parool = txtParool.Text;

            if (!Kasutajad.KasutajaOnOlemas(kasutajanimi)) //проверка есть ли пользователь
            {
                lblViga.Text = "Sellist kasutajat pole veel. Vajuta \"Registreeru\".";
                return;
            }

            if (!Kasutajad.ParoolOnOige(kasutajanimi, parool))// правильнийли пароль
            {
                lblViga.Text = "Vale parool.";
                return;
            }

            AvaPeamenu(kasutajanimi);
        }

        //регистрация: если имя занято — ошибка,
        //иначе Kasutajad.LisaKasutaja() и сразу вход
        private void BtnRegistreeru_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtKasutajanimi.Text) || string.IsNullOrWhiteSpace(txtParool.Text))
            {
                lblViga.Text = "Sisesta kasutajanimi ja parool.";
                return;
            }

            string kasutajanimi = txtKasutajanimi.Text.Trim();
            string parool = txtParool.Text;

            if (Kasutajad.KasutajaOnOlemas(kasutajanimi))
            {
                lblViga.Text = "Selline kasutajanimi on juba olemas.";
                return;
            }

            Kasutajad.LisaKasutaja(kasutajanimi, parool);
            AvaPeamenu(kasutajanimi);
        }

        //создаёт new MainForm(this) передавая себя, вызывает f.Show(),
        //затем this.Hide() (не Close()).
        private void AvaPeamenu(string kasutajanimi)
        {
            Mangija.Kasutajanimi = kasutajanimi;

            MainForm f = new MainForm(this);
            f.Show();
            this.Hide();
        }
    }
}