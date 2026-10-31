namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The bank-$86 routine addresses that stand for each enemy-projectile pre-instruction, for tools and verification that
/// read or emit native words; the simulation names pre-instructions by identity only. Every empty routine, and the
/// word zero of a freed slot, stands for <see cref="EnemyProjectilePreInstruction.Stationary"/>.
/// </summary>
internal static class EnemyProjectilePreInstructionWords
{
    /// <summary>The pre-instruction each routine address stands for.</summary>
    internal static readonly Dictionary<ushort, EnemyProjectilePreInstruction> Identities = new()
    {
        [0x0000] = EnemyProjectilePreInstruction.Stationary,
        [0xa3a3] = EnemyProjectilePreInstruction.PrePhantoonBackgroundPin,
        [0x8170] = EnemyProjectilePreInstruction.Stationary,
        [0xa327] = EnemyProjectilePreInstruction.Stationary,
        [0x84fb] = EnemyProjectilePreInstruction.Stationary,
        [0xec94] = EnemyProjectilePreInstruction.Stationary,
        [0xd0eb] = EnemyProjectilePreInstruction.Stationary,
        [0xcff7] = EnemyProjectilePreInstruction.Stationary,
        [0xb4b0] = EnemyProjectilePreInstruction.Stationary,
        [0x8d54] = EnemyProjectilePreInstruction.Stationary,
        [0x950c] = EnemyProjectilePreInstruction.Stationary,
        [0x9a44] = EnemyProjectilePreInstruction.Stationary,
        [0xbbc6] = EnemyProjectilePreInstruction.Stationary,
        [0xa05b] = EnemyProjectilePreInstruction.Stationary,
        [0xefdf] = EnemyProjectilePreInstruction.Stationary,
        [0xa919] = EnemyProjectilePreInstruction.Stationary,
        [0xa918] = EnemyProjectilePreInstruction.Stationary,
        [0xdd44] = EnemyProjectilePreInstruction.Stationary,
        [0xcaa3] = EnemyProjectilePreInstruction.Stationary,
        [0xc76d] = EnemyProjectilePreInstruction.Stationary,
        [0xe6d1] = EnemyProjectilePreInstruction.Stationary,
        [0xe604] = EnemyProjectilePreInstruction.Stationary,
        [0xe508] = EnemyProjectilePreInstruction.Stationary,
        [0xe605] = EnemyProjectilePreInstruction.DownwardGateDescent,
        [0xb6b9] = EnemyProjectilePreInstruction.EyeDoorShotFlight,
        [0xb714] = EnemyProjectilePreInstruction.EyeDoorSweatFall,
        [0xa8ef] = EnemyProjectilePreInstruction.BombTorizoStatueFragmentFall,
        [0xefe0] = EnemyProjectilePreInstruction.PickupLifetime,
        [0xbfdf] = EnemyProjectilePreInstruction.MotherBrainTurretAiming,
        [0xc0e0] = EnemyProjectilePreInstruction.MotherBrainTurretBulletFlight,
        [0xce9b] = EnemyProjectilePreInstruction.MotherBrainGlassShardFlight,
        [0xcbe7] = EnemyProjectilePreInstruction.MotherBrainTubeFall,
        [0xc84d] = EnemyProjectilePreInstruction.MotherBrainDroolHanging,
        [0xc886] = EnemyProjectilePreInstruction.MotherBrainDroolFall,
        [0xc335] = EnemyProjectilePreInstruction.MotherBrainOnionRingFlight,
        [0xc4c8] = EnemyProjectilePreInstruction.MotherBrainBombBounce,
        [0xc814] = EnemyProjectilePreInstruction.MotherBrainRainbowBeamCharge,
        [0xc94c] = EnemyProjectilePreInstruction.MotherBrainRainbowImpactTracking,
        [0xc914] = EnemyProjectilePreInstruction.MotherBrainDeathExplosionTracking,
        [0xc9d2] = EnemyProjectilePreInstruction.MotherBrainDeathDoorFragmentFall,
        [0xcafa] = EnemyProjectilePreInstruction.MotherBrainDeathSubtitleScreenPin,
        [0x8dca] = EnemyProjectilePreInstruction.DraygonGoopStuckToSamus,
        [0x8dff] = EnemyProjectilePreInstruction.DraygonTurretShotFlight,
        [0x8e0f] = EnemyProjectilePreInstruction.DraygonGoopFlight,
        [0xdcee] = EnemyProjectilePreInstruction.SporeDrift,
        [0xdd46] = EnemyProjectilePreInstruction.SporeSpawnerCadence,
        [0xea80] = EnemyProjectilePreInstruction.BotwoonBodySegment,
        [0xec05] = EnemyProjectilePreInstruction.BotwoonSpitFlight,
        [0xacad] = EnemyProjectilePreInstruction.BombTorizoOrbFall,
        [0xacfa] = EnemyProjectilePreInstruction.GoldenTorizoOrbBounce,
        [0xae6c] = EnemyProjectilePreInstruction.TorizoSonicBoomFlight,
        [0xa887] = EnemyProjectilePreInstruction.BombTorizoDroolFall,
        [0xb043] = EnemyProjectilePreInstruction.GoldenTorizoEggBounce,
        [0xb0b9] = EnemyProjectilePreInstruction.GoldenTorizoEggCharge,
        [0xb0dd] = EnemyProjectilePreInstruction.GoldenTorizoEggDrop,
        [0xb20d] = EnemyProjectilePreInstruction.GoldenTorizoSuperMissileHeld,
        [0xb237] = EnemyProjectilePreInstruction.GoldenTorizoSuperMissileThrown,
        [0xb38a] = EnemyProjectilePreInstruction.GoldenTorizoEyeBeamFlight,
        [0xba37] = EnemyProjectilePreInstruction.TourianStatueRevealFollow,
        [0xba42] = EnemyProjectilePreInstruction.TourianStatueDecorationSettled,
        [0xbe03] = EnemyProjectilePreInstruction.ShaktoolFrontCircleFlight,
        [0xbe12] = EnemyProjectilePreInstruction.ShaktoolTrailingCircleFlight,
        [0xe4fe] = EnemyProjectilePreInstruction.DustCameraCull,
        [0xb535] = EnemyProjectilePreInstruction.DragonFireballArc,
        [0x940e] = EnemyProjectilePreInstruction.RidleyFireballFlight,
        [0xd7bf] = EnemyProjectilePreInstruction.NoobTubeCrackFlickering,
        [0xd7de] = EnemyProjectilePreInstruction.NoobTubeCrackFalling,
        [0xd7fd] = EnemyProjectilePreInstruction.NoobTubeShardFlying,
        [0xd83d] = EnemyProjectilePreInstruction.NoobTubeShardFalling,
        [0xd89f] = EnemyProjectilePreInstruction.NoobTubeBubbleFalling,
        [0xd8df] = EnemyProjectilePreInstruction.NoobTubeBubbleFlying,
        [0x950d] = EnemyProjectilePreInstruction.HorizontalAfterburnFlight,
        [0x9522] = EnemyProjectilePreInstruction.VerticalAfterburnFlight,
        [0x8b5d] = EnemyProjectilePreInstruction.SkreeParticleFlight,
        [0x906b] = EnemyProjectilePreInstruction.CrocomireShotAiming,
        [0x90b3] = EnemyProjectilePreInstruction.CrocomireShotFlight,
        [0x9115] = EnemyProjectilePreInstruction.CrocomireSpikeWallCollapse,
        [0x9d56] = EnemyProjectilePreInstruction.KraidRockFlight,
        [0x9d89] = EnemyProjectilePreInstruction.KraidCeilingRockFall,
        [0x9da5] = EnemyProjectilePreInstruction.KraidShotRockRecolor,
        [0x9b29] = EnemyProjectilePreInstruction.PhantoonIntroFlameWaiting,
        [0x9b41] = EnemyProjectilePreInstruction.PhantoonIntroFlameOrbit,
        [0x9981] = EnemyProjectilePreInstruction.PhantoonCasualFlameFall,
        [0x99bf] = EnemyProjectilePreInstruction.PhantoonCasualFlameLanding,
        [0x9a01] = EnemyProjectilePreInstruction.PhantoonCasualFlameBounce,
        [0x9a45] = EnemyProjectilePreInstruction.PhantoonRageFlameOrbit,
        [0x9a94] = EnemyProjectilePreInstruction.PhantoonFlameRainFall,
        [0x9ada] = EnemyProjectilePreInstruction.PhantoonSpiralFlameOrbit,
        [0x92ba] = EnemyProjectilePreInstruction.CrocomireBridgeFragmentFall,
        [0x9eff] = EnemyProjectilePreInstruction.AlcoonFireballFlight,
        [0xcff8] = EnemyProjectilePreInstruction.KiHunterAcidFlight,
        [0xcfd5] = EnemyProjectilePreInstruction.KiHunterAcidLaunchLeft,
        [0xcfe6] = EnemyProjectilePreInstruction.KiHunterAcidLaunchRight,
        [0xd263] = EnemyProjectilePreInstruction.PowampSpikeFlight,
        [0xd3bf] = EnemyProjectilePreInstruction.WorkRobotLaserFlight,
        [0xdb5b] = EnemyProjectilePreInstruction.StokeFireballFlight,
        [0xd9db] = EnemyProjectilePreInstruction.CacatacSpikeFlight,
        [0xbc0f] = EnemyProjectilePreInstruction.PolypRockFlight,
        [0xdf39] = EnemyProjectilePreInstruction.FuneNamiheFireballFlight,
        [0xe049] = EnemyProjectilePreInstruction.MagdolliteLavaFlight,
        [0xd0ca] = EnemyProjectilePreInstruction.KagoBugWaiting,
        [0xd0ec] = EnemyProjectilePreInstruction.KagoBugJump,
        [0xd128] = EnemyProjectilePreInstruction.KagoBugFall,
        [0xf3f0] = EnemyProjectilePreInstruction.FallingSparkBounce,
        [0x9701] = EnemyProjectilePreInstruction.CeresTileFall,
        [0x9e1e] = EnemyProjectilePreInstruction.FakeKraidSpitFlight,
        [0x9e83] = EnemyProjectilePreInstruction.FakeKraidSpikeFlight,
        [0xa05c] = EnemyProjectilePreInstruction.LaserFlightLeft,
        [0xa07a] = EnemyProjectilePreInstruction.LaserFlightRight,
        [0xa0d1] = EnemyProjectilePreInstruction.PirateClawThrownLeft,
        [0xa124] = EnemyProjectilePreInstruction.PirateClawThrownRight,
        [0xb977] = EnemyProjectilePreInstruction.TourianStatueSplashMotion,
        [0xb982] = EnemyProjectilePreInstruction.TourianStatueParticleMotion,
        [0xb9fd] = EnemyProjectilePreInstruction.TourianStatueSoulMotion,
    };

    /// <summary>
    /// The one routine address of <paramref name="preInstruction"/>; a pre-instruction several empty routines stand for has
    /// no single word.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="preInstruction"/> has no single routine address.</exception>
    internal static ushort Word(EnemyProjectilePreInstruction preInstruction)
    {
        ushort[] words = [.. Identities.Where(entry => entry.Value == preInstruction).Select(entry => entry.Key)];
        return words.Length == 1
            ? words[0]
            : throw new ArgumentOutOfRangeException(nameof(preInstruction), preInstruction, $"{preInstruction} stands for {words.Length} routines, not one.");
    }
}
