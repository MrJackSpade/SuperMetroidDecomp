using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>The state of the Kago.</summary>
public sealed class KagoEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<KagoSegment> Playback { get; set; }

    /// <summary>
    /// The shots the Kago has taken (#2340): it withstands as many as its placement's hit count and dies of the next. The
    /// shell shows its slow loop, four poses of ten updates each, until its first hit, and its fast loop, the same four poses
    /// of three updates each, from then on (#2473).
    /// </summary>
    public int HitsTaken { get; internal set; }
}
