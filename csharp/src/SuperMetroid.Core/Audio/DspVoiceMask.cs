namespace SuperMetroid.Core.Audio;

/// <summary>A set of the eight voices, one bit per voice: the KON, KOFF, PMON, NON, EON and ENDX layout.</summary>
/// <param name="Bits">Bit <c>n</c> selects voice <c>n</c>.</param>
internal readonly record struct DspVoiceMask(byte Bits)
{
    /// <summary>No voices.</summary>
    public static DspVoiceMask None => default;

    /// <summary>All eight voices.</summary>
    public static DspVoiceMask All => new(byte.MaxValue);
}
