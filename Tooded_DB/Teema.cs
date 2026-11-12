using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Tooded_DB
{
    // Ühtne kujundus kogu rakendusele: värvid, font, nupud, tabelid, vahekaardid.
    // Kasutus: Teema.Rakenda(this); vormi konstruktori lõpus.
    // Kaks teemat: hele ja tume. Valik salvestatakse faili teema.txt.
    public static class Teema
    {
        public static bool Tume { get; private set; }

        public static Color Taust, Kaart, Pohi, PohiHover, Hall, HallHover, Punane, PunaneHover;
        public static Color Tekst, Pealkiri, Joon, Valitud, Vahe, Keelatud, KeelatudTekst, Vaigne, Hoiatus, Korras;

        private static readonly string Fail = Path.Combine(Kaustad.Juur, "teema.txt");

        static Teema()
        {
            bool tume = false;
            try
            {
                if (File.Exists(Fail)) tume = File.ReadAllText(Fail).Trim() == "tume";
            }
            catch { }
            Palett(tume);
        }

        // vahetab teema ja jätab valiku meelde; uued vormid saavad uued värvid
        public static void Sea(bool tume)
        {
            Palett(tume);
            try { Directory.CreateDirectory(Kaustad.Juur); File.WriteAllText(Fail, tume ? "tume" : "hele"); } catch { }
        }

        private static void Palett(bool tume)
        {
            Tume = tume;
            if (!tume)
            {
                Taust = Color.FromArgb(250, 247, 252);
                Kaart = Color.White;
                Pohi = Color.FromArgb(188, 204, 244);          // pastelne sinine
                PohiHover = Color.FromArgb(165, 184, 236);
                Hall = Color.FromArgb(220, 226, 238);          // pastelne hall
                HallHover = Color.FromArgb(200, 208, 224);
                Punane = Color.FromArgb(247, 187, 200);        // pastelne roosa
                PunaneHover = Color.FromArgb(240, 160, 178);
                Tekst = Color.FromArgb(55, 60, 90);
                Pealkiri = Color.FromArgb(110, 115, 185);
                Joon = Color.FromArgb(232, 232, 244);
                Valitud = Color.FromArgb(228, 234, 252);
                Vahe = Color.FromArgb(252, 250, 254);
                Keelatud = Color.FromArgb(236, 236, 244);
                KeelatudTekst = Color.FromArgb(160, 165, 185);
                Vaigne = Color.DimGray;
                Hoiatus = Color.Firebrick;
                Korras = Color.SeaGreen;
            }
            else
            {
                Taust = Color.FromArgb(28, 30, 42);
                Kaart = Color.FromArgb(40, 43, 60);
                Pohi = Color.FromArgb(82, 98, 168);
                PohiHover = Color.FromArgb(100, 117, 190);
                Hall = Color.FromArgb(62, 66, 90);
                HallHover = Color.FromArgb(78, 83, 110);
                Punane = Color.FromArgb(150, 68, 92);
                PunaneHover = Color.FromArgb(172, 84, 110);
                Tekst = Color.FromArgb(232, 234, 246);
                Pealkiri = Color.FromArgb(168, 176, 240);
                Joon = Color.FromArgb(58, 62, 84);
                Valitud = Color.FromArgb(58, 68, 118);
                Vahe = Color.FromArgb(35, 38, 53);
                Keelatud = Color.FromArgb(48, 51, 70);
                KeelatudTekst = Color.FromArgb(110, 115, 140);
                Vaigne = Color.FromArgb(170, 176, 200);
                Hoiatus = Color.FromArgb(255, 130, 130);
                Korras = Color.FromArgb(110, 200, 140);
            }
        }

        public static void Rakenda(Form vorm)
        {
            vorm.BackColor = Taust;
            vorm.ForeColor = Tekst;
            vorm.Font = new Font("Segoe UI", 10f);
            if (vorm.FormBorderStyle == FormBorderStyle.Sizable) Venita(vorm);
            Stiilita(vorm);
            Keel.Rakenda(vorm);
        }

        // Muudetava suurusega aken: vormi otse all olevad kontrollid (mis pole Dock-iga) saavad Anchor-i,
        // nii et tabelid venivad aknaga kaasa ja nupud/väljad jäävad oma serva külge
        private static void Venita(Form vorm)
        {
            if (vorm.MinimumSize.IsEmpty)
                vorm.MinimumSize = new Size(vorm.Width * 7 / 10, vorm.Height * 7 / 10);
            int w = vorm.ClientSize.Width, h = vorm.ClientSize.Height;
            foreach (Control c in vorm.Controls)
            {
                if (c.Dock != DockStyle.None) continue;
                AnchorStyles a = AnchorStyles.Top | AnchorStyles.Left;
                if (c is Label) a = AnchorStyles.Top | AnchorStyles.Left;
                else if (c is DataGridView)
                    a = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
                else
                {
                    if (c.Left > w / 2) a = (a & ~AnchorStyles.Left) | AnchorStyles.Right;
                    if (c.Top > h * 0.6) a = (a & ~AnchorStyles.Top) | AnchorStyles.Bottom;
                    if (c is Button && c.Width > w * 0.6) a |= AnchorStyles.Left | AnchorStyles.Right;
                }
                c.Anchor = a;
            }
        }

        private static void Stiilita(Control kontroll)
        {
            foreach (Control c in kontroll.Controls)
            {
                switch (c)
                {
                    case Button b:
                        StiilitaNupp(b);
                        break;
                    case TextBox t:
                        t.BorderStyle = BorderStyle.FixedSingle;
                        t.BackColor = Kaart;
                        t.ForeColor = Tekst;
                        break;
                    case ComboBox cb:
                        cb.FlatStyle = FlatStyle.Flat;
                        cb.BackColor = Kaart;
                        cb.ForeColor = Tekst;
                        break;
                    case NumericUpDown n:
                        n.BackColor = Kaart;
                        n.ForeColor = Tekst;
                        break;
                    case ListBox lb:
                        lb.BackColor = Kaart;
                        lb.ForeColor = Tekst;
                        break;
                    case DataGridView g:
                        StiilitaTabel(g);
                        break;
                    case Label l:
                        if (l.Font.Bold && l.Font.Size >= 12 && (l.ForeColor == SystemColors.ControlText || l.ForeColor == Tekst))
                            l.ForeColor = Pealkiri;
                        break;
                    case TabControl tc:
                        tc.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
                        foreach (TabPage p in tc.TabPages) p.BackColor = Taust;
                        break;
                    case GroupBox gb:
                        gb.ForeColor = Tekst;
                        break;
                    case Panel p when p.BackColor == SystemColors.Control && p.GetType() == typeof(Panel):
                        p.BackColor = Taust;
                        break;
                }
                if (c.HasChildren) Stiilita(c);
            }
        }

        private static void StiilitaNupp(Button b)
        {
            // oma värviga nuppe (nt Kassa kaardid) ei puutu
            if (b.BackColor != SystemColors.Control && b.BackColor != Taust) return;

            Color varv = Pohi, hover = PohiHover;
            string t = b.Text ?? "";
            if (t.StartsWith("Kustuta") || t.StartsWith("Logi välja") || t.StartsWith("Tühjenda"))
            { varv = Punane; hover = PunaneHover; }
            else if (t.StartsWith("Tühista") || t.StartsWith("Sulge") || t.StartsWith("Registreeri") && b.Parent is Form f && f is LoginForm)
            { varv = Hall; hover = HallHover; }

            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = hover;
            b.FlatAppearance.MouseDownBackColor = hover;
            b.BackColor = varv;
            b.ForeColor = Tekst;
            b.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            b.Cursor = Cursors.Hand;
            b.UseVisualStyleBackColor = false;

            b.EnabledChanged += (s, e) =>
            {
                b.BackColor = b.Enabled ? varv : Keelatud;
                b.ForeColor = b.Enabled ? Tekst : KeelatudTekst;
                b.Cursor = b.Enabled ? Cursors.Hand : Cursors.Default;
            };
            if (!b.Enabled)
            {
                b.BackColor = Keelatud;
                b.ForeColor = KeelatudTekst;
            }
        }

        private static void StiilitaTabel(DataGridView g)
        {
            g.BackgroundColor = Kaart;
            g.BorderStyle = BorderStyle.None;
            g.EnableHeadersVisualStyles = false;
            g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            g.GridColor = Joon;
            g.RowHeadersVisible = false;
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            g.ColumnHeadersHeight = 36;
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.ColumnHeadersDefaultCellStyle.BackColor = Pohi;
            g.ColumnHeadersDefaultCellStyle.ForeColor = Tekst;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Pohi;
            g.DefaultCellStyle.BackColor = Kaart;
            g.DefaultCellStyle.ForeColor = Tekst;
            g.DefaultCellStyle.SelectionBackColor = Valitud;
            g.DefaultCellStyle.SelectionForeColor = Tekst;
            g.AlternatingRowsDefaultCellStyle.BackColor = Vahe;
            g.RowTemplate.Height = 30;
        }
    }
}