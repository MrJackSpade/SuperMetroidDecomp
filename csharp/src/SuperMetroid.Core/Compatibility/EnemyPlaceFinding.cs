using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Shaktool;

namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What a neighbour finds when it reads an enemy place without testing what the place holds (#2130,
/// <see cref="RemovedEnemyLeftovers"/>): the enemy holding it or the enemy a deletion left there as it left, the record
/// the last kill cleared, or nothing while no enemy has held it.
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

    /// <summary>The record the place's last kill cleared, which reads as holding no state until a neighbour writes it.</summary>
    /// <param name="Record">The cleared record.</param>
    internal sealed record Cleared(ClearedEnemyRecord Record) : EnemyPlaceFinding;

    /// <summary>No enemy has held the place.</summary>
    internal sealed record Nothing : EnemyPlaceFinding;
}

/// <summary>
/// The record an enemy's kill cleared in its place, which its neighbours go on reading (<see cref="RemovedEnemyLeftovers"/>).
/// It holds no state, except what a Shaktool chain running out the update that killed one of its segments writes to that
/// segment's cleared record and reads back (#1543, #2143).
/// </summary>
internal sealed class ClearedEnemyRecord
{
    /// <summary>The emptied segment state a Shaktool chain orbits, orients and reverses against, keeping what it writes.</summary>
    internal ShaktoolSegmentState ShaktoolSegment { get; } = new();
}
