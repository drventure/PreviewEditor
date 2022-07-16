using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PreviewEditor
{
    [RunInstaller(true)]
    public class ComInstaller : Installer
    {
        public override void Install(IDictionary stateSaver)
        {
            try
            {
                base.Install(stateSaver);
                RegistrationServices regsrv = new RegistrationServices();
                if (!regsrv.RegisterAssembly(this.GetType().Assembly, AssemblyRegistrationFlags.SetCodeBase))
                {
                    throw new InstallException("Failed to register for COM interop.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
                throw;
            }
        }

        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);
            RegistrationServices regsrv = new RegistrationServices();
            if (!regsrv.UnregisterAssembly(this.GetType().Assembly))
            {
                throw new InstallException("Failed to unregister for COM interop.");
            }
        }
    }
}
