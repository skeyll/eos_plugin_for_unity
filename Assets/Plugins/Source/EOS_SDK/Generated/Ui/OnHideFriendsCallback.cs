// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.UI
{
	/// <summary>
	/// Function prototype definition for callbacks passed to <see cref="UIInterface.HideFriends" />
	/// </summary>
	/// <param name="data">A <see cref="HideFriendsCallbackInfo" /> containing the output information and result</param>
	public delegate void OnHideFriendsCallback(ref HideFriendsCallbackInfo data);

	[System.Runtime.InteropServices.UnmanagedFunctionPointer(Config.LibraryCallingConvention)]
	internal delegate void OnHideFriendsCallbackInternal(ref HideFriendsCallbackInfoInternal data);
}