using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>Where the Morph Ball surveillance eye mounts, and its beam's intensity.</summary>
internal static class MorphBallEyeGeometry
{
    /// <summary>The offset and animation of the mount on <paramref name="side"/> of the eye's centre.</summary>
    internal static MorphBallEyeMountDefinition Mount(MorphBallEyeMountSide side) => side switch
    {
        MorphBallEyeMountSide.Left => MorphBallEyeGeometryDefinitions.LeftMount,
        MorphBallEyeMountSide.Right => MorphBallEyeGeometryDefinitions.RightMount,
        MorphBallEyeMountSide.Above => MorphBallEyeGeometryDefinitions.AboveMount,
        MorphBallEyeMountSide.Below => MorphBallEyeGeometryDefinitions.BelowMount,
        _ => throw new ArgumentOutOfRangeException(nameof(side), side, "Undefined morph-ball eye mount side."),
    };

    /// <summary>
    /// The beam's brightness cycle: red and green trace level 16 down to 8 and back to 15 in sixteen steps of the
    /// game's authored levels; blue stays none.
    /// </summary>
    internal static RgbColor BeamColor(int phase)
    {
        if ((uint)phase >= 16) throw new ArgumentOutOfRangeException(nameof(phase));
        int intensity = 8 + Math.Abs(phase - 8);
        return new FadeLevelColour(intensity, intensity, 0).ToColour();
    }
}
