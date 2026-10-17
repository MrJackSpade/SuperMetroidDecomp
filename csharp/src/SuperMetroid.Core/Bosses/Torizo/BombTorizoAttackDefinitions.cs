namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Fixed physical placement definitions for Bomb Torizo's body projectiles: hand-relative
/// swipe placements and body-relative low-health explosion placements, each a room-pixel offset.
/// </summary>
internal static class BombTorizoAttackDefinitions
{
    /// <summary>
    /// The eleven paired X/Y placements at <c>$86:A738-$86:A763</c>. Native swipe
    /// parameters are even byte offsets <c>$00..$14</c>.
    /// </summary>
    private static readonly Offset16<RoomPixels>[] SwipePlacements =
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
        if (parameter < 0 || (uint)index >= 6u)
        {
            throw new InvalidDataException(
                $"Bomb Torizo explosion parameter ${parameter:X4} selects table index {index}.");
        }

        return RawExplosionRow(index);
    }

    /// <summary>
    /// Calculates $86:A859/$A865: gut and face explosion offsets, each with an unused
    /// centered row followed by right/left facing rows. Gut offsets are12 pixels
    /// horizontally and8 up; face offsets are16 horizontally and20 up.
    /// </summary>
    internal static Offset16<RoomPixels> RawExplosionRow(int index)
    {
        if ((uint)index >= 6u)
            throw new ArgumentOutOfRangeException(nameof(index));
        bool atFace = index >= 3;
        int horizontalDistance = atFace ? 16 : 12;
        int facing = index % 3;
        int horizontalSign = facing == 0 ? 0 : facing == 1 ? 1 : -1;
        return new((short)(horizontalSign * horizontalDistance), (short)(atFace ? -20 : -8));
    }
}