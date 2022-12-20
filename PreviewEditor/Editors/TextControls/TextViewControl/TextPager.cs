using ICSharpCode.AvalonEdit.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreviewEditor.Editors.TextControls
{
    internal class TextPager
    {
        private EditingFile _file;
        private int _pageSize = 512000;


        public TextPager(EditingFile file)
        {
            _file = file;
            BufferStart = 0;
            Location = 0;
            Columns = 0;
        }

        /// <summary>
        /// Set Columns to positive number to return a "dump mode" stream
        /// where every line is x columns long
        /// </summary>
        public int Columns { get; set; }
        public int BufferStart { get; set; }
        public int Location { get; set; }

        public Stream RawPage { get; set; }

        public Stream Page(int location)
        {
            if (location < 0) location = 0;
            if (location > _file.FileInfo.Length) location = (int)_file.FileInfo.Length;

            int bufSize = _pageSize;

            if (location < _pageSize / 2)
            {
                BufferStart = 0;
            }
            else if (location > _file.FileInfo.Length - _pageSize / 2)
            {
                BufferStart = (int)_file.FileInfo.Length - _pageSize;
                if (BufferStart < 0) BufferStart = 0;
            }
            else
            {
                BufferStart = location - _pageSize / 2;
            }
            //constrain buffer size
            if (_file.FileInfo.Length - BufferStart < bufSize) bufSize = (int)_file.FileInfo.Length - BufferStart;

            var rawbuf = new char[bufSize];
            using (var fileReader = new StreamReader(new FileStream(_file.FileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                fileReader.BaseStream.Seek(BufferStart, SeekOrigin.Begin);
                var l = fileReader.ReadBlock(rawbuf, 0, bufSize);
            }

            var buf = new char[bufSize];
            var firstCR = 0;
            var firstLF = 0;
            var lastCR = bufSize;
            var lastLF = bufSize;
            var i = 0;
            foreach (var c in rawbuf)
            {
                var co = c;
                switch (co)
                {
                    case char.MinValue:
                        co = ' ';
                        //treat a 0 as a line break
                        if (BufferStart + i > Location) lastCR = i;
                        break;
                    case '\r':
                        if (firstCR == 0) firstCR = i;
                        if (BufferStart + i > Location) lastCR = i;
                        break;
                    case '\n':
                        if (firstLF == 0) firstLF = i;
                        if (BufferStart + i > Location) lastLF = i;
                        break;
                    case '\t':
                        //preserve tabs
                        break;
                    case > char.MinValue when char.IsControl(co):
                        //filter all other control chars
                        co = ' ';
                        break;
                }
                buf[i] = co;
                i++;
            }
            var ofs = 0;
            if (BufferStart > 0)
            {
                if (firstLF > 0 && firstLF == firstCR + 1)
                {
                    ofs = firstLF + 1;
                }
                else if (firstLF > 0 && firstCR < firstLF)
                {
                    ofs = firstLF + 1;
                }
                else if (firstCR > 0)
                {
                    ofs = firstCR;
                }
            }

            if (lastCR < lastLF)
            {
                bufSize = lastCR - 1;
            }
            else
            {
                bufSize = lastLF - 1;
            }

            RawPage = new MemoryStream(rawbuf.Skip(ofs).Take(bufSize - ofs + 1).Select(c => (byte)c).ToArray());
            return new MemoryStream(buf.Skip(ofs).Take(bufSize - ofs + 1).Select(c => (byte)c).ToArray());
        }

    }
}
