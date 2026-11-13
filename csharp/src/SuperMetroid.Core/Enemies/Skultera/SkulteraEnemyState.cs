using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>
/// The state of one Skultera.
/// </summary>
public sealed class SkulteraEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<SkulteraSegment> Playback { get; set; }

    /// <summary>The swimming or turning behaviour dispatched by main AI.</summary>
    public SkulteraAction? Action { get; internal set; }

    /// <summary>Signed 16.16 room-pixel displacement per swimming update to the right, from the positive half of the population's linear-speed record.</summary>
    public SignedSixteenSixteen RightVelocity { get; internal set; }

    /// <summary>Signed 16.16 room-pixel displacement per swimming update to the left, from the negative half of the population's linear-speed record.</summary>
    public SignedSixteenSixteen LeftVelocity { get; internal set; }

    /// <summary>The vertical oscillation's phase; only its table index is meaningful, and it wraps after a full turn.</summary>
    public RotationAngle Angle { get; internal set; }

    /// <summary>Vertical oscillation radius in pixels, its authored wave height (0 through 255).</summary>
    public int Radius { get; internal set; }

    /// <summary>Published by the turn animation's last cue after a complete turn.</summary>
    public bool TurnFinished { get; internal set; }

    /// <summary>
    /// Signed angle step per unobstructed swimming update, in table units: its authored wave step (0 through 255),
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
