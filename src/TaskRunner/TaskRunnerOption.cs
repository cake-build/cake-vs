using System;
using Microsoft.VisualStudio.TaskRunnerExplorer;

namespace BrunchTaskRunner
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
