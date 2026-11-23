namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// One of the places a Powamp balloon's animation starts (#2553): the balloon segment it plays from and where that start
/// puts the balloon's sequence, so a balloon can only be started from its own animations.
/// </summary>
/// <param name="Segment">The balloon segment the animation plays from.</param>
/// <param name="Inflation">Where the start puts the balloon's inflate or sink sequence.</param>
internal sealed partial record PowampBalloonStart(PowampBalloonSegment Segment, PowampBalloonInflation Inflation);
