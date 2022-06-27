// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.Lobby
{
	/// <summary>
	/// Input parameters for the <see cref="LobbyModification.SetMaxMembers" /> function.
	/// </summary>
	public struct LobbyModificationSetMaxMembersOptions
	{
		/// <summary>
		/// New maximum number of lobby members
		/// </summary>
		public uint MaxMembers { get; set; }
	}

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
	internal struct LobbyModificationSetMaxMembersOptionsInternal : ISettable<LobbyModificationSetMaxMembersOptions>, System.IDisposable
	{
		private int m_ApiVersion;
		private uint m_MaxMembers;

		public uint MaxMembers
		{
			set
			{
				m_MaxMembers = value;
			}
		}

		public void Set(ref LobbyModificationSetMaxMembersOptions other)
		{
			m_ApiVersion = LobbyModification.LobbymodificationSetmaxmembersApiLatest;
			MaxMembers = other.MaxMembers;
		}

		public void Set(ref LobbyModificationSetMaxMembersOptions? other)
		{
			if (other.HasValue)
			{
				m_ApiVersion = LobbyModification.LobbymodificationSetmaxmembersApiLatest;
				MaxMembers = other.Value.MaxMembers;
			}
		}

		public void Dispose()
		{
		}
	}
}