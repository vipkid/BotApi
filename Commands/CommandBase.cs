using BotApi.Commands.Interfaces;
using BotApi.Commands.Permissions;
using System;
using System.Collections.Generic;

namespace BotApi.Commands
{
	/// <summary>
	/// Represents a command that may be executed. The type parameter represents the type of object that results in this command being invoked
	/// </summary>
	public abstract class CommandBase<T> : ICommand, IDisposable
    {
		/// <summary>
		/// Metadata belonging to this command
		/// </summary>
		public CommandMetadata Metadata { get; internal set; }
		/// <summary>
		/// The list of aliases the command goes by
		/// </summary>
		public IEnumerable<RegexString> Aliases => Metadata.Aliases;
		/// <summary>
		/// The object which caused this command to be invoked
		/// </summary>
		public T ResponseObject { get; set; }
		/// <summary>
		/// The permissions required to use the command
		/// </summary>
		public virtual PermissionSet Permissions { get; set; } = new PermissionSet();

		public virtual void Dispose()
		{
			Metadata = null;
		}
	}
}
