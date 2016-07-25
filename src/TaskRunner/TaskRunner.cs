using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.TaskRunnerExplorer;

namespace Cake.VisualStudio.TaskRunner
{
    [TaskRunnerExport("build.cake")]
    class TaskRunner : ITaskRunner
    {
        private static ImageSource _icon;
        private List<ITaskRunnerOption> _options = null;

        public TaskRunner()
        {
            if (_icon == null)
            {
                string folder = GetExecutableFolder();
                _icon = new BitmapImage(new Uri(Path.Combine(folder, "Resources\\logo.png")));
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
                ITaskRunnerNode hierarchy = LoadHierarchy(configPath);

                return new TaskRunnerConfig(context, hierarchy, _icon);
            });
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
            buildDev.Children.AddRange(commands);
            root.Children.Add(buildDev);

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
            ITaskRunnerCommand command = new TaskRunnerCommand(cwd, "cake", arguments);

            return command;
        }

        private static string GetExecutableFolder()
        {
            string assembly = Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(assembly);
        }
    }
}
