﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cake.VisualStudio.Helpers;

namespace Cake.VisualStudio.TaskRunner
{
    class TaskParser
    {
        private static string _loadPattern = "#load \"";
        public static SortedList<string, string> LoadTasks(string configPath)
        {
            var list = new SortedList<string, string>();

            try
            {
                var script = new ScriptContent(configPath);
                script.Parse(_loadPattern, s => s.Replace("#load", string.Empty).Trim().Trim('"', ';'));
                var document = script.ToString();
                var r = new Regex("Task\\s*\\(\\s*\\\"(.+)\\b\\\"\\s*\\)");
                var matches = r.Matches(document);
                var taskNames = matches.Cast<Match>().Select(m => m.Groups[1].Value);
                foreach (var name in taskNames)
                {
                    list.Add(name, $"--target=\"{name}\"");
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