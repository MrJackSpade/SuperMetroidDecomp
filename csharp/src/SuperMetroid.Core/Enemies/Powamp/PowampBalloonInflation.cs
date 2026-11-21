using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// Where a Powamp's balloon stands in its inflate or sink sequence (#1845): which way it goes, and the pose it stands at.
/// The balloon's animation shows the poses; each frame it shows is the pose the balloon then stands at, and before a
/// start's first frame shows, the balloon stands at the pose the start names. The body reads this record to place the
/// balloon and to restart a sinking one when it dies.
/// </summary>
/// <param name="Sequence">Which way the balloon goes through its poses.</param>
/// <param name="Pose">
/// The pose whose height the balloon sits at: the latest its animation has shown, or, before it shows one, the start's.
/// </param>
/// <remarks>
/// The game keeps the balloon's place in its animation instead and reads the pose from it; every place
/// a played animation holds reads one of the three poses. A start's place before its first frame is not one the death
/// reads: the balloon plays before its body in the enemy pass, so its first frame shows before the body's next update,
/// a frozen balloon shares its body's freeze and thaws as the body's next update begins, and every Powamp is processed
/// off screen (#2553).
/// </remarks>
public readonly record struct PowampBalloonInflation(PowampBalloonSequence Sequence, PowampBalloonPose Pose)
{
    /// <summary>The record once the balloon's animation shows <paramref name="frame"/>, the pose it then stands at.</summary>
    internal PowampBalloonInflation Showing(EnemyFrameId frame) => this with { Pose = PowampMotionDefinitions.PosesShownBy[frame] };
}
