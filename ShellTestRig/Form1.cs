using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShellTestRig
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //var previewHandler = new CodeEditPreviewHandler.CodeEditPreviewHandler();
            var previewControl = new CodeEditPreviewHandler.CodeEditPreviewHandlerControl();
            Controls.Add(previewControl);
            previewControl.Dock = DockStyle.Fill;

            //switch binary or text modes
            //previewControl.DoPreview(".\\TestSourceFile.cs");
            previewControl.DoPreview(".\\TestBinaryFile.bin");
        }
    }
}
