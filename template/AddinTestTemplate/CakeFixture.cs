using Cake.Core;
using Cake.Testing;
using Cake.Testing.Fixtures;

namespace $safeprojectname$
{
	public class CakeFixture : ToolFixture<CakeToolSettings>
    {
        public CakeFixture(bool scriptExists, string scriptName = "./build.cake") : base("cake.exe")
        {
            Environment = new FakeEnvironment(PlatformFamily.Linux);
            if (scriptExists) FileSystem.CreateFile(scriptName);
            ScriptName = scriptName;
        }

        private string ScriptName { get; set; }

        protected override void RunTool()
        {
            var tool = new CakeTool(FileSystem, Environment, ProcessRunner, Tools);
            tool.Run(ScriptName);
        }
    }
}
