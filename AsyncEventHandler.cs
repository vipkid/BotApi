using System.Threading.Tasks;

namespace BotApi
{
	/// <summary>
	/// Allows asynchronous event handling
	/// </summary>
	/// <typeparam name="TArgs"></typeparam>
	/// <param name="sender"></param>
	/// <param name="args"></param>
	/// <returns></returns>
	public delegate Task AsyncEventHandler<TArgs>(object sender, TArgs args);
}
