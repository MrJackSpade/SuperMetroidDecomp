using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>A set of the eight voices, one bit per voice: the KON, KOFF, PMON, NON, EON and ENDX layout.</summary>
/// <param name="Bits">Bit <c>n</c> selects voice <c>n</c>.</param>
internal readonly record struct DspVoiceMask(int Bits)
{
    /// <summary>Bit <c>n</c> selects voice <c>n</c>; 0 through 255.</summary>
    public int Bits { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, Bits);

    /// <summary>No voices.</summary>
    public static DspVoiceMask None => default;

    /// <summary>All eight voices.</summary>
    public static DspVoiceMask All => new(byte.MaxValue);
}
