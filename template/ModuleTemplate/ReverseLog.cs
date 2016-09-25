using System.Linq;
using Cake.Core;
using Cake.Core.Diagnostics;


namespace $safeprojectname$
{
    public sealed class ReverseLog : ICakeLog
    {
        private readonly IConsole _console;
        public Verbosity Verbosity { get; set; }

        public ReverseLog(IConsole console)
        {
            _console = console;
        }

        public void Write(Verbosity verbosity, LogLevel level, string format, params object[] args)
        {
            var message = string.Format(format, args);
            _console.WriteLine(new string(message.Reverse().ToArray()));
        }
    }
}
