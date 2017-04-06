using System.Collections.Generic;

namespace BotApi.Commands
{
	/// <summary>
	/// Describes the result of a command's execution
	/// </summary>
    public sealed class CommandResult
    {
		/// <summary>
		/// Status of the command execution
		/// </summary>
		public CommandStatus Status { get; set; }
		/// <summary>
		/// Short description of the status
		/// </summary>
		public string ShortReason { get; set; }
		/// <summary>
		/// Long description of the status
		/// </summary>
		public string LongReason { get; set; }
		/// <summary>
		/// An exception that may describe a failure status
		/// </summary>
		public System.Exception Exception { get; set; }
		/// <summary>
		/// Any warnings that occured as a result of command execution
		/// </summary>
		public List<string> Warnings { get; set; } = new List<string>();

		/// <summary>
		/// Creates a new instance of <see cref="CommandResult"/> with the given <see cref="CommandStatus"/>
		/// </summary>
		/// <param name="status"></param>
		public CommandResult(CommandStatus status)
		{
			Status = status;
		}
    }
}
