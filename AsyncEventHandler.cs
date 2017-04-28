using System.Threading.Tasks;

namespace BotApi
{
	/// <summary>
	/// Represents a method that will asynchronously handle an event when the event provides data.
	/// </summary>
	/// <typeparam name="TArgs"></typeparam>
	/// <param name="sender"></param>
	/// <param name="args"></param>
	/// <returns></returns>
	public delegate Task AsyncEventHandler<TArgs>(object sender, TArgs args);
}
