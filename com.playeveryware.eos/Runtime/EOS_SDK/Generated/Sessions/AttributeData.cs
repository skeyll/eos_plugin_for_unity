// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.Sessions
{
	/// <summary>
	/// Contains information about both session and search parameter attribution
	/// </summary>
	public struct AttributeData
	{
		/// <summary>
		/// Name of the session attribute
		/// </summary>
		public Utf8String Key { get; set; }

		public AttributeDataValue Value { get; set; }

		internal void Set(ref AttributeDataInternal other)
		{
			Key = other.Key;
			Value = other.Value;
		}
	}

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
	internal struct AttributeDataInternal : IGettable<AttributeData>, ISettable<AttributeData>, System.IDisposable
	{
		private int m_ApiVersion;
		private System.IntPtr m_Key;
		private AttributeDataValueInternal m_Value;

		public Utf8String Key
		{
			get
			{
				Utf8String value;
				Helper.Get(m_Key, out value);
				return value;
			}

			set
			{
				Helper.Set(value, ref m_Key);
			}
		}

		public AttributeDataValue Value
		{
			get
			{
				AttributeDataValue value;
				Helper.Get(ref m_Value, out value);
				return value;
			}

			set
			{
				Helper.Set(ref value, ref m_Value);
			}
		}

		public void Set(ref AttributeData other)
		{
			m_ApiVersion = SessionsInterface.AttributedataApiLatest;
			Key = other.Key;
			Value = other.Value;
		}

		public void Set(ref AttributeData? other)
		{
			if (other.HasValue)
			{
				m_ApiVersion = SessionsInterface.AttributedataApiLatest;
				Key = other.Value.Key;
				Value = other.Value.Value;
			}
		}

		public void Dispose()
		{
			Helper.Dispose(ref m_Key);
			Helper.Dispose(ref m_Value);
		}

		public void Get(out AttributeData output)
		{
			output = new AttributeData();
			output.Set(ref this);
		}
	}
}