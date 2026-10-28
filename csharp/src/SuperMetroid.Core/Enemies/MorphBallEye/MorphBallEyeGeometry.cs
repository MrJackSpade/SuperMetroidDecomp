using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>Where the Morph Ball surveillance eye mounts, and its beam's intensity.</summary>
internal static class MorphBallEyeGeometry
{
    /// <summary>The mount parameter two's low nibble <paramref name="direction"/> selects.</summary>
    /// <exception cref="InvalidDataException"><paramref name="direction"/> is not one of the four directions.</exception>
    internal static MorphBallEyeMountDefinition Mount(int direction) =>
        (uint)direction < (uint)MorphBallEyeGeometryDefinitions.Mounts.Length
            ? MorphBallEyeGeometryDefinitions.Mounts[direction]
            : throw new InvalidDataException($"Morph-ball eye mount direction {direction} exceeds its four directions.");

    /// <summary>
    /// $88:EA8B interleaves the red and green intensities, a dark blue and padding.
    /// Red and green trace intensity 16 down to 8 and back to 15 in sixteen steps; blue stays dark.
    /// </summary>
    internal static RgbColor BeamColor(int phase)
    {
        if ((uint)phase >= 16) throw new ArgumentOutOfRangeException(nameof(phase));
        int intensity = 8 + Math.Abs(phase - 8);
        return new RgbColor(intensity, intensity, 0);
    }
}
