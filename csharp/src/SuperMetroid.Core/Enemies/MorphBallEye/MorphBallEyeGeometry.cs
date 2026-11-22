namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>Where the Morph Ball surveillance eye mounts.</summary>
internal static partial class MorphBallEyeGeometry
{
    /// <summary>The mount of each side.</summary>
    private static IdentityTable<MorphBallEyeMountSide, MorphBallEyeMountDefinition> MountBySide => field ??= new(new Dictionary<MorphBallEyeMountSide, MorphBallEyeMountDefinition>
    {
        [MorphBallEyeMountSide.Left] = MorphBallEyeGeometryDefinitions.LeftMount,
        [MorphBallEyeMountSide.Right] = MorphBallEyeGeometryDefinitions.RightMount,
        [MorphBallEyeMountSide.Above] = MorphBallEyeGeometryDefinitions.AboveMount,
        [MorphBallEyeMountSide.Below] = MorphBallEyeGeometryDefinitions.BelowMount,
    });

    /// <summary>The offset and animation of the mount on <paramref name="side"/> of the eye's centre.</summary>
    internal static MorphBallEyeMountDefinition Mount(MorphBallEyeMountSide side) => MountBySide[side];
}
