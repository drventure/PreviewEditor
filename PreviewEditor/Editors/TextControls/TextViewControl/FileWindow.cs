using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;

using PreviewEditor;


namespace PreviewEditor.Editors.TextControls
{
    /// <summary>
    /// Wraps an AvalonEdit control to make it a readonly sliding window "viewer"
    /// that's capable of instantly loading and scrolling through any size huge file
    /// </summary>
    internal class FileWindow : IDisposable
    {
        EditingFile _file;
        VScrollBar _vscroll;
        TextEditor _editor;
        long _windowOffset;
        int _windowSize;

        /// <summary>
        /// The offset into the file of the current caret position
        /// </summary>
        long _caretOffset;

        long _prevCaretOffset;

        /// <summary>
        /// the line number of the current line within the window (top line is 0)
        /// </summary>
        int _activeLine = 0;
        bool _overridingPositionChanged = false;
        private bool _syncingVScroll;

        public FileWindow(EditingFile file, TextEditor editor, VScrollBar vscroll)
        {
            _file = file;
            _windowOffset = -1;     //must be unavailable initially to force load
            _windowSize = 100000;
            _caretOffset = 0;
            _prevCaretOffset = 0;

            //setup to track the editor used
            _editor = editor;
            _editor.Clear();
            _editor.TextArea.Caret.PositionChanged += TextArea_Caret_PositionChanged;
            _editor.PreviewKeyDown += _editor_PreviewKeyDown;
            _editor.TextArea.TextView.VisualLinesChanged += TextView_VisualLinesChanged;

            //hook up the vertical scroll bar
            _vscroll = vscroll;
            //use file length or the max int value
            _vscroll.Maximum = (int)_long.Min(_file.Length, int.MaxValue);
            _vscroll.Scroll += _vscroll_Scroll;

            //forces load of first window
            JumpToHome();
        }


        /// <summary>
        /// Hook to allow us to track the active line number of the caret in the current view
        /// We can then use this to preserve the top line as we transition across windows into the
        /// file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextView_VisualLinesChanged(object sender, EventArgs e)
        {
            var topLine = _editor.TextArea.TextView.GetDocumentLineByVisualTop(_editor.TextArea.TextView.ScrollOffset.Y);
            //var topLine = _editor.TextArea.TextView.VisualLines[0].FirstDocumentLine;
            var curLine = _editor.Document.GetLineByOffset(_editor.CaretOffset);

            _activeLine = curLine.LineNumber - topLine.LineNumber;
        }


        private void _editor_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var isShift = (Keyboard.Modifiers & ModifierKeys.Shift) > 0;
            var isCtrl = (Keyboard.Modifiers & ModifierKeys.Control) > 0;
            var isAlt = (Keyboard.Modifiers & ModifierKeys.Alt) > 0;
            if (e.Key == Key.End && isCtrl && !isShift && !isAlt)
            {
                JumpToEnd();
                e.Handled = true;
            }
            else if (e.Key == Key.Home && isCtrl && !isShift && !isAlt)
            {
                JumpToHome();
                e.Handled = true;
            }
        }


        private DocumentLine CaretLine(long? offset = null)
        {
            var ofs = resolveOffset(offset);
            return _editor.Document.GetLineByOffset((int)(offset - _windowOffset + 1));
        }


        private void TextArea_Caret_PositionChanged(object sender, EventArgs e)
        {
            if (!_overridingPositionChanged)
            {
                //check to see if position has become such that we need to load 
                //a new window
                _caretOffset = _editor.TextArea.Caret.Offset + _windowOffset;
                //System.Diagnostics.Debug.WriteLine("Line=" + CaretLine().LineNumber);

                //TextLocation loc = _editor.Document.GetLocation(_caretOffset);


                if (_caretOffset < (_windowSize / 2) || (_caretOffset > (_file.Length - (_windowSize / 2) + 1)))
                {
                    //we're within in lower end of the first window or the higher end of the last window
                    //so the right part of the right window is already loaded.
                    //nothing else to do
                }
                else
                {
                    var extremum = _windowSize / 10;
                    if (_caretOffset < (_windowOffset + extremum) || (_caretOffset > (_windowOffset + _windowSize - extremum)))
                    {
                        //user has moved into an extremum of the current window
                        //so shift to new window
                        loadAndPreserveWindow(_caretOffset);
                    }
                }

                //track new carent position as last position
                _prevCaretOffset = _caretOffset;
            }

            System.Diagnostics.Debug.WriteLine($"CaretOffset={_caretOffset}");

            SyncVScroll(_windowOffset);
        }


        private void SyncVScroll(long offset)
        {
            _syncingVScroll = true;
            if (_file.Length <= int.MaxValue)
            {
                //the simple case, just use the scroll value as the position
                //allow offset to fall through
            }
            else
            {
                //the file is HUGE so we have to translate from long to int
                offset = (int)((offset / _file.Length) * int.MaxValue);
            }
            _vscroll.Value = (int)offset;
            System.Diagnostics.Debug.WriteLine($"VScroll={offset}");
            _syncingVScroll = false;
        }


        private string getWindow(long? offset = null)
        {
            //get offset and bufsize, then allocate
            _windowOffset = resolveOffset(offset);

            //now adjust for windowSize
            //generally we'd want to more or less center the offset in the middle of the window
            //but don't go negative
            _windowOffset = _long.Max(_windowOffset - _windowSize / 2, 0);
            //constrain to no more than EOF - the windowsize
            _windowOffset = _long.Min(_windowOffset, _file.Length - _windowSize + 1);

            System.Diagnostics.Debug.WriteLine($"WindowOffset={_windowOffset}");

            var sz = _windowSize;
            if (_file.Length < _windowOffset + sz)
            {
                sz = (int)(_file.Length - _windowOffset);
            }
            var byt = new byte[sz];

            //open the file readonly and read buf
            var stream = File.OpenRead(_file.FullName);
            stream.Position = _windowOffset;
            stream.Read(byt, 0, sz);
            stream.Close();

            CleanBuffer(byt);

            return Encoding.ASCII.GetString(byt);
        }


        private void loadWindow(long? offset)
        {
            _overridingPositionChanged = true;
            //resolve the offset to a real constrained file position
            var ofs = resolveOffset(offset);

            //if the offset has changed, we need to load the new window
            if (ofs != _windowOffset)
            {
                //load the text up
                _editor.Text = getWindow(ofs);
            }

            //reposition cursor based on current loaded window and the requested offset
            _editor.TextArea.Caret.Offset = (int)(ofs - _windowOffset);
            _editor.TextArea.Caret.BringCaretToView();

            _overridingPositionChanged = false;

            SyncVScroll(ofs);
        }


        private void loadAndPreserveWindow(long? offset)
        {
            loadWindow(offset);

            //now scroll vertically so the top line is restored
            var newCurLine = _editor.Document.GetLineByOffset(_editor.CaretOffset);
            var newTopLineNumber = newCurLine.LineNumber - _activeLine;
            var newTopLineVerticalOffset = _editor.TextArea.TextView.GetVisualTopByDocumentLine(newTopLineNumber);
            _editor.ScrollToVerticalOffset(newTopLineVerticalOffset);
        }


        private long resolveOffset(long? offset)
        {
            var ofs = _windowOffset;
            if (offset.HasValue)
            {
                if (offset >= 0)
                {
                    //constrain to no more than length of file
                    ofs = _long.Min(offset.Value, _file.Length);
                }
                else
                {
                    //offset from EOF (where -1 means EOF, so we have to offset by one
                    ofs = _file.Length - _long.Max(offset.Value, -_file.Length) + 1;
                }
            }
            return ofs;
        }


        private void CleanBuffer(byte[] byt)
        {
            for (var i = 0; i < byt.Length; i++)
            {
                switch (byt[i])
                {
                    case 0:
                        byt[i] = 32; break;
                    default:
                        break;
                }
            }
        }


        public void JumpToEnd()
        {
            loadWindow(_file.Length);
        }


        public void JumpToHome()
        {
            loadWindow(0);
        }


        public void JumpToOffset(long offset)
        {
            loadWindow(offset);
        }


        private void _vscroll_Scroll(object sender, ScrollEventArgs e)
        {
            if (!_syncingVScroll)
            {
                var offset = e.NewValue;
                JumpToOffset(offset);
            }
        }


        public void Dispose()
        {
            _editor.TextArea.Caret.PositionChanged -= TextArea_Caret_PositionChanged;
            _editor.PreviewKeyDown -= _editor_PreviewKeyDown;
            _editor.TextArea.TextView.VisualLinesChanged -= TextView_VisualLinesChanged;
        }
    }
}
