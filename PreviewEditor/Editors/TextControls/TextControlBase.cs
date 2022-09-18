using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
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

            _editor.ShowLineNumbers = PreviewEditor.Settings.TextEditorOptions.ShowLineNumbers;
            _editor.Options.ShowColumnRuler = PreviewEditor.Settings.TextEditorOptions.ShowColumnRuler;
            _editor.Options.ColumnRulerPosition = PreviewEditor.Settings.TextEditorOptions.ColumnRulerPosition;
            _editor.FontFamily = new FontFamily(PreviewEditor.Settings.TextEditorOptions.FontFamily);
            _editor.FontSize = PreviewEditor.Settings.TextEditorOptions.FontSize;

            //load our custom highlightings
            CustomHighlighting.Load();

            //Apply syntax file based on extension of viewed file
            _editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(_file.FileInfo.Extension);
            //and apply theme given the syntax
            ApplyTheme();

            //SetDarkMode();

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
            _editor.TextArea.SelectionChanged += _editor_SelectionChanged;

            //monitor this event and forward to the subclass
            //which will then call back to this base class if needed
            _editor.KeyDown += GeneralKeyDown;
        }


        /// <summary>
        /// Implement highlight selected text
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _editor_SelectionChanged(object sender, EventArgs e)
        {
            foreach (var xformer in _editor.TextArea.TextView.LineTransformers.OfType<HighlightSelectionTransformer>().ToList())
            {
                _editor.TextArea.TextView.LineTransformers.Remove(xformer);
            }

            _editor.TextArea.TextView.LineTransformers.Add(new HighlightSelectionTransformer(_editor.SelectedText));
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
        public virtual ContextMenuStrip ContextMenu
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
                        Enabled = System.Windows.Clipboard.ContainsText(),
                    },

                    new ToolStripMenuItem("Find", null, (sender, e) =>
                    {
                        Find();
                    }, Keys.Control | Keys.F),

                    new ToolStripMenuItem("Replace", null, (sender, e) =>
                    {
                        Replace();
                    }, Keys.Control | Keys.H),

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
                        new ToolStripMenuItem("Set Column Ruler", null, SetColumnRuler),
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
                            },
                        new ToolStripMenuItem("Font...", null, (sender, e) =>
                            {
                                this.ChooseFont();
                            }),
                        new ToolStripMenuItem("Theme...", null, (sender, e) =>
                            {
                                this.ChooseTheme();
                            })
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


        public void SetColumnRuler(object sender, EventArgs e)
        {
            //var p = Mouse.GetPosition(_editor);
            //var pos = _editor.GetPositionFromPoint(p);
            //_editor.TextArea.Caret.Position = pos.Value;
            var c = _editor.TextArea.Caret.Column;

            _editor.Options.ShowColumnRuler = true;
            _editor.Options.ColumnRulerPosition = c - 1;
            PreviewEditor.Settings.TextEditorOptions.ColumnRulerPosition = _editor.Options.ColumnRulerPosition;
            PreviewEditor.Settings.TextEditorOptions.ShowColumnRuler = _editor.Options.ShowColumnRuler;
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
                else if (e.Key == Key.H)
                {
                    Replace(); e.Handled = true;
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
            else
            {
                //keys that don't matter the shift state
                if (e.Key == Key.Escape)
                {
                    //close the find panel if visible
                    _find?.Close();
                }
            }
        }


        private FindReplacePanel _find;
        private void Find()
        {
            SetupFindPanel();
            _find.Mode = FindReplacePanel.Modes.Find;
        }


        private void Replace()
        {
            SetupFindPanel();
            _find.Mode = FindReplacePanel.Modes.Replace;
        }


        private void SetupFindPanel()
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
            ReplaceNext(_find.FindText, _find.ReplaceText);
        }


        private void OnReplaceAll(object sender, EventArgs e)
        {
            System.Windows.MessageBox.Show("PeformReplaceAll");
        }


        private int _lastUsedIndex = 0;

        public bool FindNext(string search = null, bool searchForward = true)
        {
            Regex regex = _find.RegEx(search, searchForward);
            int start = regex.Options.HasFlag(RegexOptions.RightToLeft) ?
                _editor.SelectionStart :
                _editor.SelectionStart + _editor.SelectionLength;

            Match match = regex.Match(_editor.Text, start);

            if (!match.Success)  // start again from beginning or end
            {
                SystemSounds.Beep.Play();

                if (regex.Options.HasFlag(RegexOptions.RightToLeft))
                    match = regex.Match(_editor.Text, _editor.Text.Length);
                else
                    match = regex.Match(_editor.Text, 0);
            }

            if (match.Success)
            {
                _editor.Select(match.Index, match.Length);
                _lastUsedIndex = match.Index + match.Length;
                _editor.CaretOffset = match.Index + match.Length;
                _editor.TextArea.Caret.BringCaretToView();

                var t = CountOccurances(search);
                _find.UpdateResults(t.Item1, t.Item2);
            }
            else
            {
                _find.UpdateResults(0, 0);
            }

            return match.Success;
        }


        public void FindPrevious(string search = null)
        {
            FindNext(search, false);
        }


        private void ReplaceNext(string search, string replacement, bool selectedonly= false)
        {
            Regex regex = _find.RegEx(search);
            string input = _editor.Text.Substring(_editor.SelectionStart, _editor.SelectionLength);
            Match match = regex.Match(input);
            bool replaced = false;
            if (match.Success && match.Index == 0 && match.Length == input.Length)
            {
                _editor.Document.Replace(_editor.SelectionStart, _editor.SelectionLength, replacement);
                replaced = true;

                var t = CountOccurances(search);
                _find.UpdateResults(t.Item1, t.Item2);
            }
            else
            {
                _find.UpdateResults(0, 0);
            }

            if (!FindNext(search) && !replaced)
            {
                SystemSounds.Beep.Play();
            }
        }


        /// <summary>
        /// Given the current position in document, count 
        /// matches before
        /// </summary>
        /// <returns></returns>
        private Tuple<int, int> CountOccurances(string search)
        {
            var curIndex = _editor.CaretOffset;

            Regex regex = _find.RegEx(search, true);
            var matches = regex.Matches(_editor.Text, 0);

            var c = 1;
            foreach (Match m in matches)
            {
                if (m.Index + m.Length >= curIndex)
                {
                    break;
                }
                c++;
            }
            return Tuple.Create(c, matches.Count);
        }


        private void ChooseFont()
        {
            var dlg = new FontDialog();
            dlg.ShowApply = false;
            dlg.ShowColor = false;
            dlg.ShowEffects = false;
            dlg.Font = new System.Drawing.Font(_editor.FontFamily.Source, (float)_editor.FontSize);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var font = dlg.Font;
                PreviewEditor.Settings.TextEditorOptions.FontFamily = font.Name;
                PreviewEditor.Settings.TextEditorOptions.FontSize = font.Size;

                _editor.FontFamily = new FontFamily(font.Name);
                _editor.FontSize = font.Size;
            }
        }


        private void ChooseTheme()
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK && dlg.FileName != null)
            {
                Theme.Load(dlg.FileName);

                ApplyTheme();
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


        /// <summary>
        /// Assuming a theme has already been loaded as is in Settings
        /// apply those colors to the editor
        /// </summary>
        private void ApplyTheme()
        {
            //can only apply a theme if we have defined highlighting rules
            if (_editor.SyntaxHighlighting == null)
            {
                //best we can do is use the standard windows dark or light mode colors
                var c = this.Parent.BackColor;
                _editor.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B));
                c = this.Parent.ForeColor;
                _editor.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B));

                return;
            }
            else
            {
                var c = PreviewEditor.Settings.TextEditorOptions.Colors;

                //main text colors
                _editor.Background = new SolidColorBrush(c.Backcolor);
                _editor.Foreground = new SolidColorBrush(c.Forecolor);

                //various syntax coloring
                SetElementColor("Comment", c.Comments);
                SetElementColor("ReferenceTypeKeywords", c.Variables);
                SetElementColor("ValueTypeKeywords", c.Variables);
                SetElementColor("NamespaceKeywords", c.Keywords);
                SetElementColor("Keywords", c.Keywords);
                SetElementColor("ContextKeywords", c.Keywords);
                SetElementColor("GotoKeywords", c.GotoKeywords);
                SetElementColor("TypeKeywords", c.Types);
                SetElementColor("String", c.Strings);
                SetElementColor("Punctuation", c.Punctuation);
                SetElementColor("OperatorKeywords", c.Operators);
                SetElementColor("Operators", c.Operators);
                SetElementColor("Visibility", c.Visibility);
                SetElementColor("ParameterModifiers", c.Visibility);
                SetElementColor("MethodCall", c.Functions);
                SetElementColor("NumberLiteral", c.Integers);

                // Set the syntaxHighlighting
                //_editor.SyntaxHighlighting = highlighting;

                //using (var xshd_stream = File.OpenRead(@".\Dark-CSharp.xshd"))
                //{
                //    using (var xshd_reader = new XmlTextReader(xshd_stream))
                //    {
                //        _editor.SyntaxHighlighting = HighlightingLoader.Load(xshd_reader, HighlightingManager.Instance);
                //    }
                //}

            }
        }


        private void SetElementColor(string element, Color color)
        {
            var highlighting = _editor.SyntaxHighlighting;

            var hlt = highlighting.NamedHighlightingColors.Where(c => c.Name == element).FirstOrDefault();
            if (hlt != null)
            {
                hlt.Foreground = new SimpleHighlightingBrush(color);
                hlt.FontWeight = FontWeights.UltraLight;
            }
        }


        private void GetIcon()
        {
            //Free icons https://fontawesome.com/search?m=free
            var img = new FontAwesome.Sharp.IconButton();
            img.IconChar = FontAwesome.Sharp.IconChar.ArrowUp;
            img.IconSize = 16;
            img.Size = new System.Drawing.Size(100, 100);
            img.IconColor = System.Drawing.Color.PeachPuff;

            this.Controls.Add(img);
            img.BringToFront();
            img.Visible = true;
        }
    }
}
