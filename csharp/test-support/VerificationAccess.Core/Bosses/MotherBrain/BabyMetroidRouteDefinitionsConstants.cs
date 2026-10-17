
namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Cartridge values of <see cref="BabyMetroidRouteDefinitions"/> that only verification reads.</summary>
internal static class BabyMetroidRouteDefinitionsConstants
{
    /// <summary>First SNES address occupied by the route records.</summary>
    public const int SourceAddress = 0xa9ca24;
    /// <summary>Byte length of one route record: target X and Y, divisor index and movement callback words.</summary>
    public const int RecordByteLength = 8;
    /// <summary>
    /// <c>$A9:CA66</c>, the latch-onto-Samus AI function whose address the cartridge reads as the
    /// word after the final record; its set sign bit is what ends the route.
    /// </summary>
    public const ushort LatchOntoSamusFunction = 0xca66;
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
