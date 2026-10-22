using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace Tooded_DB
{
    // Üks rida ostukorvis
    public class OstukorviRida
    {
        [Browsable(false)] public int ToodeId { get; set; }
        [Browsable(false)] public int Laos { get; set; }   // palju on laos (kontrolliks)

        public string Toode { get; set; }
        public int Kogus { get; set; }
        public decimal Hind { get; set; }
        public decimal Summa => Hind * Kogus;
    }

    public class KassaForm : Form
    {
        private const int VaheLaoseis = 3;   // alla selle on laoseis punane

        private FlowLayoutPanel flowTooted;   // toodete piltide võrk
        private ComboBox comboKategooria;
        private DataGridView gridKorv;
        private Label lblKokku, lblMaksta, lblKlient;
        private TextBox txtKaart;
        private string _viimaneTsekk;   // viimase tšeki PDF tee (e-maili saatmiseks)
        private CheckBox chkBoonus;
        private Klient _klient;               // valitud klient (null = klienti pole)
        private const decimal BoonusMaar = 0.05m;   // 5% ostusummast läheb boonusesse

        private readonly BindingList<OstukorviRida> _korv = new BindingList<OstukorviRida>();

        public KassaForm()
        {
            Text = "Kassa";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1200, 700);
            MinimumSize = new Size(1000, 600);

            EhitaUi();
            Load += (s, e) => { LaeKategooriad(); UuendaSumma(); };
        }

        // ---------- Kasutajaliides ----------
        private void EhitaUi()
        {
            // parem pool: ostukorv
            var paremPaneel = new Panel { Dock = DockStyle.Right, Width = 420 };

            var lKorv = new Label { Text = "Ostukorv", Location = new Point(10, 10), AutoSize = true, Font = new Font(Font.FontFamily, 11, FontStyle.Bold) };
            gridKorv = new DataGridView
            {
                Location = new Point(10, 38),
                Size = new Size(400, 290),
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                DataSource = _korv
            };

            var btnPlus = Nupp("+", 10, 338, 55, (s, e) => MuudaKogust(1));
            var btnMiinus = Nupp("−", 70, 338, 55, (s, e) => MuudaKogust(-1));
            var btnEemalda = Nupp("Eemalda", 130, 338, 90, (s, e) => EemaldaRida());
            var btnTuhjenda = Nupp("Tühjenda", 225, 338, 90, (s, e) => { _korv.Clear(); UuendaSumma(); });

            // kliendikaart
            var lKaart = new Label { Text = "Kliendikaart:", Location = new Point(10, 390), AutoSize = true };
            txtKaart = new TextBox { Location = new Point(95, 386), Width = 130 };
            txtKaart.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; OtsiKlient(); } };
            var btnOtsiKlient = Nupp("Otsi", 232, 384, 70, (s, e) => OtsiKlient());
            var btnEemaldaKlient = Nupp("Eemalda", 308, 384, 90, (s, e) => SeaKlient(null));
            lblKlient = new Label { Location = new Point(10, 422), Size = new Size(400, 22), ForeColor = Color.DimGray, Text = "Klienti pole valitud" };
            chkBoonus = new CheckBox { Text = "Kasuta boonust", Location = new Point(10, 448), AutoSize = true, Enabled = false };
            chkBoonus.CheckedChanged += (s, e) => UuendaSumma();

            lblKokku = new Label { Location = new Point(10, 478), Size = new Size(400, 24), Font = new Font(Font.FontFamily, 11, FontStyle.Bold) };
            lblMaksta = new Label { Location = new Point(10, 504), Size = new Size(400, 34), Font = new Font(Font.FontFamily, 15, FontStyle.Bold) };

            var btnMaksa = Nupp("Maksa ja loo tšekk (PDF)", 10, 548, 400, (s, e) => Maksa());
            btnMaksa.Height = 50;

            var btnEmail = Nupp("Saada viimane tšekk e-mailile", 10, 606, 400, (s, e) => SaadaEmail());
            btnEmail.Height = 36;

            paremPaneel.Controls.AddRange(new Control[] { btnEmail, lKorv, gridKorv, btnPlus, btnMiinus, btnEemalda, btnTuhjenda,
                lKaart, txtKaart, btnOtsiKlient, btnEemaldaKlient, lblKlient, chkBoonus, lblKokku, lblMaksta, btnMaksa });

            // ülemine riba: kategooria filter
            var ylaPaneel = new Panel { Dock = DockStyle.Top, Height = 45 };
            var lK = new Label { Text = "Kategooria:", Location = new Point(10, 14), AutoSize = true };
            comboKategooria = new ComboBox { Location = new Point(90, 10), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            var lKasutaja = new Label { Text = $"Müüja: {AktiivneKasutaja.Nimi}", Location = new Point(340, 14), AutoSize = true };
            ylaPaneel.Controls.AddRange(new Control[] { lK, comboKategooria, lKasutaja });

            // keskel: piltide võrk
            flowTooted = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.WhiteSmoke,
                Padding = new Padding(8)
            };

            // Fill-paneel tuleb lisada esimesena
            Controls.Add(flowTooted);
            Controls.Add(ylaPaneel);
            Controls.Add(paremPaneel);
        }

        private Button Nupp(string tekst, int x, int y, int laius, EventHandler klikk)
        {
            var b = new Button { Text = tekst, Location = new Point(x, y), Size = new Size(laius, 32) };
            b.Click += klikk;
            return b;
        }

        // ---------- Piltide võrk ----------
        private void LaeKategooriad()
        {
            try
            {
                using var db = new ToodedContext();
                var list = new List<Kategooria> { new Kategooria { Id = 0, Kategooria_nimetus = "Kõik" } };
                list.AddRange(db.Kategooriatabel.AsNoTracking().OrderBy(k => k.Kategooria_nimetus).ToList());

                comboKategooria.SelectedIndexChanged -= KategooriaMuutus;
                comboKategooria.DataSource = list;
                comboKategooria.DisplayMember = "Kategooria_nimetus";
                comboKategooria.ValueMember = "Id";
                comboKategooria.SelectedIndexChanged += KategooriaMuutus;

                LaeTooted();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Andmebaasi viga: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }

        private void KategooriaMuutus(object sender, EventArgs e) => LaeTooted();

        private void LaeTooted()
        {
            int katId = comboKategooria.SelectedValue is int id ? id : 0;

            List<Toode> tooted;
            using (var db = new ToodedContext())
            {
                var q = db.Toodetabel.AsNoTracking().AsQueryable();
                if (katId != 0) q = q.Where(t => t.KategooriaId == katId);
                tooted = q.OrderBy(t => t.Toodenimetus).ToList();
            }

            flowTooted.SuspendLayout();

            // vabasta vanad pildid mälust
            foreach (Control c in flowTooted.Controls)
                foreach (Control sisu in c.Controls)
                    if (sisu is PictureBox pb) pb.Image?.Dispose();
            flowTooted.Controls.Clear();

            foreach (var t in tooted)
                flowTooted.Controls.Add(LooKaart(t));

            flowTooted.ResumeLayout();
        }

        // Üks toote kaart: pilt + nimi + hind + laoseis. Klikk lisab toote korvi.
        private Panel LooKaart(Toode t)
        {
            bool otsas = t.Kogus <= 0;

            var p = new Panel
            {
                Size = new Size(160, 215),
                Margin = new Padding(6),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = otsas ? Color.Gainsboro : Color.White,
                Cursor = otsas ? Cursors.No : Cursors.Hand
            };

            var pilt = new PictureBox
            {
                Location = new Point(8, 8),
                Size = new Size(142, 120),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = LaePilt(t.Pilt)
            };

            var nimi = new Label
            {
                Text = t.Toodenimetus,
                Location = new Point(4, 132),
                Size = new Size(150, 22),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(Font.FontFamily, 9.5f, FontStyle.Bold),
                AutoEllipsis = true
            };

            var hind = new Label
            {
                Text = Math.Round((decimal)t.Hind, 2).ToString("0.00") + " €",
                Location = new Point(4, 156),
                Size = new Size(150, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            bool vahe = t.Kogus <= VaheLaoseis;
            var laos = new Label
            {
                Text = otsas ? "OTSAS" : $"Laos: {t.Kogus} tk",
                Location = new Point(4, 178),
                Size = new Size(150, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = vahe ? Color.Firebrick : Color.DimGray,
                Font = new Font(Font.FontFamily, 9, vahe ? FontStyle.Bold : FontStyle.Regular)
            };

            p.Controls.AddRange(new Control[] { pilt, nimi, hind, laos });

            if (!otsas)
            {
                EventHandler lisa = (s, e) => LisaKorvi(t);
                p.Click += lisa;
                foreach (Control c in p.Controls) c.Click += lisa;   // klikk pildile/tekstile töötab sama moodi
            }
            return p;
        }

        // Laeb pildi kaustast Images ilma faili lukustamata
        private Image LaePilt(string failinimi)
        {
            if (string.IsNullOrEmpty(failinimi)) return null;
            string tee = Path.Combine(AppContext.BaseDirectory, "Images", failinimi);
            if (!File.Exists(tee)) return null;

            try
            {
                using var fs = new FileStream(tee, FileMode.Open, FileAccess.Read);
                using var tmp = Image.FromStream(fs);
                return new Bitmap(tmp);
            }
            catch { return null; }
        }

        // ---------- Ostukorv ----------
        private void LisaKorvi(Toode t)
        {
            var rida = _korv.FirstOrDefault(r => r.ToodeId == t.Id);
            int uusKogus = (rida?.Kogus ?? 0) + 1;

            if (uusKogus > t.Kogus)
            {
                MessageBox.Show($"Laos on ainult {t.Kogus} tk toodet „{t.Toodenimetus}”.", "Laost ei piisa");
                return;
            }

            if (rida == null)
            {
                _korv.Add(new OstukorviRida
                {
                    ToodeId = t.Id,
                    Toode = t.Toodenimetus,
                    Kogus = 1,
                    Hind = Math.Round((decimal)t.Hind, 2),
                    Laos = t.Kogus
                });
            }
            else
            {
                rida.Kogus = uusKogus;
            }
            UuendaSumma();
        }

        private OstukorviRida ValitudRida() => gridKorv.CurrentRow?.DataBoundItem as OstukorviRida;

        private void MuudaKogust(int muutus)
        {
            var rida = ValitudRida();
            if (rida == null) return;

            int uus = rida.Kogus + muutus;
            if (uus <= 0)
            {
                _korv.Remove(rida);
            }
            else if (uus > rida.Laos)
            {
                MessageBox.Show($"Laos on ainult {rida.Laos} tk toodet „{rida.Toode}”.", "Laost ei piisa");
                return;
            }
            else
            {
                rida.Kogus = uus;
            }
            UuendaSumma();
        }

        private void EemaldaRida()
        {
            var rida = ValitudRida();
            if (rida == null) return;
            _korv.Remove(rida);
            UuendaSumma();
        }

        private void UuendaSumma()
        {
            _korv.ResetBindings();   // värskendab tabelit ja Summa veergu
            decimal kokku = _korv.Sum(r => r.Summa);
            decimal kasutatav = KasutatavBoonus(_klient, kokku);
            lblKokku.Text = "Kokku: " + kokku.ToString("0.00") + " €" +
                            (kasutatav > 0 ? "   Boonus: −" + kasutatav.ToString("0.00") + " €" : "");
            lblMaksta.Text = "Maksta: " + (kokku - kasutatav).ToString("0.00") + " €";
        }

        // ---------- Kliendikaart ja boonus ----------
        // Kui märkeruut on sees, kasutatakse boonust nii palju kui klient omab (aga mitte rohkem kui ostusumma)
        private decimal KasutatavBoonus(Klient k, decimal kokku)
        {
            if (k == null || chkBoonus == null || !chkBoonus.Checked) return 0;
            return Math.Min(k.Boonus, kokku);
        }

        private void OtsiKlient()
        {
            string kaart = txtKaart.Text.Trim();
            if (kaart == "") { MessageBox.Show("Sisesta kliendikaardi number."); return; }

            try
            {
                using var db = new ToodedContext();
                var k = db.Kliendid.AsNoTracking().FirstOrDefault(x => x.Kliendikaart == kaart);
                if (k == null)
                {
                    MessageBox.Show("Sellist kliendikaarti ei leitud.");
                    return;
                }
                SeaKlient(k);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Andmebaasi viga: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }

        private void SeaKlient(Klient k)
        {
            _klient = k;
            chkBoonus.Checked = false;
            chkBoonus.Enabled = k != null && k.Boonus > 0;
            if (k == null)
            {
                txtKaart.Clear();
                lblKlient.Text = "Klienti pole valitud";
            }
            else
            {
                lblKlient.Text = $"{k.Nimi}   (boonus: {k.Boonus:0.00} €)";
            }
            UuendaSumma();
        }

        // ---------- Maksmine, laoseis ja PDF ----------
        private void Maksa()
        {
            if (_korv.Count == 0)
            {
                MessageBox.Show("Ostukorv on tühi.");
                return;
            }

            try
            {
                using var db = new ToodedContext();
                using var tx = db.Database.BeginTransaction();

                // 1) kontrolli värsket laoseisu andmebaasist
                var idd = _korv.Select(r => r.ToodeId).ToList();
                var tooted = db.Toodetabel.Where(t => idd.Contains(t.Id)).ToDictionary(t => t.Id);

                foreach (var r in _korv)
                {
                    if (!tooted.TryGetValue(r.ToodeId, out var t))
                    {
                        MessageBox.Show($"Toodet „{r.Toode}” enam ei ole olemas.");
                        return;
                    }
                    if (r.Kogus > t.Kogus)
                    {
                        MessageBox.Show($"Toodet „{t.Toodenimetus}” on laos ainult {t.Kogus} tk, aga korvis on {r.Kogus} tk.", "Laost ei piisa");
                        LaeTooted();
                        return;
                    }
                }

                // 2) vähenda kogust laos
                foreach (var r in _korv)
                    tooted[r.ToodeId].Kogus -= r.Kogus;

                // 3) boonus: võta klient andmebaasist uuesti (värske boonus), arvuta kasutatud ja uus boonus
                decimal kokku = _korv.Sum(r => r.Summa);
                decimal kasutatud = 0, saadud = 0;
                Klient klient = null;
                if (_klient != null)
                {
                    klient = db.Kliendid.Find(_klient.Id);
                    if (klient == null)
                    {
                        MessageBox.Show("Klienti ei leitud enam andmebaasist.");
                        return;
                    }
                    if (chkBoonus.Checked) kasutatud = Math.Min(klient.Boonus, kokku);
                }
                decimal makstud = kokku - kasutatud;
                if (klient != null)
                {
                    saadud = Math.Round(makstud * BoonusMaar, 2);
                    klient.Boonus = klient.Boonus - kasutatud + saadud;
                }
                db.SaveChanges();

                // 4) tšekk PDF-ina (kui see ebaõnnestub, jääb laoseis muutmata)
                string tee = LooTsekk(klient, kokku, kasutatud, makstud, saadud);

                // 5) müük ajalukku (Müügiajalugu vormi jaoks)
                db.Muugid.Add(new Muuk
                {
                    Kuupaev = DateTime.Now,
                    KasutajaId = AktiivneKasutaja.Id,
                    KlientId = klient?.Id,
                    Kokku = kokku,
                    KasutatudBoonus = kasutatud,
                    Makstud = makstud,
                    Arve = Path.GetFileName(tee)
                });
                db.SaveChanges();
                tx.Commit();
                _viimaneTsekk = tee;

                string teade = "Ost on sooritatud!\nMaksta: " + makstud.ToString("0.00") + " €";
                if (klient != null)
                    teade += $"\nBoonus: kasutatud {kasutatud:0.00} €, saadud {saadud:0.00} €, kokku {klient.Boonus:0.00} €";
                teade += "\nTšekk on salvestatud:\n" + tee;

                _korv.Clear();
                SeaKlient(klient == null ? null : klient);   // näitab uut boonust
                LaeTooted();   // võrk näitab uut laoseisu

                MessageBox.Show(teade, "Valmis");
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(tee) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ostu ei õnnestunud sooritada (laoseisu ei muudetud):\n" + (ex.InnerException?.Message ?? ex.Message));
            }
        }

        // ---------- Tšeki saatmine e-mailile ----------
        private void SaadaEmail()
        {
            if (string.IsNullOrEmpty(_viimaneTsekk) || !File.Exists(_viimaneTsekk))
            {
                MessageBox.Show("Esmalt tee ost, et tekiks tšekk.");
                return;
            }

            string email = Kysi("Saada tšekk", "Sisesta saaja e-mail:", _klient?.Email ?? "");
            if (string.IsNullOrWhiteSpace(email)) return;

            try
            {
                Cursor = Cursors.WaitCursor;
                Epost.Saada(email.Trim(), _viimaneTsekk);
                MessageBox.Show("Tšekk saadetud: " + email.Trim());
            }
            catch (Exception ex)
            {
                MessageBox.Show("E-maili saatmine ebaõnnestus:\n" + (ex.InnerException?.Message ?? ex.Message));
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private string Kysi(string pealkiri, string kysimus, string algne)
        {
            using var f = new Form
            {
                Text = pealkiri,
                ClientSize = new Size(360, 130),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };
            var l = new Label { Text = kysimus, Location = new Point(15, 15), AutoSize = true };
            var tb = new TextBox { Location = new Point(15, 42), Width = 330, Text = algne };
            var ok = new Button { Text = "OK", Location = new Point(180, 85), DialogResult = DialogResult.OK };
            var cancel = new Button { Text = "Tühista", Location = new Point(265, 85), DialogResult = DialogResult.Cancel };
            f.Controls.AddRange(new Control[] { l, tb, ok, cancel });
            f.AcceptButton = ok;
            f.CancelButton = cancel;
            return f.ShowDialog(this) == DialogResult.OK ? tb.Text : null;
        }

        private string LooTsekk(Klient klient, decimal kokku, decimal kasutatud, decimal makstud, decimal saadud)
        {
            string kaust = Path.Combine(AppContext.BaseDirectory, "Arved");
            Directory.CreateDirectory(kaust);

            DateTime aeg = DateTime.Now;
            string tee = Path.Combine(kaust, $"Arve_{aeg:yyyyMMdd_HHmmss}.pdf");

            var doc = new PdfDocument();
            doc.Info.Title = "Arve";
            var page = doc.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            var fPealkiri = new XFont("Arial", 20, XFontStyle.Bold);
            var fNormaalne = new XFont("Arial", 11, XFontStyle.Regular);
            var fPaks = new XFont("Arial", 11, XFontStyle.Bold);
            var fSuur = new XFont("Arial", 14, XFontStyle.Bold);

            double w = page.Width.Point;
            double h = page.Height.Point;
            double y = 40;

            gfx.DrawString("Arve", fPealkiri, XBrushes.Black, new XRect(0, y, w, 30), XStringFormats.TopCenter);
            y += 45;
            gfx.DrawString($"Kuupäev: {aeg:dd.MM.yyyy HH:mm}", fNormaalne, XBrushes.Black, new XRect(40, y, w - 80, 18), XStringFormats.TopLeft);
            y += 20;
            gfx.DrawString($"Müüja: {AktiivneKasutaja.Nimi}", fNormaalne, XBrushes.Black, new XRect(40, y, w - 80, 18), XStringFormats.TopLeft);
            y += 20;
            if (klient != null)
            {
                gfx.DrawString($"Klient: {klient.Nimi}  (kaart {klient.Kliendikaart})", fNormaalne, XBrushes.Black, new XRect(40, y, w - 80, 18), XStringFormats.TopLeft);
                y += 20;
            }
            y += 15;

            // tabeli päis
            gfx.DrawString("Toode", fPaks, XBrushes.Black, new XRect(40, y, 250, 18), XStringFormats.TopLeft);
            gfx.DrawString("Kogus", fPaks, XBrushes.Black, new XRect(300, y, 60, 18), XStringFormats.TopRight);
            gfx.DrawString("Hind", fPaks, XBrushes.Black, new XRect(370, y, 70, 18), XStringFormats.TopRight);
            gfx.DrawString("Summa", fPaks, XBrushes.Black, new XRect(450, y, w - 490, 18), XStringFormats.TopRight);
            y += 20;
            gfx.DrawLine(XPens.Black, 40, y, w - 40, y);
            y += 8;

            foreach (var r in _korv)
            {
                if (y > h - 120)   // uus leht, kui ruum otsa saab
                {
                    gfx.Dispose();
                    page = doc.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = 40;
                }
                gfx.DrawString(r.Toode, fNormaalne, XBrushes.Black, new XRect(40, y, 250, 18), XStringFormats.TopLeft);
                gfx.DrawString(r.Kogus.ToString(), fNormaalne, XBrushes.Black, new XRect(300, y, 60, 18), XStringFormats.TopRight);
                gfx.DrawString(r.Hind.ToString("0.00"), fNormaalne, XBrushes.Black, new XRect(370, y, 70, 18), XStringFormats.TopRight);
                gfx.DrawString(r.Summa.ToString("0.00"), fNormaalne, XBrushes.Black, new XRect(450, y, w - 490, 18), XStringFormats.TopRight);
                y += 22;
            }

            y += 5;
            gfx.DrawLine(XPens.Black, 40, y, w - 40, y);
            y += 12;
            gfx.DrawString("Kokku: " + kokku.ToString("0.00") + " €", fPaks, XBrushes.Black, new XRect(40, y, w - 80, 20), XStringFormats.TopRight);
            y += 22;
            if (klient != null)
            {
                gfx.DrawString("Kasutatud boonus: −" + kasutatud.ToString("0.00") + " €", fNormaalne, XBrushes.Black, new XRect(40, y, w - 80, 20), XStringFormats.TopRight);
                y += 22;
            }
            gfx.DrawString("Maksta: " + makstud.ToString("0.00") + " €", fSuur, XBrushes.Black, new XRect(40, y, w - 80, 24), XStringFormats.TopRight);
            y += 30;
            if (klient != null)
            {
                gfx.DrawString("Saadud boonus: +" + saadud.ToString("0.00") + " €   Boonus kokku: " + klient.Boonus.ToString("0.00") + " €", fNormaalne, XBrushes.Black, new XRect(40, y, w - 80, 20), XStringFormats.TopRight);
            }

            gfx.Dispose();
            doc.Save(tee);
            doc.Close();
            return tee;
        }
    }
}