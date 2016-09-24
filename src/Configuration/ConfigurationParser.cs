// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using IniParser;
using IniParser.Model.Configuration;
using IniParser.Parser;

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
    }
}