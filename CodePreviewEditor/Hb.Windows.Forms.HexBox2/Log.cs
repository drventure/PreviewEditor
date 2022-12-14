using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Hb.Windows.Forms
{
    internal static class Log
    {
        private static string _format = "{0} {1} [HEXBOX] {2}";

        private static string date
        {
            get
            {
                return $"{DateTime.Now:yyyy-MM-ddTHH:mm:ss:fff}";
            }
        }

        public static string Format(string message, [CallerMemberName] string type = "DEBUG")
        {
            return string.Format(_format, date, type.ToUpper(), message);
        }

        public static string Format(Exception ex, [CallerMemberName] string type = "DEBUG")
        {
            return string.Format(_format, date, type.ToUpper(), "Exception: " + ex.ToString());
        }

        public static string Format(Exception ex, string message, [CallerMemberName] string type = "DEBUG")
        {
            return string.Format(_format, date, type.ToUpper(), message + " - Exception: " + ex.ToString());
        }


        public static void Debug(string message)
        {
            System.Diagnostics.Debug.WriteLine(Format(message));
        }


        public static void Error(string message)
        {
            System.Diagnostics.Debug.WriteLine(Format(message));
        }
        
        
        public static void Error(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(Format(ex));
        }


        public static void Error(Exception ex, string message)
        {
            System.Diagnostics.Debug.WriteLine(Format(ex, message));
        }
    }
}
