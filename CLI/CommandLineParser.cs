using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BotApi.CLI
{
	/// <summary>
	/// A simple command-line parser for retrieving basic information from a command-line. Array types are not supported
	/// </summary>
	public class CommandLineParser
	{
		private List<FlagSet> _flags = new List<FlagSet>();
		private Dictionary<FlagSet, object> _results = new Dictionary<FlagSet, object>();
		private string[] _source;

		/// <summary>
		/// Resets the CommandLineParser, removing any results and flags, and clearing the source
		/// </summary>
		/// <returns></returns>
		public CommandLineParser Reset()
		{
			_flags.Clear();
			_results.Clear();
			_source = null;

			return this;
		}

		/// <summary>
		/// Adds a flag to be parsed
		/// </summary>
		/// <param name="flag">The flag to be added</param>
		/// <param name="noArgs">Whether or not the flag is followed by an argument</param>
		public CommandLineParser AddFlag(string flag, bool noArgs = false)
		{
			FlagSet flags = new FlagSet(flag) { NoArgs = noArgs };
			return AddFlags(flags);
		}

		/// <summary>
		/// Adds a range of flags to be parsed
		/// </summary>
		/// <param name="flags">The FlagSet to be added</param>
		/// <returns></returns>
		public CommandLineParser AddFlags(FlagSet flags)
		{
			if (_flags.Contains(flags))
			{
				return this;
			}

			_flags.Add(flags);

			return this;
		}

		/// <summary>
		/// Sets the source from which arguments will be parsed
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public CommandLineParser SetSource(string[] source)
		{
			_source = source;

			for (int i = 0; i < source.Length - 1; i++)
			{
				string flag = source[i].ToLowerInvariant();
				string argument = source[i + 1];

				FlagSet flags = _flags.FirstOrDefault(f => f.Contains(flag));
				if (flags == null)
				{
					continue;
				}

				if (flags.NoArgs)
				{
					_results.Add(flags, true);
				}
				else
				{
					_results.Add(flags, argument);
				}
			}

			return this;
		}

		/// <summary>
		/// Gets the result of a FlagSet, cast to the given type parameter. Array types are not supported
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="flags"></param>
		/// <returns></returns>
		public T Get<T>(FlagSet flags)
		{
			if (!_results.ContainsKey(flags))
			{
				return default(T);
			}

			object result = _results[flags];
			Type t = typeof(T);

			if (t == typeof(string))
			{
				if (result == null)
				{
					return (T)(object)string.Empty;
				}

				return (T)result;
			}

			if (t.IsValueType)
			{
				TypeConverter tc = TypeDescriptor.GetConverter(t);
				return (T)tc.ConvertFromString(result.ToString());
			}

			return (T)Activator.CreateInstance(t, result);
		}

		/// <summary>
		/// Gets the result of a flag, cast to the given type parameter. Array types are not supported
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="flag"></param>
		/// <returns></returns>
		public T Get<T>(string flag)
		{
			FlagSet flags = _flags.FirstOrDefault(f => f.Contains(flag));
			if (flags == null)
			{
				return default(T);
			}

			return Get<T>(flags);
		}

		/// <summary>
		/// Attempts to get the result of a flag, cast to the given type parameter. Array types are not supported
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="flag"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool TryGet<T>(string flag, out T value)
		{
			FlagSet flags = _flags.FirstOrDefault(f => f.Contains(flag));
			if (flags == null)
			{
				value = default(T);
				return false;
			}

			return TryGet(flags, out value);
		}

		/// <summary>
		/// Attempts to get the result of a FlagSet, cast to the given type parameter. Array types are not supported
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="flags"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool TryGet<T>(FlagSet flags, out T value)
		{
			object result = _results[flags];

			if (result == null)
			{
				//Null result shouldn't happen, but return false if it does
				value = default(T);
				return false;
			}

			Type t = typeof(T);

			//Strings get special handling because the result object is a string
			if (t == typeof(string))
			{
				if (result == null)
				{
					//Null strings shouldn't happen, but return false if it does
					value = default(T);
					return false;
				}

				value = (T)result;
				return true;
			}

			//Value types get converted with a TypeConverter
			if (t.IsValueType)
			{
				try
				{
					TypeConverter tc = TypeDescriptor.GetConverter(t);
					value = (T)tc.ConvertFrom(result);
					return true;
				}
				catch
				{
					value = default(T);
					return false;
				}
			}

			try
			{
				//Reference types get created with an Activator
				value = (T)Activator.CreateInstance(t, result);
				return true;
			}
			catch
			{
				value = default(T);
				return false;
			}
		}
	}
}
