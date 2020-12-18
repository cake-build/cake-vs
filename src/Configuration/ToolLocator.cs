// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cake.VisualStudio.Helpers;
using Microsoft.VisualStudio.Shell;

namespace Cake.VisualStudio.Configuration
{
    internal sealed class ToolLocator
    {
        private readonly List<string> _executableNames;
        private List<string> _knownPaths { get; set; } = new List<string>();

        public ToolLocator(string executableName)
        {
            _executable = executableName;
        }

        public ToolLocator AddEnvironmentVariables(string variable = "CAKE_PATHS_TOOLS")
        {
            var env = Environment.GetEnvironmentVariable(variable);

            if (!string.IsNullOrWhiteSpace(env) && Directory.Exists(env))
            {
                _knownPaths.Add(env);
            }

            return this;
        }

        public ToolLocator AddKnownPaths(params string[] paths)
        {
            _knownPaths.AddRange(paths);
            return this;
        }

        public ToolLocator AddConfigPath(Func<ConfigurationParser> configFunc)
        {
            var path = configFunc?.Invoke()?.GetToolsPath();

            if (path != null)
            {
                _knownPaths.Add(path);
                _knownPaths.Add(Path.Combine(path, "Cake"));
            }

            return this;
        }

        public string Locate(string workingDirectory)
        {
            foreach (var path in _knownPaths.Select(p => p.TrimPrefix("./")))
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
