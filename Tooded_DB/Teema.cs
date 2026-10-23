using System;
using System.Drawing;
using System.Windows.Forms;

namespace Tooded_DB
{
    // Ühtne kujundus kogu rakendusele: värvid, font, nupud, tabelid, vahekaardid.
    // Kasutus: Teema.Rakenda(this); vormi konstruktori lõpus.
    public static class Teema
    {
        public static readonly Color Taust = Color.FromArgb(250, 247, 252);
        public static readonly Color Kaart = Color.White;
        public static readonly Color Pohi = Color.FromArgb(188, 204, 244);          // pastelne sinine
        public static readonly Color PohiHover = Color.FromArgb(165, 184, 236);
        public static readonly Color Hall = Color.FromArgb(220, 226, 238);          // pastelne hall
        public static readonly Color HallHover = Color.FromArgb(200, 208, 224);
        public static readonly Color Punane = Color.FromArgb(247, 187, 200);        // pastelne roosa
        public static readonly Color PunaneHover = Color.FromArgb(240, 160, 178);
        public static readonly Color Tekst = Color.FromArgb(55, 60, 90);
        public static readonly Color Pealkiri = Color.FromArgb(110, 115, 185);
        public static readonly Color Joon = Color.FromArgb(232, 232, 244);

        public static void Rakenda(Form vorm)
        {
            vorm.BackColor = Taust;
            vorm.ForeColor = Tekst;
            vorm.Font = new Font("Segoe UI", 10f);
            if (vorm.FormBorderStyle == FormBorderStyle.Sizable) Venita(vorm);
            Stiilita(vorm);
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
                if (c is DataGridView)
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
            b.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            b.Cursor = Cursors.Hand;
            b.UseVisualStyleBackColor = false;

            b.EnabledChanged += (s, e) =>
            {
                b.BackColor = b.Enabled ? varv : Color.FromArgb(236, 236, 244);
                b.ForeColor = b.Enabled ? Tekst : Color.FromArgb(160, 165, 185);
                b.Cursor = b.Enabled ? Cursors.Hand : Cursors.Default;
            };
            if (!b.Enabled)
            {
                b.BackColor = Color.FromArgb(236, 236, 244);
                b.ForeColor = Color.FromArgb(160, 165, 185);
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
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(228, 234, 252);
            g.DefaultCellStyle.SelectionForeColor = Tekst;
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(252, 250, 254);
            g.RowTemplate.Height = 30;
        }
    }
}