namespace BotApi.Commands.Interfaces
{
	/// <summary>
	/// Represents contextual objects about the bot environment provided to a command upon execution
	/// </summary>
	public interface IEnvironmentContext
    {
		/// <summary>
		/// A CommandRegistry that the command may use
		/// </summary>
		CommandRegistry Registry { get; set; }
		/// <summary>
		/// The Registry's parser
		/// </summary>
		ICommandParser Parser { get; }
    }
}
