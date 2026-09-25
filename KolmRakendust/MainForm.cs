using System;
using System.Drawing;
using System.Windows.Forms;

namespace KolmRakendust
{
    public class MainForm : Form
    {
        Button btnPilt, btnMath, btnMatch, btnAjalugu, btnLogiValja;
        Label lbl;
        bool valjaLogitud = false;
        LoginForm loginVorm;//для того что б не пересоздавалось окно логина

        public MainForm(LoginForm loginVorm)
        {
            this.loginVorm = loginVorm;

            this.Text = "Kolm rakendust";
            this.Width = 320;
            this.Height = 340;
            Teema.StiliseeriVorm(this);

            lbl = new Label();
            if (string.IsNullOrEmpty(Mangija.Kasutajanimi))
            {
                lbl.Text = "Vali rakendus:";
            }
            else
            {
                lbl.Text = "Tere, " + Mangija.Kasutajanimi + "!\nVali rakendus:";
            }
            lbl.Font = new Font("Arial", 14);
            lbl.ForeColor = Teema.TekstiVarv;
            lbl.AutoSize = true;
            lbl.Location = new Point(20, 20);

            btnPilt = new Button();
            btnPilt.Text = "1. Pildivaataja";
            btnPilt.Size = new Size(220, 35);
            btnPilt.Location = new Point(20, 70);
            btnPilt.Click += BtnPilt_Click;
            Teema.StiliseeriNupp(btnPilt);

            btnMath = new Button();
            btnMath.Text = "2. Matemaatiline mäng";
            btnMath.Size = new Size(220, 35);
            btnMath.Location = new Point(20, 115);
            btnMath.Click += BtnMath_Click;
            Teema.StiliseeriNupp(btnMath);

            btnMatch = new Button();
            btnMatch.Text = "3. Sarnaste piltide mäng";
            btnMatch.Size = new Size(220, 35);
            btnMatch.Location = new Point(20, 160);
            btnMatch.Click += BtnMatch_Click;
            Teema.StiliseeriNupp(btnMatch);

            btnAjalugu = new Button();
            btnAjalugu.Text = "Tulemuste ajalugu";
            btnAjalugu.Size = new Size(220, 35);
            btnAjalugu.Location = new Point(20, 215);
            btnAjalugu.Click += BtnAjalugu_Click;
            Teema.StiliseeriNupp(btnAjalugu);

            btnLogiValja = new Button();
            btnLogiValja.Text = "Logi välja";
            btnLogiValja.Size = new Size(220, 35);
            btnLogiValja.Location = new Point(20, 260);
            btnLogiValja.Click += BtnLogiValja_Click;
            Teema.StiliseeriNupp(btnLogiValja);

            this.Controls.Add(lbl);
            this.Controls.Add(btnPilt);
            this.Controls.Add(btnMath);
            this.Controls.Add(btnMatch);
            this.Controls.Add(btnAjalugu);
            this.Controls.Add(btnLogiValja);

            // kui peamenüü suletakse (mitte väljalogimise ega mängu avamise kaudu),
            // sulgub kogu rakendus
            this.FormClosed += new FormClosedEventHandler(MainForm_FormClosed);
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!valjaLogitud)
            {
                Application.Exit();
            }
        }

        private void BtnPilt_Click(object sender, EventArgs e)
        {
            PictureViewerForm f = new PictureViewerForm(this);
            f.Show();
            this.Hide();
        }

        private void BtnMath_Click(object sender, EventArgs e)
        {
            MathQuizForm f = new MathQuizForm(this);
            f.Show();
            this.Hide();
        }

        private void BtnMatch_Click(object sender, EventArgs e)
        {
            MatchingGameForm f = new MatchingGameForm(this);
            f.Show();
            this.Hide();
        }

        private void BtnAjalugu_Click(object sender, EventArgs e)
        {
            string ajalugu = Mangija.LoeAjalugu();
            MessageBox.Show(
                "Kokku punkte: " + Mangija.KogutudPunktid + "\n\n" + ajalugu,
                "Tulemuste ajalugu");
        }

        //BtnLogiValja_Click — очищает Mangija.Kasutajanimi,
        //показывает loginVorm.Show(), закрывает себя
        private void BtnLogiValja_Click(object sender, EventArgs e)
        {
            valjaLogitud = true;
            Mangija.Kasutajanimi = "";

            loginVorm.Show();
            this.Close();
        }
    }
}