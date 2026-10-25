namespace SuperMetroid.Core.Enemies;

/// <summary>The population parameter that makes a Rio-like enemy follow a parent (#1412).</summary>
internal static class EnemyFollowers
{
    /// <summary>
    /// Whether a Rio, Norfair Rio, Lower Norfair Rio or Norfair lava jumper whose population parameter one is
    /// <paramref name="parameter1"/> follows a parent rather than leading: a negative parameter marks a follower.
    /// </summary>
    internal static bool FollowsParent(int parameter1) => EnemyMath.IsNegative16(parameter1);
}
