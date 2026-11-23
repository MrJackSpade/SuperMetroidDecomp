namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What a new enemy projectile starts from in its place (#1476, #1590, #2572): the whole position and velocity the place's
/// previous projectile left there (<see cref="EnemyProjectileHandOver"/>). Until the room spawns into a place, its
/// previous projectile is the one the room before left there, so the room's first spawn into each place starts where and
/// as fast as that one was; after that, it is the place's last projectile of this room, read as it stands when the spawn
/// takes the place. A spawn that sets its position or velocity itself overrides what it starts from; one that does not
/// keeps it.
/// </summary>
/// <remarks>
/// Taken over from the room before when the room's enemy projectiles are built, before its population loads
/// (<see cref="TakeOverFrom"/>); consulted by every spawn as it takes a free place (<see cref="StartOf"/>); handed to the
/// next room at the room load's teardown (<see cref="HandOverTo"/>). The session's first room finds nothing left and
/// starts every place at rest at the room's origin. Evidence: the original spawn clears a slot's sub-positions and the
/// values its initializers write, and leaves its whole positions and velocities; deletion clears only the slot's identity,
/// and room loading deletes every projectile without clearing them. The 13%, 100%, lowc and Ridley movies diverge in
/// enemy projectiles without the values carried across rooms.
/// </remarks>
public sealed partial class EnemyProjectilePlaceLeftovers
{
    /// <summary>What the room before left in each place, in place order; rest at the origin before any room has left one.</summary>
    internal EnemyProjectileHandOver[] FromRoomBefore { get; }

    /// <summary>Starts a room's places with nothing left in them.</summary>
    /// <param name="placeCount">The number of enemy-projectile places, at least one.</param>
    public EnemyProjectilePlaceLeftovers(int placeCount) =>
        FromRoomBefore = new EnemyProjectileHandOver[Ensure.BetweenInclusive(1, int.MaxValue, placeCount)];

    /// <summary>The places start from what the room before left in each of them.</summary>
    /// <param name="handOver">What the room before left for this one.</param>
    public void TakeOverFrom(RoomHandOver handOver)
    {
        Ensure.NotNull(handOver);
        for (int place = 0; place < FromRoomBefore.Length; place++)
            FromRoomBefore[place] = handOver.EnemyProjectile(place);
    }

    /// <summary>
    /// What a spawn into <paramref name="place"/> starts from: what the place's last projectile of this room left, or what
    /// the room before left there when this room has spawned none into it.
    /// </summary>
    /// <param name="place">The zero-based enemy-projectile place the spawn takes.</param>
    /// <param name="leftByLastProjectile">What the place's last projectile of this room leaves as the spawn takes the place; null for none.</param>
    public EnemyProjectileHandOver StartOf(int place, EnemyProjectileHandOver? leftByLastProjectile) =>
        leftByLastProjectile ?? FromRoomBefore[place];

    /// <summary>Leaves what each place holds for the room that replaces this one.</summary>
    /// <param name="handOver">What this room leaves for the next.</param>
    /// <param name="leftByLastProjectiles">What each place's last projectile of this room leaves, in place order; null for a place this room spawned nothing into.</param>
    public void HandOverTo(RoomHandOver handOver, IReadOnlyList<EnemyProjectileHandOver?> leftByLastProjectiles)
    {
        Ensure.NotNull(handOver);
        Ensure.NotNull(leftByLastProjectiles);
        if (leftByLastProjectiles.Count != FromRoomBefore.Length)
            throw new ArgumentException($"The room has {FromRoomBefore.Length} enemy-projectile places, not {leftByLastProjectiles.Count}.", nameof(leftByLastProjectiles));
        handOver.LeaveEnemyProjectiles(Enumerable.Range(0, FromRoomBefore.Length).Select(place => StartOf(place, leftByLastProjectiles[place])));
    }
}
