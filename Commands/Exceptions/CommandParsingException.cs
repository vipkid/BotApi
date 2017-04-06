﻿using System;

namespace BotApi.Commands.Exceptions
{
	/// <summary>
	/// Enum describing parser fail reasons
	/// </summary>
	public enum ParserFailReason
	{
		/// <summary>
		/// No input was provided to the parser
		/// </summary>
		EmptyInput,
		/// <summary>
		/// No command was found matching the description provided to the parser
		/// </summary>
		UnknownCommandName,
		/// <summary>
		/// Invalid arguments were passed to the parser when attempting to prepare a command
		/// </summary>
		InvalidArguments,
		/// <summary>
		/// An attempt to register a command occured, but the command has no executor method
		/// </summary>
		NoExecutorFound,
		/// <summary>
		/// An attempt to register a command occured, but one or more parameters are invalid
		/// </summary>
		InvalidParameter,
		/// <summary>
		/// An attempt to register a command occured, but its executor method is malformed
		/// </summary>
		MalformedExecutor,
		/// <summary>
		/// The provided Type does not inherit CommandBase
		/// </summary>
		IncorrectType,
		/// <summary>
		/// The parser failed to parse arguments for the command
		/// </summary>
		ParsingFailed
	}

	/// <summary>
	/// Exception thrown when a command fails to be parsed
	/// </summary>
	public class CommandParsingException : CommandException
	{
		/// <summary>
		/// Reason why the exception was thrown
		/// </summary>
		public ParserFailReason FailReason { get; }

		public CommandParsingException(ParserFailReason type, string message, Exception innerException = null)
			: base(message, innerException)
		{
			FailReason = type;
		}
	}
}
