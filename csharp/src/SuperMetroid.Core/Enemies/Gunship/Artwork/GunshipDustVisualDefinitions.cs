using P = SuperMetroid.Core.Enemies.Gunship.GunshipDustProgram;

namespace SuperMetroid.Core.Enemies.Gunship.Artwork;

/// <summary>The gunship liftoff dust clouds' installed frames and the projectile definition's native list identity.</summary>
internal static class GunshipDustVisualDefinitions
{
    /// <summary>
    /// <c>InstList_EnemyProjectile_GunshipLiftoffDustClouds_Index0_0</c> at $86:A197: the list the dust's projectile definition
    /// names. Spawning starts the parameter's typed program instead.
    /// </summary>
    internal const ushort DefinitionList = 0xa197;

    /// <summary>The frames in installed order.</summary>
    internal static IReadOnlyList<GunshipDustFrame> Frames { get; } = [new(P.RightInnerLoop, 0), new(P.RightInnerLoop, 1), new(P.RightInnerLoop, 2), new(P.RightInnerLoop, 3), new(P.RightInnerLoop, 4), new(P.RightInnerLoop, 5), new(P.RightInnerLoop, 6), new(P.RightInnerLoop, 7), new(P.RightMiddleLoop, 0), new(P.RightMiddleLoop, 1), new(P.RightMiddleLoop, 2), new(P.RightMiddleLoop, 3), new(P.RightMiddleLoop, 4), new(P.RightMiddleLoop, 5), new(P.RightMiddleLoop, 6), new(P.RightMiddleLoop, 7), new(P.RightOuterLoop, 0), new(P.RightOuterLoop, 1), new(P.RightOuterLoop, 2), new(P.RightOuterLoop, 3), new(P.RightOuterLoop, 4), new(P.RightOuterLoop, 5), new(P.RightOuterLoop, 6), new(P.LeftInnerLoop, 0), new(P.LeftInnerLoop, 1), new(P.LeftInnerLoop, 2), new(P.LeftInnerLoop, 3), new(P.LeftInnerLoop, 4), new(P.LeftInnerLoop, 5), new(P.LeftInnerLoop, 6), new(P.LeftInnerLoop, 7), new(P.LeftMiddleLoop, 0), new(P.LeftMiddleLoop, 1), new(P.LeftMiddleLoop, 2), new(P.LeftMiddleLoop, 3), new(P.LeftMiddleLoop, 4), new(P.LeftMiddleLoop, 5), new(P.LeftMiddleLoop, 6), new(P.LeftMiddleLoop, 7), new(P.LeftOuterLoop, 0), new(P.LeftOuterLoop, 1), new(P.LeftOuterLoop, 2), new(P.LeftOuterLoop, 3), new(P.LeftOuterLoop, 4), new(P.LeftOuterLoop, 5), new(P.LeftOuterLoop, 6)];

    /// <summary>The installed artwork key of <paramref name="frame"/>: its native operand's address.</summary>
    internal static ushort PresentationKey(GunshipDustFrame frame) => (frame.Program, frame.Phase) switch
    {
        (P.RightInnerLoop, 0) => 0xa19d,
        (P.RightInnerLoop, 1) => 0xa1a1,
        (P.RightInnerLoop, 2) => 0xa1a5,
        (P.RightInnerLoop, 3) => 0xa1a9,
        (P.RightInnerLoop, 4) => 0xa1ad,
        (P.RightInnerLoop, 5) => 0xa1b1,
        (P.RightInnerLoop, 6) => 0xa1b5,
        (P.RightInnerLoop, 7) => 0xa1b9,
        (P.RightMiddleLoop, 0) => 0xa1c7,
        (P.RightMiddleLoop, 1) => 0xa1cb,
        (P.RightMiddleLoop, 2) => 0xa1cf,
        (P.RightMiddleLoop, 3) => 0xa1d3,
        (P.RightMiddleLoop, 4) => 0xa1d7,
        (P.RightMiddleLoop, 5) => 0xa1db,
        (P.RightMiddleLoop, 6) => 0xa1df,
        (P.RightMiddleLoop, 7) => 0xa1e3,
        (P.RightOuterLoop, 0) => 0xa1f1,
        (P.RightOuterLoop, 1) => 0xa1f5,
        (P.RightOuterLoop, 2) => 0xa1f9,
        (P.RightOuterLoop, 3) => 0xa1fd,
        (P.RightOuterLoop, 4) => 0xa201,
        (P.RightOuterLoop, 5) => 0xa205,
        (P.RightOuterLoop, 6) => 0xa209,
        (P.LeftInnerLoop, 0) => 0xa217,
        (P.LeftInnerLoop, 1) => 0xa21b,
        (P.LeftInnerLoop, 2) => 0xa21f,
        (P.LeftInnerLoop, 3) => 0xa223,
        (P.LeftInnerLoop, 4) => 0xa227,
        (P.LeftInnerLoop, 5) => 0xa22b,
        (P.LeftInnerLoop, 6) => 0xa22f,
        (P.LeftInnerLoop, 7) => 0xa233,
        (P.LeftMiddleLoop, 0) => 0xa241,
        (P.LeftMiddleLoop, 1) => 0xa245,
        (P.LeftMiddleLoop, 2) => 0xa249,
        (P.LeftMiddleLoop, 3) => 0xa24d,
        (P.LeftMiddleLoop, 4) => 0xa251,
        (P.LeftMiddleLoop, 5) => 0xa255,
        (P.LeftMiddleLoop, 6) => 0xa259,
        (P.LeftMiddleLoop, 7) => 0xa25d,
        (P.LeftOuterLoop, 0) => 0xa26b,
        (P.LeftOuterLoop, 1) => 0xa26f,
        (P.LeftOuterLoop, 2) => 0xa273,
        (P.LeftOuterLoop, 3) => 0xa277,
        (P.LeftOuterLoop, 4) => 0xa27b,
        (P.LeftOuterLoop, 5) => 0xa27f,
        (P.LeftOuterLoop, 6) => 0xa283,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined gunship dust frame."),
    };
}
