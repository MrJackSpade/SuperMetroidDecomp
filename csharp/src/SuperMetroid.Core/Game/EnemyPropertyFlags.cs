namespace SuperMetroid.Core.Game;

/// <summary>
/// Independently combinable bits in the native enemy-property word. Only meanings already
/// exercised by translated code are named; unknown cartridge bits remain raw rather than
/// receiving plausible-but-unverified labels.
/// </summary>
[Flags]
public enum EnemyProperties : ushort
{
    None = 0,
    Invisible = 0x0100,
    Deleted = 0x0200,
    IgnoreSamusCollision = 0x0400,
    ProcessOffScreen = 0x0800,
    ProcessInstructions = 0x2000,

    /// <summary>
    /// Native property $4000. Enemy death retains the physical slot as definition $DAFF,
    /// and the death-projectile instruction stream later reconstructs it from the immutable
    /// population/spawn record. Rinka is the first translated family that exercises this
    /// lifecycle, so the bit now has directly observed behavior rather than a speculative name.
    /// </summary>
    RespawnIfKilled = 0x4000,
}

/// <summary>
/// Independently combinable bits in the native enemy extra-property word whose behavior is
/// already proven by the live room-enemy path.
/// </summary>
[Flags]
public enum EnemyExtraProperties : ushort
{
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
    public static bool HasAny(this ushort word, EnemyProperties flags) =>
        ((EnemyProperties)word & flags) != 0;

    public static bool HasAll(this ushort word, EnemyProperties flags) =>
        ((EnemyProperties)word & flags) == flags;

    public static ushort With(this ushort word, EnemyProperties flags) =>
        unchecked((ushort)(word | (ushort)flags));

    public static ushort Without(this ushort word, EnemyProperties flags) =>
        unchecked((ushort)(word & ~(ushort)flags));

    public static bool HasAny(this ushort word, EnemyExtraProperties flags) =>
        ((EnemyExtraProperties)word & flags) != 0;

    public static bool HasAll(this ushort word, EnemyExtraProperties flags) =>
        ((EnemyExtraProperties)word & flags) == flags;

    public static ushort With(this ushort word, EnemyExtraProperties flags) =>
        unchecked((ushort)(word | (ushort)flags));

    public static ushort Without(this ushort word, EnemyExtraProperties flags) =>
        unchecked((ushort)(word & ~(ushort)flags));
}
