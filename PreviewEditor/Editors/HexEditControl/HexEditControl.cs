using ICSharpCode.AvalonEdit;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;

using WpfHexaEditor;


namespace PreviewEditor.Editors
{
    /// <summary>
    /// </summary>
    internal class HexEditControl : PreviewControlBase
    {
        #region private
        private HexEditor _editor;
        #endregion

        #region protected
        protected override string AlternateViewName => "Text";

        #endregion




        public bool IsApplicable
        {
            //hex editor is always applicable
            get => true;
        }


        public HexEditControl()
        {
            this.ParentChanged += OnParentChanged;
        }


        public HexEditControl(EditingFile file) : this()
        {
            _file = file;
        }


        private void OnParentChanged(object sender, EventArgs e)
        {
            if (this.Parent is null) return;

            _host = new ElementHost();
            this.Controls.Add(_host);
            _host.Dock = DockStyle.Fill;

            //init the control once it's sited
            this.TabStop = true;
            this.TabIndex = 0;
            
            //init the control once it's sited
            _editor = new HexEditor();
            _editor.BorderThickness = new Thickness(0);
            _host.Child = _editor;

            _editor.ByteGrouping = WpfHexaEditor.Core.ByteSpacerGroup.FourByte;
            _editor.BytePerLine = 32;
            _editor.ForegroundSecondColor = new SolidColorBrush(Colors.DarkGray);
            _editor.ForegroundOffSetHeaderColor = new SolidColorBrush(Colors.Silver);
            _editor.AllowBuildinCtrla = true;
            _editor.AllowBuildinCtrlc = true;
            _editor.AllowBuildinCtrlz = true;
            _editor.AllowBuildinCtrlv = true;
            _editor.AllowBuildinCtrly = true;
            _editor.AllowExtend = true;
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
                    //read only is always based on whether it's editable or not
                    _editor.ReadOnlyMode = !_file.IsHexEditable;
                    //but in any case, use the stream that was passed in
                    stream = _file.Stream;
                    _editor.Stream = stream;
                }
                else
                {
                    if (_file.IsHexEditable)
                    {
                        _editor.ReadOnlyMode = false;
                        // _editor.Stream = _file.FileInfo.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                        //Load file into memory stream so it's not locked open
                        var mstream = new MemoryStream(File.ReadAllBytes(_file.FileInfo.FullName));
                        _editor.Stream = mstream;
                    }
                    else
                    {
                        _editor.ReadOnlyMode = true;
                        //in read only mode, can't necessarily load the whole file into memory.
                        _editor.Stream = _file.FileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    }
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

            if (isCtrl && !isShift && !isAlt)
            {
                if (e.Key == Key.Home)
                {
                    _editor.SetPosition(0);
                    _editor.Focus();
                }
                else if (e.Key == Key.End) 
                {
                    _editor.SetPosition(_editor.Length);
                    _editor.Focus();
                }
            }
        }


        protected override void Cut() 
        {
            //TODO
        }


        protected override void Copy()
        {
            _editor.CopyToClipboard(WpfHexaEditor.Core.CopyPasteMode.AsciiString);
        }


        protected override void Paste() 
        { 
            //TODO
        }


        protected override void Save(bool prompt = false)
        {
        }


        protected override ContextMenuStrip BuildContextMenu()
        {
            var menu = new ContextMenuStrip();
            menu.Items.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("Undo", null, (sender, e) =>
                {
                    _editor.Undo();
                }, Keys.Control | Keys.Z),

                new ToolStripMenuItem("Options", null, new ToolStripItem[] {
                    new ToolStripMenuItem("Show Status Bar", null, (sender, e) =>
                        {
                            _editor.StatusBarVisibility = _editor.StatusBarVisibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
                            PreviewEditor.Settings.HexEditorOptions.ShowStatusBar = _editor.StatusBarVisibility == Visibility.Visible;
                        }, Keys.Control | Keys.Shift | Keys.L)
                        {
                            Checked = _editor.StatusBarVisibility == Visibility.Visible,
                            Enabled = _file.IsTextLoadable,
                            MergeAction = MergeAction.Append,
                            MergeIndex = 0,
                        },
                    })
                    {
                        MergeAction = MergeAction.MatchOnly,
                    },

                new ToolStripSeparator(),

                new ToolStripMenuItem("Bookmarks...", null,
                    new ToolStripItem[]
                    {
                        new ToolStripMenuItem("Set Bookmark", null, (sender, e) =>
                        {
                            _editor.SetBookMark();
                        })
                        {
                            MergeAction = MergeAction.Insert,
                            MergeIndex = 6,
                        },
                        new ToolStripMenuItem("Clear Bookmark", null, (sender, e) =>
                        {
                            _editor.ClearScrollMarker(_editor.SelectionStart);
                        })
                        {
                            MergeAction = MergeAction.Insert,
                            MergeIndex = 7,
                        },

                    }),
                
                new ToolStripSeparator()                        
                {
                    MergeAction = MergeAction.Insert,
                    MergeIndex = 8,
                },

                //this is just regular copy
                //new ToolStripMenuItem("Copy as ASCII", null, (sender, e) =>
                //{
                //    _editor.CopyToClipboard(WpfHexaEditor.Core.CopyPasteMode.AsciiString);
                //}),

                new ToolStripMenuItem("Copy as Hexadecimal", null, (sender, e) =>
                    {
                        _editor.CopyToClipboard(WpfHexaEditor.Core.CopyPasteMode.HexaString);
                    }) 
                    {
                        MergeAction = MergeAction.Insert,
                        MergeIndex = 2,
                    },

                new ToolStripMenuItem("Copy Selection for...", null,
                    new ToolStripItem[] {
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
                    })
                    {
                        MergeAction = MergeAction.Insert,
                        MergeIndex = 3,
                    },

                new ToolStripSeparator()
                    {
                        MergeAction = MergeAction.Insert,
                        MergeIndex = 11,
                    },

                new ToolStripMenuItem("Paste (overwrite)", null, (sender, e) =>
                    {
                        //TODO
                    }, Keys.Control | Keys.V)
                    {
                        MergeAction = MergeAction.Insert,
                        MergeIndex = 5,
                    },

                new ToolStripMenuItem("Delete", null, (sender, e) =>
                    {
                        _editor.DeleteSelection();
                    }, Keys.Delete)
                    {
                        MergeAction = MergeAction.Insert,
                        MergeIndex = 6,
                    },

                new ToolStripSeparator()
                    {
                        MergeAction = MergeAction.Insert,
                        MergeIndex = 7,
                    },

                new ToolStripMenuItem("Fill Selection with Byte", null, (sender, e) =>
                    {
                        //_editor.FillWithByte();
                    })
                    {
                        MergeAction = MergeAction.Insert,
                        MergeIndex = 8,
                    },

                new ToolStripMenuItem("Replace Byte in Selection", null, (sender, e) =>
                    {
                        //_editor.FillWithByte();
                    }, Keys.Control | Keys.H)
                    {
                        MergeAction = MergeAction.Insert,
                        MergeIndex = 9,
                    },

                new ToolStripSeparator()
                    {
                        MergeAction = MergeAction.Insert,
                        MergeIndex = 18,
                    },

                new ToolStripMenuItem("Find All Occurrences of Selection", null, (sender, e) =>
                    {
                        //TODO
                    }, Keys.Control | Keys.F)
                    {
                        MergeAction = MergeAction.Insert,
                        MergeIndex = 19,
                    },

                new ToolStripSeparator()
                    {
                        MergeAction = MergeAction.Insert,
                        MergeIndex = 20,
                    },

                new ToolStripMenuItem("Select All", null, (sender, e) =>
                    {
                        //TODO
                    }, Keys.Control | Keys.A)
                    {
                        MergeAction = MergeAction.Insert,
                        MergeIndex = 21,
                    },
            });

            //merge menu items
            var baseMenu = base.BuildContextMenu();
            ToolStripManager.Merge(menu, baseMenu);

            return baseMenu;
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


        public override void Close()
        {
            if (_editor != null)
            {
                _editor.CloseProvider();
                _host.Child = null;
                _editor = null;
            }
        }


        /// <summary>
        /// Request to switch editors
        /// </summary>
        protected override void OnSwitchEditorRequested()
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
            base.OnSwitchEditorRequested();
        }
    }
}
