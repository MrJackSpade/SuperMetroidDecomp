
namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Cartridge values of <see cref="DraygonHealthBandDefinitions"/> that only verification reads.</summary>
internal static class DraygonHealthBandDefinitionsConstants
{
    /// <summary>
    /// <c>DraygonHealthBasedPaletteThresholds</c> at $A5:96EF. Eight reachable words are
    /// followed by an unreachable $FFFF terminator at $A5:96FF.
    /// </summary>
    public const int NativeThresholdAddress = 0xa596ef;

    /// <summary>Provides the native Draygon health thresholds for palette transition comparison.</summary>
    extension(DraygonHealthBandDefinitions)
    {
        /// <inheritdoc cref="DraygonHealthBandDefinitionsConstants.NativeThresholdAddress"/>
        internal static int NativeThresholdAddress => DraygonHealthBandDefinitionsConstants.NativeThresholdAddress;
    }
}
