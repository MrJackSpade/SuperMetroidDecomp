namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A lossless view over one native projectile type/family word.</summary>
public readonly record struct SamusProjectileTypeWord(ushort Raw)
{
    /// <summary>Beam equipment combination carried by the projectile's four low bits.</summary>
    private const ushort BeamCombinationMask = 15;
    /// <summary>Native marker distinguishing a charged beam from an ordinary beam payload.</summary>
    internal const ushort ChargedBeamMarker = 1 << 4;
    /// <summary>Four-bit projectile family in native bits 8 through 11.</summary>
    internal const ushort FamilyMask = 15 << 8;
    /// <summary>High-bit marker installed for active projectiles and resident PLM triggers.</summary>
    internal const ushort LiveMarker = 1 << 15;
    /// <summary>Payload bits retained when a projectile becomes a resident PLM trigger.</summary>
    private const ushort ResidentPlmPayloadMask = (1 << 13) - 1;

    /// <summary>The low twelve bits: the family nibble with the beam and control payload, below the lifecycle nibble.</summary>
    private const ushort PayloadBits = (1 << 12) - 1;

    /// <summary>The equipped-beam bits a charged beam keeps: the four beam components and the Charge bit.</summary>
    internal const SamusBeamFlags EquippedBeamBits = (SamusBeamFlags.Wave | SamusBeamFlags.Ice | SamusBeamFlags.Spazer | SamusBeamFlags.Plasma | SamusBeamFlags.Charge);

    /// <summary>Low-nibble beam combination; all sixteen values are defined.</summary>
    public SamusBeamCombination BeamCombination => (SamusBeamCombination)(Raw & BeamCombinationMask);
    /// <summary>
    /// The family field in its encoded position, a multiple of 256 from 0 through 3840, for the cartridge's ordered
    /// family comparisons (explosions and later families compare at or above <see cref="SamusProjectileFamily.BeamExplosion"/>).
    /// </summary>
    public int FamilyValue => Raw & FamilyMask;
    /// <summary>Named projectile family represented by the family nibble.</summary>
    public SamusProjectileFamily Family => (SamusProjectileFamily)FamilyValue;
    /// <summary>Whether the native charged-beam marker is set.</summary>
    public bool IsChargedBeam => (Raw & ChargedBeamMarker) != 0;

    /// <summary>Whether the projectile carries any of <paramref name="beams"/> among the equipped beam bits it was fired with.</summary>
    public bool HasAnyBeam(SamusBeamFlags beams) => (Raw & (beams & EquippedBeamBits).ToNativeWord()) != 0;

    /// <summary>Tests whether the family nibble exactly matches the requested family.</summary>
    public bool IsFamily(SamusProjectileFamily family) => FamilyValue == (ushort)family;

    /// <summary>
    /// Tests the complete low twelve bits against a plain family value. Native callers use
    /// this stricter form when beam/control payload bits must also be zero, while ignoring
    /// lifecycle state in the high nibble.
    /// </summary>
    public bool HasPlainFamilyPayload(SamusProjectileFamily family) =>
        (Raw & PayloadBits) == (ushort)family;

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
    public static ushort CreateBeam(SuperMetroid.Core.Samus.Projectiles.SamusBeamFlags equippedBeams, bool charged) => charged
        ? (ushort)((equippedBeams & EquippedBeamBits).ToNativeWord() | LiveMarker | ChargedBeamMarker)
        : (ushort)(equippedBeams.ToNativeWord() | LiveMarker);

    /// <summary>Converts a raw projectile type word to its lossless semantic view.</summary>
    public static implicit operator SamusProjectileTypeWord(ushort raw) => new(raw);
}
