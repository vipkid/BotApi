using BotApi.Commands.Interfaces;
using System.Collections.Generic;
using Utilities;

namespace BotApi.Commands.Parsing.ObjectConverters
{
	public class IntArrayObjectConverter : IObjectConverter
	{
		public object ConvertFromArray<T>(string[] arguments, EnvironmentContext ctx, T responseTo)
		{
			int[] array = new int[arguments.Length];

			for (int i = 0; i < arguments.Length; i++)
			{
				if (!int.TryParse(arguments[i], out int res))
				{
					return null;
				}
				array[i] = res;
			}

			return array;
		}

		public object ConvertFromString<T>(string argument, EnvironmentContext ctx, T responseTo)
		{
			List<string> arguments = argument.Explode();
			int[] array = new int[arguments.Count];

			for (int i = 0; i < arguments.Count; i++)
			{
				if (!int.TryParse(arguments[i], out int res))
				{
					return null;
				}
				array[i] = res;
			}

			return array;
		}
	}
}
