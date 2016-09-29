// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cake.VisualStudio.TaskRunner
{
    internal class ScriptContent
    {
        internal ScriptContent(string filePath)
        {
            Content = File.ReadAllLines(filePath).ToList();
            FilePath = new FileInfo(filePath).FullName;
        }

        private List<string> Content { get; }
        private Dictionary<int, ScriptContent> Loads { get; set; } = new Dictionary<int, ScriptContent>();
        private string FilePath { get; set; }

        internal void Parse(string pattern, Func<string, string> stripFunc)
        {
            foreach (var line in Content.Where(l => l.StartsWith(pattern)).ToList())
            {
                var dirPath = new FileInfo(FilePath).Directory ?? new DirectoryInfo(Directory.GetCurrentDirectory());
                var path = Path.Combine(dirPath.FullName, stripFunc.Invoke(line));
                Loads.Add(Content.IndexOf(line), new ScriptContent(path));
            }
            foreach (var load in Loads)
            {
                load.Value.Parse(pattern, stripFunc);
            }
        }

        public override string ToString()
        {
            foreach (var load in Loads)
            {
                Content[load.Key] = load.Value.ToString();
            }
            return string.Join(Environment.NewLine,
                Content.SelectMany(c => c.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)));
        }
    }
}