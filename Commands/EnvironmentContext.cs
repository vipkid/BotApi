using BotApi.Commands.Interfaces;
using System;

namespace BotApi.Commands
{
	public class EnvironmentContext : MarshalByRefObject, IEnvironmentContext
	{
		/// <summary>
		/// <see cref="ICommandParser"/> used when parsing commands
		/// </summary>
		public ICommandParser Parser { get; private set; }
		/// <summary>
		/// <see cref="CommandRegistry"/> containing registered commands
		/// </summary>
		public CommandRegistry Registry { get; set; }

		public EnvironmentContext(ICommandParser parser, CommandRegistry registry)
		{
			Parser = parser;
			Registry = registry;
		}
	}
}
