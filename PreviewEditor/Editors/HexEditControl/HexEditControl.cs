using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.Integration;

using WpfHexaEditor;


namespace PreviewEditor.Editors
{
    internal class HexEditControl : ElementHost, IPreviewEditorControl
    {
        private FileInfo _fileInfo;
        private HexEditor _editor;

        public bool IsApplicable
        {
            //hex editor is always applicable
            get => true;
        }


        public HexEditControl()
        {
            this.ParentChanged += OnParentChanged;
        }


        private void OnParentChanged(object sender, EventArgs e)
        {
            //init the control once it's sited
            _editor = new HexEditor();
            _editor.BorderThickness = new Thickness(0);
            this.Child = _editor;

            try
            {
                var stream = new MemoryStream();
                _fileInfo.OpenRead().CopyTo(stream);
                _editor.Stream = stream;
            }
            catch (Exception ex)
            {
                //TODO update the status label?
            }

            //once we've initialized, unhook the event
            this.ParentChanged -= OnParentChanged;
        }


        public HexEditControl(FileInfo fileInfo) : this()
        {
            _fileInfo = fileInfo;
        }


        public void Close()
        {
            if (_editor != null)
            {
                _editor.CloseProvider();
                this.Child = null;
                _editor = null;
            }
        }
    }
}
