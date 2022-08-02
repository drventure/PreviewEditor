using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using WpfHexaEditor;


namespace PreviewEditor.Editors
{
    internal class WPFHexEditControl : ElementHost, IPreviewEditorControl
    {
        /// <summary>
        /// Represents a request to switch editors
        /// </summary>
        public event SwitchEditorRequestedEventHandler SwitchEditorRequested;

        private EditingFile _file;
        private HexEditor _editor;


        public bool IsApplicable
        {
            //hex editor is always applicable
            get => true;
        }


        public WPFHexEditControl()
        {
            this.ParentChanged += OnParentChanged;
        }


        public WPFHexEditControl(EditingFile file) : this()
        {
            _file = file;
        }


        private void OnParentChanged(object sender, EventArgs e)
        {
            //init the control once it's sited
            _editor = new HexEditor();
            _editor.BorderThickness = new Thickness(0);
            this.Child = _editor;

            _editor.ByteGrouping = WpfHexaEditor.Core.ByteSpacerGroup.FourByte;
            _editor.BytePerLine = 32;
            _editor.AllowAutoHighLightSelectionByte = false;

            _editor.BytesDeleted += _editor_BytesDeleted;
            _editor.BytesModified += _editor_BytesModified;
            _editor.SelectionLengthChanged += _editor_SelectionLengthChanged;
            _editor.SelectionStartChanged += _editor_SelectionStartChanged;

            SetDarkMode();

            try
            {
                Stream stream;
                if (_file.Stream is not null)
                {
                    stream = _file.Stream;
                    _editor.Stream = stream;
                }
                else
                {
                    _editor.FileName = _file.FileInfo.FullName;
                }
            }
            catch (Exception ex)
            {
                //TODO update the status label?
                MessageBox.Show($"{ex}");
            }

            //once we've initialized, unhook the event
            this.ParentChanged -= OnParentChanged;
        }


        private void _editor_SelectionStartChanged(object sender, EventArgs e)
        {
            _file.SelectionStart = _editor.SelectionStart;
        }


        private void _editor_SelectionLengthChanged(object sender, EventArgs e)
        {
            _file.SelectionLength = _editor.SelectionLength;
        }


        private void _editor_BytesModified(object sender, WpfHexaEditor.Core.EventArguments.ByteEventArgs e)
        {
            _file.SetDirty();
        }


        private void _editor_BytesDeleted(object sender, EventArgs e)
        {
            _file.SetDirty();
        }


        private void SetDarkMode()
        {
            _editor.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            _editor.Background = new SolidColorBrush(Color.FromRgb(0x1e, 0x1e, 0x1e));
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


        private void OnSwitchEditor()
        {
            if (_editor.FileName == null)
            {
                // We have an editable stream
                _file.Stream = _editor.Stream;
            }
            else if (_file.IsTextEditable)
            {
                // get the editable stream from the hex editor
                _file.Stream = _editor.Stream;
            }
            else
            {
                // just pass the Editing file on through
            }
            SwitchEditorRequested.Invoke(this, new SwitchEditorRequestedEventArgs(_file));
        }
    }
}
