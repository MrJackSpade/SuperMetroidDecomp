using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>
/// Verification's native view of the Atomic identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class AtomicNativeWords
{
    /// <summary>The routine word naming each <see cref="AtomicHorizontalMovement"/>.</summary>
    internal static readonly NativeWordIdentities<AtomicHorizontalMovement> AtomicHorizontalMovements = new(new Dictionary<AtomicHorizontalMovement, ushort>
    {
        [AtomicHorizontalMovement.MoveLeft] = 0xe443,
        [AtomicHorizontalMovement.MoveRight] = 0xe462,
    });

    /// <summary>The routine word naming each <see cref="AtomicVerticalMovement"/>.</summary>
    internal static readonly NativeWordIdentities<AtomicVerticalMovement> AtomicVerticalMovements = new(new Dictionary<AtomicVerticalMovement, ushort>
    {
        [AtomicVerticalMovement.MoveUp] = 0xe405,
        [AtomicVerticalMovement.MoveDown] = 0xe424,
    });
}
