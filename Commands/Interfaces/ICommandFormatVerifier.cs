using BotApi.Commands.Exceptions;
using System;
using System.Collections.Generic;

namespace BotApi.Commands.Interfaces
{
	/// <summary>
	/// Represents an object that handles verifying command parameter formatting
	/// </summary>
	public interface ICommandFormatVerifier
    {
		/// <summary>
		/// Verifies that the order of the parameters of a command are correct.
		/// Parameters must be ordered as follows:
		/// * An optional parameter may not come before a required parameter
		/// * An unknown length parameter must not be followed by any parameters
		/// </summary>
		/// <param name="commandType"></param>
		/// <param name="ex"></param>
		/// <returns></returns>
		CommandMetadata TryVerifyFormat(Type commandType, IEnumerable<string> aliases, string description, out CommandParsingException ex);
	}
}
