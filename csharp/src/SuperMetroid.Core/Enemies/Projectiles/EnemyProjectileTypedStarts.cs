using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Enemies.YappingMaws;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.Core.Enemies.Stoke;
using SuperMetroid.Core.Enemies.Nuclear;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.Enemies.Deaths;
using SuperMetroid.Core.Enemies.Chozo;
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
using SuperMetroid.Core.Enemies.EyeDoor;
using SuperMetroid.Core.Enemies.NoobTube;
using SuperMetroid.Core.Enemies.SaveStation;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The typed program each converted projectile kind starts in place of its definition's initial list (#641).
/// A kind absent here still starts its list.
/// </summary>
internal static class EnemyProjectileTypedStarts
{
    /// <summary>Each converted kind's typed initial program, as a factory for a new animation running it.</summary>
    private static readonly Dictionary<RoomEnemyProjectileKind, Func<EnemyProjectileAnimation>> InitialAnimations = new()
    {
        [RoomEnemyProjectileKind.DragonFireball] = () => new DragonFireballAnimation(DragonFireballSegment.RisingLeft),
        [RoomEnemyProjectileKind.WorkRobotLaserUpLeft] = () => new WorkRobotLaserAnimation(WorkRobotLaserSegment.Initial),
        [RoomEnemyProjectileKind.WorkRobotLaserHorizontal] = () => new WorkRobotLaserAnimation(WorkRobotLaserSegment.Initial),
        [RoomEnemyProjectileKind.WorkRobotLaserDownLeft] = () => new WorkRobotLaserAnimation(WorkRobotLaserSegment.Initial),
        [RoomEnemyProjectileKind.WorkRobotLaserUpRight] = () => new WorkRobotLaserAnimation(WorkRobotLaserSegment.Initial),
        [RoomEnemyProjectileKind.WorkRobotLaserDownRight] = () => new WorkRobotLaserAnimation(WorkRobotLaserSegment.Initial),
        [RoomEnemyProjectileKind.FallingSpark] = () => new FallingSparkAnimation(FallingSparkSegment.Falling),
        [RoomEnemyProjectileKind.PowampSpike] = () => new PowampSpikeAnimation(PowampSpikeSegment.Initial),
        [RoomEnemyProjectileKind.KiHunterAcidSpitLeft] = () => new KiHunterAcidSpitAnimation(KiHunterAcidSpitSegment.Left),
        [RoomEnemyProjectileKind.KiHunterAcidSpitRight] = () => new KiHunterAcidSpitAnimation(KiHunterAcidSpitSegment.Right),
        [RoomEnemyProjectileKind.AlcoonFireball] = () => new AlcoonFireballAnimation(AlcoonFireballSegment.Initial),
        [RoomEnemyProjectileKind.FuneFireball] = () => new FuneNamiheFireballAnimation(FuneNamiheFireballSegment.Left),
        [RoomEnemyProjectileKind.NamiheFireball] = () => new FuneNamiheFireballAnimation(FuneNamiheFireballSegment.Left),
        [RoomEnemyProjectileKind.PolypRock] = () => new PolypRockAnimation(PolypRockSegment.Initial),
        [RoomEnemyProjectileKind.LavaThrownByMagdollite] = () => new MagdolliteLavaAnimation(MagdolliteLavaSegment.Left),
        [RoomEnemyProjectileKind.SkreeParticleDownRight] = () => new SkreeMetareeParticleAnimation(SkreeMetareeParticleSegment.Skree),
        [RoomEnemyProjectileKind.SkreeParticleUpRight] = () => new SkreeMetareeParticleAnimation(SkreeMetareeParticleSegment.Skree),
        [RoomEnemyProjectileKind.SkreeParticleDownLeft] = () => new SkreeMetareeParticleAnimation(SkreeMetareeParticleSegment.Skree),
        [RoomEnemyProjectileKind.SkreeParticleUpLeft] = () => new SkreeMetareeParticleAnimation(SkreeMetareeParticleSegment.Skree),
        [RoomEnemyProjectileKind.MetareeParticleDownRight] = () => new SkreeMetareeParticleAnimation(SkreeMetareeParticleSegment.Metaree),
        [RoomEnemyProjectileKind.MetareeParticleUpRight] = () => new SkreeMetareeParticleAnimation(SkreeMetareeParticleSegment.Metaree),
        [RoomEnemyProjectileKind.MetareeParticleDownLeft] = () => new SkreeMetareeParticleAnimation(SkreeMetareeParticleSegment.Metaree),
        [RoomEnemyProjectileKind.MetareeParticleUpLeft] = () => new SkreeMetareeParticleAnimation(SkreeMetareeParticleSegment.Metaree),
        [RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep] = () => new ChozoTourianDustAnimation(ChozoTourianDustSegment.Footsteps),
        [RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstepAlternate] = () => new ChozoTourianDustAnimation(ChozoTourianDustSegment.SpikeClearingExplosions),
        [RoomEnemyProjectileKind.TourianStatueDescentDust] = () => new ChozoTourianDustAnimation(ChozoTourianDustSegment.TourianDescentDust),
        [RoomEnemyProjectileKind.EnemyDeathPickup] = () => new EnemyDeathProjectileAnimation(EnemyDeathProjectileSegment.SmallEnergy),
        [RoomEnemyProjectileKind.EnemyDeathExplosion] = () => new EnemyDeathProjectileAnimation(EnemyDeathProjectileSegment.SmallEnergy),
        [RoomEnemyProjectileKind.CacatacSpike] = () => new CacatacProjectileAnimation(CacatacProjectileSegment.LeftFacingUp),
        [RoomEnemyProjectileKind.NuclearWaffleBody] = () => new NuclearWaffleProjectileAnimation(NuclearWaffleProjectileSegment.Initial),
        [RoomEnemyProjectileKind.StokeProjectile] = () => new StokeProjectileAnimation(StokeProjectileSegment.Initial),
        [RoomEnemyProjectileKind.ShaktoolAttackFrontCircle] = () => new ShaktoolProjectileAnimation(ShaktoolProjectileSegment.Front),
        [RoomEnemyProjectileKind.ShaktoolAttackMiddleCircle] = () => new ShaktoolProjectileAnimation(ShaktoolProjectileSegment.Middle),
        [RoomEnemyProjectileKind.ShaktoolAttackBackCircle] = () => new ShaktoolProjectileAnimation(ShaktoolProjectileSegment.Back),
        [RoomEnemyProjectileKind.YappingMawBody] = () => new YappingMawBodyProjectileAnimation(YappingMawBodyProjectileSegment.FacingDown),
        [RoomEnemyProjectileKind.EyeDoorProjectile] = () => new EyeDoorProjectileAnimation(EyeDoorProjectileSegment.Initial),
        [RoomEnemyProjectileKind.EyeDoorSweat] = () => new EyeDoorSweatAnimation(EyeDoorSweatSegment.Falling),
        [RoomEnemyProjectileKind.SaveStationElectricity] = () => new SaveStationElectricityAnimation(SaveStationElectricitySegment.Start),
        [RoomEnemyProjectileKind.DownwardGateMoving] = () => new DownwardGateProjectileAnimation(DownwardGateProjectileSegment.Moving),
        [RoomEnemyProjectileKind.DownwardGateClosed] = () => new DownwardGateProjectileAnimation(DownwardGateProjectileSegment.Closed),
        [RoomEnemyProjectileKind.NoobTubeCrack] = () => new NoobTubeProjectileAnimation(NoobTubeProjectileSegment.Crack),
        [RoomEnemyProjectileKind.NoobTubeShard] = () => new NoobTubeProjectileAnimation(NoobTubeProjectileSegment.Shard0),
        [RoomEnemyProjectileKind.NoobTubeReleasedAirBubble] = () => new NoobTubeProjectileAnimation(NoobTubeProjectileSegment.ReleasedAirBubble),
        [RoomEnemyProjectileKind.MotherBrainRainbowBeamExplosion] = () => new MiscDustProjectileAnimation(MiscDustProjectileSegment.RainbowBeamExplosion),
        [RoomEnemyProjectileKind.EyeDoorSmoke] = () => new MiscDustProjectileAnimation(MiscDustProjectileSegment.SmallExplosion),
        [RoomEnemyProjectileKind.MiscDustExplosion] = () => new MiscDustProjectileAnimation(MiscDustProjectileSegment.BeamCharge),
    };

    /// <summary>A new animation running the typed initial program of <paramref name="kind"/>, or null when the kind has none.</summary>
    internal static EnemyProjectileAnimation? Initial(RoomEnemyProjectileKind kind) =>
        InitialAnimations.TryGetValue(kind, out Func<EnemyProjectileAnimation>? start) ? start() : null;
}
