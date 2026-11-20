using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies.EyeDoor;
using SuperMetroid.Core.Enemies.Kago;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Enemies.NoobTube;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Cartridge values and helpers of <see cref="EnemyProjectileMotionWords"/> that only verification reads.</summary>
internal static class EnemyProjectileMotionWordsConstants3
{
    /// <summary>The motion each routine address stands for, resolved through the projectile's behaviour, or holding still.</summary>
    internal static readonly Dictionary<ushort, EnemyProjectileMotionWord> Motions = new()
    {
        [0x0000] = Still,
        [0xa3a3] = Still,
        [0x8170] = Still,
        [0xa327] = Still,
        [0x84fb] = Still,
        [0xec94] = Still,
        [0xd0eb] = Still,
        [0xcff7] = Still,
        [0xb4b0] = Still,
        [0x8d54] = Still,
        [0x950c] = Still,
        [0x9a44] = Still,
        [0xbbc6] = Still,
        [0xa05b] = Still,
        [0xefdf] = Still,
        [0xa919] = Still,
        [0xa918] = Still,
        [0xdd44] = Still,
        [0xcaa3] = Still,
        [0xc76d] = Still,
        [0xe6d1] = Still,
        [0xe604] = Still,
        [0xe508] = Still,
        [0xe605] = Of(SingleMotion.Moving),
        [0xb6b9] = Of(EyeDoorProjectileMotion.ShotFlying),
        [0xb714] = Of(EyeDoorProjectileMotion.SweatFalling),
        [0xa8ef] = Of(TorizoProjectileMotion.StatueFragmentFalling),
        [0xefe0] = Of(SingleMotion.Moving),
        [0xbfdf] = Of(MotherBrainProjectileMotion.TurretAiming),
        [0xc0e0] = Of(SingleMotion.Moving),
        [0xce9b] = Of(MotherBrainProjectileMotion.GlassShardFlying),
        [0xcbe7] = Of(MotherBrainProjectileMotion.TubeFalling),
        [0xc84d] = Of(MotherBrainProjectileMotion.DroolHanging),
        [0xc886] = Of(MotherBrainProjectileMotion.DroolFalling),
        [0xc335] = Of(MotherBrainProjectileMotion.OnionRingFlying),
        [0xc4c8] = Of(MotherBrainProjectileMotion.BombBouncing),
        [0xc814] = Of(MotherBrainProjectileMotion.RainbowBeamCharging),
        [0xc94c] = Of(MotherBrainProjectileMotion.RainbowImpactTracking),
        [0xc914] = Of(MotherBrainProjectileMotion.DeathExplosionTracking),
        [0xc9d2] = Of(MotherBrainProjectileMotion.DoorFragmentFalling),
        [0xcafa] = Of(MotherBrainProjectileMotion.SubtitleHeld),
        [0x8dca] = Owned<DraygonShots.DraygonShotBehavior>(owner => owner.GoopStuckToSamus),
        [0x8dff] = Owned<DraygonShots.DraygonShotBehavior>(owner => owner.TurretShotFlying),
        [0x8e0f] = Owned<DraygonShots.DraygonShotBehavior>(owner => owner.GoopFlying),
        [0xdcee] = Of(SingleMotion.Moving),
        [0xdd46] = Of(SporeSpawnSpawnerMotion.Releasing),
        [0xea80] = Of(BotwoonProjectileMotion.BodySegment),
        [0xec05] = Of(BotwoonProjectileMotion.SpitFlying),
        [0xacad] = Of(TorizoProjectileMotion.BombOrbFalling),
        [0xacfa] = Of(TorizoProjectileMotion.GoldenOrbBouncing),
        [0xae6c] = Of(TorizoProjectileMotion.SonicBoomFlying),
        [0xa887] = Of(TorizoProjectileMotion.DroolFalling),
        [0xb043] = Of(TorizoProjectileMotion.EggBouncing),
        [0xb0b9] = Of(TorizoProjectileMotion.EggCharging),
        [0xb0dd] = Of(TorizoProjectileMotion.EggDropping),
        [0xb20d] = Of(TorizoProjectileMotion.SuperMissileHeld),
        [0xb237] = Of(TorizoProjectileMotion.SuperMissileThrown),
        [0xb38a] = Of(TorizoProjectileMotion.EyeBeamFlying),
        [EnemyProjectileMotionWords.TourianStatueRevealFollowing] = Of(TourianStatueDecorationMotion.FollowingReveal),
        [0xba42] = Of(TourianStatueDecorationMotion.Settled),
        [0xe4fe] = Of(SingleMotion.Moving),
        [0xb535] = Of(SingleMotion.Moving),
        [0x940e] = Of(RidleyFireballMotion.FireballFlying),
        [0xd7bf] = Of(NoobTubePieceMotion.CrackFlickering),
        [0xd7de] = Of(NoobTubePieceMotion.CrackFalling),
        [0xd7fd] = Of(NoobTubePieceMotion.ShardFlying),
        [0xd83d] = Of(NoobTubePieceMotion.ShardFalling),
        [0xd89f] = Of(NoobTubePieceMotion.BubbleFalling),
        [0xd8df] = Of(NoobTubePieceMotion.BubbleFlying),
        [0x950d] = Of(RidleyFireballMotion.HorizontalAfterburnFlying),
        [0x9522] = Of(RidleyFireballMotion.VerticalAfterburnFlying),
        [0x8b5d] = Of(SingleMotion.Moving),
        [0x906b] = Of(CrocomireProjectileMotion.ShotAiming),
        [0x90b3] = Of(CrocomireProjectileMotion.ShotFlying),
        [0x9115] = Of(CrocomireProjectileMotion.SpikeWallCollapsing),
        [0x9d56] = Of(KraidRockMotion.Flying),
        [0x9d89] = Of(KraidRockMotion.CeilingRockFalling),
        [0x9da5] = Still,
        [0x9b29] = Owned<PhantoonCombat.PhantoonFlameBehavior>(owner => owner.IntroWaiting),
        [0x9b41] = Owned<PhantoonCombat.PhantoonFlameBehavior>(owner => owner.IntroOrbiting),
        [0x9981] = Owned<PhantoonCombat.PhantoonFlameBehavior>(owner => owner.CasualFalling),
        [0x99bf] = Owned<PhantoonCombat.PhantoonFlameBehavior>(owner => owner.CasualLanding),
        [0x9a01] = Owned<PhantoonCombat.PhantoonFlameBehavior>(owner => owner.CasualBouncing),
        [0x9a45] = Owned<PhantoonCombat.PhantoonFlameBehavior>(owner => owner.RageOrbiting),
        [0x9a94] = Owned<PhantoonCombat.PhantoonFlameBehavior>(owner => owner.RainFalling),
        [0x9ada] = Owned<PhantoonCombat.PhantoonFlameBehavior>(owner => owner.SpiralOrbiting),
        [0x92ba] = Of(CrocomireProjectileMotion.BridgeFragmentFalling),
        [0x9eff] = Of(SingleMotion.Moving),
        [0xcff8] = Of(KiHunterAcidMotion.Falling),
        [0xcfd5] = Of(KiHunterAcidMotion.Launching),
        [0xcfe6] = Of(KiHunterAcidMotion.Launching),
        [0xd263] = Of(SingleMotion.Moving),
        [0xd3bf] = Of(SingleMotion.Moving),
        [0xd9db] = Of(SingleMotion.Moving),
        [0xbc0f] = Of(SingleMotion.Moving),
        [0xdf39] = Of(SingleMotion.Moving),
        [0xe049] = Of(SingleMotion.Moving),
        [0xd0ca] = Of(KagoBugMotion.Waiting),
        [0xd0ec] = Of(KagoBugMotion.Jumping),
        [0xd128] = Of(KagoBugMotion.Falling),
        [0xf3f0] = Of(SingleMotion.Moving),
        [0x9701] = Of(SingleMotion.Moving),
        [0x9e1e] = Owned<FakeKraidBehavior.SpitAndSpikeBehavior>(owner => owner.SpitFlying),
        [0x9e83] = Owned<FakeKraidBehavior.SpitAndSpikeBehavior>(owner => owner.SpikeFlying),
        [0xa05c] = Of(SingleMotion.Moving),
        [0xa07a] = Of(SingleMotion.Moving),
        [0xa0d1] = Of(SingleMotion.Moving),
        [0xa124] = Of(SingleMotion.Moving),
        [0xb977] = Of(TourianStatueEffectMotion.SplashHolding),
        [0xb982] = Of(TourianStatueEffectMotion.ParticleFalling),
        [0xb9fd] = Of(TourianStatueEffectMotion.SoulRising),
    };
    /// <summary>A routine that stands for holding still.</summary>
    internal static EnemyProjectileMotionWord Still => EnemyProjectileStillWord.Instance;
    /// <summary>A routine that stands for <paramref name="motion"/> of the family motion behaviour bound to the projectile.</summary>
    /// <typeparam name="TMotion">The family's motions.</typeparam>
    /// <param name="motion">The motion.</param>
    internal static EnemyProjectileFamilyMotionWord<TMotion> Of<TMotion>(TMotion motion) where TMotion : struct, Enum => new(motion);
    /// <summary>A routine that stands for the motion <paramref name="motion"/> picks of the behaviour bound to the projectile.</summary>
    /// <typeparam name="TBehavior">The behaviour that owns the motion.</typeparam>
    /// <param name="motion">Picks the behaviour's motion.</param>
    internal static EnemyProjectileOwnedMotionWord<TBehavior> Owned<TBehavior>(Func<TBehavior, EnemyProjectileOwnedMotion> motion)
        where TBehavior : EnemyProjectileBehavior => new(motion);
}
