using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
