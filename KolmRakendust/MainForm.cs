using System;
using System.Drawing;
using System.Windows.Forms;

namespace KolmRakendust
{
    public class MainForm : Form
    {
        Button btnPilt, btnMath, btnMatch;
        Label lbl;

        public MainForm()
        {
            this.Text = "Kolm rakendust";
            this.Width = 320;
            this.Height = 260;

            lbl = new Label();
            lbl.Text = "Vali rakendus:";
            lbl.Font = new Font("Arial", 14);
            lbl.AutoSize = true;
            lbl.Location = new Point(20, 20);

            btnPilt = new Button();
            btnPilt.Text = "1. Pildivaataja";
            btnPilt.Size = new Size(220, 35);
            btnPilt.Location = new Point(20, 60);
            btnPilt.Click += BtnPilt_Click;

            btnMath = new Button();
            btnMath.Text = "2. Matemaatiline mäng";
            btnMath.Size = new Size(220, 35);
            btnMath.Location = new Point(20, 105);
            btnMath.Click += BtnMath_Click;

            btnMatch = new Button();
            btnMatch.Text = "3. Sarnaste piltide mäng";
            btnMatch.Size = new Size(220, 35);
            btnMatch.Location = new Point(20, 150);
            btnMatch.Click += BtnMatch_Click;

            this.Controls.Add(lbl);
            this.Controls.Add(btnPilt);
            this.Controls.Add(btnMath);
            this.Controls.Add(btnMatch);
        }

        private void BtnPilt_Click(object sender, EventArgs e)
        {
            PictureViewerForm f = new PictureViewerForm();
            f.Show();
        }

        private void BtnMath_Click(object sender, EventArgs e)
        {
            MathQuizForm f = new MathQuizForm();
            f.Show();
        }

        private void BtnMatch_Click(object sender, EventArgs e)
        {
            MatchingGameForm f = new MatchingGameForm();
            f.Show();
        }
    }
}
