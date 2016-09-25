// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Cake.VisualStudio.Helpers;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Constants = Cake.VisualStudio.Helpers.Constants;

namespace Cake.VisualStudio.Menus
{
    internal static class MenuHelpers
    {

        internal static bool DownloadFileToProject(string downloadPath, string targetFileName)
        {
            var dte = CakePackage.Dte;
            return DownloadFileToProject(downloadPath, targetFileName, targetPath =>
            {
                var solItems = ProjectHelpers.GetSolutionItemsProject(dte);
                solItems?.AddFileToProject(targetPath);
                if (solItems != null)
                    VsShellUtilities.LogMessage(Constants.PackageName,
                        $"New file added to Solution Items for solution {dte.Solution.FullName}",
                        __ACTIVITYLOG_ENTRYTYPE.ALE_INFORMATION);
            });
        }

        internal static bool DownloadFileToProject(string downloadPath, string targetFileName,
            Action<string> installCallback)
        {
            var dte = CakePackage.Dte;
            try
            {
                var slnFilePath = new FileInfo(dte.Solution.FullName).FullName;
                var cakeScriptPath = dte.Solution.FindProjectItem(Constants.ScriptFileName)?.FileNames[1];
                var targetPath = Path.Combine(new FileInfo(string.IsNullOrWhiteSpace(cakeScriptPath) ? slnFilePath : cakeScriptPath).Directory.FullName, targetFileName);
                bool confirm = true;
                if (File.Exists(targetPath))
                {
                    confirm = VsShellUtilities.PromptYesNo("File already exists. Overwrite?", $"Downloading {targetFileName}",
                        OLEMSGICON.OLEMSGICON_QUERY, CakePackage.Shell);
                }
                if (!confirm) return true;
                try
                {
                    ProjectHelpers.CheckFileOutOfSourceControl(targetPath);
                }
                catch (Exception)
                {
                    // ignored
                }
                using (var wc = new WebClient())
                {
                    wc.DownloadFile(downloadPath, targetPath);
                    VsShellUtilities.LogMessage(Constants.PackageName,
                        $"File downloaded from '{downloadPath}' to '{targetPath}'",
                        __ACTIVITYLOG_ENTRYTYPE.ALE_INFORMATION);
                    installCallback.Invoke(targetPath);
                }
                return true;
            }
            catch
            {
                VsShellUtilities.LogError(Constants.PackageName, $"There was an error downloading the requested file: '{downloadPath}'");
                return false;
            }
        }

        internal static readonly Action<string> ProjectInstallCommand = s =>
        {
            var dte = CakePackage.Dte;
            var scriptPath = CakePackage.Dte.Solution?.FindProjectItem(Constants.ScriptFileName);
            EnvDTE.Project proj;
            if (scriptPath != null && scriptPath.FileCount == 1)
            {
                proj = scriptPath.ContainingProject;
            }
            else
            {
                proj = ProjectHelpers.GetSolutionItemsProject(dte);
            }
            if (proj == null) return;
            proj.AddFileToProject(s);
            VsShellUtilities.LogMessage(Constants.PackageName,
                $"New file added to {proj.Name} for solution {dte.Solution.FullName}",
                __ACTIVITYLOG_ENTRYTYPE.ALE_INFORMATION);
        };
    }
}