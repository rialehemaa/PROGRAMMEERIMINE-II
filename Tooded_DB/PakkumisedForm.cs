using Microsoft.EntityFrameworkCore;

namespace Tooded_DB
{
    // muuda = true: Müüja/Omanik seab soodusi ja komplekte ("Pakkumised (haldus)")
    // muuda = false: Klient näeb ainult kehtivaid pakkumisi ("Nädala pakkumised")
    public class PakkumisedForm : Form
    {
        private readonly bool _muuda;
        private TabControl tabs;
        private DataGridView gridSoodused, gridKomplektid;
        private Label lblValitud;
        private NumericUpDown numSoodus;
        private DateTimePicker dtAlates, dtKuni;
        private int? _valitudToode;

        // komplekti vorm
        private ComboBox cTuup, cToode, cToode2;
        private NumericUpDown nKogus, nTasuta, nProtsent, nHind;
        private DateTimePicker kAlates, kKuni;
        private Control pToode2, pTasuta, pHind, pProtsent;
        private int? _valitudPakkumine;
        private ComboBox cStaatus;

        public PakkumisedForm(bool muuda)
        {
            _muuda = muuda;
            Text = muuda ? "Pakkumised (haldus)" : "Nädala pakkumised";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1150, 760);
            MinimumSize = new Size(900, 600);

            tabs = new TabControl { Dock = DockStyle.Fill };
            var tabS = new TabPage("Soodused");
            var tabK = new TabPage("Komplektid");
            tabs.TabPages.Add(tabS);
            tabs.TabPages.Add(tabK);
            Controls.Add(tabs);

            EhitaSoodused(tabS);
            EhitaKomplektid(tabK);

            Load += (s, e) =>
            {
                if (_muuda && AktiivneKasutaja.OnKlient)
                {
                    MessageBox.Show("Pakkumisi saab seada ainult müüja või omanik.");
                    Close();
                    return;
                }
                LaeSoodused();
                LaeKomplektid();
                if (_muuda) LaeValikud();
            };
            Teema.Rakenda(this);
        }

        private static DataGridView UusGrid() => new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            RowHeadersVisible = false,
            MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White
        };

        private static Control Paar(string silt, Control c)
        {
            var p = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = new Padding(4, 2, 8, 6)
            };
            p.Controls.Add(new Label { Text = silt, AutoSize = true, Margin = new Padding(0, 0, 0, 2) });
            p.Controls.Add(c);
            return p;
        }

        // ---------- Soodused ----------
        private void EhitaSoodused(TabPage tab)
        {
            gridSoodused = UusGrid();
            gridSoodused.CellClick += (s, e) =>
            {
                if (!_muuda || e.RowIndex < 0) return;
                _valitudToode = (int)gridSoodused.Rows[e.RowIndex].Cells["Id"].Value;
                lblValitud.Text = "Valitud: " + gridSoodused.Rows[e.RowIndex].Cells["Toode"].Value;
                try
                {
                    using var db = new ToodedContext();
                    var t = db.Toodetabel.AsNoTracking().FirstOrDefault(x => x.Id == _valitudToode.Value);
                    if (t != null && t.Soodus > 0)
                    {
                        numSoodus.Value = Math.Max(numSoodus.Minimum, Math.Min(numSoodus.Maximum, t.Soodus));
                        dtAlates.Value = t.SoodusAlates ?? DateTime.Today;
                        dtKuni.Value = t.SoodusKuni ?? DateTime.Today.AddDays(7);
                    }
                }
                catch { }
            };
            tab.Controls.Add(gridSoodused);

            if (!_muuda) return;

            var paneel = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(6) };
            lblValitud = new Label { Text = "Vali tabelist toode", AutoSize = true, Margin = new Padding(8, 6, 20, 0) };
            numSoodus = new NumericUpDown { Width = 70, Minimum = 1, Maximum = 90, Value = 30 };
            dtAlates = new DateTimePicker { Width = 130, Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            dtKuni = new DateTimePicker { Width = 130, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(7) };
            var btnSea = new Button { Text = "Sea soodus", Size = new Size(130, 34), Margin = new Padding(8, 22, 4, 0) };
            btnSea.Click += SeaSoodus;
            var btnKustuta = new Button { Text = "Kustuta soodus", Size = new Size(150, 34), Margin = new Padding(4, 22, 4, 0) };
            btnKustuta.Click += KustutaSoodus;
            var btnLopeta = new Button { Text = "Lõpeta kohe", Size = new Size(130, 34), Margin = new Padding(4, 22, 4, 0) };
            btnLopeta.Click += LopetaSoodus;
            paneel.Controls.Add(lblValitud);
            paneel.SetFlowBreak(lblValitud, true);
            paneel.Controls.Add(Paar("Soodus (%)", numSoodus));
            paneel.Controls.Add(Paar("Alates", dtAlates));
            paneel.Controls.Add(Paar("Kuni", dtKuni));
            paneel.Controls.Add(btnSea);
            paneel.Controls.Add(btnLopeta);
            paneel.Controls.Add(btnKustuta);
            tab.Controls.Add(paneel);
        }

        private void LaeSoodused()
        {
            try
            {
                using var db = new ToodedContext();
                var tooted = db.Toodetabel.AsNoTracking().Include(t => t.Kategooria).OrderBy(t => t.Toodenimetus).ToList();
                if (!_muuda) tooted = tooted.Where(Allahindlus.SoodusKehtib).ToList();

                gridSoodused.DataSource = tooted.Select(t =>
                {
                    decimal tava = Math.Round((decimal)t.Hind, 2);
                    string staatus = t.Soodus <= 0 ? "" :
                        Allahindlus.SoodusKehtib(t) ? "Kehtib" :
                        (t.SoodusAlates != null && t.SoodusAlates.Value.Date > DateTime.Today ? "Tulemas" : "Lõppenud");
                    string yh = t.Uhik == "kg" ? " / kg" : "";
                    return new
                    {
                        t.Id,
                        Toode = t.Toodenimetus,
                        Kategooria = t.Kategooria?.Kategooria_nimetus,
                        Tavahind = tava.ToString("0.00") + " €" + yh,
                        Soodus = t.Soodus > 0 ? "−" + t.Soodus + " %" : "",
                        Uushind = t.Soodus > 0 ? Math.Round(tava * (100 - t.Soodus) / 100m, 2).ToString("0.00") + " €" + yh : "",
                        Alates = t.Soodus > 0 ? (t.SoodusAlates?.ToString("dd.MM.yyyy") ?? "kohe") : "",
                        Kuni = t.Soodus > 0 ? (t.SoodusKuni?.ToString("dd.MM.yyyy") ?? "piiramatult") : "",
                        Staatus = staatus
                    };
                }).ToList();
                if (gridSoodused.Columns["Id"] != null) gridSoodused.Columns["Id"].Visible = false;
                if (gridSoodused.Columns["Toode"] != null) gridSoodused.Columns["Toode"].FillWeight = 190;
                if (gridSoodused.Columns["Kategooria"] != null) gridSoodused.Columns["Kategooria"].FillWeight = 170;
                if (gridSoodused.Columns["Uushind"] != null) gridSoodused.Columns["Uushind"].HeaderText = "Uus hind";

                // uus hind hallilt, kui soodus pole (veel) jõus
                foreach (DataGridViewRow r in gridSoodused.Rows)
                {
                    string st = r.Cells["Staatus"].Value?.ToString();
                    if (st == "Tulemas" || st == "Lõppenud")
                    {
                        r.Cells["Uushind"].Style.ForeColor = Color.Gray;
                        r.Cells["Soodus"].Style.ForeColor = Color.Gray;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Andmebaasi viga: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }

        private void SeaSoodus(object sender, EventArgs e)
        {
            if (_valitudToode == null) { MessageBox.Show("Vali tabelist toode."); return; }
            if (dtKuni.Value.Date < dtAlates.Value.Date) { MessageBox.Show("„Kuni” ei tohi olla enne „Alates” kuupäeva."); return; }
            if (dtKuni.Value.Date < DateTime.Today) { MessageBox.Show("Kuupäev ei tohi olla minevikus."); return; }

            try
            {
                using var db = new ToodedContext();
                var t = db.Toodetabel.Find(_valitudToode.Value);
                if (t == null) { MessageBox.Show("Toodet ei leitud."); return; }
                t.Soodus = (int)numSoodus.Value;
                t.SoodusAlates = dtAlates.Value.Date;
                t.SoodusKuni = dtKuni.Value.Date;
                db.SaveChanges();
                LaeSoodused();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Andmebaasi viga: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }

        private void LopetaSoodus(object sender, EventArgs e)
        {
            if (_valitudToode == null) { MessageBox.Show("Vali tabelist toode."); return; }
            try
            {
                using var db = new ToodedContext();
                var t = db.Toodetabel.Find(_valitudToode.Value);
                if (t == null || t.Soodus <= 0) { MessageBox.Show("Sellel tootel pole soodust."); return; }
                t.SoodusKuni = DateTime.Today.AddDays(-1);
                db.SaveChanges();
                LaeSoodused();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Andmebaasi viga: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }

        private void KustutaSoodus(object sender, EventArgs e)
        {
            if (_valitudToode == null) { MessageBox.Show("Vali tabelist toode."); return; }
            try
            {
                using var db = new ToodedContext();
                var t = db.Toodetabel.Find(_valitudToode.Value);
                if (t == null) { MessageBox.Show("Toodet ei leitud."); return; }
                t.Soodus = 0;
                t.SoodusAlates = null;
                t.SoodusKuni = null;
                db.SaveChanges();
                LaeSoodused();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Andmebaasi viga: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }

        // ---------- Komplektid ----------
        private void EhitaKomplektid(TabPage tab)
        {
            gridKomplektid = UusGrid();
            gridKomplektid.CellClick += (s, e) =>
            {
                if (!_muuda || e.RowIndex < 0) return;
                _valitudPakkumine = (int)gridKomplektid.Rows[e.RowIndex].Cells["Id"].Value;
                TaidaVorm(_valitudPakkumine.Value);
            };
            tab.Controls.Add(gridKomplektid);

            if (!_muuda) return;

            var paneel = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(6) };
            cTuup = new ComboBox { Width = 260, DropDownStyle = ComboBoxStyle.DropDownList };
            cTuup.Items.AddRange(new object[] { PakkumiseTuubid.OstaSaaTasuta, PakkumiseTuubid.KomplektHind, PakkumiseTuubid.TeineProtsent });
            cTuup.SelectedIndex = 0;
            cToode = new ComboBox { Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cToode2 = new ComboBox { Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            nKogus = new NumericUpDown { Width = 70, Minimum = 1, Maximum = 100, Value = 2 };
            nTasuta = new NumericUpDown { Width = 70, Minimum = 1, Maximum = 100, Value = 1 };
            nHind = new NumericUpDown { Width = 90, Minimum = 0.01m, Maximum = 10000, DecimalPlaces = 2, Value = 1.00m };
            nProtsent = new NumericUpDown { Width = 70, Minimum = 1, Maximum = 100, Value = 50 };
            kAlates = new DateTimePicker { Width = 130, Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            kKuni = new DateTimePicker { Width = 130, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(7) };

            cStaatus = new ComboBox { Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
            cStaatus.Items.AddRange(new object[] { "Aktiivne", "Lõpetatud" });
            cStaatus.SelectedIndex = 0;
            var btnLisa = new Button { Text = "Lisa uus", Size = new Size(120, 34), Margin = new Padding(8, 22, 4, 0) };
            btnLisa.Click += LisaPakkumine;
            var btnSalvesta = new Button { Text = "Salvesta muudatused", Size = new Size(190, 34), Margin = new Padding(4, 22, 4, 0) };
            btnSalvesta.Click += SalvestaPakkumine;
            var btnKustuta = new Button { Text = "Kustuta valitud", Size = new Size(150, 34), Margin = new Padding(4, 22, 4, 0) };
            btnKustuta.Click += KustutaPakkumine;

            paneel.Controls.Add(Paar("Tüüp", cTuup));
            paneel.Controls.Add(Paar("Toode", cToode));
            paneel.Controls.Add(pToode2 = Paar("Teine toode", cToode2));
            paneel.Controls.Add(Paar("Kogus (N)", nKogus));
            paneel.Controls.Add(pTasuta = Paar("Tasuta (M)", nTasuta));
            paneel.Controls.Add(pHind = Paar("Komplekti hind (€)", nHind));
            paneel.Controls.Add(pProtsent = Paar("Teise toote soodus (%)", nProtsent));
            paneel.Controls.Add(Paar("Alates", kAlates));
            paneel.Controls.Add(Paar("Kuni", kKuni));
            paneel.Controls.Add(Paar("Staatus", cStaatus));
            paneel.Controls.Add(btnLisa);
            paneel.Controls.Add(btnSalvesta);
            paneel.Controls.Add(btnKustuta);
            tab.Controls.Add(paneel);

            cTuup.SelectedIndexChanged += (s, e) => UuendaVorm();
            UuendaVorm();
        }

        // näitab ainult valitud tüübile vajalikke välju
        private void UuendaVorm()
        {
            string t = cTuup.SelectedItem?.ToString();
            pTasuta.Visible = t == PakkumiseTuubid.OstaSaaTasuta;
            pHind.Visible = t == PakkumiseTuubid.KomplektHind;
            pToode2.Visible = t == PakkumiseTuubid.TeineProtsent;
            pProtsent.Visible = t == PakkumiseTuubid.TeineProtsent;
        }

        private void LaeValikud()
        {
            try
            {
                using var db = new ToodedContext();
                var list = db.Toodetabel.AsNoTracking().OrderBy(t => t.Toodenimetus).Select(t => new { t.Id, t.Toodenimetus }).ToList();
                cToode.DataSource = list.ToList();
                cToode.DisplayMember = "Toodenimetus";
                cToode.ValueMember = "Id";
                cToode2.DataSource = list.ToList();
                cToode2.DisplayMember = "Toodenimetus";
                cToode2.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Andmebaasi viga: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }

        private void LaeKomplektid()
        {
            try
            {
                using var db = new ToodedContext();
                var list = db.Pakkumised.AsNoTracking().Include(p => p.Toode).Include(p => p.Toode2)
                    .OrderByDescending(p => p.Kuni).ToList();
                if (!_muuda) list = list.Where(p => p.Kehtib).ToList();

                gridKomplektid.DataSource = list.Select(p => new
                {
                    p.Id,
                    Pakkumine = p.Kirjeldus(),
                    Alates = p.Alates.ToString("dd.MM.yyyy"),
                    Kuni = p.Kuni.ToString("dd.MM.yyyy"),
                    Staatus = p.Kehtib ? "Kehtib" : (p.Kuni.Date < DateTime.Today ? "Lõppenud" : "Tulemas")
                }).ToList();
                if (gridKomplektid.Columns["Id"] != null) gridKomplektid.Columns["Id"].Visible = false;
                if (gridKomplektid.Columns["Pakkumine"] != null) gridKomplektid.Columns["Pakkumine"].FillWeight = 400;
                foreach (var nimi in new[] { "Alates", "Kuni", "Staatus" })
                    if (gridKomplektid.Columns[nimi] != null) gridKomplektid.Columns[nimi].FillWeight = 70;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Andmebaasi viga: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }

        // paneeli väljadest pakkumine (null = vigane sisend, teade juba näidatud)
        private Pakkumine LooVormist(Pakkumine p)
        {
            if (cToode.SelectedValue == null) { MessageBox.Show("Vali toode."); return null; }
            if (kKuni.Value.Date < kAlates.Value.Date) { MessageBox.Show("„Kuni” ei tohi olla enne „Alates” kuupäeva."); return null; }

            string tuup = cTuup.SelectedItem.ToString();
            p.Tuup = tuup;
            p.ToodeId = (int)cToode.SelectedValue;
            p.Kogus = (int)nKogus.Value;
            p.Alates = kAlates.Value.Date;
            p.Kuni = kKuni.Value.Date;
            p.Tasuta = 0; p.KomplektHind = 0; p.Toode2Id = null; p.Protsent = 0;

            if (tuup == PakkumiseTuubid.OstaSaaTasuta) p.Tasuta = (int)nTasuta.Value;
            else if (tuup == PakkumiseTuubid.KomplektHind)
            {
                if (p.Kogus < 2) { MessageBox.Show("Komplektis peab olema vähemalt 2 toodet."); return null; }
                p.KomplektHind = nHind.Value;
            }
            else
            {
                if (cToode2.SelectedValue == null || (int)cToode2.SelectedValue == p.ToodeId)
                { MessageBox.Show("Vali teine toode (see peab olema erinev)."); return null; }
                p.Toode2Id = (int)cToode2.SelectedValue;
                p.Protsent = (int)nProtsent.Value;
                p.Kogus = 1;
            }

            if (cStaatus.SelectedItem?.ToString() == "Lõpetatud" && p.Kuni >= DateTime.Today)
                p.Kuni = DateTime.Today.AddDays(-1);
            else if (p.Kuni < DateTime.Today && cStaatus.SelectedItem?.ToString() == "Aktiivne")
            { MessageBox.Show("Aktiivse pakkumise „Kuni” ei tohi olla minevikus."); return null; }
            return p;
        }

        private void LisaPakkumine(object sender, EventArgs e)
        {
            var p = LooVormist(new Pakkumine());
            if (p == null) return;
            try
            {
                using var db = new ToodedContext();
                db.Pakkumised.Add(p);
                db.SaveChanges();
                LaeKomplektid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Andmebaasi viga: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }

        private void SalvestaPakkumine(object sender, EventArgs e)
        {
            if (_valitudPakkumine == null) { MessageBox.Show("Vali tabelist pakkumine."); return; }
            try
            {
                using var db = new ToodedContext();
                var p = db.Pakkumised.Find(_valitudPakkumine.Value);
                if (p == null) { MessageBox.Show("Pakkumist ei leitud."); return; }
                if (LooVormist(p) == null) return;
                db.SaveChanges();
                LaeKomplektid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Andmebaasi viga: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }

        // valitud pakkumise andmed paneeli väljadele (et saaks muuta)
        private void TaidaVorm(int id)
        {
            try
            {
                using var db = new ToodedContext();
                var p = db.Pakkumised.AsNoTracking().FirstOrDefault(x => x.Id == id);
                if (p == null) return;
                cTuup.SelectedItem = p.Tuup;
                cToode.SelectedValue = p.ToodeId;
                if (p.Toode2Id != null) cToode2.SelectedValue = p.Toode2Id.Value;
                nKogus.Value = Math.Max(nKogus.Minimum, Math.Min(nKogus.Maximum, p.Kogus));
                nTasuta.Value = Math.Max(nTasuta.Minimum, Math.Min(nTasuta.Maximum, Math.Max(1, p.Tasuta)));
                nHind.Value = Math.Max(nHind.Minimum, Math.Min(nHind.Maximum, p.KomplektHind > 0 ? p.KomplektHind : 1));
                nProtsent.Value = Math.Max(nProtsent.Minimum, Math.Min(nProtsent.Maximum, Math.Max(1, p.Protsent)));
                kAlates.Value = p.Alates;
                kKuni.Value = p.Kuni;
                cStaatus.SelectedItem = p.Kuni.Date < DateTime.Today ? "Lõpetatud" : "Aktiivne";
            }
            catch { }
        }

        private void KustutaPakkumine(object sender, EventArgs e)
        {
            if (_valitudPakkumine == null) { MessageBox.Show("Vali tabelist pakkumine."); return; }
            try
            {
                using var db = new ToodedContext();
                var p = db.Pakkumised.Find(_valitudPakkumine.Value);
                if (p != null) { db.Pakkumised.Remove(p); db.SaveChanges(); }
                _valitudPakkumine = null;
                LaeKomplektid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Andmebaasi viga: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }
    }
}
