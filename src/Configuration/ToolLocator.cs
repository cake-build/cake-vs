// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Cake.VisualStudio.Helpers;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cake.VisualStudio.Configuration
{
    internal sealed class ToolLocator
    {
        private readonly string _executable;
        private List<string> KnownPaths { get; set; } = new List<string>();

        public ToolLocator(string executableName)
        {
            _executable = executableName;
        }

        public ToolLocator AddEnvironmentVariables(string variable = "CAKE_PATHS_TOOLS")
        {
            var env = Environment.GetEnvironmentVariable(variable);
            if (!string.IsNullOrWhiteSpace(env) && Directory.Exists(env))
            {
                KnownPaths.Add(env);
            }
            return this;
        }

        public ToolLocator AddKnownPaths(params string[] paths)
        {
            KnownPaths.AddRange(paths);
            return this;
        }

        public ToolLocator AddConfigPath(Func<ConfigurationParser> configFunc)
        {
            var path = configFunc?.Invoke()?.GetToolsPath();
            if (path != null)
            {
                KnownPaths.Add(path);
                KnownPaths.Add(Path.Combine(path, "Cake"));
            }
            return this;
        }

        public string Locate(string workingDirectory)
        {
            foreach (var path in KnownPaths.Select(p => p.TrimPrefix("./")))
            {
                var fullPath = Path.Combine(workingDirectory, path, _executable);
                if (File.Exists(fullPath)) return fullPath;
            }
            if (PathHelpers.ExistsOnPath(_executable))
            {
                return _executable; // assume PATH
            }
            return null;
        }
    }
}
