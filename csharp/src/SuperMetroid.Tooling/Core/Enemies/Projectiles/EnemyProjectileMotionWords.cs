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
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.Core.Enemies.SpacePirates;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The bank-$86 routine addresses that stand for each enemy-projectile pre-instruction, for tools and verification that
/// read or emit native words: each stands for the motion of its projectile's behaviour that replaced it (#1843), or for
/// none, holding still. Every empty routine, the pre-Phantoon room projectile's (whose background pin the room's
/// background-scroll rule performs, not the projectile), and the word zero of a freed slot, stand for holding still. A
/// routine of a kind that moves one way stands for <see cref="SingleMotion.Moving"/>.
/// </summary>
internal static class EnemyProjectileMotionWords
{
    /// <summary>The motion each routine address stands for, or null for holding still.</summary>
    internal static readonly Dictionary<ushort, Enum?> Motions = new()
    {
        [0x0000] = null,
        [0xa3a3] = null,
        [0x8170] = null,
        [0xa327] = null,
        [0x84fb] = null,
        [0xec94] = null,
        [0xd0eb] = null,
        [0xcff7] = null,
        [0xb4b0] = null,
        [0x8d54] = null,
        [0x950c] = null,
        [0x9a44] = null,
        [0xbbc6] = null,
        [0xa05b] = null,
        [0xefdf] = null,
        [0xa919] = null,
        [0xa918] = null,
        [0xdd44] = null,
        [0xcaa3] = null,
        [0xc76d] = null,
        [0xe6d1] = null,
        [0xe604] = null,
        [0xe508] = null,
        [0xe605] = SingleMotion.Moving,
        [0xb6b9] = EyeDoorProjectileMotion.ShotFlying,
        [0xb714] = EyeDoorProjectileMotion.SweatFalling,
        [0xa8ef] = TorizoProjectileMotion.StatueFragmentFalling,
        [0xefe0] = SingleMotion.Moving,
        [0xbfdf] = MotherBrainProjectileMotion.TurretAiming,
        [0xc0e0] = SingleMotion.Moving,
        [0xce9b] = MotherBrainProjectileMotion.GlassShardFlying,
        [0xcbe7] = MotherBrainProjectileMotion.TubeFalling,
        [0xc84d] = MotherBrainProjectileMotion.DroolHanging,
        [0xc886] = MotherBrainProjectileMotion.DroolFalling,
        [0xc335] = MotherBrainProjectileMotion.OnionRingFlying,
        [0xc4c8] = MotherBrainProjectileMotion.BombBouncing,
        [0xc814] = MotherBrainProjectileMotion.RainbowBeamCharging,
        [0xc94c] = MotherBrainProjectileMotion.RainbowImpactTracking,
        [0xc914] = MotherBrainProjectileMotion.DeathExplosionTracking,
        [0xc9d2] = MotherBrainProjectileMotion.DoorFragmentFalling,
        [0xcafa] = MotherBrainProjectileMotion.SubtitleHeld,
        [0x8dca] = DraygonShotMotion.GoopStuckToSamus,
        [0x8dff] = DraygonShotMotion.TurretShotFlying,
        [0x8e0f] = DraygonShotMotion.GoopFlying,
        [0xdcee] = SporeSpawnProjectileMotion.SporeDrifting,
        [0xdd46] = SporeSpawnProjectileMotion.SpawnerReleasing,
        [0xea80] = BotwoonProjectileMotion.BodySegment,
        [0xec05] = BotwoonProjectileMotion.SpitFlying,
        [0xacad] = TorizoProjectileMotion.BombOrbFalling,
        [0xacfa] = TorizoProjectileMotion.GoldenOrbBouncing,
        [0xae6c] = TorizoProjectileMotion.SonicBoomFlying,
        [0xa887] = TorizoProjectileMotion.DroolFalling,
        [0xb043] = TorizoProjectileMotion.EggBouncing,
        [0xb0b9] = TorizoProjectileMotion.EggCharging,
        [0xb0dd] = TorizoProjectileMotion.EggDropping,
        [0xb20d] = TorizoProjectileMotion.SuperMissileHeld,
        [0xb237] = TorizoProjectileMotion.SuperMissileThrown,
        [0xb38a] = TorizoProjectileMotion.EyeBeamFlying,
        [0xba37] = TourianStatueDecorationMotion.FollowingReveal,
        [0xba42] = TourianStatueDecorationMotion.Settled,
        [0xbe03] = ShaktoolCircleMotion.Leading,
        [0xbe12] = ShaktoolCircleMotion.Trailing,
        [0xe4fe] = SingleMotion.Moving,
        [0xb535] = SingleMotion.Moving,
        [0x940e] = RidleyFireballMotion.FireballFlying,
        [0xd7bf] = NoobTubePieceMotion.CrackFlickering,
        [0xd7de] = NoobTubePieceMotion.CrackFalling,
        [0xd7fd] = NoobTubePieceMotion.ShardFlying,
        [0xd83d] = NoobTubePieceMotion.ShardFalling,
        [0xd89f] = NoobTubePieceMotion.BubbleFalling,
        [0xd8df] = NoobTubePieceMotion.BubbleFlying,
        [0x950d] = RidleyFireballMotion.HorizontalAfterburnFlying,
        [0x9522] = RidleyFireballMotion.VerticalAfterburnFlying,
        [0x8b5d] = SingleMotion.Moving,
        [0x906b] = CrocomireProjectileMotion.ShotAiming,
        [0x90b3] = CrocomireProjectileMotion.ShotFlying,
        [0x9115] = CrocomireProjectileMotion.SpikeWallCollapsing,
        [0x9d56] = KraidRockMotion.Flying,
        [0x9d89] = KraidRockMotion.CeilingRockFalling,
        [0x9da5] = KraidRockMotion.ShotRockRecolouring,
        [0x9b29] = PhantoonFlameMotion.IntroWaiting,
        [0x9b41] = PhantoonFlameMotion.IntroOrbiting,
        [0x9981] = PhantoonFlameMotion.CasualFalling,
        [0x99bf] = PhantoonFlameMotion.CasualLanding,
        [0x9a01] = PhantoonFlameMotion.CasualBouncing,
        [0x9a45] = PhantoonFlameMotion.RageOrbiting,
        [0x9a94] = PhantoonFlameMotion.RainFalling,
        [0x9ada] = PhantoonFlameMotion.SpiralOrbiting,
        [0x92ba] = CrocomireProjectileMotion.BridgeFragmentFalling,
        [0x9eff] = SingleMotion.Moving,
        [0xcff8] = KiHunterAcidMotion.Falling,
        [0xcfd5] = KiHunterAcidMotion.LaunchingLeft,
        [0xcfe6] = KiHunterAcidMotion.LaunchingRight,
        [0xd263] = SingleMotion.Moving,
        [0xd3bf] = SingleMotion.Moving,
        [0xd9db] = SingleMotion.Moving,
        [0xbc0f] = SingleMotion.Moving,
        [0xdf39] = SingleMotion.Moving,
        [0xe049] = SingleMotion.Moving,
        [0xd0ca] = KagoBugMotion.Waiting,
        [0xd0ec] = KagoBugMotion.Jumping,
        [0xd128] = KagoBugMotion.Falling,
        [0xf3f0] = SingleMotion.Moving,
        [0x9701] = SingleMotion.Moving,
        [0x9e1e] = FakeKraidProjectileMotion.SpitFlying,
        [0x9e83] = FakeKraidProjectileMotion.SpikeFlying,
        [0xa05c] = PirateLaserMotion.FlyingLeft,
        [0xa07a] = PirateLaserMotion.FlyingRight,
        [0xa0d1] = PirateClawMotion.ThrownLeft,
        [0xa124] = PirateClawMotion.ThrownRight,
        [0xb977] = TourianStatueEffectMotion.SplashHolding,
        [0xb982] = TourianStatueEffectMotion.ParticleFalling,
        [0xb9fd] = TourianStatueEffectMotion.SoulRising,
    };

    /// <summary>
    /// The one routine address of <paramref name="motion"/>; holding still, and the one motion of the kinds that move
    /// one way, stand for several routines and have no single word.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="motion"/> has no single routine address.</exception>
    internal static ushort Word(Enum? motion)
    {
        ushort[] words = [.. Motions.Where(entry => Equals(entry.Value, motion)).Select(entry => entry.Key)];
        return words.Length == 1
            ? words[0]
            : throw new ArgumentOutOfRangeException(nameof(motion), motion, $"{motion?.ToString() ?? "Holding still"} stands for {words.Length} routines, not one.");
    }
}
