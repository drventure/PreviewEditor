using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Input;
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
            if (this.Parent is null) return;

            //init the control once it's sited
            _editor = new HexEditor();
            _editor.BorderThickness = new Thickness(0);
            this.Child = _editor;

            _editor.ByteGrouping = WpfHexaEditor.Core.ByteSpacerGroup.FourByte;
            _editor.BytePerLine = 32;
            _editor.AllowAutoHighLightSelectionByte = false;
            _editor.AllowContextMenu = false;

            _editor.BytesDeleted += _editor_BytesDeleted;
            _editor.BytesModified += _editor_BytesModified;
            _editor.SelectionLengthChanged += _editor_SelectionLengthChanged;
            _editor.SelectionStartChanged += _editor_SelectionStartChanged;
            _editor.PreviewKeyDown += _editor_KeyDown;
            _editor.MouseDown += _editor_MouseDown;

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
                    //TODO need to make sure file is NOT opened in locked mode
                    //because user may use Explorer to delete/rename etc
                    //and we don't want to block those operations
                    //even if it means loosing edits here
                    _editor.Stream = _file.FileInfo.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                    // _editor.FileName = _file.FileInfo.FullName;
                }
            }
            catch (Exception ex)
            {
                //TODO update the status label?
                System.Windows.Forms.MessageBox.Show($"{ex}");
            }

            //once we've initialized, unhook the event
            this.ParentChanged -= OnParentChanged;

            _editor.Focus();

            if (_file.SelectionStart > 0)
            {
                _editor.SetPosition(_file.SelectionStart, _file.SelectionLength);
            }
        }


        private void _editor_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                e.Handled = true;
                this.ContextMenu.Show(this, this.PointToClient(System.Windows.Forms.Cursor.Position));
            }
        }


        private void _editor_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var isCtrl = (Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) != 0;
            var isShift = (Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Shift) != 0;
            var isAlt = (Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Alt) != 0;
            if (isCtrl && isShift && !isAlt && e.Key == System.Windows.Input.Key.M)
            {
                var mnu = this.ContextMenu;
            }
        }


        public new ContextMenuStrip ContextMenu
        {
            get
            {
                var menu = new ContextMenuStrip();
                menu.Items.AddRange(new ToolStripItem[]
                {
                    new ToolStripMenuItem("Undo", null, (sender, e) =>
                    {
                        _editor.Undo();
                    }, Keys.Control | Keys.Z),
                    new ToolStripSeparator(),
                    new ToolStripMenuItem("Bookmarks...", null,
                        new ToolStripItem[]
                        {
                            new ToolStripMenuItem("Set Bookmark", null, (sender, e) =>
                            {
                                _editor.SetBookMark();
                            }),
                            new ToolStripMenuItem("Clear Bookmark", null, (sender, e) =>
                            {
                                _editor.ClearScrollMarker(_editor.SelectionStart);
                            }),
                        }),
                    new ToolStripSeparator(),
                    new ToolStripMenuItem("Copy as Hexadecimal", null, (sender, e) =>
                    {
                        _editor.CopyToClipboard(WpfHexaEditor.Core.CopyPasteMode.HexaString);
                    }),
                    new ToolStripMenuItem("Copy as ASCII", null, (sender, e) =>
                    {
                        _editor.CopyToClipboard(WpfHexaEditor.Core.CopyPasteMode.AsciiString);
                    }),
                    new ToolStripMenuItem("Copy Selection as...", null,
                        new ToolStripItem[]
                        {
                            new ToolStripMenuItem("C", null, (sender, e) =>
                            {
                                _editor.CopyToClipboard(WpfHexaEditor.Core.CopyPasteMode.CCode);
                            }),
                            new ToolStripMenuItem("C#", null, (sender, e) =>
                            {
                                _editor.CopyToClipboard(WpfHexaEditor.Core.CopyPasteMode.CSharpCode);
                            }),
                            new ToolStripMenuItem("F#", null, (sender, e) =>
                            {
                                _editor.CopyToClipboard(WpfHexaEditor.Core.CopyPasteMode.FSharpCode);
                            }),
                            new ToolStripMenuItem("Java", null, (sender, e) =>
                            {
                                _editor.CopyToClipboard(WpfHexaEditor.Core.CopyPasteMode.JavaCode);
                            }),
                            new ToolStripMenuItem("Visual Basic.Net", null, (sender, e) =>
                            {
                                _editor.CopyToClipboard(WpfHexaEditor.Core.CopyPasteMode.VbNetCode);
                            }),
                            new ToolStripMenuItem("TBL string", null, (sender, e) =>
                            {
                                _editor.CopyToClipboard(WpfHexaEditor.Core.CopyPasteMode.TblString);
                            }),
                        }),
                    new ToolStripSeparator(),
                    new ToolStripMenuItem("Paste (overwrite)", null, (sender, e) =>
                    {
                        //TODO
                    }, Keys.Control | Keys.V),
                    new ToolStripSeparator(),
                    new ToolStripMenuItem("Fill Selection with Byte", null, (sender, e) =>
                    {
                        //_editor.FillWithByte();
                    }),
                    new ToolStripMenuItem("Replace Byte in Selection", null, (sender, e) =>
                    {
                        //_editor.FillWithByte();
                    }, Keys.Control | Keys.H),
                    new ToolStripSeparator(),
                    new ToolStripMenuItem("Delete", null, (sender, e) =>
                    {
                        _editor.DeleteSelection();
                    }, Keys.Delete),
                    new ToolStripSeparator(),
                    new ToolStripMenuItem("Find All Occurrences of Selection", null, (sender, e) =>
                    {
                        //TODO
                    }, Keys.Control | Keys.F),
                    new ToolStripSeparator(),
                    new ToolStripMenuItem("Select All", null, (sender, e) =>
                    {
                        //TODO
                    }, Keys.Control | Keys.A),
                    new ToolStripSeparator(),
                    new ToolStripMenuItem("Switch to Text Editor", null, (sender, e) =>
                    {
                        OnSwitchEditor();
                    }, Keys.Control | Keys.T),
                });

                return menu;
            }
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


        /// <summary>
        /// Request to switch editors
        /// </summary>
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
