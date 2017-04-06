using BotApi.Commands.Interfaces;

namespace BotApi.Commands.Parsing.ObjectConverters
{
	public sealed class StringObjectConverter : IObjectConverter
	{
		public object ConvertFromArray<T>(string[] arguments, IEnvironmentContext ctx, T responseTo)
		{
			return string.Join(" ", arguments);
		}

		public object ConvertFromString<T>(string argument, IEnvironmentContext ctx, T responseTo)
		{
			return argument;
		}
	}
}
