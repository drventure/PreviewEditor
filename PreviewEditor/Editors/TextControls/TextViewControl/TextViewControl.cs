using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using System.Windows.Threading;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;

namespace PreviewEditor.Editors.TextControls
{
    /// <summary>
    /// This control views text files of arbitrary size.
    /// it comes into play when a file is deemed too big for the text viewer to handle it
    /// yet it's still considered a text file (as opposed to a binary file)
    /// </summary>
    internal class TextViewControl : TextControlBase
    {
        public TextViewControl(EditingFile file) : base(file)
        {
        }


        public TextViewControl() : this(null)
        {
        }


        protected override void OnParentChanged()
        {
            //init the control once it's sited
            _editor.IsReadOnly = true;

            try
            {
                //TODO for now, just load 2 megs worth and make it read only
                var fstream = File.Open(_file.FileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var bytes = new byte[2 * 1000 * 1000];
                fstream.Read(bytes, 0, bytes.Length);
                var mstream = new MemoryStream();
                fstream.CopyTo(mstream);
                fstream.Close();
                _editor.Load(mstream);
                mstream.Close();
            }
            catch 
            {
                //TODO update the status label?
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
            //foreach (var h in _editor.SyntaxHighlighting.NamedHighlightingColors)
            //{
            //    var f = h.Foreground;
            //    var b = h.Background;
            //}
        }


        internal override void OnSwitchEditor()
        {
            //we'll just pass the EditingFile on through with no stream
            base.OnSwitchEditor();
        }
    }
}
