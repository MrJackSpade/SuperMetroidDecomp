namespace SuperMetroid.Core.Enemies;

/// <summary>Where a room graphics set shows a member's tiles in the enemy tile area (#1412).</summary>
public enum EnemyTilePlacement
{
    /// <summary>Directly after the previous member's tiles.</summary>
    FollowingPrevious,

    /// <summary>At the fixed block the member's graphics-set record names.</summary>
    AtFixedBlock,
}
