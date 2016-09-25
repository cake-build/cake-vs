// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Cake.VisualStudio.Helpers;
using Microsoft.VisualStudio.TaskRunnerExplorer;

namespace Cake.VisualStudio.TaskRunner
{
    [TaskRunnerExport(Constants.ScriptFileName)]
    class TaskRunner : ITaskRunner
    {
        private static ImageSource _icon;
        private List<ITaskRunnerOption> _options = null;
        private string _executablePath;

        public TaskRunner()
        {
            if (_icon == null)
            {
                string folder = GetExecutableFolder();
                _icon = new BitmapImage(new Uri(Path.Combine(folder, "Resources\\TaskRunner.png")));
            }
        }

        private void InitializeCakeRunnerOptions()
        {
            _options = new List<ITaskRunnerOption>
            {
                new TaskRunnerOption("Verbose", PackageIds.cmdVerbose, PackageGuids.guidCakePackageCmdSet, false,
                    "-Verbosity=\"Diagnostic\""),
                new TaskRunnerOption("Debug", PackageIds.cmdDebug, PackageGuids.guidCakePackageCmdSet, false, "-debug"),
                new TaskRunnerOption("Dry Run", PackageIds.cmdDryRun, PackageGuids.guidCakePackageCmdSet, false, "-dryrun")
            };
        }

        public List<ITaskRunnerOption> Options
        {
            get
            {
                if (_options == null)
                {
                    InitializeCakeRunnerOptions();
                }

                return _options;
            }
        }

        public async Task<ITaskRunnerConfig> ParseConfig(ITaskRunnerCommandContext context, string configPath)
        {
            return await Task.Run(() =>
            {
                ITaskRunnerNode hierarchy = LoadTasks(configPath);

                return new TaskRunnerConfig(context, hierarchy, _icon);
            });
        }

        private ITaskRunnerNode LoadTasks(string configPath)
        {
            string cwd = Path.GetDirectoryName(configPath);
            this._executablePath = GetCakePath(cwd);
            return string.IsNullOrWhiteSpace(_executablePath) ? NotFoundNode(configPath) : LoadHierarchy(configPath);
        }

        private ITaskRunnerNode NotFoundNode(string configPath)
        {
            var message = "Could not find Cake.exe in local folder or in PATH";
            CakePackage.Dte.ShowStatusBarText(message);
            return new TaskRunnerNode("Cake.exe not found", true)
            {
                Description = message,
                Command = new TaskRunnerCommand(Path.GetDirectoryName(configPath), "echo", message),
            };
            /*
             * return new TaskRunnerNode("Cake")
            {
                Children =
                {
                    new TaskRunnerNode("Cake.exe not found", true)
                    {
                        Description = message,
                        Command = new TaskRunnerCommand(Path.GetDirectoryName(configPath), "echo", message),
                    }
                }
            };
            */
        }

        private ITaskRunnerNode LoadHierarchy(string configPath)
        {
            string configFileName = Path.GetFileName(configPath);
            string cwd = Path.GetDirectoryName(configPath);

            ITaskRunnerNode root = new TaskRunnerNode("Cake");

            // Build
            TaskRunnerNode buildDev = CreateTask(cwd, $"Default ({configFileName})", "Runs 'cake build.cake'", configFileName);
            var tasks = TaskParser.LoadTasks(configPath);
            var commands =
                tasks.Select(
                    t =>
                        CreateTask(cwd, t.Key, $"Runs {configFileName} with the \"{t.Key}\" target",
                            buildDev.Command.Args + $" {t.Value}"));
            var nodes = commands as IList<TaskRunnerNode> ?? commands.ToList();
            buildDev.Children.AddRange(nodes);
            root.Children.Add(buildDev);
            CakePackage.Dte.ShowStatusBarText($"Loaded {nodes.Count} tasks from {configFileName}");
            return root;
        }

        private TaskRunnerNode CreateTask(string cwd, string name, string desc, string args)
        {
            var task = new TaskRunnerNode(name, true)
            {
                Description = desc,
                Command = GetCommand(cwd, args)
            };

            //ApplyOverrides(task);

            return task;
        }

        private ITaskRunnerCommand GetCommand(string cwd, string arguments)
        {
            ITaskRunnerCommand command = new TaskRunnerCommand(cwd, _executablePath, arguments);

            return command;
        }

        private static string GetCakePath(string cwd)
        {
            var knownPaths = new[] {"tools/Cake/Cake.exe", "Cake/Cake.exe", "Cake.exe"};
            foreach (var path in knownPaths)
            {
                var fullPath = Path.Combine(cwd, path);
                if (File.Exists(fullPath)) return fullPath;
            }
            if (PathHelpers.ExistsOnPath("cake.exe") || PathHelpers.ExistsOnPath("cake"))
            {
                return "cake"; // assume PATH
            }
            return null;
        }

        private static string GetExecutableFolder()
        {
            string assembly = Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(assembly);
        }
    }
}