// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.VisualStudio.TaskRunnerExplorer;

namespace Cake.VisualStudio.TaskRunner
{
    public class TaskRunnerOption : ITaskRunnerOption
    {
        public TaskRunnerOption(string name, uint id, Guid guid, bool isChecked, string command)
        {
            Name = name;
            Id = id;
            Guid = guid;
            Checked = isChecked;
            Command = command;
        }

        public bool Checked { get; set; }
        public uint Id { get; }
        public Guid Guid { get; }
        public string Name { get; }
        public string Command { get; set; }
    }
}