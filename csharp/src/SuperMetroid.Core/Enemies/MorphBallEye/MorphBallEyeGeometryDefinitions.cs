namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>Directional mounting for the Morph Ball surveillance eye: each mount is one tile from the eye's centre.</summary>
internal static class MorphBallEyeGeometryDefinitions
{
    /// <summary>The mount a tile to the left of the eye's centre, facing left.</summary>
    internal static readonly MorphBallEyeMountDefinition LeftMount = new(new(-8, 0), MorphBallEyeSegment.MountFacingLeft);

    /// <summary>The mount a tile to the right of the eye's centre, facing right.</summary>
    internal static readonly MorphBallEyeMountDefinition RightMount = new(new(8, 0), MorphBallEyeSegment.MountFacingRight);

    /// <summary>The mount a tile above the eye's centre, facing up.</summary>
    internal static readonly MorphBallEyeMountDefinition AboveMount = new(new(0, -8), MorphBallEyeSegment.MountFacingUp);

    /// <summary>The mount a tile below the eye's centre, facing down.</summary>
    internal static readonly MorphBallEyeMountDefinition BelowMount = new(new(0, 8), MorphBallEyeSegment.MountFacingDown);
}
