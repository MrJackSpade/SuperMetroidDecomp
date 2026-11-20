namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>How Golden Torizo's egg moves itself (#1843, #2554).</summary>
internal enum GoldenTorizoEggMotion
{
    /// <summary>Bounces the egg for its timer, then launches it horizontally.</summary>
    EggBouncing,

    /// <summary>Accelerates the hatched egg toward a wall.</summary>
    EggCharging,

    /// <summary>Drops an egg that struck a wall to the floor, where it hatches or bursts.</summary>
    EggDropping,
}
