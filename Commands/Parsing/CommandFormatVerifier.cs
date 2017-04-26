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
		/// executor method parameter must be of type <see cref="EnvironmentContext"/>.
		/// The command class must contain at least one method with a <see cref="CommandExecutorAttribute"/>.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="ex"></param>
		/// <returns></returns>
		public CommandMetadata TryVerifyFormat(Type type, IEnumerable<RegexString> aliases, string description, out CommandParsingException ex)
		{
			if (!typeof(ICommand).IsAssignableFrom(type))
			{
				ex = new CommandParsingException(ParserFailReason.IncorrectType, $"Cannot assign type '{type.Name}' to type '{nameof(ICommand)}'.");
				return null;
			}

			MethodInfo executor;
			try
			{
				executor = type.GetRuntimeMethods().First(
					m => m.GetCustomAttribute<CommandExecutorAttribute>() != null
				);
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

			if (!EnsureMethodStructure(executor, type, out ex))
			{
				//Fail on bad method structure
				return null;
			}
			
			Dictionary<ParameterInfo, CommandParameterAttribute> paramData = ParseParameters(executor.GetParameters().Skip(1), out int argCount, out ex);
			if (paramData == null)
			{
				//Fail on bad parameters
				return null;
			}

			IEnumerable<MethodInfo> subExecutors = type.GetRuntimeMethods().Where(
				m => m.GetCustomAttribute<CommandSubExecutorAttribute>() != null
			);

			List<CommandMetadata> subMetadata = new List<CommandMetadata>();

			//Check every sub-executor's parameters
			foreach (MethodInfo info in subExecutors)
			{
				if (!EnsureMethodStructure(info, type, out ex))
				{
					//Fail on bad method structure
					return null;
				}

				Dictionary<ParameterInfo, CommandParameterAttribute> subParamData = ParseParameters(info.GetParameters().Skip(1), out int subArgCount, out ex);
				if (subParamData == null)
				{
					//Fail on bad parameters
					return null;
				}
				subMetadata.Add(new CommandMetadata(info, type, subArgCount, info.GetCustomAttribute<CommandSubExecutorAttribute>().SubCommands, description, subParamData, null));
			}

			return new CommandMetadata(executor, type, argCount, aliases, description, paramData, subMetadata);
		}

		/// <summary>
		/// Ensures a method conforms to some simple rules.
		/// An executor must:
		/// * Return <see cref="Task{CommandResult}"/>
		/// * Have at least 1 parameter which is of type <see cref="EnvironmentContext"/> or a child class thereof
		/// </summary>
		/// <param name="info"></param>
		/// <param name="type"></param>
		/// <param name="ex"></param>
		/// <returns></returns>
		private bool EnsureMethodStructure(MethodInfo info, Type type, out CommandParsingException ex)
		{
			//Method must return Task<CommandResult>
			if (info.ReturnType != typeof(Task<CommandResult>))
			{
				ex = new CommandParsingException(
					ParserFailReason.MalformedExecutor,
					$"Executor of command '{type.Name}' does not return '{nameof(Task)}<{nameof(CommandResult)}>'."
				);
				return false;
			}

			//Method must have at least 1 parameter which is an EnvironmentContext (or derivative thereof)
			if (info.GetParameters().Length < 1 || !typeof(EnvironmentContext).IsAssignableFrom(info.GetParameters()[0].ParameterType))
			{
				ex = new CommandParsingException(
					ParserFailReason.MalformedExecutor,
					$"Executor of command '{type.Name}' does not have '{nameof(EnvironmentContext)}' as its first parameter."
				);
				return false;
			}

			ex = null;
			return true;
		}

		/// <summary>
		/// Ensures that the parameters on an executing method are valid
		/// </summary>
		/// <param name="parameters"></param>
		/// <param name="args"></param>
		/// <param name="ex"></param>
		/// <returns></returns>
		private Dictionary<ParameterInfo, CommandParameterAttribute> ParseParameters(IEnumerable<ParameterInfo> parameters, out int args, out CommandParsingException ex)
		{
			bool optionalFound = false;
			bool unknownLengthFound = false;
			args = 0;
			Dictionary<ParameterInfo, CommandParameterAttribute> paramData = new Dictionary<ParameterInfo, CommandParameterAttribute>();

			foreach (ParameterInfo param in parameters) //Skip 1 as the first parameter is the environment context
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
					args = -1;
				}
				if (!attr.Optional)
				{
					args += attr.Repetitions;
				}
			}

			ex = null;
			return paramData;
		}
	}
}
