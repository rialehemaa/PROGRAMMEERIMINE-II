namespace Tooded_DB
{
    public class FormMain : Form
    {
        public FormMain()
        {
            Text = "Peamenüü";
            StartPosition = FormStartPosition.CenterScreen;

            string roll = AktiivneKasutaja.Roll;
            bool omanik = roll == Rollid.Omanik;
            bool muuja = roll == Rollid.Muuja;
            bool klient = roll == Rollid.Klient;

            var tere = new Label
            {
                Text = $"Tere, {AktiivneKasutaja.Nimi}!   Roll: {roll}",
                Dock = DockStyle.Fill,
                Padding = new Padding(14, 12, 0, 0)
            };
            var ylaPaneel = new Panel { Dock = DockStyle.Top, Height = 44 };
            var keeleValik = Keel.Valik(this);
            keeleValik.Dock = DockStyle.Top;
            var keelePaneel = new Panel { Dock = DockStyle.Right, Width = 100, Padding = new Padding(4, 9, 4, 0) };
            keelePaneel.Controls.Add(keeleValik);
            ylaPaneel.Controls.Add(tere);
            ylaPaneel.Controls.Add(keelePaneel);
            var btnTeema = new Button { Text = Teema.Tume ? "Hele teema" : "Tume teema", Dock = DockStyle.Fill };
            btnTeema.Click += (s, e) => { Teema.Sea(!Teema.Tume); DialogResult = DialogResult.Yes; };
            var teemaPaneel = new Panel { Dock = DockStyle.Right, Width = 112, Padding = new Padding(4, 9, 4, 9) };   // väiksem nupp
            teemaPaneel.Controls.Add(btnTeema);
            ylaPaneel.Controls.Add(teemaPaneel);

            // Iga roll näeb ainult neid nuppe, millega ta saab töötada (ülejäänud on peidetud)
            var nupud = new List<(string tekst, Action ava)>();
            if (omanik || muuja) nupud.Add(("Kassa", () => { using var f = new KassaForm(); f.ShowDialog(this); }));
            nupud.Add(("Pood", () => { using var f = new Form3(); f.ShowDialog(this); }));                       // toodete vaatamine
            if (omanik) nupud.Add(("Ladu (haldus)", () => { using var f = new Form2(); f.ShowDialog(this); }));   // laoseis, toodete lisamine/muutmine
            if (omanik) nupud.Add(("Kliendid (admin)", () => { using var f = new KliendidForm(); f.ShowDialog(this); }));
            if (muuja) nupud.Add(("Kliendid (admin)", () => { using var f = new KliendidForm(); f.ShowDialog(this); }));
            if (omanik || muuja) nupud.Add(("Müügiajalugu", () => { using var f = new MuugidForm(); f.ShowDialog(this); }));
            if (omanik) nupud.Add(("Statistika", () => { using var f = new StatistikaForm(); f.ShowDialog(this); }));
            if (omanik || klient) nupud.Add(("Veebipood", () => { using var f = new KassaForm(true); f.ShowDialog(this); }));
            if (omanik || muuja) nupud.Add(("Pakkumised (haldus)", () => { using var f = new PakkumisedForm(true); f.ShowDialog(this); }));
            if (omanik || klient) nupud.Add(("Nädala pakkumised", () => { using var f = new PakkumisedForm(false); f.ShowDialog(this); }));
            if (omanik || klient) nupud.Add(("Ostunimekiri", () => { using var f = new OstunimekiriForm(); f.ShowDialog(this); }));
            if (klient) nupud.Add(("Ostuajalugu", () => { using var f = new OstudForm(); f.ShowDialog(this); }));

            var tabel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = nupud.Count + 1,
                Padding = new Padding(14, 4, 14, 14)
            };
            tabel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            foreach (var n in nupud)
            {
                var b = new Button { Text = n.tekst, Dock = DockStyle.Fill, Margin = new Padding(0, 5, 0, 5) };
                var ava = n.ava;
                b.Click += (s, e) => ava();
                tabel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / (nupud.Count + 1)));
                tabel.Controls.Add(b);
            }

            var btnValja = new Button { Text = "Logi välja", Dock = DockStyle.Fill, Margin = new Padding(0, 5, 0, 5) };
            btnValja.Click += (s, e) => { DialogResult = DialogResult.Retry; };
            tabel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / (nupud.Count + 1)));
            tabel.Controls.Add(btnValja);

            Controls.Add(tabel);
            Controls.Add(ylaPaneel);

            ClientSize = new Size(440, 64 + (nupud.Count + 1) * 56);
            MinimumSize = new Size(300, 64 + (nupud.Count + 1) * 44);
            Teema.Rakenda(this);
        }
    }
}