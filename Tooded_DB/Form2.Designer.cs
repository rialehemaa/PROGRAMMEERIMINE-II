


namespace Tooded_DB
{
    partial class Form2
    {

        private System.Windows.Forms.DataGridView dataGridViewTooted;
        private System.Windows.Forms.TextBox txtNimetus;
        private System.Windows.Forms.TextBox txtKogus;
        private System.Windows.Forms.TextBox txtHind;
        private System.Windows.Forms.ComboBox comboKategooria;
        private System.Windows.Forms.PictureBox picturePilt;
        private System.Windows.Forms.Button btnLisa;
        private System.Windows.Forms.Button btnUuenda;
        private System.Windows.Forms.Button btnKustuta;
        private System.Windows.Forms.Button btnValiPilt;

        private void InitializeComponent()
        {
            dataGridViewTooted = new DataGridView();
            txtNimetus = new TextBox();
            txtKogus = new TextBox();
            txtHind = new TextBox();
            comboKategooria = new ComboBox();
            picturePilt = new PictureBox();
            btnLisa = new Button();
            btnUuenda = new Button();
            btnKustuta = new Button();
            btnValiPilt = new Button();
            button1 = new Button();
            button3 = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridViewTooted).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picturePilt).BeginInit();
            SuspendLayout();
            // 
            // dataGridViewTooted
            // 
            dataGridViewTooted.ColumnHeadersHeight = 29;
            dataGridViewTooted.Location = new Point(12, 12);
            dataGridViewTooted.MultiSelect = false;
            dataGridViewTooted.Name = "dataGridViewTooted";
            dataGridViewTooted.ReadOnly = true;
            dataGridViewTooted.RowHeadersWidth = 51;
            dataGridViewTooted.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewTooted.Size = new Size(555, 328);
            dataGridViewTooted.TabIndex = 0;
            dataGridViewTooted.CellClick += dataGridViewTooted_CellClick;
            dataGridViewTooted.CellContentClick += dataGridViewTooted_CellContentClick;
            // 
            // txtNimetus
            // 
            txtNimetus.Location = new Point(601, 12);
            txtNimetus.Name = "txtNimetus";
            txtNimetus.PlaceholderText = "Toote nimetus";
            txtNimetus.Size = new Size(200, 23);
            txtNimetus.TabIndex = 1;
            // 
            // txtKogus
            // 
            txtKogus.Location = new Point(601, 52);
            txtKogus.Name = "txtKogus";
            txtKogus.PlaceholderText = "Kogus";
            txtKogus.Size = new Size(200, 23);
            txtKogus.TabIndex = 2;
            // 
            // txtHind
            // 
            txtHind.Location = new Point(601, 92);
            txtHind.Name = "txtHind";
            txtHind.PlaceholderText = "Hind €";
            txtHind.Size = new Size(200, 23);
            txtHind.TabIndex = 3;
            // 
            // comboKategooria
            // 
            comboKategooria.Location = new Point(601, 132);
            comboKategooria.Name = "comboKategooria";
            comboKategooria.Size = new Size(200, 23);
            comboKategooria.TabIndex = 4;
            // 
            // picturePilt
            // 
            picturePilt.BorderStyle = BorderStyle.FixedSingle;
            picturePilt.Location = new Point(601, 172);
            picturePilt.Name = "picturePilt";
            picturePilt.Size = new Size(200, 150);
            picturePilt.SizeMode = PictureBoxSizeMode.Zoom;
            picturePilt.TabIndex = 5;
            picturePilt.TabStop = false;
            // 
            // btnLisa
            // 
            btnLisa.Location = new Point(430, 349);
            btnLisa.Name = "btnLisa";
            btnLisa.Size = new Size(118, 36);
            btnLisa.TabIndex = 7;
            btnLisa.Text = "Lisa";
            btnLisa.Click += btnLisa_Click;
            // 
            // btnUuenda
            // 
            btnUuenda.Location = new Point(564, 349);
            btnUuenda.Name = "btnUuenda";
            btnUuenda.Size = new Size(117, 36);
            btnUuenda.TabIndex = 8;
            btnUuenda.Text = "Uuenda";
            btnUuenda.Click += btnUuenda_Click;
            // 
            // btnKustuta
            // 
            btnKustuta.Location = new Point(699, 349);
            btnKustuta.Name = "btnKustuta";
            btnKustuta.Size = new Size(111, 36);
            btnKustuta.TabIndex = 9;
            btnKustuta.Text = "Kustuta";
            btnKustuta.Click += btnKustuta_Click;
            // 
            // btnValiPilt
            // 
            btnValiPilt.Location = new Point(293, 349);
            btnValiPilt.Name = "btnValiPilt";
            btnValiPilt.Size = new Size(120, 36);
            btnValiPilt.TabIndex = 6;
            btnValiPilt.Text = "Otsi fail";
            btnValiPilt.Click += btnValiPilt_Click;
            // 
            // button1
            // 
            button1.Location = new Point(12, 349);
            button1.Name = "button1";
            button1.Size = new Size(123, 36);
            button1.TabIndex = 10;
            button1.Text = "Lisa kategooria";
            // 
            // button3
            // 
            button3.Location = new Point(151, 349);
            button3.Name = "button3";
            button3.Size = new Size(127, 36);
            button3.TabIndex = 12;
            button3.Text = "Kustuta kategooria";
            // 
            // Form2
            // 
            ClientSize = new Size(1202, 520);
            Controls.Add(button3);
            Controls.Add(button1);
            Controls.Add(dataGridViewTooted);
            Controls.Add(txtNimetus);
            Controls.Add(txtKogus);
            Controls.Add(txtHind);
            Controls.Add(comboKategooria);
            Controls.Add(picturePilt);
            Controls.Add(btnValiPilt);
            Controls.Add(btnLisa);
            Controls.Add(btnUuenda);
            Controls.Add(btnKustuta);
            Name = "Form2";
            Text = "Toodete haldus";
            Load += Form2_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridViewTooted).EndInit();
            ((System.ComponentModel.ISupportInitialize)picturePilt).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private Button button1;
        private Button button3;
    }
}