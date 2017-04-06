using System;
using System.Threading.Tasks;
using WebSocketting;

namespace BotApi
{
	/// <summary>
	/// Used to handle authentication and connection to a service
	/// </summary>
    public interface IConnectionModule
    {
		/// <summary>
		/// An asynchronous method to authenticate the bot against whatever service it is using
		/// </summary>
		/// <returns></returns>
		Task<bool> AuthenticateAsync();

		/// <summary>
		/// An asynchronous method to connect the bot to whatever service it is using
		/// </summary>
		/// <returns></returns>
		Task<bool> ConnectAsync();

		event EventHandler Connected;
		event EventHandler<DisconnectEventArgs> Disconnected;
	}
}
