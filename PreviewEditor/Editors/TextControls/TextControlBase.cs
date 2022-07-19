using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Integration;
using System.Windows.Input;
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
        /// <summary>
        /// size beyond which control becomes read only and this editor won't handle it
        /// </summary>
        protected const int MAXEDITABLESIZE = 2 * 1000 * 1000;

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
            _editor.KeyDown += GeneralKeyDown; ;

            _editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(_fileInfo.Extension);
            _editor.FontFamily = new System.Windows.Media.FontFamily("Consolas");
            _editor.FontSize = 14;
            SetDarkMode();

            SetFoldingStrategy();

            //once we've initialized, unhook the event
            this.ParentChanged -= OnParentChanged;
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


        /// <summary>
        /// check the first 8k for any null chars
        /// If there is one, this is likely a binary file
        /// </summary>
        /// <returns></returns>
        protected bool IsLikelyTextFile()
        {
            StreamReader fileReader = null;
            try
            {
                var l = _fileInfo.Length;
                l = l > 8000 ? 8000 : l;
                if (l == 0) return true;
                char[] chars = new char[l];
                using (fileReader = new StreamReader(new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    var buffer = fileReader.ReadBlock(chars, 0, (int)l);
                }
                for (var i = 0; i < l; i++)
                {
                    if (chars[i] == 0) return false; 
                }
                return true;
            } 
            catch (Exception ex)
            {
                _ = ex;
                return false;
            }
            finally
            {
            }
        }


        public abstract bool IsApplicable { get; }


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
