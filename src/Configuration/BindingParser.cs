// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Cake.VisualStudio.Configuration
{
    internal class TaskBinding
    {
        public IEnumerable<string> BeforeBuild { get; set; } = new List<string>();
        public IEnumerable<string> AfterBuild { get; set; } = new List<string>();
        public IEnumerable<string> Clean { get; set; } = new List<string>();
        public IEnumerable<string> Open { get; set; } = new List<string>();

        internal string ToXml()
        {
            var sb = new StringBuilder();
            sb.Append("<binding ");
            if (BeforeBuild.Any()) sb.Append($"{BindingTargets.BeforeBuild}='{string.Join(",", BeforeBuild)}' ");
            if (AfterBuild.Any()) sb.Append($"{BindingTargets.AfterBuild}='{string.Join(",", AfterBuild)}' ");
            if (Clean.Any()) sb.Append($"{BindingTargets.Clean}='{string.Join(",", Clean)}' ");
            if (Open.Any()) sb.Append($"{BindingTargets.Open}='{string.Join(",", Open)}' ");
            sb.Append("/>");
            return sb.ToString();
        }

        internal static TaskBinding FromXml(string bindingsXml)
        {
            var binding = new TaskBinding();
            var xml = XDocument.Parse(bindingsXml, LoadOptions.None);
            binding.BeforeBuild = xml.Root.GetTasksForTarget(BindingTargets.BeforeBuild);
            binding.AfterBuild = xml.Root.GetTasksForTarget(BindingTargets.AfterBuild);
            binding.Clean = xml.Root.GetTasksForTarget(BindingTargets.Clean);
            binding.Open = xml.Root.GetTasksForTarget(BindingTargets.Open);
            return binding;
        }
    }

    internal static class BindingTargets
    {
        internal const string BeforeBuild = "BeforeBuild";
        internal const string AfterBuild = "AfterBuild";
        internal const string Clean = "Clean";
        internal const string Open = "ProjectOpen";
    }
}