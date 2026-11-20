using SuperMetroid.Core.Areas.Tourian;
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
using SuperMetroid.Core.Spatial;

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
        [0xb6b9] = SingleMotion.Moving,
        [0xb714] = SingleMotion.Moving,
        [0xa8ef] = TorizoProjectileMotion.StatueFragmentFalling,
        [0xefe0] = SingleMotion.Moving,
        [0xbfdf] = MotherBrainKeptMotion.TurretAiming,
        [0xc0e0] = SingleMotion.Moving,
        [0xce9b] = MotherBrainProjectileMotion.GlassShardFlying,
        [0xcbe7] = MotherBrainKeptMotion.TubeFalling,
        [0xc84d] = MotherBrainKeptMotion.DroolHanging,
        [0xc886] = MotherBrainProjectileMotion.DroolFalling,
        [0xc335] = MotherBrainKeptMotion.OnionRingFlying,
        [0xc4c8] = MotherBrainKeptMotion.BombBouncing,
        [0xc814] = MotherBrainKeptMotion.RainbowBeamCharging,
        [0xc94c] = MotherBrainKeptMotion.RainbowImpactTracking,
        [0xc914] = MotherBrainKeptMotion.DeathExplosionTracking,
        [0xc9d2] = MotherBrainKeptMotion.DoorFragmentFalling,
        [0xcafa] = MotherBrainProjectileMotion.SubtitleHeld,
        [0x8dca] = SingleMotion.Moving,
        [0x8dff] = DraygonShotMotion.TurretShotFlying,
        [0x8e0f] = DraygonShotMotion.GoopFlying,
        [0xdcee] = SingleMotion.Moving,
        [0xdd46] = SporeSpawnSpawnerMotion.Releasing,
        [0xea80] = SingleMotion.Moving,
        [0xec05] = SingleMotion.Moving,
        [0xacad] = TorizoChozoOrbMotion.BombOrbFalling,
        [0xacfa] = TorizoChozoOrbMotion.GoldenOrbBouncing,
        [0xae6c] = TorizoProjectileMotion.SonicBoomFlying,
        [0xa887] = TorizoProjectileMotion.DroolFalling,
        [0xb043] = GoldenTorizoEggMotion.EggBouncing,
        [0xb0b9] = GoldenTorizoEggMotion.EggCharging,
        [0xb0dd] = GoldenTorizoEggMotion.EggDropping,
        [0xb20d] = GoldenTorizoWeaponMotion.SuperMissileHeld,
        [0xb237] = GoldenTorizoWeaponMotion.SuperMissileThrown,
        [0xb38a] = GoldenTorizoWeaponMotion.EyeBeamFlying,
        [0xba37] = TourianStatueDecorationMotion.FollowingReveal,
        [0xba42] = TourianStatueDecorationMotion.Settled,
        [0xe4fe] = SingleMotion.Moving,
        [0xb535] = SingleMotion.Moving,
        [0x940e] = RidleyFireballMotion.FireballFlying,
        [0xd7bf] = NoobTubeCrackMotion.Flickering,
        [0xd7de] = NoobTubeCrackMotion.Falling,
        [0xd7fd] = NoobTubePieceMotion.ShardFlying,
        [0xd83d] = NoobTubePieceMotion.ShardFalling,
        [0xd89f] = NoobTubePieceMotion.BubbleFalling,
        [0xd8df] = NoobTubePieceMotion.BubbleFlying,
        [0x950d] = RidleyFireballMotion.HorizontalAfterburnFlying,
        [0x9522] = RidleyFireballMotion.VerticalAfterburnFlying,
        [0x8b5d] = SingleMotion.Moving,
        [0x906b] = CrocomireProjectileMotion.ShotAiming,
        [0x90b3] = CrocomireProjectileMotion.ShotFlying,
        [0x9115] = SingleMotion.Moving,
        [0x9d56] = KraidRockMotion.Flying,
        [0x9d89] = KraidRockMotion.CeilingRockFalling,
        [0x9da5] = null,
        [0x9b29] = PhantoonFlameMotion.IntroWaiting,
        [0x9b41] = PhantoonFlameMotion.IntroOrbiting,
        [0x9981] = SingleMotion.Moving,
        [0x99bf] = PhantoonLandedFlameMotion.CasualLanding,
        [0x9a01] = PhantoonLandedFlameMotion.CasualBouncing,
        [0x9a45] = PhantoonFlameMotion.RageOrbiting,
        [0x9a94] = SingleMotion.Moving,
        [0x9ada] = PhantoonFlameMotion.SpiralOrbiting,
        [0x92ba] = SingleMotion.Moving,
        [0x9eff] = SingleMotion.Moving,
        [0xcff8] = KiHunterAcidMotion.Falling,
        [0xcfd5] = KiHunterAcidMotion.Launching,
        [0xcfe6] = KiHunterAcidMotion.Launching,
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
        [0x9e1e] = SingleMotion.Moving,
        [0x9e83] = SingleMotion.Moving,
        [0xa05c] = SingleMotion.Moving,
        [0xa07a] = SingleMotion.Moving,
        [0xa0d1] = SingleMotion.Moving,
        [0xa124] = SingleMotion.Moving,
        [0xb977] = TourianStatueEffectMotion.SplashHolding,
        [0xb982] = TourianStatueEffectMotion.ParticleFalling,
        [0xb9fd] = TourianStatueEffectMotion.SoulRising,
    };

    /// <summary>
    /// The routine that flies a Space Pirate's or Mother Brain's laser each way (#2278): both stand for
    /// <see cref="SingleMotion.Moving"/>, and the laser's state keeps the way it flies.
    /// </summary>
    internal static readonly IReadOnlyDictionary<HorizontalDirection, ushort> PirateLaserFlights = new Dictionary<HorizontalDirection, ushort>
    {
        [HorizontalDirection.Left] = 0xa05c,
        [HorizontalDirection.Right] = 0xa07a,
    };

    /// <summary>
    /// The routine that flies a Ninja Space Pirate's claw thrown each way (#2278): both stand for
    /// <see cref="SingleMotion.Moving"/>, and the claw's state keeps the way it was thrown.
    /// </summary>
    internal static readonly IReadOnlyDictionary<HorizontalDirection, ushort> PirateClawThrows = new Dictionary<HorizontalDirection, ushort>
    {
        [HorizontalDirection.Left] = 0xa0d1,
        [HorizontalDirection.Right] = 0xa124,
    };

    /// <summary>
    /// The one routine address of <paramref name="motion"/>; holding still, and the one motion of the kinds that move
    /// one way, stand for several routines and have no single word: a family that keeps its way in its state names its
    /// routines by that way (<see cref="PirateLaserFlights"/>, <see cref="PirateClawThrows"/>).
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
