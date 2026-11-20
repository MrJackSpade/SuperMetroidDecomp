using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>
/// The state of one Atomic: its spinning animation. It chases Samus at its spawn settings' speed, which it reads where it
/// moves (#2476).
/// </summary>
public sealed class AtomicEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<AtomicSegment> Playback { get; set; }
}
