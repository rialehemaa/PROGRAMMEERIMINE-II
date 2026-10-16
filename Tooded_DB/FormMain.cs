namespace Tooded_DB
{
    public class FormMain : Form
    {
        public FormMain()
        {
            Text = "Peamenüü";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            ClientSize = new Size(300, 375);

            var tere = new Label
            {
                Text = $"Tere, {AktiivneKasutaja.Nimi}!   Roll: {AktiivneKasutaja.Roll}",
                Location = new Point(30, 15),
                AutoSize = true
            };

            var btnKassa = new Button { Text = "Kassa", Location = new Point(30, 50), Size = new Size(240, 45) };
            btnKassa.Click += (s, e) =>
            {
                using var f = new KassaForm();
                f.ShowDialog(this);
            };

            var btnTooted = new Button { Text = "Tooted (haldus)", Location = new Point(30, 105), Size = new Size(240, 45) };
            btnTooted.Click += (s, e) =>
            {
                using var f = new Form2();
                f.ShowDialog(this);
            };

            var btnKliendid = new Button { Text = "Kliendid (admin)", Location = new Point(30, 160), Size = new Size(240, 45) };
            btnKliendid.Click += (s, e) =>
            {
                using var f = new KliendidForm();
                f.ShowDialog(this);
            };

            var btnMuugid = new Button { Text = "Müügiajalugu", Location = new Point(30, 215), Size = new Size(240, 45) };
            btnMuugid.Click += (s, e) =>
            {
                using var f = new MuugidForm();
                f.ShowDialog(this);
            };

            var btnValja = new Button { Text = "Logi välja", Location = new Point(30, 310), Size = new Size(240, 40) };
            btnValja.Click += (s, e) => { DialogResult = DialogResult.Retry; };

            // Omanikul on täielik juurdepääs (Tooted + Kassa), müüjale on kättesaadav ainult Kassa
            btnTooted.Enabled = AktiivneKasutaja.OnOmanik;
            btnKliendid.Enabled = AktiivneKasutaja.OnOmanik;
            btnMuugid.Enabled = AktiivneKasutaja.OnOmanik;

            Controls.AddRange(new Control[] { tere, btnKassa, btnTooted, btnKliendid, btnMuugid, btnValja });
        }
    }
}