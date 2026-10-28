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
        [Browsable(false)] public decimal Laos { get; set; }   // palju on laos (kontrolliks)
        [Browsable(false)] public decimal HindSoodus { get; set; }   // ühiku hind pärast toote protsentsoodust
        [Browsable(false)] public string Selgitus { get; set; } = "";   // mis soodus kehtis (tšeki jaoks)
        [Browsable(false)] public bool OnTort { get; set; }              // kuulub tortide kategooriasse
        [Browsable(false)] public string Uhik { get; set; } = "tk";
        [Browsable(false)] public decimal TukiKaal { get; set; }
        [Browsable(false)] public decimal Soodus { get; set; }           // toote/komplekti soodus sellel real (€)
        [Browsable(false)] public decimal SpTort { get; set; }           // sünnipäeva soodus tordile (€)
        [Browsable(false)] public decimal Brutto => Math.Round(Hind * Kogus, 2);
        [Browsable(false)] public decimal Summa => Brutto - Soodus;      // pärast toodete sooduseid
        [Browsable(false)] public string KogusTekst => Uhik == "kg"
            ? (Kogus < 1 ? $"{Kogus * 1000:0} g" : $"{Kogus:0.###} kg")
            : $"{Kogus:0} tk";
        [Browsable(false)] public string HindTekst => Hind.ToString("0.00") + (Uhik == "kg" ? "/kg" : "");

        public string Toode { get; set; }
        public decimal Kogus { get; set; }
        public decimal Hind { get; set; }      // tavahind (tk või kg kohta)
        public string SoodusTekst
        {
            get
            {
                decimal kokku = Soodus + SpTort;
                if (kokku <= 0) return "";
                string tekst = "−" + kokku.ToString("0.00") + " €";
                var osad = new List<string>();
                if (Soodus > 0 && Selgitus != "") osad.Add(Selgitus);
                if (SpTort > 0) osad.Add("sünnipäev −5 %");
                return osad.Count > 0 ? tekst + " (" + string.Join(", ", osad) + ")" : tekst;
            }
        }
        public decimal Rida => Summa - SpTort;   // rea summa kliendile
    }

    // Ostukorvi kokkuvõte (kasutab nii ekraan kui tšekk)
    public class Kokkuvote
    {
        public decimal Brutto;          // tavahindadega
        public decimal ToodeSoodus;     // toodete ja komplektide soodus
        public decimal Vahe;            // pärast toodete sooduseid
        public SynnipaevaTulemus Sp = new SynnipaevaTulemus();
        public decimal Boonus;
        public decimal Maksta;
        public decimal Saast => ToodeSoodus + Sp.Kokku + Boonus;
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
        private ComboBox comboMakse;
        private Klient _klient;               // valitud klient (null = klienti pole)
        private const decimal BoonusMaar = 0.05m;   // 5% ostusummast läheb boonusesse

        private List<Pakkumine> _pakkumised = new List<Pakkumine>();
        private readonly BindingList<OstukorviRida> _korv = new BindingList<OstukorviRida>();

        private readonly bool _veeb;   // true = klient tellib veebipoes

        public KassaForm(bool veeb = false)
        {
            _veeb = veeb;
            Text = veeb ? "Veebipood" : "Kassa";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1150, 680);
            MinimumSize = new Size(1000, 600);

            EhitaUi();
            Teema.Rakenda(this);
            Load += (s, e) =>
            {
                LaeKategooriad();
                if (_veeb)
                {
                    var k = KlientSeos.Leia(this);
                    if (k != null) SeaKlient(k);
                }
                UuendaSumma();
            };
        }

        // ---------- Kasutajaliides ----------
        private void EhitaUi()
        {
            // parem pool: ostukorv
            var paremPaneel = new Panel { Dock = DockStyle.Right, Width = 460, AutoScroll = true };

            var lKorv = new Label { Text = "Ostukorv", Location = new Point(10, 10), AutoSize = true, Font = new Font(Font.FontFamily, 11, FontStyle.Bold) };
            gridKorv = new DataGridView
            {
                Location = new Point(10, 38),
                Size = new Size(430, 290),
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                DataSource = _korv
            };

            gridKorv.DataBindingComplete += (s, e) =>
            {
                if (gridKorv.Columns["SoodusTekst"] != null) gridKorv.Columns["SoodusTekst"].HeaderText = "Soodus";
                if (gridKorv.Columns["Rida"] != null)
                {
                    gridKorv.Columns["Rida"].HeaderText = "Summa";
                    gridKorv.Columns["Rida"].DefaultCellStyle.Format = "0.00";
                }
                if (gridKorv.Columns["Hind"] != null) gridKorv.Columns["Hind"].DefaultCellStyle.Format = "0.00";
                if (gridKorv.Columns["Toode"] != null) gridKorv.Columns["Toode"].FillWeight = 130;
                if (gridKorv.Columns["SoodusTekst"] != null) gridKorv.Columns["SoodusTekst"].FillWeight = 150;
            };
            gridKorv.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.RowIndex >= gridKorv.Rows.Count) return;
                if (gridKorv.Rows[e.RowIndex].DataBoundItem is not OstukorviRida r) return;
                string nimi = gridKorv.Columns[e.ColumnIndex].Name;
                if (nimi == "Kogus") { e.Value = Keel.Uhikud(r.KogusTekst); e.FormattingApplied = true; }
                else if (nimi == "Hind") { e.Value = Keel.Uhikud(r.HindTekst); e.FormattingApplied = true; }
            };

            var btnPlus = Nupp("+", 10, 338, 55, (s, e) => MuudaKogust(1));
            var btnMiinus = Nupp("−", 70, 338, 55, (s, e) => MuudaKogust(-1));
            var btnEemalda = Nupp("Eemalda", 130, 338, 90, (s, e) => EemaldaRida());
            var btnTuhjenda = Nupp("Tühjenda", 225, 338, 100, (s, e) => { _korv.Clear(); UuendaSumma(); });

            // kliendikaart
            var lKaart = new Label { Text = "Kaart / nimi:", Location = new Point(10, 390), AutoSize = true };
            txtKaart = new TextBox { Location = new Point(115, 386), Width = 112 };
            txtKaart.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; OtsiKlient(); } };
            var btnOtsiKlient = Nupp("Otsi", 232, 384, 70, (s, e) => OtsiKlient());
            var btnEemaldaKlient = Nupp("Eemalda", 308, 384, 90, (s, e) => SeaKlient(null));
            lblKlient = new Label { Location = new Point(10, 422), Size = new Size(430, 22), ForeColor = Color.DimGray, Text = "Klienti pole valitud" };
            chkBoonus = new CheckBox { Text = "Kasuta boonust", Location = new Point(10, 448), AutoSize = true, Enabled = false };
            chkBoonus.CheckedChanged += (s, e) => UuendaSumma();

            var lMakse = new Label { Text = "Makseviis:", Location = new Point(10, 482), AutoSize = true };
            comboMakse = new ComboBox { Location = new Point(110, 478), Width = 320, DropDownStyle = ComboBoxStyle.DropDownList };
            if (_veeb) comboMakse.Items.AddRange(new object[] { "Kaardiga", "Arvega" });
            else comboMakse.Items.AddRange(new object[] { "Sularahas", "Kaardiga", "Arvega" });
            comboMakse.SelectedIndex = 0;

            lblKokku = new Label { Location = new Point(10, 514), Size = new Size(430, 50), Font = new Font(Font.FontFamily, 10, FontStyle.Bold) };
            lblMaksta = new Label { Location = new Point(10, 566), Size = new Size(430, 34), Font = new Font(Font.FontFamily, 15, FontStyle.Bold) };

            var btnMaksa = Nupp(_veeb ? "Telli ja maksa" : "Maksa ja loo tšekk (PDF)", 10, 606, 430, (s, e) => Maksa());
            btnMaksa.Height = 50;

            var btnEmail = Nupp("Saada viimane tšekk e-mailile", 10, 664, 430, (s, e) => SaadaEmail());
            btnEmail.Height = 36;

            paremPaneel.Controls.AddRange(new Control[] { btnEmail, lKorv, gridKorv, btnPlus, btnMiinus, btnEemalda, btnTuhjenda,
                lKaart, txtKaart, btnOtsiKlient, btnEemaldaKlient, lblKlient, chkBoonus, lMakse, comboMakse, lblKokku, lblMaksta, btnMaksa });

            // ülemine riba: kategooria filter
            var ylaPaneel = new Panel { Dock = DockStyle.Top, Height = 45 };
            var lK = new Label { Text = "Kategooria:", Location = new Point(10, 14), AutoSize = true };
            comboKategooria = new ComboBox { Location = new Point(115, 10), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            var lKasutaja = new Label { Text = _veeb ? $"Klient: {AktiivneKasutaja.Nimi}" : $"Müüja: {AktiivneKasutaja.Nimi}", Location = new Point(365, 14), AutoSize = true };
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
                var list = new List<Kategooria> { new Kategooria { Id = 0, Kategooria_nimetus = Keel.T("Kõik") } };
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
                _pakkumised = db.Pakkumised.AsNoTracking().Include(p => p.Toode).Include(p => p.Toode2).ToList()
                                .Where(p => p.Kehtib).ToList();
                var q = db.Toodetabel.AsNoTracking().Include(t => t.Kategooria).AsQueryable();
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
                Text = Keel.T(t.Toodenimetus),
                Location = new Point(4, 132),
                Size = new Size(150, 22),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(Font.FontFamily, 9.5f, FontStyle.Bold),
                AutoEllipsis = true
            };

            bool soodus = Allahindlus.SoodusKehtib(t);
            var hind = new Label
            {
                Text = Keel.T(soodus
                    ? $"−{t.Soodus}%  {Allahindlus.HindPraegu(t):0.00} €{(t.Uhik == "kg" ? "/kg" : "")}"
                    : Math.Round((decimal)t.Hind, 2).ToString("0.00") + " €" + (t.Uhik == "kg" ? " / kg" : "")),
                Location = new Point(4, 156),
                Size = new Size(150, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = soodus ? Color.Firebrick : SystemColors.ControlText,
                Font = new Font(Font.FontFamily, soodus ? 8.5f : 9f, soodus ? FontStyle.Bold : FontStyle.Regular)
            };

            bool vahe = true;   // ainult "OTSAS" näidatakse punasena
            var laos = new Label
            {
                Text = otsas ? Keel.T("OTSAS") : "",
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
            string tee = Path.Combine(Kaustad.Pildid, failinimi);
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
        // Kaalutavale tootele küsitakse kogus (kg, g või tk), tükitoote puhul lisatakse 1 tk
        private decimal? KysiKogus(Toode t)
        {
            using var f = new Form
            {
                Text = Keel.T("Kogus:") + " " + Keel.T(t.Toodenimetus),
                ClientSize = new Size(380, 210),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };
            decimal hind = Allahindlus.HindPraegu(t);
            var l1 = new Label { Text = $"Hind: {hind:0.00} € / kg", Location = new Point(15, 15), AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold) };
            var tb = new TextBox { Location = new Point(15, 55), Width = 150, Text = "1" };
            var cu = new ComboBox { Location = new Point(175, 55), Width = 90, DropDownStyle = ComboBoxStyle.DropDownList };
            cu.Items.AddRange(new object[] { "kg", "g" });
            if (t.TukiKaal > 0) cu.Items.Add("tk");
            cu.SelectedIndex = 0;
            var lTulemus = new Label { Location = new Point(15, 100), Size = new Size(350, 50), Font = new Font("Segoe UI", 11, FontStyle.Bold) };
            var ok = new Button { Text = "Lisa", Location = new Point(195, 160), Size = new Size(80, 34), DialogResult = DialogResult.OK };
            var cancel = new Button { Text = "Tühista", Location = new Point(285, 160), Size = new Size(80, 34), DialogResult = DialogResult.Cancel };

            decimal? Kg()
            {
                if (!decimal.TryParse(tb.Text.Trim().Replace(',', '.'), System.Globalization.NumberStyles.Number,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal x) || x <= 0) return null;
                switch (cu.SelectedItem.ToString())
                {
                    case "g": return Math.Round(x / 1000m, 3);
                    case "tk": return Math.Round(x * t.TukiKaal, 3);
                    default: return Math.Round(x, 3);
                }
            }
            void Uuenda()
            {
                var kg = Kg();
                if (kg == null || kg <= 0) { lTulemus.Text = "Sisesta kogus"; ok.Enabled = false; return; }
                lTulemus.Text = $"= {kg:0.###} kg\nHind: {Math.Round(kg.Value * hind, 2):0.00} €";
                ok.Enabled = true;
            }
            tb.TextChanged += (s, e) => Uuenda();
            cu.SelectedIndexChanged += (s, e) => Uuenda();
            Uuenda();

            f.Controls.AddRange(new Control[] { l1, tb, cu, lTulemus, ok, cancel });
            f.AcceptButton = ok;
            f.CancelButton = cancel;
            Teema.Rakenda(f);
            return f.ShowDialog(this) == DialogResult.OK ? Kg() : null;
        }

        private void LisaKorvi(Toode t)
        {
            decimal lisa = 1;
            if (t.Uhik == "kg")
            {
                var k = KysiKogus(t);
                if (k == null) return;
                lisa = k.Value;
            }

            var rida = _korv.FirstOrDefault(r => r.ToodeId == t.Id);
            decimal uusKogus = (rida?.Kogus ?? 0) + lisa;

            if (uusKogus > t.Kogus)
            {
                MessageBox.Show($"Seda toodet „{t.Toodenimetus}” pole piisavalt laos.", "Laost ei piisa");
                return;
            }

            if (rida == null)
            {
                _korv.Add(new OstukorviRida
                {
                    ToodeId = t.Id,
                    Toode = t.Toodenimetus,
                    Kogus = lisa,
                    Uhik = t.Uhik ?? "tk",
                    TukiKaal = t.TukiKaal,
                    Hind = Math.Round((decimal)t.Hind, 2),
                    HindSoodus = Allahindlus.HindPraegu(t),
                    OnTort = t.Kategooria?.Kategooria_nimetus == Allahindlus.TortideKategooria,
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

            decimal samm = rida.Uhik == "kg" ? 0.1m : 1m;
            decimal uus = Math.Round(rida.Kogus + muutus * samm, 3);
            if (uus <= 0)
            {
                _korv.Remove(rida);
            }
            else if (uus > rida.Laos)
            {
                MessageBox.Show($"Toodet „{rida.Toode}” pole piisavalt laos.", "Laost ei piisa");
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

        private Kokkuvote Arvuta(Klient klient, bool kasutaBoonust)
        {
            Allahindlus.Arvuta(_korv, _pakkumised);
            var k = new Kokkuvote();
            k.Brutto = _korv.Sum(r => r.Brutto);
            k.ToodeSoodus = _korv.Sum(r => r.Soodus);
            k.Vahe = k.Brutto - k.ToodeSoodus;
            decimal tort = _korv.Where(r => r.OnTort).Sum(r => r.Summa);
            k.Sp = Allahindlus.Synnipaev(klient, k.Vahe, tort);
            // tordi −5 % kuvatakse tordi enda real (veerg "Soodus")
            foreach (var tr in _korv.Where(x => x.OnTort))
                tr.SpTort = k.Sp.Tort > 0 ? Math.Round(tr.Summa * Allahindlus.SynnipaevaTortMaar, 2) : 0;
            decimal pärast = k.Vahe - k.Sp.Kokku;
            if (klient != null && kasutaBoonust) k.Boonus = Math.Min(klient.Boonus, pärast);
            k.Maksta = pärast - k.Boonus;
            return k;
        }

        private void UuendaSumma()
        {
            var k = Arvuta(_klient, chkBoonus != null && chkBoonus.Checked);
            _korv.ResetBindings();   // värskendab tabelit ja Summa veergu
            string tekst = Keel.T("Kokku: ") + k.Brutto.ToString("0.00") + " €";
            if (k.Saast - k.Boonus > 0) tekst += "\n" + Keel.T("Soodus kokku: ") + "−" + (k.ToodeSoodus + k.Sp.Kokku).ToString("0.00") + " €";
            if (k.Boonus > 0) tekst += "\n" + Keel.T("Boonus: ") + "−" + k.Boonus.ToString("0.00") + " €";
            lblKokku.Text = tekst;
            lblMaksta.Text = Keel.T("Maksta: ") + k.Maksta.ToString("0.00") + " €";
        }

        // ---------- Kliendikaart ja boonus ----------
        // Otsib kliendi kaardi numbri või nime (eesnimi + perekonnanimi) järgi
        private void OtsiKlient()
        {
            string tekst = txtKaart.Text.Trim();
            if (tekst == "") { MessageBox.Show("Sisesta kliendikaardi number või nimi."); return; }

            try
            {
                using var db = new ToodedContext();
                var k = db.Kliendid.AsNoTracking().FirstOrDefault(x => x.Kliendikaart == tekst);
                if (k != null) { SeaKlient(k); return; }

                var sonad = tekst.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var leitud = db.Kliendid.AsNoTracking().ToList()
                    .Where(x => sonad.All(w => (x.Nimi ?? "").IndexOf(w, StringComparison.OrdinalIgnoreCase) >= 0))
                    .OrderBy(x => x.Nimi).ToList();

                if (leitud.Count == 0) { MessageBox.Show("Klienti ei leitud."); return; }
                if (leitud.Count == 1) { SeaKlient(leitud[0]); return; }

                var valitud = ValiKlient(leitud);
                if (valitud != null) SeaKlient(valitud);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Andmebaasi viga: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }

        // Mitu sobivat klienti: kasutaja valib nimekirjast
        private Klient ValiKlient(List<Klient> list)
        {
            using var f = new Form
            {
                Text = "Vali klient",
                ClientSize = new Size(420, 300),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };
            var lb = new ListBox { Location = new Point(12, 12), Size = new Size(396, 230) };
            foreach (var k in list) lb.Items.Add($"{k.Nimi}   ({k.Kliendikaart})");
            lb.SelectedIndex = 0;
            var ok = new Button { Text = "Vali", Location = new Point(238, 252), Size = new Size(80, 34), DialogResult = DialogResult.OK };
            var cancel = new Button { Text = "Tühista", Location = new Point(328, 252), Size = new Size(80, 34), DialogResult = DialogResult.Cancel };
            lb.DoubleClick += (s, e) => { if (lb.SelectedIndex >= 0) f.DialogResult = DialogResult.OK; };
            f.Controls.AddRange(new Control[] { lb, ok, cancel });
            f.AcceptButton = ok;
            f.CancelButton = cancel;
            Teema.Rakenda(f);
            return f.ShowDialog(this) == DialogResult.OK && lb.SelectedIndex >= 0 ? list[lb.SelectedIndex] : null;
        }

        private void SeaKlient(Klient k)
        {
            _klient = k;
            chkBoonus.Checked = false;
            chkBoonus.Enabled = k != null && k.Boonus > 0;
            if (k == null)
            {
                txtKaart.Clear();
                lblKlient.Text = Keel.T("Klienti pole valitud");
            }
            else
            {
                lblKlient.Text = $"{k.Nimi}   (boonus: {k.Boonus:0.00} €)" +
                                 (Allahindlus.OnSynnipaevaPaev(k) ? "   🎂 −15 %" : Allahindlus.OnSynnipaevaAeg(k) ? "   🎂 −10 %" : "");
            }
            UuendaSumma();
        }

        // ---------- Maksmine, laoseis ja PDF ----------
        // summa, mis klient praegu maksma peab (sama arvutus mis UuendaSumma-s)
        private decimal ArvutaMaksta() => Arvuta(_klient, chkBoonus.Checked).Maksta;

        // Sularaha: küsib, kui palju klient andis, ja näitab jooksvalt, kui palju tagasi anda
        private decimal? KysiSularaha(decimal maksta)
        {
            using var f = new Form
            {
                Text = "Sularahas maksmine",
                ClientSize = new Size(360, 230),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };
            var lMaksta = new Label { Text = "Maksta: " + maksta.ToString("0.00") + " €", Location = new Point(15, 15), AutoSize = true, Font = new Font("Segoe UI", 13, FontStyle.Bold) };
            var l2 = new Label { Text = "Klient andis (€):", Location = new Point(15, 60), AutoSize = true };
            var tb = new TextBox { Location = new Point(15, 85), Width = 330, Text = maksta.ToString("0.00") };
            var lTagasi = new Label { Location = new Point(15, 125), Size = new Size(330, 30), Font = new Font("Segoe UI", 13, FontStyle.Bold) };
            var ok = new Button { Text = "OK", Location = new Point(175, 180), Size = new Size(80, 34), DialogResult = DialogResult.OK };
            var cancel = new Button { Text = "Tühista", Location = new Point(265, 180), Size = new Size(80, 34), DialogResult = DialogResult.Cancel };

            bool Loe(out decimal sisestatud) =>
                decimal.TryParse(tb.Text.Trim().Replace(',', '.'), System.Globalization.NumberStyles.Number,
                    System.Globalization.CultureInfo.InvariantCulture, out sisestatud);

            void Uuenda()
            {
                if (Loe(out decimal a) && a >= maksta)
                {
                    lTagasi.Text = "Tagasi anda: " + (a - maksta).ToString("0.00") + " €";
                    lTagasi.ForeColor = Color.SeaGreen;
                    ok.Enabled = true;
                }
                else
                {
                    lTagasi.Text = "Antud summa on liiga väike";
                    lTagasi.ForeColor = Color.Firebrick;
                    ok.Enabled = false;
                }
            }
            tb.TextChanged += (s, e) => Uuenda();
            Uuenda();

            f.Controls.AddRange(new Control[] { lMaksta, l2, tb, lTagasi, ok, cancel });
            f.AcceptButton = ok;
            f.CancelButton = cancel;
            Teema.Rakenda(f);
            if (f.ShowDialog(this) == DialogResult.OK && Loe(out decimal tulemus)) return tulemus;
            return null;
        }

        // Kaardimakse veebipoes (andmeid ei salvestata)
        private bool KysiKaart(decimal maksta)
        {
            using var f = new Form
            {
                Text = "Kaardimakse",
                ClientSize = new Size(360, 230),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };
            var l0 = new Label { Text = "Maksta: " + maksta.ToString("0.00") + " €", Location = new Point(15, 12), AutoSize = true, Font = new Font("Segoe UI", 13, FontStyle.Bold) };
            var l1 = new Label { Text = "Kaardi number:", Location = new Point(15, 52), AutoSize = true };
            var tbNr = new TextBox { Location = new Point(15, 76), Width = 330, MaxLength = 19, PlaceholderText = "0000 0000 0000 0000" };
            var l2 = new Label { Text = "Kehtib (KK/AA):", Location = new Point(15, 112), AutoSize = true };
            var tbKehtib = new TextBox { Location = new Point(15, 136), Width = 120, MaxLength = 5, PlaceholderText = "12/28" };
            var l3 = new Label { Text = "CVC:", Location = new Point(175, 112), AutoSize = true };
            var tbCvc = new TextBox { Location = new Point(175, 136), Width = 80, MaxLength = 3, UseSystemPasswordChar = true };
            var ok = new Button { Text = "Maksa", Location = new Point(175, 180), Size = new Size(80, 34), DialogResult = DialogResult.None };
            var cancel = new Button { Text = "Tühista", Location = new Point(265, 180), Size = new Size(80, 34), DialogResult = DialogResult.Cancel };
            ok.Click += (s, e) =>
            {
                string nr = tbNr.Text.Replace(" ", "");
                if (nr.Length < 12 || !nr.All(char.IsDigit) || tbKehtib.Text.Trim().Length < 4 || tbCvc.Text.Length != 3)
                {
                    MessageBox.Show("Kontrolli kaardi andmeid (number, kehtivus, CVC).");
                    return;
                }
                f.DialogResult = DialogResult.OK;
            };
            f.Controls.AddRange(new Control[] { l0, l1, tbNr, l2, tbKehtib, l3, tbCvc, ok, cancel });
            f.CancelButton = cancel;
            Teema.Rakenda(f);
            return f.ShowDialog(this) == DialogResult.OK;
        }

        private void Maksa()
        {
            if (_korv.Count == 0)
            {
                MessageBox.Show("Ostukorv on tühi.");
                return;
            }

            decimal? andis = null;
            if (_veeb && comboMakse.SelectedItem.ToString() == "Kaardiga" && !KysiKaart(ArvutaMaksta())) return;
            if (comboMakse.SelectedItem.ToString() == "Sularahas")
            {
                andis = KysiSularaha(ArvutaMaksta());
                if (andis == null) return;
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

                // 3) soodused ja boonus: võta klient andmebaasist uuesti (värske boonus)
                Klient klient = null;
                if (_klient != null)
                {
                    klient = db.Kliendid.Find(_klient.Id);
                    if (klient == null)
                    {
                        MessageBox.Show("Klienti ei leitud enam andmebaasist.");
                        return;
                    }
                }
                var kv = Arvuta(klient, chkBoonus.Checked);
                decimal kokku = kv.Vahe, sp = kv.Sp.Kokku, kasutatud = kv.Boonus, saadud = 0;
                decimal makstud = kv.Maksta;
                string makseviis = comboMakse.SelectedItem.ToString();
                if (andis != null && andis < makstud)
                {
                    MessageBox.Show("Antud summa on väiksem kui maksta.");
                    return;
                }
                if (klient != null)
                {
                    saadud = Math.Round(makstud * BoonusMaar, 2);
                    klient.Boonus = klient.Boonus - kasutatud + saadud;
                }
                db.SaveChanges();

                // 4) tšekk PDF-ina (kui see ebaõnnestub, jääb laoseis muutmata)
                string tee = LooTsekk(klient, kv, saadud, makseviis, andis);

                // 5) müük ajalukku (Müügiajalugu vormi jaoks)
                db.Muugid.Add(new Muuk
                {
                    Kuupaev = DateTime.Now,
                    KasutajaId = AktiivneKasutaja.Id,
                    KlientId = klient?.Id,
                    Kokku = kokku,
                    KasutatudBoonus = kasutatud,
                    SynnipaevaSoodus = sp,
                    Maksetuup = makseviis,
                    Makstud = makstud,
                    Arve = Path.GetFileName(tee),
                    SaadudBoonus = saadud,
                    Read = _korv.Select(r => new MuugiRida { ToodeId = r.ToodeId, Toode = r.Toode, Kogus = r.Kogus, Summa = r.Rida }).ToList()
                });
                db.SaveChanges();
                tx.Commit();
                _viimaneTsekk = tee;

                string teade = Keel.T("Ost on sooritatud!") + "\n" + Keel.T("Makseviis:") + " " + Keel.T(makseviis) + "\n" + Keel.T("Maksta:") + " " + makstud.ToString("0.00") + " €";
                if (andis != null)
                    teade += $"\n{Keel.T("Klient andis:")} {andis:0.00} €\n{Keel.T("TAGASI ANDA:")} {andis - makstud:0.00} €";
                if (klient != null)
                    teade += $"\n{Keel.T("Boonus:")} {Keel.T("kasutatud")} {kasutatud:0.00} €, {Keel.T("saadud")} {saadud:0.00} €, {Keel.T("kokku")} {klient.Boonus:0.00} €";
                teade += "\n" + Keel.T("Tšekk on salvestatud:") + "\n" + tee;

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

        // Viitenumber (7-3-1 kontrollnumbriga) ja näidis-arvelduskontod Arvega-makseks
        private static string Viitenumber(DateTime aeg)
        {
            string baas = aeg.ToString("yyMMddHHmm");
            int[] kaal = { 7, 3, 1 };
            int summa = 0;
            for (int i = 0; i < baas.Length; i++)
                summa += (baas[baas.Length - 1 - i] - '0') * kaal[i % 3];
            int kontroll = (10 - summa % 10) % 10;
            return baas + kontroll;
        }

        private string LooTsekk(Klient klient, Kokkuvote kv, decimal saadud, string makseviis, decimal? andis)
        {
            string kaust = Kaustad.Arved;
            Directory.CreateDirectory(kaust);

            DateTime aeg = DateTime.Now;
            string tee = Path.Combine(kaust, $"Arve_{aeg:yyyyMMdd_HHmmss}.pdf");
            bool arvega = makseviis.StartsWith("Arvega");

            var doc = new PdfDocument();
            doc.Info.Title = "Arve";
            var page = doc.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            var fPealkiri = new XFont("Arial", 20, XFontStyle.Bold);
            var fNormaalne = new XFont("Arial", 11, XFontStyle.Regular);
            var fVaike = new XFont("Arial", 9, XFontStyle.Regular);
            var fPaks = new XFont("Arial", 11, XFontStyle.Bold);
            var fSuur = new XFont("Arial", 14, XFontStyle.Bold);

            double w = page.Width.Point;
            double h = page.Height.Point;
            double y = 40;

            void Vasak(string t, XFont f, double kõrgus = 20) { gfx.DrawString(t, f, XBrushes.Black, new XRect(40, y, w - 80, 18), XStringFormats.TopLeft); y += kõrgus; }
            void Parem(string t, XFont f, double kõrgus = 22) { gfx.DrawString(t, f, XBrushes.Black, new XRect(40, y, w - 80, 20), XStringFormats.TopRight); y += kõrgus; }

            gfx.DrawString("POOD", fPealkiri, XBrushes.Black, new XRect(0, y, w, 30), XStringFormats.TopCenter);
            y += 28;
            gfx.DrawString(Keel.T("Kassatšekk / Arve"), fNormaalne, XBrushes.Black, new XRect(0, y, w, 18), XStringFormats.TopCenter);
            y += 30;
            Vasak($"{Keel.T("Kuupäev:")} {aeg:dd.MM.yyyy HH:mm}", fNormaalne);
            Vasak(_veeb ? Keel.T("Veebipood") + " (" + Keel.T("tellija") + ": " + AktiivneKasutaja.Nimi + ")" : $"{Keel.T("Müüja:")} {AktiivneKasutaja.Nimi}", fNormaalne);
            if (klient != null) Vasak($"{Keel.T("Klient:")} {klient.Nimi}  ({Keel.T("kaart")} {klient.Kliendikaart})", fNormaalne);
            Vasak($"{Keel.T("Makseviis:")} {Keel.T(makseviis)}", fNormaalne);
            y += 15;

            // tabeli päis
            gfx.DrawString(Keel.T("Toode"), fPaks, XBrushes.Black, new XRect(40, y, 250, 18), XStringFormats.TopLeft);
            gfx.DrawString(Keel.T("Kogus"), fPaks, XBrushes.Black, new XRect(300, y, 60, 18), XStringFormats.TopRight);
            gfx.DrawString(Keel.T("Hind"), fPaks, XBrushes.Black, new XRect(370, y, 70, 18), XStringFormats.TopRight);
            gfx.DrawString(Keel.T("Summa"), fPaks, XBrushes.Black, new XRect(450, y, w - 490, 18), XStringFormats.TopRight);
            y += 20;
            gfx.DrawLine(XPens.Black, 40, y, w - 40, y);
            y += 8;

            foreach (var r in _korv)
            {
                if (y > h - 160)   // uus leht, kui ruum otsa saab
                {
                    gfx.Dispose();
                    page = doc.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = 40;
                }
                gfx.DrawString(Keel.T(r.Toode), fNormaalne, XBrushes.Black, new XRect(40, y, 250, 18), XStringFormats.TopLeft);
                gfx.DrawString(Keel.Uhikud(r.KogusTekst), fNormaalne, XBrushes.Black, new XRect(300, y, 60, 18), XStringFormats.TopRight);
                gfx.DrawString(Keel.Uhikud(r.HindTekst), fNormaalne, XBrushes.Black, new XRect(370, y, 70, 18), XStringFormats.TopRight);
                gfx.DrawString(r.Brutto.ToString("0.00"), fNormaalne, XBrushes.Black, new XRect(450, y, w - 490, 18), XStringFormats.TopRight);
                y += 20;
                if (r.Soodus > 0)
                {
                    gfx.DrawString($"   {Keel.T(r.Toode)} {r.Selgitus}", fVaike, XBrushes.Black, new XRect(40, y, 350, 16), XStringFormats.TopLeft);
                    gfx.DrawString("−" + r.Soodus.ToString("0.00"), fVaike, XBrushes.Black, new XRect(450, y, w - 490, 16), XStringFormats.TopRight);
                    y += 18;
                }
                if (r.SpTort > 0)
                {
                    gfx.DrawString($"   {Keel.T(r.Toode)} {Keel.T("sünnipäev")} −5 %", fVaike, XBrushes.Black, new XRect(40, y, 350, 16), XStringFormats.TopLeft);
                    gfx.DrawString("−" + r.SpTort.ToString("0.00"), fVaike, XBrushes.Black, new XRect(450, y, w - 490, 16), XStringFormats.TopRight);
                    y += 18;
                }
                y += 4;
            }

            y += 5;
            gfx.DrawLine(XPens.Black, 40, y, w - 40, y);
            y += 12;
            Parem(Keel.T("Kokku:") + " " + kv.Brutto.ToString("0.00") + " €", fPaks);
            if (kv.ToodeSoodus > 0) Parem(Keel.T("Pakkumised ja soodused:") + " −" + kv.ToodeSoodus.ToString("0.00") + " €", fNormaalne);
            if (kv.Sp.Uldine > 0) Parem(kv.Sp.Tekst.Split(new[] { " ja ", " и ", " and " }, StringSplitOptions.None)[0] + " " + Keel.T("(kogu ostukorv)") + ": −" + kv.Sp.Uldine.ToString("0.00") + " €", fNormaalne);
            if (klient != null && kv.Boonus > 0) Parem(Keel.T("Kasutatud boonus:") + " −" + kv.Boonus.ToString("0.00") + " €", fNormaalne);
            Parem(arvega ? Keel.T("Tasuda:") + " " + kv.Maksta.ToString("0.00") + " € " + Keel.T("(7 päeva jooksul)") : Keel.T("Maksta:") + " " + kv.Maksta.ToString("0.00") + " €", fSuur, 30);
            if (kv.Saast > 0) Parem(Keel.T("Sääst kokku:") + " " + kv.Saast.ToString("0.00") + " €", fPaks, 26);
            if (andis != null)
            {
                Parem(Keel.T("Sularahas antud:") + " " + andis.Value.ToString("0.00") + " €", fNormaalne);
                Parem(Keel.T("Tagasi:") + " " + (andis.Value - kv.Maksta).ToString("0.00") + " €", fPaks, 28);
            }
            if (klient != null)
                Parem(Keel.T("Saadud boonus:") + " +" + saadud.ToString("0.00") + " €   " + Keel.T("Boonus kokku:") + " " + klient.Boonus.ToString("0.00") + " €", fNormaalne);

            if (arvega)
            {
                y += 10;
                gfx.DrawLine(XPens.Black, 40, y, w - 40, y);
                y += 10;
                Vasak(Keel.T("Arvega maksmine"), fPaks);
                Vasak(Keel.T("Saaja:") + " POOD OÜ", fNormaalne, 18);
                Vasak("Swedbank:  EE12 2200 0000 0123 4567", fNormaalne, 18);
                Vasak("SEB:             EE12 1010 0000 0123 4567", fNormaalne, 18);
                Vasak("LHV:             EE12 7700 0000 0123 4567", fNormaalne, 18);
                Vasak(Keel.T("Viitenumber:") + " " + Viitenumber(aeg), fPaks, 18);
                Vasak(Keel.T("Summa") + ": " + kv.Maksta.ToString("0.00") + " €   " + Keel.T("Tähtaeg:") + " " + aeg.AddDays(7).ToString("dd.MM.yyyy"), fNormaalne, 18);
            }
            y += 20;
            gfx.DrawString(Keel.T("Täname ostu eest!"), fPaks, XBrushes.Black, new XRect(0, y, w, 20), XStringFormats.TopCenter);

            gfx.Dispose();
            doc.Save(tee);
            doc.Close();
            return tee;
        }
    }
}
