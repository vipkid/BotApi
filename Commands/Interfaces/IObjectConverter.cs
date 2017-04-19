namespace BotApi.Commands.Interfaces
{
	/// <summary>
	/// Responsible from converting a string to a specific object type
	/// </summary>
    public interface IObjectConverter
    {
		/// <summary>
		/// Converts a string array to an object
		/// </summary>
		/// <param name="arguments"></param>
		/// <returns></returns>
		object ConvertFromArray<T>(string[] arguments, EnvironmentContext context, T responseTo);

		/// <summary>
		/// Converts a single string to an object
		/// </summary>
		/// <param name="argument"></param>
		/// <returns></returns>
		object ConvertFromString<T>(string argument, EnvironmentContext context, T responseTo);
    }
}
