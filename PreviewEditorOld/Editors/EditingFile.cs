using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PreviewEditor.Editors
{
    /// <summary>
    /// Represents the single file currently being pointed to/opened for editing
    /// </summary>
    internal class EditingFile 
    {
        private FileInfo _fileInfo;

        public EditingFile(FileInfo fileinfo)
        {
            _fileInfo = fileinfo;
        }


        public EditingFile(string filename) : this(new FileInfo(filename))
        {
        }


        /// <summary>
        /// Returns the FileInfo on the file that was selected for preview
        /// </summary>
        public FileInfo FileInfo { get { return _fileInfo; } }


        /// <summary>
        /// returns true if this file is not too big to be edited
        /// but the file ALSO must be non read only and NOT system
        /// </summary>
        public bool IsTextEditable
        {
            get
            {
                var ro = _fileInfo.Attributes.HasFlag(FileAttributes.ReadOnly) || _fileInfo.Attributes.HasFlag(FileAttributes.System);
                return !ro && _fileInfo.Length <= PreviewEditor.Settings.TextEditorOptions.MaxEditableFileSize;
            }
        }

        /// <summary>
        /// returns true if this file is not too big to be edited
        /// but the file ALSO must be non read only and NOT system
        /// </summary>
        public bool IsHexEditable
        {
            get
            {
                var ro = _fileInfo.Attributes.HasFlag(FileAttributes.ReadOnly) || _fileInfo.Attributes.HasFlag(FileAttributes.System);
                return !ro && _fileInfo.Length <= PreviewEditor.Settings.HexEditorOptions.MaxEditableFileSize;
            }
        }

        /// <summary>
        /// If set, represents a stream of the given file to be edited
        /// This is used when switching from a TextEditor to a HexEditor or back
        /// </summary>
        public Stream Stream { get; set; }


        /// <summary>
        /// Represents the position of the cursor or the start of the selection
        /// </summary>
        public long SelectionStart { get; set; }


        /// <summary>
        /// Represents the length of the selection 
        /// </summary>
        public long SelectionLength { get; set; }


        /// <summary>
        /// True if the file has been changed
        /// </summary>
        public bool IsDirty { get; set; }


        /// <summary>
        /// Marks the file as changed
        /// </summary>
        public void SetDirty() { this.IsDirty = true; }


        /// <summary>
        /// check the first 8k for any null chars
        /// If there is one, this is likely a binary file
        /// </summary>
        /// <returns></returns>
        public bool IsLikelyTextFile
        {
            get
            {
                StreamReader fileReader = null;
                var r = false;
                try
                {
                    var l = this.FileInfo.Length;
                    l = l > 8000 ? 8000 : l;
                    if (l == 0) return true;
                    char[] chars = new char[l];
                    using (fileReader = new StreamReader(new FileStream(this.FileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
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
}

