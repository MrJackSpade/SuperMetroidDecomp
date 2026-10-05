namespace SuperMetroid.Core.Game;

/// <summary>Directional mounting and yellow beam intensity for the Morph Ball surveillance eye.</summary>
internal static class MorphBallEyeGeometryDefinitions
{
    /// <summary>
    /// InitAI_Eye at $A8:9058 selects offsets at $90CA/$90D2 and programs at $90DA.
    /// Parameter two's low nibble names left, right, up or down; the mount is one tile
    /// from the eye center along that direction.
    /// </summary>
    internal static (short X, short Y, ushort Program) Mount(int direction) => direction switch
    {
        0 => (-8, 0, MorphBallEyeInstructionProgramDefinitions.MountFacingLeft),
        1 => (8, 0, MorphBallEyeInstructionProgramDefinitions.MountFacingRight),
        2 => (0, -8, MorphBallEyeInstructionProgramDefinitions.MountFacingUp),
        3 => (0, 8, MorphBallEyeInstructionProgramDefinitions.MountFacingDown),
        _ => throw new InvalidDataException($"Morph-ball eye mount direction {direction} exceeds its four directions."),
    };

    /// <summary>
    /// $88:EA8B interleaves red/green COLDATA bytes, zero blue intensity and padding.
    /// Both active channels trace intensity16 down to8 and back to15 in sixteen steps.
    /// Channel selector bits must remain present because the native fade decrements raw bytes.
    /// </summary>
    internal static (byte Red, byte Green) BeamColor(int phase)
    {
        if ((uint)phase >= 16) throw new ArgumentOutOfRangeException(nameof(phase));
        int intensity = 8 + Math.Abs(phase - 8);
        return ((byte)(0x20 | intensity), (byte)(0x40 | intensity));
    }
}
