using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>Where the Morph Ball surveillance eye mounts, and its beam's intensity.</summary>
internal static class MorphBallEyeGeometry
{
    /// <summary>The offset and program of the mount on <paramref name="side"/> of the eye's centre.</summary>
    internal static MorphBallEyeMountDefinition Mount(MorphBallEyeMountSide side) => side switch
    {
        MorphBallEyeMountSide.Left => MorphBallEyeGeometryDefinitions.LeftMount,
        MorphBallEyeMountSide.Right => MorphBallEyeGeometryDefinitions.RightMount,
        MorphBallEyeMountSide.Above => MorphBallEyeGeometryDefinitions.AboveMount,
        MorphBallEyeMountSide.Below => MorphBallEyeGeometryDefinitions.BelowMount,
        _ => throw new ArgumentOutOfRangeException(nameof(side), side, "Undefined morph-ball eye mount side."),
    };

    /// <summary>
    /// The beam colour table interleaves the red and green intensities, a dark blue and padding.
    /// Red and green trace intensity 16 down to 8 and back to 15 in sixteen steps; blue stays dark.
    /// </summary>
    internal static RgbColor BeamColor(int phase)
    {
        if ((uint)phase >= 16) throw new ArgumentOutOfRangeException(nameof(phase));
        int intensity = 8 + Math.Abs(phase - 8);
        return new RgbColor(intensity, intensity, 0);
    }
}
