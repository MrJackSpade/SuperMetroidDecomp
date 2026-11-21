namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// Where a Powamp's balloon stands in its inflate or sink sequence (#1845): which way it goes, and how many of the
/// sequence's three poses it has shown. The balloon's animation shows the poses; the balloon records each one as it shows,
/// and the body reads this record to place the balloon and to restart a sinking one when it dies.
/// </summary>
/// <param name="Sequence">Which way the balloon goes through its poses.</param>
/// <param name="PosesShown">How many of the sequence's poses it has shown, zero through three.</param>
public readonly partial record struct PowampBalloonInflation(PowampBalloonSequence Sequence, int PosesShown)
{
    /// <summary>
    /// The pose whose height the balloon sits at: the latest it has shown, or the sequence's first before it shows one.
    /// </summary>
    public PowampBalloonPose Pose => Poses[PosesShown == 0 ? 0 : PosesShown - 1];

    /// <summary>The record once the balloon has shown its next pose.</summary>
    /// <exception cref="InvalidOperationException">The balloon has already shown all three poses of its sequence.</exception>
    internal PowampBalloonInflation AfterShowingPose() =>
        PosesShown < Poses.Count
            ? this with { PosesShown = PosesShown + 1 }
            : throw new InvalidOperationException($"A Powamp balloon {Sequence} showed a pose after all {Poses.Count} of its sequence.");

    /// <summary>The sequence's poses in the order the balloon shows them.</summary>
    private IReadOnlyList<PowampBalloonPose> Poses => Sequence == PowampBalloonSequence.Sinking
        ? PowampMotionDefinitions.SinkingPoses
        : PowampMotionDefinitions.InflatingPoses;
}
