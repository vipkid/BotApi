using System;

namespace BotApi.Commands.Exceptions
{
	public abstract class CommandException : Exception
	{
		public CommandException(string message, Exception innerException = null) 
			: base(message, innerException)
		{
		}
	}
}
