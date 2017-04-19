using BotApi.Commands.Interfaces;
using System;

namespace BotApi.Commands
{
	/// <summary>
	/// Contains references to objects that may prove useful in the execution of a command
	/// </summary>
	public class EnvironmentContext : IEnvironmentContext
	{
		/// <summary>
		/// <see cref="ICommandParser"/> used when parsing commands
		/// </summary>
		public ICommandParser Parser { get; private set; }
		/// <summary>
		/// <see cref="CommandRegistry"/> containing registered commands
		/// </summary>
		public CommandRegistry Registry { get; set; }

		/// <summary>
		/// Generates a new <see cref="EnvironmentContext"/> with the given command parser and registry
		/// </summary>
		/// <param name="parser"></param>
		/// <param name="registry"></param>
		public EnvironmentContext(ICommandParser parser, CommandRegistry registry)
		{
			Parser = parser;
			Registry = registry;
		}
	}
}
