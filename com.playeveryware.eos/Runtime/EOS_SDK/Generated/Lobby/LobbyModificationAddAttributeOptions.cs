// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.Lobby
{
	/// <summary>
	/// Input parameters for the <see cref="LobbyModification.AddAttribute" /> function.
	/// </summary>
	public struct LobbyModificationAddAttributeOptions
	{
		/// <summary>
		/// Key/Value pair describing the attribute to add to the lobby
		/// </summary>
		public AttributeData? Attribute { get; set; }

		/// <summary>
		/// Is this attribute public or private to the lobby and its members
		/// </summary>
		public LobbyAttributeVisibility Visibility { get; set; }
	}

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
	internal struct LobbyModificationAddAttributeOptionsInternal : ISettable<LobbyModificationAddAttributeOptions>, System.IDisposable
	{
		private int m_ApiVersion;
		private System.IntPtr m_Attribute;
		private LobbyAttributeVisibility m_Visibility;

		public AttributeData? Attribute
		{
			set
			{
				Helper.Set<AttributeData, AttributeDataInternal>(ref value, ref m_Attribute);
			}
		}

		public LobbyAttributeVisibility Visibility
		{
			set
			{
				m_Visibility = value;
			}
		}

		public void Set(ref LobbyModificationAddAttributeOptions other)
		{
			m_ApiVersion = LobbyModification.LobbymodificationAddattributeApiLatest;
			Attribute = other.Attribute;
			Visibility = other.Visibility;
		}

		public void Set(ref LobbyModificationAddAttributeOptions? other)
		{
			if (other.HasValue)
			{
				m_ApiVersion = LobbyModification.LobbymodificationAddattributeApiLatest;
				Attribute = other.Value.Attribute;
				Visibility = other.Value.Visibility;
			}
		}

		public void Dispose()
		{
			Helper.Dispose(ref m_Attribute);
		}
	}
}