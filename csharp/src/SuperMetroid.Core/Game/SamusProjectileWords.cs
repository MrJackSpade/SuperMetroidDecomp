namespace SuperMetroid.Core.Game;

/// <summary>Verified projectile-family values stored in bits 8–11 of WRAM <c>$0C18</c>.</summary>
/// <remarks>
/// Family <c>$4</c> remains unnamed because the translated paths do not yet establish its
/// producer. Omitting it is intentional; casting its raw nibble to this enum still preserves
/// the value for diagnostics without blessing a guess as domain terminology.
/// </remarks>
public enum SamusProjectileFamily : ushort
{
    /// <summary>Beam family, including charged and uncharged beam payload bits.</summary>
    Beam = 0x0000,
    /// <summary>Ordinary missile family.</summary>
    Missile = 0x0100,
    /// <summary>Super Missile family.</summary>
    SuperMissile = 0x0200,
    /// <summary>Power Bomb family.</summary>
    PowerBomb = 0x0300,
    /// <summary>Ordinary bomb family.</summary>
    Bomb = 0x0500,
    /// <summary>Beam-impact explosion family.</summary>
    BeamExplosion = 0x0700,
    /// <summary>Missile-impact explosion family.</summary>
    MissileExplosion = 0x0800,
}

/// <summary>
/// Ten valid direction-table indices stored in a projectile direction word's low nibble.
/// The duplicated vertical directions retain Samus's facing side, which selects distinct
/// muzzle offsets even though their velocity axes match.
/// </summary>
public enum SamusProjectileDirection : byte
{
    /// <summary>Vertical upward direction retaining right-facing muzzle placement.</summary>
    UpFacingRight = 0,
    /// <summary>Diagonal up-right direction.</summary>
    UpRight = 1,
    /// <summary>Horizontal right direction.</summary>
    Right = 2,
    /// <summary>Diagonal down-right direction.</summary>
    DownRight = 3,
    /// <summary>Vertical downward direction retaining right-facing muzzle placement.</summary>
    DownFacingRight = 4,
    /// <summary>Vertical downward direction retaining left-facing muzzle placement.</summary>
    DownFacingLeft = 5,
    /// <summary>Diagonal down-left direction.</summary>
    DownLeft = 6,
    /// <summary>Horizontal left direction.</summary>
    Left = 7,
    /// <summary>Diagonal up-left direction.</summary>
    UpLeft = 8,
    /// <summary>Vertical upward direction retaining left-facing muzzle placement.</summary>
    UpFacingLeft = 9,
}

/// <summary>A lossless view over Samus's native equipped-beam word.</summary>
public readonly record struct SamusBeamLoadoutWord(ushort Raw)
{
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

/// <summary>A lossless view over one native projectile type/family word.</summary>
public readonly record struct SamusProjectileTypeWord(ushort Raw)
{
    private const ushort BeamCombinationMask = 0x000f;
    private const ushort ChargedBeamMarker = 0x0010;
    private const ushort FamilyMask = 0x0f00;
    private const ushort LiveMarker = 0x8000;
    private const ushort ResidentPlmPayloadMask = 0x1fff;

    /// <summary>Gets the low-nibble beam combination; all sixteen values are defined.</summary>
    public SamusBeamCombination BeamCombination => (SamusBeamCombination)(Raw & BeamCombinationMask);
    /// <summary>Gets the raw projectile-family nibble in its native bit position.</summary>
    public ushort FamilyValue => (ushort)(Raw & FamilyMask);
    /// <summary>Gets the named projectile family represented by the family nibble.</summary>
    public SamusProjectileFamily Family => (SamusProjectileFamily)FamilyValue;
    /// <summary>Gets whether the native charged-beam marker is set.</summary>
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

/// <summary>A lossless view over one native projectile direction/lifecycle word.</summary>
public readonly record struct SamusProjectileDirectionWord(ushort Raw)
{
    private const ushort DirectionMask = 0x000f;
    private const ushort LowByteLifecycleMask = 0x00f0;

    /// <summary>
    /// Bit written by the common enemy and enemy-projectile collision walkers to make the
    /// projectile's own pre-instruction dispose of or otherwise react to the contact.
    /// </summary>
    private const ushort CollisionLifecycleState = 0x0010;

    /// <summary>Gets the low-nibble native direction-table index.</summary>
    public byte DirectionIndex => (byte)(Raw & DirectionMask);
    /// <summary>Gets the named direction represented by the low nibble.</summary>
    public SamusProjectileDirection Direction => (SamusProjectileDirection)DirectionIndex;

    /// <summary>
    /// Native pre-instructions treat any state in bits 4–7 as a lifecycle transition and
    /// delete or skip the ordinary movement path. No individual meaning is assigned here.
    /// </summary>
    public bool HasLowByteLifecycleState => (Raw & LowByteLifecycleMask) != 0;

    /// <summary>
    /// Firing and missile movement accept only indices 0–9 with no lifecycle bits in the
    /// rest of the low byte. High-byte state remains untouched and intentionally unnamed.
    /// </summary>
    public bool IsValidInitialDirection =>
        !HasLowByteLifecycleState && DirectionIndex <= 9;

    /// <summary>
    /// Returns the native direction word after installing collision lifecycle state
    /// <c>$10</c>, without altering its low-nibble direction or unrelated high-byte state.
    /// </summary>
    public ushort WithCollisionLifecycleState() =>
        unchecked((ushort)(Raw | CollisionLifecycleState));
}
