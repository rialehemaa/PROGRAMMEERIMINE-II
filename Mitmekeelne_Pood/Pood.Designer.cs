namespace Pood_andmebaasiga;

partial class Pood
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Pood));
        lblNimetus = new Label();
        txtNimetus = new TextBox();
        lblHind = new Label();
        txtHind = new TextBox();
        btnLisa = new Button();
        comboLanguage = new ComboBox();
        SuspendLayout();
        // 
        // lblNimetus
        // 
        resources.ApplyResources(lblNimetus, "lblNimetus");
        lblNimetus.Location = new Point(12, 50);
        lblNimetus.Name = "lblNimetus";
        lblNimetus.Size = new Size(110, 20);
        // 
        // txtNimetus
        // 
        txtNimetus.Location = new Point(130, 47);
        txtNimetus.Name = "txtNimetus";
        txtNimetus.Size = new Size(200, 27);
        // 
        // lblHind
        // 
        resources.ApplyResources(lblHind, "lblHind");
        lblHind.Location = new Point(12, 90);
        lblHind.Name = "lblHind";
        lblHind.Size = new Size(110, 20);
        // 
        // txtHind
        // 
        txtHind.Location = new Point(130, 87);
        txtHind.Name = "txtHind";
        txtHind.Size = new Size(200, 27);
        // 
        // btnLisa
        // 
        resources.ApplyResources(btnLisa, "btnLisa");
        btnLisa.Location = new Point(130, 127);
        btnLisa.Name = "btnLisa";
        btnLisa.Size = new Size(200, 32);
        // 
        // comboLanguage
        // 
        comboLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
        comboLanguage.Location = new Point(230, 10);
        comboLanguage.Name = "comboLanguage";
        comboLanguage.Size = new Size(100, 28);
        comboLanguage.SelectedIndexChanged += comboLanguage_SelectedIndexChanged;
        // 
        // Pood
        // 
        resources.ApplyResources(this, "$this");
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(350, 180);
        Controls.Add(lblNimetus);
        Controls.Add(txtNimetus);
        Controls.Add(lblHind);
        Controls.Add(txtHind);
        Controls.Add(btnLisa);
        Controls.Add(comboLanguage);
        Name = "Pood";
        Load += Pood_Load;
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label lblNimetus;
    private TextBox txtNimetus;
    private Label lblHind;
    private TextBox txtHind;
    private Button btnLisa;
    private ComboBox comboLanguage;
}
