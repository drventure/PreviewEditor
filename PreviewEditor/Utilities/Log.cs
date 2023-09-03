using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace PreviewEditor
{
    internal static class Log
    {
        public static bool Enabled = false;
        public static string Filename = null;

        private static string TAG = "[PREVIEWEDITOR]";
        private static string _format = "{0} {1} {3} {4}\r\n";

        private static string date
        {
            get
            {
                return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            }
        }

        private static string Format(string message, [CallerMemberName] string type = "")
        {
            return string.Format(_format, date, type.ToUpper(), TAG, message);
        }

        private static string Format(Exception ex, [CallerMemberName] string type = "")
        {
            return string.Format(_format, date, type.ToUpper(), TAG, "Exception: " + ex.ToString());
        }


        private static string Format(Exception ex, string message, [CallerMemberName] string type = "")
        {
            return string.Format(_format, date, type.ToUpper(), TAG, message + " - Exception: " + ex.ToString());
        }


        private static void WriteBuf(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }


        private static void WriteFile(string message)
        {
            if (Filename == null) return;
            try
            {
                File.AppendAllText(Filename, message);
            }
            catch { }
        }


        private static void WriteOut(string message)
        {
            WriteBuf(message);
            WriteFile(message);
        }

        public static void Debug(string message)
        {
            if (!Enabled) return;
            WriteOut(Format(message));
        }


        public static void Error(string message)
        {
            if (!Enabled) return;
            WriteOut(Format(message));
        }


        public static void Error(Exception ex)
        {
            if (!Enabled) return;
            WriteOut(Format(ex));
        }


        public static void Error(Exception ex, string message)
        {
            if (!Enabled) return;
            WriteOut(Format(ex, message));
        }
    }
}
