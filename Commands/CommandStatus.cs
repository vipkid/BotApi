namespace BotApi.Commands
{
	/// <summary>
	/// Describes the state of a command's execution
	/// </summary>
	public enum CommandStatus
	{
		/// <summary>
		/// The command executed successfully
		/// </summary>
		Success,
		/// <summary>
		/// The command executed, but had some issues
		/// </summary>
		Warning,
		/// <summary>
		/// The command failed to execute
		/// </summary>
		Error,
		/// <summary>
		/// The command was not executed
		/// </summary>
		NoExecution
	}
}
