// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel.Design;
using Cake.VisualStudio.Helpers;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Constants = Cake.VisualStudio.Helpers.Constants;

namespace Cake.VisualStudio.Menus
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class InstallShellBootstrapperCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = PackageIds.cmdidInstallShellBootstrapperCommand;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("6003519f-6876-4db3-ad29-8d5379949869");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package _package;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstallShellBootstrapperCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner _package, not null.</param>
        private InstallShellBootstrapperCommand(Package package)
        {
            _package = package ?? throw new ArgumentNullException("package");

            var commandService = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static InstallShellBootstrapperCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner _package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return _package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner _package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new InstallShellBootstrapperCommand(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            var dte = CakePackage.Dte;

            if (dte == null)
            {
                return;
            }

            if (dte.Solution == null || dte.Solution.Count == 0)
            {
                ServiceProvider.ShowMessageBox("No solution opened");
            }
            else
            {
                if (MenuHelpers.DownloadFileToProject(Constants.BashUri, "build.sh"))
                {
                    VsShellUtilities.LogMessage(Constants.PackageName, "Bootstrapper installed into solution", __ACTIVITYLOG_ENTRYTYPE.ALE_INFORMATION);
                    ServiceProvider.ShowMessageBox("Cake Bootstrapper script successfully downloaded.");
                }
            }

            // Show a message box to prove we were here
        }
    }
}