using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace KolmRakendust
{
    public class MatchingGameForm : Form
    {
        GroupBox grpSuurus;
        RadioButton rb4, rb6;
        Button btnAlusta;
        int valitudSuurus = 4;

        List<Label> sildid = new List<Label>();
        Label esimeneValitud = null;
        Label teineValitud = null;
        Timer varjaTimer;

        Label lblAeg;
        Timer aegTimer;
        int mooduSekundeid = 0;
        bool aegKaib = false;

        public MatchingGameForm()
        {
            this.Text = "Sarnaste piltide leidmise mäng";
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.Width = 560;
            this.Height = 200;

            grpSuurus = new GroupBox();
            grpSuurus.Text = "Mängulaua suurus";
            grpSuurus.Location = new Point(20, 15);
            grpSuurus.Size = new Size(210, 90);

            rb4 = new RadioButton();
            rb4.Text = "4 x 4 (8 paari)";
            rb4.Location = new Point(15, 25);
            rb4.AutoSize = true;
            rb4.Checked = true;

            rb6 = new RadioButton();
            rb6.Text = "6 x 6 (18 paari)";
            rb6.Location = new Point(15, 55);
            rb6.AutoSize = true;

            grpSuurus.Controls.Add(rb4);
            grpSuurus.Controls.Add(rb6);

            btnAlusta = new Button();
            btnAlusta.Text = "Alusta mängu";
            btnAlusta.Size = new Size(150, 35);
            btnAlusta.Location = new Point(260, 40);
            btnAlusta.Click += BtnAlusta_Click;

            lblAeg = new Label();
            lblAeg.Font = new Font("Arial", 12);
            lblAeg.AutoSize = true;
            lblAeg.Location = new Point(20, 10);
            lblAeg.Visible = false;

            this.Controls.Add(grpSuurus);
            this.Controls.Add(btnAlusta);
            this.Controls.Add(lblAeg);

            // Taimer, mis peidab kaks mittesobivat ikooni
            // 750 ms pärast - täpselt nagu tuutorialis
            varjaTimer = new Timer();
            varjaTimer.Interval = 750;
            varjaTimer.Tick += VarjaTimer_Tick;

            aegTimer = new Timer();
            aegTimer.Interval = 1000;
            aegTimer.Tick += AegTimer_Tick;
        }

        private void BtnAlusta_Click(object sender, EventArgs e)
        {
            valitudSuurus = rb6.Checked ? 6 : 4;

            foreach (Label l in sildid)
            {
                this.Controls.Remove(l);
            }
            sildid.Clear();
            esimeneValitud = null;
            teineValitud = null;
            varjaTimer.Stop();
            aegTimer.Stop();
            mooduSekundeid = 0;
            aegKaib = false;

            grpSuurus.Visible = false;
            btnAlusta.Visible = false;
            lblAeg.Visible = true;
            lblAeg.Text = "Aeg: 0 sekundit";
            lblAeg.Location = new Point(20, 10);

            int ruudu = valitudSuurus == 4 ? 130 : 87;
            this.Width = valitudSuurus * ruudu + 25;
            this.Height = 40 + valitudSuurus * ruudu + 45;

            LooSildid();
            MaaraIkoonidSildile();
        }

        private void LooSildid()
        {
            int ruudu = valitudSuurus == 4 ? 130 : 87;
            int algusY = 40;
            // 4x4 puhul täpselt tuutoriali suurus (48), suurema laua puhul väiksem
            int fontSuurus = valitudSuurus == 4 ? 48 : 22;
            string fondiNimi = valitudSuurus == 4 ? "Webdings" : "Arial";
            int veerg = 0, rida = 0;

            for (int i = 0; i < valitudSuurus * valitudSuurus; i++)
            {
                Label l = new Label();
                l.Size = new Size(ruudu, ruudu);
                l.Location = new Point(veerg * ruudu, algusY + rida * ruudu);
                l.Font = new Font(fondiNimi, fontSuurus, FontStyle.Bold);
                l.TextAlign = ContentAlignment.MiddleCenter;
                l.BackColor = Color.CornflowerBlue;
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Click += Silt_Click;

                sildid.Add(l);
                this.Controls.Add(l);

                veerg++;
                if (veerg == valitudSuurus) { veerg = 0; rida++; }
            }
        }

        // Igal ikoonil on Webdings fondis oma tähendus:
        // "!" = ämblik, "N" = silm, "," = tšillipipar,
        // "k", "b", "v", "w", "z" = teised Webdings ikoonid
        private void MaaraIkoonidSildile()
        {
            List<string> ikoonid;

            if (valitudSuurus == 4)
            {
                // täpselt tuutoriali nimekiri
                ikoonid = new List<string>()
                {
                    "!", "!", "N", "N", ",", ",", "k", "k",
                    "b", "b", "v", "v", "w", "w", "z", "z"
                };
            }
            else
            {
                // 6x6 laud on minu enda lisatud valik (tuutorialis seda pole),
                // seega kasutan lihtsaid tähti, sest Webdingsi 18 ikooni koodid
                // pole tuutorialis kinnitatud
                ikoonid = new List<string>();
                for (int i = 0; i < (valitudSuurus * valitudSuurus) / 2; i++)
                {
                    string taht = ((char)('A' + i)).ToString();
                    ikoonid.Add(taht);
                    ikoonid.Add(taht);
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

            if (!aegKaib)
            {
                aegKaib = true;
                mooduSekundeid = 0;
                lblAeg.Text = "Aeg: 0 sekundit";
                aegTimer.Start();
            }

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
                esimeneValitud = null;
                teineValitud = null;
                return;
            }

            // kaks erinevat ikooni valitud - käivita taimer,
            // mis 750 ms pärast need peidab
            varjaTimer.Start();
        }

        private void AegTimer_Tick(object sender, EventArgs e)
        {
            mooduSekundeid++;
            lblAeg.Text = "Aeg: " + mooduSekundeid + " sekundit";
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
            aegKaib = false;
            MessageBox.Show("Palju õnne! Leidsid kõik paarid " + mooduSekundeid + " sekundiga!", "Võit!");

            foreach (Label l in sildid)
            {
                this.Controls.Remove(l);
            }
            sildid.Clear();

            lblAeg.Visible = false;
            grpSuurus.Visible = true;
            btnAlusta.Visible = true;
            this.Width = 560;
            this.Height = 200;
        }
    }
}