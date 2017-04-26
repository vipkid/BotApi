﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace BotApi.Commands.Attributes
{
	/// <summary>
	/// Decorates a method to mark it as a sub-executor for a command
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class CommandSubExecutorAttribute : Attribute
	{
		/// <summary>
		/// The phrase used to identify the sub-executor from a command string
		/// </summary>
		public IEnumerable<RegexString> SubCommands { get; }

		/// <summary>
		/// Creates a new <see cref="CommandSubExecutorAttribute"/> with the given <see cref="RegexString"/> phrases
		/// </summary>
		/// <param name="subCmd"></param>
		/// <param param name="subCmds"></param>
		public CommandSubExecutorAttribute(RegexString subCmd, params RegexString[] subCmds)
		{
			List<RegexString> subs = new List<RegexString>() { subCmd };
			subs.AddRange(subCmds);
			SubCommands = subs;
		}

		/// <summary>
		/// Creates a new <see cref="CommandSubExecutorAttribute"/> with the given phrases
		/// </summary>
		/// <param name="subCmd"></param>
		/// <param name="subCmds"></param>
		public CommandSubExecutorAttribute(string subCmd, params string[] subCmds)
		{
			List<RegexString> subs = new List<RegexString>() { subCmd };
			foreach (string sub in subCmds)
			{
				subs.Add(sub);
			}

			SubCommands = subs;
		}
	}
}
