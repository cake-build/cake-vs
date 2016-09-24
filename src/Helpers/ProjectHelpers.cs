// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Cake.VisualStudio.Helpers
{
    public static class ProjectHelpers
    {
        private static DTE2 _dte = CakePackage.Dte;

        public static void CheckFileOutOfSourceControl(string file)
        {
            if (!File.Exists(file) || _dte.Solution.FindProjectItem(file) == null)
                return;

            if (_dte.SourceControl.IsItemUnderSCC(file) && !_dte.SourceControl.IsItemCheckedOut(file))
                _dte.SourceControl.CheckOutItem(file);

            var info = new FileInfo(file);
            info.IsReadOnly = false;
        }

        public static void AddFileToProject(this Project project, string file, string itemType = null)
        {
            if (project.IsKind(ProjectTypes.ASPNET_5))
                return;

            try
            {
                if (_dte.Solution.FindProjectItem(file) == null)
                {
                    var item = project.ProjectItems.AddFromFile(file);

                    if (string.IsNullOrEmpty(itemType)
                        || project.IsKind(ProjectTypes.WEBSITE_PROJECT)
                        || project.IsKind(ProjectTypes.UNIVERSAL_APP))
                        return;

                    item.Properties.Item("ItemType").Value = "None";
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public static void AddNestedFile(string parentFile, string newFile)
        {
            var item = _dte.Solution.FindProjectItem(parentFile);

            try
            {
                if (item == null
                    || item.ContainingProject == null
                    || item.ContainingProject.IsKind(ProjectTypes.ASPNET_5))
                    return;

                if (item.ProjectItems == null || item.ContainingProject.IsKind(ProjectTypes.UNIVERSAL_APP))
                {
                    item.ContainingProject.AddFileToProject(newFile);
                }
                else if (_dte.Solution.FindProjectItem(newFile) == null)
                {
                    item.ProjectItems.AddFromFile(newFile);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public static bool IsKind(this Project project, string kindGuid)
        {
            return project.Kind.Equals(kindGuid, StringComparison.OrdinalIgnoreCase);
        }

        public static void DeleteFileFromProject(string file)
        {
            var item = _dte.Solution.FindProjectItem(file);

            if (item == null)
                return;
            try
            {
                item.Delete();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private static IEnumerable<Project> GetChildProjects(Project parent)
        {
            try
            {
                if (!parent.IsKind(ProjectKinds.vsProjectKindSolutionFolder) && parent.Collection == null)  // Unloaded
                    return Enumerable.Empty<Project>();

                if (!string.IsNullOrEmpty(parent.FullName))
                    return new[] { parent };
            }
            catch (COMException)
            {
                return Enumerable.Empty<Project>();
            }

            return parent.ProjectItems
                    .Cast<ProjectItem>()
                    .Where(p => p.SubProject != null)
                    .SelectMany(p => GetChildProjects(p.SubProject));
        }

        internal static Project GetSolutionItemsProject(DTE2 dte)
        {
            var solItems =
                            dte.Solution.Projects.Cast<Project>().FirstOrDefault(p => p.Name == "Solution Items" || p.Kind == EnvDTE.Constants.vsProjectItemKindSolutionItems);
            var projs = dte.Solution.Projects.Cast<Project>().ToList();
            if (solItems == null)
            {
                try
                {
                    var sol2 = (Solution2)dte.Solution;
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
            return solItems;
        }
    }

    public static class ProjectTypes
    {
        public const string ASPNET_5 = "{8BB2217D-0F2D-49D1-97BC-3654ED321F3B}";
        public const string WEBSITE_PROJECT = "{E24C65DC-7377-472B-9ABA-BC803B73C61A}";
        public const string UNIVERSAL_APP = "{262852C6-CD72-467D-83FE-5EEB1973A190}";
    }
}