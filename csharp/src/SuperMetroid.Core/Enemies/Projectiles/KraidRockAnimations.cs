using SuperMetroid.Core.Animation;
using Beat = SuperMetroid.Core.Animation.AnimationBeat<SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileFrameId, SuperMetroid.Core.Enemies.Projectiles.KraidRockCue>;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Kraid's typed rock animations (#641, #1600, #1817).</summary>
internal static class KraidRockAnimations
{
    /// <summary>
    /// Each rock segment's beats: the shared and right rising rocks hold their still pose for good (#1627: the game
    /// shows it for the longest duration a frame can have and then rests on it); the spat rock's burst stops its flight, shows five four-update frames of the rock breaking apart, then deletes itself.
    /// </summary>
    internal static AnimationSegments<KraidRockSegment, EnemyProjectileFrameId, KraidRockCue> Segments { get; } = new(
        new Dictionary<KraidRockSegment, Beat[]>
        {
            [KraidRockSegment.SharedPose] = [new Beat.HeldFrame(EnemyProjectileFrameId.KraidRockLarge)],
            [KraidRockSegment.RisingRightPose] = [new Beat.HeldFrame(EnemyProjectileFrameId.KraidRockSmall)],
            [KraidRockSegment.SpitRockShot] =
            [
                new Beat.Event(KraidRockCue.StopFlyingFromNextPass),
                new Beat.Frame(EnemyProjectileFrameId.KraidRockSpark, 4),
                new Beat.Frame(EnemyProjectileFrameId.KraidRockFlareBall, 4),
                new Beat.Frame(EnemyProjectileFrameId.KraidRockFlareRingCored, 4),
                new Beat.Frame(EnemyProjectileFrameId.KraidRockFlareRing, 4),
                new Beat.Frame(EnemyProjectileFrameId.KraidRockFlareRingFading, 4),
                new Beat.Event(KraidRockCue.Delete),
            ],
        });

    /// <summary>
    /// The animation that starts at each segment: each plays its segment once and holds its last frame; the burst plays
    /// its cue, five frames and its deletion.
    /// </summary>
    internal static IReadOnlyDictionary<KraidRockSegment, Animation<KraidRockSegment>> StartingAt { get; } = new Dictionary<KraidRockSegment, Animation<KraidRockSegment>>
    {
        [KraidRockSegment.SharedPose] = new([Segments.Whole(KraidRockSegment.SharedPose)], AnimationEnd.Holds),
        [KraidRockSegment.RisingRightPose] = new([Segments.Whole(KraidRockSegment.RisingRightPose)], AnimationEnd.Holds),
        [KraidRockSegment.SpitRockShot] = new([Segments.Whole(KraidRockSegment.SpitRockShot)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<KraidRockSegment> StartingWith(KraidRockSegment segment) => StartingAt[segment];
}
