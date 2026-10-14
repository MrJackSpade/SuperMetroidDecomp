using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="BabyMetroidRouteDefinitions"/> that only verification reads.</summary>
internal static class BabyMetroidRouteDefinitionsConstants
{
    /// <summary>First SNES address occupied by the route records.</summary>
    public const int SourceAddress = 0xa9ca24;
    /// <summary>
    /// Complete source length, including the terminal <c>$CA66</c> word at
    /// <c>$A9:CA64-$CA65</c> observed through the final record's overlapping +8 read.
    /// </summary>
    public const int SourceByteLength = 66;

    /// <summary>Provides the source span for the Baby Metroid route data used by extraction checks.</summary>
    extension(BabyMetroidRouteDefinitions)
    {
        /// <inheritdoc cref="BabyMetroidRouteDefinitionsConstants.SourceAddress"/>
        internal static int SourceAddress => BabyMetroidRouteDefinitionsConstants.SourceAddress;
        /// <inheritdoc cref="BabyMetroidRouteDefinitionsConstants.SourceByteLength"/>
        internal static int SourceByteLength => BabyMetroidRouteDefinitionsConstants.SourceByteLength;
    }
}
