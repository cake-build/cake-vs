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
                if (confirm)
                {
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
                        var solItems = ProjectHelpers.GetSolutionItemsProject(dte);
                        solItems?.AddFileToProject(targetPath);
                        if (solItems != null)
                            VsShellUtilities.LogMessage(Constants.PackageName,
                                $"New file added to Solution Items for solution {dte.Solution.FullName}",
                                __ACTIVITYLOG_ENTRYTYPE.ALE_INFORMATION);
                    }
                }
                return true;
            }
            catch
            {
                VsShellUtilities.LogError(Constants.PackageName, $"There was an error downloading the requested file: '{downloadPath}'");
                return false;
            }
        }
    }
}
