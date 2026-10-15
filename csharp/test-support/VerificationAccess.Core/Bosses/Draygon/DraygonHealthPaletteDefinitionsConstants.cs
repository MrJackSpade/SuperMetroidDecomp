
namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Cartridge values of <see cref="DraygonHealthPaletteDefinitions"/> that only verification reads.</summary>
internal static class DraygonHealthPaletteDefinitionsConstants
{
    /// <summary>
    /// <c>DraygonHealthBasedPaletteThresholds</c> at $A5:96EF. Eight reachable words are
    /// followed by an unreachable $FFFF terminator at $A5:96FF.
    /// </summary>
    public const int NativeThresholdAddress = 0xa596ef;

    /// <summary>Provides the native Draygon health thresholds for palette transition comparison.</summary>
    extension(DraygonHealthPaletteDefinitions)
    {
        /// <inheritdoc cref="DraygonHealthPaletteDefinitionsConstants.NativeThresholdAddress"/>
        internal static int NativeThresholdAddress => DraygonHealthPaletteDefinitionsConstants.NativeThresholdAddress;
    }
}
