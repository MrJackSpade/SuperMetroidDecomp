using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>The state of the escape Etecoon.</summary>
public sealed class EscapeEtecoonEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<EscapeEtecoonSegment> Playback { get; set; }


    /// <summary>
    /// The signed 8.8 walking velocity in pixels per update consumed by the walking behaviour; only its X is meaningful. A
    /// wall reverses it by negating it.
    /// </summary>
    public EightEightVector<RoomPixels> Velocity { get; set; }

    /// <summary>The installed frame behaviour.</summary>
    public EscapeEtecoonFrameBehavior? FrameBehavior { get; set; }
}
