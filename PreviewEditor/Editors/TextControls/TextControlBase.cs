using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Search;


namespace PreviewEditor.Editors.TextControls
{
    /// <summary>
    /// Since both the TextEditor and the large text file viewer rely on AvalonEdit
    /// we'll use this base class to abstract out the commonalities between the two
    /// </summary>
    internal class TextControlBase : UserControl, IPreviewEditorControl
    {
        /// <summary>
        /// Represents a request to switch editors
        /// </summary>
        public event SwitchEditorRequestedEventHandler SwitchEditorRequested;

        protected string[] EXTENSIONS = new string[] { ".txt", ".log", ".cs", ".vb", ".csproj", ".vbproj", ".c", ".cpp", ".bat", ".ps", ".h" };

        protected EditingFile _file;
        protected ElementHost _host;
        protected TextEditor _editor;

        protected DispatcherTimer _foldingUpdateTimer;
        protected FoldingManager _foldingManager;
        protected dynamic _foldingStrategy;


        public TextControlBase() : base()
        {
            this.ParentChanged += editor_ParentChanged;
        }


        public TextControlBase(EditingFile file) : this()
        {
            _file = file;
        }


        protected virtual void OnParentChanged()
        { }


        private void editor_ParentChanged(object sender, EventArgs e)
        {
            if (this.Parent is null) return;
            if (LicenseManager.UsageMode != LicenseUsageMode.Runtime) return;

            _host = new ElementHost();
            this.Controls.Add(_host);
            _host.Dock = DockStyle.Fill;

            //init the control once it's sited
            this.TabStop = true;
            this.TabIndex = 0;
            _editor = new TextEditor();
            _editor.Focusable = true;
            _host.Child = _editor;

            var c = this.Parent.BackColor;
            _editor.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B));
            c = this.Parent.ForeColor;
            _editor.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B));
            _editor.ShowLineNumbers = PreviewEditor.Settings.TextEditorOptions.ShowLineNumbers;
            _editor.Options.ShowColumnRuler = PreviewEditor.Settings.TextEditorOptions.ShowColumnRuler;
            _editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(_file.FileInfo.Extension);
            _editor.FontFamily = new System.Windows.Media.FontFamily("Consolas");
            _editor.FontSize = 14;

            SetDarkMode();

            SetFoldingStrategy();

            //once we've initialized, unhook the event
            this.OnParentChanged();

            this.Focus();
            _editor.Focus();

            if (_file.SelectionStart != 0)
            {
                _editor.SelectionStart = _file.SelectionStart >= int.MaxValue ? 0 : (int)_file.SelectionStart;
                _editor.SelectionLength = _file.SelectionLength >= int.MaxValue ? 0 : (int)_file.SelectionLength;
                _editor.TextArea.Caret.Show();
                _editor.CaretOffset = _editor.SelectionStart + _editor.SelectionLength;
                _editor.TextArea.Caret.BringCaretToView();
            }

            //hook up events AFTER the document has been loaded
            _editor.MouseDown += (sender, e) => { updateEditingFile(); };
            _editor.KeyDown += (sender, e) => { updateEditingFile(); };
            _editor.TextChanged += (sender, e) => { updateEditingFile(); _file.SetDirty(); };
            _editor.MouseDown += _editor_MouseDown;
            _editor.TextChanged += _editor_TextChanged;

            //monitor this event and forward to the subclass
            //which will then call back to this base class if needed
            _editor.KeyDown += GeneralKeyDown;
        }


        private void updateEditingFile()
        {
            _file.SelectionStart = _editor.SelectionStart;
            _file.SelectionLength = _editor.SelectionLength;
        }


        private void _editor_TextChanged(object sender, EventArgs e)
        {
        }


        private void _editor_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                this.ContextMenu.Show(this, this.PointToClient(System.Windows.Forms.Cursor.Position));
            }
        }


        /// <summary>
        /// Build up the ContextMenu
        /// </summary>
        public virtual new ContextMenuStrip ContextMenu
        {
            get
            {
                var menu = new ContextMenuStrip();
                menu.Items.AddRange(new ToolStripItem[] {
                    new ToolStripMenuItem("Cut", null, (sender, e) =>
                    {
                        _editor.Cut();
                    }, Keys.Control | Keys.X),

                    new ToolStripMenuItem("Copy", null, (sender, e) =>
                    {
                        _editor.Copy();
                    }, Keys.Control | Keys.C),

                    new ToolStripMenuItem("Paste", null, (sender, e) =>
                    {
                        _editor.Paste();
                    }, Keys.Control | Keys.V)
                    {
                        Enabled = Clipboard.ContainsText(),
                    },

                    new ToolStripMenuItem("Find", null, (sender, e) =>
                    {
                        Find();
                    }, Keys.Control | Keys.F),

                    new ToolStripSeparator(),

                    new ToolStripMenuItem("Options", null, new ToolStripMenuItem[] {
                        new ToolStripMenuItem("Show Line Numbers", null, (sender, e) =>
                            {
                                _editor.ShowLineNumbers = !_editor.ShowLineNumbers;
                                PreviewEditor.Settings.TextEditorOptions.ShowLineNumbers = _editor.ShowLineNumbers;
                            }, Keys.Control | Keys.Shift | Keys.L)
                            {
                                Checked = _editor.ShowLineNumbers
                            },
                        new ToolStripMenuItem("Show Column Ruler", null, (sender, e) =>
                            {
                                _editor.Options.ShowColumnRuler = !_editor.Options.ShowColumnRuler;
                                _editor.Options.ColumnRulerPosition = 80;
                                PreviewEditor.Settings.TextEditorOptions.ShowColumnRuler = _editor.Options.ShowColumnRuler;
                            }, Keys.Control | Keys.Shift | Keys.C)
                            {
                                Checked = _editor.Options.ShowColumnRuler
                            },
                        new ToolStripMenuItem("Show Spaces", null, (sender, e) =>
                            {
                                _editor.Options.ShowSpaces = !_editor.Options.ShowSpaces;
                                PreviewEditor.Settings.TextEditorOptions.ShowSpaces = _editor.Options.ShowSpaces;
                            })
                            {
                                Checked = _editor.Options.ShowSpaces
                            },
                        new ToolStripMenuItem("Show Tabs", null, (sender, e) =>
                            {
                                _editor.Options.ShowTabs = !_editor.Options.ShowTabs;
                                PreviewEditor.Settings.TextEditorOptions.ShowTabs = _editor.Options.ShowTabs;
                            })
                            {
                                Checked = _editor.Options.ShowTabs
                            }
                    }),

                    new ToolStripSeparator(),

                    new ToolStripMenuItem("Save", null, mnuSave, Keys.Control | Keys.S),
                    new ToolStripSeparator(),

                    new ToolStripMenuItem("Show in Hex", null, (sender, e) =>
                    {
                        OnSwitchEditor();
                    })
                });

                return menu;
            }
        }


        private void mnuSave(object sender, EventArgs e)
        {
            try
            {
                //TODO automatically make writable if possible?
                _editor.Save(_file.FileInfo.FullName);
            }
            catch
            {
                //TODO handle exception
            }
        }


        private void mnuSaveAs(object sender, EventArgs e)
        {
            try
            {
                //TODO automatically make writable if possible?
                var dlg = new SaveFileDialog();
                dlg.InitialDirectory = _file.FileInfo.DirectoryName;
                dlg.FileName = _file.FileInfo.Name;
                dlg.OverwritePrompt = true;
                dlg.Title = "Save file as...";
                dlg.CheckPathExists = true;
                dlg.CheckFileExists = false;

                dlg.ShowDialog(this);
                if (dlg.FileName != "")
                {
                    var fs = dlg.OpenFile();
                    _editor.Save(fs);
                    fs.Close();
                }
            }
            catch
            {
                //TODO handle exception
            }
        }


        private void SetFoldingStrategy()
        {
            if (_editor.SyntaxHighlighting == null)
            {
                _foldingStrategy = null;
            }
            else
            {
                switch (_editor.SyntaxHighlighting.Name)
                {
                    case "XML":
                        _foldingStrategy = new XmlFoldingStrategy();
                        _editor.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.DefaultIndentationStrategy();
                        break;
                    case "C#":
                    case "C++":
                    case "PHP":
                    case "Java":
                        _editor.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.CSharp.CSharpIndentationStrategy(_editor.Options);
                        _foldingStrategy = new BraceFoldingStrategy();
                        break;
                    default:
                        _editor.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.DefaultIndentationStrategy();
                        _foldingStrategy = null;
                        break;
                }
            }

            if (_foldingStrategy != null)
            {
                if (_foldingManager == null)
                {
                    _foldingManager = FoldingManager.Install(_editor.TextArea);
                }
                _foldingStrategy?.UpdateFoldings(_foldingManager, _editor.Document);

                //setup up a timer to refold periodically
                _foldingUpdateTimer = new DispatcherTimer();
                _foldingUpdateTimer.Interval = TimeSpan.FromSeconds(2);
                _foldingUpdateTimer.Tick += (sender, e) =>
                {
                    if (_foldingStrategy != null)
                    {
                        _foldingStrategy.UpdateFoldings(_foldingManager, _editor.Document);
                    }
                };
                _foldingUpdateTimer.Start();
            }
            else if (_foldingManager != null)
            {
                FoldingManager.Uninstall(_foldingManager);
                _foldingManager = null;
            }
        }


        private void SetDarkMode()
        {
            _editor.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            _editor.Background = new SolidColorBrush(Color.FromRgb(0x1e, 0x1e, 0x1e));
            //foreach (var h in _editor.SyntaxHighlighting.NamedHighlightingColors)
            //{
            //    var f = h.Foreground;
            //    var b = h.Background;
            //}
        }


        /// <summary>
        /// Handle General Text Control Keys
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void GeneralKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var isCtrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            var isShift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            var isAlt = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);
            if (isCtrl && !isShift && !isAlt)
            {
                if (e.Key == Key.T)
                {
                    OnSwitchEditor();
                }
                else if (e.Key == Key.F)
                {
                    Find(); e.Handled = true;
                }
                else if (e.Key == Key.G)
                {
                    GotoLinePrompt(); e.Handled = true;
                }
            }
            else if (!isCtrl && !isShift && !isAlt)
            {
                if (e.Key == Key.F3)
                {
                    this.FindNext(); e.Handled = true;
                }
            }
            else if (isShift && !isCtrl && !isAlt)
            {
                if (e.Key == Key.F3)
                {
                    this.FindPrevious(); e.Handled = true;
                }
            }
        }


        private FindReplacePanel _find;
        private void Find()
        {
            var SCROLLBARWIDTH = System.Windows.Forms.SystemInformation.VerticalScrollBarWidth;
            if (_find is null)
            {
                _find = new FindReplacePanel();
                this.SizeChanged += (sender, e) =>
                {
                    _find.Left = this.Width - _find.Width - SCROLLBARWIDTH;
                    _find.Top = 0;
                };

                this.Controls.Add(_find);
                _find.Top = 0;
                _find.Left = this.Width - _find.Width - SCROLLBARWIDTH;

                _find.FindNext += OnFindNext;
                _find.FindPrevious += OnFindPrevious;
                _find.ReplaceNext += OnReplaceNext;
                _find.ReplaceAll += OnReplaceAll;
            }

            _lastUsedIndex = _editor.SelectionStart;
            _find.Visible = true;
            _find.BringToFront();

            _find.Focus();
            //var dlg = new FindReplace.FindReplaceDialog(_editor);
            //dlg.ShowDialog();
        }


        private void OnFindNext(object sender, EventArgs e)
        {
            FindNext(_find.FindText);
        }


        private void OnFindPrevious(object sender, EventArgs e)
        {
            FindPrevious(_find.FindText);
        }


        private void OnReplaceNext(object sender, EventArgs e)
        {
            MessageBox.Show("PeformReplaceNext");
        }


        private void OnReplaceAll(object sender, EventArgs e)
        {
            MessageBox.Show("PeformReplaceAll");
        }


        private int _lastUsedIndex = 0;

        public void FindNext(string search = null)
        {
            if (string.IsNullOrEmpty(search))
            {
                //nothing passed in, so use last find
                search = _find?.FindText;
            }

            //if nothing to find, bail
            if (string.IsNullOrEmpty(search))
            {
                return;
            }


            if (string.IsNullOrEmpty(_editor.Text))
            {
                return;
            }

            if (_lastUsedIndex >= _editor.Document.TextLength)
            {
                _lastUsedIndex = 0;
            }

            var wrapped = false;
            do
            {
                int nIndex = _editor.Text.IndexOf(search, _lastUsedIndex);
                if (nIndex != -1)
                {
                    var area = _editor.TextArea;
                    _editor.Select(nIndex, search.Length);
                    _lastUsedIndex = nIndex + search.Length;
                    _editor.CaretOffset = nIndex + search.Length;
                    _editor.TextArea.Caret.BringCaretToView();
                    break;
                }
                else if (wrapped)
                {
                    break;
                }
                else
                {
                    wrapped = true;
                    _lastUsedIndex = 0;
                    SystemSounds.Beep.Play();
                }
            } while (true);
        }


        public void FindPrevious(string search = null)
        {
            if (string.IsNullOrEmpty(search))
            {
                //nothing passed in, so use last find
                search = _find?.FindText;
            }

            //if nothing to find, bail
            if (string.IsNullOrEmpty(search))
            {
                _lastUsedIndex = 0;
                return;
            }


            if (string.IsNullOrEmpty(_editor.Text))
            {
                _lastUsedIndex = 0;
                return;
            }

            if (_lastUsedIndex > 0)
            {
                _lastUsedIndex = _lastUsedIndex - search.Length;
            }

            var wrapped = false;
            do
            {
                int nIndex = _editor.Text.LastIndexOf(search, _lastUsedIndex - 1);
                if (nIndex != -1)
                {
                    var area = _editor.TextArea;
                    _editor.Select(nIndex, search.Length);
                    _lastUsedIndex = nIndex + search.Length;
                    _editor.CaretOffset = nIndex + search.Length;
                    _editor.TextArea.Caret.BringCaretToView();
                    break;
                }
                else if (wrapped)
                {
                    break;
                }
                else
                {
                    wrapped = true;
                    _lastUsedIndex = _editor.Document.TextLength;
                    SystemSounds.Exclamation.Play();
                }
            } while (true);
        }


        private void Replace(string s, string replacement, bool selectedonly)
        {
            int nIndex = -1;
            if (selectedonly)
            {
                nIndex = _editor.Text.IndexOf(s, _editor.SelectionStart, _editor.SelectionLength);
            }
            else
            {
                nIndex = _editor.Text.IndexOf(s);
            }

            if (nIndex != -1)
            {
                _editor.Document.Replace(nIndex, s.Length, replacement);


                _editor.Select(nIndex, replacement.Length);
            }
            else
            {
                _lastUsedIndex = 0;
                MessageBox.Show("End of file");
            }
        }


        private void GotoLinePrompt()
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox("Go to what line number:",
                                   "Goto Line",
                                   "1",
                                   0,
                                   0);
            if (input != null)
            {
                int line = 1;
                if (int.TryParse(input, out line)) GotoLine(line);
            }
        }


        private void GotoLine(int line)
        {
            //verify range
            if (line < 1) line = 1;
            if (line > _editor.Document.LineCount) line = _editor.Document.LineCount;

            _editor.CaretOffset = _editor.Document.GetLineByNumber(line).Offset;
            _editor.TextArea.Caret.BringCaretToView();
        }


        public void Close()
        {
            _editor = null;
            _host.Child = null;
            _host = null;
        }


        internal virtual void OnSwitchEditor()
        {
            SwitchEditorRequested.Invoke(this, new SwitchEditorRequestedEventArgs(_file));
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // TextControlBase
            // 
            this.Name = "TextControlBase";
            this.Size = new System.Drawing.Size(728, 194);
            this.ResumeLayout(false);

        }
    }
}
