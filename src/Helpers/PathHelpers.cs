// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Linq;

namespace Cake.VisualStudio.Helpers
{
    internal static class PathHelpers
    {
        internal static bool ExistsOnPath(string fileName)
        {
            return GetFullPath(fileName) != null;
        }

        internal static string GetFullPath(string fileName)
        {
            if (File.Exists(fileName))
                return Path.GetFullPath(fileName);

            var values = Environment.GetEnvironmentVariable("PATH");
            return values?.Split(';').Select(path => Path.Combine(path, fileName)).FirstOrDefault(File.Exists);
        }
    }
}