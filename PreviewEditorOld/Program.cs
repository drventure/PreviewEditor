using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PreviewEditor
{
    internal static class Program
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int pid);


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //since this is a winforms app, have to handle the console special
            if (!AttachConsole(-1))
            {
                AllocConsole();
            }

            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("PreviewEditor Windows PreviewHandler for editing Text and Binary files");
            Console.WriteLine("(c) 2022 drVenture and Associates, all rights reserved");
            Console.WriteLine("");

            foreach (var arg in args)
            {
                if (new string[] { "/h", "-h", "/?", "-?" }.Contains(arg.ToLower()))
                {
                    WriteUsage();
                    Environment.Exit(0);
                }
                else if (new string[] { "-r" }.Contains(arg.ToLower()))
                {
                    try
                    {
                        PreviewEditor.Register(typeof(PreviewEditorHandler));
                        Console.WriteLine("Registered PreviewEditor successfully");
                        Environment.Exit(0);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Unable to register PreviewEditor");
                        Console.WriteLine("");
                        Console.WriteLine(ex.ToString());
                        Environment.Exit(1);
                    }
                }
                else if (new string[] { "-u" }.Contains(arg.ToLower()))
                {
                    try
                    {
                        PreviewEditor.Unregister(typeof(PreviewEditorHandler));
                        Console.WriteLine("Unregistered PreviewEditor successfully");
                        Environment.Exit(0);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Unable to unregister PreviewEditor");
                        Console.WriteLine("");
                        Console.WriteLine(ex.ToString());
                        Environment.Exit(1);
                    }
                }
            }

            Console.WriteLine("Unknown or unspecified argument");
            Console.WriteLine("");

            WriteUsage();

            Environment.Exit(0);
        }


        internal static void WriteUsage()
        {
            Console.WriteLine("usage:");
            Console.WriteLine(" -h        this help");
            Console.WriteLine(" -r        register this preview handler");
            Console.WriteLine(" -u        unregister this preview handler");
            Console.WriteLine("");
        }
    }
}
