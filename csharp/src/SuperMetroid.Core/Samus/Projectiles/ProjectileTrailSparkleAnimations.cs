using SuperMetroid.Core.Animation;
using F = SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame;
using R = SuperMetroid.Core.Samus.Projectiles.ProjectileTrailRun;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The trail sparkles' animations (#2348, #2458), played through the shared animation player: each sparkle shows its frames
/// for their durations, a move cue nudges a sparkle a pixel down before the next frame shows, and each animation plays its
/// runs once and ends.
/// </summary>
internal static class ProjectileTrailSparkleAnimations
{
    /// <summary>The times an ice sparkle shows its clustered frame before it starts to twinkle.</summary>
    internal const int IceClusteredShows = 4;

    /// <summary>The times an ice sparkle twinkles before each of its first two falls.</summary>
    internal const int IceTwinklingShows = 2;

    /// <summary>The times an ice sparkle falls a pixel and shows its scattered frame.</summary>
    internal const int IceScatteredFalls = 8;

    /// <summary>
    /// Every run's beats. An ice sparkle shows its clustered frame, twinkles, falls a pixel, twinkles again, then falls a
    /// pixel before every scattered frame and once more before its fading frame, which holds for its beat; the left and
    /// right ice sparkles run alike, each falling on its own side.
    /// </summary>
    private static AnimationSegments<R, F, ProjectileTrailCue> Segments { get; } = new(
        new Dictionary<R, AnimationBeat<F, ProjectileTrailCue>[]>
        {
            [R.IceClustered] = [Show(F.IceSparkleClustered, 1)],
            [R.IceTwinkling] = [Show(F.IceSparkleTwinkling, 1)],
            [R.LeftIceFall] = [Cue(ProjectileTrailCue.MoveLeftDown)],
            [R.RightIceFall] = [Cue(ProjectileTrailCue.MoveRightDown)],
            [R.LeftIceScatteredFall] = [Cue(ProjectileTrailCue.MoveLeftDown), Show(F.IceSparkleScattered, 1)],
            [R.RightIceScatteredFall] = [Cue(ProjectileTrailCue.MoveRightDown), Show(F.IceSparkleScattered, 1)],
            [R.IceFading] = [Show(F.IceSparkleFading, 4)],
            [R.Wave] =
            [
                Show(F.WaveRing, 4),
                Show(F.WaveRingSpreading, 4),
                Show(F.WaveRingWide, 4),
                Show(F.WaveRingBreaking, 4),
            ],
            [R.Missile] =
            [
                Show(F.MissileSmokePuff, 4),
                Show(F.MissileSmokeBillowing, 4),
                Show(F.MissileSmokeThinning, 4),
                Show(F.MissileSmokeFading, 4),
            ],
        });

    /// <summary>The animation of each sparkle: its runs played once, after which the sparkle is gone.</summary>
    private static Dictionary<ProjectileTrailSegment, Animation<R>> StartingAt { get; } = new Dictionary<ProjectileTrailSegment, Animation<R>>
    {
        [ProjectileTrailSegment.LeftIce] = IceSparkle(R.LeftIceFall, R.LeftIceScatteredFall),
        [ProjectileTrailSegment.RightIce] = IceSparkle(R.RightIceFall, R.RightIceScatteredFall),
        [ProjectileTrailSegment.Wave] = new([Segments.Whole(R.Wave)], AnimationEnd.Stops),
        [ProjectileTrailSegment.Missile] = new([Segments.Whole(R.Missile)], AnimationEnd.Stops),
    };

    /// <summary>A sparkle's player of <paramref name="sparkle"/>, showing nothing until its first update shows its first frame.</summary>
    internal static AnimationPlayer<R, F, ProjectileTrailCue> Start(ProjectileTrailSegment sparkle) =>
        AnimationPlayer<R, F, ProjectileTrailCue>.Starting(Segments, StartingAt[sparkle]);

    /// <summary>
    /// The beats <paramref name="sparkle"/>'s animation plays from its start to its end, in order, each run laid out as
    /// often as it plays.
    /// </summary>
    internal static IReadOnlyList<AnimationBeat<F, ProjectileTrailCue>> PlayedBeats(ProjectileTrailSegment sparkle) =>
        Segments.PlayedBeats(StartingAt[sparkle]);

    /// <summary>
    /// An ice sparkle's animation, falling with <paramref name="fall"/> and <paramref name="scatteredFall"/>: clustered,
    /// twinkling, a fall, twinkling again, the scattered falls and a last fall before it fades.
    /// </summary>
    private static Animation<R> IceSparkle(R fall, R scatteredFall) => new(
        [
            Segments.Repeated(R.IceClustered, IceClusteredShows),
            Segments.Repeated(R.IceTwinkling, IceTwinklingShows),
            Segments.Whole(fall),
            Segments.Repeated(R.IceTwinkling, IceTwinklingShows),
            Segments.Repeated(scatteredFall, IceScatteredFalls),
            Segments.Whole(fall),
            Segments.Whole(R.IceFading),
        ],
        AnimationEnd.Stops);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<F, ProjectileTrailCue> Show(F frame, int duration) =>
        new AnimationBeat<F, ProjectileTrailCue>.Frame(frame, duration);

    /// <summary>Raises <paramref name="cue"/> before the next frame shows.</summary>
    private static AnimationBeat<F, ProjectileTrailCue> Cue(ProjectileTrailCue cue) =>
        new AnimationBeat<F, ProjectileTrailCue>.Event(cue);
}
