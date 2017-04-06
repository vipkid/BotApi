using BotApi.Commands.Exceptions;
using BotApi.Commands.Parsing;
using System;
using System.Collections.Generic;

namespace BotApi.Commands.Interfaces
{
	/// <summary>
	/// Represents an object that handles parsing input into commands
	/// </summary>
	public interface ICommandParser
    {
		/// <summary>
		/// Format verifier used to verify a command's parameter format
		/// </summary>
		ICommandFormatVerifier Verifier { get; set; }
		/// <summary>
		/// A dictionary of object converters used for argument parsing
		/// </summary>
		Dictionary<Type, IObjectConverter> Converters { get; }

		/// <summary>
		/// Verifies that the order of the parameters belonging to a command is correct, then adds metadata to the command.
		/// Typically this will call <see cref="ICommandFormatVerifier.TryVerifyFormat"/>
		/// </summary>
		/// <param name="commandType"></param>
		/// <param name="ex"></param>
		/// <returns></returns>
		CommandMetadata RegisterMetadata(Type commandType, IEnumerable<string> aliases, out CommandException ex);

		/// <summary>
		/// Registers a converter for converting strings to strongly typed objects
		/// </summary>
		/// <param name="type"></param>
		/// <param name="converter"></param>
		/// <returns></returns>
		bool RegisterObjectConverter(Type type, IObjectConverter converter);

		/// <summary>
		/// Deregisters a converter
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		bool DeregisterObjectConverter(Type type);

		/// <summary>
		/// Gathers metadata for commands in the given registry from the given input, and passes out an
		/// IEnumerable of strings which may be used with <see cref="PrepareCommand"/>
		/// </summary>
		/// <param name="input"></param>
		/// <param name="registry"></param>
		/// <param name="arguments"></param>
		/// <returns></returns>
		IEnumerable<CommandMetadata> GetMetadataFromInput(string input, string trigger, CommandRegistry registry, out IEnumerable<string> arguments);

		/// <summary>
		/// Attempts to parse a string of input into a result used for executing a command
		/// </summary>
		/// <param name="arguments">Arguments being passed to the command</param>
		/// <param name="metadata">Metadata of the command being prepared</param>
		/// <param name="registry">The command registry in which the metadata was found</param>
		/// <param name="ctx">The environment context passed to the command</param>
		/// <param name="responseTo">The object the command is responding to</param>
		/// <returns></returns>
		ParserResult<T> PrepareCommand<T>(IEnumerable<string> arguments, CommandMetadata metadata, CommandRegistry registry, IEnvironmentContext ctx, T responseTo, out CommandParsingException ex);
	}
}
