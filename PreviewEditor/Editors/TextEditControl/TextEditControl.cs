using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Integration;

using ICSharpCode.AvalonEdit;


namespace PreviewEditor.Editors
{
    internal class TextEditControl : ElementHost, IPreviewEditorControl
    {
        private string[] EXTENSIONS = new string[] { ".txt", ".log", ".cs", ".vb", ".csproj", ".vbproj", ".c", ".cpp", ".bat", ".ps" };

        private FileInfo _fileInfo;
        private TextEditor _editor;

        public TextEditControl()
        {
            this.ParentChanged += OnParentChanged;
        }


        private void OnParentChanged(object sender, EventArgs e)
        {
            //init the control once it's sited
            _editor = new TextEditor();
            this.Child = _editor;

            try
            {
                var buf = File.ReadAllText(_fileInfo.FullName);
                _editor.Text = buf;
            }
            catch (Exception ex)
            {
                //TODO update the status label?
            }

            //once we've initialized, unhook the event
            this.ParentChanged -= OnParentChanged;
        }


        public TextEditControl(FileInfo fileInfo) : this()
        { 
            _fileInfo = fileInfo;
        }


        public bool IsApplicable
        {
            get
            {
                if (EXTENSIONS.Contains(_fileInfo.Extension))
                {
                    //we can be reasonably sure any of these are text files
                    return true;
                }

                //for now just return false otherwise
                return false;
            }
        }


        public void Close()
        {
            if (_editor != null)
            {
                _editor = null;
            }

            this.Child = null;
        }
    }
}
