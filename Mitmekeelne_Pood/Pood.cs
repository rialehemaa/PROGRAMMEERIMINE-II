using System.ComponentModel;
using System.Globalization;

namespace Pood_andmebaasiga;

public partial class Pood : Form
{
    bool _keelLaetud = false;

    public Pood()
    {
        InitializeComponent();
    }

    private void Pood_Load(object sender, EventArgs e)
    {
        _keelLaetud = false;

        comboLanguage.Items.Clear();
        comboLanguage.Items.Add("Eesti");
        comboLanguage.Items.Add("English");

        string lang = Properties.Settings.Default.UserLanguage;

        if (lang == "en-US")
            comboLanguage.SelectedItem = "English";
        else
            comboLanguage.SelectedItem = "Eesti";

        _keelLaetud = true;
    }

    private void ApplyResourcesToControl(Control ctrl, ComponentResourceManager res)
    {
        res.ApplyResources(ctrl, ctrl.Name);

        foreach (Control child in ctrl.Controls)
        {
            ApplyResourcesToControl(child, res);
        }
    }

    private void ChangeLanguage(string lang)
    {
        // Salvesta valik
        Properties.Settings.Default.UserLanguage = lang;
        Properties.Settings.Default.Save();

        // Muuda kultuur
        Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);
        Thread.CurrentThread.CurrentCulture = new CultureInfo(lang);

        // Rakenda ressursid uuesti
        var res = new ComponentResourceManager(typeof(Pood));
        ApplyResourcesToControl(this, res);
        res.ApplyResources(this, "$this");
    }

    private void comboLanguage_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (!_keelLaetud)
            return;

        if (comboLanguage.SelectedItem.ToString() == "English")
            ChangeLanguage("en-US");
        else
            ChangeLanguage("et-EE");
    }
}
