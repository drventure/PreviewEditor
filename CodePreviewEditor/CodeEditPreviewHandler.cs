using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;
using SharpShell.Attributes;
using SharpShell.SharpPreviewHandler;


namespace CodeEditPreviewHandler
{
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.AllFiles)]
    [DisplayName("Code Edit PreviewHandler")]
    [PreviewHandler(DisableLowILProcessIsolation = false)]
    [Guid("933A47DF-A6EE-4ECB-9A8F-3DEC6AC9FA07")]
    public class CodeEditPreviewHandler : SharpPreviewHandler, IDisposable
    {
        static CodeEditPreviewHandler()
        {
            //SetupAssemblyInterceptor();
            OverrideAssemblyResolution();
        }


        private bool _disposedValue;     
        /// <summary>
        /// Disposes objects
        /// </summary>
        /// <param name="disposing">Is Disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    //_codeEditPreviewHandlerControl.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }


        /// <inheritdoc />
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// DoPreview must create the preview handler user interface and initialize it with data
        /// provided by the shell.
        /// </summary>
        /// <returns>
        /// The preview handler user interface
        /// </returns>
        protected override PreviewHandlerControl DoPreview()
        {
            //  Create the handler control
            var handlerControl = new CodeEditPreviewHandlerControl();

            //  Do we have a file path? If so, we can do a preview
            if (!string.IsNullOrEmpty(SelectedFilePath))
                handlerControl.DoPreview(SelectedFilePath);

            //  Return the handler control
            return handlerControl;
        }


        /// <summary>
        /// Intercept loading of assemblies to load certain versions and
        /// to load from resources instead of the filesystem
        /// </summary>
        private static void OverrideAssemblyResolution()
        {
            //we have to manually help resolve certain assembly references because this is a plugin
            //and we don't have access to the main application's config file to set bindingredirects
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
        /// to load from resources instead of the filesystem
        /// </summary>
        private static void SetupAssemblyInterceptor()
        {
            var curdir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            //we have to manually help resolve certain assembly references because this is a plugin
            //and we don't have access to the main application's config file to set bindingredirects
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
