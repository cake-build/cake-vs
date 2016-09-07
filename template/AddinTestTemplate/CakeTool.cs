using System.Collections.Generic;
using System.IO;
using Cake.Core;
using Cake.Core.IO;
using Cake.Core.Tooling;

namespace $safeprojectname$
{
    public class CakeTool : Tool<CakeToolSettings> {

        public CakeTool(IFileSystem fileSystem, ICakeEnvironment environment, IProcessRunner processRunner, IToolLocator tools) : base(fileSystem, environment, processRunner, tools)
        {
            FileSystem = fileSystem;
            ProcessRunner = processRunner;
        }

        private IProcessRunner ProcessRunner { get; set; }

        private IFileSystem FileSystem { get; set; }

        protected override string GetToolName() => "Cake";

        protected override IEnumerable<string> GetToolExecutableNames()
        {
            yield return "cake.exe";
            yield return "cake";
        }

        public void Run(string scriptFilePath)
        {
            if (!FileSystem.Exist((FilePath) scriptFilePath)) throw new FileNotFoundException();
            var args = new ProcessArgumentBuilder();
            args.Append(scriptFilePath);
            ProcessRunner.Start("cake.exe", new ProcessSettings {Arguments = args});
        }
    }
}