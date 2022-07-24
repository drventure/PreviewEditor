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

        protected FileInfo _fileInfo;
        protected TextEditor _editor;

        protected DispatcherTimer _foldingUpdateTimer;
        protected FoldingManager _foldingManager;
        protected dynamic _foldingStrategy;


        public TextControlBase(FileInfo fileInfo) : this()
        {
            _fileInfo = fileInfo;   
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

            _editor.MouseDown += _editor_MouseDown;

            _editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(_fileInfo.Extension);
            _editor.FontFamily = new System.Windows.Media.FontFamily("Consolas");
            _editor.FontSize = 14;

            SetDarkMode();

            SetFoldingStrategy();

            //once we've initialized, unhook the event
            this.ParentChanged -= OnParentChanged;
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
                );

                menu.MenuItems.Add(
                    new MenuItem("Copy", (sender, e) =>
                    {
                        _editor.Copy();
                    })
                );

                menu.MenuItems.Add(
                    new MenuItem("Paste", (sender, e) =>
                    {
                        _editor.Paste();
                    })
                );

                menu.MenuItems.Add(new MenuItem("Save", mnuSave));
                menu.MenuItems.Add(new MenuItem("Save As", mnuSaveAs));

                menu.MenuItems.Add(
                    new MenuItem("Switch to Hex", (sender, e) =>
                    {
                        //TODO
                    })
                );

                return menu;
            }
        }


        private void mnuSave(object sender, EventArgs e)
        {
                try
                {
                    //TODO automatically make writable if possible?
                    _editor.Save(_fileInfo.FullName);
                }
                catch (Exception ex)
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
                dlg.InitialDirectory = _fileInfo.DirectoryName;
                dlg.FileName = _fileInfo.Name;
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
            catch (Exception ex)
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
            return;
            foreach (var h in _editor.SyntaxHighlighting.NamedHighlightingColors)
            {
                var f = h.Foreground;
                var b = h.Background;
            }
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
    }
}
