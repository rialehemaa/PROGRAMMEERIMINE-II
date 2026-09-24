using System;
using System.Drawing;
using System.Media;
using System.Windows.Forms;

namespace KolmRakendust
{
    public class MathQuizForm : Form
    {
        GroupBox grpTase, grpAeg;
        RadioButton[] taseNupud = new RadioButton[3];
        RadioButton[] ajaNupud = new RadioButton[3];
        int[] ajaValikud = { 30, 45, 60 };

        Label lblAeg;
        TextBox txtAeg;
        Label[] opA = new Label[4];
        Label[] opTehe = new Label[4];
        Label[] opB = new Label[4];
        Label[] vordub = new Label[4];
        NumericUpDown[] vastused = new NumericUpDown[4];
        Label[] tulemused = new Label[4];
        Button btnAlusta, btnKontrolli;
        Timer timer;
        int sekundeid;
        MathProblem[] ylesanded = new MathProblem[4];
        char[] tehted = { '+', '-', '*', '/' };
        int valitudTase;

        public MathQuizForm()
        {
            this.Text = "Matemaatiline mäng";
            this.Width = 560;
            this.Height = 520;

            LisaNavigatsioonMenu();

            grpTase = new GroupBox();
            grpTase.Text = "Tase";
            grpTase.Location = new Point(20, 15);
            grpTase.Size = new Size(170, 115);
            string[] taseSildid = { "1. - 4. klass", "5. - 9. klass", "10. - 12. klass" };
            for (int i = 0; i < 3; i++)
            {
                taseNupud[i] = new RadioButton();
                taseNupud[i].Text = taseSildid[i];
                taseNupud[i].Location = new Point(15, 25 + i * 28);
                taseNupud[i].AutoSize = true;
                grpTase.Controls.Add(taseNupud[i]);
            }
            taseNupud[0].Checked = true;

            grpAeg = new GroupBox();
            grpAeg.Text = "Aeg";
            grpAeg.Location = new Point(210, 15);
            grpAeg.Size = new Size(170, 115);
            string[] ajaSildid = { "30 sekundit", "45 sekundit", "60 sekundit" };
            for (int i = 0; i < 3; i++)
            {
                ajaNupud[i] = new RadioButton();
                ajaNupud[i].Text = ajaSildid[i];
                ajaNupud[i].Location = new Point(15, 25 + i * 28);
                ajaNupud[i].AutoSize = true;
                grpAeg.Controls.Add(ajaNupud[i]);
            }
            ajaNupud[0].Checked = true;

            lblAeg = new Label();
            lblAeg.Text = "Aega jäänud";
            lblAeg.AutoSize = true;
            lblAeg.Font = new Font("Arial", 12);
            lblAeg.Location = new Point(120, 145);

            txtAeg = new TextBox();
            txtAeg.Location = new Point(230, 143);
            txtAeg.Width = 110;
            txtAeg.ReadOnly = true;
            txtAeg.Text = "30 sekundit";

            int y = 190;
            for (int i = 0; i < 4; i++)
            {
                opA[i] = new Label();
                opA[i].AutoSize = true;
                opA[i].Font = new Font("Arial", 14);
                opA[i].Location = new Point(30, y);

                opTehe[i] = new Label();
                opTehe[i].AutoSize = true;
                opTehe[i].Font = new Font("Arial", 14);
                opTehe[i].Location = new Point(110, y);

                opB[i] = new Label();
                opB[i].AutoSize = true;
                opB[i].Font = new Font("Arial", 14);
                opB[i].Location = new Point(150, y);

                vordub[i] = new Label();
                vordub[i].Text = "=";
                vordub[i].AutoSize = true;
                vordub[i].Font = new Font("Arial", 14);
                vordub[i].Location = new Point(220, y);

                vastused[i] = new NumericUpDown();
                vastused[i].Location = new Point(250, y);
                vastused[i].Width = 100;
                vastused[i].Minimum = -100000;
                vastused[i].Maximum = 100000;
                vastused[i].Font = new Font("Arial", 12);
                vastused[i].Enabled = false;

                tulemused[i] = new Label();
                tulemused[i].AutoSize = true;
                tulemused[i].Font = new Font("Arial", 10);
                tulemused[i].Location = new Point(360, y + 4);
                tulemused[i].Text = "";

                this.Controls.Add(opA[i]);
                this.Controls.Add(opTehe[i]);
                this.Controls.Add(opB[i]);
                this.Controls.Add(vordub[i]);
                this.Controls.Add(vastused[i]);
                this.Controls.Add(tulemused[i]);

                y += 50;
            }

            btnAlusta = new Button();
            btnAlusta.Text = "Alusta mängu";
            btnAlusta.Size = new Size(150, 35);
            btnAlusta.Location = new Point(50, y + 20);
            btnAlusta.Click += BtnAlusta_Click;

            btnKontrolli = new Button();
            btnKontrolli.Text = "Kontrolli vastuseid";
            btnKontrolli.Size = new Size(150, 35);
            btnKontrolli.Location = new Point(220, y + 20);
            btnKontrolli.Enabled = false;
            btnKontrolli.Click += BtnKontrolli_Click;

            this.Controls.Add(grpTase);
            this.Controls.Add(grpAeg);
            this.Controls.Add(lblAeg);
            this.Controls.Add(txtAeg);
            this.Controls.Add(btnAlusta);
            this.Controls.Add(btnKontrolli);

            timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += Timer_Tick;
        }

        // Menüü, mis lubab liikuda otse teise rakenduse juurde
        private void LisaNavigatsioonMenu()
        {
            MainMenu menu = new MainMenu();
            MenuItem menuRakendused = new MenuItem("Rakendused");
            menuRakendused.MenuItems.Add("Pildivaataja", new EventHandler(MenuPilt_Select));
            menuRakendused.MenuItems.Add("Sarnaste piltide mäng", new EventHandler(MenuMatch_Select));
            menuRakendused.MenuItems.Add("-");
            menuRakendused.MenuItems.Add("Peamenüü", new EventHandler(MenuPeamenu_Select));
            menu.MenuItems.Add(menuRakendused);
            this.Menu = menu;
        }

        private void MenuPilt_Select(object sender, EventArgs e)
        {
            this.Close();
            PictureViewerForm f = new PictureViewerForm();
            f.Show();
        }

        private void MenuMatch_Select(object sender, EventArgs e)
        {
            this.Close();
            MatchingGameForm f = new MatchingGameForm();
            f.Show();
        }

        private void MenuPeamenu_Select(object sender, EventArgs e)
        {
            this.Close();
            MainForm f = new MainForm();
            f.Show();
        }

        private void BtnAlusta_Click(object sender, EventArgs e)
        {
            int taseIndeks = 0;
            for (int i = 0; i < 3; i++)
            {
                if (taseNupud[i].Checked) taseIndeks = i;
            }

            int ajaIndeks = 0;
            for (int i = 0; i < 3; i++)
            {
                if (ajaNupud[i].Checked) ajaIndeks = i;
            }

            valitudTase = taseIndeks;

            for (int i = 0; i < 4; i++)
            {
                ylesanded[i] = new MathProblem(tehted[i], taseIndeks);
                opA[i].Text = ylesanded[i].A.ToString();
                opTehe[i].Text = ylesanded[i].Tehe.ToString();
                opB[i].Text = ylesanded[i].B.ToString();
                vastused[i].Value = 0;
                vastused[i].Enabled = true;
                tulemused[i].Text = "";
            }

            sekundeid = ajaValikud[ajaIndeks];
            txtAeg.Text = sekundeid + " sekundit";
            btnAlusta.Enabled = false;
            btnKontrolli.Enabled = true;
            grpTase.Enabled = false;
            grpAeg.Enabled = false;
            timer.Start();
        }

        private void BtnKontrolli_Click(object sender, EventArgs e)
        {
            timer.Stop();
            LopetaMang();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            sekundeid--;
            txtAeg.Text = sekundeid + " sekundit";

            if (sekundeid <= 0)
            {
                timer.Stop();
                LopetaMang();
            }
        }

        private void LopetaMang()
        {
            int oigeid = 0;
            for (int i = 0; i < 4; i++)
            {
                if (ylesanded[i].Kontrolli((int)vastused[i].Value))
                {
                    oigeid++;
                    tulemused[i].Text = "✓ Õige";
                    tulemused[i].ForeColor = Color.Green;
                    SystemSounds.Asterisk.Play();
                }
                else
                {
                    tulemused[i].Text = "✗ Õige vastus: " + ylesanded[i].Vastus;
                    tulemused[i].ForeColor = Color.Red;
                    SystemSounds.Hand.Play();
                }
                vastused[i].Enabled = false;
            }

            int punktid = oigeid * 10 * (valitudTase + 1);
            string[] taseNimed = { "1.-4. klass", "5.-9. klass", "10.-12. klass" };
            Mangija.SalvestaTulemus(
                "Matemaatiline mäng",
                taseNimed[valitudTase] + ", õigeid " + oigeid + "/4",
                punktid);

            MessageBox.Show(
                "Õigeid vastuseid: " + oigeid + " / 4\nSaadud punktid: " + punktid,
                "Tulemus");
            btnAlusta.Enabled = true;
            btnKontrolli.Enabled = false;
            grpTase.Enabled = true;
            grpAeg.Enabled = true;
        }
    }
}