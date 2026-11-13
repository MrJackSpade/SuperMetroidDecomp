using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>The state of the Kago.</summary>
public sealed class KagoEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<KagoSegment> Playback { get; set; }

    /// <summary>The current behaviour, installed at initialization and replaced on the first AI update.</summary>
    public KagoEnemyFunction? Function { get; internal set; }

    /// <summary>Whether the shell runs its three-frame loop rather than its ten-frame one.</summary>
    public bool UsesFastAnimation { get; internal set; }

    /// <summary>
    /// The shots the Kago has left. The game decrements first and dies only when the signed result
    /// is negative, so population value ten permits eleven accepted shots.
    /// </summary>
    public int HitCounter { get; internal set; }
}
