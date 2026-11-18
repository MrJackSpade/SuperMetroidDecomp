using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>The state of one Rio.</summary>
public sealed class RioEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal RioEnemyState(RioAction action) => Action = action;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<RioSegment> Playback { get; set; }

    /// <summary>Signed 1/256-pixel horizontal dive velocity retained while the hovering phase temporarily stops X.</summary>
    public SignedEightEight SavedHorizontalVelocity { get; internal set; }

    /// <summary>The behaviour that runs after the main AI's RNG advance.</summary>
    public RioAction Action { get; internal set; }

    /// <summary>The Rio's velocity in 256ths of a pixel an update, positive right and down.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>
    /// Whether its swoop's opening or its cooldown animation has finished: set by the animation's finishing cue and cleared
    /// as its main AI moves on. It stays a mark beside the action rather than a phase of it (#2450): a wall or floor can
    /// turn the dive into its rebound before the opening the dive waits for has finished, or on the update after; the
    /// opening's finish then carries through the rebound and, as the Rio lands, ends its wait for the cooldown animation
    /// at once.
    /// </summary>
    public bool AnimationFinished { get; internal set; }
}
