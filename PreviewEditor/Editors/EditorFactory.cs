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
        /// <summary>
        /// size beyond which control becomes read only and this editor won't handle it
        /// </summary>
        private const int MAXEDITABLESIZE = 2 * 1000 * 1000;


        internal static IPreviewEditorControl GetEditor(EditingFile file)
        {
            if (file.IsLikelyTextFile)
                if (file.FileInfo.Length > MAXEDITABLESIZE)
                {
                    return new TextViewControl(file);
                }
                else
                {
                    return new TextEditControl(file);
                }
            else
            {
                return new WPFHexEditControl(file);
            }
        }
    }
}
