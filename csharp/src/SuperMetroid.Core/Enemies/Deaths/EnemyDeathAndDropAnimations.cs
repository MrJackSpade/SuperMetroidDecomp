using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Deaths.EnemyDeathAndDropCueKind;
using P = SuperMetroid.Core.Enemies.Deaths.EnemyDeathAndDropSegment;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Deaths;

/// <summary>
/// The drops the generic enemy-death explosions become, and the respawn pause (#641, #2554). Each frame shows a
/// <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="EnemyDeathAndDropCue"/>.
/// </summary>
internal static class EnemyDeathAndDropAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<EnemyDeathAndDropSegment, EnemyProjectileFrameId, EnemyDeathAndDropCue> Segments { get; } = new(
        new Dictionary<EnemyDeathAndDropSegment, AnimationBeat<EnemyProjectileFrameId, EnemyDeathAndDropCue>[]>
    {
        [P.RespawnPause] =
        [
            Show(EnemyProjectileFrameId.Blank, 64),
            Cue(new EnemyDeathAndDropCue.Signal(K.RespawnEnemy)),
            Cue(new EnemyDeathAndDropCue.Signal(K.Delete)),
        ],
        [P.SmallEnergy] =
        [
            Show(EnemyProjectileFrameId.EnemyPickupEnergySmallDark, 8),
            Show(EnemyProjectileFrameId.EnemyPickupEnergySmallRinged, 8),
            Show(EnemyProjectileFrameId.EnemyPickupEnergySmallBright, 8),
            Show(EnemyProjectileFrameId.EnemyPickupEnergyScattered, 8),
        ],
        [P.BigEnergy] =
        [
            Show(EnemyProjectileFrameId.EnemyPickupEnergyBigBright, 8),
            Show(EnemyProjectileFrameId.EnemyPickupEnergyBigRinged, 8),
            Show(EnemyProjectileFrameId.EnemyPickupEnergyBigDark, 8),
            Show(EnemyProjectileFrameId.EnemyPickupEnergyScattered, 8),
        ],
        [P.Missiles] =
        [
            Show(EnemyProjectileFrameId.EnemyPickupMissile, 5),
            Show(EnemyProjectileFrameId.EnemyPickupMissileFlashing, 5),
        ],
        [P.SuperMissiles] =
        [
            Show(EnemyProjectileFrameId.EnemyPickupSuperMissile, 5),
            Show(EnemyProjectileFrameId.EnemyPickupSuperMissileFlashing, 5),
        ],
        [P.PowerBombs] =
        [
            Show(EnemyProjectileFrameId.EnemyPickupPowerBombBright, 5),
            Show(EnemyProjectileFrameId.EnemyPickupPowerBombFading, 5),
            Show(EnemyProjectileFrameId.EnemyPickupPowerBombDim, 5),
            Show(EnemyProjectileFrameId.EnemyPickupPowerBombFlashing, 5),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<EnemyDeathAndDropSegment, Animation<EnemyDeathAndDropSegment>> StartingAt { get; } = new Dictionary<EnemyDeathAndDropSegment, Animation<EnemyDeathAndDropSegment>>
    {
        [P.RespawnPause] = new([Segments.Whole(P.RespawnPause)], AnimationEnd.Holds),
        [P.SmallEnergy] = Animation<EnemyDeathAndDropSegment>.Looping([Segments.Whole(P.SmallEnergy)]),
        [P.BigEnergy] = Animation<EnemyDeathAndDropSegment>.Looping([Segments.Whole(P.BigEnergy)]),
        [P.Missiles] = Animation<EnemyDeathAndDropSegment>.Looping([Segments.Whole(P.Missiles)]),
        [P.SuperMissiles] = Animation<EnemyDeathAndDropSegment>.Looping([Segments.Whole(P.SuperMissiles)]),
        [P.PowerBombs] = Animation<EnemyDeathAndDropSegment>.Looping([Segments.Whole(P.PowerBombs)]),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<EnemyDeathAndDropSegment> StartingWith(EnemyDeathAndDropSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyDeathAndDropCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyDeathAndDropCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyDeathAndDropCue> Cue(EnemyDeathAndDropCue cue) => new AnimationBeat<EnemyProjectileFrameId, EnemyDeathAndDropCue>.Event(cue);
}
