namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>Directional mounting and yellow beam intensity for the Morph Ball surveillance eye.</summary>
internal static class MorphBallEyeGeometryDefinitions
{
    /// <summary>
    /// The eye's initialization selects offsets and programs.
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
