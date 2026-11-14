namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The bank-$86 routine addresses that stand for each enemy-projectile pre-instruction, for tools and verification that
/// read or emit native words; the simulation names pre-instructions by identity only. Every empty routine, the
/// pre-Phantoon room projectile's (whose background pin the room's background-scroll rule performs, not the
/// projectile), and the word zero of a freed slot, stand for <see cref="EnemyProjectileFrameBehavior.Stationary"/>.
/// </summary>
internal static class EnemyProjectileFrameBehaviorWords
{
    /// <summary>The pre-instruction each routine address stands for.</summary>
    internal static readonly Dictionary<ushort, EnemyProjectileFrameBehavior> Identities = new()
    {
        [0x0000] = EnemyProjectileFrameBehavior.Stationary,
        [0xa3a3] = EnemyProjectileFrameBehavior.Stationary,
        [0x8170] = EnemyProjectileFrameBehavior.Stationary,
        [0xa327] = EnemyProjectileFrameBehavior.Stationary,
        [0x84fb] = EnemyProjectileFrameBehavior.Stationary,
        [0xec94] = EnemyProjectileFrameBehavior.Stationary,
        [0xd0eb] = EnemyProjectileFrameBehavior.Stationary,
        [0xcff7] = EnemyProjectileFrameBehavior.Stationary,
        [0xb4b0] = EnemyProjectileFrameBehavior.Stationary,
        [0x8d54] = EnemyProjectileFrameBehavior.Stationary,
        [0x950c] = EnemyProjectileFrameBehavior.Stationary,
        [0x9a44] = EnemyProjectileFrameBehavior.Stationary,
        [0xbbc6] = EnemyProjectileFrameBehavior.Stationary,
        [0xa05b] = EnemyProjectileFrameBehavior.Stationary,
        [0xefdf] = EnemyProjectileFrameBehavior.Stationary,
        [0xa919] = EnemyProjectileFrameBehavior.Stationary,
        [0xa918] = EnemyProjectileFrameBehavior.Stationary,
        [0xdd44] = EnemyProjectileFrameBehavior.Stationary,
        [0xcaa3] = EnemyProjectileFrameBehavior.Stationary,
        [0xc76d] = EnemyProjectileFrameBehavior.Stationary,
        [0xe6d1] = EnemyProjectileFrameBehavior.Stationary,
        [0xe604] = EnemyProjectileFrameBehavior.Stationary,
        [0xe508] = EnemyProjectileFrameBehavior.Stationary,
        [0xe605] = EnemyProjectileFrameBehavior.DownwardGateDescent,
        [0xb6b9] = EnemyProjectileFrameBehavior.EyeDoorShotFlight,
        [0xb714] = EnemyProjectileFrameBehavior.EyeDoorSweatFall,
        [0xa8ef] = EnemyProjectileFrameBehavior.BombTorizoStatueFragmentFall,
        [0xefe0] = EnemyProjectileFrameBehavior.PickupLifetime,
        [0xbfdf] = EnemyProjectileFrameBehavior.MotherBrainTurretAiming,
        [0xc0e0] = EnemyProjectileFrameBehavior.MotherBrainTurretBulletFlight,
        [0xce9b] = EnemyProjectileFrameBehavior.MotherBrainGlassShardFlight,
        [0xcbe7] = EnemyProjectileFrameBehavior.MotherBrainTubeFall,
        [0xc84d] = EnemyProjectileFrameBehavior.MotherBrainDroolHanging,
        [0xc886] = EnemyProjectileFrameBehavior.MotherBrainDroolFall,
        [0xc335] = EnemyProjectileFrameBehavior.MotherBrainOnionRingFlight,
        [0xc4c8] = EnemyProjectileFrameBehavior.MotherBrainBombBounce,
        [0xc814] = EnemyProjectileFrameBehavior.MotherBrainRainbowBeamCharge,
        [0xc94c] = EnemyProjectileFrameBehavior.MotherBrainRainbowImpactTracking,
        [0xc914] = EnemyProjectileFrameBehavior.MotherBrainDeathExplosionTracking,
        [0xc9d2] = EnemyProjectileFrameBehavior.MotherBrainDeathDoorFragmentFall,
        [0xcafa] = EnemyProjectileFrameBehavior.MotherBrainDeathSubtitleFixedPlace,
        [0x8dca] = EnemyProjectileFrameBehavior.DraygonGoopStuckToSamus,
        [0x8dff] = EnemyProjectileFrameBehavior.DraygonTurretShotFlight,
        [0x8e0f] = EnemyProjectileFrameBehavior.DraygonGoopFlight,
        [0xdcee] = EnemyProjectileFrameBehavior.SporeDrift,
        [0xdd46] = EnemyProjectileFrameBehavior.SporeSpawnerCadence,
        [0xea80] = EnemyProjectileFrameBehavior.BotwoonBodySegment,
        [0xec05] = EnemyProjectileFrameBehavior.BotwoonSpitFlight,
        [0xacad] = EnemyProjectileFrameBehavior.BombTorizoOrbFall,
        [0xacfa] = EnemyProjectileFrameBehavior.GoldenTorizoOrbBounce,
        [0xae6c] = EnemyProjectileFrameBehavior.TorizoSonicBoomFlight,
        [0xa887] = EnemyProjectileFrameBehavior.BombTorizoDroolFall,
        [0xb043] = EnemyProjectileFrameBehavior.GoldenTorizoEggBounce,
        [0xb0b9] = EnemyProjectileFrameBehavior.GoldenTorizoEggCharge,
        [0xb0dd] = EnemyProjectileFrameBehavior.GoldenTorizoEggDrop,
        [0xb20d] = EnemyProjectileFrameBehavior.GoldenTorizoSuperMissileHeld,
        [0xb237] = EnemyProjectileFrameBehavior.GoldenTorizoSuperMissileThrown,
        [0xb38a] = EnemyProjectileFrameBehavior.GoldenTorizoEyeBeamFlight,
        [0xba37] = EnemyProjectileFrameBehavior.TourianStatueRevealFollow,
        [0xba42] = EnemyProjectileFrameBehavior.TourianStatueDecorationSettled,
        [0xbe03] = EnemyProjectileFrameBehavior.ShaktoolFrontCircleFlight,
        [0xbe12] = EnemyProjectileFrameBehavior.ShaktoolTrailingCircleFlight,
        [0xe4fe] = EnemyProjectileFrameBehavior.DustCameraCull,
        [0xb535] = EnemyProjectileFrameBehavior.DragonFireballArc,
        [0x940e] = EnemyProjectileFrameBehavior.RidleyFireballFlight,
        [0xd7bf] = EnemyProjectileFrameBehavior.NoobTubeCrackFlickering,
        [0xd7de] = EnemyProjectileFrameBehavior.NoobTubeCrackFalling,
        [0xd7fd] = EnemyProjectileFrameBehavior.NoobTubeShardFlying,
        [0xd83d] = EnemyProjectileFrameBehavior.NoobTubeShardFalling,
        [0xd89f] = EnemyProjectileFrameBehavior.NoobTubeBubbleFalling,
        [0xd8df] = EnemyProjectileFrameBehavior.NoobTubeBubbleFlying,
        [0x950d] = EnemyProjectileFrameBehavior.HorizontalAfterburnFlight,
        [0x9522] = EnemyProjectileFrameBehavior.VerticalAfterburnFlight,
        [0x8b5d] = EnemyProjectileFrameBehavior.SkreeParticleFlight,
        [0x906b] = EnemyProjectileFrameBehavior.CrocomireShotAiming,
        [0x90b3] = EnemyProjectileFrameBehavior.CrocomireShotFlight,
        [0x9115] = EnemyProjectileFrameBehavior.CrocomireSpikeWallCollapse,
        [0x9d56] = EnemyProjectileFrameBehavior.KraidRockFlight,
        [0x9d89] = EnemyProjectileFrameBehavior.KraidCeilingRockFall,
        [0x9da5] = EnemyProjectileFrameBehavior.KraidShotRockRecolor,
        [0x9b29] = EnemyProjectileFrameBehavior.PhantoonIntroFlameWaiting,
        [0x9b41] = EnemyProjectileFrameBehavior.PhantoonIntroFlameOrbit,
        [0x9981] = EnemyProjectileFrameBehavior.PhantoonCasualFlameFall,
        [0x99bf] = EnemyProjectileFrameBehavior.PhantoonCasualFlameLanding,
        [0x9a01] = EnemyProjectileFrameBehavior.PhantoonCasualFlameBounce,
        [0x9a45] = EnemyProjectileFrameBehavior.PhantoonRageFlameOrbit,
        [0x9a94] = EnemyProjectileFrameBehavior.PhantoonFlameRainFall,
        [0x9ada] = EnemyProjectileFrameBehavior.PhantoonSpiralFlameOrbit,
        [0x92ba] = EnemyProjectileFrameBehavior.CrocomireBridgeFragmentFall,
        [0x9eff] = EnemyProjectileFrameBehavior.AlcoonFireballFlight,
        [0xcff8] = EnemyProjectileFrameBehavior.KiHunterAcidFlight,
        [0xcfd5] = EnemyProjectileFrameBehavior.KiHunterAcidLaunchLeft,
        [0xcfe6] = EnemyProjectileFrameBehavior.KiHunterAcidLaunchRight,
        [0xd263] = EnemyProjectileFrameBehavior.PowampSpikeFlight,
        [0xd3bf] = EnemyProjectileFrameBehavior.WorkRobotLaserFlight,
        [0xd9db] = EnemyProjectileFrameBehavior.CacatacSpikeFlight,
        [0xbc0f] = EnemyProjectileFrameBehavior.PolypRockFlight,
        [0xdf39] = EnemyProjectileFrameBehavior.FuneNamiheFireballFlight,
        [0xe049] = EnemyProjectileFrameBehavior.MagdolliteLavaFlight,
        [0xd0ca] = EnemyProjectileFrameBehavior.KagoBugWaiting,
        [0xd0ec] = EnemyProjectileFrameBehavior.KagoBugJump,
        [0xd128] = EnemyProjectileFrameBehavior.KagoBugFall,
        [0xf3f0] = EnemyProjectileFrameBehavior.FallingSparkBounce,
        [0x9701] = EnemyProjectileFrameBehavior.CeresTileFall,
        [0x9e1e] = EnemyProjectileFrameBehavior.FakeKraidSpitFlight,
        [0x9e83] = EnemyProjectileFrameBehavior.FakeKraidSpikeFlight,
        [0xa05c] = EnemyProjectileFrameBehavior.LaserFlightLeft,
        [0xa07a] = EnemyProjectileFrameBehavior.LaserFlightRight,
        [0xa0d1] = EnemyProjectileFrameBehavior.PirateClawThrownLeft,
        [0xa124] = EnemyProjectileFrameBehavior.PirateClawThrownRight,
        [0xb977] = EnemyProjectileFrameBehavior.TourianStatueSplashMotion,
        [0xb982] = EnemyProjectileFrameBehavior.TourianStatueParticleMotion,
        [0xb9fd] = EnemyProjectileFrameBehavior.TourianStatueSoulMotion,
    };

    /// <summary>
    /// The one routine address of <paramref name="frameBehavior"/>; a pre-instruction several empty routines stand for has
    /// no single word.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frameBehavior"/> has no single routine address.</exception>
    internal static ushort Word(EnemyProjectileFrameBehavior frameBehavior)
    {
        ushort[] words = [.. Identities.Where(entry => entry.Value == frameBehavior).Select(entry => entry.Key)];
        return words.Length == 1
            ? words[0]
            : throw new ArgumentOutOfRangeException(nameof(frameBehavior), frameBehavior, $"{frameBehavior} stands for {words.Length} routines, not one.");
    }
}
