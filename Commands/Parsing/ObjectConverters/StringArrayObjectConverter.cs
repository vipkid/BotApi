using BotApi.Commands.Interfaces;
using System;

namespace BotApi.Commands.Parsing.ObjectConverters
{
	public sealed class StringArrayObjectConverter : IObjectConverter
	{
		public object ConvertFromArray<T>(string[] arguments, IEnvironmentContext ctx, T responseTo)
		{
			return arguments;
		}

		public object ConvertFromString<T>(string argument, IEnvironmentContext ctx, T responseTo)
		{
			return argument.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
		}
	}
}
