using System.Collections.Generic;

namespace BotApi.Commands.Parsing
{
	/// <summary>
	/// Represents the result from parsing a command
	/// </summary>
    public sealed class ParserResult<T>
    {
		/// <summary>
		/// The command that was prepared
		/// </summary>
		public CommandBase<T> Command { get; set; }
		/// <summary>
		/// The arguments to be passed to the command.
		/// </summary>
		public List<object> Arguments { get; set; }
    }
}
