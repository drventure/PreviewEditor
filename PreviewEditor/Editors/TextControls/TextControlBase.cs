using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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


namespace PreviewEditor.Editors
{
    /// <summary>
    /// Since both the TextEditor and the large text file viewer rely on AvalonEdit
    /// we'll use this base class to abstract out the commonalities between the two
    /// </summary>
    internal abstract class TextControlBase : ElementHost, IPreviewEditorControl
    {
        protected string[] EXTENSIONS = new string[] { ".txt", ".log", ".cs", ".vb", ".csproj", ".vbproj", ".c", ".cpp", ".bat", ".ps", ".h" };

        protected EditingFile _file;
        protected TextEditor _editor;

        protected DispatcherTimer _foldingUpdateTimer;
        protected FoldingManager _foldingManager;
        protected dynamic _foldingStrategy;


        public TextControlBase(EditingFile file) : this()
        {
            _file = file;   
        }


        public TextControlBase()
        {
            this.ParentChanged += OnParentChanged;
        }


        private void OnParentChanged(object sender, EventArgs e)
        {
            //init the control once it's sited
            _editor = new TextEditor();
            this.Child = _editor;

            //monitor this event and forward to the subclass
            //which will then call back to this base class if needed
            _editor.KeyDown += GeneralKeyDown;

            _editor.MouseDown += (sender, e) => { updateEditingFile(); };
            _editor.KeyDown += (sender, e) => { updateEditingFile(); };
            _editor.TextChanged += (sender, e) => { updateEditingFile(); _file.SetDirty(); };
            _editor.MouseDown += _editor_MouseDown;
            _editor.TextChanged += _editor_TextChanged;

            _editor.ShowLineNumbers = PreviewEditor.Settings.TextEditorOptions.ShowLineNumbers;
            _editor.Options.ShowColumnRuler = PreviewEditor.Settings.TextEditorOptions.ShowColumnRuler;
            _editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(_file.FileInfo.Extension);
            _editor.FontFamily = new System.Windows.Media.FontFamily("Consolas");
            _editor.FontSize = 14;

            SetDarkMode();

            SetFoldingStrategy();

            //once we've initialized, unhook the event
            this.ParentChanged -= OnParentChanged;
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
        public override ContextMenu ContextMenu
        {
            get
            {
                var menu = new ContextMenu();
                menu.MenuItems.Add(
                    new MenuItem("Cut", (sender, e) =>
                    {
                        _editor.Cut();
                    })
                    { 
                        Shortcut = Shortcut.CtrlX, 
                        MergeOrder = 0 }
                );

                menu.MenuItems.Add(
                    new MenuItem("Copy", (sender, e) =>
                    {
                        _editor.Copy();
                    })
                    { Shortcut = Shortcut.CtrlC, MergeOrder = 0 }
                );

                menu.MenuItems.Add(
                    new MenuItem("Paste", (sender, e) =>
                    {
                        _editor.Paste();
                    })
                    { 
                        Shortcut = Shortcut.CtrlV, 
                        Enabled = Clipboard.ContainsText(),
                        MergeOrder = 0 
                    }
                );

                menu.MenuItems.Add(new MenuItem("-") { MergeOrder = 0 });

                menu.MenuItems.Add(
                    new MenuItem("Show Line Numbers", (sender, e) =>
                        {
                            _editor.ShowLineNumbers = !_editor.ShowLineNumbers;
                            PreviewEditor.Settings.TextEditorOptions.ShowLineNumbers = _editor.ShowLineNumbers;
                        })
                        {
                            Shortcut = Shortcut.CtrlShiftL,
                            MergeOrder = 50,
                            Checked = _editor.ShowLineNumbers
                        }
                );
                menu.MenuItems.Add(
                    new MenuItem("Show Column Ruler", (sender, e) =>
                    {
                        _editor.Options.ShowColumnRuler = !_editor.Options.ShowColumnRuler;
                        _editor.Options.ColumnRulerPosition = 80;
                        PreviewEditor.Settings.TextEditorOptions.ShowColumnRuler = _editor.Options.ShowColumnRuler;
                    })
                    {
                        Shortcut = Shortcut.CtrlShiftC,
                        MergeOrder = 50,
                        Checked = _editor.Options.ShowColumnRuler
                    }
                );
                menu.MenuItems.Add(
                    new MenuItem("Show Spaces", (sender, e) =>
                    {
                        _editor.Options.ShowSpaces = !_editor.Options.ShowSpaces;
                        PreviewEditor.Settings.TextEditorOptions.ShowSpaces = _editor.Options.ShowSpaces;
                    })
                    {
                        MergeOrder = 50,
                        Checked = _editor.Options.ShowSpaces
                    }
                );
                menu.MenuItems.Add(
                    new MenuItem("Show Tabs", (sender, e) =>
                    {
                        _editor.Options.ShowTabs = !_editor.Options.ShowTabs;
                        PreviewEditor.Settings.TextEditorOptions.ShowTabs = _editor.Options.ShowTabs;
                    })
                    {
                        MergeOrder = 50,
                        Checked = _editor.Options.ShowTabs
                    }
                );
                menu.MenuItems.Add(new MenuItem("-") { MergeOrder = 50 });

                menu.MenuItems.Add(new MenuItem("Save", mnuSave) { Shortcut = Shortcut.CtrlS, MergeOrder = 70 });
                menu.MenuItems.Add(new MenuItem("Save As", mnuSaveAs) { Shortcut = Shortcut.CtrlA, MergeOrder = 70 });
                menu.MenuItems.Add(new MenuItem("-") { MergeOrder = 70 });

                menu.MenuItems.Add(
                    new MenuItem("Show in Hex", (sender, e) =>
                    {
                        //TODO
                    })
                    { MergeOrder = 80 }
                );

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
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.Key == Key.T)
                {
                    //ToggleEditor();
                }
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


        public IPreviewEditorControl SwitchToEditor()
        {
            return EditorFactory.GetHexEditor(_file);
        }
    }
}
