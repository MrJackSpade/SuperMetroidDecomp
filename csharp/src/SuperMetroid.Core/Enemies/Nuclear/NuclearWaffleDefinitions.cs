namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>Compiled authored definitions for Puromi/Nuclear Waffle.</summary>
internal static class NuclearWaffleDefinitions
{

    /// <summary>The Sidehopper landing sound, queued when a joint makes its rising turn.</summary>
    internal static readonly SoundEffectId TurnSoundEffect = EnemySound.LowThud;

    /// <summary>
    /// Where the two sweeps mirror: straight up, counted in the turn after the first, a full turn and a quarter on from
    /// zero, so each sweep crosses the top of the orbit without its angle wrapping.
    /// </summary>
    private static readonly NuclearWaffleSweepAngle SweepCenter =
        NuclearWaffleSweepAngle.FromSteps(RotationAngle.StepsPerTurn + RotationAngle.StepsPerTurn / 4);

    /// <summary>
    /// The steps each sweep reaches to either side of its centre: five sixteenths of a turn, from a sixteenth of a turn
    /// below one side of the orbit to a sixteenth below the other.
    /// </summary>
    private const int SweepReachSteps = 5 * RotationAngle.StepsPerTurn / 16;

    /// <summary>
    /// The steps the joint-turn thresholds lie to either side of the centre: a quarter turn, level with the orbit's centre
    /// on either side, where a link passes from rising to falling.
    /// </summary>
    private const int TurnReachSteps = RotationAngle.StepsPerTurn / 4;

    /// <summary>The steps between same-kind links, with the interleaved links halfway between: three thirty-seconds of a turn.</summary>
    private const int SegmentPitchSteps = 3 * RotationAngle.StepsPerTurn / 32;

    /// <summary>
    /// The backward sweep, with its endpoints, link spacing and turn thresholds: from the far side of the center back,
    /// its links trailing.
    /// </summary>
    internal static readonly NuclearWaffleSweepDefinition BackwardSweep =
        new(SweepCenter.TurnedBySteps(SweepReachSteps), SweepCenter.TurnedBySteps(-SweepReachSteps), -SegmentPitchSteps, -SegmentPitchSteps / 2,
            SweepCenter.TurnedBySteps(TurnReachSteps), SweepCenter.TurnedBySteps(-TurnReachSteps));

    /// <summary>The forward sweep: the mirror of <see cref="BackwardSweep"/>.</summary>
    internal static readonly NuclearWaffleSweepDefinition ForwardSweep =
        new(SweepCenter.TurnedBySteps(-SweepReachSteps), SweepCenter.TurnedBySteps(SweepReachSteps), SegmentPitchSteps, SegmentPitchSteps / 2,
            SweepCenter.TurnedBySteps(-TurnReachSteps), SweepCenter.TurnedBySteps(TurnReachSteps));
}
