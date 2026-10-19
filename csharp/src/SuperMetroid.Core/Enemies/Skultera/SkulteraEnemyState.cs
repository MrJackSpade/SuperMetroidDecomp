using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>
/// The state of one Skultera (spelled <c>Skulltera</c> in the upstream symbols; the enemy header and the game's
/// visible name use <c>Skultera</c>). The function, both swimming speeds and the oscillation angle are values the
/// cartridge keeps in the enemy's common record, so they are emptied with that record when the enemy is killed
/// (<see cref="ClearWithRecord"/>); the rest outlive the kill.
/// </summary>
public sealed class SkulteraEnemyState
{
    /// <summary>Where the typed animation (#641) resumes.</summary>
    internal AnimationProgramCursor<SkulteraProgram> Program { get; set; }

    /// <summary>The swimming or turning behaviour dispatched by main AI; none once a kill has emptied the record.</summary>
    public SkulteraEnemyFunction? Function { get; internal set; }

    /// <summary>Signed 16.16 room-pixel displacement per swimming update to the right, from the positive half of the population's linear-speed record.</summary>
    public SnesSignedSixteenSixteen RightVelocity { get; internal set; }

    /// <summary>Signed 16.16 room-pixel displacement per swimming update to the left, from the negative half of the population's linear-speed record.</summary>
    public SnesSignedSixteenSixteen LeftVelocity { get; internal set; }

    /// <summary>The vertical oscillation's phase; only its table index is meaningful, and it wraps after a full turn.</summary>
    public SnesAngle Angle { get; internal set; }

    /// <summary>Vertical oscillation radius in pixels, population parameter two's low byte (0 through 255).</summary>
    public int Radius { get; internal set; }

    /// <summary>Published by the turn animation's last cue after a complete turn.</summary>
    public bool TurnFinished { get; internal set; }

    /// <summary>
    /// Signed angle step per unobstructed swimming update, in table units: population parameter two's high byte
    /// (0 through 255), negated at every completed turn.
    /// </summary>
    public int AngleDelta { get; internal set; }

    /// <summary>Oscillation offset retained from the preceding movement attempt, in pixels (at most the radius either way).</summary>
    public int PreviousYOffset { get; internal set; }

    /// <summary>Current oscillation offset in pixels (at most the radius either way).</summary>
    public int CurrentYOffset { get; internal set; }

    /// <summary>Advances the oscillation phase by <see cref="AngleDelta"/>, wrapping after a full turn.</summary>
    internal void AdvanceAngle() => Angle = Angle.AddTableUnits(AngleDelta);

    /// <summary>Empties the values the cartridge keeps in the enemy's common record, when a kill empties that record.</summary>
    internal void ClearWithRecord()
    {
        Function = null;
        RightVelocity = SnesSignedSixteenSixteen.Zero;
        LeftVelocity = SnesSignedSixteenSixteen.Zero;
        Angle = SnesAngle.Zero;
    }
}
