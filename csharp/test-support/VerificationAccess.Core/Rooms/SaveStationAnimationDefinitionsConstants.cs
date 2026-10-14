using SuperMetroid.Core.Rooms;

/// <summary>Cartridge values of <see cref="SaveStationAnimationDefinitions"/> that only verification reads.</summary>
internal static class SaveStationAnimationDefinitionsConstants
{
    /// <summary>Native address of the compiled loop-count operand for parity verification.</summary>
    public const int NativeSaveAnimationLoopsAddress = 0x84aff9;

    /// <summary>Provides the native save station animation loop address for save sequence checks.</summary>
    extension(SaveStationAnimationDefinitions)
    {
        /// <inheritdoc cref="SaveStationAnimationDefinitionsConstants.NativeSaveAnimationLoopsAddress"/>
        internal static int NativeSaveAnimationLoopsAddress => SaveStationAnimationDefinitionsConstants.NativeSaveAnimationLoopsAddress;
    }
}
