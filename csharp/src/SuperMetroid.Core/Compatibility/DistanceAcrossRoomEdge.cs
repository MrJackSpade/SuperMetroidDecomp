namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How far apart two places lie, and on which side, when either may stand past the room's top or left edge (#2568): the
/// distance is taken the short way across the ends of the room's coordinates, their difference kept within the
/// coordinates and read as signed, so an actor parked or moving past the room's edge (<see cref="OffRoomPlaces"/>)
/// still lies a short distance from one inside the room, on the side it lies, and a place more than half the
/// coordinates away reads as lying on the other side. Draygon's claws reach for Samus and his pass ends against his
/// parked places this way, Phantoon's swoop clamps to its bounds, the gunship's boarding box and lift heights, a Wave
/// combo homing on Samus, a Metroid's recoil from a shot, enemy explosions and hits, grapple reach and the patrol posts
/// of walking Space Pirates are all measured so.
/// </summary>
/// <remarks>
/// Consulted wherever the game compares two places, or measures between them, and either can lie past the room's top
/// or left edge, in the update of the actor whose test it is. Evidence: the original measures every distance and side
/// test between two places as a sixteen-bit subtraction and branches on its sign, or on its magnitude after negating a
/// negative result; Draygon's leftward goop pass (<c>$A5:8DB2</c>) compares his X with <c>$FFB0</c> by the sign of
/// their difference; the Draygon, Phantoon, Mother Brain and Ridley recorded movies replay those tests on every update.
/// </remarks>
[CompatibilityService("distances and side tests between two places are taken the short way across the ends of the room's coordinates, so an actor past the room's top or left lies a short distance from one inside, on its own side")]
public static class DistanceAcrossRoomEdge
{
    /// <summary>
    /// How far <paramref name="to"/> lies past <paramref name="from"/>, -32768 through 32767 pixels: negative when it lies
    /// above or left of it, taken the short way across the ends of the room's coordinates.
    /// </summary>
    /// <param name="to">The place measured to, which may be computed past either end of the room's coordinates.</param>
    /// <param name="from">The place measured from, which may be computed past either end of the room's coordinates.</param>
    public static int Apart(int to, int from) => CoordinateAxis.Difference(to, from);

    /// <summary>
    /// How far apart two places lie, whichever lies first, 0 through 32768 pixels, taken the short way as
    /// <see cref="Apart"/> takes it.
    /// </summary>
    /// <param name="to">One place.</param>
    /// <param name="from">The other place.</param>
    public static int Between(int to, int from) => CoordinateAxis.Distance(to, from);

    /// <summary>
    /// Whether <paramref name="place"/> lies above or left of <paramref name="reference"/>, taken the short way as
    /// <see cref="Apart"/> takes it: a place more than half the room's coordinates past the reference lies before it.
    /// </summary>
    /// <param name="place">The place tested.</param>
    /// <param name="reference">The place it is compared with.</param>
    public static bool LiesBefore(int place, int reference) => CoordinateAxis.Precedes(place, reference);
}
