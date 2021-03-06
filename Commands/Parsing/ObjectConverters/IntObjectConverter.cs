﻿using BotApi.Commands.Interfaces;
namespace BotApi.Commands.Parsing.ObjectConverters
{
	public sealed class IntObjectConverter : IObjectConverter
	{
		public object ConvertFromArray<T>(string[] arguments, EnvironmentContext ctx, T responseTo)
		{
			if (!int.TryParse(string.Join(" ", arguments), out int res))
			{
				return null;
			}
			return res;
		}

		public object ConvertFromString<T>(string argument, EnvironmentContext ctx, T responseTo)
		{
			if (!int.TryParse(argument, out int res))
			{
				return null;
			}
			return res;
		}
	}
}
