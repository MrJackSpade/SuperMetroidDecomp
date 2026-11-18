using SuperMetroid.Core.Animation;
using F = SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The trail sparkles' animations (#2348), played through the shared animation player: each sparkle shows its frames for
/// their durations, a move cue nudges a sparkle a pixel down before the next frame shows, and each animation plays its
/// segment once and ends.
/// </summary>
internal static class ProjectileTrailSparkleAnimations
{
    /// <summary>
    /// Every sparkle animation's beats. The left ice sparkle first falls a pixel after six frames, then after two more, then
    /// after every remaining frame, and its last frame holds for four updates; the right ice sparkle shows the same frames,
    /// falling on the right side.
    /// </summary>
    internal static AnimationSegments<ProjectileTrailSegment, ProjectileTrailFrame, ProjectileTrailCue> Segments { get; } = new(
        new Dictionary<ProjectileTrailSegment, AnimationBeat<ProjectileTrailFrame, ProjectileTrailCue>[]>
        {
            [ProjectileTrailSegment.Empty] = [],
            [ProjectileTrailSegment.LeftIce] =
            [
                Show(F.IceSparkleClustered, 1),
                Show(F.IceSparkleClustered, 1),
                Show(F.IceSparkleClustered, 1),
                Show(F.IceSparkleClustered, 1),
                Show(F.IceSparkleTwinkling, 1),
                Show(F.IceSparkleTwinkling, 1),
                Cue(ProjectileTrailCue.MoveLeftDown),
                Show(F.IceSparkleTwinkling, 1),
                Show(F.IceSparkleTwinkling, 1),
                Cue(ProjectileTrailCue.MoveLeftDown),
                Show(F.IceSparkleScattered, 1),
                Cue(ProjectileTrailCue.MoveLeftDown),
                Show(F.IceSparkleScattered, 1),
                Cue(ProjectileTrailCue.MoveLeftDown),
                Show(F.IceSparkleScattered, 1),
                Cue(ProjectileTrailCue.MoveLeftDown),
                Show(F.IceSparkleScattered, 1),
                Cue(ProjectileTrailCue.MoveLeftDown),
                Show(F.IceSparkleScattered, 1),
                Cue(ProjectileTrailCue.MoveLeftDown),
                Show(F.IceSparkleScattered, 1),
                Cue(ProjectileTrailCue.MoveLeftDown),
                Show(F.IceSparkleScattered, 1),
                Cue(ProjectileTrailCue.MoveLeftDown),
                Show(F.IceSparkleScattered, 1),
                Cue(ProjectileTrailCue.MoveLeftDown),
                Show(F.IceSparkleFading, 4),
            ],
            [ProjectileTrailSegment.RightIce] =
            [
                Show(F.IceSparkleClustered, 1),
                Show(F.IceSparkleClustered, 1),
                Show(F.IceSparkleClustered, 1),
                Show(F.IceSparkleClustered, 1),
                Show(F.IceSparkleTwinkling, 1),
                Show(F.IceSparkleTwinkling, 1),
                Cue(ProjectileTrailCue.MoveRightDown),
                Show(F.IceSparkleTwinkling, 1),
                Show(F.IceSparkleTwinkling, 1),
                Cue(ProjectileTrailCue.MoveRightDown),
                Show(F.IceSparkleScattered, 1),
                Cue(ProjectileTrailCue.MoveRightDown),
                Show(F.IceSparkleScattered, 1),
                Cue(ProjectileTrailCue.MoveRightDown),
                Show(F.IceSparkleScattered, 1),
                Cue(ProjectileTrailCue.MoveRightDown),
                Show(F.IceSparkleScattered, 1),
                Cue(ProjectileTrailCue.MoveRightDown),
                Show(F.IceSparkleScattered, 1),
                Cue(ProjectileTrailCue.MoveRightDown),
                Show(F.IceSparkleScattered, 1),
                Cue(ProjectileTrailCue.MoveRightDown),
                Show(F.IceSparkleScattered, 1),
                Cue(ProjectileTrailCue.MoveRightDown),
                Show(F.IceSparkleScattered, 1),
                Cue(ProjectileTrailCue.MoveRightDown),
                Show(F.IceSparkleFading, 4),
            ],
            [ProjectileTrailSegment.Wave] =
            [
                Show(F.WaveRing, 4),
                Show(F.WaveRingSpreading, 4),
                Show(F.WaveRingWide, 4),
                Show(F.WaveRingBreaking, 4),
            ],
            [ProjectileTrailSegment.Missile] =
            [
                Show(F.MissileSmokePuff, 4),
                Show(F.MissileSmokeBillowing, 4),
                Show(F.MissileSmokeThinning, 4),
                Show(F.MissileSmokeFading, 4),
            ],
        });

    /// <summary>The animation of each sparkle: its segment played once, after which the sparkle is gone.</summary>
    internal static IReadOnlyDictionary<ProjectileTrailSegment, Animation<ProjectileTrailSegment>> StartingAt { get; } =
        Enum.GetValues<ProjectileTrailSegment>().ToDictionary(segment => segment, PlayedOnce);

    /// <summary>A sparkle's playback of <paramref name="segment"/>, before its first frame.</summary>
    internal static AnimationPlayback<ProjectileTrailSegment> Start(ProjectileTrailSegment segment) =>
        new(StartingAt[segment], AnimationPlace.StartOfAnimation);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<ProjectileTrailFrame, ProjectileTrailCue> Show(ProjectileTrailFrame frame, int duration) =>
        new AnimationBeat<ProjectileTrailFrame, ProjectileTrailCue>.Frame(frame, duration);

    /// <summary>Raises <paramref name="cue"/> before the next frame shows.</summary>
    private static AnimationBeat<ProjectileTrailFrame, ProjectileTrailCue> Cue(ProjectileTrailCue cue) =>
        new AnimationBeat<ProjectileTrailFrame, ProjectileTrailCue>.Event(cue);

    /// <summary>The animation that plays all of <paramref name="segment"/> once and ends.</summary>
    private static Animation<ProjectileTrailSegment> PlayedOnce(ProjectileTrailSegment segment) =>
        new([Segments.Whole(segment)], AnimationEnd.Stops);
}
