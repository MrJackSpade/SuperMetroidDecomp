using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.EffectActors;
using K = SuperMetroid.Core.Enemies.Deaths.EnemyDeathExplosionCueKind;
using X = SuperMetroid.Core.Enemies.Deaths.EnemyDeathExplosionSegment;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Deaths;

/// <summary>
/// The generic enemy-death explosions (#641, #2554). Each frame shows a <see cref="EnemyProjectileFrameId"/> and each cue is
/// a <see cref="EnemyDeathExplosionCue"/> with the values it acts on; each explosion ends by becoming its drop.
/// </summary>
internal static class EnemyDeathExplosionAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<EnemyDeathExplosionSegment, EnemyProjectileFrameId, EnemyDeathExplosionCue> Segments { get; } = new(
        new Dictionary<EnemyDeathExplosionSegment, AnimationBeat<EnemyProjectileFrameId, EnemyDeathExplosionCue>[]>
    {
        [X.BigExplosion] =
        [
            Cue(new EnemyDeathExplosionCue.SpawnEffectActor(RoomEffectActorKind.SmallExplosion, EnemyDeathExplosionDefinitions.NarrowSpriteSpread)),
            Cue(new EnemyDeathExplosionCue.SpawnEffectActor(RoomEffectActorKind.Smoke, EnemyDeathExplosionDefinitions.NarrowSpriteSpread)),
            Show(EnemyProjectileFrameId.Blank, 8),
            Cue(new EnemyDeathExplosionCue.Signal(K.PlaySmallExplosionSound)),
        ],
        [X.BigExplosionEnd] =
        [
            Cue(new EnemyDeathExplosionCue.Signal(K.BecomePickup)),
        ],
        [X.FakeKraidExplosion] =
        [
            Cue(new EnemyDeathExplosionCue.SpawnEffectActor(RoomEffectActorKind.SmallExplosion, EnemyDeathExplosionDefinitions.WideSpriteSpread)),
            Cue(new EnemyDeathExplosionCue.SpawnEffectActor(RoomEffectActorKind.Smoke, EnemyDeathExplosionDefinitions.WideSpriteSpread)),
            Cue(new EnemyDeathExplosionCue.SpawnEffectActor(RoomEffectActorKind.BigDustCloud, EnemyDeathExplosionDefinitions.WideSpriteSpread)),
            Show(EnemyProjectileFrameId.Blank, 8),
            Cue(new EnemyDeathExplosionCue.Signal(K.PlaySmallExplosionSound)),
        ],
        [X.FakeKraidExplosionEnd] =
        [
            Cue(new EnemyDeathExplosionCue.Signal(K.BecomePickup)),
        ],
        [X.NormalExplosion] =
        [
            Show(EnemyProjectileFrameId.EnemyDeathFireballSmall, 5),
            Show(EnemyProjectileFrameId.EnemyDeathFireballLarge, 5),
            Show(EnemyProjectileFrameId.EnemyDeathFlamingRing, 5),
            Cue(new EnemyDeathExplosionCue.Signal(K.PlayEnemyKilledSound)),
            Show(EnemyProjectileFrameId.EnemyDeathFireballHuge, 5),
            Show(EnemyProjectileFrameId.EnemyDeathWideRing, 5),
            Show(EnemyProjectileFrameId.EnemyDeathWideRingFading, 5),
            Cue(new EnemyDeathExplosionCue.Signal(K.BecomePickup)),
        ],
        [X.SmallExplosion] =
        [
            Show(EnemyProjectileFrameId.EnemyDeathSpark, 4),
            Show(EnemyProjectileFrameId.EnemyDeathFireballSmall, 6),
            Show(EnemyProjectileFrameId.EnemyDeathFireballLarge, 5),
            Cue(new EnemyDeathExplosionCue.Signal(K.PlayEnemyKilledSound)),
            Show(EnemyProjectileFrameId.EnemyDeathFlamingRing, 5),
            Show(EnemyProjectileFrameId.EnemyDeathStarburst, 5),
            Show(EnemyProjectileFrameId.EnemyDeathEmbers, 6),
            Cue(new EnemyDeathExplosionCue.Signal(K.BecomePickup)),
        ],
        [X.KilledBySamusContact] =
        [
            Show(EnemyProjectileFrameId.EnemyDeathFireballLarge, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFireballCluster, 2),
            Show(EnemyProjectileFrameId.EnemyDeathStarburstCluster, 2),
            Show(EnemyProjectileFrameId.EnemyDeathEmbersCluster, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourFireballsAmidEmbers, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourFireballsAmidAsh, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourEmbersFlyingApart, 2),
            Cue(new EnemyDeathExplosionCue.Signal(K.PlayContactKilledSound)),
            Show(EnemyProjectileFrameId.EnemyDeathFourEmbersSpreading, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourEmbersCooling, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourEmbersDimming, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourAshDrifting, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourAshSpreading, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourAshSinking, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourAshFalling, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourAshSettling, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourAshDispersing, 2),
            Cue(new EnemyDeathExplosionCue.Signal(K.BecomePickup)),
        ],
    });

    /// <summary>How many bursts the big explosion makes before it becomes the drop.</summary>
    private const int BigExplosionBursts = 5;

    /// <summary>How many bursts Fake Kraid's explosion makes before it becomes the drop.</summary>
    private const int FakeKraidExplosionBursts = 16;

    /// <summary>
    /// Each explosion's animation, by the segment it starts at (#2061): the big explosion's five bursts and Fake Kraid's
    /// sixteen, each scattering its sprites, before it becomes the drop; the other explosions play their segment once.
    /// </summary>
    internal static IReadOnlyDictionary<EnemyDeathExplosionSegment, Animation<EnemyDeathExplosionSegment>> StartingAt { get; } =
        new Dictionary<EnemyDeathExplosionSegment, Animation<EnemyDeathExplosionSegment>>
        {
            [X.SmallExplosion] = new([Segments.Whole(X.SmallExplosion)], AnimationEnd.Holds),
            [X.KilledBySamusContact] = new([Segments.Whole(X.KilledBySamusContact)], AnimationEnd.Holds),
            [X.NormalExplosion] = new([Segments.Whole(X.NormalExplosion)], AnimationEnd.Holds),
            [X.FakeKraidExplosion] = new([Segments.Repeated(X.FakeKraidExplosion, FakeKraidExplosionBursts), Segments.Whole(X.FakeKraidExplosionEnd)], AnimationEnd.Holds),
            [X.BigExplosion] = new([Segments.Repeated(X.BigExplosion, BigExplosionBursts), Segments.Whole(X.BigExplosionEnd)], AnimationEnd.Holds),
        };

    /// <summary>The animation each death explosion starts.</summary>
    internal static IReadOnlyDictionary<EnemyDeathExplosion, Animation<EnemyDeathExplosionSegment>> Explosions { get; } =
        new Dictionary<EnemyDeathExplosion, Animation<EnemyDeathExplosionSegment>>
        {
            [EnemyDeathExplosion.Small] = StartingAt[X.SmallExplosion],
            [EnemyDeathExplosion.SamusContact] = StartingAt[X.KilledBySamusContact],
            [EnemyDeathExplosion.Normal] = StartingAt[X.NormalExplosion],
            [EnemyDeathExplosion.FakeKraid] = StartingAt[X.FakeKraidExplosion],
            [EnemyDeathExplosion.Big] = StartingAt[X.BigExplosion],
        };

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyDeathExplosionCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyDeathExplosionCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyDeathExplosionCue> Cue(EnemyDeathExplosionCue cue) => new AnimationBeat<EnemyProjectileFrameId, EnemyDeathExplosionCue>.Event(cue);
}
