// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Windows.Media;
using Cake.VisualStudio.Configuration;
using Cake.VisualStudio.Helpers;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TaskRunnerExplorer;
using Constants = Cake.VisualStudio.Helpers.Constants;

namespace Cake.VisualStudio.TaskRunner
{
    class TaskRunnerConfig : ITaskRunnerConfig
    {
        private ITaskRunnerCommandContext _context;

        public TaskRunnerConfig(ITaskRunnerCommandContext context, ITaskRunnerNode hierarchy, ImageSource icon)
        {
            _context = context;
            TaskHierarchy = hierarchy;
            Icon = icon;
        }

        public ImageSource Icon { get; }

        public ITaskRunnerNode TaskHierarchy { get; }

        public void Dispose()
        {
            // Nothing to dispose
        }

        public string LoadBindings(string configPath)
        {
            string bindingPath = ConfigurationParser.GetConfigFilePath(configPath) ?? configPath + ".bindings";
            return File.Exists(bindingPath) ? new ConfigurationParser(bindingPath).LoadBinding().ToXml() : "<binding />";
        }

        public bool SaveBindings(string configPath, string bindingsXml)
        {
            string bindingPath = ConfigurationParser.GetConfigFilePath(configPath, true) ?? configPath + ".bindings";
            var config = new ConfigurationParser(bindingPath);
            try
            {
                ProjectHelpers.CheckFileOutOfSourceControl(bindingPath);

                if (bindingsXml == "<binding />" && File.Exists(bindingPath))
                {
                    config.RemoveBindings();
                }
                else
                {
                    config.SaveBinding(TaskBinding.FromXml(bindingsXml));
                    ProjectHelpers.AddNestedFile(configPath, bindingPath);
                }

                IVsPersistDocData persistDocData;
                if (!CakePackage.IsDocumentDirty(configPath, out persistDocData) && persistDocData != null)
                {
                    int cancelled;
                    string newName;
                    persistDocData.SaveDocData(VSSAVEFLAGS.VSSAVE_SilentSave, out newName, out cancelled);
                }
                else if (persistDocData == null)
                {
                    new FileInfo(configPath).LastWriteTime = DateTime.Now;
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }
        }
    }
}