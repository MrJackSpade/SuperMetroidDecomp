using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Samus's equipped beams as the beam-selection tables read them. The beam set stays typed, including bits whose
/// meaning is unknown, so an invalid or debug-edited loadout still takes the cartridge's rejection branch.
/// </summary>
/// <param name="Beams">The equipped beam set, unknown bits included.</param>
public readonly record struct SamusBeamLoadoutWord(SamusBeamFlags Beams)
{
    /// <summary>Native defensive checks index by all twelve low equipped-beam bits.</summary>
    private const ushort ConfigurationMask = (1 << 12) - 1;

    /// <summary>The four beam components whose combination selects a beam table row.</summary>
    private const ushort CombinationMask = 15;

    /// <summary>The equipment bits whose meanings are verified.</summary>
    private const SamusBeamFlags VerifiedBeams =
        SamusBeamFlags.Wave |
        SamusBeamFlags.Ice |
        SamusBeamFlags.Spazer |
        SamusBeamFlags.Plasma |
        SamusBeamFlags.Charge;

    /// <summary>
    /// Exact low-twelve-bit index used by native defensive table checks. Unlike
    /// <see cref="SamusProjectileTypeWord.BeamCombination"/>, this deliberately retains unknown/debug-edited bits
    /// so invalid WRAM state still takes the cartridge's rejection branch.
    /// </summary>
    public int ConfigurationIndex => Beams.ToEquipmentWord() & ConfigurationMask;

    /// <summary>
    /// The combination $90:AC8D and $90:ACCD index their tables by, or null when unknown bits
    /// 4-11 make the twelve-bit index exceed $F.
    /// </summary>
    public SamusBeamCombination? Combination => ConfigurationIndex <= CombinationMask
        ? SamusBeamCombinations.FromTableIndex(ConfigurationIndex)
        : null;

    /// <summary>The combination FireSBA ($90:CCC0) reads: the low nibble alone, ignoring bits 4-11.</summary>
    public SamusBeamCombination LowNibbleCombination => SamusBeamCombinations.FromTableIndex(Beams.ToEquipmentWord() & CombinationMask);

    /// <summary>Only the equipment bits whose meanings are verified.</summary>
    public SamusBeamFlags KnownFlags => Beams & VerifiedBeams;

    /// <summary>Tests whether any requested verified beam-equipment bits are set.</summary>
    public bool HasAny(SamusBeamFlags flags) => (KnownFlags & flags) != SamusBeamFlags.None;

    /// <summary>Views a beam set through its loadout word.</summary>
    public static implicit operator SamusBeamLoadoutWord(SamusBeamFlags beams) => new(beams);
}
