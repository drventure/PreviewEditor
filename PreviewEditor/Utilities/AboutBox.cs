using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PreviewEditor.Utilities
{
    internal partial class AboutBox : Form
    {
        public AboutBox()
        {
            InitializeComponent();
        }


        private void AboutBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Enter) 
            {
                this.Close();
            }
        }

        private void AboutBox_Load(object sender, EventArgs e)
        {
            //lblVersion.Text = string.Format(lblVersion.Text, System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            var ver = this.GetType().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion.ToString();
            ver = ver.Substring(0, ver.IndexOf("+"));
            lblVersion.Text = string.Format(ver);
        }
    }
}
