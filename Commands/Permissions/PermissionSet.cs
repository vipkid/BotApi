using System.Collections;
using System.Collections.Generic;

namespace BotApi.Commands.Permissions
{
	/// <summary>
	/// Maintains a set of permissions
	/// </summary>
	public class PermissionSet : IEnumerable<string>, ICollection<string>
	{
		internal HashSet<string> _permissions = new HashSet<string>();

		public int Count => _permissions.Count;

		public bool IsReadOnly => false;

		/// <summary>
		/// Adds a permission to the set
		/// </summary>
		/// <param name="permission"></param>
		public void Add(string item)
		{
			if (string.IsNullOrWhiteSpace(item))
			{
				return;
			}

			_permissions.Add(item);
		}

		/// <summary>
		/// Removes all permissions from the set
		/// </summary>
		public void Clear()
		{
			_permissions.Clear();
		}

		/// <summary>
		/// Copies the permission set to an array
		/// </summary>
		/// <param name="array"></param>
		/// <param name="arrayIndex"></param>
		public void CopyTo(string[] array, int arrayIndex)
		{
			_permissions.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Removes a permission from the set
		/// </summary>
		/// <param name="permission"></param>
		public bool Remove(string item)
		{
			return _permissions.Remove(item);
		}

		/// <summary>
		/// Determines if this permission set contains all the permissions of another set
		/// </summary>
		/// <param name="set"></param>
		/// <returns></returns>
		public bool ContainsSet(PermissionSet set)
		{
			if (set == null)
			{
				return true;
			}

			return set._permissions.IsSubsetOf(_permissions);
		}

		/// <summary>
		/// Determines if a single permission is contained in the set
		/// </summary>
		/// <param name="permission"></param>
		/// <returns></returns>
		public bool Contains(string permission)
		{
			if (string.IsNullOrWhiteSpace(permission))
			{
				return true;
			}

			return _permissions.Contains(permission);
		}

		public IEnumerator<string> GetEnumerator()
		{
			return ((IEnumerable<string>)_permissions).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<string>)_permissions).GetEnumerator();
		}
	}
}
