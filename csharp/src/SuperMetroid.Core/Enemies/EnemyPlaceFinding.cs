using SuperMetroid.Core.Enemies.Shaktool;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// What a neighbour finds when it reads an enemy place without testing what the place holds (#2130,
/// <see cref="RemovedEnemyLeftovers"/>): the enemy holding it or the enemy a deletion left there as it left, the emptied
/// remains the last kill left, or nothing while no enemy has held it.
/// </summary>
internal abstract record EnemyPlaceFinding
{
    /// <summary>Only the findings below exist.</summary>
    private EnemyPlaceFinding()
    {
    }

    /// <summary>The enemy holding the place, or the enemy a deletion left there with its state as it left it.</summary>
    /// <param name="Entity">The enemy found, holding its state.</param>
    internal sealed record Enemy(IEnemyEntity Entity) : EnemyPlaceFinding;

    /// <summary>The emptied remains the place's last kill left, which hold no state until a neighbour writes them.</summary>
    /// <param name="Remains">The killed enemy's remains.</param>
    internal sealed record Killed(KilledEnemyRemains Remains) : EnemyPlaceFinding;

    /// <summary>No enemy has held the place.</summary>
    internal sealed record Nothing : EnemyPlaceFinding;
}

/// <summary>
/// What an enemy's kill leaves in its place, emptied, which its neighbours go on reading (<see cref="RemovedEnemyLeftovers"/>).
/// It holds no state, except what a Shaktool chain running out the update that killed one of its segments writes to that
/// segment's remains and reads back (#1543, #2143).
/// </summary>
internal sealed class KilledEnemyRemains
{
    /// <summary>The emptied segment state a Shaktool chain orbits, orients and reverses against, keeping what it writes.</summary>
    internal ShaktoolSegmentState ShaktoolSegment { get; } = new();
}
