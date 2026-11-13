namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The five trail animations as their steps: timed poses, one-pixel moves and the end.</summary>
internal static class ProjectileTrailAnimationDefinitions
{
    /// <summary>Ends a trail without drawing another frame.</summary>
    private static readonly ProjectileTrailStep[] Empty =
    [
        new ProjectileTrailStep.End(),
    ];

    /// <summary>
    /// The left ice sparkle's seventeen poses. It first falls a pixel after six poses, then after two more, then
    /// after every remaining pose; the last pose holds for four updates.
    /// </summary>
    private static readonly ProjectileTrailStep[] LeftIce =
    [
        new ProjectileTrailStep.Pose(1, ProjectileTrailFrame.LeftIce0),
        new ProjectileTrailStep.Pose(1, ProjectileTrailFrame.LeftIce1),
        new ProjectileTrailStep.Pose(1, ProjectileTrailFrame.LeftIce2),
        new ProjectileTrailStep.Pose(1, ProjectileTrailFrame.LeftIce3),
        new ProjectileTrailStep.Pose(1, ProjectileTrailFrame.LeftIce4),
        new ProjectileTrailStep.Pose(1, ProjectileTrailFrame.LeftIce5),
        new ProjectileTrailStep.Move(ProjectileTrailCue.MoveLeftDown),
        new ProjectileTrailStep.Pose(1, ProjectileTrailFrame.LeftIce6),
        new ProjectileTrailStep.Pose(1, ProjectileTrailFrame.LeftIce7),
        new ProjectileTrailStep.Move(ProjectileTrailCue.MoveLeftDown),
        new ProjectileTrailStep.Pose(1, ProjectileTrailFrame.LeftIce8),
        new ProjectileTrailStep.Move(ProjectileTrailCue.MoveLeftDown),
        new ProjectileTrailStep.Pose(1, ProjectileTrailFrame.LeftIce9),
        new ProjectileTrailStep.Move(ProjectileTrailCue.MoveLeftDown),
        new ProjectileTrailStep.Pose(1, ProjectileTrailFrame.LeftIce10),
        new ProjectileTrailStep.Move(ProjectileTrailCue.MoveLeftDown),
        new ProjectileTrailStep.Pose(1, ProjectileTrailFrame.LeftIce11),
        new ProjectileTrailStep.Move(ProjectileTrailCue.MoveLeftDown),
        new ProjectileTrailStep.Pose(1, ProjectileTrailFrame.LeftIce12),
        new ProjectileTrailStep.Move(ProjectileTrailCue.MoveLeftDown),
        new ProjectileTrailStep.Pose(1, ProjectileTrailFrame.LeftIce13),
        new ProjectileTrailStep.Move(ProjectileTrailCue.MoveLeftDown),
        new ProjectileTrailStep.Pose(1, ProjectileTrailFrame.LeftIce14),
        new ProjectileTrailStep.Move(ProjectileTrailCue.MoveLeftDown),
        new ProjectileTrailStep.Pose(1, ProjectileTrailFrame.LeftIce15),
        new ProjectileTrailStep.Move(ProjectileTrailCue.MoveLeftDown),
        new ProjectileTrailStep.Pose(4, ProjectileTrailFrame.LeftIce16),
        new ProjectileTrailStep.End(),
    ];

    /// <summary>The right ice sparkle, the left sparkle's poses falling on the right stream.</summary>
    private static readonly ProjectileTrailStep[] RightIce =
    [
        new ProjectileTrailStep.Pose(1, ProjectileTrailFrame.RightIce0),
        new ProjectileTrailStep.Pose(1, ProjectileTrailFrame.RightIce1),
        new ProjectileTrailStep.Pose(1, ProjectileTrailFrame.RightIce2),
        new ProjectileTrailStep.Pose(1, ProjectileTrailFrame.RightIce3),
        new ProjectileTrailStep.Pose(1, ProjectileTrailFrame.RightIce4),
        new ProjectileTrailStep.Pose(1, ProjectileTrailFrame.RightIce5),
        new ProjectileTrailStep.Move(ProjectileTrailCue.MoveRightDown),
        new ProjectileTrailStep.Pose(1, ProjectileTrailFrame.RightIce6),
        new ProjectileTrailStep.Pose(1, ProjectileTrailFrame.RightIce7),
        new ProjectileTrailStep.Move(ProjectileTrailCue.MoveRightDown),
        new ProjectileTrailStep.Pose(1, ProjectileTrailFrame.RightIce8),
        new ProjectileTrailStep.Move(ProjectileTrailCue.MoveRightDown),
        new ProjectileTrailStep.Pose(1, ProjectileTrailFrame.RightIce9),
        new ProjectileTrailStep.Move(ProjectileTrailCue.MoveRightDown),
        new ProjectileTrailStep.Pose(1, ProjectileTrailFrame.RightIce10),
        new ProjectileTrailStep.Move(ProjectileTrailCue.MoveRightDown),
        new ProjectileTrailStep.Pose(1, ProjectileTrailFrame.RightIce11),
        new ProjectileTrailStep.Move(ProjectileTrailCue.MoveRightDown),
        new ProjectileTrailStep.Pose(1, ProjectileTrailFrame.RightIce12),
        new ProjectileTrailStep.Move(ProjectileTrailCue.MoveRightDown),
        new ProjectileTrailStep.Pose(1, ProjectileTrailFrame.RightIce13),
        new ProjectileTrailStep.Move(ProjectileTrailCue.MoveRightDown),
        new ProjectileTrailStep.Pose(1, ProjectileTrailFrame.RightIce14),
        new ProjectileTrailStep.Move(ProjectileTrailCue.MoveRightDown),
        new ProjectileTrailStep.Pose(1, ProjectileTrailFrame.RightIce15),
        new ProjectileTrailStep.Move(ProjectileTrailCue.MoveRightDown),
        new ProjectileTrailStep.Pose(4, ProjectileTrailFrame.RightIce16),
        new ProjectileTrailStep.End(),
    ];

    /// <summary>The wave trail's four poses, shared by both sides.</summary>
    private static readonly ProjectileTrailStep[] Wave =
    [
        new ProjectileTrailStep.Pose(4, ProjectileTrailFrame.Wave0),
        new ProjectileTrailStep.Pose(4, ProjectileTrailFrame.Wave1),
        new ProjectileTrailStep.Pose(4, ProjectileTrailFrame.Wave2),
        new ProjectileTrailStep.Pose(4, ProjectileTrailFrame.Wave3),
        new ProjectileTrailStep.End(),
    ];

    /// <summary>The missile trail's four poses, also drawn by Super Missiles.</summary>
    private static readonly ProjectileTrailStep[] Missile =
    [
        new ProjectileTrailStep.Pose(4, ProjectileTrailFrame.Missile0),
        new ProjectileTrailStep.Pose(4, ProjectileTrailFrame.Missile1),
        new ProjectileTrailStep.Pose(4, ProjectileTrailFrame.Missile2),
        new ProjectileTrailStep.Pose(4, ProjectileTrailFrame.Missile3),
        new ProjectileTrailStep.End(),
    ];

    /// <summary>Each animation's steps.</summary>
    internal static readonly IReadOnlyDictionary<ProjectileTrailSegment, ProjectileTrailStep[]> Animations =
        new Dictionary<ProjectileTrailSegment, ProjectileTrailStep[]>
    {
        [ProjectileTrailSegment.Empty] = Empty,
        [ProjectileTrailSegment.LeftIce] = LeftIce,
        [ProjectileTrailSegment.RightIce] = RightIce,
        [ProjectileTrailSegment.Wave] = Wave,
        [ProjectileTrailSegment.Missile] = Missile,
    };
}
