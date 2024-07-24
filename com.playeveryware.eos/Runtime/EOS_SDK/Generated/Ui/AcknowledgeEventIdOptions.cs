// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.UI
{
	/// <summary>
	/// Input parameters for the <see cref="UIInterface.AcknowledgeEventId" />.
	/// </summary>
	public struct AcknowledgeEventIdOptions
	{
		/// <summary>
		/// The ID being acknowledged.
		/// </summary>
		public ulong UiEventId { get; set; }

		/// <summary>
		/// The result to use for the acknowledgment.
		/// When acknowledging <see cref="Presence.JoinGameAcceptedCallbackInfo" /> this should be the
		/// result code from the JoinSession call.
		/// </summary>
		public Result Result { get; set; }
	}

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
	internal struct AcknowledgeEventIdOptionsInternal : ISettable<AcknowledgeEventIdOptions>, System.IDisposable
	{
		private int m_ApiVersion;
		private ulong m_UiEventId;
		private Result m_Result;

		public ulong UiEventId
		{
			set
			{
				m_UiEventId = value;
			}
		}

		public Result Result
		{
			set
			{
				m_Result = value;
			}
		}

		public void Set(ref AcknowledgeEventIdOptions other)
		{
			m_ApiVersion = UIInterface.AcknowledgeeventidApiLatest;
			UiEventId = other.UiEventId;
			Result = other.Result;
		}

		public void Set(ref AcknowledgeEventIdOptions? other)
		{
			if (other.HasValue)
			{
				m_ApiVersion = UIInterface.AcknowledgeeventidApiLatest;
				UiEventId = other.Value.UiEventId;
				Result = other.Value.Result;
			}
		}

		public void Dispose()
		{
		}
	}
}