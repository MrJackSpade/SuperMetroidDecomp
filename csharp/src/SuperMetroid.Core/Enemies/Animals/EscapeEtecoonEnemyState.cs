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
    /// The signed 1/256-pixel horizontal walking velocity in pixels per update the walk moves it by. A wall reverses it
    /// by negating it.
    /// </summary>
    public SignedEightEight Velocity { get; private set; }

    /// <summary>What the Etecoon is doing.</summary>
    public EscapeEtecoonAction Action { get; private set; }

    /// <summary>Sets the Etecoon walking at <paramref name="velocity"/>, as its setup does once it stays in the room.</summary>
    internal void StartWalking(SignedEightEight velocity) => Velocity = velocity;

    /// <summary>Reverses the walk, as a wall does.</summary>
    internal void TurnAround() => Velocity = Velocity.Negated();

    /// <summary>Starts the Etecoon's escape, as its animation's escape cue does.</summary>
    internal void Escape() => Action = EscapeEtecoonAction.Escaping;

    /// <summary>Starts the Etecoon's gratitude, as its animation's gratitude cue does.</summary>
    internal void ExpressGratitude() => Action = EscapeEtecoonAction.ExpressingGratitude;
}
