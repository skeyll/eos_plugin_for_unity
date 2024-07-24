// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.Platform
{
	/// <summary>
	/// Platform RTC options.
	/// </summary>
	public struct RTCOptions
	{
		/// <summary>
		/// This field is for platform specific initialization if any.
		/// 
		/// If provided then the structure will be located in <System>/eos_<System>.h.
		/// The structure will be named EOS_<System>_RTCOptions.
		/// </summary>
		public System.IntPtr PlatformSpecificOptions { get; set; }

		/// <summary>
		/// Configures RTC behavior upon entering to any background application statuses
		/// </summary>
		public RTCBackgroundMode BackgroundMode { get; set; }

		internal void Set(ref RTCOptionsInternal other)
		{
			PlatformSpecificOptions = other.PlatformSpecificOptions;
			BackgroundMode = other.BackgroundMode;
		}
	}

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
	internal struct RTCOptionsInternal : IGettable<RTCOptions>, ISettable<RTCOptions>, System.IDisposable
	{
		private int m_ApiVersion;
		private System.IntPtr m_PlatformSpecificOptions;
		private RTCBackgroundMode m_BackgroundMode;

		public System.IntPtr PlatformSpecificOptions
		{
			get
			{
				return m_PlatformSpecificOptions;
			}

			set
			{
				m_PlatformSpecificOptions = value;
			}
		}

		public RTCBackgroundMode BackgroundMode
		{
			get
			{
				return m_BackgroundMode;
			}

			set
			{
				m_BackgroundMode = value;
			}
		}

		public void Set(ref RTCOptions other)
		{
			m_ApiVersion = PlatformInterface.RtcoptionsApiLatest;
			PlatformSpecificOptions = other.PlatformSpecificOptions;
			BackgroundMode = other.BackgroundMode;
		}

		public void Set(ref RTCOptions? other)
		{
			if (other.HasValue)
			{
				m_ApiVersion = PlatformInterface.RtcoptionsApiLatest;
				PlatformSpecificOptions = other.Value.PlatformSpecificOptions;
				BackgroundMode = other.Value.BackgroundMode;
			}
		}

		public void Dispose()
		{
			Helper.Dispose(ref m_PlatformSpecificOptions);
		}

		public void Get(out RTCOptions output)
		{
			output = new RTCOptions();
			output.Set(ref this);
		}
	}
}