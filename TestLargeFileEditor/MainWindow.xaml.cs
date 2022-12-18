using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TestLargeFileEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FileWindow _window = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            var opn = new OpenFileDialog();
            opn.InitialDirectory = @"C:\Dev\Darin\CodePreviewEditor";
            var r = opn.ShowDialog();
            if (r == System.Windows.Forms.DialogResult.OK)
            {
                var file = opn.FileName;

                _window?.Dispose();
                _window = new FileWindow(file, Editor);
            }
        }
    }
}
