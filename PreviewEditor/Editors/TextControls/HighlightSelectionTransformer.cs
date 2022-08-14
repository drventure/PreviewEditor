using System;
using System.Windows.Media;

using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;


public class HighlightSelectionTransformer : DocumentColorizingTransformer
{
    private readonly string _selectedText;

    public HighlightSelectionTransformer(string selectedText)
    {
        _selectedText = selectedText;
    }


    protected override void ColorizeLine(DocumentLine line)
    {
        if (string.IsNullOrEmpty(_selectedText))
        {
            return;
        }

        int lineStartOffset = line.Offset;
        string text = CurrentContext.Document.GetText(line);
        int start = 0;
        int index;

        //TODO Does this need to work with Regexs and other find options?
        while ((index = text.IndexOf(_selectedText, start, StringComparison.Ordinal)) >= 0)
        {
            ChangeLinePart(
              lineStartOffset + index, // startOffset
              lineStartOffset + index + _selectedText.Length, // endOffset
              element => element.TextRunProperties.SetBackgroundBrush(Brushes.CadetBlue));
            start = index + 1; // search for next occurrence
        }
    }
}