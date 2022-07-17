using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using PreviewHandler.Sdk;
using PreviewHandler.Sdk.Controls;
using PreviewHandler.Sdk.Handlers;


namespace PreviewEditor
{
    /// <summary>
    /// Main entry point of Preview handler.
    /// This is what the hosting app will dynamically instantiate
    /// The base class handles all the messy COM stuff.
    /// 
    /// Note that there are 2 different base classes. This one 
    /// is based on the FileBasedPreviewHandler, which accepts a FILENAME
    /// for the argument.
    /// 
    /// Not using the stream based handler right now.
    /// </summary>
    [PreviewHandler("Preview Editor Handler", ".cs;.vb;.html;.htm;.log;.txt;.cshtml;.css;.csproj;.vbproj", "{C71E74A6-4C57-4297-90E3-A221F6EECF24}")]
    [ProgId("PreviewEditorHandler")]
    [Guid("D2A0858D-6E99-4A5A-87C9-2590A1B1B720")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    public sealed class PreviewEditorHandler : FileBasedPreviewHandler
    {
        PreviewEditorControl _control;


        /// <summary>
        /// This static constructor allows me to override
        /// assembly resolution and load assemblies from embedded resources
        /// </summary>
        static PreviewEditorHandler()
        {
            //log.Debug("Handler.DoPreview: Constructor");
            //log.Debug("Version: " + typeof(CodeEditPreviewHandler).Assembly.GetName().Version);

            //SetupAssemblyInterceptor();
            //OverrideAssemblyResolution();
        }


        protected override IPreviewHandlerControl CreatePreviewHandlerControl()
        {

            _control = new PreviewEditorControl();
            return _control;
        }


        public override void DoPreview()
        {
            _control.DoPreview(FilePath);
        }


        /// <summary>
        /// Intercept loading of assemblies to load certain versions and
        /// to load from resources instead of the file system
        /// </summary>
        private static void OverrideAssemblyResolution()
        {
            //we have to manually help resolve certain assembly references because this is a plugin
            //and we don't have access to the main application's config file to set binding redirects
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                var name = args.Name;

                var asmName = new AssemblyName(name);

                //App.Write($"Resolving {name}");

                if (name.Contains("SharpShell"))
                {
                    asmName.Version = new Version("2.7.2.0");
                    return Assembly.Load(asmName);
                }
                else
                {
                    return null;
                }
            };

        }


        /// <summary>
        /// Intercept loading of assemblies to load certain versions and
        /// to load from resources instead of the file system
        /// </summary>
        private static void SetupAssemblyInterceptor()
        {
            var curdir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            //we have to manually help resolve certain assembly references because this is a plugin
            //and we don't have access to the main application's config file to set binding redirects
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                var name = args.Name;

                //App.Write($"Resolving {name}");

                var internalDlls = new string[]
                {
                    "SharpShell",
                };
                if (internalDlls.Any(s => name.Contains(s)))
                {
                    //App.Write("   loaded from resource...");
                    return LoadResourceAssembly(args.Name);
                    //name = name.Replace("4.0.0.0", "5.0.0.0");
                    //name = Path.Combine(curdir, "System.Text.Json.dll");
                    //return Assembly.Load(name);
                }
                //else if (name.Contains("System.Runtime.CompilerServices.Unsafe"))
                //{
                //    name = name.Replace("4.0.4.1", "4.0.5.0");
                //    //name = Path.Combine(curdir, "System.Runtime.CompilerServices.Unsafe.dll");
                //    return Assembly.Load(name);
                //}
                //else if (name.Contains("System.Threading.Tasks.Extensions"))
                //{
                //    name = "System.Threading.Tasks.Extensions, Version=4.2.0.1, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51";
                //    //name = Path.Combine(curdir, "System.Threading.Tasks.Extensions.dll");
                //    return Assembly.Load(name);
                //}
                else
                {
                    return null;
                }
            };

        }

        /// <summary>
        /// Load missing assemblies from compiled-in resources instead
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static Assembly LoadResourceAssembly(string assemblyName)
        {
            String resourceName = $"CodePreviewEditor.AssemblyResources.{new AssemblyName(assemblyName).Name}.dll";

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                Byte[] assemblyData = new Byte[stream.Length];
                stream.Read(assemblyData, 0, assemblyData.Length);
                return Assembly.Load(assemblyData);
            }
        }

    }
}
