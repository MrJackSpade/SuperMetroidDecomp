namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>Directional mounting and yellow beam intensity for the Morph Ball surveillance eye.</summary>
internal static class MorphBallEyeGeometryDefinitions
{
    /// <summary>
    /// InitAI_Eye at $A8:9058 selects offsets at $90CA/$90D2 and programs at $90DA.
    /// Parameter two's low nibble names left, right, up or down; the mount is one tile
    /// from the eye center along that direction.
    /// </summary>
    internal static readonly MorphBallEyeMountDefinition[] Mounts =
    [
        new(new(-8, 0), MorphBallEyeProgram.MountFacingLeft),
        new(new(8, 0), MorphBallEyeProgram.MountFacingRight),
        new(new(0, -8), MorphBallEyeProgram.MountFacingUp),
        new(new(0, 8), MorphBallEyeProgram.MountFacingDown),
    ];
}
