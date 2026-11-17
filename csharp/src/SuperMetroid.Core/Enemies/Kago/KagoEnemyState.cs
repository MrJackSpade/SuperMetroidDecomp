using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>The state of the Kago.</summary>
public sealed class KagoEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<KagoSegment> Playback { get; set; }

    /// <summary>Whether the shell runs its three-frame loop rather than its ten-frame one.</summary>
    public bool UsesFastAnimation { get; internal set; }

    /// <summary>
    /// The shots the Kago has taken (#2340): it withstands as many as its placement's hit count and dies of the next.
    /// </summary>
    public int HitsTaken { get; internal set; }
}
