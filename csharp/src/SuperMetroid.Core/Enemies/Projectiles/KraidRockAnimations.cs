using SuperMetroid.Core.Animation;
using Beat = SuperMetroid.Core.Animation.AnimationBeat<SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileFrameId, SuperMetroid.Core.Enemies.Projectiles.KraidRockCue>;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Kraid's typed rock animations (#641, #1600, #1817).</summary>
internal static class KraidRockAnimations
{
    /// <summary>
    /// Each rock segment's beats: the shared and right rising rocks hold their still pose for good (#1627: the game
    /// shows it for the longest duration a frame can have and then rests on it); the spat rock's burst takes the burst's frame
    /// behaviour, shows five four-update frames of the rock breaking apart, then deletes itself.
    /// </summary>
    internal static AnimationSegments<KraidRockSegment, EnemyProjectileFrameId, KraidRockCue> Segments { get; } = new(
        new Dictionary<KraidRockSegment, Beat[]>
        {
            [KraidRockSegment.SharedPose] = [new Beat.HeldFrame(EnemyProjectileFrameId.KraidRock)],
            [KraidRockSegment.RisingRightPose] = [new Beat.HeldFrame(EnemyProjectileFrameId.KraidRockRisingRightPose)],
            [KraidRockSegment.SpitRockShot] =
            [
                new Beat.Event(KraidRockCue.DrawFromSharedSheetFromNextPass),
                new Beat.Frame(EnemyProjectileFrameId.KraidRockBurstFirst, 4),
                new Beat.Frame(EnemyProjectileFrameId.KraidRockBurstSecond, 4),
                new Beat.Frame(EnemyProjectileFrameId.KraidRockBurstThird, 4),
                new Beat.Frame(EnemyProjectileFrameId.KraidRockBurstFourth, 4),
                new Beat.Frame(EnemyProjectileFrameId.KraidRockBurstFifth, 4),
                new Beat.Event(KraidRockCue.Delete),
            ],
        });

    /// <summary>
    /// The animation that starts at each segment: each plays its segment once and holds its last frame; the burst plays
    /// its cue, five frames and its deletion.
    /// </summary>
    internal static IReadOnlyDictionary<KraidRockSegment, Animation<KraidRockSegment>> StartingAt { get; } = new Dictionary<KraidRockSegment, Animation<KraidRockSegment>>
    {
        [KraidRockSegment.SharedPose] = new([new(KraidRockSegment.SharedPose, 0, 1, 1)], AnimationEnd.Holds),
        [KraidRockSegment.RisingRightPose] = new([new(KraidRockSegment.RisingRightPose, 0, 1, 1)], AnimationEnd.Holds),
        [KraidRockSegment.SpitRockShot] = new([new(KraidRockSegment.SpitRockShot, 0, 7, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<KraidRockSegment> StartingWith(KraidRockSegment segment) => StartingAt[segment];
}
