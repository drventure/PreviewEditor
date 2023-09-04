using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PreviewEditor.Editors.TextControls;


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
            if (file.IsTextEditable)
            {
                return new TextEditControl(file);
            }
            else
            {
                return new TextViewControl(file);
            }
        }


        internal static IPreviewEditorControl GetHexEditor(EditingFile file)
        {
            return new HexEditControl(file);
        }
    }
}
