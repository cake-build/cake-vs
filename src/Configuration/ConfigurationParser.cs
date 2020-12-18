// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Cake.VisualStudio.Helpers;
using IniParser;
using IniParser.Model.Configuration;
using IniParser.Parser;
using System.IO;
using System.Linq;

namespace Cake.VisualStudio.Configuration
{
    internal sealed class ConfigurationParser
    {
        private static IniDataParser Parser => new IniDataParser(new IniParserConfiguration {AssigmentSpacer = ""});
        internal string SectionName { get; set; } = "TaskRunnerBindings";

        public ConfigurationParser(string filePath)
        {
            FilePath = filePath;
        }

        private string FilePath { get; set; }
        internal void SaveBinding(TaskBinding binding)
        {
            var parser = new FileIniDataParser(Parser);
            var data = parser.ReadFile(FilePath);
            data.UpdateBindings(binding);
            parser.WriteFile(FilePath, data);
        }

        internal TaskBinding LoadBinding()
        {
            var parser = new FileIniDataParser(Parser);
            var data = parser.ReadFile(FilePath);
            return new TaskBinding
            {
                BeforeBuild = data[SectionName].ReadValues(BindingTargets.BeforeBuild),
                AfterBuild = data[SectionName].ReadValues(BindingTargets.AfterBuild),
                Clean = data[SectionName].ReadValues(BindingTargets.Clean),
                Open = data[SectionName].ReadValues(BindingTargets.Open)
            };
        }

        internal void RemoveBindings()
        {
            var parser = new FileIniDataParser(Parser);
            var data = parser.ReadFile(FilePath);
            var removeSection = data.Sections.RemoveSection(SectionName);
            parser.WriteFile(FilePath, data);
        }

        /// <summary>
        /// Gets a key from the Paths section of cake.config
        /// </summary>
        /// <param name="key">The key to retrieve.</param>
        /// <returns>The config value</returns>
        /// <remarks>
        /// This defaults the key to Tools since that's what we're using currently.
        /// I've left this an argument in case we add a non-standard key to cake.config to specifically override VS.
        /// </remarks>
        internal string GetToolsPath(string key = "Tools")
        {
            if (!File.Exists(FilePath))
            {
                return null;
            }

            var parser = new FileIniDataParser(Parser);
            var data = parser.ReadFile(FilePath);
            if (data.Sections.ContainsSection("Paths"))
            {
                return data["Paths"].ReadValues(key).LastOrDefault();
            }
            return null;
        }

        internal static string GetConfigFilePath(string configPath, bool create = false)
        {
            string bindingPath;
            var path = CakePackage.Dte?.Solution?.FindProjectItem(Constants.ConfigFileName);
            if (path != null && path.FileCount == 1)
            {
                bindingPath = path.FileNames[1];
            }
            else
            {
                var cpath = Path.Combine(Path.GetDirectoryName(configPath), Constants.ConfigFileName);
                try
                {

                    if (!File.Exists(cpath) && create) File.Create(cpath).Close();
                    if (File.Exists(cpath)) ProjectHelpers.GetSolutionItemsProject(CakePackage.Dte).AddFileToProject(cpath);
                }
                catch
                {
                    // ignored
                }
                bindingPath = cpath;
            }
            return string.IsNullOrWhiteSpace(bindingPath) ? null : bindingPath; // remove the empty string scenario
        }
    }
}