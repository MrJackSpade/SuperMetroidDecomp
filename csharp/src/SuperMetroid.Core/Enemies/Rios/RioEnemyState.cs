using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The state of one Rio. The velocities, function and animation handshake are values the cartridge keeps in the
/// enemy's common record, so they are emptied with that record when the enemy is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class RioEnemyState
{
    /// <summary>Where the typed animation (#641) resumes.</summary>
    internal AnimationProgramCursor<RioProgram> Program { get; set; }

    /// <summary>Signed 8.8 horizontal dive velocity retained while the hovering phase temporarily stops X.</summary>
    public SignedEightEight SavedHorizontalVelocity { get; internal set; }

    /// <summary>The behaviour dispatched after the main AI's RNG advance; none once a kill has emptied the record.</summary>
    public RioEnemyFunction? Function { get; internal set; }

    /// <summary>Signed 8.8 velocity; the horizontal component feeds the 16.16 collision helper.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>Handshake set by the animation-finished cue and consumed by main AI.</summary>
    public bool AnimationFinished { get; internal set; }

    /// <summary>The last animation installed by a behavioural transition; none at room load.</summary>
    internal RioProgram? InstalledProgram { get; set; }

    /// <summary>Empties the values the cartridge keeps in the enemy's common record, when a kill empties that record.</summary>
    internal void ClearWithRecord()
    {
        SavedHorizontalVelocity = default;
        Function = null;
        Velocity = new EightEightVector<RoomPixels>(0, 0);
        AnimationFinished = false;
    }
}
