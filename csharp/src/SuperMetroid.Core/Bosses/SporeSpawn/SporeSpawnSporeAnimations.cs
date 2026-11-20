using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using P = SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnProjectileSegment;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>A spore's animations (#641, #2011, #2554): its three five-update poses, looping, and a shot spore's burst, which takes its impact properties after its first pose, queues its killed sound after its fourth, then requests its drop and deletes itself.</summary>
internal static class SporeSpawnSporeAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<SporeSpawnProjectileSegment, EnemyProjectileFrameId, SporeSpawnSporeCue> Segments { get; } = new(
        new Dictionary<SporeSpawnProjectileSegment, AnimationBeat<EnemyProjectileFrameId, SporeSpawnSporeCue>[]>
    {
        [P.Spore] =
        [
            Show(EnemyProjectileFrameId.SporeSpawnSporeGlowing, 5),
            Show(EnemyProjectileFrameId.SporeSpawnSporeDim, 5),
            Show(EnemyProjectileFrameId.SporeSpawnSporeSparkling, 5),
        ],
        [P.SporeShot] =
        [
            Show(EnemyProjectileFrameId.SporeSpawnSporeBurstSpark, 1),
            Cue(SporeSpawnSporeCue.SetImpactProperties),
            Show(EnemyProjectileFrameId.SporeSpawnSporeBurstSpark, 3),
            Show(EnemyProjectileFrameId.SporeSpawnSporeBurstFireballSmall, 6),
            Show(EnemyProjectileFrameId.SporeSpawnSporeBurstFireballLarge, 5),
            Cue(SporeSpawnSporeCue.QueueKilledSound),
            Show(EnemyProjectileFrameId.SporeSpawnSporeBurstFlamingRing, 5),
            Show(EnemyProjectileFrameId.SporeSpawnSporeBurstStarburst, 5),
            Show(EnemyProjectileFrameId.SporeSpawnSporeBurstEmbers, 6),
            Cue(SporeSpawnSporeCue.RequestDrop),
            Cue(SporeSpawnSporeCue.Delete),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<SporeSpawnProjectileSegment, Animation<SporeSpawnProjectileSegment>> StartingAt { get; } = new Dictionary<SporeSpawnProjectileSegment, Animation<SporeSpawnProjectileSegment>>
    {
        [P.Spore] = Animation<SporeSpawnProjectileSegment>.Looping([Segments.Whole(P.Spore)]),
        [P.SporeShot] = new([Segments.Whole(P.SporeShot)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<SporeSpawnProjectileSegment> StartingWith(SporeSpawnProjectileSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> passes.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, SporeSpawnSporeCue> Show(EnemyProjectileFrameId frame, int duration) =>
        new AnimationBeat<EnemyProjectileFrameId, SporeSpawnSporeCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, SporeSpawnSporeCue> Cue(SporeSpawnSporeCue cue) =>
        new AnimationBeat<EnemyProjectileFrameId, SporeSpawnSporeCue>.Event(cue);
}
