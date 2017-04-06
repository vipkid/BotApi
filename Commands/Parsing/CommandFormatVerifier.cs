using BotApi.Commands.Attributes;
using BotApi.Commands.Exceptions;
using BotApi.Commands.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BotApi.Commands
{
	/// <summary>
	/// Verifies commands follow the correct parameter formatting rules
	/// </summary>
	public sealed class CommandFormatVerifier : ICommandFormatVerifier
	{
		/// <summary>
		/// Verifies that the order of the parameters of a command are correct.
		/// Parameters must be ordered as follows:
		/// * An optional parameter may not come before a required parameter
		/// * An unknown length parameter must not be followed by any parameters.
		/// Further, a command must return a <see cref="CommandResult"/> or <see cref="Task{CommandResult}"/> and its first
		/// executor method parameter must be of type <see cref="IEnvironmentContext"/>.
		/// The command class must contain at least one method with a <see cref="CommandExecutorAttribute"/> or <see cref="CommandExecutorAsyncAttribute"/>.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="ex"></param>
		/// <returns></returns>
		public CommandMetadata TryVerifyFormat(Type type, IEnumerable<string> aliases, out CommandParsingException ex)
		{
			if (!typeof(ICommand).IsAssignableFrom(type))
			{
				ex = new CommandParsingException(ParserFailReason.IncorrectType, $"Cannot assign type '{type.Name}' to type '{nameof(ICommand)}'.");
				return null;
			}
			
			MethodInfo executor;
			try
			{
				executor = type.GetRuntimeMethods().First(m => m.GetCustomAttribute<CommandExecutorAttribute>() != null);
			}
			catch (Exception e)
			{
				ex = new CommandParsingException(
					ParserFailReason.NoExecutorFound,
					$"Failed to discover an executor for command '{type.Name}'.",
					e
				);
				return null;
			}

			if (executor.ReturnType != typeof(Task<CommandResult>))
			{
				ex = new CommandParsingException(
					ParserFailReason.MalformedExecutor,
					$"Executor of command '{type.Name}' does not return '{nameof(Task)}<{nameof(CommandResult)}>'."
				);
				return null;
			}

			if (executor.GetParameters().Length < 1 || !typeof(IEnvironmentContext).IsAssignableFrom(executor.GetParameters()[0].ParameterType))
			{
				ex = new CommandParsingException(
					ParserFailReason.MalformedExecutor,
					$"Executor of command '{type.Name}' does not have '{nameof(IEnvironmentContext)}' as its first parameter."
				);
				return null;
			}

			bool optionalFound = false;
			bool unknownLengthFound = false;
			int requiredArguments = 0;
			Dictionary<ParameterInfo, CommandParameterAttribute> paramData = new Dictionary<ParameterInfo, CommandParameterAttribute>();
			foreach (ParameterInfo param in executor.GetParameters().Skip(1)) //Skip 1 as the first parameter is the environment context
			{
				CommandParameterAttribute attr = param.GetCustomAttribute<CommandParameterAttribute>();

				if (attr == null)
				{
					attr = new CommandParameterAttribute(param.IsOptional);
				}

				paramData.Add(param, attr);

				if (optionalFound && !attr.Optional)
				{
					ex = new CommandParsingException(
						ParserFailReason.InvalidParameter,
						$"Parameter '{param.Name}' is required, but follows an optional parameter."
					);
					return null;
				}

				if (unknownLengthFound)
				{
					ex = new CommandParsingException(
						ParserFailReason.InvalidParameter,
						$"Parameter '{param.Name}' follows an unknown-length parameter."
					);
					return null;
				}

				if (attr.Optional)
				{
					optionalFound = true;
				}
				if (attr.Repetitions < 1)
				{
					unknownLengthFound = true;
					requiredArguments = -1;
				}
				if (!attr.Optional)
				{
					requiredArguments += attr.Repetitions;
				}
			}

			ex = null;
			return new CommandMetadata(executor, type, requiredArguments, aliases, paramData);
		}
	}
}
