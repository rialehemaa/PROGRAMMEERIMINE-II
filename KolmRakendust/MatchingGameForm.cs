using System;
using System.Collections.Generic;
using System.Drawing;
using System.Media;
using System.Windows.Forms;

namespace KolmRakendust
{
    public class MatchingGameForm : Form
    {
        GroupBox grpSuurus, grpRaskus;
        RadioButton rb4, rb6, rb8, rb10;
        RadioButton rbKerge, rbKeskmine, rbRaske;
        Button btnAlusta, btnValju;
        int valitudSuurus = 4;
        int ajaLimiit = 60;

        List<Label> sildid = new List<Label>();
        Label esimeneValitud = null;
        Label teineValitud = null;
        Timer varjaTimer;

        Label lblAeg;
        Timer aegTimer;
        int jaanudAeg;
        MainForm peaVorm;
        bool liigub = false;

        public MatchingGameForm(MainForm peaVorm)
        {
            this.peaVorm = peaVorm;

            this.Text = "Sarnaste piltide leidmise mäng";
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.Width = 560;
            this.Height = 300;
            Teema.StiliseeriVorm(this);

            this.FormClosed += new FormClosedEventHandler(MatchingGameForm_FormClosed);

            LisaNavigatsioonMenu();

            grpSuurus = new GroupBox();
            grpSuurus.Text = "Mängulaua suurus";
            grpSuurus.Location = new Point(20, 15);
            grpSuurus.Size = new Size(210, 150);
            Teema.StiliseeriGrupp(grpSuurus);

            rb4 = new RadioButton();
            rb4.Text = "4 x 4 (8 paari)";
            rb4.Location = new Point(15, 25);
            rb4.AutoSize = true;
            rb4.Checked = true;

            rb6 = new RadioButton();
            rb6.Text = "6 x 6 (18 paari)";
            rb6.Location = new Point(15, 55);
            rb6.AutoSize = true;

            rb8 = new RadioButton();
            rb8.Text = "8 x 8 (32 paari)";
            rb8.Location = new Point(15, 85);
            rb8.AutoSize = true;

            rb10 = new RadioButton();
            rb10.Text = "10 x 10 (50 paari)";
            rb10.Location = new Point(15, 115);
            rb10.AutoSize = true;

            grpSuurus.Controls.Add(rb4);
            grpSuurus.Controls.Add(rb6);
            grpSuurus.Controls.Add(rb8);
            grpSuurus.Controls.Add(rb10);

            grpRaskus = new GroupBox();
            grpRaskus.Text = "Raskusaste (aeg)";
            grpRaskus.Location = new Point(250, 15);
            grpRaskus.Size = new Size(210, 150);
            Teema.StiliseeriGrupp(grpRaskus);

            rbKerge = new RadioButton();
            rbKerge.Text = "Kerge (1:00)";
            rbKerge.Location = new Point(15, 25);
            rbKerge.AutoSize = true;
            rbKerge.Checked = true;

            rbKeskmine = new RadioButton();
            rbKeskmine.Text = "Keskmine (1:15)";
            rbKeskmine.Location = new Point(15, 55);
            rbKeskmine.AutoSize = true;

            rbRaske = new RadioButton();
            rbRaske.Text = "Raske (2:30)";
            rbRaske.Location = new Point(15, 85);
            rbRaske.AutoSize = true;

            grpRaskus.Controls.Add(rbKerge);
            grpRaskus.Controls.Add(rbKeskmine);
            grpRaskus.Controls.Add(rbRaske);

            btnAlusta = new Button();
            btnAlusta.Text = "Alusta mängu";
            btnAlusta.Size = new Size(150, 35);
            btnAlusta.Location = new Point(110, 180);
            btnAlusta.Click += BtnAlusta_Click;
            Teema.StiliseeriNupp(btnAlusta);

            lblAeg = new Label();
            lblAeg.Font = new Font("Arial", 12);
            lblAeg.ForeColor = Teema.TekstiVarv;
            lblAeg.AutoSize = true;
            lblAeg.Location = new Point(20, 5);
            lblAeg.Visible = false;

            btnValju = new Button();
            btnValju.Text = "Välju mängust";
            btnValju.Size = new Size(150, 35);
            btnValju.Location = new Point(280, 180);
            btnValju.Click += BtnValju_Click;
            Teema.StiliseeriNupp(btnValju);

            this.Controls.Add(grpSuurus);
            this.Controls.Add(grpRaskus);
            this.Controls.Add(btnAlusta);
            this.Controls.Add(lblAeg);
            this.Controls.Add(btnValju);

            // Taimer, mis peidab kaks mittesobivat ikooni
            // 750 ms pärast - täpselt nagu tuutorialis
            varjaTimer = new Timer();
            varjaTimer.Interval = 750;
            varjaTimer.Tick += VarjaTimer_Tick;

            // Taimer, mis loeb allapoole mängule antud ajapiirangust
            aegTimer = new Timer();
            aegTimer.Interval = 1000;
            aegTimer.Tick += AegTimer_Tick;
        }

        // Menüü, mis lubab liikuda otse teise rakenduse juurde
        private void LisaNavigatsioonMenu()
        {
            MainMenu menu = new MainMenu();
            MenuItem menuRakendused = new MenuItem("Rakendused");
            menuRakendused.MenuItems.Add("Pildivaataja", new EventHandler(MenuPilt_Select));
            menuRakendused.MenuItems.Add("Matemaatiline mäng", new EventHandler(MenuMath_Select));
            menuRakendused.MenuItems.Add("-");
            menuRakendused.MenuItems.Add("Peamenüü", new EventHandler(MenuPeamenu_Select));
            menu.MenuItems.Add(menuRakendused);
            this.Menu = menu;
        }

        private void MatchingGameForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!liigub)
            {
                peaVorm.Show();
            }
        }

        private void MenuPilt_Select(object sender, EventArgs e)
        {
            liigub = true;
            this.Close();
            PictureViewerForm f = new PictureViewerForm(peaVorm);
            f.Show();
        }

        private void MenuMath_Select(object sender, EventArgs e)
        {
            liigub = true;
            this.Close();
            MathQuizForm f = new MathQuizForm(peaVorm);
            f.Show();
        }

        private void MenuPeamenu_Select(object sender, EventArgs e)
        {
            liigub = true;
            this.Close();
            peaVorm.Show();
        }

        private void BtnValju_Click(object sender, EventArgs e)
        {
            liigub = true;
            varjaTimer.Stop();
            aegTimer.Stop();
            this.Close();
            peaVorm.Show();
        }

        private string FormatAeg(int sekundid)
        {
            int min = sekundid / 60;
            int sek = sekundid % 60;
            return min + ":" + sek.ToString("00");
        }

        //размери сетки
        private int RuuduSuurus()
        {
            if (valitudSuurus == 4) return 130;
            if (valitudSuurus == 6) return 87;
            if (valitudSuurus == 8) return 65;
            return 52; // 10x10
        }
        //размери шрифта
        private int FondiSuurus()
        {
            if (valitudSuurus == 4) return 48;
            if (valitudSuurus == 6) return 22;
            if (valitudSuurus == 8) return 16;
            return 14; // 10x10
        }

        private void BtnAlusta_Click(object sender, EventArgs e)
        {
            if (rb10.Checked) valitudSuurus = 10;
            else if (rb8.Checked) valitudSuurus = 8;
            else if (rb6.Checked) valitudSuurus = 6;
            else valitudSuurus = 4;

            ajaLimiit = 60;
            if (rbKeskmine.Checked) ajaLimiit = 75;
            if (rbRaske.Checked) ajaLimiit = 150;

            foreach (Label l in sildid)
            {
                this.Controls.Remove(l);
            }
            sildid.Clear();
            esimeneValitud = null;
            teineValitud = null;
            varjaTimer.Stop();
            aegTimer.Stop();

            grpSuurus.Visible = false;
            grpRaskus.Visible = false;
            btnAlusta.Visible = false;
            lblAeg.Visible = true;
            lblAeg.Location = new Point(20, 5);

            jaanudAeg = ajaLimiit;
            lblAeg.Text = "Aega jäänud: " + FormatAeg(jaanudAeg);

            int ruudu = RuuduSuurus();
            this.Width = valitudSuurus * ruudu + 25;
            this.Height = 40 + valitudSuurus * ruudu + 45;

            // mängu ajal ei mahu nupp enam Alusta mängu kõrvale ära -
            // tõsta see ülemisse paremasse nurka, kus kaardid veel ei alga (need algavad y=40 juures)
            btnValju.Size = new Size(130, 25);
            btnValju.Location = new Point(this.Width - 150, 5);

            LooSildid();
            MaaraIkoonidSildile();

            aegTimer.Start();
        }

        private void LooSildid()
        {
            int ruudu = RuuduSuurus();
            int algusY = 40;
            int fontSuurus = FondiSuurus();
            string fondiNimi = "Arial";
            if (valitudSuurus == 4) fondiNimi = "Webdings";
            int veerg = 0, rida = 0;

            for (int i = 0; i < valitudSuurus * valitudSuurus; i++)
            {
                Label l = new Label();
                l.Size = new Size(ruudu, ruudu);
                l.Location = new Point(veerg * ruudu, algusY + rida * ruudu);
                l.Font = new Font(fondiNimi, fontSuurus, FontStyle.Bold);
                l.TextAlign = ContentAlignment.MiddleCenter;
                l.BackColor = Teema.KaardiVarv;
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Click += Silt_Click;

                sildid.Add(l);
                this.Controls.Add(l);

                veerg++;
                if (veerg == valitudSuurus) { veerg = 0; rida++; }
            }
        }

        // 4x4 puhul täpselt tuutoriali Webdings ikoonid:
        // "!" = ämblik, "N" = silm, "," = tšillipipar,
        // "k", "b", "v", "w", "z" = teised Webdings ikoonid.
        // Suuremate laudade jaoks (6x6, 10x10, tuutorialis pole neid)
        // kasutan numbreid, sest neid jagub piisavalt (kuni 50 paari)
        private void MaaraIkoonidSildile()
        {
            List<string> ikoonid = new List<string>();

            if (valitudSuurus == 4)
            {
                ikoonid.AddRange(new string[]
                {
                    "!", "!", "N", "N", ",", ",", "k", "k",
                    "b", "b", "v", "v", "w", "w", "z", "z"
                });
            }
            else
            {
                int paariArv = (valitudSuurus * valitudSuurus) / 2;
                for (int i = 1; i <= paariArv; i++)
                {
                    ikoonid.Add(i.ToString());
                    ikoonid.Add(i.ToString());
                }
            }

            Random rnd = new Random();
            foreach (Label l in sildid)
            {
                int juhuslikIndeks = rnd.Next(ikoonid.Count);
                l.Text = ikoonid[juhuslikIndeks];
                l.ForeColor = l.BackColor;
                ikoonid.RemoveAt(juhuslikIndeks);
            }
        }

        private void Silt_Click(object sender, EventArgs e)
        {
            // taimer töötab ainult siis, kui kaks mittesobivat
            // ikooni on juba näidatud - siis klõpse ignoreeritakse
            if (varjaTimer.Enabled) return;

            Label klikitud = sender as Label;
            if (klikitud == null) return;

            // kui ikoon on juba must, on kaart juba avatud - ignoreeri
            if (klikitud.ForeColor == Color.Black) return;

            // kui esimeneValitud on null, on see paari esimene ikoon
            if (esimeneValitud == null)
            {
                esimeneValitud = klikitud;
                esimeneValitud.ForeColor = Color.Black;
                return;
            }

            // see on paari teine ikoon
            teineValitud = klikitud;
            teineValitud.ForeColor = Color.Black;

            KontrolliVoitu();

            // kui mängija valis kaks sobivat ikooni, jäävad need
            // mustaks ja taimerit ei käivitata
            if (esimeneValitud.Text == teineValitud.Text)
            {
                SystemSounds.Asterisk.Play();
                esimeneValitud = null;
                teineValitud = null;
                return;
            }

            // kaks erinevat ikooni valitud - käivita taimer,
            // mis 750 ms pärast need peidab
            SystemSounds.Exclamation.Play();
            varjaTimer.Start();
        }
        //обратный отсчёт времени, при 0 → MangKaotatud()
        private void AegTimer_Tick(object sender, EventArgs e)
        {
            jaanudAeg--;
            lblAeg.Text = "Aega jäänud: " + FormatAeg(jaanudAeg);

            if (jaanudAeg <= 0)
            {
                aegTimer.Stop();
                MangKaotatud();
            }
        }

        private void VarjaTimer_Tick(object sender, EventArgs e)
        {
            varjaTimer.Stop();

            esimeneValitud.ForeColor = esimeneValitud.BackColor;
            teineValitud.ForeColor = teineValitud.BackColor;

            esimeneValitud = null;
            teineValitud = null;
        }

        // Käib läbi kõik sildid ja kontrollib, kas ikooni värv
        // on sama, mis taust (st peidus). Kui kõik on avatud
        // (sobitatud), on mängija võitnud
        private void KontrolliVoitu()
        {
            foreach (Label l in sildid)
            {
                if (l.ForeColor == l.BackColor) return;
            }

            aegTimer.Stop();
            SystemSounds.Beep.Play();

            // punktid: rohkem allesjäänud aega ja suurem laud annavad rohkem punkte
            int kordaja;
            if (valitudSuurus == 4) kordaja = 1;
            else if (valitudSuurus == 6) kordaja = 2;
            else if (valitudSuurus == 8) kordaja = 3;
            else kordaja = 4; // 10x10
            int punktid = jaanudAeg * 5 * kordaja;

            Mangija.SalvestaTulemus(
                "Sarnaste piltide mäng",
                valitudSuurus + "x" + valitudSuurus + ", jäi aega " + FormatAeg(jaanudAeg),
                punktid);

            MessageBox.Show(
                "Palju õnne! Leidsid kõik paarid!\nAega jäi alles: " + FormatAeg(jaanudAeg) +
                "\nSaadud punktid: " + punktid,
                "Võit!");

            LopetaJaNaitaSeadistust();
        }

        private void MangKaotatud()
        {
            Mangija.SalvestaTulemus(
                "Sarnaste piltide mäng",
                valitudSuurus + "x" + valitudSuurus + ", aeg sai otsa",
                0);

            MessageBox.Show("Aeg sai otsa! Proovi uuesti.", "Mäng läbi");

            LopetaJaNaitaSeadistust();
        }
        //общий метод возврата на экран настроек
        //(и после победы, и после проигрыша)
        private void LopetaJaNaitaSeadistust()
        {
            foreach (Label l in sildid)
            {
                this.Controls.Remove(l);
            }
            sildid.Clear();

            lblAeg.Visible = false;
            grpSuurus.Visible = true;
            grpRaskus.Visible = true;
            btnAlusta.Visible = true;
            this.Width = 560;
            this.Height = 300;

            btnValju.Size = new Size(150, 35);
            btnValju.Location = new Point(280, 180);
        }
    }
}