using System;
using System.Drawing;
using System.Windows.Forms;


namespace PreviewEditorHost
{
    public class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private Button btnSelect;
        private Panel pnlPreviewHost;
        private Button btnViewLargeFile1;
        private Button btnViewLargeFile2;
        private Button btnViewLargeJSON;
        private Button btnHexFile;
        private Button btnColorPicker;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnSelect = new System.Windows.Forms.Button();
            this.pnlPreviewHost = new System.Windows.Forms.Panel();
            this.btnViewLargeFile1 = new System.Windows.Forms.Button();
            this.btnViewLargeFile2 = new System.Windows.Forms.Button();
            this.btnViewLargeJSON = new System.Windows.Forms.Button();
            this.btnHexFile = new System.Windows.Forms.Button();
            this.btnColorPicker = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnSelect
            // 
            this.btnSelect.Location = new System.Drawing.Point(12, 12);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(172, 48);
            this.btnSelect.TabIndex = 0;
            this.btnSelect.Text = "Select File";
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // pnlPreviewHost
            // 
            this.pnlPreviewHost.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlPreviewHost.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlPreviewHost.Location = new System.Drawing.Point(199, 12);
            this.pnlPreviewHost.Name = "pnlPreviewHost";
            this.pnlPreviewHost.Size = new System.Drawing.Size(1242, 583);
            this.pnlPreviewHost.TabIndex = 1;
            // 
            // btnViewLargeFile1
            // 
            this.btnViewLargeFile1.Location = new System.Drawing.Point(12, 66);
            this.btnViewLargeFile1.Name = "btnViewLargeFile1";
            this.btnViewLargeFile1.Size = new System.Drawing.Size(172, 48);
            this.btnViewLargeFile1.TabIndex = 2;
            this.btnViewLargeFile1.Text = "Large File 1";
            this.btnViewLargeFile1.UseVisualStyleBackColor = true;
            this.btnViewLargeFile1.Click += new System.EventHandler(this.btnViewLargeFile1_Click);
            // 
            // btnViewLargeFile2
            // 
            this.btnViewLargeFile2.Location = new System.Drawing.Point(12, 120);
            this.btnViewLargeFile2.Name = "btnViewLargeFile2";
            this.btnViewLargeFile2.Size = new System.Drawing.Size(172, 48);
            this.btnViewLargeFile2.TabIndex = 3;
            this.btnViewLargeFile2.Text = "Large File 2";
            this.btnViewLargeFile2.UseVisualStyleBackColor = true;
            this.btnViewLargeFile2.Click += new System.EventHandler(this.btnViewLargeFile2_Click);
            // 
            // btnViewLargeJSON
            // 
            this.btnViewLargeJSON.Location = new System.Drawing.Point(12, 174);
            this.btnViewLargeJSON.Name = "btnViewLargeJSON";
            this.btnViewLargeJSON.Size = new System.Drawing.Size(172, 48);
            this.btnViewLargeJSON.TabIndex = 4;
            this.btnViewLargeJSON.Text = "Large JSON";
            this.btnViewLargeJSON.UseVisualStyleBackColor = true;
            this.btnViewLargeJSON.Click += new System.EventHandler(this.btnViewLargeJSON_Click);
            // 
            // btnHexFile
            // 
            this.btnHexFile.Location = new System.Drawing.Point(12, 228);
            this.btnHexFile.Name = "btnHexFile";
            this.btnHexFile.Size = new System.Drawing.Size(172, 48);
            this.btnHexFile.TabIndex = 5;
            this.btnHexFile.Text = "Hex File";
            this.btnHexFile.UseVisualStyleBackColor = true;
            this.btnHexFile.Click += new System.EventHandler(this.btnHexFile_Click);
            // 
            // btnColorPicker
            // 
            this.btnColorPicker.Location = new System.Drawing.Point(12, 547);
            this.btnColorPicker.Name = "btnColorPicker";
            this.btnColorPicker.Size = new System.Drawing.Size(172, 48);
            this.btnColorPicker.TabIndex = 6;
            this.btnColorPicker.Text = "ColorPicker";
            this.btnColorPicker.UseVisualStyleBackColor = true;
            this.btnColorPicker.Click += new System.EventHandler(this.btnColorPicker_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1453, 607);
            this.Controls.Add(this.btnColorPicker);
            this.Controls.Add(this.btnHexFile);
            this.Controls.Add(this.btnViewLargeJSON);
            this.Controls.Add(this.btnViewLargeFile2);
            this.Controls.Add(this.btnViewLargeFile1);
            this.Controls.Add(this.pnlPreviewHost);
            this.Controls.Add(this.btnSelect);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private void MainForm_Load(object sender, EventArgs e)
        {
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            var opn = new OpenFileDialog();
            opn.InitialDirectory = @"C:\Dev\Darin\CodePreviewEditor\PreviewEditor";
            var r = opn.ShowDialog();
            if (r == DialogResult.OK)
            {
                View(opn.FileName);
            }
        }

        private void btnViewLargeFile1_Click(object sender, EventArgs e)
        {
            View("..\\..\\..\\LargeTextFile1.txt");
        }


        private void btnViewLargeFile2_Click(object sender, EventArgs e)
        {
            View("..\\..\\..\\LargeTextFile2.txt");
        }


        private void View(string file)
        {
            var prv = new PreviewEditor.PreviewEditorControl();
            prv.Dock = DockStyle.Fill;

            pnlPreviewHost.Controls.Clear();
            pnlPreviewHost.Controls.Add(prv);
            prv.Visible = true;
            prv.Refresh();

            //in a real host, the PreviewHandlerBase interface called IPreviewHandlerVisuals is called
            //which then forwards the colors on to these methods on the control
            prv.SetBackgroundColor(Color.FromArgb(0x1e, 0x1e, 0x1e));
            prv.SetTextColor(Color.WhiteSmoke);

            prv.DoPreview<string>(file);
        }


        private void btnViewLargeJSON_Click(object sender, EventArgs e)
        {
            View("..\\..\\..\\VeryLargeJSON.json");
        }


        private void btnHexFile_Click(object sender, EventArgs e)
        {
            View("..\\..\\..\\SampleBinaryFile.bin");
        }

        private void btnColorPicker_Click(object sender, EventArgs e)
        {
            var f = new Cyotek.Windows.Forms.ColorPickerDialog();
            if (f.ShowDialog() == DialogResult.OK)
            { 
                MessageBox.Show($"Color is {f.Color}");
            }      
        }
    }
}
