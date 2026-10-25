using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>Directional mounting and yellow beam intensity for the Morph Ball surveillance eye.</summary>
internal static class MorphBallEyeGeometryDefinitions
{
    /// <summary>
    /// InitAI_Eye at $A8:9058 selects offsets at $90CA/$90D2 and programs at $90DA.
    /// Parameter two's low nibble names left, right, up or down; the mount is one tile
    /// from the eye center along that direction.
    /// </summary>
    internal static MorphBallEyeMountDefinition Mount(int direction) => direction switch
    {
        0 => new(new(-8, 0), MorphBallEyeProgram.MountFacingLeft),
        1 => new(new(8, 0), MorphBallEyeProgram.MountFacingRight),
        2 => new(new(0, -8), MorphBallEyeProgram.MountFacingUp),
        3 => new(new(0, 8), MorphBallEyeProgram.MountFacingDown),
        _ => throw new InvalidDataException($"Morph-ball eye mount direction {direction} exceeds its four directions."),
    };

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
