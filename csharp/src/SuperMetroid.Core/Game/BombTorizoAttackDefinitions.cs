namespace SuperMetroid.Core.Game;

/// <summary>One hand-relative placement used by Bomb Torizo's explosive swipe.</summary>
internal readonly record struct BombTorizoSwipeDefinition(short XOffset, short YOffset);

/// <summary>One body-relative placement used by Bomb Torizo's low-health explosions.</summary>
internal readonly record struct BombTorizoExplosionDefinition(short XOffset, short YOffset);

/// <summary>Fixed physical placement definitions for Bomb Torizo's body projectiles.</summary>
internal static class BombTorizoAttackDefinitions
{
    /// <summary>
    /// The eleven paired X/Y placements at <c>$86:A738-$86:A763</c>. Native swipe
    /// parameters are even byte offsets <c>$00..$14</c>.
    /// </summary>
    private static readonly BombTorizoSwipeDefinition[] SwipePlacements =
    [
        new(-30, -52),
        new(-40, -28),
        new(-47, -11),
        new(-31, 9),
        new(-21, 21),
        new(-1, 20),
        new(-28, -52),
        new(-43, -27),
        new(-48, -10),
        new(-31, 9),
        new(-21, 20),
    ];

    /// <summary>
    /// The six paired X/Y placements at <c>$86:A859-$86:A870</c>. Rows zero and three
    /// are unused padding between the gut and face facing pairs, but remain part of the
    /// native adjusted-parameter lookup.
    /// </summary>
    private static readonly BombTorizoExplosionDefinition[] ExplosionPlacements =
    [
        new(0, -8),
        new(12, -8),
        new(-12, -8),
        new(0, -20),
        new(16, -20),
        new(-16, -20),
    ];

    /// <summary>
    /// Returns the swipe placement selected by the native byte offset. Retail callers use
    /// even values; the translated host's bounded odd restored values retain their prior
    /// word-index truncation instead of gaining a new behavior during this migration.
    /// </summary>
    internal static BombTorizoSwipeDefinition Swipe(ushort parameter)
    {
        int index = parameter >> 1;
        if ((uint)index >= (uint)SwipePlacements.Length)
        {
            throw new InvalidDataException(
                $"Bomb Torizo swipe parameter ${parameter:X4} exceeds " +
                "its eleven cartridge selections.");
        }

        return SwipePlacements[index];
    }

    /// <summary>
    /// Applies initializer <c>$86:A81B</c>'s facing-dependent operand adjustment and
    /// returns the selected low-health explosion placement.
    /// </summary>
    internal static BombTorizoExplosionDefinition LowHealthExplosion(
        ushort parameter,
        bool facingRight)
    {
        int adjusted = parameter + 2 + (facingRight ? 0 : 2);
        int index = adjusted >> 1;
        if ((uint)index >= (uint)ExplosionPlacements.Length)
        {
            throw new InvalidDataException(
                $"Bomb Torizo explosion parameter ${parameter:X4} selects table index {index}.");
        }

        return ExplosionPlacements[index];
    }

    /// <summary>Exposes one raw explosion row for cartridge parity verification.</summary>
    internal static BombTorizoExplosionDefinition RawExplosionRow(int index) =>
        (uint)index < (uint)ExplosionPlacements.Length
            ? ExplosionPlacements[index]
            : throw new ArgumentOutOfRangeException(nameof(index));
}
