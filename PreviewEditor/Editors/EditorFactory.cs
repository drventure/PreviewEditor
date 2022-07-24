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


        internal static IPreviewEditorControl GetEditor(FileInfo fileInfo)
        {
            if (IsLikelyTextFile(fileInfo))
                if (fileInfo.Length > MAXEDITABLESIZE)
                {
                    return new TextViewControl(fileInfo);
                }
                else
                {
                    return new TextEditControl(fileInfo);
                }
            else
            {
                return new WPFHexEditControl(fileInfo);
            }
        }


        /// <summary>
        /// check the first 8k for any null chars
        /// If there is one, this is likely a binary file
        /// </summary>
        /// <returns></returns>
        private static bool IsLikelyTextFile(FileInfo fileInfo)
        {
            StreamReader fileReader = null;
            var r = false;
            try
            {
                var l = fileInfo.Length;
                l = l > 8000 ? 8000 : l;
                if (l == 0) return true;
                char[] chars = new char[l];
                using (fileReader = new StreamReader(new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    l = fileReader.ReadBlock(chars, 0, (int)l);
                }
                r = true;
                for (int i = 0; i < l; i++)
                {
                    if (chars[i] == 0)
                    {
                        r = false;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex;
                r = false;
            }
            finally
            {
            }
            return r;
        }



    }
}
