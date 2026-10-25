using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>
/// The state of one Stoke: its walking speeds, facing and behaviour. Every value here is one the cartridge keeps in
/// the enemy's common record, so it is emptied with that record when the enemy is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class StokeEnemyState
{
    /// <summary>Where the typed animation (#641) resumes.</summary>
    internal AnimationProgramCursor<StokeProgram> Program { get; set; }

    /// <summary>Signed 16.16 room-pixel displacement per walking update to the right, from the positive half of the population's linear-speed record.</summary>
    public SignedSixteenSixteen RightVelocity { get; internal set; }

    /// <summary>Signed 16.16 room-pixel displacement per walking update to the left, from the negative half of the population's linear-speed record.</summary>
    public SignedSixteenSixteen LeftVelocity { get; internal set; }

    /// <summary>Facing latch changed by animation cues, one update after a turn request; initialized from population parameter 1.</summary>
    public StokeDirection Direction { get; internal set; }

    /// <summary>The current walking or attacking behaviour; none once a kill has emptied the record.</summary>
    public StokeAiFunction? Function { get; internal set; }

    /// <summary>Empties every value, when a kill empties the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        RightVelocity = SignedSixteenSixteen.Zero;
        LeftVelocity = SignedSixteenSixteen.Zero;
        Direction = StokeDirection.Left;
        Function = null;
    }
}
