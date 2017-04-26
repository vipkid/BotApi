using System;
using System.Text.RegularExpressions;

namespace BotApi
{
	/// <summary>
	/// A string that can be a regex or not
	/// </summary>
	public class RegexString
	{
		private Regex _regex;
		private string _string;
		private Match _match;
		private bool _matches;

		/// <summary>
		/// Options describing how this RegexString will function
		/// </summary>
		public RegexStringOptions Options { get; set; } = new RegexStringOptions();

		/// <summary>
		/// Creates a new <see cref="RegexText"/> with the given string and <see cref="RegexStringOptions"/>
		/// </summary>
		/// <param name="pattern"></param>
		/// <param name="isRegex"></param>
		public RegexString(string pattern, RegexStringOptions options)
		{
			if (!options.PlainText)
			{
				if (options.EnforceMatchAtStartPosition)
				{
					pattern = "^" + pattern;
				}
				if (options.EnforceMatchAtEndPosition)
				{
					pattern = pattern + "$";
				}

				_regex = new Regex(pattern);
			}
			else
			{
				_string = pattern;
			}
		}

		/// <summary>
		/// Determines if this RegexString matches the given input
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public bool Matches(string input)
		{
			if (_regex != null)
			{
				return _matches = (_match = _regex.Match(input)).Success;
			}

			return _matches = input.StartsWith(_string);
		}

		/// <summary>
		/// Removes the matched string from the input.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public string RemoveMatchedString(string input)
		{
			if (!_matches)
			{
				return input;
			}

			if (_regex != null)
			{
				if (_match.Success)
				{
					return input.Remove(0, _match.Length);
				}

				throw new InvalidOperationException($"Matching error - {nameof(RegexString)}.{nameof(Matches)} must be called before removing the matched string.");
			}
			else
			{
				return input.Remove(0, _string.Length);
			}
		}

		public static implicit operator RegexString(string str)
		{
			return new RegexString(str, RegexStringOptions.MatchStartOptions);
		}

		public override string ToString()
		{
			if (Options.PlainText)
			{
				return _string;
			}
			return _regex.ToString();
		}
	}
}
