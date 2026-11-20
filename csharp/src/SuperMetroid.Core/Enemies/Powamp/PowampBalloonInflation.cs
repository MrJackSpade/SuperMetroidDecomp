namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// Where a Powamp's balloon stands in its inflate or sink sequence (#1845): which way it goes, and how many of the
/// sequence's three poses it has shown. The balloon's animation shows the poses; the balloon records each one as it shows,
/// and the body reads this record to place the balloon and to restart a sinking one when it dies.
/// </summary>
/// <param name="Sequence">Which way the balloon goes through its poses.</param>
/// <param name="PosesShown">
/// How many of the sequence's poses it has shown: its animation's place in the sequence, which shows each of the three at
/// most once and then holds the last.
/// </param>
public readonly record struct PowampBalloonInflation(PowampBalloonSequence Sequence, int PosesShown)
{
    /// <summary>
    /// The pose whose height the balloon sits at: the latest it has shown, or the sequence's first before it shows one. As
    /// the game reads the balloon's place in its animation, a place past the sequence's poses reads as its first.
    /// </summary>
    public PowampBalloonPose Pose => PosesShown >= 1 && PosesShown <= Poses.Count ? Poses[PosesShown - 1] : Poses[0];

    /// <summary>The record once the balloon has shown its next pose.</summary>
    internal PowampBalloonInflation AfterShowingPose() => this with { PosesShown = PosesShown + 1 };

    /// <summary>The sequence's poses in the order the balloon shows them.</summary>
    private IReadOnlyList<PowampBalloonPose> Poses => Sequence == PowampBalloonSequence.Sinking
        ? PowampMotionDefinitions.SinkingPoses
        : PowampMotionDefinitions.InflatingPoses;
}
