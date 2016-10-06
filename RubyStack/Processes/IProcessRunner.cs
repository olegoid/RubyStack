using System.Collections.Generic;

namespace RubyStack
{
	public interface IProcessRunner
	{
		ProcessResult Run (string path, string arguments = null, IEnumerable<int> noExceptionOnExitCodes = null);

		ProcessResult Run (string path, string arguments = null, IEnumerable<string> stdinCommands = null, IEnumerable<int> noExceptionOnExitCodes = null);

		ProcessResult RunCommand (string command, string arguments = null, CheckExitCode checkExitCode = CheckExitCode.FailIfNotSuccess);
	}
}
