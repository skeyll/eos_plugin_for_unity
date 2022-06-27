// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.KWS
{
	/// <summary>
	/// Output parameters for the <see cref="OnPermissionsUpdateReceivedCallback" /> Function.
	/// </summary>
	public struct PermissionsUpdateReceivedCallbackInfo : ICallbackInfo
	{
		/// <summary>
		/// Context that was passed into <see cref="KWSInterface.AddNotifyPermissionsUpdateReceived" />
		/// </summary>
		public object ClientData { get; set; }

		/// <summary>
		/// Recipient Local user id
		/// </summary>
		public ProductUserId LocalUserId { get; set; }

		public Result? GetResultCode()
		{
			return null;
		}

		internal void Set(ref PermissionsUpdateReceivedCallbackInfoInternal other)
		{
			ClientData = other.ClientData;
			LocalUserId = other.LocalUserId;
		}
	}

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
	internal struct PermissionsUpdateReceivedCallbackInfoInternal : ICallbackInfoInternal, IGettable<PermissionsUpdateReceivedCallbackInfo>, ISettable<PermissionsUpdateReceivedCallbackInfo>, System.IDisposable
	{
		private System.IntPtr m_ClientData;
		private System.IntPtr m_LocalUserId;

		public object ClientData
		{
			get
			{
				object value;
				Helper.Get(m_ClientData, out value);
				return value;
			}

			set
			{
				Helper.Set(value, ref m_ClientData);
			}
		}

		public System.IntPtr ClientDataAddress
		{
			get
			{
				return m_ClientData;
			}
		}

		public ProductUserId LocalUserId
		{
			get
			{
				ProductUserId value;
				Helper.Get(m_LocalUserId, out value);
				return value;
			}

			set
			{
				Helper.Set(value, ref m_LocalUserId);
			}
		}

		public void Set(ref PermissionsUpdateReceivedCallbackInfo other)
		{
			ClientData = other.ClientData;
			LocalUserId = other.LocalUserId;
		}

		public void Set(ref PermissionsUpdateReceivedCallbackInfo? other)
		{
			if (other.HasValue)
			{
				ClientData = other.Value.ClientData;
				LocalUserId = other.Value.LocalUserId;
			}
		}

		public void Dispose()
		{
			Helper.Dispose(ref m_ClientData);
			Helper.Dispose(ref m_LocalUserId);
		}

		public void Get(out PermissionsUpdateReceivedCallbackInfo output)
		{
			output = new PermissionsUpdateReceivedCallbackInfo();
			output.Set(ref this);
		}
	}
}