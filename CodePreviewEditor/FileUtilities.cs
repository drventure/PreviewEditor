using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeEditPreviewHandler
{
    internal static class FileUtilities
    {
        /// <summary>
        /// returns a list of extensions (".xxx" format) that should be treated as text
        /// everything else will be treated as hex
        /// </summary>
        /// <returns></returns>
        public static List<string> TextFileExtensions()
        {
            return new List<string>(new string[] { ".cs", ".vb", ".txt", ".htm", ".html", ".csproj", ".log", ".me", ".readme" });
        }


        /// <summary>
        /// return true if the given filename looks like a text file that we should
        /// use the text editor for
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool IsTextFile(string filePath)
        {
            var ext = Path.GetExtension(filePath);
            if (ext == null || ext.Length == 0) return false;

            return TextFileExtensions().Contains(ext.ToLower());
        }
    }
}
