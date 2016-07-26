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
                var slnDirPath = Path.Combine(new FileInfo(dte.Solution.FullName).Directory.FullName);
                var targetPath = Path.Combine(slnDirPath, targetFileName);
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
                        var solItems =
                            dte.Solution.Projects.Cast<Project>().FirstOrDefault(p => p.Name == "Solution Items" && p.Kind == EnvDTE.Constants.vsProjectItemKindSolutionItems);
                        if (solItems == null)
                        {
                            try
                            {
                                Solution2 sol2 = (Solution2) CakePackage.Dte.Solution;
                                solItems = sol2.AddSolutionFolder("Solution Items");
                                VsShellUtilities.LogMessage(Constants.PackageName,
                                    $"Created Solution Items project for solution {dte.Solution.FullName}",
                                    __ACTIVITYLOG_ENTRYTYPE.ALE_INFORMATION);
                            }
                            catch
                            {
                                // ignored
                            }
                        }
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
