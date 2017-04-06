using System;
using System.Collections.Generic;
using System.Linq;
using BotApi.Commands.Exceptions;
using BotApi.Commands.Interfaces;
using BotApi.Commands.Parsing.ObjectConverters;
using BotApi.Commands.Parsing;
using System.Threading.Tasks;
using Utilities;

namespace BotApi.Commands
{
	/// <summary>
	/// Responsible for storing and retrieving commands
	/// </summary>
	public sealed class CommandRegistry
	{
		private List<CommandMetadata> _commands;
		public ICommandParser Parser { get; }

		/// <summary>
		/// Creates a new instance of a <see cref="CommandRegistry"/>
		/// </summary>
		public CommandRegistry(ICommandParser parser)
		{
			_commands = new List<CommandMetadata>();

			Parser = parser;
			Parser.RegisterObjectConverter(typeof(string[]), new StringArrayObjectConverter());
			Parser.RegisterObjectConverter(typeof(string), new StringObjectConverter());
			Parser.RegisterObjectConverter(typeof(int), new IntObjectConverter());
		}

		/// <summary>
		/// Registers a new command type into the registry, along with any arguments required for constructing it
		/// </summary>
		/// <param name="commandType">Type of the command to be registered</param>
		/// <param name="e">CommandException that may occur while parsing the command type</param>
		/// <param name="cctorArgs">Any constructor arguments used to construct the command</param>
		/// <returns>true if registration succeeds</returns>
		public bool RegisterCommand(Type commandType, IEnumerable<string> aliases, out CommandException e)
		{
			CommandMetadata metadata;
			if ((metadata = Parser.RegisterMetadata(commandType, aliases, out e)) == null)
			{
				return false;
			}

			_commands.Add(metadata);
			e = null;
			return true;
		}

		/// <summary>
		/// Attempts to find and execute a command using the given input.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="input">Whole text to generate the command with</param>
		/// <param name="trigger">Trigger to be removed from the text</param>
		/// <param name="ctx">EnvironmentContext to be provided to the command</param>
		/// <param name="responseTo">The object which caused the command to be invoked</param>
		/// <returns></returns>
		public async Task<CommandResult> RunCommandAsync<T>(string input, string trigger, IEnvironmentContext ctx, T responseTo)
		{
			if (string.IsNullOrWhiteSpace(input))
			{
				return new CommandResult(CommandStatus.Error)
				{
					ShortReason = "Invalid input",
					LongReason = "No valid input detected - an empty string cannot be used to run a command."
				};
			}

			IEnumerable<CommandMetadata> metadatas = Parser.GetMetadataFromInput(input, trigger, this, out IEnumerable<string> args);
			if (metadatas == null || metadatas.Empty())
			{
				return new CommandResult(CommandStatus.Error)
				{
					ShortReason = "Unknown command",
					LongReason = $"No command metadata found capable of parsing '{input}'."
				};
			}

			CommandMetadata metadata = metadatas.First();
			ParserResult<T> parseResult = Parser.PrepareCommand(args, metadata, this, ctx, responseTo, out CommandParsingException ex);

			if (ex != null)
			{
				return new CommandResult(CommandStatus.Error)
				{
					ShortReason = "Command parsing failed",
					LongReason = ex.BuildErrorString()
				};
			}

			CommandResult result;
			try
			{
				result = await Task.Run(() => (Task<CommandResult>)metadata.ExecutingMethod.Invoke(parseResult.Command, parseResult.Arguments.ToArray())).ConfigureAwait(false);
			}
			catch (Exception e)
			{
				return new CommandResult(CommandStatus.Error)
				{
					ShortReason = "Command execution failed",
					LongReason = e.BuildErrorString(),
					Exception = e
				};
			}

			if (metadatas.Count() > 1)
			{
				result.Status = CommandStatus.Warning;
				result.Warnings.Add($"Multiple commands matched search string '{input.Split(' ')[0]}': {string.Join(", ", metadatas.Select(m => m.TypeName))}");
			}

			return result;
		}

		/// <summary>
		/// Returns the first command metadata that has one or more of the given aliases
		/// </summary>
		/// <param name="aliases"></param>
		/// <returns></returns>
		public CommandMetadata GetMetadata(params string[] aliases)
		{
			return _commands.FirstOrDefault(c => c.Aliases.Intersect(aliases).Count() > 0);
		}

		/// <summary>
		/// Returns all command metadata that has one or more of the given aliases
		/// </summary>
		/// <param name="aliases"></param>
		/// <returns></returns>
		public IEnumerable<CommandMetadata> GetMetadatas(params string[] aliases)
		{
			return _commands.Where(c => c.Aliases.Intersect(aliases).Count() > 0);
		}
	}
}
