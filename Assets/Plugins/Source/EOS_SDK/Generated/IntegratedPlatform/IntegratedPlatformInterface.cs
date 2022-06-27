// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.IntegratedPlatform
{
	public sealed partial class IntegratedPlatformInterface
	{
		public const int CreateintegratedplatformoptionscontainerApiLatest = 1;

		/// <summary>
		/// A macro to identify the Steam integrated platform.
		/// </summary>
		public static readonly Utf8String IptSteam = "STEAM";

		public const int OptionsApiLatest = 1;

		public const int SteamOptionsApiLatest = 1;

		/// <summary>
		/// Creates an integrated platform options container handle. This handle can used to add multiple options to your container which will then be applied with <see cref="Platform.PlatformInterface.Create" />.
		/// The resulting handle must be released by calling <see cref="IntegratedPlatformOptionsContainer.Release" /> once it has been passed to <see cref="Platform.PlatformInterface.Create" />.
		/// <seealso cref="IntegratedPlatformOptionsContainer.Release" />
		/// <seealso cref="Platform.PlatformInterface.Create" />
		/// <seealso cref="IntegratedPlatformOptionsContainer.Add" />
		/// </summary>
		/// <param name="options">structure containing operation input parameters.</param>
		/// <param name="outIntegratedPlatformOptionsContainerHandle">Pointer to an integrated platform options container handle to be set if successful.</param>
		/// <returns>
		/// Success if we successfully created the integrated platform options container handle pointed at in OutIntegratedPlatformOptionsContainerHandle, or an error result if the input data was invalid.
		/// </returns>
		public static Result CreateIntegratedPlatformOptionsContainer(ref CreateIntegratedPlatformOptionsContainerOptions options, out IntegratedPlatformOptionsContainer outIntegratedPlatformOptionsContainerHandle)
		{
			CreateIntegratedPlatformOptionsContainerOptionsInternal optionsInternal = new CreateIntegratedPlatformOptionsContainerOptionsInternal();
			optionsInternal.Set(ref options);

			var outIntegratedPlatformOptionsContainerHandleAddress = System.IntPtr.Zero;

			var funcResult = Bindings.EOS_IntegratedPlatform_CreateIntegratedPlatformOptionsContainer(ref optionsInternal, ref outIntegratedPlatformOptionsContainerHandleAddress);

			Helper.Dispose(ref optionsInternal);

			Helper.Get(outIntegratedPlatformOptionsContainerHandleAddress, out outIntegratedPlatformOptionsContainerHandle);

			return funcResult;
		}
	}
}