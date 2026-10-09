namespace SuperMetroid.Core.Game;

/// <summary>
/// Independently combinable bits in the native enemy-property word. Only meanings already
/// exercised by translated code are named; unknown cartridge bits remain raw rather than
/// receiving plausible-but-unverified labels.
/// </summary>
[Flags]
public enum EnemyProperties : ushort
{
    /// <summary>Suppresses ordinary enemy drawing while leaving the slot's AI and collision state resident.</summary>
    Invisible = 0x0100,

    /// <summary>Marks the physical slot as deleted so the room-enemy passes no longer process it.</summary>
    Deleted = 0x0200,

    /// <summary>Excludes the enemy from the ordinary touch-collision pass against Samus.</summary>
    IgnoreSamusCollision = 0x0400,

    /// <summary>Allows enemy AI and instructions to run when the actor lies outside the active screen bounds.</summary>
    ProcessOffScreen = 0x0800,

    /// <summary>Prevents Plasma Beam projectiles from passing through the enemy after a hit.</summary>
    BlocksPlasmaBeam = 0x1000,

    /// <summary>Enables execution of the enemy's timed instruction-list stream.</summary>
    ProcessInstructions = 0x2000,

    /// <summary>
    /// Native property $4000. Enemy death retains the physical slot as definition $DAFF,
    /// and the death-projectile instruction stream later reconstructs it from the immutable
    /// population/spawn record. Rinka is the first translated family that exercises this
    /// lifecycle, so the bit now has directly observed behavior rather than a speculative name.
    /// </summary>
    RespawnIfKilled = 0x4000,

    /// <summary>
    /// Makes the actor participate in the solid-enemy collision pass even while unfrozen.
    /// Frozen enemies enter that pass independently of this flag.
    /// </summary>
    SolidToSamus = 0x8000,
}

/// <summary>
/// Independently combinable bits in the native enemy extra-property word whose behavior is
/// already proven by the live room-enemy path.
/// </summary>
[Flags]
public enum EnemyExtraProperties : ushort
{
    /// <summary>No translated extra-property behavior is enabled.</summary>
    None = 0,

    /// <summary>
    /// Selects the extended-spritemap path and admits the actor even when its ordinary
    /// radius-based visibility test would reject it.
    /// </summary>
    UsesExtendedSpritemap = 0x0004,

    /// <summary>Set when the instruction interpreter installs a new timed frame.</summary>
    NewInstructionFrame = 0x8000,
}

/// <summary>
/// Semantic operations over raw enemy words. Keeping the underlying slot fields as
/// <see cref="ushort"/> preserves their one-to-one WRAM/debugger representation.
/// </summary>
public static class EnemyPropertyFlagExtensions
{
    /// <summary>
    /// All independently composable high-byte bits in a cartridge enemy-property word.
    /// The low byte remains family-owned data and is deliberately preserved by every helper.
    /// </summary>
    public const ushort KnownPropertyFlagMask = 0xff00;

    /// <summary>
    /// Reads the typed high-byte flags while retaining the family-specific low byte in the
    /// caller's raw word. This is the explicit escape hatch for lossless cartridge records.
    /// </summary>
    public static EnemyProperties ReadFlagsChecked(this ushort word)
    {
        ushort highByte = unchecked((ushort)(word & KnownPropertyFlagMask));
        EnemyProperties flags = (EnemyProperties)highByte;
        if ((highByte & ~(ushort)(EnemyProperties.Invisible |
                EnemyProperties.Deleted |
                EnemyProperties.IgnoreSamusCollision |
                EnemyProperties.ProcessOffScreen |
                EnemyProperties.BlocksPlasmaBeam |
                EnemyProperties.ProcessInstructions |
                EnemyProperties.RespawnIfKilled |
                EnemyProperties.SolidToSamus)) != 0)
        {
            throw new InvalidDataException(
                $"Enemy property word ${word:X4} contains an unnamed high-byte flag.");
        }

        return flags;
    }

    /// <summary>Tests whether a raw enemy-property word contains at least one requested named flag.</summary>
    public static bool HasAny(this ushort word, EnemyProperties flags) =>
        (word.ReadFlagsChecked() & flags) != 0;

    /// <summary>Sets named enemy-property bits while preserving all other native word data.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="flags"/> contains low-byte or undefined bits.</exception>
    public static ushort With(this ushort word, EnemyProperties flags)
    {
        ValidateKnown(flags);
        return unchecked((ushort)(word | (ushort)flags));
    }

    /// <summary>Clears named enemy-property bits while preserving all other native word data.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="flags"/> contains low-byte or undefined bits.</exception>
    public static ushort Without(this ushort word, EnemyProperties flags)
    {
        ValidateKnown(flags);
        return unchecked((ushort)(word & ~(ushort)flags));
    }

    /// <summary>Clears and sets named bits while preserving family-specific low-byte data.</summary>
    public static ushort Replace(
        this ushort word,
        EnemyProperties clear,
        EnemyProperties set) =>
        word.Without(clear).With(set);

    /// <summary>Tests whether a raw extra-property word contains at least one requested translated flag.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="flags"/> contains an untranslated bit.</exception>
    public static bool HasAny(this ushort word, EnemyExtraProperties flags)
    {
        ValidateKnown(flags);
        return ((EnemyExtraProperties)word & flags) != 0;
    }

    /// <summary>Sets translated extra-property bits while preserving every other native bit.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="flags"/> contains an untranslated bit.</exception>
    public static ushort With(this ushort word, EnemyExtraProperties flags)
    {
        ValidateKnown(flags);
        return unchecked((ushort)(word | (ushort)flags));
    }

    /// <summary>Clears translated extra-property bits while preserving every other native bit.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="flags"/> contains an untranslated bit.</exception>
    public static ushort Without(this ushort word, EnemyExtraProperties flags)
    {
        ValidateKnown(flags);
        return unchecked((ushort)(word & ~(ushort)flags));
    }

    /// <summary>
    /// Preserves explicitly named, still-untranslated extra-property bits without allowing
    /// a caller to smuggle a proven flag through a numeric mask. This keeps ROM state
    /// lossless while forcing understood behavior through <see cref="EnemyExtraProperties"/>.
    /// </summary>
    public static ushort WithUntranslatedExtraBits(
        this ushort word,
        ushort rawBits,
        string sourceContext)
    {
        const ushort knownMask =
            (ushort)(EnemyExtraProperties.UsesExtendedSpritemap |
                EnemyExtraProperties.NewInstructionFrame);
        if ((rawBits & knownMask) != 0)
        {
            throw new ArgumentException(
                $"{sourceContext} attempted to write named enemy extra-property bits " +
                $"through the raw escape hatch: ${rawBits:X4}.",
                nameof(rawBits));
        }

        return unchecked((ushort)(word | rawBits));
    }

    /// <summary>Ensures an enemy-property mask contains only named high-byte flags, leaving family-owned low-byte data out of the typed flag set.</summary>
    /// <param name="flags">Named enemy-property bits requested by a flag operation.</param>
    /// <exception cref="ArgumentOutOfRangeException">The value contains low-byte data or an unnamed high-byte bit.</exception>
    private static void ValidateKnown(EnemyProperties flags)
    {
        if (((ushort)flags & ~KnownPropertyFlagMask) != 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(flags), flags, "EnemyProperties accepts only named high-byte flags.");
        }
    }

    /// <summary>Ensures an extra-property mask contains only translated bits whose behavior is represented by <see cref="EnemyExtraProperties"/>.</summary>
    /// <param name="flags">Extra-property bits requested by a flag operation.</param>
    /// <exception cref="ArgumentOutOfRangeException">The value contains a bit without translated behavior.</exception>
    private static void ValidateKnown(EnemyExtraProperties flags)
    {
        const EnemyExtraProperties known =
            EnemyExtraProperties.UsesExtendedSpritemap |
            EnemyExtraProperties.NewInstructionFrame;
        if ((flags & ~known) != 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(flags), flags, "EnemyExtraProperties accepts only proven flags.");
        }
    }
}
