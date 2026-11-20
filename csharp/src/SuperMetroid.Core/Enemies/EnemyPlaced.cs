namespace SuperMetroid.Core.Enemies;

/// <summary>
/// An enemy a placement built (#2554): the enemy, and, when it heads a group, how each of the parts the room lists right
/// after it is built, in listing order.
/// </summary>
/// <param name="Enemy">The enemy built in the placement's place.</param>
/// <param name="Parts">How each of the group's parts is built; empty for an enemy that heads no group.</param>
internal sealed record EnemyPlaced(IEnemyEntity Enemy, IReadOnlyList<EnemyPartBuild> Parts)
{
    /// <summary>An enemy that heads no group.</summary>
    /// <param name="enemy">The enemy built.</param>
    internal static EnemyPlaced Single(IEnemyEntity enemy) => new(enemy, []);
}
