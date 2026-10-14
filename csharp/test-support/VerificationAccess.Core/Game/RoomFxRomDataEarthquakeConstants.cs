using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="RoomFxRomData.Earthquake"/> that only verification reads.</summary>
internal static class RoomFxRomDataEarthquakeConstants
{
    /// <summary>Bank-$A0 table at $872D of BG displacement records for earthquake types.</summary>
    public const int BgDisplacementTableAddress = 0xa0872d;
    /// <summary>Byte stride between consecutive earthquake displacement records.</summary>
    public const int BytesPerType = 8;

    /// <summary>Provides earthquake BG displacement table location and bytes per type for room effect decoding.</summary>
    extension(RoomFxRomData.Earthquake)
    {
        /// <inheritdoc cref="RoomFxRomDataEarthquakeConstants.BgDisplacementTableAddress"/>
        internal static int BgDisplacementTableAddress => RoomFxRomDataEarthquakeConstants.BgDisplacementTableAddress;
        /// <inheritdoc cref="RoomFxRomDataEarthquakeConstants.BytesPerType"/>
        internal static int BytesPerType => RoomFxRomDataEarthquakeConstants.BytesPerType;
    }
}
