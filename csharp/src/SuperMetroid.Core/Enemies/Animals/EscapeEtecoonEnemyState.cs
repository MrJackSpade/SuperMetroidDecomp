using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>The state of the escape Etecoon.</summary>
public sealed partial class EscapeEtecoonEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal EscapeEtecoonEnemyState(EscapeEtecoonAction action) => Action = action;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<EscapeEtecoonSegment> Playback { get; set; }


    /// <summary>The way the Etecoon walks (#2369); a wall turns it around.</summary>
    public HorizontalDirection Direction { get; internal set; }

    /// <summary>How far the Etecoon walks each update toward its <see cref="Direction"/>.</summary>
    internal SignedEightEight Speed { get; set; }

    /// <summary>The Etecoon's horizontal velocity: its <see cref="Speed"/> toward its <see cref="Direction"/>.</summary>
    internal SignedEightEight Velocity => Speed.Toward(Direction);

    /// <summary>What the Etecoon is doing.</summary>
    public EscapeEtecoonAction Action { get; internal set; }

    /// <summary>
    /// Sets the Etecoon walking toward <paramref name="direction"/> at <paramref name="speed"/>, as its setup does once it
    /// stays in the room.
    /// </summary>
    internal void StartWalking(HorizontalDirection direction, SignedEightEight speed)
    {
        Direction = direction;
        Speed = speed;
    }

    /// <summary>Reverses the walk, as a wall does.</summary>
    internal void TurnAround() => Direction = Direction.Reversed();

    /// <summary>Starts the Etecoon's escape, as its animation's escape cue does.</summary>
    internal void Escape() => Action = EscapeEtecoonAction.Escaping;

    /// <summary>Starts the Etecoon's gratitude, as its animation's gratitude cue does.</summary>
    internal void ExpressGratitude() => Action = EscapeEtecoonAction.ExpressingGratitude;
}
