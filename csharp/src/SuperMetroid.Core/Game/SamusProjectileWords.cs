namespace SuperMetroid.Core.Game;

/// <summary>Verified projectile-family values stored in bits 8–11 of WRAM <c>$0C18</c>.</summary>
/// <remarks>
/// Family <c>$4</c> remains unnamed because the translated paths do not yet establish its
/// producer. Omitting it is intentional; casting its raw nibble to this enum still preserves
/// the value for diagnostics without blessing a guess as domain terminology.
/// </remarks>
public enum SamusProjectileFamily : ushort
{
    Beam = 0x0000,
    Missile = 0x0100,
    SuperMissile = 0x0200,
    PowerBomb = 0x0300,
    Bomb = 0x0500,
    BeamExplosion = 0x0700,
    MissileExplosion = 0x0800,
}

/// <summary>
/// Ten valid direction-table indices stored in a projectile direction word's low nibble.
/// The duplicated vertical directions retain Samus's facing side, which selects distinct
/// muzzle offsets even though their velocity axes match.
/// </summary>
public enum SamusProjectileDirection : byte
{
    UpFacingRight = 0,
    UpRight = 1,
    Right = 2,
    DownRight = 3,
    DownFacingRight = 4,
    DownFacingLeft = 5,
    DownLeft = 6,
    Left = 7,
    UpLeft = 8,
    UpFacingLeft = 9,
}

/// <summary>A lossless view over Samus's native equipped-beam word.</summary>
public readonly record struct SamusBeamLoadoutWord(ushort Raw)
{
    private const ushort CombinationMask = 0x000f;
    private const ushort NativeConfigurationMask = 0x0fff;

    /// <summary>
    /// Retail table index formed by the four independently combinable beam bits. Values
    /// 12–15 remain possible in corrupted/debug-edited state even though retail inventory
    /// prevents the Spazer+Plasma combinations.
    /// </summary>
    public int CombinationIndex => Raw & CombinationMask;

    /// <summary>
    /// Exact low-twelve-bit index used by native defensive table checks. Unlike
    /// <see cref="CombinationIndex"/>, this deliberately retains unknown/debug-edited bits
    /// so invalid WRAM state still takes the cartridge's rejection branch.
    /// </summary>
    public int NativeConfigurationIndex => Raw & NativeConfigurationMask;

    /// <summary>Only the equipment bits whose meanings are verified.</summary>
    public SamusBeamFlags KnownFlags => (SamusBeamFlags)(Raw & (ushort)(
        SamusBeamFlags.Wave |
        SamusBeamFlags.Ice |
        SamusBeamFlags.Spazer |
        SamusBeamFlags.Plasma |
        SamusBeamFlags.Charge));

    public bool HasAny(SamusBeamFlags flags) => (KnownFlags & flags) != 0;

    /// <summary>
    /// Replaces the four-bit retail combination index while preserving Charge and every
    /// other raw bit. Debug menus use this to emulate changing only the selected beams.
    /// </summary>
    public ushort WithCombinationIndex(int combinationIndex)
    {
        if ((uint)combinationIndex > CombinationMask)
            throw new ArgumentOutOfRangeException(nameof(combinationIndex));
        return (ushort)((Raw & ~CombinationMask) | combinationIndex);
    }

    public static implicit operator SamusBeamLoadoutWord(ushort raw) => new(raw);
}

/// <summary>A lossless view over one native projectile type/family word.</summary>
public readonly record struct SamusProjectileTypeWord(ushort Raw)
{
    private const ushort BeamCombinationMask = 0x000f;
    private const ushort ChargedBeamMarker = 0x0010;
    private const ushort SpazerSbaMarker = 0x0020;
    private const ushort FamilyMask = 0x0f00;
    private const ushort LiveMarker = 0x8000;

    public int BeamCombinationIndex => Raw & BeamCombinationMask;
    public ushort FamilyValue => (ushort)(Raw & FamilyMask);
    public SamusProjectileFamily Family => (SamusProjectileFamily)FamilyValue;
    public bool IsChargedBeam => (Raw & ChargedBeamMarker) != 0;
    public bool IsSpazerSba => (Raw & SpazerSbaMarker) != 0;
    public bool IsLive => (Raw & LiveMarker) != 0;

    public bool IsFamily(SamusProjectileFamily family) => FamilyValue == (ushort)family;

    /// <summary>
    /// Tests the complete low twelve bits against a plain family value. Native callers use
    /// this stricter form when beam/control payload bits must also be zero, while ignoring
    /// lifecycle state in the high nibble.
    /// </summary>
    public bool HasPlainFamilyPayload(SamusProjectileFamily family) =>
        (Raw & 0x0fff) == (ushort)family;

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

    public byte DirectionIndex => (byte)(Raw & DirectionMask);
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

    public static implicit operator SamusProjectileDirectionWord(ushort raw) => new(raw);
}
