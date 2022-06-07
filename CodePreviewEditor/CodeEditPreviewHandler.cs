using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
