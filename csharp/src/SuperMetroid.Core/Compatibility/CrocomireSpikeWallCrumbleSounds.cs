namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Which pieces of Crocomire's crumbling spike wall crumble audibly when they land (#1542): every other piece, counting
/// from the top piece, which does. Every landing piece raises a dust cloud and queues the big explosion; only these also
/// queue the crumble before it, so the sound queue holds four crumbles for the wall's eight pieces.
/// </summary>
/// <remarks>
/// Consulted by a falling piece's update on the update it reaches the floor, before it queues the big explosion.
/// Evidence: the piece's update queues the crumble only when its enemy-projectile index is even. The wall's eight pieces
/// are spawned into a pool emptied just before, so they take its last eight places, the top piece the first of them; that
/// place's index is even, and the indexes alternate from there down the wall.
/// </remarks>
[CompatibilityService("Only every other piece of Crocomire's crumbling spike wall, from the top piece, queues the crumble sound when it lands")]
public static class CrocomireSpikeWallCrumbleSounds
{
    /// <summary>Whether the piece at <paramref name="piece"/> queues the crumble sound when it lands.</summary>
    /// <param name="piece">The piece's place in the wall, zero for the top piece.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="piece"/> is negative.</exception>
    public static bool Crumbles(int piece) => Ensure.AtLeastZero(piece) % 2 == 0;
}
