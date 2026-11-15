using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>The state of the escape Etecoon.</summary>
public sealed class EscapeEtecoonEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal EscapeEtecoonEnemyState(EscapeEtecoonAction action) => Action = action;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<EscapeEtecoonSegment> Playback { get; set; }


    /// <summary>
    /// The signed 1/256-pixel walking velocity in pixels per update the walk moves it by; only its X is meaningful. A
    /// wall reverses it by negating it.
    /// </summary>
    public EightEightVector<RoomPixels> Velocity { get; set; }

    /// <summary>What the Etecoon is doing.</summary>
    public EscapeEtecoonAction Action { get; set; }
}
