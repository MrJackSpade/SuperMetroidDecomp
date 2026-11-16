using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>
/// The state of one Skultera.
/// </summary>
public sealed class SkulteraEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/> toward <paramref name="heading"/>.</summary>
    internal SkulteraEnemyState(SkulteraAction action, HorizontalDirection heading)
    {
        Action = action;
        Heading = heading;
    }

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<SkulteraSegment> Playback { get; set; }

    /// <summary>The swimming or turning behaviour main AI runs.</summary>
    public SkulteraAction Action { get; internal set; }

    /// <summary>The way the Skultera swims, or turns to swim (#2165).</summary>
    public HorizontalDirection Heading { get; internal set; }

    /// <summary>The displacement in 1/65536 room pixels per swimming update to the right: the speed the Skultera spawns with.</summary>
    public SignedSixteenSixteen RightVelocity { get; internal set; }

    /// <summary>The displacement in 1/65536 room pixels per swimming update to the left: the speed the Skultera spawns with, reversed.</summary>
    public SignedSixteenSixteen LeftVelocity { get; internal set; }

    /// <summary>The vertical oscillation's phase; only its whole step is meaningful, and it wraps after a full turn.</summary>
    public RotationAngle Angle { get; internal set; }

    /// <summary>Vertical oscillation radius in pixels, its authored wave height (0 through 255).</summary>
    public int Radius { get; internal set; }

    /// <summary>Published by the turn animation's last cue after a complete turn.</summary>
    public bool TurnFinished { get; internal set; }

    /// <summary>
    /// Signed angle step per unobstructed swimming update, in steps (256ths of a turn): its authored wave step (0 through 255),
    /// negated at every completed turn.
    /// </summary>
    public int AngleDelta { get; internal set; }

    /// <summary>Oscillation offset retained from the preceding movement attempt, in pixels (at most the radius either way).</summary>
    public int PreviousYOffset { get; internal set; }

    /// <summary>Current oscillation offset in pixels (at most the radius either way).</summary>
    public int CurrentYOffset { get; internal set; }

    /// <summary>Advances the oscillation phase by <see cref="AngleDelta"/>, wrapping after a full turn.</summary>
    internal void AdvanceAngle() => Angle = Angle.TurnedBySteps(AngleDelta);
}
