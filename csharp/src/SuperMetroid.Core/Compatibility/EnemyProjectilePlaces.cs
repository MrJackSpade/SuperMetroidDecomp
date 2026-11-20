namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The room's enemy-projectile places and the order gameplay runs them in (#2556). The room holds
/// <see cref="Capacity"/> places; a spawn takes the free place searched from the last down, and every pass over the
/// projectiles (their motions and animations, their collisions with Samus and her shots, their drawing) runs from the
/// last place down, so a projectile spawned below the one being run runs later in the same pass and one spawned above it
/// waits for the next. A spawn also leaves the place it took, or that it found none, which a Powamp's death explosion
/// reads (<see cref="LeftoverNumberDeathExplosion{TExplosion}.LeftByLatestSpawn"/>).
/// </summary>
/// <remarks>
/// Consulted by the room's enemy-projectile engine for every spawn and every pass; read by a shot Powamp's death as it
/// starts. Lifetime: the room's; the room's enemy system creates it with its projectile engine. Evidence: the original
/// enemy-projectile spawn searches its eighteen slots from the last down for a free one and leaves the slot's offset in a
/// register its caller may keep, and the projectile, collision and drawing loops index the same slots from the last
/// down, reading each slot when they reach it.
/// </remarks>
[CompatibilityService("the room's enemy projectiles take eighteen places, searched and run from the last down, and a spawn leaves the place it took for a death explosion to read")]
public sealed class EnemyProjectilePlaces
{
    /// <summary>How many enemy projectiles the room holds at once.</summary>
    public const int Capacity = 18;

    /// <summary>Every place, from the last down.</summary>
    private static readonly int[] RunOrder = [.. Enumerable.Range(0, Capacity).Reverse()];

    /// <summary>The places in the order spawns search them and every pass runs them: from the last down.</summary>
    public IReadOnlyList<int> InRunOrder => RunOrder;

    /// <summary>The place the room's latest spawn took, or null when it found every place held or none has run.</summary>
    public int? LatestSpawn { get; private set; }

    /// <summary>Records what the latest spawn found: the place it took, or null when every place was held.</summary>
    /// <param name="place">The zero-based place the spawn took, or null.</param>
    internal void RecordSpawn(int? place) =>
        LatestSpawn = place is { } taken ? Ensure.BetweenInclusive(0, Capacity - 1, taken) : null;
}
