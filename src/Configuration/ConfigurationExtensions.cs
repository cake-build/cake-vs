// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using IniParser.Model;

namespace Cake.VisualStudio.Configuration
{
    internal static class ConfigurationExtensions
    {
        internal static void UpdateBindings(this IniData data, TaskBinding binding, string sectionName = "TaskRunnerBindings")
        {
            if (data.Sections.ContainsSection(sectionName)) data.Sections.RemoveSection(sectionName);
            data.Sections.AddSection(sectionName);
            data[sectionName].AddIfAny(BindingTargets.BeforeBuild, binding.BeforeBuild);
            data[sectionName].AddIfAny(BindingTargets.AfterBuild, binding.AfterBuild);
            data[sectionName].AddIfAny(BindingTargets.Clean, binding.Clean);
            data[sectionName].AddIfAny(BindingTargets.Open, binding.Open);
        }

        internal static IEnumerable<string> GetTasksForTarget(this XElement element, string target)
        {
            return element?.Attribute(target) == null
                ? new List<string>()
                : element.Attribute(target).Value.Split(',').Select(s => s.Trim());
        }

        private static void AddIfAny(this KeyDataCollection collection, string target, IEnumerable<string> data)
        {
            var values = data as IList<string> ?? data.ToList();
            if (values.Any())
            {
                collection[target] = string.Join(",", values).Trim(',');
            }
        }

        internal static IEnumerable<string> ReadValues(this KeyDataCollection collection, string target)
        {
            if (collection == null) return new List<string>();
            try
            {
                var raw = collection[target];
                return raw?.Split(',').Select(s => s.Trim().TrimEnd(',')) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        internal static string ReadValue(this KeyDataCollection collection, string target)
        {
            if (collection == null) return string.Empty;
            try
            {
                var raw = collection[target];
                return raw?.Trim().TrimEnd(',') ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}