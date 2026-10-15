using SuperMetroid.Core.Enemies.Skree;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Enemies.Polyp;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.Core.Enemies.Alcoon;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Enemies.Powamp;
using SuperMetroid.Core.Enemies.FallingSparks;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.Core.Enemies.Dragon;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The typed program each converted projectile kind starts in place of its definition's native initial list (#641).
/// A kind absent here still starts its native list.
/// </summary>
internal static class EnemyProjectileTypedStarts
{
    /// <summary>Each converted kind's typed initial program, as a factory for a new animation running it.</summary>
    private static readonly Dictionary<RoomEnemyProjectileKind, Func<EnemyProjectileAnimation>> InitialPrograms = new()
    {
        [RoomEnemyProjectileKind.DragonFireball] = () => new DragonFireballAnimation(DragonFireballProgram.RisingLeft),
        [RoomEnemyProjectileKind.WorkRobotLaserUpLeft] = () => new WorkRobotLaserAnimation(WorkRobotLaserProgram.Initial),
        [RoomEnemyProjectileKind.WorkRobotLaserHorizontal] = () => new WorkRobotLaserAnimation(WorkRobotLaserProgram.Initial),
        [RoomEnemyProjectileKind.WorkRobotLaserDownLeft] = () => new WorkRobotLaserAnimation(WorkRobotLaserProgram.Initial),
        [RoomEnemyProjectileKind.WorkRobotLaserUpRight] = () => new WorkRobotLaserAnimation(WorkRobotLaserProgram.Initial),
        [RoomEnemyProjectileKind.WorkRobotLaserDownRight] = () => new WorkRobotLaserAnimation(WorkRobotLaserProgram.Initial),
        [RoomEnemyProjectileKind.FallingSpark] = () => new FallingSparkAnimation(FallingSparkProgram.Falling),
        [RoomEnemyProjectileKind.PowampSpike] = () => new PowampSpikeAnimation(PowampSpikeProgram.Initial),
        [RoomEnemyProjectileKind.KiHunterAcidSpitLeft] = () => new KiHunterAcidSpitAnimation(KiHunterAcidSpitProgram.Left),
        [RoomEnemyProjectileKind.KiHunterAcidSpitRight] = () => new KiHunterAcidSpitAnimation(KiHunterAcidSpitProgram.Right),
        [RoomEnemyProjectileKind.AlcoonFireball] = () => new AlcoonFireballAnimation(AlcoonFireballProgram.Initial),
        [RoomEnemyProjectileKind.FuneFireball] = () => new FuneNamiheFireballAnimation(FuneNamiheFireballProgram.Left),
        [RoomEnemyProjectileKind.NamiheFireball] = () => new FuneNamiheFireballAnimation(FuneNamiheFireballProgram.Left),
        [RoomEnemyProjectileKind.PolypRock] = () => new PolypRockAnimation(PolypRockProgram.Initial),
        [RoomEnemyProjectileKind.LavaThrownByMagdollite] = () => new MagdolliteLavaAnimation(MagdolliteLavaProgram.Left),
        [RoomEnemyProjectileKind.SkreeParticleDownRight] = () => new SkreeMetareeParticleAnimation(SkreeMetareeParticleProgram.Skree),
        [RoomEnemyProjectileKind.SkreeParticleUpRight] = () => new SkreeMetareeParticleAnimation(SkreeMetareeParticleProgram.Skree),
        [RoomEnemyProjectileKind.SkreeParticleDownLeft] = () => new SkreeMetareeParticleAnimation(SkreeMetareeParticleProgram.Skree),
        [RoomEnemyProjectileKind.SkreeParticleUpLeft] = () => new SkreeMetareeParticleAnimation(SkreeMetareeParticleProgram.Skree),
        [RoomEnemyProjectileKind.MetareeParticleDownRight] = () => new SkreeMetareeParticleAnimation(SkreeMetareeParticleProgram.Metaree),
        [RoomEnemyProjectileKind.MetareeParticleUpRight] = () => new SkreeMetareeParticleAnimation(SkreeMetareeParticleProgram.Metaree),
        [RoomEnemyProjectileKind.MetareeParticleDownLeft] = () => new SkreeMetareeParticleAnimation(SkreeMetareeParticleProgram.Metaree),
        [RoomEnemyProjectileKind.MetareeParticleUpLeft] = () => new SkreeMetareeParticleAnimation(SkreeMetareeParticleProgram.Metaree),
    };

    /// <summary>A new animation running the typed initial program of <paramref name="kind"/>, or null when the kind has none.</summary>
    internal static EnemyProjectileAnimation? Initial(RoomEnemyProjectileKind kind) =>
        InitialPrograms.TryGetValue(kind, out Func<EnemyProjectileAnimation>? start) ? start() : null;
}
