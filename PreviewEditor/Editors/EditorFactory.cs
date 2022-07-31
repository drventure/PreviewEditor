using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PreviewEditor.Editors
{
    internal static class EditorFactory
    {
        internal static IPreviewEditorControl GetEditor(EditingFile file)
        {
            if (file.IsLikelyTextFile)
            {
                return GetTextEditor(file);
            }
            else
            {
                return GetHexEditor(file);
            }
        }


        internal static IPreviewEditorControl GetTextEditor(EditingFile file)
        {
            if (file.FileInfo.Length > PreviewEditor.Settings.TextEditorOptions.MaxEditableFileSize)
            {
                return new TextViewControl(file);
            }
            else
            {
                return new TextEditControl(file);
            }
        }


        internal static IPreviewEditorControl GetHexEditor(EditingFile file)
        {
            return new WPFHexEditControl(file);
        }

    }
}
