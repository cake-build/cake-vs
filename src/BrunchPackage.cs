using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;

namespace BrunchTaskRunner
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", Constants.VERSION, IconResourceID = 400)]
    [Guid("4da50b2f-e51e-448a-8fa7-cdb0a5013ee9")]
    public sealed class BrunchPackage : Package
    {
        internal static DTE2 Dte;

        protected override void Initialize()
        {
            Dte = (DTE2)GetService(typeof(DTE));

            Telemetry.Initialize(Dte, Constants.VERSION, "563f0ff8-e4bd-4f17-9821-464bbd5722e4");

            base.Initialize();
        }

        public static bool IsDocumentDirty(string documentPath, out IVsPersistDocData persistDocData)
        {
            var serviceProvider = new ServiceProvider((IServiceProvider)Dte);

            IVsHierarchy vsHierarchy;
            uint itemId, docCookie;
            VsShellUtilities.GetRDTDocumentInfo(
                serviceProvider, documentPath, out vsHierarchy, out itemId, out persistDocData, out docCookie);
            if (persistDocData != null)
            {
                int isDirty;
                persistDocData.IsDocDataDirty(out isDirty);
                return isDirty == 1;
            }

            return false;
        }
    }
}
