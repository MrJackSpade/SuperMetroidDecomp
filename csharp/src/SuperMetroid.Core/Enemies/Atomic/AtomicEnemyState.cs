using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>The state of one Atomic: its spinning animation and the speed it chases Samus at on each axis.</summary>
public sealed class AtomicEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<AtomicSegment> Playback { get; set; }

    /// <summary>
    /// The speed, in pixels per update, at which it moves toward Samus on each axis: added when she is below or to the
    /// right of it (or level with it) and subtracted when she is above or to the left.
    /// </summary>
    public SignedSixteenSixteen Speed { get; internal set; }
}
