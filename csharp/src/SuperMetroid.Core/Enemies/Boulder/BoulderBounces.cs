namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>Chooses the quadratic curve of a Boulder's next bounce.</summary>
public static class BoulderBounces
{
    /// <summary>The number of ground impacts after which the Boulder stops bouncing and rolls.</summary>
    public static int ImpactCount => BoulderBounceDefinitions.ImpactRestarts.Length;

    /// <summary>The curve place the impact after <paramref name="impactsSoFar"/> earlier ones restarts the bounce from.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="impactsSoFar"/> is outside zero through two.</exception>
    public static QuadraticSpeedCurvePlace RestartPlace(int impactsSoFar) =>
        (uint)impactsSoFar < (uint)BoulderBounceDefinitions.ImpactRestarts.Length
            ? BoulderBounceDefinitions.ImpactRestarts[impactsSoFar]
            : throw new ArgumentOutOfRangeException(nameof(impactsSoFar));
}
