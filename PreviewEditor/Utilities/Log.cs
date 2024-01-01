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
        public static string Filename = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "PreviewEditor.log");

        private static string TAG = "PRVWED";
        private static string _format = "{0} [{1}] {2} - {3}\r\n";

        private static string date
        {
            get
            {
                return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ff");
            }
        }

        private static string Format(string message, string caller)
        {
            return string.Format(_format, date, TAG, caller, message);
        }

        private static string Format(Exception ex, string caller)
        {
            return string.Format(_format, date, TAG, caller, "Exception: " + ex.ToString());
        }


        private static string Format(Exception ex, string message, string caller)
        {
            return string.Format(_format, date, TAG, caller, message + " - Exception: " + ex.ToString());
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

        public static void Debug(string message, [CallerMemberName] string caller = "")
        {
            if (!Enabled) return;
            WriteOut(Format(message, caller));
        }


        public static void Error(string message, [CallerMemberName] string caller = "")
        {
            if (!Enabled) return;
            WriteOut(Format(message, caller));
        }


        public static void Error(Exception ex, [CallerMemberName] string caller = "")
        {
            if (!Enabled) return;
            WriteOut(Format(ex, caller));
        }


        public static void Error(Exception ex, string message, [CallerMemberName] string caller = "")
        {
            if (!Enabled) return;
            WriteOut(Format(ex, message, caller));
        }
    }
}
