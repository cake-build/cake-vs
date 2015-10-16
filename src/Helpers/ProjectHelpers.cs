using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;

namespace BrunchTaskRunner
{
    public static class ProjectHelpers
    {
        private static DTE2 _dte = BrunchPackage.Dte;

        public static void CheckFileOutOfSourceControl(string file)
        {
            if (!File.Exists(file) || _dte.Solution.FindProjectItem(file) == null)
                return;

            if (_dte.SourceControl.IsItemUnderSCC(file) && !_dte.SourceControl.IsItemCheckedOut(file))
                _dte.SourceControl.CheckOutItem(file);

            FileInfo info = new FileInfo(file);
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
                    ProjectItem item = project.ProjectItems.AddFromFile(file);

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
            ProjectItem item = _dte.Solution.FindProjectItem(parentFile);

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
            ProjectItem item = _dte.Solution.FindProjectItem(file);

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
    }

    public static class ProjectTypes
    {
        public const string ASPNET_5 = "{8BB2217D-0F2D-49D1-97BC-3654ED321F3B}";
        public const string WEBSITE_PROJECT = "{E24C65DC-7377-472B-9ABA-BC803B73C61A}";
        public const string UNIVERSAL_APP = "{262852C6-CD72-467D-83FE-5EEB1973A190}";
    }
}
