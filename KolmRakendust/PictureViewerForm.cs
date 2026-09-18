using System;
using System.Drawing;
using System.Windows.Forms;

namespace KolmRakendust
{
    public class PictureViewerForm : Form
    {
        PictureBox pic;
        CheckBox chkVeni, chkSlaid;
        Button btnNaita, btnTaust, btnTyhjenda, btnPoorra, btnPeegelda, btnSulge;
        PictureManager manager = new PictureManager();
        Timer slaidTimer;

        public PictureViewerForm()
        {
            this.Text = "Pildivaataja";
            this.Width = 800;
            this.Height = 640;

            pic = new PictureBox();
            pic.Dock = DockStyle.Fill;
            pic.SizeMode = PictureBoxSizeMode.Normal;
            pic.BackColor = Color.White;

            Panel paneel = new Panel();
            paneel.Dock = DockStyle.Bottom;
            paneel.Height = 75;

            chkVeni = new CheckBox();
            chkVeni.Text = "Venita";
            chkVeni.Location = new Point(10, 10);
            chkVeni.CheckedChanged += ChkVeni_CheckedChanged;

            chkSlaid = new CheckBox();
            chkSlaid.Text = "Slaidiesitus";
            chkSlaid.Location = new Point(10, 40);
            chkSlaid.CheckedChanged += ChkSlaid_CheckedChanged;

            btnNaita = new Button();
            btnNaita.Text = "Näita pilte";
            btnNaita.Location = new Point(150, 8);
            btnNaita.Size = new Size(100, 28);
            btnNaita.Click += BtnNaita_Click;

            btnTaust = new Button();
            btnTaust.Text = "Määra taustavärv";
            btnTaust.Location = new Point(260, 8);
            btnTaust.Size = new Size(140, 28);
            btnTaust.Click += BtnTaust_Click;

            btnTyhjenda = new Button();
            btnTyhjenda.Text = "Tühjenda pilt";
            btnTyhjenda.Location = new Point(410, 8);
            btnTyhjenda.Size = new Size(110, 28);
            btnTyhjenda.Click += BtnTyhjenda_Click;

            btnPoorra = new Button();
            btnPoorra.Text = "Pööra 90°";
            btnPoorra.Location = new Point(150, 40);
            btnPoorra.Size = new Size(100, 28);
            btnPoorra.Click += BtnPoorra_Click;

            btnPeegelda = new Button();
            btnPeegelda.Text = "Peegelda";
            btnPeegelda.Location = new Point(260, 40);
            btnPeegelda.Size = new Size(100, 28);
            btnPeegelda.Click += BtnPeegelda_Click;

            btnSulge = new Button();
            btnSulge.Text = "Sulge";
            btnSulge.Location = new Point(700, 22);
            btnSulge.Size = new Size(80, 28);
            btnSulge.Click += BtnSulge_Click;

            paneel.Controls.Add(chkVeni);
            paneel.Controls.Add(chkSlaid);
            paneel.Controls.Add(btnNaita);
            paneel.Controls.Add(btnTaust);
            paneel.Controls.Add(btnTyhjenda);
            paneel.Controls.Add(btnPoorra);
            paneel.Controls.Add(btnPeegelda);
            paneel.Controls.Add(btnSulge);

            this.Controls.Add(pic);
            this.Controls.Add(paneel);

            slaidTimer = new Timer();
            slaidTimer.Interval = 2000;
            slaidTimer.Tick += SlaidTimer_Tick;
        }

        private void ChkVeni_CheckedChanged(object sender, EventArgs e)
        {
            pic.SizeMode = chkVeni.Checked ? PictureBoxSizeMode.StretchImage : PictureBoxSizeMode.Normal;
        }

        private void ChkSlaid_CheckedChanged(object sender, EventArgs e)
        {
            if (chkSlaid.Checked)
            {
                if (manager.Count < 2)
                {
                    MessageBox.Show("Slaidiesituse jaoks lisa vähemalt 2 pilti.", "Slaidiesitus");
                    chkSlaid.Checked = false;
                    return;
                }
                slaidTimer.Start();
            }
            else
            {
                slaidTimer.Stop();
            }
        }

        private void SlaidTimer_Tick(object sender, EventArgs e)
        {
            manager.Jargmine();
            NaitaPraegust();
        }

        private void BtnNaita_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Pildifailid|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                foreach (string fail in ofd.FileNames)
                {
                    manager.Lisa(fail);
                }
                NaitaPraegust();
            }
        }

        private void BtnTaust_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            if (cd.ShowDialog() == DialogResult.OK)
            {
                pic.BackColor = cd.Color;
            }
        }

        private void BtnTyhjenda_Click(object sender, EventArgs e)
        {
            manager.Tyhjenda();
            pic.Image = null;
            slaidTimer.Stop();
            chkSlaid.Checked = false;
        }

        private void BtnPoorra_Click(object sender, EventArgs e)
        {
            if (pic.Image == null) return;
            pic.Image.RotateFlip(RotateFlipType.Rotate90FlipNone);
            pic.Invalidate();
        }

        private void BtnPeegelda_Click(object sender, EventArgs e)
        {
            if (pic.Image == null) return;
            pic.Image.RotateFlip(RotateFlipType.RotateNoneFlipX);
            pic.Invalidate();
        }

        private void NaitaPraegust()
        {
            if (manager.PraeguneTee == null)
            {
                pic.Image = null;
                return;
            }
            pic.Load(manager.PraeguneTee);
        }

        private void BtnSulge_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}