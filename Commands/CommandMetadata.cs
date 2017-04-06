using BotApi.Commands.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BotApi.Commands
{
	/// <summary>
	/// Contains information about a command
	/// </summary>
	public class CommandMetadata
    {
		/// <summary>
		/// The method that will be called when the command is executed
		/// </summary>
		public MethodInfo ExecutingMethod { get; private set; }
		/// <summary>
		/// The number of arguments that are required for the ExecutingMethod to be executed
		/// </summary>
		public int RequiredArguments { get; private set; }
		/// <summary>
		/// Key-value pairs describing each parameter of the executing method of the command
		/// </summary>
		public Dictionary<ParameterInfo, CommandParameterAttribute> ParameterData { get; private set; }
		/// <summary>
		/// The command's Type
		/// </summary>
		public Type Type { get; private set; }
		/// <summary>
		/// The command's Type name
		/// </summary>
		public string TypeName => Type.Name;
		/// <summary>
		/// Aliases the command can be executed with
		/// </summary>
		public IEnumerable<string> Aliases { get; private set; }

		public CommandMetadata(
			MethodInfo executor,
			Type type, 
			int reqArgs,
			IEnumerable<string> aliases,
			Dictionary<ParameterInfo, CommandParameterAttribute> paramData)
		{
			ParameterData = paramData;
			ExecutingMethod = executor;
			Type = type;
			RequiredArguments = reqArgs;
			Aliases = aliases;
		}
    }
}
