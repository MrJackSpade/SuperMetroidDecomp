using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>
/// The state of one Waver: its horizontal velocity, wave phase and animation selection. Every value here is one the
/// cartridge keeps in the enemy's common record, so it is emptied with that record when the enemy is killed
/// (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class WaverEnemyState
{
    /// <summary>Where the typed animation (#641) resumes.</summary>
    internal AnimationProgramCursor<WaverProgram> Program { get; set; }

    /// <summary>Signed 16.16 horizontal displacement per update, one and a half pixels either way; a wall negates it.</summary>
    public SnesSignedSixteenSixteen XVelocity { get; internal set; }

    /// <summary>The animation selection currently installed.</summary>
    public WaverAnimationSelector CurrentInstructionListIndex { get; internal set; }

    /// <summary>
    /// The vertical wave's phase; only its table index is read, so it wraps after a full turn. It advances two table
    /// units per unobstructed update and a vertical collision reflects it by half a turn.
    /// </summary>
    public SnesAngle Angle { get; internal set; }

    /// <summary>Set by the spin animation's last cue after the four-frame spin completes.</summary>
    public bool SpinFinished { get; internal set; }

    /// <summary>Requested animation selection: one flag selects facing, the other the temporary spin family.</summary>
    public WaverAnimationSelector RequestedInstructionListIndex { get; internal set; }

    /// <summary>Advances the wave phase two table units, wrapping after a full turn.</summary>
    internal void AdvanceWave() => Angle = Angle.AddTableUnits(2);

    /// <summary>Reflects the wave phase by half a turn, as a vertical collision does.</summary>
    internal void ReflectWave() => Angle = Angle.AddTableUnits(EnemyByteAngleDefinitions.HalfTurn);

    /// <summary>Empties every value, when a kill empties the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        XVelocity = SnesSignedSixteenSixteen.Zero;
        CurrentInstructionListIndex = WaverAnimationSelector.None;
        Angle = SnesAngle.Zero;
        SpinFinished = false;
        RequestedInstructionListIndex = WaverAnimationSelector.None;
    }
}
