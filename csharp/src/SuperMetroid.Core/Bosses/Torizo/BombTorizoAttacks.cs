using static SuperMetroid.Core.Bosses.Torizo.BombTorizoAttackDefinitions;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Places Bomb Torizo's swipes and low-health explosions from their native parameters and facing.</summary>
internal static class BombTorizoAttacks
{
    /// <summary>
    /// Returns the swipe placement selected by the native byte offset. Retail callers use
    /// even values; the translated host's bounded odd restored values retain their prior
    /// word-index truncation instead of gaining a new behavior during this migration.
    /// </summary>
    internal static Offset16<RoomPixels> Swipe(int parameter)
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
    internal static Offset16<RoomPixels> LowHealthExplosion(
        int parameter,
        bool facingRight)
    {
        int adjusted = parameter + 2 + (facingRight ? 0 : 2);
        int index = adjusted >> 1;
        if (parameter < 0 || (uint)index >= (uint)ExplosionRows.Length)
        {
            throw new InvalidDataException(
                $"Bomb Torizo explosion parameter ${parameter:X4} selects table index {index}.");
        }

        return RawExplosionRow(index);
    }

    /// <summary>One gut or face explosion row: an unused centered row, then the right- and left-facing rows.</summary>
    internal static Offset16<RoomPixels> RawExplosionRow(int index) =>
        (uint)index < (uint)ExplosionRows.Length ? ExplosionRows[index] : throw new ArgumentOutOfRangeException(nameof(index));
}
