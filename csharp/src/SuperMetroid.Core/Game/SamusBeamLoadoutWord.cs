namespace SuperMetroid.Core.Game;

/// <summary>A lossless view over Samus's native equipped-beam word.</summary>
public readonly record struct SamusBeamLoadoutWord(ushort Raw)
{
    /// <summary>Native defensive checks index by all twelve low equipped-beam bits.</summary>
    private const ushort NativeConfigurationMask = 0x0fff;

    /// <summary>
    /// Exact low-twelve-bit index used by native defensive table checks. Unlike
    /// <see cref="SamusProjectileTypeWord.BeamCombination"/>, this deliberately retains unknown/debug-edited bits
    /// so invalid WRAM state still takes the cartridge's rejection branch.
    /// </summary>
    public int NativeConfigurationIndex => Raw & NativeConfigurationMask;

    /// <summary>
    /// The combination $90:AC8D and $90:ACCD index their tables by, or null when unknown bits
    /// 4-11 make the twelve-bit index exceed $F.
    /// </summary>
    public SamusBeamCombination? Combination => NativeConfigurationIndex <= 0xf
        ? (SamusBeamCombination)NativeConfigurationIndex
        : null;

    /// <summary>The combination FireSBA ($90:CCC0) reads: the low nibble alone, ignoring bits 4-11.</summary>
    public SamusBeamCombination LowNibbleCombination => (SamusBeamCombination)(Raw & 0x000f);

    /// <summary>Only the equipment bits whose meanings are verified.</summary>
    public SamusBeamFlags KnownFlags => (SamusBeamFlags)(Raw & (ushort)(
        SamusBeamFlags.Wave |
        SamusBeamFlags.Ice |
        SamusBeamFlags.Spazer |
        SamusBeamFlags.Plasma |
        SamusBeamFlags.Charge));

    /// <summary>Tests whether any requested verified beam-equipment bits are set.</summary>
    public bool HasAny(SamusBeamFlags flags) => (KnownFlags & flags) != 0;

    /// <summary>Converts a raw equipped-beam word to its lossless semantic view.</summary>
    public static implicit operator SamusBeamLoadoutWord(ushort raw) => new(raw);
}
