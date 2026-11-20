using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>How a Boulder falls, bounces, rolls and breaks (#2279).</summary>
internal static class BoulderMotionDefinitions
{
    /// <summary>
    /// The largest bounce clock: the place past the curve's last mark, which the curve rejects before another increment.
    /// </summary>
    internal static readonly QuadraticSpeedCurvePlace LargestBounceSpeed = QuadraticSpeedCurvePlace.PastLastMark;

    /// <summary>The bounce clock's place the first rebound starts from.</summary>
    internal static readonly QuadraticSpeedCurvePlace FirstReboundBounceSpeed = QuadraticSpeedCurvePlace.FromMarks(32);

    /// <summary>The change in the bounce clock per falling or rebounding update: one curve mark.</summary>
    internal static readonly QuadraticSpeedCurvePlace BounceSpeedStep = QuadraticSpeedCurvePlace.OneMark;

    /// <summary>
    /// The largest clock the initial fall, and the travel clock while bouncing, reach: the travel clock's larger cap.
    /// </summary>
    internal static readonly QuadraticSpeedCurvePlace MaximumBouncingSpeed = QuadraticSpeedCurvePlace.FromMarks(80);

    /// <summary>The change in the travel clock per bouncing update, an eighth of a mark.</summary>
    internal static readonly QuadraticSpeedCurvePlace BouncingTravelSpeedStep = QuadraticSpeedCurvePlace.FromMarkFraction(1, 8);

    /// <summary>The change in the travel clock per rolling update, a quarter of a mark.</summary>
    internal static readonly QuadraticSpeedCurvePlace RollingTravelSpeedStep = QuadraticSpeedCurvePlace.FromMarkFraction(1, 4);

    /// <summary>The largest travel clock while rolling.</summary>
    internal static readonly QuadraticSpeedCurvePlace MaximumRollingTravelSpeed = QuadraticSpeedCurvePlace.FromMarks(64);

    /// <summary>
    /// The pixels a Boulder that rolls at once hangs above its resting place: the game raises it as it raises a falling one,
    /// by the least it can.
    /// </summary>
    internal const int RollingStartLift = 1;

    /// <summary>
    /// The pixels below its resting place a rolling Boulder's ground probe reaches, until the rolling stops changing its
    /// height.
    /// </summary>
    internal const int RollingGroundReach = 2;

    /// <summary>The dust a Boulder breaks into at its terminal impact.</summary>
    internal const DustExplosionAnimation BreakDust = DustExplosionAnimation.RockParticles;
}
