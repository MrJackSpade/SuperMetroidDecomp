namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>Where each of a Boulder's ground impacts restarts its bounce on the speed curve.</summary>
public static class BoulderBounceDefinitions
{
    /// <summary>
    /// The bounce clock's place each impact restarts it from, in the order the impacts happen: the first two bounce it
    /// back up from marks 24 and 16, and the third installs the curve's start as it settles into rolling.
    /// </summary>
    internal static readonly QuadraticSpeedCurvePlace[] ImpactRestarts =
        [QuadraticSpeedCurvePlace.FromMarks(24), QuadraticSpeedCurvePlace.FromMarks(16), QuadraticSpeedCurvePlace.Start];
}
