namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Reads Phantoon's wave mode word (#1461), the eye's parameter word its setup copies: bit zero selects the 128-line
/// rather than the 64-line wave. This type is the one reader of the word's bits.
/// </summary>
[EncodingOwner("Phantoon wave mode word")]
internal static class PhantoonWaveModeWord
{
    /// <summary>The mode bit that selects the 128-line rather than the 64-line wave.</summary>
    private const int LongWaveBit = 1;

    /// <summary>The samples in half of one cycle of the wave <paramref name="mode"/> selects.</summary>
    /// <param name="mode">The wave channel's nonzero mode.</param>
    internal static int HalfCycle(int mode) => (mode & LongWaveBit) != 0
        ? PhantoonWaveDefinitions.LongHalfCycle
        : PhantoonWaveDefinitions.ShortHalfCycle;
}
