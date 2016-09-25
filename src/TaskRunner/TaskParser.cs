// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Cake.VisualStudio.Helpers;

namespace Cake.VisualStudio.TaskRunner
{
    class TaskParser
    {
        public static SortedList<string, string> LoadTasks(string configPath)
        {
            var list = new SortedList<string, string>();

            try
            {
                var document = File.ReadAllText(configPath);
                var r = new Regex("Task\\([\\w\"](\\w+)\\\"*\\)");
                var matches = r.Matches(document);
                var taskNames = matches.Cast<Match>().Select(m => m.Groups[1].Value);
                foreach (var name in taskNames)
                {
                    list.Add(name, $"-Target=\"{name}\"");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            return list;
        }
    }
}