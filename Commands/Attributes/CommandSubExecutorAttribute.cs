using System;
using System.Collections.Generic;

namespace BotApi.Commands.Attributes
{
	/// <summary>
	/// Decorates a method to mark it as a sub-executor for a command
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class CommandSubExecutorAttribute : Attribute
	{
		/// <summary>
		/// The phrase used to identify the sub-executor from a command string
		/// </summary>
		public IEnumerable<string> SubCommands { get; }

		/// <summary>
		/// Creates a new <see cref="CommandSubExecutorAttribute"/> with the given phrase
		/// </summary>
		/// <param name="subCmd"></param>
		/// <param param name="subCmds"></param>
		public CommandSubExecutorAttribute(string subCmd, params string[] subCmds)
		{
			List<string> subs = new List<string>() { subCmd };
			subs.AddRange(subCmds);
			SubCommands = subs;
		}
	}
}
