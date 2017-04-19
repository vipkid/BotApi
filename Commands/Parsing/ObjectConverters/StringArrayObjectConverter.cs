using BotApi.Commands.Interfaces;
using System;

namespace BotApi.Commands.Parsing.ObjectConverters
{
	public sealed class StringArrayObjectConverter : IObjectConverter
	{
		public object ConvertFromArray<T>(string[] arguments, EnvironmentContext ctx, T responseTo)
		{
			return arguments;
		}

		public object ConvertFromString<T>(string argument, EnvironmentContext ctx, T responseTo)
		{
			return argument.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
		}
	}
}
