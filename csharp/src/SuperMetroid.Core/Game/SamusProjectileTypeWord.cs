namespace SuperMetroid.Core.Game;

/// <summary>A lossless view over one native projectile type/family word.</summary>
public readonly record struct SamusProjectileTypeWord(ushort Raw)
{
    /// <summary>Beam equipment combination carried by the projectile's four low bits.</summary>
    private const ushort BeamCombinationMask = 0x000f;
    /// <summary>Native marker distinguishing a charged beam from an ordinary beam payload.</summary>
    private const ushort ChargedBeamMarker = 0x0010;
    /// <summary>Four-bit projectile family in native bits 8 through 11.</summary>
    private const ushort FamilyMask = 0x0f00;
    /// <summary>High-bit marker installed for active projectiles and resident PLM triggers.</summary>
    internal const ushort LiveMarker = 0x8000;
    /// <summary>Payload bits retained when a projectile becomes a resident PLM trigger.</summary>
    private const ushort ResidentPlmPayloadMask = 0x1fff;

    /// <summary>Low-nibble beam combination; all sixteen values are defined.</summary>
    public SamusBeamCombination BeamCombination => (SamusBeamCombination)(Raw & BeamCombinationMask);
    /// <summary>Raw projectile-family nibble in its native bit position.</summary>
    public ushort FamilyValue => (ushort)(Raw & FamilyMask);
    /// <summary>Named projectile family represented by the family nibble.</summary>
    public SamusProjectileFamily Family => (SamusProjectileFamily)FamilyValue;
    /// <summary>Whether the native charged-beam marker is set.</summary>
    public bool IsChargedBeam => (Raw & ChargedBeamMarker) != 0;

    /// <summary>Tests whether the family nibble exactly matches the requested family.</summary>
    public bool IsFamily(SamusProjectileFamily family) => FamilyValue == (ushort)family;

    /// <summary>
    /// Tests the complete low twelve bits against a plain family value. Native callers use
    /// this stricter form when beam/control payload bits must also be zero, while ignoring
    /// lifecycle state in the high nibble.
    /// </summary>
    public bool HasPlainFamilyPayload(SamusProjectileFamily family) =>
        (Raw & 0x0fff) == (ushort)family;

    /// <summary>
    /// Reproduces generic PLM trigger setup <c>$84:C7E2</c>: retain projectile payload
    /// bits 0-12 and set the native live/collision marker before writing PLM_Timers.
    /// </summary>
    public ushort AsResidentPlmTriggerWord() =>
        (ushort)((Raw & ResidentPlmPayloadMask) | LiveMarker);

    /// <summary>
    /// Replaces only the verified family nibble. Every beam/control bit outside that field
    /// survives exactly, including bits whose meanings have not yet been translated.
    /// </summary>
    public ushort WithFamily(SamusProjectileFamily family) =>
        (ushort)((Raw & ~FamilyMask) | (ushort)family);

    /// <summary>
    /// Reproduces bank $90's two distinct beam constructions. Charged firing deliberately
    /// keeps only known beam equipment bits before installing the native charged/live
    /// markers; uncharged firing preserves the complete loadout word as the ROM does.
    /// </summary>
    public static ushort CreateBeam(ushort equippedBeams, bool charged) => charged
        ? (ushort)((equippedBeams & 0x100f) | LiveMarker | ChargedBeamMarker)
        : (ushort)(equippedBeams | LiveMarker);

    /// <summary>Converts a raw projectile type word to its lossless semantic view.</summary>
    public static implicit operator SamusProjectileTypeWord(ushort raw) => new(raw);
}
