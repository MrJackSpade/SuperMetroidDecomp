namespace SuperMetroid.Core.Enemies;

/// <summary>Whether a placement leads or follows the enemy before it (#1472).</summary>
internal enum EnemyFollowerRole
{
    /// <summary>It leads.</summary>
    Leader,

    /// <summary>It follows the enemy before it.</summary>
    Follower,
}
