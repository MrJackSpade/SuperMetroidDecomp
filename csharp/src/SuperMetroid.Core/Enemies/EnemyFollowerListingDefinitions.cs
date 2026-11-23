namespace SuperMetroid.Core.Enemies;

/// <summary>How the room lists a lava jumper's or Rio's follower (#1472, #2556): right after the enemy it follows.</summary>
internal static partial class EnemyFollowerListingDefinitions
{
    /// <summary>The leader, then its follower.</summary>
    internal static EnemyComposite<EnemyFollowerRole> Listing { get; } = new([EnemyFollowerRole.Leader, EnemyFollowerRole.Follower]);
}
