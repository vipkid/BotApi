using BotApi.Commands.Attributes;
using BotApi.Commands.Exceptions;
using BotApi.Commands.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Utilities;

namespace BotApi.Commands.Parsing
{
	public sealed class CommandParser : ICommandParser
	{
		public ICommandFormatVerifier Verifier { get; set; } = new CommandFormatVerifier();
		public Dictionary<Type, IObjectConverter> Converters { get; }

		private Dictionary<CommandMetadata, object> _commandCache = new Dictionary<CommandMetadata, object>();

		public CommandParser()
		{
			Converters = new Dictionary<Type, IObjectConverter>();
		}

		public CommandMetadata RegisterMetadata(Type commandType, IEnumerable<string> aliases, out CommandException exception)
		{
			CommandMetadata metadata;
			if ((metadata = Verifier.TryVerifyFormat(commandType, aliases, out var ex)) == null)
			{
				exception = ex;
				return null;
			}

			exception = null;
			return metadata;
		}

		public bool RegisterObjectConverter(Type type, IObjectConverter converter)
		{
			if (Converters.ContainsKey(type))
			{
				return false;
			}

			Converters.Add(type, converter);
			return true;
		}

		public bool DeregisterObjectConverter(Type type)
		{
			return Converters.Remove(type);
		}

		public IEnumerable<CommandMetadata> GetMetadataFromInput(string input, string trigger, CommandRegistry registry, out IEnumerable<string> arguments)
		{
			if (string.IsNullOrWhiteSpace(input))
			{
				arguments = null;
				return null;
			}

			input = input.Remove(0, trigger.Length);
			if (string.IsNullOrWhiteSpace(input))
			{
				arguments = null;
				return null;
			}

			List<string> args = input.Explode();

			//Skip(1) to remove the command name
			arguments = args.Skip(1);

			IEnumerable<CommandMetadata> metadata = registry.GetMetadatas(args[0]);
			return metadata;
		}

		public ParserResult<T> PrepareCommand<T>(IEnumerable<string> arguments, CommandMetadata metadata, CommandRegistry registry, EnvironmentContext ctx, T responseTo, out CommandParsingException ex)
		{
			ParserResult<T> result = new ParserResult<T>();

			if (arguments == null)
			{
				ex = new CommandParsingException(ParserFailReason.InvalidArguments, "Arguments provided to a command may not be null.");
				return null;
			}

			if (arguments.Count() < metadata.RequiredArguments)
			{
				ex = new CommandParsingException(ParserFailReason.InvalidArguments, $"Insuffucient arguments provided. Command {metadata.TypeName} requires {metadata.RequiredArguments} {"argument".Pluralise(metadata.RequiredArguments)} but was provided with {arguments.Count()}.");
				return null;
			}

			if (arguments.Count() > 0 && metadata.HasSubcommands)
			{
				//Find the first subcommand with an alias that matches the first argument
				CommandMetadata subData = metadata.SubcommandMetadata.FirstOrDefault(m => m.Aliases.Contains(arguments.First().ToLowerInvariant()));
				if (subData != null)
				{
					//Metadata is now the subcommand
					metadata = subData;
					//Arguments have the subcommand name removed
					arguments = arguments.Skip(1);
				}
			}

			List<object> parseResult = ParseArguments(ctx, arguments, metadata, responseTo, out CommandParsingException pEx);
			if (pEx != null)
			{
				ex = new CommandParsingException(ParserFailReason.ParsingFailed, "An attempt to parse a parameter failed. See inner exception for more details.", pEx);
				return null;
			}

			result.Arguments = parseResult;
			if (_commandCache.ContainsKey(metadata))
			{
				result.Command = (CommandBase<T>)_commandCache[metadata];
			}
			else
			{
				result.Command = (CommandBase<T>)Activator.CreateInstance(metadata.Type);
				_commandCache.Add(metadata, result.Command);
			}
			result.Command.ResponseObject = responseTo;

			ex = null;
			return result;
		}

		/// <summary>
		/// Attempts to parse an enumerable string of arguments into a list of strongly
		/// typed objects that will be provided to a command's executing method
		/// </summary>
		/// <param name="arguments"></param>
		/// <param name="metadata"></param>
		/// <returns></returns>
		private List<object> ParseArguments<T>(EnvironmentContext ctx, IEnumerable<string> arguments, CommandMetadata metadata, T responseTo, out CommandParsingException ex)
		{
			List<object> objects = new List<object>() { ctx };
			int index = 0;

			foreach (KeyValuePair<ParameterInfo, CommandParameterAttribute> kvp in metadata.ParameterData)
			{
				//Get the number of arguments going in to the parameter
				int count = kvp.Value.Repetitions <= 0 ? arguments.Count() - index
														: kvp.Value.Repetitions;

				if (index >= arguments.Count())
				{
					//If we've used all our arguments, just add empty ones to satisfy the
					//method call for the command
					objects.Add(CreateEmptyArgument(kvp.Key));
					continue;
				}

				string[] args = arguments.ReadToArray(index, count);

				if (!Converters.ContainsKey(kvp.Key.ParameterType))
				{
					//Convert default
					object conversion = ConvertType(kvp.Key.ParameterType, args, ctx, responseTo, out ex);
					if (ex != null)
					{
						return null;
					}

					objects.Add(conversion);
				}
				else
				{
					//Use a defined converter
					IObjectConverter converter = Converters[kvp.Key.ParameterType];
					object conversion = converter.ConvertFromArray(args, ctx, responseTo);

					if (conversion == null)
					{
						ex = new CommandParsingException(
							ParserFailReason.ParsingFailed,
							$"Type conversion failed: Failed to convert '{string.Join(" ", args)}' to Type '{ kvp.Key.ParameterType.Name }'.",
							new Exception($"Conversion failed in '{converter.GetType().Name}.'")
						);
						return null;
					}

					objects.Add(conversion);
				}

				index += count;
			}

			ex = null;
			return objects;
		}

		/// <summary>
		/// Attempts to convert an array of strings into a specific type
		/// </summary>
		/// <param name="type"></param>
		/// <param name="arguments"></param>
		/// <returns></returns>
		private object ConvertType<T>(Type type, string[] arguments, EnvironmentContext ctx, T responseTo, out CommandParsingException ex)
		{
			string rejectionStr = $"failed to create an instance of {type.Name} from arguments '{string.Join(" ", arguments)}'.";

			if (type == typeof(string))
			{
				//strings get special treatment - why convert to a string when it already is one?
				ex = null;
				return string.Join(" ", arguments);
			}

			TypeInfo tInfo = type.GetTypeInfo();

			if (tInfo.IsValueType)
			{
				try
				{
					//value types are converted with a TypeConverter
					TypeConverter tc = TypeDescriptor.GetConverter(type);
					ex = null;
					return tc.ConvertFromString(string.Join(" ", arguments));
				}
				catch (Exception e)
				{
					ex = new CommandParsingException(ParserFailReason.ParsingFailed, $"TypeConverter {rejectionStr}", e);
					return null;
				}
			}

			if (tInfo.IsArray)
			{
				//Arrays get their own method
				Array array = CreateArray(type, arguments, ctx, responseTo, out ex);
				if (ex != null)
				{
					return null;
				}

				return array;
			}

			try
			{
				//Reference types are created with the Activator class
				ex = null;
				return Activator.CreateInstance(type, arguments);
			}
			catch (Exception e)
			{
				ex = new CommandParsingException(ParserFailReason.ParsingFailed, $"Activator {rejectionStr}", e);
				return null;
			}
		}

		/// <summary>
		/// Creates an array of the given type from the given arguments
		/// </summary>
		/// <param name="type"></param>
		/// <param name="arguments"></param>
		/// <returns></returns>
		private Array CreateArray<T>(Type type, string[] arguments, EnvironmentContext ctx, T responseTo, out CommandParsingException ex)
		{
			IObjectConverter elementConverter = null;
			Type elementType = type.GetElementType();

			if (Converters.ContainsKey(elementType))
			{
				elementConverter = Converters[elementType];
			}

			string failedConvert = $"Failed to convert '{string.Join(" ", arguments)}' to Type {type.Name}.";
			string failedCreate = $"failed to create an instance of type {elementType.Name} from argument ";

			//Create the generic array of the required size
			Array array = (Array)Activator.CreateInstance(type, arguments.Length);
			for (int i = 0; i < array.Length; i++)
			{
				if (elementConverter != null)
				{
					//if we have a converter, use it
					object conversion = elementConverter.ConvertFromString(arguments[i], ctx, responseTo);
					if (conversion == null)
					{
						ex = new CommandParsingException(
							ParserFailReason.ParsingFailed,
							failedConvert,
							new Exception($"Conversion failed by '{elementConverter.GetType().Name}.'")
						);
						return null;
					}
					array.SetValue(conversion, i);
					continue;
				}

				if (elementType == typeof(string))
				{
					//strings are special, so have special treatment
					array.SetValue(arguments[i], i);
					continue;
				}

				if (elementType.GetTypeInfo().IsValueType)
				{
					//value types can be created with a typeconverter
					TypeConverter tc = TypeDescriptor.GetConverter(type.GetElementType());
					try
					{
						//but a bad argument will throw an exception, so handle that
						array.SetValue(tc.ConvertFrom(arguments[i]), i);
					}
					catch (Exception e)
					{
						ex = new CommandParsingException(ParserFailReason.ParsingFailed, $"TypeConverter {failedCreate} '{arguments[i]}'.", e);
						return null;
					}
				}
				else
				{
					//reference types need to be created with the Activator
					try
					{
						//once again, bad arguments can throw an exception
						object element = Activator.CreateInstance(elementType, arguments[i]);
						array.SetValue(element, i);
					}
					catch (Exception e)
					{
						ex = new CommandParsingException(ParserFailReason.ParsingFailed, $"Activator {failedCreate} '{arguments[i]}'.", e);
						return null;
					}
				}
			}

			ex = null;
			return array;
		}

		/// <summary>
		/// Creates a default instance of the given parameter's type to fill an optional parameter argument
		/// </summary>
		/// <param name="paramInfo"></param>
		/// <returns></returns>
		private object CreateEmptyArgument(ParameterInfo paramInfo)
		{
			if (paramInfo.HasDefaultValue)
			{
				return paramInfo.DefaultValue;
			}

			if (paramInfo.ParameterType == typeof(string))
			{
				return string.Empty;
			}

			return Activator.CreateInstance(paramInfo.ParameterType);
		}
	}
}
