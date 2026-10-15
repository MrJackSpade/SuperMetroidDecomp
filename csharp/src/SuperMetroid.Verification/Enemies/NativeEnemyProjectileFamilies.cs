namespace SuperMetroid.Verification.Enemies;

/// <summary>Every typed enemy-projectile family with a ROM oracle, for recordings that translate typed cursors.</summary>
internal static class NativeEnemyProjectileFamilies
{
    /// <summary>The families, one per typed program set.</summary>
    internal static IReadOnlyList<NativeEnemyProjectileFamily> All { get; } =
    [
        global::SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainEffectAnimationsVerification.Family,
        global::SuperMetroid.Verification.Enemies.Alcoon.AlcoonFireballAnimationsVerification.Family,
        global::SuperMetroid.Verification.Enemies.Cacatac.CacatacProjectileAnimationsVerification.Family,
        global::SuperMetroid.Verification.Enemies.Chozo.ChozoTourianDustAnimationsVerification.Family,
        global::SuperMetroid.Verification.Enemies.Deaths.EnemyDeathProjectileAnimationsVerification.Family,
        global::SuperMetroid.Verification.Enemies.Dragon.DragonFireballAnimationsVerification.Family,
        global::SuperMetroid.Verification.Enemies.FallingSparks.FallingSparkAnimationsVerification.Family,
        global::SuperMetroid.Verification.Enemies.FuneNamihe.FuneNamiheFireballAnimationsVerification.Family,
        global::SuperMetroid.Verification.Enemies.Kago.KagoBugProjectileAnimationsVerification.Family,
        global::SuperMetroid.Verification.Enemies.KiHunters.KiHunterAcidSpitAnimationsVerification.Family,
        global::SuperMetroid.Verification.Enemies.Magdollite.MagdolliteLavaAnimationsVerification.Family,
        global::SuperMetroid.Verification.Enemies.Nuclear.NuclearWaffleProjectileAnimationsVerification.Family,
        global::SuperMetroid.Verification.Enemies.Polyp.PolypRockAnimationsVerification.Family,
        global::SuperMetroid.Verification.Enemies.Powamp.PowampSpikeAnimationsVerification.Family,
        global::SuperMetroid.Verification.Enemies.Projectiles.MiscDustProjectileAnimationsVerification.Family,
        global::SuperMetroid.Verification.Enemies.Shaktool.ShaktoolProjectileAnimationsVerification.Family,
        global::SuperMetroid.Verification.Enemies.Skree.SkreeMetareeParticleAnimationsVerification.Family,
        global::SuperMetroid.Verification.Enemies.Stoke.StokeProjectileAnimationsVerification.Family,
        global::SuperMetroid.Verification.Enemies.WorkRobots.WorkRobotLaserAnimationsVerification.Family,
        global::SuperMetroid.Verification.Enemies.YappingMaws.YappingMawBodyProjectileAnimationsVerification.Family,
        global::SuperMetroid.Verification.Rooms.Doors.EyeDoorProjectileAnimationsVerification.Family,
        global::SuperMetroid.Verification.Rooms.Plms.DownwardGateProjectileAnimationsVerification.Family,
        global::SuperMetroid.Verification.Rooms.Plms.NoobTubeProjectileAnimationsVerification.Family,
    ];
}
