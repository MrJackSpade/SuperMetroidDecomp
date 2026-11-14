namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// An enemy that collides by its frames' hitboxes never meets a bomb lying on the room's leftmost pixel column (#1847):
/// the bomb is passed over as though no bomb were there, while an enemy that collides by its radius meets it as it meets a
/// bomb anywhere else.
/// </summary>
/// <remarks>
/// Consulted by the enemies' bomb pass for each exploding bomb it tests against an enemy that collides by its frames'
/// hitboxes, before the overlap test. Evidence: the original's frame-hitbox bomb collision (<c>$A0:9D79</c>) skips a
/// bomb place whose X position is zero before reading its type, the test it uses for an empty place; the radius bomb
/// collision has no such test.
/// </remarks>
[CompatibilityService("an enemy that collides by its frames' hitboxes never meets a bomb lying on the room's leftmost pixel column")]
public static class LeftEdgeBombMissesFrameHitboxes
{
    /// <summary>The room pixel column whose bombs enemies that collide by their frames' hitboxes pass over.</summary>
    public const int PassedColumn = 0;

    /// <summary>Whether an enemy that collides by its frames' hitboxes passes over a bomb whose whole X is <paramref name="bombX"/>.</summary>
    /// <param name="bombX">The bomb's whole X position in room pixels.</param>
    public static bool Misses(int bombX) => bombX == PassedColumn;
}
