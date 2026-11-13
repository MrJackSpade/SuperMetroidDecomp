namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>Every typed enemy-projectile family with a ROM oracle, for recordings that translate typed cursors.</summary>
internal static class NativeEnemyProjectileFamilies
{
    /// <summary>The families, one per typed program set.</summary>
    internal static IReadOnlyList<NativeEnemyProjectileFamily> All { get; } =
    [
        EyeDoorSweatNativeListAddresses.Family,
        TourianStatueProjectileNativeListAddresses.Family,
        SaveStationElectricityNativeListAddresses.Family,
        MotherBrainEffectNativeListAddresses.Family,
        MotherBrainTopTubeNativeListAddresses.Family,
        AlcoonFireballNativeListAddresses.Family,
        CacatacProjectileNativeListAddresses.Family,
        ChozoTourianDustNativeListAddresses.Family,
        EnemyDeathProjectileNativeListAddresses.Family,
        DragonFireballNativeListAddresses.Family,
        FallingSparkNativeListAddresses.Family,
        FuneNamiheFireballNativeListAddresses.Family,
        KagoBugProjectileNativeListAddresses.Family,
        KiHunterAcidSpitNativeListAddresses.Family,
        MagdolliteLavaNativeListAddresses.Family,
        NuclearWaffleProjectileNativeListAddresses.Family,
        PolypRockNativeListAddresses.Family,
        PowampSpikeNativeListAddresses.Family,
        MiscDustProjectileNativeListAddresses.Family,
        ShaktoolProjectileNativeListAddresses.Family,
        SkreeMetareeParticleNativeListAddresses.Family,
        SpacePirateProjectileNativeListAddresses.Family,
        WorkRobotLaserNativeListAddresses.Family,
        YappingMawBodyProjectileNativeListAddresses.Family,
        EyeDoorProjectileNativeListAddresses.Family,
        DownwardGateProjectileNativeListAddresses.Family,
        NoobTubeProjectileNativeListAddresses.Family,
    ];
}
