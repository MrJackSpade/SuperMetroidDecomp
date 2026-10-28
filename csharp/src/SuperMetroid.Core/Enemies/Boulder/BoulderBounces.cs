namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>Chooses the quadratic curve of a Boulder's next bounce.</summary>
public static class BoulderBounces
{
    /// <summary>The curve index for the impact with <paramref name="remainingBounces"/> bounces left.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="remainingBounces"/> is outside zero through two.</exception>
    public static ushort SpeedIndex(int remainingBounces) =>
        (uint)remainingBounces < (uint)BoulderBounceDefinitions.SpeedIndices.Length
            ? BoulderBounceDefinitions.SpeedIndices[remainingBounces]
            : throw new ArgumentOutOfRangeException(nameof(remainingBounces));
}
