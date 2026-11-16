namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>Where the Morph Ball surveillance eye mounts.</summary>
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
}
