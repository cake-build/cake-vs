using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.TaskRunnerExplorer;

namespace BrunchTaskRunner
{
    [TaskRunnerExport("brunch-config.js", "brunch-config.coffee", "config.coffee")]
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

        private void InitializeBrunchRunnerOptions()
        {
            _options = new List<ITaskRunnerOption>();
            _options.Add(new TaskRunnerOption("Debug", PackageIds.cmdDebug, PackageGuids.guidBrunchPackageCmdSet, false, "--debug"));
        }

        public List<ITaskRunnerOption> Options
        {
            get
            {
                if (_options == null)
                {
                    InitializeBrunchRunnerOptions();
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

            Telemetry.TrackEvent(configFileName.ToLowerInvariant());

            ITaskRunnerNode root = new TaskRunnerNode(Constants.TASK_CATEGORY);

            // Build
            TaskRunnerNode build = new TaskRunnerNode("Build", false);
            TaskRunnerNode buildDev = CreateTask(cwd, "build", "Runs 'brunch build'", "/c brunch build");
            build.Children.Add(buildDev);

            TaskRunnerNode buildProd = CreateTask(cwd, "build production", "Runs 'brunch build --production'", "/c brunch build -P");
            build.Children.Add(buildProd);

            root.Children.Add(build);

            // Watch
            TaskRunnerNode watch = new TaskRunnerNode("Watch", false);
            TaskRunnerNode watchDev = CreateTask(cwd, "watch", "Runs 'brunch watch'", "/c brunch watch");
            watch.Children.Add(watchDev);

            TaskRunnerNode watchProd = CreateTask(cwd, "watch production", "Runs 'brunch watch --production'", "/c brunch watch -P");
            watch.Children.Add(watchProd);

            root.Children.Add(watch);

            return root;
        }

        private TaskRunnerNode CreateTask(string cwd, string name, string desc, string args)
        {
            var task = new TaskRunnerNode(name, true)
            {
                Description = desc,
                Command = GetCommand(cwd, args)
            };

            ApplyOverrides(task);

            return task;
        }

        private void ApplyOverrides(ITaskRunnerNode parent)
        {
            var files = Directory.EnumerateFiles(parent.Command.WorkingDirectory).Where(f => f.Contains(".overrides."));

            foreach (string file in files)
            {
                int dot = file.LastIndexOf('.');
                string name = file.Substring(dot + 1);

                var task = new TaskRunnerNode($"env: {name}", true)
                {
                    Description = $"Runs '{parent.Name} --env {name}'",
                    Command = GetCommand(parent.Command.WorkingDirectory, $"{parent.Command.Args} --env {name}")
                };

                parent.Children.Add(task);
            }
        }

        private ITaskRunnerCommand GetCommand(string cwd, string arguments)
        {
            ITaskRunnerCommand command = new TaskRunnerCommand(cwd, "cmd", arguments);

            return command;
        }

        private static string GetExecutableFolder()
        {
            string assembly = Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(assembly);
        }
    }
}
