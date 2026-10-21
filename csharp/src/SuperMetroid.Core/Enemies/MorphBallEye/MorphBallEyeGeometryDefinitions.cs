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
    /// $88:EA8B interleaves red/green COLDATA bytes, zero blue intensity and padding.
    /// Both active channels trace intensity16 down to8 and back to15 in sixteen steps.
    /// Channel selector bits must remain present because the native fade decrements raw bytes.
    /// </summary>
    internal static RedGreenLevels BeamColor(int phase)
    {
        if ((uint)phase >= 16) throw new ArgumentOutOfRangeException(nameof(phase));
        int intensity = 8 + Math.Abs(phase - 8);
        return new RedGreenLevels((byte)(0x20 | intensity), (byte)(0x40 | intensity));
    }
}
