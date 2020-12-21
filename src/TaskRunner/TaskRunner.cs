// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Cake.VisualStudio.Configuration;
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
                var folder = GetExecutableFolder();
                _icon = new BitmapImage(new Uri(Path.Combine(folder, "Resources\\TaskRunner.png")));
            }
        }

        private void InitializeCakeRunnerOptions()
        {
            _options = new List<ITaskRunnerOption>
            {
                new TaskRunnerOption("Verbose", PackageIds.cmdVerbose, PackageGuids.guidCakePackageCmdSet, false,
                    "--verbosity=\"Diagnostic\""),
                new TaskRunnerOption("Debug", PackageIds.cmdDebug, PackageGuids.guidCakePackageCmdSet, false, "--debug"),
                new TaskRunnerOption("Dry Run", PackageIds.cmdDryRun, PackageGuids.guidCakePackageCmdSet, false, "--dryrun")
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
                var hierarchy = LoadTasks(configPath);

                return new TaskRunnerConfig(context, hierarchy, _icon);
            });
        }

        private ITaskRunnerNode LoadTasks(string configPath)
        {
            var cwd = Path.GetDirectoryName(configPath);
            _executablePath = GetCakePath(cwd);

            if (string.IsNullOrWhiteSpace(_executablePath))
            {
                return NotFoundNode(configPath);
            }
            else
            {
                return LoadHierarchy(configPath);
            }
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
        }

        private ITaskRunnerNode LoadHierarchy(string configPath)
        {
            var configFileName = Path.GetFileName(configPath);
            var cwd = Path.GetDirectoryName(configPath);

            ITaskRunnerNode root = new TaskRunnerNode("Cake");

            // Build
            var buildDev = CreateTask(cwd, $"Default ({configFileName})", "Runs 'cake build.cake'", configFileName);
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
            var locator = new ToolLocator(new List<string> { "dotnet-cake.exe", "dotnet-cake", "cake.exe" })
                .AddConfigPath(() => new ConfigurationParser(ConfigurationParser.GetConfigFilePath(cwd)))
                .AddEnvironmentVariables()
                .AddKnownPaths("tools/Cake", "Cake", ".");
            var path = locator.Locate(cwd);
            return string.IsNullOrWhiteSpace(path)
                ? PathHelpers.ExistsOnPath("cake.exe")
                    ? "cake"
                    : null
                : path;
        }

        private static string GetExecutableFolder()
        {
            var assembly = Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(assembly);
        }
    }
}