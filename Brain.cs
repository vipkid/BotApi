using BotApi.Commands;
using BotApi.Commands.Exceptions;
using BotApi.Commands.Interfaces;
using BotApi.Commands.Parsing;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BotApi
{
	/// <summary>
	/// Manages major functionality of a bot
	/// </summary>
    public abstract class Brain : IDisposable
    {
		/// <summary>
		/// CommandRegistry used to contain bot commands
		/// </summary>
		public CommandRegistry Registry { get; set; }
		/// <summary>
		/// ICommandParser for parsing commands
		/// </summary>
		public ICommandParser CommandParser { get; set; } = new CommandParser();
		/// <summary>
		/// ICommandContext passed to all command executors
		/// </summary>
		public EnvironmentContext EnvironmentContext { get; set; }
		/// <summary>
		/// IConnectionModule used for authenticating and connecting the bot to a service
		/// </summary>
		public IConnectionModule Connector { get; set; }

		private ManualResetEvent _disconnect = new ManualResetEvent(false);
		private Action _connectCallback;

		private bool _hasInitialized = false;

		/// <summary>
		/// Initializes the bot, creating a default <see cref="ICommandParser"/>, and a <see cref="CommandRegistry"/>
		/// </summary>
		/// <param name="cmdContext"></param>
		/// <param name="connector"></param>
		public virtual void Initialize(IConnectionModule connector, Action connectCallback)
		{
			CommandParser = new CommandParser();
			Registry = new CommandRegistry(CommandParser);

			_connectCallback = connectCallback;
			Connector = connector;
			Connector.Connected += Connector_Connected;

			_hasInitialized = true;
		}

		/// <summary>
		/// Sets the <see cref="EnvironmentContext"/> used by commands
		/// </summary>
		/// <param name="context"></param>
		public void SetContext(EnvironmentContext context)
		{
			EnvironmentContext = context;
		}

		/// <summary>
		/// Registers a command that may be invoked by this brain
		/// </summary>
		/// <param name="cmdType">Command's type</param>
		/// <param name="aliases">Aliases used for executing the command</param>
		/// <param name="exception">Any exceptions thrown while attempting to parse the command type</param>
		/// <returns></returns>
		public bool RegisterCommand(Type cmdType, IEnumerable<string> aliases, string description, out CommandException exception) => 
			Registry.RegisterCommand(cmdType, aliases, description, out exception);
		
		/// <summary>
		/// Synchronously attempts to connect the bot via its <see cref="IConnectionModule"/>, then blocks the calling thread until
		/// the bot is disconnected.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if a call to <see cref="Initialize"/> has not occured</exception>
		public void ConnectAndWait()
		{
			if (!_hasInitialized)
			{
				throw new InvalidOperationException("The brain must be initialized before connecting.", new InvalidOperationException($"{nameof(Brain)}.{nameof(Initialize)} has not been called."));
			}

			try
			{
				ConnectAsync().GetAwaiter().GetResult();
				_disconnect.WaitOne();
			}
			catch (Exception ex)
			{
				Console.WriteLine("Websocket disconnected: " + ex);
			}
		}

		/// <summary>
		/// Asychronously attempts to connect the bot via its <see cref="IConnectionModule"/>
		/// </summary>
		/// <returns>True if authentication and connection succeeds</returns>
		/// <exception cref="InvalidOperationException">Thrown if a call to <see cref="Initialize"/> has not occured</exception>
		public virtual async Task<bool> ConnectAsync()
		{
			if (!_hasInitialized)
			{
				throw new InvalidOperationException("The brain must be initialized before connecting.", new InvalidOperationException($"{nameof(Brain)}.{nameof(Initialize)} has not been called."));
			}

			if (!(await Connector.AuthenticateAsync().ConfigureAwait(false)))
			{
				return false;
			}

			return await Connector.ConnectAsync().ConfigureAwait(false);
		}

		public void Disconnect()
		{
			_disconnect.Set();
		}

		public virtual void Dispose()
		{
			_disconnect.Set();
			CommandParser = null;
			Registry = null;
			EnvironmentContext = null;
		}

		private void Connector_Connected(object sender, EventArgs e)
		{
			_connectCallback?.Invoke();
		}
	}
}
