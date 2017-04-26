using BotApi.Commands.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BotApi.Commands
{
	/// <summary>
	/// Contains information about a command
	/// </summary>
	public class CommandMetadata
    {
		/// <summary>
		/// Whether or not the metadata should be reused when the command is run
		/// </summary>
		public bool UseMetadataCaching { get; set; } = true;
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
		/// A message describing this command
		/// </summary>
		public string Description { get; set; }
		/// <summary>
		/// Aliases the command can be executed with
		/// </summary>
		public IEnumerable<RegexString> Aliases { get; private set; }
		/// <summary>
		/// Metadata for each subcommand defined on this command
		/// </summary>
		public IEnumerable<CommandMetadata> SubcommandMetadata { get; private set; }
		/// <summary>
		/// Whether or not this command has subcommands
		/// </summary>
		public bool HasSubcommands => SubcommandMetadata != null && SubcommandMetadata.Count() > 0;

		public CommandMetadata(
			MethodInfo executor,
			Type type, 
			int reqArgs,
			IEnumerable<RegexString> aliases,
			string description,
			Dictionary<ParameterInfo, CommandParameterAttribute> paramData,
			IEnumerable<CommandMetadata> subcommandMetadata)
		{
			ParameterData = paramData;
			ExecutingMethod = executor;
			Type = type;
			RequiredArguments = reqArgs;
			Aliases = aliases;
			Description = description;
			SubcommandMetadata = subcommandMetadata;
		}
    }
}
