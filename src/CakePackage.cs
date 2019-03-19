// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;
using Cake.VisualStudio.ContentType;
using Cake.VisualStudio.Helpers;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Threading;
using IServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Cake.VisualStudio
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [Guid(PackageGuids.guidCakePackageString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideLanguageService(typeof(CakeLanguageService), Helpers.Constants.CakeContentType, 100)]
    [ProvideLanguageExtension(typeof(CakeLanguageService), ".cake")]
    public sealed partial class CakePackage : AsyncPackage, IVsShellPropertyEvents
    {
        private static DTE2 _dte;
        internal static DTE2 Dte => _dte ?? (_dte = (DTE2) GetGlobalService(typeof(DTE)));
        internal static IVsUIShell Shell => _shell ?? (_shell = (IVsUIShell) GetGlobalService(typeof(IVsUIShell)));
        uint _cookie;
        private static IVsUIShell _shell;

        protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            Logger.Initialize(this, Vsix.Name);
            //base.Initialize();

            // switch to the UI thread
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            var shellService = GetService(typeof(SVsShell)) as IVsShell;

            if (shellService != null)
            {
                ErrorHandler.ThrowOnFailure(shellService.AdviseShellPropertyChanges(this, out _cookie));
            }

            Menus.InstallBootstrapperCommand.Initialize(this);
            Menus.InstallShellBootstrapperCommand.Initialize(this);
            Menus.InstallConfigFileCommand.Initialize(this);
            //return base.InitializeAsync(cancellationToken, progress);
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

        public int OnShellPropertyChange(int propid, object var)
        {
            // when zombie state changes to false, finish package initialization
            if ((int)__VSSPROPID.VSSPROPID_Zombie == propid)
            {
                if ((bool)var == false)
                {
                    // zombie state dependent code
                    // Dte = (DTE2)GetService(typeof(DTE));
                    // eventlistener no longer needed

                    var shellService = GetService(typeof(SVsShell)) as IVsShell;

                    if (shellService != null)
                    {
                        ErrorHandler.ThrowOnFailure(shellService.UnadviseShellPropertyChanges(_cookie));
                    }

                    _cookie = 0;
                }
            }

            return VSConstants.S_OK;
        }
    }
}