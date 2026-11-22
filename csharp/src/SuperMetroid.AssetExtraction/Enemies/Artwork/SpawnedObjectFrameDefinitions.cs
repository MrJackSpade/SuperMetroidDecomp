using SuperMetroid.Core;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>
/// The projectile frames and room effect-actor kinds each enemy definition can spawn, and those each common spawner (code
/// outside the enemy behaviours) spawns (#1392), which the pre-coloured spawned-object sheets hold, each on the sheet its spawn's graphics name (#1513).
/// Generated from the source by tools/frame-definitions-generator; regenerate rather than edit.
/// </summary>
[ArchitectureReview("large", "Generated data: the projectile frames and effect-actor kinds each enemy definition spawns; it holds no logic, and splitting it by family would only scatter one table the sheet composer reads as a whole.")]
internal static partial class SpawnedObjectFrameDefinitions
{
    /// <summary>The projectile frames <c>Bosses/Botwoon/BotwoonBehavior.cs</c> spawns.</summary>
    private static readonly EnemyProjectileFrameId[] BotwoonProjectiles =
    [
        EnemyProjectileFrameId.BotwoonBodyUpLeftRipple0,
        EnemyProjectileFrameId.BotwoonBodyUpLeftRipple90,
        EnemyProjectileFrameId.BotwoonBodyUpLeftRipple180,
        EnemyProjectileFrameId.BotwoonBodyUpLeftRipple270,
        EnemyProjectileFrameId.BotwoonBodyLeftRipple0,
        EnemyProjectileFrameId.BotwoonBodyLeftRipple90,
        EnemyProjectileFrameId.BotwoonBodyLeftRipple180,
        EnemyProjectileFrameId.BotwoonBodyLeftRipple270,
        EnemyProjectileFrameId.BotwoonBodyDownLeftRipple0,
        EnemyProjectileFrameId.BotwoonBodyDownLeftRipple90,
        EnemyProjectileFrameId.BotwoonBodyDownLeftRipple180,
        EnemyProjectileFrameId.BotwoonBodyDownLeftRipple270,
        EnemyProjectileFrameId.BotwoonBodyDownRipple0,
        EnemyProjectileFrameId.BotwoonBodyDownRipple90,
        EnemyProjectileFrameId.BotwoonBodyDownRipple180,
        EnemyProjectileFrameId.BotwoonBodyDownRipple270,
        EnemyProjectileFrameId.BotwoonBodyDownRightRipple0,
        EnemyProjectileFrameId.BotwoonBodyDownRightRipple90,
        EnemyProjectileFrameId.BotwoonBodyDownRightRipple180,
        EnemyProjectileFrameId.BotwoonBodyDownRightRipple270,
        EnemyProjectileFrameId.BotwoonBodyRightRipple0,
        EnemyProjectileFrameId.BotwoonBodyRightRipple90,
        EnemyProjectileFrameId.BotwoonBodyRightRipple180,
        EnemyProjectileFrameId.BotwoonBodyRightRipple270,
        EnemyProjectileFrameId.BotwoonBodyUpRightRipple0,
        EnemyProjectileFrameId.BotwoonBodyUpRightRipple90,
        EnemyProjectileFrameId.BotwoonBodyUpRightRipple180,
        EnemyProjectileFrameId.BotwoonBodyUpRightRipple270,
        EnemyProjectileFrameId.BotwoonBodyUpRipple0,
        EnemyProjectileFrameId.BotwoonBodyUpRipple90,
        EnemyProjectileFrameId.BotwoonBodyUpRipple180,
        EnemyProjectileFrameId.BotwoonBodyUpRipple270,
        EnemyProjectileFrameId.BotwoonTailUp,
        EnemyProjectileFrameId.BotwoonTailUpLeft,
        EnemyProjectileFrameId.BotwoonTailLeft,
        EnemyProjectileFrameId.BotwoonTailDownLeft,
        EnemyProjectileFrameId.BotwoonTailDown,
        EnemyProjectileFrameId.BotwoonTailDownRight,
        EnemyProjectileFrameId.BotwoonTailRight,
        EnemyProjectileFrameId.BotwoonTailUpRight,
        EnemyProjectileFrameId.BotwoonSpitDim,
        EnemyProjectileFrameId.BotwoonSpitBright,
        EnemyProjectileFrameId.BotwoonSpitRinged,
        EnemyProjectileFrameId.BotwoonSpitRingedBright,
        EnemyProjectileFrameId.BotwoonSpitDissolving,
    ];

    /// <summary>The room effect actors <c>Bosses/Botwoon/BotwoonBehavior.cs</c> spawns.</summary>
    private static readonly RoomEffectActorKind[] BotwoonEffectActors = [];

    /// <summary>The projectile frames <c>Bosses/Crocomire/CrocomireBehavior.cs</c> spawns.</summary>
    private static readonly EnemyProjectileFrameId[] CrocomireProjectiles =
    [
        EnemyProjectileFrameId.CrocomireSpikeWallPiece,
    ];

    /// <summary>The room effect actors <c>Bosses/Crocomire/CrocomireBehavior.cs</c> spawns.</summary>
    private static readonly RoomEffectActorKind[] CrocomireEffectActors = [];

    /// <summary>The projectile frames <c>Bosses/Kraid/FakeKraidBehavior.cs</c> spawns.</summary>
    private static readonly EnemyProjectileFrameId[] FakeKraidProjectiles =
    [
        EnemyProjectileFrameId.FakeKraidSpit,
        EnemyProjectileFrameId.FakeKraidSpikeLeft,
        EnemyProjectileFrameId.FakeKraidSpikeRight,
    ];

    /// <summary>The room effect actors <c>Bosses/Kraid/FakeKraidBehavior.cs</c> spawns.</summary>
    private static readonly RoomEffectActorKind[] FakeKraidEffectActors = [];

    /// <summary>The projectile frames <c>Bosses/Phantoon/PhantoonBehavior.cs</c> spawns.</summary>
    private static readonly EnemyProjectileFrameId[] PhantoonProjectiles =
    [
        EnemyProjectileFrameId.PhantoonFlameRingedEye,
        EnemyProjectileFrameId.PhantoonFlameBrightEye,
        EnemyProjectileFrameId.PhantoonFlameFlickering,
        EnemyProjectileFrameId.PhantoonFlameSplashed,
        EnemyProjectileFrameId.PhantoonFlameSwelling,
        EnemyProjectileFrameId.PhantoonFlameLeaning,
        EnemyProjectileFrameId.PhantoonFlameResting,
        EnemyProjectileFrameId.PhantoonFlameRestingSwaying,
        EnemyProjectileFrameId.PhantoonFlameRestingLow,
        EnemyProjectileFrameId.PhantoonFlameDying,
        EnemyProjectileFrameId.PhantoonFlameGuttering,
        EnemyProjectileFrameId.PhantoonFlameWisps,
        EnemyProjectileFrameId.PhantoonFlameLastWisp,
        EnemyProjectileFrameId.PhantoonFlameKindling,
        EnemyProjectileFrameId.PhantoonFlameTeardrop,
        EnemyProjectileFrameId.PhantoonFlameSputtering,
    ];

    /// <summary>The room effect actors <c>Bosses/Phantoon/PhantoonBehavior.cs</c> spawns.</summary>
    private static readonly RoomEffectActorKind[] PhantoonEffectActors = [];

    /// <summary>The projectile frames <c>Bosses/SporeSpawn/SporeSpawnBehavior.cs</c> spawns.</summary>
    private static readonly EnemyProjectileFrameId[] SporeSpawnProjectiles =
    [
        EnemyProjectileFrameId.SporeSpawnSpawnerClosed,
        EnemyProjectileFrameId.SporeSpawnSpawnerOpening,
        EnemyProjectileFrameId.SporeSpawnSpawnerOpen,
        EnemyProjectileFrameId.SporeSpawnStalk,
    ];

    /// <summary>The room effect actors <c>Bosses/SporeSpawn/SporeSpawnBehavior.cs</c> spawns.</summary>
    private static readonly RoomEffectActorKind[] SporeSpawnEffectActors = [];

    /// <summary>The projectile frames <c>Bosses/Torizo/TorizoBehavior.cs</c> spawns.</summary>
    private static readonly EnemyProjectileFrameId[] TorizoProjectiles =
    [
        EnemyProjectileFrameId.TorizoDroolDrop,
        EnemyProjectileFrameId.TorizoDroolSplashRising,
        EnemyProjectileFrameId.TorizoDroolSplashSpreading,
        EnemyProjectileFrameId.TorizoDroolSplashFlat,
        EnemyProjectileFrameId.TorizoChozoOrbFacingLeft,
        EnemyProjectileFrameId.TorizoChozoOrbFacingRight,
        EnemyProjectileFrameId.TorizoChozoOrbSpark,
        EnemyProjectileFrameId.TorizoChozoOrbFlareBall,
        EnemyProjectileFrameId.TorizoChozoOrbFlareRingCored,
        EnemyProjectileFrameId.TorizoChozoOrbFlareRing,
        EnemyProjectileFrameId.TorizoChozoOrbFlareRingFading,
        EnemyProjectileFrameId.TorizoChozoOrbFlameBurst,
        EnemyProjectileFrameId.TorizoChozoOrbFlameDome,
        EnemyProjectileFrameId.TorizoChozoOrbFlameColumnShort,
        EnemyProjectileFrameId.TorizoChozoOrbFlameColumnTall,
        EnemyProjectileFrameId.TorizoChozoOrbFlameBurstAboveDome,
        EnemyProjectileFrameId.TorizoChozoOrbFlameEmbersAboveDome,
        EnemyProjectileFrameId.TorizoSonicBoomLeftSmall,
        EnemyProjectileFrameId.TorizoSonicBoomLeftMedium,
        EnemyProjectileFrameId.TorizoSonicBoomLeftLarge,
        EnemyProjectileFrameId.TorizoSonicBoomRightSmall,
        EnemyProjectileFrameId.TorizoSonicBoomRightMedium,
        EnemyProjectileFrameId.TorizoSonicBoomRightLarge,
        EnemyProjectileFrameId.TorizoSonicBoomSpark,
        EnemyProjectileFrameId.TorizoSonicBoomFlareBall,
        EnemyProjectileFrameId.TorizoSonicBoomFlareRingCored,
        EnemyProjectileFrameId.TorizoSonicBoomFlareRing,
        EnemyProjectileFrameId.TorizoSonicBoomFlareRingFading,
        EnemyProjectileFrameId.GoldenTorizoEggWholeLeft,
        EnemyProjectileFrameId.GoldenTorizoEggCrackedLeft,
        EnemyProjectileFrameId.GoldenTorizoEggSplittingLeft,
        EnemyProjectileFrameId.GoldenTorizoEggHatchingLeft,
        EnemyProjectileFrameId.GoldenTorizoEggWholeRight,
        EnemyProjectileFrameId.GoldenTorizoEggCrackedRight,
        EnemyProjectileFrameId.GoldenTorizoEggSplittingRight,
        EnemyProjectileFrameId.GoldenTorizoEggHatchingRight,
        EnemyProjectileFrameId.GoldenTorizoHatchlingLeftClawsRaised,
        EnemyProjectileFrameId.GoldenTorizoHatchlingLeftClawsLowered,
        EnemyProjectileFrameId.GoldenTorizoHatchlingLeftHunched,
        EnemyProjectileFrameId.GoldenTorizoHatchlingRightClawsRaised,
        EnemyProjectileFrameId.GoldenTorizoHatchlingRightClawsLowered,
        EnemyProjectileFrameId.GoldenTorizoHatchlingRightHunched,
        EnemyProjectileFrameId.GoldenTorizoHatchlingLeftCrumbling,
        EnemyProjectileFrameId.GoldenTorizoHatchlingLeftCollapsing,
        EnemyProjectileFrameId.GoldenTorizoHatchlingLeftSlumped,
        EnemyProjectileFrameId.GoldenTorizoHatchlingLeftRemains,
        EnemyProjectileFrameId.GoldenTorizoHatchlingLeftRemainsFlat,
        EnemyProjectileFrameId.GoldenTorizoHatchlingRightCrumbling,
        EnemyProjectileFrameId.GoldenTorizoHatchlingRightCollapsing,
        EnemyProjectileFrameId.GoldenTorizoHatchlingRightSlumped,
        EnemyProjectileFrameId.GoldenTorizoHatchlingRightRemains,
        EnemyProjectileFrameId.GoldenTorizoHatchlingRightRemainsFlat,
        EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingDown,
        EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingDownRight,
        EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingRight,
        EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingUpRight,
        EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingUp,
        EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingUpLeft,
        EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingLeft,
        EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingDownLeft,
        EnemyProjectileFrameId.GoldenTorizoSuperMissileFireballSmall,
        EnemyProjectileFrameId.GoldenTorizoSuperMissileFireballLarge,
        EnemyProjectileFrameId.GoldenTorizoSuperMissileFlamingRing,
        EnemyProjectileFrameId.GoldenTorizoSuperMissileFireballHuge,
        EnemyProjectileFrameId.GoldenTorizoSuperMissileWideRing,
        EnemyProjectileFrameId.GoldenTorizoSuperMissileWideRingFading,
        EnemyProjectileFrameId.GoldenTorizoEyeBeamSpark,
        EnemyProjectileFrameId.GoldenTorizoEyeBeamFlareBall,
        EnemyProjectileFrameId.GoldenTorizoEyeBeamFlareRingCored,
        EnemyProjectileFrameId.GoldenTorizoEyeBeamFlareRing,
        EnemyProjectileFrameId.GoldenTorizoEyeBeamFlareRingFading,
        EnemyProjectileFrameId.GoldenTorizoEyeBeamFlameBurst,
        EnemyProjectileFrameId.GoldenTorizoEyeBeamFlameDome,
        EnemyProjectileFrameId.GoldenTorizoEyeBeamFlameColumnShort,
        EnemyProjectileFrameId.GoldenTorizoEyeBeamFlameColumnTall,
        EnemyProjectileFrameId.GoldenTorizoEyeBeamFlameBurstAboveDome,
        EnemyProjectileFrameId.GoldenTorizoEyeBeamFlameEmbersAboveDome,
    ];

    /// <summary>The room effect actors <c>Bosses/Torizo/TorizoBehavior.cs</c> spawns.</summary>
    private static readonly RoomEffectActorKind[] TorizoEffectActors = [];

    /// <summary>The projectile frames <c>Enemies/Alcoon/AlcoonBehavior.cs</c> spawns.</summary>
    private static readonly EnemyProjectileFrameId[] AlcoonProjectiles =
    [
        EnemyProjectileFrameId.AlcoonFireballTurned0,
        EnemyProjectileFrameId.AlcoonFireballTurned90,
        EnemyProjectileFrameId.AlcoonFireballTurned180,
        EnemyProjectileFrameId.AlcoonFireballTurned270,
    ];

    /// <summary>The room effect actors <c>Enemies/Alcoon/AlcoonBehavior.cs</c> spawns.</summary>
    private static readonly RoomEffectActorKind[] AlcoonEffectActors = [];

    /// <summary>The projectile frames <c>Enemies/Cacatac/CacatacBehavior.cs</c> spawns.</summary>
    private static readonly EnemyProjectileFrameId[] CacatacProjectiles =
    [
        EnemyProjectileFrameId.CacatacSpikeLeftHigh,
        EnemyProjectileFrameId.CacatacSpikeUpLeft,
        EnemyProjectileFrameId.CacatacSpikeUp,
        EnemyProjectileFrameId.CacatacSpikeUpRight,
        EnemyProjectileFrameId.CacatacSpikeRightHigh,
        EnemyProjectileFrameId.CacatacSpikeLeftLow,
        EnemyProjectileFrameId.CacatacSpikeDownLeft,
        EnemyProjectileFrameId.CacatacSpikeDown,
        EnemyProjectileFrameId.CacatacSpikeDownRight,
        EnemyProjectileFrameId.CacatacSpikeRightLow,
    ];

    /// <summary>The room effect actors <c>Enemies/Cacatac/CacatacBehavior.cs</c> spawns.</summary>
    private static readonly RoomEffectActorKind[] CacatacEffectActors = [];

    /// <summary>The projectile frames <c>Enemies/Dragon/DragonBehavior.cs</c> spawns.</summary>
    private static readonly EnemyProjectileFrameId[] DragonProjectiles =
    [
        EnemyProjectileFrameId.DragonFireballRisingLeftFlaring,
        EnemyProjectileFrameId.DragonFireballRisingLeftCompact,
        EnemyProjectileFrameId.DragonFireballRisingRightFlaring,
        EnemyProjectileFrameId.DragonFireballRisingRightCompact,
        EnemyProjectileFrameId.DragonFireballFallingLeftFlaring,
        EnemyProjectileFrameId.DragonFireballFallingLeftCompact,
        EnemyProjectileFrameId.DragonFireballFallingRightFlaring,
        EnemyProjectileFrameId.DragonFireballFallingRightCompact,
    ];

    /// <summary>The room effect actors <c>Enemies/Dragon/DragonBehavior.cs</c> spawns.</summary>
    private static readonly RoomEffectActorKind[] DragonEffectActors = [];

    /// <summary>The projectile frames <c>Enemies/FuneNamihe/FuneNamiheBehavior.cs</c> spawns.</summary>
    private static readonly EnemyProjectileFrameId[] FuneNamiheProjectiles =
    [
        EnemyProjectileFrameId.FuneNamiheFireballLeftTrailing,
        EnemyProjectileFrameId.FuneNamiheFireballLeftRound,
        EnemyProjectileFrameId.FuneNamiheFireballLeftDissolving,
        EnemyProjectileFrameId.FuneNamiheFireballRightTrailing,
        EnemyProjectileFrameId.FuneNamiheFireballRightRound,
        EnemyProjectileFrameId.FuneNamiheFireballRightDissolving,
    ];

    /// <summary>The room effect actors <c>Enemies/FuneNamihe/FuneNamiheBehavior.cs</c> spawns.</summary>
    private static readonly RoomEffectActorKind[] FuneNamiheEffectActors = [];

    /// <summary>The projectile frames <c>Enemies/Kago/KagoBehavior.cs</c> spawns.</summary>
    private static readonly EnemyProjectileFrameId[] KagoProjectiles =
    [
        EnemyProjectileFrameId.KagoBugCurledUp,
        EnemyProjectileFrameId.KagoBugCrouched,
        EnemyProjectileFrameId.KagoBugStanding,
        EnemyProjectileFrameId.KagoBugLeaping,
    ];

    /// <summary>The room effect actors <c>Enemies/Kago/KagoBehavior.cs</c> spawns.</summary>
    private static readonly RoomEffectActorKind[] KagoEffectActors = [];

    /// <summary>The projectile frames <c>Enemies/KiHunters/KiHunterBehavior.cs</c> spawns.</summary>
    private static readonly EnemyProjectileFrameId[] KiHunterProjectiles =
    [
        EnemyProjectileFrameId.KiHunterAcidSpitLeftDroplet,
        EnemyProjectileFrameId.KiHunterAcidSpitLeftStreak,
        EnemyProjectileFrameId.KiHunterAcidSpitLeftBlob,
        EnemyProjectileFrameId.KiHunterAcidSpitLeftBlobTrailing,
        EnemyProjectileFrameId.KiHunterAcidSpitLeftBlobSpattering,
        EnemyProjectileFrameId.KiHunterAcidSpitLeftBall,
        EnemyProjectileFrameId.KiHunterAcidSpitSplashMound,
        EnemyProjectileFrameId.KiHunterAcidSpitSplashSpreading,
        EnemyProjectileFrameId.KiHunterAcidSpitSplashWide,
        EnemyProjectileFrameId.KiHunterAcidSpitSplashFlat,
        EnemyProjectileFrameId.KiHunterAcidSpitSplashFading,
        EnemyProjectileFrameId.KiHunterAcidSpitRightDroplet,
        EnemyProjectileFrameId.KiHunterAcidSpitRightStreak,
        EnemyProjectileFrameId.KiHunterAcidSpitRightBlob,
        EnemyProjectileFrameId.KiHunterAcidSpitRightBlobTrailing,
        EnemyProjectileFrameId.KiHunterAcidSpitRightBlobSpattering,
        EnemyProjectileFrameId.KiHunterAcidSpitRightBall,
    ];

    /// <summary>The room effect actors <c>Enemies/KiHunters/KiHunterBehavior.cs</c> spawns.</summary>
    private static readonly RoomEffectActorKind[] KiHunterEffectActors = [];

    /// <summary>The projectile frames <c>Enemies/Magdollite/MagdolliteBehavior.cs</c> spawns.</summary>
    private static readonly EnemyProjectileFrameId[] MagdolliteProjectiles =
    [
        EnemyProjectileFrameId.MagdolliteLavaLeft,
        EnemyProjectileFrameId.MagdolliteLavaRight,
    ];

    /// <summary>The room effect actors <c>Enemies/Magdollite/MagdolliteBehavior.cs</c> spawns.</summary>
    private static readonly RoomEffectActorKind[] MagdolliteEffectActors = [];

    /// <summary>The projectile frames <c>Enemies/Metaree/MetareeBehavior.cs</c> spawns.</summary>
    private static readonly EnemyProjectileFrameId[] MetareeProjectiles =
    [
        EnemyProjectileFrameId.MetareeDebris,
    ];

    /// <summary>The room effect actors <c>Enemies/Metaree/MetareeBehavior.cs</c> spawns.</summary>
    private static readonly RoomEffectActorKind[] MetareeEffectActors = [];

    /// <summary>The projectile frames <c>Enemies/Metroids/MetroidBehavior.cs</c> spawns.</summary>
    private static readonly EnemyProjectileFrameId[] MetroidProjectiles = [];

    /// <summary>The room effect actors <c>Enemies/Metroids/MetroidBehavior.cs</c> spawns.</summary>
    private static readonly RoomEffectActorKind[] MetroidEffectActors =
    [
        RoomEffectActorKind.MetroidElectricity,
        RoomEffectActorKind.MetroidShell,
    ];

    /// <summary>The projectile frames <c>Enemies/Nuclear/NuclearWaffleBehavior.cs</c> spawns.</summary>
    private static readonly EnemyProjectileFrameId[] NuclearWaffleProjectiles =
    [
        EnemyProjectileFrameId.NuclearWaffleFireballSwirl,
        EnemyProjectileFrameId.NuclearWaffleFireballBlazing,
        EnemyProjectileFrameId.NuclearWaffleFireballMottled,
        EnemyProjectileFrameId.NuclearWaffleFireballSwirlTurned,
        EnemyProjectileFrameId.NuclearWaffleFireballBlazingRinged,
        EnemyProjectileFrameId.NuclearWaffleFireballMottledDense,
        EnemyProjectileFrameId.NuclearWaffleFireballMottledTurned,
        EnemyProjectileFrameId.NuclearWaffleFireballMottledSparse,
    ];

    /// <summary>The room effect actors <c>Enemies/Nuclear/NuclearWaffleBehavior.cs</c> spawns.</summary>
    private static readonly RoomEffectActorKind[] NuclearWaffleEffectActors =
    [
        RoomEffectActorKind.NuclearWaffleBody,
        RoomEffectActorKind.NuclearWaffleLeftExplosion,
        RoomEffectActorKind.NuclearWaffleRightExplosion,
        RoomEffectActorKind.NuclearWaffleSplash,
    ];

    /// <summary>The projectile frames <c>Enemies/Polyp/PolypBehavior.cs</c> spawns.</summary>
    private static readonly EnemyProjectileFrameId[] PolypProjectiles =
    [
        EnemyProjectileFrameId.PolypRock,
    ];

    /// <summary>The room effect actors <c>Enemies/Polyp/PolypBehavior.cs</c> spawns.</summary>
    private static readonly RoomEffectActorKind[] PolypEffectActors = [];

    /// <summary>The projectile frames <c>Enemies/Powamp/PowampBehavior.cs</c> spawns.</summary>
    private static readonly EnemyProjectileFrameId[] PowampProjectiles =
    [
        EnemyProjectileFrameId.PowampSpikeStar,
        EnemyProjectileFrameId.PowampSpikeCluster,
        EnemyProjectileFrameId.PowampSpikeGlow,
    ];

    /// <summary>The room effect actors <c>Enemies/Powamp/PowampBehavior.cs</c> spawns.</summary>
    private static readonly RoomEffectActorKind[] PowampEffectActors = [];

    /// <summary>The projectile frames <c>Enemies/Skree/SkreeBehavior.cs</c> spawns.</summary>
    private static readonly EnemyProjectileFrameId[] SkreeProjectiles =
    [
        EnemyProjectileFrameId.SkreeDebris,
    ];

    /// <summary>The room effect actors <c>Enemies/Skree/SkreeBehavior.cs</c> spawns.</summary>
    private static readonly RoomEffectActorKind[] SkreeEffectActors = [];

    /// <summary>The projectile frames <c>Enemies/SpacePirates/NinjaSpacePirateBehavior.cs</c> spawns.</summary>
    private static readonly EnemyProjectileFrameId[] NinjaSpacePirateProjectiles =
    [
        EnemyProjectileFrameId.SpacePirateClawLeftTurned0,
        EnemyProjectileFrameId.SpacePirateClawLeftTurned45,
        EnemyProjectileFrameId.SpacePirateClawLeftTurned90,
        EnemyProjectileFrameId.SpacePirateClawLeftTurned135,
        EnemyProjectileFrameId.SpacePirateClawLeftTurned180,
        EnemyProjectileFrameId.SpacePirateClawLeftTurned225,
        EnemyProjectileFrameId.SpacePirateClawLeftTurned270,
        EnemyProjectileFrameId.SpacePirateClawLeftTurned315,
        EnemyProjectileFrameId.SpacePirateClawRightTurned0,
        EnemyProjectileFrameId.SpacePirateClawRightTurned45,
        EnemyProjectileFrameId.SpacePirateClawRightTurned90,
        EnemyProjectileFrameId.SpacePirateClawRightTurned135,
        EnemyProjectileFrameId.SpacePirateClawRightTurned180,
        EnemyProjectileFrameId.SpacePirateClawRightTurned225,
        EnemyProjectileFrameId.SpacePirateClawRightTurned270,
        EnemyProjectileFrameId.SpacePirateClawRightTurned315,
    ];

    /// <summary>The room effect actors <c>Enemies/SpacePirates/NinjaSpacePirateBehavior.cs</c> spawns.</summary>
    private static readonly RoomEffectActorKind[] NinjaSpacePirateEffectActors = [];

    /// <summary>The projectile frames <c>Enemies/Sparks/SparkBehavior.cs</c> spawns.</summary>
    private static readonly EnemyProjectileFrameId[] SparkProjectiles =
    [
        EnemyProjectileFrameId.FallingSparkUpright,
        EnemyProjectileFrameId.FallingSparkDiagonal,
        EnemyProjectileFrameId.FallingSparkUprightSmall,
    ];

    /// <summary>The room effect actors <c>Enemies/Sparks/SparkBehavior.cs</c> spawns.</summary>
    private static readonly RoomEffectActorKind[] SparkEffectActors =
    [
        RoomEffectActorKind.FallingSparkTrail,
    ];

    /// <summary>The projectile frames <c>Enemies/YappingMaws/YappingMawBehavior.cs</c> spawns.</summary>
    private static readonly EnemyProjectileFrameId[] YappingMawProjectiles =
    [
        EnemyProjectileFrameId.YappingMawFacingDown,
        EnemyProjectileFrameId.YappingMawFacingUp,
    ];

    /// <summary>The room effect actors <c>Enemies/YappingMaws/YappingMawBehavior.cs</c> spawns.</summary>
    private static readonly RoomEffectActorKind[] YappingMawEffectActors =
    [
        RoomEffectActorKind.YappingMawBaseFacingDown,
        RoomEffectActorKind.YappingMawBaseFacingUp,
    ];

    /// <summary>The projectile frames <c>Areas/Ceres/CeresElevatorDoorFactory.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] CeresElevatorDoorSharedProjectiles =
    [
        EnemyProjectileFrameId.DustExplosionSparksGathering,
        EnemyProjectileFrameId.DustExplosionSparksDrawingIn,
        EnemyProjectileFrameId.DustExplosionSparksClosingIn,
        EnemyProjectileFrameId.DustExplosionSparksConverging,
        EnemyProjectileFrameId.DustExplosionSparksNearing,
        EnemyProjectileFrameId.DustExplosionSparksMeeting,
        EnemyProjectileFrameId.DustExplosionSparksTwistedGathering,
        EnemyProjectileFrameId.DustExplosionSparksTwistedDrawingIn,
        EnemyProjectileFrameId.DustExplosionSparksTwistedClosingIn,
        EnemyProjectileFrameId.DustExplosionSparksTwistedConverging,
        EnemyProjectileFrameId.DustExplosionSparksTwistedNearing,
        EnemyProjectileFrameId.DustExplosionSparksTwistedMeeting,
        EnemyProjectileFrameId.DustExplosionSpark,
        EnemyProjectileFrameId.DustExplosionFireballSmall,
        EnemyProjectileFrameId.DustExplosionFireballLarge,
        EnemyProjectileFrameId.DustExplosionFlamingRing,
        EnemyProjectileFrameId.DustExplosionStarburst,
        EnemyProjectileFrameId.DustExplosionEmbers,
        EnemyProjectileFrameId.DustExplosionGreenOrbSmall,
        EnemyProjectileFrameId.DustExplosionGreenOrb,
        EnemyProjectileFrameId.DustExplosionGreenRingCored,
        EnemyProjectileFrameId.DustExplosionGreenRingThick,
        EnemyProjectileFrameId.DustExplosionGreenRing,
        EnemyProjectileFrameId.DustExplosionFlareGlint,
        EnemyProjectileFrameId.DustExplosionFlareGlow,
        EnemyProjectileFrameId.DustExplosionFlareBall,
        EnemyProjectileFrameId.DustExplosionFlareRingCored,
        EnemyProjectileFrameId.DustExplosionFlareRing,
        EnemyProjectileFrameId.DustExplosionFlareRingFading,
        EnemyProjectileFrameId.DustExplosionPuff,
        EnemyProjectileFrameId.DustExplosionCloud,
        EnemyProjectileFrameId.DustExplosionCloudThinning,
        EnemyProjectileFrameId.DustExplosionCloudThin,
        EnemyProjectileFrameId.DustExplosionCloudFaint,
        EnemyProjectileFrameId.DustExplosionPuffSmall,
        EnemyProjectileFrameId.DustExplosionPuffThin,
        EnemyProjectileFrameId.DustExplosionPuffFaint,
        EnemyProjectileFrameId.DustExplosionSmokePuffLow,
        EnemyProjectileFrameId.DustExplosionSmokePuffRising,
        EnemyProjectileFrameId.DustExplosionSmokePuffScattering,
        EnemyProjectileFrameId.DustExplosionSmokePuffFading,
        EnemyProjectileFrameId.DustExplosionFireCloud,
        EnemyProjectileFrameId.DustExplosionFireCloudSpreading,
        EnemyProjectileFrameId.DustExplosionFireCloudBreaking,
        EnemyProjectileFrameId.DustExplosionFireCloudEmbers,
        EnemyProjectileFrameId.DustExplosionFireballHuge,
        EnemyProjectileFrameId.DustExplosionWideRing,
        EnemyProjectileFrameId.DustExplosionWideRingFading,
        EnemyProjectileFrameId.DustExplosionPebblesBursting,
        EnemyProjectileFrameId.DustExplosionPebblesFlyingOut,
        EnemyProjectileFrameId.DustExplosionPebblesFlyingApart,
        EnemyProjectileFrameId.DustExplosionPebblesClimbing,
        EnemyProjectileFrameId.DustExplosionPebblesSlowing,
        EnemyProjectileFrameId.DustExplosionPebblesPeaking,
        EnemyProjectileFrameId.DustExplosionPebblesTurningOver,
        EnemyProjectileFrameId.DustExplosionPebblesStartingDown,
        EnemyProjectileFrameId.DustExplosionPebblesDropping,
        EnemyProjectileFrameId.DustExplosionPebblesFalling,
        EnemyProjectileFrameId.DustExplosionPebblesTurningAshen,
        EnemyProjectileFrameId.DustExplosionPebblesTumbling,
        EnemyProjectileFrameId.DustExplosionPebblesPlunging,
        EnemyProjectileFrameId.DustExplosionPebblesPlummeting,
        EnemyProjectileFrameId.DustExplosionPebblesScattering,
        EnemyProjectileFrameId.DustExplosionPebblesFallingAway,
        EnemyProjectileFrameId.DustExplosionPuffUpLeft,
        EnemyProjectileFrameId.DustExplosionPuffPair,
        EnemyProjectileFrameId.DustExplosionPuffTrio,
        EnemyProjectileFrameId.DustExplosionCloudsDense,
        EnemyProjectileFrameId.DustExplosionCloudsThinning,
        EnemyProjectileFrameId.DustExplosionCloudsThin,
        EnemyProjectileFrameId.DustExplosionCloudsFaint,
        EnemyProjectileFrameId.DustExplosionCloudFaintRight,
        EnemyProjectileFrameId.DustExplosionBubbleSmall,
        EnemyProjectileFrameId.DustExplosionBubbles,
        EnemyProjectileFrameId.DustExplosionBubblesRising,
        EnemyProjectileFrameId.DustExplosionBubblesSpreading,
        EnemyProjectileFrameId.DustExplosionBubblesScattered,
        EnemyProjectileFrameId.DustExplosionBubblesSparse,
        EnemyProjectileFrameId.DustExplosionBubblesFew,
        EnemyProjectileFrameId.DustExplosionBubblesLast,
        EnemyProjectileFrameId.DustExplosionSpeck,
    ];

    /// <summary>The room effect actors <c>Areas/Ceres/CeresElevatorDoorFactory.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] CeresElevatorDoorSharedEffectActors = [];

    /// <summary>The projectile frames <c>Areas/Ceres/CeresSteamFactory.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] CeresSteamSharedProjectiles =
    [
        EnemyProjectileFrameId.EnemyDeathFireballSmall,
        EnemyProjectileFrameId.EnemyDeathFireballLarge,
        EnemyProjectileFrameId.EnemyDeathFlamingRing,
        EnemyProjectileFrameId.EnemyDeathFireballHuge,
        EnemyProjectileFrameId.EnemyDeathWideRing,
        EnemyProjectileFrameId.EnemyDeathWideRingFading,
        EnemyProjectileFrameId.EnemyDeathSpark,
        EnemyProjectileFrameId.EnemyDeathStarburst,
        EnemyProjectileFrameId.EnemyDeathEmbers,
        EnemyProjectileFrameId.EnemyPickupEnergySmallDark,
        EnemyProjectileFrameId.EnemyPickupEnergySmallRinged,
        EnemyProjectileFrameId.EnemyPickupEnergySmallBright,
        EnemyProjectileFrameId.EnemyPickupEnergyScattered,
        EnemyProjectileFrameId.EnemyPickupEnergyBigBright,
        EnemyProjectileFrameId.EnemyPickupEnergyBigRinged,
        EnemyProjectileFrameId.EnemyPickupEnergyBigDark,
        EnemyProjectileFrameId.EnemyPickupMissile,
        EnemyProjectileFrameId.EnemyPickupMissileFlashing,
        EnemyProjectileFrameId.EnemyPickupSuperMissile,
        EnemyProjectileFrameId.EnemyPickupSuperMissileFlashing,
        EnemyProjectileFrameId.EnemyPickupPowerBombBright,
        EnemyProjectileFrameId.EnemyPickupPowerBombFading,
        EnemyProjectileFrameId.EnemyPickupPowerBombDim,
        EnemyProjectileFrameId.EnemyPickupPowerBombFlashing,
        EnemyProjectileFrameId.EnemyDeathFireballCluster,
        EnemyProjectileFrameId.EnemyDeathStarburstCluster,
        EnemyProjectileFrameId.EnemyDeathEmbersCluster,
        EnemyProjectileFrameId.EnemyDeathFourFireballsAmidEmbers,
        EnemyProjectileFrameId.EnemyDeathFourFireballsAmidAsh,
        EnemyProjectileFrameId.EnemyDeathFourEmbersFlyingApart,
        EnemyProjectileFrameId.EnemyDeathFourEmbersSpreading,
        EnemyProjectileFrameId.EnemyDeathFourEmbersCooling,
        EnemyProjectileFrameId.EnemyDeathFourEmbersDimming,
        EnemyProjectileFrameId.EnemyDeathFourAshDrifting,
        EnemyProjectileFrameId.EnemyDeathFourAshSpreading,
        EnemyProjectileFrameId.EnemyDeathFourAshSinking,
        EnemyProjectileFrameId.EnemyDeathFourAshFalling,
        EnemyProjectileFrameId.EnemyDeathFourAshSettling,
        EnemyProjectileFrameId.EnemyDeathFourAshDispersing,
    ];

    /// <summary>The room effect actors <c>Areas/Ceres/CeresSteamFactory.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] CeresSteamSharedEffectActors =
    [
        RoomEffectActorKind.BigDustCloud,
        RoomEffectActorKind.SmallExplosion,
        RoomEffectActorKind.Smoke,
    ];

    /// <summary>The projectile frames <c>Areas/Tourian/TourianEntranceStatueBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] TourianEntranceStatueSharedProjectiles =
    [
        EnemyProjectileFrameId.ChozoDustPuff,
        EnemyProjectileFrameId.ChozoDustPuffPair,
        EnemyProjectileFrameId.ChozoDustPuffTrio,
        EnemyProjectileFrameId.ChozoDustCloudsDense,
        EnemyProjectileFrameId.TourianStatueSplashLow,
        EnemyProjectileFrameId.TourianStatueSplashRising,
        EnemyProjectileFrameId.TourianStatueSplashScattering,
        EnemyProjectileFrameId.TourianStatueSplashFading,
        EnemyProjectileFrameId.TourianStatueEyeGlowRingFaint,
        EnemyProjectileFrameId.TourianStatueEyeGlowRing,
        EnemyProjectileFrameId.TourianStatueEyeGlowRingCrossed,
        EnemyProjectileFrameId.TourianStatueEyeGlowStreak,
        EnemyProjectileFrameId.TourianStatueEyeGlowCrossRinged,
        EnemyProjectileFrameId.TourianStatueEyeGlowSparkle,
        EnemyProjectileFrameId.TourianStatueEyeGlowSparkleSmall,
        EnemyProjectileFrameId.TourianStatueEyeGlowStreakSmall,
        EnemyProjectileFrameId.TourianStatueEyeGlowDot,
        EnemyProjectileFrameId.TourianStatueParticleTurned0,
        EnemyProjectileFrameId.TourianStatueParticleTurned90,
        EnemyProjectileFrameId.TourianStatueParticleTurned180,
        EnemyProjectileFrameId.TourianStatueParticleTurned270,
        EnemyProjectileFrameId.TourianStatueTailBright,
        EnemyProjectileFrameId.TourianStatueTailSparkles,
        EnemyProjectileFrameId.TourianStatueTailDim,
        EnemyProjectileFrameId.TourianStatueTailFaint,
        EnemyProjectileFrameId.TourianStatueSoulMouthClosed,
        EnemyProjectileFrameId.TourianStatueSoulMouthOpen,
        EnemyProjectileFrameId.TourianStatueBaseDecoration,
        EnemyProjectileFrameId.TourianStatueRidley,
        EnemyProjectileFrameId.TourianStatuePhantoon,
    ];

    /// <summary>The room effect actors <c>Areas/Tourian/TourianEntranceStatueBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] TourianEntranceStatueSharedEffectActors = [];

    /// <summary>The projectile frames <c>Bosses/Botwoon/BotwoonBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] BotwoonSharedProjectiles =
    [
        EnemyProjectileFrameId.DustExplosionSparksGathering,
        EnemyProjectileFrameId.DustExplosionSparksDrawingIn,
        EnemyProjectileFrameId.DustExplosionSparksClosingIn,
        EnemyProjectileFrameId.DustExplosionSparksConverging,
        EnemyProjectileFrameId.DustExplosionSparksNearing,
        EnemyProjectileFrameId.DustExplosionSparksMeeting,
        EnemyProjectileFrameId.DustExplosionSparksTwistedGathering,
        EnemyProjectileFrameId.DustExplosionSparksTwistedDrawingIn,
        EnemyProjectileFrameId.DustExplosionSparksTwistedClosingIn,
        EnemyProjectileFrameId.DustExplosionSparksTwistedConverging,
        EnemyProjectileFrameId.DustExplosionSparksTwistedNearing,
        EnemyProjectileFrameId.DustExplosionSparksTwistedMeeting,
        EnemyProjectileFrameId.DustExplosionSpark,
        EnemyProjectileFrameId.DustExplosionFireballSmall,
        EnemyProjectileFrameId.DustExplosionFireballLarge,
        EnemyProjectileFrameId.DustExplosionFlamingRing,
        EnemyProjectileFrameId.DustExplosionStarburst,
        EnemyProjectileFrameId.DustExplosionEmbers,
        EnemyProjectileFrameId.DustExplosionGreenOrbSmall,
        EnemyProjectileFrameId.DustExplosionGreenOrb,
        EnemyProjectileFrameId.DustExplosionGreenRingCored,
        EnemyProjectileFrameId.DustExplosionGreenRingThick,
        EnemyProjectileFrameId.DustExplosionGreenRing,
        EnemyProjectileFrameId.DustExplosionFlareGlint,
        EnemyProjectileFrameId.DustExplosionFlareGlow,
        EnemyProjectileFrameId.DustExplosionFlareBall,
        EnemyProjectileFrameId.DustExplosionFlareRingCored,
        EnemyProjectileFrameId.DustExplosionFlareRing,
        EnemyProjectileFrameId.DustExplosionFlareRingFading,
        EnemyProjectileFrameId.DustExplosionPuff,
        EnemyProjectileFrameId.DustExplosionCloud,
        EnemyProjectileFrameId.DustExplosionCloudThinning,
        EnemyProjectileFrameId.DustExplosionCloudThin,
        EnemyProjectileFrameId.DustExplosionCloudFaint,
        EnemyProjectileFrameId.DustExplosionPuffSmall,
        EnemyProjectileFrameId.DustExplosionPuffThin,
        EnemyProjectileFrameId.DustExplosionPuffFaint,
        EnemyProjectileFrameId.DustExplosionSmokePuffLow,
        EnemyProjectileFrameId.DustExplosionSmokePuffRising,
        EnemyProjectileFrameId.DustExplosionSmokePuffScattering,
        EnemyProjectileFrameId.DustExplosionSmokePuffFading,
        EnemyProjectileFrameId.DustExplosionFireCloud,
        EnemyProjectileFrameId.DustExplosionFireCloudSpreading,
        EnemyProjectileFrameId.DustExplosionFireCloudBreaking,
        EnemyProjectileFrameId.DustExplosionFireCloudEmbers,
        EnemyProjectileFrameId.DustExplosionFireballHuge,
        EnemyProjectileFrameId.DustExplosionWideRing,
        EnemyProjectileFrameId.DustExplosionWideRingFading,
        EnemyProjectileFrameId.DustExplosionPebblesBursting,
        EnemyProjectileFrameId.DustExplosionPebblesFlyingOut,
        EnemyProjectileFrameId.DustExplosionPebblesFlyingApart,
        EnemyProjectileFrameId.DustExplosionPebblesClimbing,
        EnemyProjectileFrameId.DustExplosionPebblesSlowing,
        EnemyProjectileFrameId.DustExplosionPebblesPeaking,
        EnemyProjectileFrameId.DustExplosionPebblesTurningOver,
        EnemyProjectileFrameId.DustExplosionPebblesStartingDown,
        EnemyProjectileFrameId.DustExplosionPebblesDropping,
        EnemyProjectileFrameId.DustExplosionPebblesFalling,
        EnemyProjectileFrameId.DustExplosionPebblesTurningAshen,
        EnemyProjectileFrameId.DustExplosionPebblesTumbling,
        EnemyProjectileFrameId.DustExplosionPebblesPlunging,
        EnemyProjectileFrameId.DustExplosionPebblesPlummeting,
        EnemyProjectileFrameId.DustExplosionPebblesScattering,
        EnemyProjectileFrameId.DustExplosionPebblesFallingAway,
        EnemyProjectileFrameId.DustExplosionPuffUpLeft,
        EnemyProjectileFrameId.DustExplosionPuffPair,
        EnemyProjectileFrameId.DustExplosionPuffTrio,
        EnemyProjectileFrameId.DustExplosionCloudsDense,
        EnemyProjectileFrameId.DustExplosionCloudsThinning,
        EnemyProjectileFrameId.DustExplosionCloudsThin,
        EnemyProjectileFrameId.DustExplosionCloudsFaint,
        EnemyProjectileFrameId.DustExplosionCloudFaintRight,
        EnemyProjectileFrameId.DustExplosionBubbleSmall,
        EnemyProjectileFrameId.DustExplosionBubbles,
        EnemyProjectileFrameId.DustExplosionBubblesRising,
        EnemyProjectileFrameId.DustExplosionBubblesSpreading,
        EnemyProjectileFrameId.DustExplosionBubblesScattered,
        EnemyProjectileFrameId.DustExplosionBubblesSparse,
        EnemyProjectileFrameId.DustExplosionBubblesFew,
        EnemyProjectileFrameId.DustExplosionBubblesLast,
        EnemyProjectileFrameId.DustExplosionSpeck,
        EnemyProjectileFrameId.EnemyDeathFireballSmall,
        EnemyProjectileFrameId.EnemyDeathFireballLarge,
        EnemyProjectileFrameId.EnemyDeathFlamingRing,
        EnemyProjectileFrameId.EnemyDeathFireballHuge,
        EnemyProjectileFrameId.EnemyDeathWideRing,
        EnemyProjectileFrameId.EnemyDeathWideRingFading,
        EnemyProjectileFrameId.EnemyDeathSpark,
        EnemyProjectileFrameId.EnemyDeathStarburst,
        EnemyProjectileFrameId.EnemyDeathEmbers,
        EnemyProjectileFrameId.EnemyPickupEnergySmallDark,
        EnemyProjectileFrameId.EnemyPickupEnergySmallRinged,
        EnemyProjectileFrameId.EnemyPickupEnergySmallBright,
        EnemyProjectileFrameId.EnemyPickupEnergyScattered,
        EnemyProjectileFrameId.EnemyPickupEnergyBigBright,
        EnemyProjectileFrameId.EnemyPickupEnergyBigRinged,
        EnemyProjectileFrameId.EnemyPickupEnergyBigDark,
        EnemyProjectileFrameId.EnemyPickupMissile,
        EnemyProjectileFrameId.EnemyPickupMissileFlashing,
        EnemyProjectileFrameId.EnemyPickupSuperMissile,
        EnemyProjectileFrameId.EnemyPickupSuperMissileFlashing,
        EnemyProjectileFrameId.EnemyPickupPowerBombBright,
        EnemyProjectileFrameId.EnemyPickupPowerBombFading,
        EnemyProjectileFrameId.EnemyPickupPowerBombDim,
        EnemyProjectileFrameId.EnemyPickupPowerBombFlashing,
        EnemyProjectileFrameId.EnemyDeathFireballCluster,
        EnemyProjectileFrameId.EnemyDeathStarburstCluster,
        EnemyProjectileFrameId.EnemyDeathEmbersCluster,
        EnemyProjectileFrameId.EnemyDeathFourFireballsAmidEmbers,
        EnemyProjectileFrameId.EnemyDeathFourFireballsAmidAsh,
        EnemyProjectileFrameId.EnemyDeathFourEmbersFlyingApart,
        EnemyProjectileFrameId.EnemyDeathFourEmbersSpreading,
        EnemyProjectileFrameId.EnemyDeathFourEmbersCooling,
        EnemyProjectileFrameId.EnemyDeathFourEmbersDimming,
        EnemyProjectileFrameId.EnemyDeathFourAshDrifting,
        EnemyProjectileFrameId.EnemyDeathFourAshSpreading,
        EnemyProjectileFrameId.EnemyDeathFourAshSinking,
        EnemyProjectileFrameId.EnemyDeathFourAshFalling,
        EnemyProjectileFrameId.EnemyDeathFourAshSettling,
        EnemyProjectileFrameId.EnemyDeathFourAshDispersing,
    ];

    /// <summary>The room effect actors <c>Bosses/Botwoon/BotwoonBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] BotwoonSharedEffectActors =
    [
        RoomEffectActorKind.BigDustCloud,
        RoomEffectActorKind.BigExplosion,
        RoomEffectActorKind.SmallExplosion,
        RoomEffectActorKind.Smoke,
        RoomEffectActorKind.SmokePuff,
    ];

    /// <summary>The projectile frames <c>Bosses/Crocomire/CrocomireBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] CrocomireSharedProjectiles =
    [
        EnemyProjectileFrameId.CrocomireMouthBallSmall,
        EnemyProjectileFrameId.CrocomireMouthBallMedium,
        EnemyProjectileFrameId.CrocomireMouthBallLarge,
        EnemyProjectileFrameId.CrocomireMouthBallLargest,
        EnemyProjectileFrameId.CrocomireBridgeFragment,
        EnemyProjectileFrameId.CrocomireShotSpark,
        EnemyProjectileFrameId.CrocomireShotFlareBall,
        EnemyProjectileFrameId.CrocomireShotFlareRingCored,
        EnemyProjectileFrameId.CrocomireShotFlareRing,
        EnemyProjectileFrameId.CrocomireShotFlareRingFading,
        EnemyProjectileFrameId.DustExplosionSparksGathering,
        EnemyProjectileFrameId.DustExplosionSparksDrawingIn,
        EnemyProjectileFrameId.DustExplosionSparksClosingIn,
        EnemyProjectileFrameId.DustExplosionSparksConverging,
        EnemyProjectileFrameId.DustExplosionSparksNearing,
        EnemyProjectileFrameId.DustExplosionSparksMeeting,
        EnemyProjectileFrameId.DustExplosionSparksTwistedGathering,
        EnemyProjectileFrameId.DustExplosionSparksTwistedDrawingIn,
        EnemyProjectileFrameId.DustExplosionSparksTwistedClosingIn,
        EnemyProjectileFrameId.DustExplosionSparksTwistedConverging,
        EnemyProjectileFrameId.DustExplosionSparksTwistedNearing,
        EnemyProjectileFrameId.DustExplosionSparksTwistedMeeting,
        EnemyProjectileFrameId.DustExplosionSpark,
        EnemyProjectileFrameId.DustExplosionFireballSmall,
        EnemyProjectileFrameId.DustExplosionFireballLarge,
        EnemyProjectileFrameId.DustExplosionFlamingRing,
        EnemyProjectileFrameId.DustExplosionStarburst,
        EnemyProjectileFrameId.DustExplosionEmbers,
        EnemyProjectileFrameId.DustExplosionGreenOrbSmall,
        EnemyProjectileFrameId.DustExplosionGreenOrb,
        EnemyProjectileFrameId.DustExplosionGreenRingCored,
        EnemyProjectileFrameId.DustExplosionGreenRingThick,
        EnemyProjectileFrameId.DustExplosionGreenRing,
        EnemyProjectileFrameId.DustExplosionFlareGlint,
        EnemyProjectileFrameId.DustExplosionFlareGlow,
        EnemyProjectileFrameId.DustExplosionFlareBall,
        EnemyProjectileFrameId.DustExplosionFlareRingCored,
        EnemyProjectileFrameId.DustExplosionFlareRing,
        EnemyProjectileFrameId.DustExplosionFlareRingFading,
        EnemyProjectileFrameId.DustExplosionPuff,
        EnemyProjectileFrameId.DustExplosionCloud,
        EnemyProjectileFrameId.DustExplosionCloudThinning,
        EnemyProjectileFrameId.DustExplosionCloudThin,
        EnemyProjectileFrameId.DustExplosionCloudFaint,
        EnemyProjectileFrameId.DustExplosionPuffSmall,
        EnemyProjectileFrameId.DustExplosionPuffThin,
        EnemyProjectileFrameId.DustExplosionPuffFaint,
        EnemyProjectileFrameId.DustExplosionSmokePuffLow,
        EnemyProjectileFrameId.DustExplosionSmokePuffRising,
        EnemyProjectileFrameId.DustExplosionSmokePuffScattering,
        EnemyProjectileFrameId.DustExplosionSmokePuffFading,
        EnemyProjectileFrameId.DustExplosionFireCloud,
        EnemyProjectileFrameId.DustExplosionFireCloudSpreading,
        EnemyProjectileFrameId.DustExplosionFireCloudBreaking,
        EnemyProjectileFrameId.DustExplosionFireCloudEmbers,
        EnemyProjectileFrameId.DustExplosionFireballHuge,
        EnemyProjectileFrameId.DustExplosionWideRing,
        EnemyProjectileFrameId.DustExplosionWideRingFading,
        EnemyProjectileFrameId.DustExplosionPebblesBursting,
        EnemyProjectileFrameId.DustExplosionPebblesFlyingOut,
        EnemyProjectileFrameId.DustExplosionPebblesFlyingApart,
        EnemyProjectileFrameId.DustExplosionPebblesClimbing,
        EnemyProjectileFrameId.DustExplosionPebblesSlowing,
        EnemyProjectileFrameId.DustExplosionPebblesPeaking,
        EnemyProjectileFrameId.DustExplosionPebblesTurningOver,
        EnemyProjectileFrameId.DustExplosionPebblesStartingDown,
        EnemyProjectileFrameId.DustExplosionPebblesDropping,
        EnemyProjectileFrameId.DustExplosionPebblesFalling,
        EnemyProjectileFrameId.DustExplosionPebblesTurningAshen,
        EnemyProjectileFrameId.DustExplosionPebblesTumbling,
        EnemyProjectileFrameId.DustExplosionPebblesPlunging,
        EnemyProjectileFrameId.DustExplosionPebblesPlummeting,
        EnemyProjectileFrameId.DustExplosionPebblesScattering,
        EnemyProjectileFrameId.DustExplosionPebblesFallingAway,
        EnemyProjectileFrameId.DustExplosionPuffUpLeft,
        EnemyProjectileFrameId.DustExplosionPuffPair,
        EnemyProjectileFrameId.DustExplosionPuffTrio,
        EnemyProjectileFrameId.DustExplosionCloudsDense,
        EnemyProjectileFrameId.DustExplosionCloudsThinning,
        EnemyProjectileFrameId.DustExplosionCloudsThin,
        EnemyProjectileFrameId.DustExplosionCloudsFaint,
        EnemyProjectileFrameId.DustExplosionCloudFaintRight,
        EnemyProjectileFrameId.DustExplosionBubbleSmall,
        EnemyProjectileFrameId.DustExplosionBubbles,
        EnemyProjectileFrameId.DustExplosionBubblesRising,
        EnemyProjectileFrameId.DustExplosionBubblesSpreading,
        EnemyProjectileFrameId.DustExplosionBubblesScattered,
        EnemyProjectileFrameId.DustExplosionBubblesSparse,
        EnemyProjectileFrameId.DustExplosionBubblesFew,
        EnemyProjectileFrameId.DustExplosionBubblesLast,
        EnemyProjectileFrameId.DustExplosionSpeck,
        EnemyProjectileFrameId.EnemyDeathFireballSmall,
        EnemyProjectileFrameId.EnemyDeathFireballLarge,
        EnemyProjectileFrameId.EnemyDeathFlamingRing,
        EnemyProjectileFrameId.EnemyDeathFireballHuge,
        EnemyProjectileFrameId.EnemyDeathWideRing,
        EnemyProjectileFrameId.EnemyDeathWideRingFading,
        EnemyProjectileFrameId.EnemyDeathSpark,
        EnemyProjectileFrameId.EnemyDeathStarburst,
        EnemyProjectileFrameId.EnemyDeathEmbers,
        EnemyProjectileFrameId.EnemyPickupEnergySmallDark,
        EnemyProjectileFrameId.EnemyPickupEnergySmallRinged,
        EnemyProjectileFrameId.EnemyPickupEnergySmallBright,
        EnemyProjectileFrameId.EnemyPickupEnergyScattered,
        EnemyProjectileFrameId.EnemyPickupEnergyBigBright,
        EnemyProjectileFrameId.EnemyPickupEnergyBigRinged,
        EnemyProjectileFrameId.EnemyPickupEnergyBigDark,
        EnemyProjectileFrameId.EnemyPickupMissile,
        EnemyProjectileFrameId.EnemyPickupMissileFlashing,
        EnemyProjectileFrameId.EnemyPickupSuperMissile,
        EnemyProjectileFrameId.EnemyPickupSuperMissileFlashing,
        EnemyProjectileFrameId.EnemyPickupPowerBombBright,
        EnemyProjectileFrameId.EnemyPickupPowerBombFading,
        EnemyProjectileFrameId.EnemyPickupPowerBombDim,
        EnemyProjectileFrameId.EnemyPickupPowerBombFlashing,
        EnemyProjectileFrameId.EnemyDeathFireballCluster,
        EnemyProjectileFrameId.EnemyDeathStarburstCluster,
        EnemyProjectileFrameId.EnemyDeathEmbersCluster,
        EnemyProjectileFrameId.EnemyDeathFourFireballsAmidEmbers,
        EnemyProjectileFrameId.EnemyDeathFourFireballsAmidAsh,
        EnemyProjectileFrameId.EnemyDeathFourEmbersFlyingApart,
        EnemyProjectileFrameId.EnemyDeathFourEmbersSpreading,
        EnemyProjectileFrameId.EnemyDeathFourEmbersCooling,
        EnemyProjectileFrameId.EnemyDeathFourEmbersDimming,
        EnemyProjectileFrameId.EnemyDeathFourAshDrifting,
        EnemyProjectileFrameId.EnemyDeathFourAshSpreading,
        EnemyProjectileFrameId.EnemyDeathFourAshSinking,
        EnemyProjectileFrameId.EnemyDeathFourAshFalling,
        EnemyProjectileFrameId.EnemyDeathFourAshSettling,
        EnemyProjectileFrameId.EnemyDeathFourAshDispersing,
    ];

    /// <summary>The room effect actors <c>Bosses/Crocomire/CrocomireBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] CrocomireSharedEffectActors = CeresSteamSharedEffectActors;

    /// <summary>The projectile frames <c>Bosses/Draygon/DraygonBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] DraygonSharedProjectiles =
    [
        EnemyProjectileFrameId.DraygonGoopSpiky,
        EnemyProjectileFrameId.DraygonGoopPuffed,
        EnemyProjectileFrameId.DraygonGoopRounded,
        EnemyProjectileFrameId.DraygonGoopLopsided,
        EnemyProjectileFrameId.DraygonGoopCurled,
        EnemyProjectileFrameId.DraygonGoopScattered,
        EnemyProjectileFrameId.DraygonWallTurretSparksGathering,
        EnemyProjectileFrameId.DraygonWallTurretSparksDrawingIn,
        EnemyProjectileFrameId.DraygonWallTurretSparksClosingIn,
        EnemyProjectileFrameId.DraygonWallTurretSparksConverging,
        EnemyProjectileFrameId.DraygonWallTurretSparksNearing,
        EnemyProjectileFrameId.DraygonWallTurretSparksMeeting,
        EnemyProjectileFrameId.DraygonWallTurretSparksTwistedGathering,
        EnemyProjectileFrameId.DraygonWallTurretSparksTwistedDrawingIn,
        EnemyProjectileFrameId.DraygonWallTurretSparksTwistedClosingIn,
        EnemyProjectileFrameId.DraygonWallTurretSparksTwistedConverging,
        EnemyProjectileFrameId.DraygonWallTurretSparksTwistedNearing,
        EnemyProjectileFrameId.DraygonWallTurretSparksTwistedMeeting,
        EnemyProjectileFrameId.DraygonWallTurretGlowFaint,
        EnemyProjectileFrameId.DraygonWallTurretGlowDim,
        EnemyProjectileFrameId.DraygonWallTurretGlowBright,
        EnemyProjectileFrameId.DraygonWallTurretOrbForming,
        EnemyProjectileFrameId.DraygonWallTurretOrbBright,
        EnemyProjectileFrameId.DraygonWallTurretOrbFlickering,
        EnemyProjectileFrameId.DraygonWallTurretOrbDimmed,
        EnemyProjectileFrameId.DustExplosionSparksGathering,
        EnemyProjectileFrameId.DustExplosionSparksDrawingIn,
        EnemyProjectileFrameId.DustExplosionSparksClosingIn,
        EnemyProjectileFrameId.DustExplosionSparksConverging,
        EnemyProjectileFrameId.DustExplosionSparksNearing,
        EnemyProjectileFrameId.DustExplosionSparksMeeting,
        EnemyProjectileFrameId.DustExplosionSparksTwistedGathering,
        EnemyProjectileFrameId.DustExplosionSparksTwistedDrawingIn,
        EnemyProjectileFrameId.DustExplosionSparksTwistedClosingIn,
        EnemyProjectileFrameId.DustExplosionSparksTwistedConverging,
        EnemyProjectileFrameId.DustExplosionSparksTwistedNearing,
        EnemyProjectileFrameId.DustExplosionSparksTwistedMeeting,
        EnemyProjectileFrameId.DustExplosionSpark,
        EnemyProjectileFrameId.DustExplosionFireballSmall,
        EnemyProjectileFrameId.DustExplosionFireballLarge,
        EnemyProjectileFrameId.DustExplosionFlamingRing,
        EnemyProjectileFrameId.DustExplosionStarburst,
        EnemyProjectileFrameId.DustExplosionEmbers,
        EnemyProjectileFrameId.DustExplosionGreenOrbSmall,
        EnemyProjectileFrameId.DustExplosionGreenOrb,
        EnemyProjectileFrameId.DustExplosionGreenRingCored,
        EnemyProjectileFrameId.DustExplosionGreenRingThick,
        EnemyProjectileFrameId.DustExplosionGreenRing,
        EnemyProjectileFrameId.DustExplosionFlareGlint,
        EnemyProjectileFrameId.DustExplosionFlareGlow,
        EnemyProjectileFrameId.DustExplosionFlareBall,
        EnemyProjectileFrameId.DustExplosionFlareRingCored,
        EnemyProjectileFrameId.DustExplosionFlareRing,
        EnemyProjectileFrameId.DustExplosionFlareRingFading,
        EnemyProjectileFrameId.DustExplosionPuff,
        EnemyProjectileFrameId.DustExplosionCloud,
        EnemyProjectileFrameId.DustExplosionCloudThinning,
        EnemyProjectileFrameId.DustExplosionCloudThin,
        EnemyProjectileFrameId.DustExplosionCloudFaint,
        EnemyProjectileFrameId.DustExplosionPuffSmall,
        EnemyProjectileFrameId.DustExplosionPuffThin,
        EnemyProjectileFrameId.DustExplosionPuffFaint,
        EnemyProjectileFrameId.DustExplosionSmokePuffLow,
        EnemyProjectileFrameId.DustExplosionSmokePuffRising,
        EnemyProjectileFrameId.DustExplosionSmokePuffScattering,
        EnemyProjectileFrameId.DustExplosionSmokePuffFading,
        EnemyProjectileFrameId.DustExplosionFireCloud,
        EnemyProjectileFrameId.DustExplosionFireCloudSpreading,
        EnemyProjectileFrameId.DustExplosionFireCloudBreaking,
        EnemyProjectileFrameId.DustExplosionFireCloudEmbers,
        EnemyProjectileFrameId.DustExplosionFireballHuge,
        EnemyProjectileFrameId.DustExplosionWideRing,
        EnemyProjectileFrameId.DustExplosionWideRingFading,
        EnemyProjectileFrameId.DustExplosionPebblesBursting,
        EnemyProjectileFrameId.DustExplosionPebblesFlyingOut,
        EnemyProjectileFrameId.DustExplosionPebblesFlyingApart,
        EnemyProjectileFrameId.DustExplosionPebblesClimbing,
        EnemyProjectileFrameId.DustExplosionPebblesSlowing,
        EnemyProjectileFrameId.DustExplosionPebblesPeaking,
        EnemyProjectileFrameId.DustExplosionPebblesTurningOver,
        EnemyProjectileFrameId.DustExplosionPebblesStartingDown,
        EnemyProjectileFrameId.DustExplosionPebblesDropping,
        EnemyProjectileFrameId.DustExplosionPebblesFalling,
        EnemyProjectileFrameId.DustExplosionPebblesTurningAshen,
        EnemyProjectileFrameId.DustExplosionPebblesTumbling,
        EnemyProjectileFrameId.DustExplosionPebblesPlunging,
        EnemyProjectileFrameId.DustExplosionPebblesPlummeting,
        EnemyProjectileFrameId.DustExplosionPebblesScattering,
        EnemyProjectileFrameId.DustExplosionPebblesFallingAway,
        EnemyProjectileFrameId.DustExplosionPuffUpLeft,
        EnemyProjectileFrameId.DustExplosionPuffPair,
        EnemyProjectileFrameId.DustExplosionPuffTrio,
        EnemyProjectileFrameId.DustExplosionCloudsDense,
        EnemyProjectileFrameId.DustExplosionCloudsThinning,
        EnemyProjectileFrameId.DustExplosionCloudsThin,
        EnemyProjectileFrameId.DustExplosionCloudsFaint,
        EnemyProjectileFrameId.DustExplosionCloudFaintRight,
        EnemyProjectileFrameId.DustExplosionBubbleSmall,
        EnemyProjectileFrameId.DustExplosionBubbles,
        EnemyProjectileFrameId.DustExplosionBubblesRising,
        EnemyProjectileFrameId.DustExplosionBubblesSpreading,
        EnemyProjectileFrameId.DustExplosionBubblesScattered,
        EnemyProjectileFrameId.DustExplosionBubblesSparse,
        EnemyProjectileFrameId.DustExplosionBubblesFew,
        EnemyProjectileFrameId.DustExplosionBubblesLast,
        EnemyProjectileFrameId.DustExplosionSpeck,
        EnemyProjectileFrameId.EnemyDeathFireballSmall,
        EnemyProjectileFrameId.EnemyDeathFireballLarge,
        EnemyProjectileFrameId.EnemyDeathFlamingRing,
        EnemyProjectileFrameId.EnemyDeathFireballHuge,
        EnemyProjectileFrameId.EnemyDeathWideRing,
        EnemyProjectileFrameId.EnemyDeathWideRingFading,
        EnemyProjectileFrameId.EnemyDeathSpark,
        EnemyProjectileFrameId.EnemyDeathStarburst,
        EnemyProjectileFrameId.EnemyDeathEmbers,
        EnemyProjectileFrameId.EnemyPickupEnergySmallDark,
        EnemyProjectileFrameId.EnemyPickupEnergySmallRinged,
        EnemyProjectileFrameId.EnemyPickupEnergySmallBright,
        EnemyProjectileFrameId.EnemyPickupEnergyScattered,
        EnemyProjectileFrameId.EnemyPickupEnergyBigBright,
        EnemyProjectileFrameId.EnemyPickupEnergyBigRinged,
        EnemyProjectileFrameId.EnemyPickupEnergyBigDark,
        EnemyProjectileFrameId.EnemyPickupMissile,
        EnemyProjectileFrameId.EnemyPickupMissileFlashing,
        EnemyProjectileFrameId.EnemyPickupSuperMissile,
        EnemyProjectileFrameId.EnemyPickupSuperMissileFlashing,
        EnemyProjectileFrameId.EnemyPickupPowerBombBright,
        EnemyProjectileFrameId.EnemyPickupPowerBombFading,
        EnemyProjectileFrameId.EnemyPickupPowerBombDim,
        EnemyProjectileFrameId.EnemyPickupPowerBombFlashing,
        EnemyProjectileFrameId.EnemyDeathFireballCluster,
        EnemyProjectileFrameId.EnemyDeathStarburstCluster,
        EnemyProjectileFrameId.EnemyDeathEmbersCluster,
        EnemyProjectileFrameId.EnemyDeathFourFireballsAmidEmbers,
        EnemyProjectileFrameId.EnemyDeathFourFireballsAmidAsh,
        EnemyProjectileFrameId.EnemyDeathFourEmbersFlyingApart,
        EnemyProjectileFrameId.EnemyDeathFourEmbersSpreading,
        EnemyProjectileFrameId.EnemyDeathFourEmbersCooling,
        EnemyProjectileFrameId.EnemyDeathFourEmbersDimming,
        EnemyProjectileFrameId.EnemyDeathFourAshDrifting,
        EnemyProjectileFrameId.EnemyDeathFourAshSpreading,
        EnemyProjectileFrameId.EnemyDeathFourAshSinking,
        EnemyProjectileFrameId.EnemyDeathFourAshFalling,
        EnemyProjectileFrameId.EnemyDeathFourAshSettling,
        EnemyProjectileFrameId.EnemyDeathFourAshDispersing,
    ];

    /// <summary>The room effect actors <c>Bosses/Draygon/DraygonBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] DraygonSharedEffectActors =
    [
        RoomEffectActorKind.BigDustCloud,
        RoomEffectActorKind.BigExplosion,
        RoomEffectActorKind.DraygonBreathBubbles,
        RoomEffectActorKind.DraygonFoam,
        RoomEffectActorKind.DudShot,
        RoomEffectActorKind.EvirFacingLeft,
        RoomEffectActorKind.EvirFacingRight,
        RoomEffectActorKind.SmallExplosion,
        RoomEffectActorKind.Smoke,
    ];

    /// <summary>The projectile frames <c>Bosses/Kraid/FakeKraidBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] FakeKraidSharedProjectiles = CeresSteamSharedProjectiles;

    /// <summary>The room effect actors <c>Bosses/Kraid/FakeKraidBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] FakeKraidSharedEffectActors = CeresSteamSharedEffectActors;

    /// <summary>The projectile frames <c>Bosses/Kraid/KraidBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] KraidSharedProjectiles =
    [
        EnemyProjectileFrameId.KraidRockLarge,
        EnemyProjectileFrameId.KraidRockSmall,
        EnemyProjectileFrameId.KraidRockSpark,
        EnemyProjectileFrameId.KraidRockFlareBall,
        EnemyProjectileFrameId.KraidRockFlareRingCored,
        EnemyProjectileFrameId.KraidRockFlareRing,
        EnemyProjectileFrameId.KraidRockFlareRingFading,
        EnemyProjectileFrameId.DustExplosionSparksGathering,
        EnemyProjectileFrameId.DustExplosionSparksDrawingIn,
        EnemyProjectileFrameId.DustExplosionSparksClosingIn,
        EnemyProjectileFrameId.DustExplosionSparksConverging,
        EnemyProjectileFrameId.DustExplosionSparksNearing,
        EnemyProjectileFrameId.DustExplosionSparksMeeting,
        EnemyProjectileFrameId.DustExplosionSparksTwistedGathering,
        EnemyProjectileFrameId.DustExplosionSparksTwistedDrawingIn,
        EnemyProjectileFrameId.DustExplosionSparksTwistedClosingIn,
        EnemyProjectileFrameId.DustExplosionSparksTwistedConverging,
        EnemyProjectileFrameId.DustExplosionSparksTwistedNearing,
        EnemyProjectileFrameId.DustExplosionSparksTwistedMeeting,
        EnemyProjectileFrameId.DustExplosionSpark,
        EnemyProjectileFrameId.DustExplosionFireballSmall,
        EnemyProjectileFrameId.DustExplosionFireballLarge,
        EnemyProjectileFrameId.DustExplosionFlamingRing,
        EnemyProjectileFrameId.DustExplosionStarburst,
        EnemyProjectileFrameId.DustExplosionEmbers,
        EnemyProjectileFrameId.DustExplosionGreenOrbSmall,
        EnemyProjectileFrameId.DustExplosionGreenOrb,
        EnemyProjectileFrameId.DustExplosionGreenRingCored,
        EnemyProjectileFrameId.DustExplosionGreenRingThick,
        EnemyProjectileFrameId.DustExplosionGreenRing,
        EnemyProjectileFrameId.DustExplosionFlareGlint,
        EnemyProjectileFrameId.DustExplosionFlareGlow,
        EnemyProjectileFrameId.DustExplosionFlareBall,
        EnemyProjectileFrameId.DustExplosionFlareRingCored,
        EnemyProjectileFrameId.DustExplosionFlareRing,
        EnemyProjectileFrameId.DustExplosionFlareRingFading,
        EnemyProjectileFrameId.DustExplosionPuff,
        EnemyProjectileFrameId.DustExplosionCloud,
        EnemyProjectileFrameId.DustExplosionCloudThinning,
        EnemyProjectileFrameId.DustExplosionCloudThin,
        EnemyProjectileFrameId.DustExplosionCloudFaint,
        EnemyProjectileFrameId.DustExplosionPuffSmall,
        EnemyProjectileFrameId.DustExplosionPuffThin,
        EnemyProjectileFrameId.DustExplosionPuffFaint,
        EnemyProjectileFrameId.DustExplosionSmokePuffLow,
        EnemyProjectileFrameId.DustExplosionSmokePuffRising,
        EnemyProjectileFrameId.DustExplosionSmokePuffScattering,
        EnemyProjectileFrameId.DustExplosionSmokePuffFading,
        EnemyProjectileFrameId.DustExplosionFireCloud,
        EnemyProjectileFrameId.DustExplosionFireCloudSpreading,
        EnemyProjectileFrameId.DustExplosionFireCloudBreaking,
        EnemyProjectileFrameId.DustExplosionFireCloudEmbers,
        EnemyProjectileFrameId.DustExplosionFireballHuge,
        EnemyProjectileFrameId.DustExplosionWideRing,
        EnemyProjectileFrameId.DustExplosionWideRingFading,
        EnemyProjectileFrameId.DustExplosionPebblesBursting,
        EnemyProjectileFrameId.DustExplosionPebblesFlyingOut,
        EnemyProjectileFrameId.DustExplosionPebblesFlyingApart,
        EnemyProjectileFrameId.DustExplosionPebblesClimbing,
        EnemyProjectileFrameId.DustExplosionPebblesSlowing,
        EnemyProjectileFrameId.DustExplosionPebblesPeaking,
        EnemyProjectileFrameId.DustExplosionPebblesTurningOver,
        EnemyProjectileFrameId.DustExplosionPebblesStartingDown,
        EnemyProjectileFrameId.DustExplosionPebblesDropping,
        EnemyProjectileFrameId.DustExplosionPebblesFalling,
        EnemyProjectileFrameId.DustExplosionPebblesTurningAshen,
        EnemyProjectileFrameId.DustExplosionPebblesTumbling,
        EnemyProjectileFrameId.DustExplosionPebblesPlunging,
        EnemyProjectileFrameId.DustExplosionPebblesPlummeting,
        EnemyProjectileFrameId.DustExplosionPebblesScattering,
        EnemyProjectileFrameId.DustExplosionPebblesFallingAway,
        EnemyProjectileFrameId.DustExplosionPuffUpLeft,
        EnemyProjectileFrameId.DustExplosionPuffPair,
        EnemyProjectileFrameId.DustExplosionPuffTrio,
        EnemyProjectileFrameId.DustExplosionCloudsDense,
        EnemyProjectileFrameId.DustExplosionCloudsThinning,
        EnemyProjectileFrameId.DustExplosionCloudsThin,
        EnemyProjectileFrameId.DustExplosionCloudsFaint,
        EnemyProjectileFrameId.DustExplosionCloudFaintRight,
        EnemyProjectileFrameId.DustExplosionBubbleSmall,
        EnemyProjectileFrameId.DustExplosionBubbles,
        EnemyProjectileFrameId.DustExplosionBubblesRising,
        EnemyProjectileFrameId.DustExplosionBubblesSpreading,
        EnemyProjectileFrameId.DustExplosionBubblesScattered,
        EnemyProjectileFrameId.DustExplosionBubblesSparse,
        EnemyProjectileFrameId.DustExplosionBubblesFew,
        EnemyProjectileFrameId.DustExplosionBubblesLast,
        EnemyProjectileFrameId.DustExplosionSpeck,
        EnemyProjectileFrameId.EnemyDeathFireballSmall,
        EnemyProjectileFrameId.EnemyDeathFireballLarge,
        EnemyProjectileFrameId.EnemyDeathFlamingRing,
        EnemyProjectileFrameId.EnemyDeathFireballHuge,
        EnemyProjectileFrameId.EnemyDeathWideRing,
        EnemyProjectileFrameId.EnemyDeathWideRingFading,
        EnemyProjectileFrameId.EnemyDeathSpark,
        EnemyProjectileFrameId.EnemyDeathStarburst,
        EnemyProjectileFrameId.EnemyDeathEmbers,
        EnemyProjectileFrameId.EnemyPickupEnergySmallDark,
        EnemyProjectileFrameId.EnemyPickupEnergySmallRinged,
        EnemyProjectileFrameId.EnemyPickupEnergySmallBright,
        EnemyProjectileFrameId.EnemyPickupEnergyScattered,
        EnemyProjectileFrameId.EnemyPickupEnergyBigBright,
        EnemyProjectileFrameId.EnemyPickupEnergyBigRinged,
        EnemyProjectileFrameId.EnemyPickupEnergyBigDark,
        EnemyProjectileFrameId.EnemyPickupMissile,
        EnemyProjectileFrameId.EnemyPickupMissileFlashing,
        EnemyProjectileFrameId.EnemyPickupSuperMissile,
        EnemyProjectileFrameId.EnemyPickupSuperMissileFlashing,
        EnemyProjectileFrameId.EnemyPickupPowerBombBright,
        EnemyProjectileFrameId.EnemyPickupPowerBombFading,
        EnemyProjectileFrameId.EnemyPickupPowerBombDim,
        EnemyProjectileFrameId.EnemyPickupPowerBombFlashing,
        EnemyProjectileFrameId.EnemyDeathFireballCluster,
        EnemyProjectileFrameId.EnemyDeathStarburstCluster,
        EnemyProjectileFrameId.EnemyDeathEmbersCluster,
        EnemyProjectileFrameId.EnemyDeathFourFireballsAmidEmbers,
        EnemyProjectileFrameId.EnemyDeathFourFireballsAmidAsh,
        EnemyProjectileFrameId.EnemyDeathFourEmbersFlyingApart,
        EnemyProjectileFrameId.EnemyDeathFourEmbersSpreading,
        EnemyProjectileFrameId.EnemyDeathFourEmbersCooling,
        EnemyProjectileFrameId.EnemyDeathFourEmbersDimming,
        EnemyProjectileFrameId.EnemyDeathFourAshDrifting,
        EnemyProjectileFrameId.EnemyDeathFourAshSpreading,
        EnemyProjectileFrameId.EnemyDeathFourAshSinking,
        EnemyProjectileFrameId.EnemyDeathFourAshFalling,
        EnemyProjectileFrameId.EnemyDeathFourAshSettling,
        EnemyProjectileFrameId.EnemyDeathFourAshDispersing,
    ];

    /// <summary>The room effect actors <c>Bosses/Kraid/KraidBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] KraidSharedEffectActors =
    [
        RoomEffectActorKind.BigDustCloud,
        RoomEffectActorKind.DudShot,
        RoomEffectActorKind.SmallExplosion,
        RoomEffectActorKind.Smoke,
    ];

    /// <summary>The projectile frames <c>Bosses/MotherBrain/BabyMetroidBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] BabyMetroidSharedProjectiles = CeresSteamSharedProjectiles;

    /// <summary>The room effect actors <c>Bosses/MotherBrain/BabyMetroidBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] BabyMetroidSharedEffectActors = CeresSteamSharedEffectActors;

    /// <summary>The projectile frames <c>Bosses/MotherBrain/BabyMetroidCutsceneBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] BabyMetroidCutsceneSharedProjectiles = CeresElevatorDoorSharedProjectiles;

    /// <summary>The room effect actors <c>Bosses/MotherBrain/BabyMetroidCutsceneBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] BabyMetroidCutsceneSharedEffectActors = [];

    /// <summary>The projectile frames <c>Bosses/MotherBrain/MotherBrainBodyFactory.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] MotherBrainBodySharedProjectiles =
    [
        EnemyProjectileFrameId.RidleyFireballSpark,
        EnemyProjectileFrameId.RidleyFireballFireCloud,
        EnemyProjectileFrameId.RidleyFireballFireCloudSpreading,
        EnemyProjectileFrameId.RidleyFireballFireCloudBreaking,
        EnemyProjectileFrameId.RidleyFireballFireCloudEmbers,
        EnemyProjectileFrameId.SpacePirateLaserFlareGlint,
        EnemyProjectileFrameId.SpacePirateLaserFlareGlow,
        EnemyProjectileFrameId.SpacePirateLaserFlareBall,
        EnemyProjectileFrameId.SpacePirateLaserEmergingBright,
        EnemyProjectileFrameId.SpacePirateLaserEmergingDim,
        EnemyProjectileFrameId.SpacePirateLaserExtendingBright,
        EnemyProjectileFrameId.SpacePirateLaserExtendingDim,
        EnemyProjectileFrameId.SpacePirateLaserLengtheningBright,
        EnemyProjectileFrameId.SpacePirateLaserLengtheningDim,
        EnemyProjectileFrameId.SpacePirateLaserFullLengthBright,
        EnemyProjectileFrameId.SpacePirateLaserFullLengthDim,
        EnemyProjectileFrameId.MotherBrainTurretFacingLeft,
        EnemyProjectileFrameId.MotherBrainTurretFacingDownLeft,
        EnemyProjectileFrameId.MotherBrainTurretFacingDown,
        EnemyProjectileFrameId.MotherBrainTurretFacingDownRight,
        EnemyProjectileFrameId.MotherBrainTurretFacingRight,
        EnemyProjectileFrameId.MotherBrainTurretFacingUpRight,
        EnemyProjectileFrameId.MotherBrainTurretFacingUp,
        EnemyProjectileFrameId.MotherBrainTurretFacingUpLeft,
        EnemyProjectileFrameId.MotherBrainTurretBulletFacingLeft,
        EnemyProjectileFrameId.MotherBrainTurretBulletFacingDownLeft,
        EnemyProjectileFrameId.MotherBrainTurretBulletFacingDown,
        EnemyProjectileFrameId.MotherBrainTurretBulletFacingDownRight,
        EnemyProjectileFrameId.MotherBrainTurretBulletFacingRight,
        EnemyProjectileFrameId.MotherBrainTurretBulletFacingUpRight,
        EnemyProjectileFrameId.MotherBrainTurretBulletFacingUp,
        EnemyProjectileFrameId.MotherBrainTurretBulletFacingUpLeft,
        EnemyProjectileFrameId.MotherBrainTurretBulletFireCloud,
        EnemyProjectileFrameId.MotherBrainTurretBulletFireCloudSpreading,
        EnemyProjectileFrameId.MotherBrainTurretBulletFireCloudBreaking,
        EnemyProjectileFrameId.MotherBrainTurretBulletFireCloudEmbers,
        EnemyProjectileFrameId.MotherBrainTurretBulletSpeck,
        EnemyProjectileFrameId.MotherBrainBlueRingForming,
        EnemyProjectileFrameId.MotherBrainBlueRingOpening,
        EnemyProjectileFrameId.MotherBrainBlueRingSpreading,
        EnemyProjectileFrameId.MotherBrainBlueRingGrowing,
        EnemyProjectileFrameId.MotherBrainBlueRingSwelling,
        EnemyProjectileFrameId.MotherBrainBlueRingFullSize,
        EnemyProjectileFrameId.MotherBrainBlueRingFireballSmall,
        EnemyProjectileFrameId.MotherBrainBlueRingFireballLarge,
        EnemyProjectileFrameId.MotherBrainBlueRingFlamingRing,
        EnemyProjectileFrameId.MotherBrainBlueRingFireballHuge,
        EnemyProjectileFrameId.MotherBrainBlueRingWideRing,
        EnemyProjectileFrameId.MotherBrainBlueRingWideRingFading,
        EnemyProjectileFrameId.MotherBrainBombGlowRed,
        EnemyProjectileFrameId.MotherBrainBombGlowOrangeRight,
        EnemyProjectileFrameId.MotherBrainBombGlowYellowRight,
        EnemyProjectileFrameId.MotherBrainBombGlowYellowTop,
        EnemyProjectileFrameId.MotherBrainBombGlowOrangeCenter,
        EnemyProjectileFrameId.MotherBrainBombGlowYellowBottom,
        EnemyProjectileFrameId.MotherBrainBombGlowYellowLeft,
        EnemyProjectileFrameId.MotherBrainBombGlowOrangeLeft,
        EnemyProjectileFrameId.MotherBrainBombGlowRedDim,
        EnemyProjectileFrameId.MotherBrainHandBeamOrbSmall,
        EnemyProjectileFrameId.MotherBrainHandBeamOrbRed,
        EnemyProjectileFrameId.MotherBrainHandBeamOrbRedYellowCore,
        EnemyProjectileFrameId.MotherBrainHandBeamOrbOrange,
        EnemyProjectileFrameId.MotherBrainHandBeamOrbWhiteCore,
        EnemyProjectileFrameId.MotherBrainHandBeamOrbYellow,
        EnemyProjectileFrameId.MotherBrainHandBeamOrbWhite,
        EnemyProjectileFrameId.MotherBrainRainbowChargeSparksSpread,
        EnemyProjectileFrameId.MotherBrainRainbowChargeSparksClosing,
        EnemyProjectileFrameId.MotherBrainRainbowChargeStreaksClosing,
        EnemyProjectileFrameId.MotherBrainRainbowChargeStreaksNear,
        EnemyProjectileFrameId.MotherBrainRainbowChargeStreaksGathered,
        EnemyProjectileFrameId.MotherBrainRainbowChargeStreaksAndFireballs,
        EnemyProjectileFrameId.MotherBrainDroolSpeck,
        EnemyProjectileFrameId.MotherBrainDroolBead,
        EnemyProjectileFrameId.MotherBrainDroolHanging,
        EnemyProjectileFrameId.MotherBrainDroolStretching,
        EnemyProjectileFrameId.MotherBrainDroolStrand,
        EnemyProjectileFrameId.MotherBrainDroolDrop,
        EnemyProjectileFrameId.MotherBrainDroolSplashRising,
        EnemyProjectileFrameId.MotherBrainDroolSplashScattered,
        EnemyProjectileFrameId.MotherBrainDroolSplashFlat,
        EnemyProjectileFrameId.MotherBrainDroolSplashFaint,
        EnemyProjectileFrameId.MotherBrainEscapeDoorDebrisEdgeTurned0,
        EnemyProjectileFrameId.MotherBrainEscapeDoorDebrisFaceTurned0,
        EnemyProjectileFrameId.MotherBrainEscapeDoorDebrisEdgeTurned90,
        EnemyProjectileFrameId.MotherBrainEscapeDoorDebrisFaceTurned90,
        EnemyProjectileFrameId.MotherBrainEscapeDoorDebrisEdgeTurned180,
        EnemyProjectileFrameId.MotherBrainEscapeDoorDebrisFaceTurned180,
        EnemyProjectileFrameId.MotherBrainEscapeDoorDebrisEdgeTurned270,
        EnemyProjectileFrameId.MotherBrainEscapeDoorDebrisFaceTurned270,
        EnemyProjectileFrameId.MotherBrainBreathLargePuff,
        EnemyProjectileFrameId.MotherBrainBreathLargePlume,
        EnemyProjectileFrameId.MotherBrainBreathLargePlumeLong,
        EnemyProjectileFrameId.MotherBrainBreathLargePlumeFull,
        EnemyProjectileFrameId.MotherBrainBreathLargeDispersing,
        EnemyProjectileFrameId.MotherBrainBreathLargeThinning,
        EnemyProjectileFrameId.MotherBrainBreathLargeFading,
        EnemyProjectileFrameId.MotherBrainBreathLargeFaint,
        EnemyProjectileFrameId.MotherBrainBreathSmallPuff,
        EnemyProjectileFrameId.MotherBrainBreathSmallPuffGrowing,
        EnemyProjectileFrameId.MotherBrainBreathSmallCloud,
        EnemyProjectileFrameId.MotherBrainBreathSmallCloudLong,
        EnemyProjectileFrameId.MotherBrainBreathSmallDispersing,
        EnemyProjectileFrameId.MotherBrainBreathSmallThinning,
        EnemyProjectileFrameId.MotherBrainBreathSmallFading,
        EnemyProjectileFrameId.MotherBrainBreathSmallFaint,
        EnemyProjectileFrameId.MotherBrainEscapeSubtitle,
        EnemyProjectileFrameId.MotherBrainTopTubeTopRight,
        EnemyProjectileFrameId.MotherBrainTopTubeTopLeft,
        EnemyProjectileFrameId.MotherBrainTopTubeTopMiddleLeft,
        EnemyProjectileFrameId.MotherBrainTopTubeTopMiddleRight,
        EnemyProjectileFrameId.MotherBrainGlassShardUpTurned0,
        EnemyProjectileFrameId.MotherBrainGlassShardUpTurned45,
        EnemyProjectileFrameId.MotherBrainGlassShardUpTurned90,
        EnemyProjectileFrameId.MotherBrainGlassShardUpTurned135,
        EnemyProjectileFrameId.MotherBrainGlassShardUpTurned180,
        EnemyProjectileFrameId.MotherBrainGlassShardUpTurned225,
        EnemyProjectileFrameId.MotherBrainGlassShardUpTurned270,
        EnemyProjectileFrameId.MotherBrainGlassShardUpTurned315,
        EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned0,
        EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned45,
        EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned90,
        EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned135,
        EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned180,
        EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned225,
        EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned270,
        EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned315,
        EnemyProjectileFrameId.MotherBrainGlassShardRightTurned0,
        EnemyProjectileFrameId.MotherBrainGlassShardRightTurned45,
        EnemyProjectileFrameId.MotherBrainGlassShardRightTurned90,
        EnemyProjectileFrameId.MotherBrainGlassShardRightTurned135,
        EnemyProjectileFrameId.MotherBrainGlassShardRightTurned180,
        EnemyProjectileFrameId.MotherBrainGlassShardRightTurned225,
        EnemyProjectileFrameId.MotherBrainGlassShardRightTurned270,
        EnemyProjectileFrameId.MotherBrainGlassShardRightTurned315,
        EnemyProjectileFrameId.MotherBrainGlassShardDownRightTurned0,
        EnemyProjectileFrameId.MotherBrainGlassShardDownRightTurned45,
        EnemyProjectileFrameId.MotherBrainGlassShardDownRightTurned90,
        EnemyProjectileFrameId.MotherBrainGlassShardDownRightTurned135,
        EnemyProjectileFrameId.MotherBrainGlassShardDownRightTurned180,
        EnemyProjectileFrameId.MotherBrainGlassShardDownRightTurned225,
        EnemyProjectileFrameId.MotherBrainGlassShardDownRightTurned270,
        EnemyProjectileFrameId.MotherBrainGlassShardDownRightTurned315,
        EnemyProjectileFrameId.MotherBrainGlassShardDownTurned0,
        EnemyProjectileFrameId.MotherBrainGlassShardDownTurned45,
        EnemyProjectileFrameId.MotherBrainGlassShardDownTurned90,
        EnemyProjectileFrameId.MotherBrainGlassShardDownTurned135,
        EnemyProjectileFrameId.MotherBrainGlassShardDownTurned180,
        EnemyProjectileFrameId.MotherBrainGlassShardDownTurned225,
        EnemyProjectileFrameId.MotherBrainGlassShardDownTurned270,
        EnemyProjectileFrameId.MotherBrainGlassShardDownTurned315,
        EnemyProjectileFrameId.MotherBrainGlassShardDownLeftTurned0,
        EnemyProjectileFrameId.MotherBrainGlassShardDownLeftTurned45,
        EnemyProjectileFrameId.MotherBrainGlassShardDownLeftTurned90,
        EnemyProjectileFrameId.MotherBrainGlassShardDownLeftTurned135,
        EnemyProjectileFrameId.MotherBrainGlassShardDownLeftTurned180,
        EnemyProjectileFrameId.MotherBrainGlassShardDownLeftTurned225,
        EnemyProjectileFrameId.MotherBrainGlassShardDownLeftTurned270,
        EnemyProjectileFrameId.MotherBrainGlassShardDownLeftTurned315,
        EnemyProjectileFrameId.MotherBrainGlassShardLeftTurned0,
        EnemyProjectileFrameId.MotherBrainGlassShardLeftTurned45,
        EnemyProjectileFrameId.MotherBrainGlassShardLeftTurned90,
        EnemyProjectileFrameId.MotherBrainGlassShardLeftTurned135,
        EnemyProjectileFrameId.MotherBrainGlassShardLeftTurned180,
        EnemyProjectileFrameId.MotherBrainGlassShardLeftTurned225,
        EnemyProjectileFrameId.MotherBrainGlassShardLeftTurned270,
        EnemyProjectileFrameId.MotherBrainGlassShardLeftTurned315,
        EnemyProjectileFrameId.MotherBrainGlassShardUpLeftTurned0,
        EnemyProjectileFrameId.MotherBrainGlassShardUpLeftTurned45,
        EnemyProjectileFrameId.MotherBrainGlassShardUpLeftTurned90,
        EnemyProjectileFrameId.MotherBrainGlassShardUpLeftTurned135,
        EnemyProjectileFrameId.MotherBrainGlassShardUpLeftTurned180,
        EnemyProjectileFrameId.MotherBrainGlassShardUpLeftTurned225,
        EnemyProjectileFrameId.MotherBrainGlassShardUpLeftTurned270,
        EnemyProjectileFrameId.MotherBrainGlassShardUpLeftTurned315,
        EnemyProjectileFrameId.MotherBrainGlassSparkleCross,
        EnemyProjectileFrameId.MotherBrainGlassSparkleCrossSmall,
        EnemyProjectileFrameId.MotherBrainGlassSparkleDiagonal,
        EnemyProjectileFrameId.MotherBrainGlassSparkleDiagonalFaint,
        EnemyProjectileFrameId.DustExplosionSparksGathering,
        EnemyProjectileFrameId.DustExplosionSparksDrawingIn,
        EnemyProjectileFrameId.DustExplosionSparksClosingIn,
        EnemyProjectileFrameId.DustExplosionSparksConverging,
        EnemyProjectileFrameId.DustExplosionSparksNearing,
        EnemyProjectileFrameId.DustExplosionSparksMeeting,
        EnemyProjectileFrameId.DustExplosionSparksTwistedGathering,
        EnemyProjectileFrameId.DustExplosionSparksTwistedDrawingIn,
        EnemyProjectileFrameId.DustExplosionSparksTwistedClosingIn,
        EnemyProjectileFrameId.DustExplosionSparksTwistedConverging,
        EnemyProjectileFrameId.DustExplosionSparksTwistedNearing,
        EnemyProjectileFrameId.DustExplosionSparksTwistedMeeting,
        EnemyProjectileFrameId.DustExplosionSpark,
        EnemyProjectileFrameId.DustExplosionFireballSmall,
        EnemyProjectileFrameId.DustExplosionFireballLarge,
        EnemyProjectileFrameId.DustExplosionFlamingRing,
        EnemyProjectileFrameId.DustExplosionStarburst,
        EnemyProjectileFrameId.DustExplosionEmbers,
        EnemyProjectileFrameId.DustExplosionGreenOrbSmall,
        EnemyProjectileFrameId.DustExplosionGreenOrb,
        EnemyProjectileFrameId.DustExplosionGreenRingCored,
        EnemyProjectileFrameId.DustExplosionGreenRingThick,
        EnemyProjectileFrameId.DustExplosionGreenRing,
        EnemyProjectileFrameId.DustExplosionFlareGlint,
        EnemyProjectileFrameId.DustExplosionFlareGlow,
        EnemyProjectileFrameId.DustExplosionFlareBall,
        EnemyProjectileFrameId.DustExplosionFlareRingCored,
        EnemyProjectileFrameId.DustExplosionFlareRing,
        EnemyProjectileFrameId.DustExplosionFlareRingFading,
        EnemyProjectileFrameId.DustExplosionPuff,
        EnemyProjectileFrameId.DustExplosionCloud,
        EnemyProjectileFrameId.DustExplosionCloudThinning,
        EnemyProjectileFrameId.DustExplosionCloudThin,
        EnemyProjectileFrameId.DustExplosionCloudFaint,
        EnemyProjectileFrameId.DustExplosionPuffSmall,
        EnemyProjectileFrameId.DustExplosionPuffThin,
        EnemyProjectileFrameId.DustExplosionPuffFaint,
        EnemyProjectileFrameId.DustExplosionSmokePuffLow,
        EnemyProjectileFrameId.DustExplosionSmokePuffRising,
        EnemyProjectileFrameId.DustExplosionSmokePuffScattering,
        EnemyProjectileFrameId.DustExplosionSmokePuffFading,
        EnemyProjectileFrameId.DustExplosionFireCloud,
        EnemyProjectileFrameId.DustExplosionFireCloudSpreading,
        EnemyProjectileFrameId.DustExplosionFireCloudBreaking,
        EnemyProjectileFrameId.DustExplosionFireCloudEmbers,
        EnemyProjectileFrameId.DustExplosionFireballHuge,
        EnemyProjectileFrameId.DustExplosionWideRing,
        EnemyProjectileFrameId.DustExplosionWideRingFading,
        EnemyProjectileFrameId.DustExplosionPebblesBursting,
        EnemyProjectileFrameId.DustExplosionPebblesFlyingOut,
        EnemyProjectileFrameId.DustExplosionPebblesFlyingApart,
        EnemyProjectileFrameId.DustExplosionPebblesClimbing,
        EnemyProjectileFrameId.DustExplosionPebblesSlowing,
        EnemyProjectileFrameId.DustExplosionPebblesPeaking,
        EnemyProjectileFrameId.DustExplosionPebblesTurningOver,
        EnemyProjectileFrameId.DustExplosionPebblesStartingDown,
        EnemyProjectileFrameId.DustExplosionPebblesDropping,
        EnemyProjectileFrameId.DustExplosionPebblesFalling,
        EnemyProjectileFrameId.DustExplosionPebblesTurningAshen,
        EnemyProjectileFrameId.DustExplosionPebblesTumbling,
        EnemyProjectileFrameId.DustExplosionPebblesPlunging,
        EnemyProjectileFrameId.DustExplosionPebblesPlummeting,
        EnemyProjectileFrameId.DustExplosionPebblesScattering,
        EnemyProjectileFrameId.DustExplosionPebblesFallingAway,
        EnemyProjectileFrameId.DustExplosionPuffUpLeft,
        EnemyProjectileFrameId.DustExplosionPuffPair,
        EnemyProjectileFrameId.DustExplosionPuffTrio,
        EnemyProjectileFrameId.DustExplosionCloudsDense,
        EnemyProjectileFrameId.DustExplosionCloudsThinning,
        EnemyProjectileFrameId.DustExplosionCloudsThin,
        EnemyProjectileFrameId.DustExplosionCloudsFaint,
        EnemyProjectileFrameId.DustExplosionCloudFaintRight,
        EnemyProjectileFrameId.DustExplosionBubbleSmall,
        EnemyProjectileFrameId.DustExplosionBubbles,
        EnemyProjectileFrameId.DustExplosionBubblesRising,
        EnemyProjectileFrameId.DustExplosionBubblesSpreading,
        EnemyProjectileFrameId.DustExplosionBubblesScattered,
        EnemyProjectileFrameId.DustExplosionBubblesSparse,
        EnemyProjectileFrameId.DustExplosionBubblesFew,
        EnemyProjectileFrameId.DustExplosionBubblesLast,
        EnemyProjectileFrameId.DustExplosionSpeck,
        EnemyProjectileFrameId.EnemyDeathFireballSmall,
        EnemyProjectileFrameId.EnemyDeathFireballLarge,
        EnemyProjectileFrameId.EnemyDeathFlamingRing,
        EnemyProjectileFrameId.EnemyDeathFireballHuge,
        EnemyProjectileFrameId.EnemyDeathWideRing,
        EnemyProjectileFrameId.EnemyDeathWideRingFading,
        EnemyProjectileFrameId.EnemyDeathSpark,
        EnemyProjectileFrameId.EnemyDeathStarburst,
        EnemyProjectileFrameId.EnemyDeathEmbers,
        EnemyProjectileFrameId.EnemyPickupEnergySmallDark,
        EnemyProjectileFrameId.EnemyPickupEnergySmallRinged,
        EnemyProjectileFrameId.EnemyPickupEnergySmallBright,
        EnemyProjectileFrameId.EnemyPickupEnergyScattered,
        EnemyProjectileFrameId.EnemyPickupEnergyBigBright,
        EnemyProjectileFrameId.EnemyPickupEnergyBigRinged,
        EnemyProjectileFrameId.EnemyPickupEnergyBigDark,
        EnemyProjectileFrameId.EnemyPickupMissile,
        EnemyProjectileFrameId.EnemyPickupMissileFlashing,
        EnemyProjectileFrameId.EnemyPickupSuperMissile,
        EnemyProjectileFrameId.EnemyPickupSuperMissileFlashing,
        EnemyProjectileFrameId.EnemyPickupPowerBombBright,
        EnemyProjectileFrameId.EnemyPickupPowerBombFading,
        EnemyProjectileFrameId.EnemyPickupPowerBombDim,
        EnemyProjectileFrameId.EnemyPickupPowerBombFlashing,
        EnemyProjectileFrameId.EnemyDeathFireballCluster,
        EnemyProjectileFrameId.EnemyDeathStarburstCluster,
        EnemyProjectileFrameId.EnemyDeathEmbersCluster,
        EnemyProjectileFrameId.EnemyDeathFourFireballsAmidEmbers,
        EnemyProjectileFrameId.EnemyDeathFourFireballsAmidAsh,
        EnemyProjectileFrameId.EnemyDeathFourEmbersFlyingApart,
        EnemyProjectileFrameId.EnemyDeathFourEmbersSpreading,
        EnemyProjectileFrameId.EnemyDeathFourEmbersCooling,
        EnemyProjectileFrameId.EnemyDeathFourEmbersDimming,
        EnemyProjectileFrameId.EnemyDeathFourAshDrifting,
        EnemyProjectileFrameId.EnemyDeathFourAshSpreading,
        EnemyProjectileFrameId.EnemyDeathFourAshSinking,
        EnemyProjectileFrameId.EnemyDeathFourAshFalling,
        EnemyProjectileFrameId.EnemyDeathFourAshSettling,
        EnemyProjectileFrameId.EnemyDeathFourAshDispersing,
    ];

    /// <summary>The room effect actors <c>Bosses/MotherBrain/MotherBrainBodyFactory.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] MotherBrainBodySharedEffectActors = KraidSharedEffectActors;

    /// <summary>The projectile frames <c>Bosses/MotherBrain/RinkaBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] RinkaSharedProjectiles = BotwoonSharedProjectiles;

    /// <summary>The room effect actors <c>Bosses/MotherBrain/RinkaBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] RinkaSharedEffectActors = CeresSteamSharedEffectActors;

    /// <summary>The projectile frames <c>Bosses/MotherBrain/ZebetiteBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] ZebetiteSharedProjectiles = CeresSteamSharedProjectiles;

    /// <summary>The room effect actors <c>Bosses/MotherBrain/ZebetiteBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] ZebetiteSharedEffectActors = CeresSteamSharedEffectActors;

    /// <summary>The projectile frames <c>Bosses/Phantoon/PhantoonBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] PhantoonSharedProjectiles = BotwoonSharedProjectiles;

    /// <summary>The room effect actors <c>Bosses/Phantoon/PhantoonBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] PhantoonSharedEffectActors = KraidSharedEffectActors;

    /// <summary>The projectile frames <c>Bosses/Ridley/CeresRidleyEntityFactory.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] CeresRidleyEntitySharedProjectiles =
    [
        EnemyProjectileFrameId.RidleyFireballTurned0,
        EnemyProjectileFrameId.RidleyFireballTurned90,
        EnemyProjectileFrameId.RidleyFireballTurned180,
        EnemyProjectileFrameId.RidleyFireballTurned270,
        EnemyProjectileFrameId.RidleyFireballSpark,
        EnemyProjectileFrameId.RidleyFireballFireCloud,
        EnemyProjectileFrameId.RidleyFireballFireCloudSpreading,
        EnemyProjectileFrameId.RidleyFireballFireCloudBreaking,
        EnemyProjectileFrameId.RidleyFireballFireCloudEmbers,
        EnemyProjectileFrameId.EnemyDeathFireballSmall,
        EnemyProjectileFrameId.EnemyDeathFireballLarge,
        EnemyProjectileFrameId.EnemyDeathFlamingRing,
        EnemyProjectileFrameId.EnemyDeathFireballHuge,
        EnemyProjectileFrameId.EnemyDeathWideRing,
        EnemyProjectileFrameId.EnemyDeathWideRingFading,
        EnemyProjectileFrameId.EnemyDeathSpark,
        EnemyProjectileFrameId.EnemyDeathStarburst,
        EnemyProjectileFrameId.EnemyDeathEmbers,
        EnemyProjectileFrameId.EnemyPickupEnergySmallDark,
        EnemyProjectileFrameId.EnemyPickupEnergySmallRinged,
        EnemyProjectileFrameId.EnemyPickupEnergySmallBright,
        EnemyProjectileFrameId.EnemyPickupEnergyScattered,
        EnemyProjectileFrameId.EnemyPickupEnergyBigBright,
        EnemyProjectileFrameId.EnemyPickupEnergyBigRinged,
        EnemyProjectileFrameId.EnemyPickupEnergyBigDark,
        EnemyProjectileFrameId.EnemyPickupMissile,
        EnemyProjectileFrameId.EnemyPickupMissileFlashing,
        EnemyProjectileFrameId.EnemyPickupSuperMissile,
        EnemyProjectileFrameId.EnemyPickupSuperMissileFlashing,
        EnemyProjectileFrameId.EnemyPickupPowerBombBright,
        EnemyProjectileFrameId.EnemyPickupPowerBombFading,
        EnemyProjectileFrameId.EnemyPickupPowerBombDim,
        EnemyProjectileFrameId.EnemyPickupPowerBombFlashing,
        EnemyProjectileFrameId.EnemyDeathFireballCluster,
        EnemyProjectileFrameId.EnemyDeathStarburstCluster,
        EnemyProjectileFrameId.EnemyDeathEmbersCluster,
        EnemyProjectileFrameId.EnemyDeathFourFireballsAmidEmbers,
        EnemyProjectileFrameId.EnemyDeathFourFireballsAmidAsh,
        EnemyProjectileFrameId.EnemyDeathFourEmbersFlyingApart,
        EnemyProjectileFrameId.EnemyDeathFourEmbersSpreading,
        EnemyProjectileFrameId.EnemyDeathFourEmbersCooling,
        EnemyProjectileFrameId.EnemyDeathFourEmbersDimming,
        EnemyProjectileFrameId.EnemyDeathFourAshDrifting,
        EnemyProjectileFrameId.EnemyDeathFourAshSpreading,
        EnemyProjectileFrameId.EnemyDeathFourAshSinking,
        EnemyProjectileFrameId.EnemyDeathFourAshFalling,
        EnemyProjectileFrameId.EnemyDeathFourAshSettling,
        EnemyProjectileFrameId.EnemyDeathFourAshDispersing,
    ];

    /// <summary>The room effect actors <c>Bosses/Ridley/CeresRidleyEntityFactory.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] CeresRidleyEntitySharedEffectActors = CeresSteamSharedEffectActors;

    /// <summary>The projectile frames <c>Bosses/Ridley/RidleyEntityFactory.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] RidleyEntitySharedProjectiles =
    [
        EnemyProjectileFrameId.RidleyFireballTurned0,
        EnemyProjectileFrameId.RidleyFireballTurned90,
        EnemyProjectileFrameId.RidleyFireballTurned180,
        EnemyProjectileFrameId.RidleyFireballTurned270,
        EnemyProjectileFrameId.RidleyFireballSpark,
        EnemyProjectileFrameId.RidleyFireballFireCloud,
        EnemyProjectileFrameId.RidleyFireballFireCloudSpreading,
        EnemyProjectileFrameId.RidleyFireballFireCloudBreaking,
        EnemyProjectileFrameId.RidleyFireballFireCloudEmbers,
        EnemyProjectileFrameId.DustExplosionSparksGathering,
        EnemyProjectileFrameId.DustExplosionSparksDrawingIn,
        EnemyProjectileFrameId.DustExplosionSparksClosingIn,
        EnemyProjectileFrameId.DustExplosionSparksConverging,
        EnemyProjectileFrameId.DustExplosionSparksNearing,
        EnemyProjectileFrameId.DustExplosionSparksMeeting,
        EnemyProjectileFrameId.DustExplosionSparksTwistedGathering,
        EnemyProjectileFrameId.DustExplosionSparksTwistedDrawingIn,
        EnemyProjectileFrameId.DustExplosionSparksTwistedClosingIn,
        EnemyProjectileFrameId.DustExplosionSparksTwistedConverging,
        EnemyProjectileFrameId.DustExplosionSparksTwistedNearing,
        EnemyProjectileFrameId.DustExplosionSparksTwistedMeeting,
        EnemyProjectileFrameId.DustExplosionSpark,
        EnemyProjectileFrameId.DustExplosionFireballSmall,
        EnemyProjectileFrameId.DustExplosionFireballLarge,
        EnemyProjectileFrameId.DustExplosionFlamingRing,
        EnemyProjectileFrameId.DustExplosionStarburst,
        EnemyProjectileFrameId.DustExplosionEmbers,
        EnemyProjectileFrameId.DustExplosionGreenOrbSmall,
        EnemyProjectileFrameId.DustExplosionGreenOrb,
        EnemyProjectileFrameId.DustExplosionGreenRingCored,
        EnemyProjectileFrameId.DustExplosionGreenRingThick,
        EnemyProjectileFrameId.DustExplosionGreenRing,
        EnemyProjectileFrameId.DustExplosionFlareGlint,
        EnemyProjectileFrameId.DustExplosionFlareGlow,
        EnemyProjectileFrameId.DustExplosionFlareBall,
        EnemyProjectileFrameId.DustExplosionFlareRingCored,
        EnemyProjectileFrameId.DustExplosionFlareRing,
        EnemyProjectileFrameId.DustExplosionFlareRingFading,
        EnemyProjectileFrameId.DustExplosionPuff,
        EnemyProjectileFrameId.DustExplosionCloud,
        EnemyProjectileFrameId.DustExplosionCloudThinning,
        EnemyProjectileFrameId.DustExplosionCloudThin,
        EnemyProjectileFrameId.DustExplosionCloudFaint,
        EnemyProjectileFrameId.DustExplosionPuffSmall,
        EnemyProjectileFrameId.DustExplosionPuffThin,
        EnemyProjectileFrameId.DustExplosionPuffFaint,
        EnemyProjectileFrameId.DustExplosionSmokePuffLow,
        EnemyProjectileFrameId.DustExplosionSmokePuffRising,
        EnemyProjectileFrameId.DustExplosionSmokePuffScattering,
        EnemyProjectileFrameId.DustExplosionSmokePuffFading,
        EnemyProjectileFrameId.DustExplosionFireCloud,
        EnemyProjectileFrameId.DustExplosionFireCloudSpreading,
        EnemyProjectileFrameId.DustExplosionFireCloudBreaking,
        EnemyProjectileFrameId.DustExplosionFireCloudEmbers,
        EnemyProjectileFrameId.DustExplosionFireballHuge,
        EnemyProjectileFrameId.DustExplosionWideRing,
        EnemyProjectileFrameId.DustExplosionWideRingFading,
        EnemyProjectileFrameId.DustExplosionPebblesBursting,
        EnemyProjectileFrameId.DustExplosionPebblesFlyingOut,
        EnemyProjectileFrameId.DustExplosionPebblesFlyingApart,
        EnemyProjectileFrameId.DustExplosionPebblesClimbing,
        EnemyProjectileFrameId.DustExplosionPebblesSlowing,
        EnemyProjectileFrameId.DustExplosionPebblesPeaking,
        EnemyProjectileFrameId.DustExplosionPebblesTurningOver,
        EnemyProjectileFrameId.DustExplosionPebblesStartingDown,
        EnemyProjectileFrameId.DustExplosionPebblesDropping,
        EnemyProjectileFrameId.DustExplosionPebblesFalling,
        EnemyProjectileFrameId.DustExplosionPebblesTurningAshen,
        EnemyProjectileFrameId.DustExplosionPebblesTumbling,
        EnemyProjectileFrameId.DustExplosionPebblesPlunging,
        EnemyProjectileFrameId.DustExplosionPebblesPlummeting,
        EnemyProjectileFrameId.DustExplosionPebblesScattering,
        EnemyProjectileFrameId.DustExplosionPebblesFallingAway,
        EnemyProjectileFrameId.DustExplosionPuffUpLeft,
        EnemyProjectileFrameId.DustExplosionPuffPair,
        EnemyProjectileFrameId.DustExplosionPuffTrio,
        EnemyProjectileFrameId.DustExplosionCloudsDense,
        EnemyProjectileFrameId.DustExplosionCloudsThinning,
        EnemyProjectileFrameId.DustExplosionCloudsThin,
        EnemyProjectileFrameId.DustExplosionCloudsFaint,
        EnemyProjectileFrameId.DustExplosionCloudFaintRight,
        EnemyProjectileFrameId.DustExplosionBubbleSmall,
        EnemyProjectileFrameId.DustExplosionBubbles,
        EnemyProjectileFrameId.DustExplosionBubblesRising,
        EnemyProjectileFrameId.DustExplosionBubblesSpreading,
        EnemyProjectileFrameId.DustExplosionBubblesScattered,
        EnemyProjectileFrameId.DustExplosionBubblesSparse,
        EnemyProjectileFrameId.DustExplosionBubblesFew,
        EnemyProjectileFrameId.DustExplosionBubblesLast,
        EnemyProjectileFrameId.DustExplosionSpeck,
        EnemyProjectileFrameId.EnemyDeathFireballSmall,
        EnemyProjectileFrameId.EnemyDeathFireballLarge,
        EnemyProjectileFrameId.EnemyDeathFlamingRing,
        EnemyProjectileFrameId.EnemyDeathFireballHuge,
        EnemyProjectileFrameId.EnemyDeathWideRing,
        EnemyProjectileFrameId.EnemyDeathWideRingFading,
        EnemyProjectileFrameId.EnemyDeathSpark,
        EnemyProjectileFrameId.EnemyDeathStarburst,
        EnemyProjectileFrameId.EnemyDeathEmbers,
        EnemyProjectileFrameId.EnemyPickupEnergySmallDark,
        EnemyProjectileFrameId.EnemyPickupEnergySmallRinged,
        EnemyProjectileFrameId.EnemyPickupEnergySmallBright,
        EnemyProjectileFrameId.EnemyPickupEnergyScattered,
        EnemyProjectileFrameId.EnemyPickupEnergyBigBright,
        EnemyProjectileFrameId.EnemyPickupEnergyBigRinged,
        EnemyProjectileFrameId.EnemyPickupEnergyBigDark,
        EnemyProjectileFrameId.EnemyPickupMissile,
        EnemyProjectileFrameId.EnemyPickupMissileFlashing,
        EnemyProjectileFrameId.EnemyPickupSuperMissile,
        EnemyProjectileFrameId.EnemyPickupSuperMissileFlashing,
        EnemyProjectileFrameId.EnemyPickupPowerBombBright,
        EnemyProjectileFrameId.EnemyPickupPowerBombFading,
        EnemyProjectileFrameId.EnemyPickupPowerBombDim,
        EnemyProjectileFrameId.EnemyPickupPowerBombFlashing,
        EnemyProjectileFrameId.EnemyDeathFireballCluster,
        EnemyProjectileFrameId.EnemyDeathStarburstCluster,
        EnemyProjectileFrameId.EnemyDeathEmbersCluster,
        EnemyProjectileFrameId.EnemyDeathFourFireballsAmidEmbers,
        EnemyProjectileFrameId.EnemyDeathFourFireballsAmidAsh,
        EnemyProjectileFrameId.EnemyDeathFourEmbersFlyingApart,
        EnemyProjectileFrameId.EnemyDeathFourEmbersSpreading,
        EnemyProjectileFrameId.EnemyDeathFourEmbersCooling,
        EnemyProjectileFrameId.EnemyDeathFourEmbersDimming,
        EnemyProjectileFrameId.EnemyDeathFourAshDrifting,
        EnemyProjectileFrameId.EnemyDeathFourAshSpreading,
        EnemyProjectileFrameId.EnemyDeathFourAshSinking,
        EnemyProjectileFrameId.EnemyDeathFourAshFalling,
        EnemyProjectileFrameId.EnemyDeathFourAshSettling,
        EnemyProjectileFrameId.EnemyDeathFourAshDispersing,
    ];

    /// <summary>The room effect actors <c>Bosses/Ridley/RidleyEntityFactory.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] RidleyEntitySharedEffectActors = CeresSteamSharedEffectActors;

    /// <summary>The projectile frames <c>Bosses/SporeSpawn/SporeSpawnBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] SporeSpawnSharedProjectiles =
    [
        EnemyProjectileFrameId.SporeSpawnSporeGlowing,
        EnemyProjectileFrameId.SporeSpawnSporeDim,
        EnemyProjectileFrameId.SporeSpawnSporeSparkling,
        EnemyProjectileFrameId.SporeSpawnSporeBurstSpark,
        EnemyProjectileFrameId.SporeSpawnSporeBurstFireballSmall,
        EnemyProjectileFrameId.SporeSpawnSporeBurstFireballLarge,
        EnemyProjectileFrameId.SporeSpawnSporeBurstFlamingRing,
        EnemyProjectileFrameId.SporeSpawnSporeBurstStarburst,
        EnemyProjectileFrameId.SporeSpawnSporeBurstEmbers,
        EnemyProjectileFrameId.DustExplosionSparksGathering,
        EnemyProjectileFrameId.DustExplosionSparksDrawingIn,
        EnemyProjectileFrameId.DustExplosionSparksClosingIn,
        EnemyProjectileFrameId.DustExplosionSparksConverging,
        EnemyProjectileFrameId.DustExplosionSparksNearing,
        EnemyProjectileFrameId.DustExplosionSparksMeeting,
        EnemyProjectileFrameId.DustExplosionSparksTwistedGathering,
        EnemyProjectileFrameId.DustExplosionSparksTwistedDrawingIn,
        EnemyProjectileFrameId.DustExplosionSparksTwistedClosingIn,
        EnemyProjectileFrameId.DustExplosionSparksTwistedConverging,
        EnemyProjectileFrameId.DustExplosionSparksTwistedNearing,
        EnemyProjectileFrameId.DustExplosionSparksTwistedMeeting,
        EnemyProjectileFrameId.DustExplosionSpark,
        EnemyProjectileFrameId.DustExplosionFireballSmall,
        EnemyProjectileFrameId.DustExplosionFireballLarge,
        EnemyProjectileFrameId.DustExplosionFlamingRing,
        EnemyProjectileFrameId.DustExplosionStarburst,
        EnemyProjectileFrameId.DustExplosionEmbers,
        EnemyProjectileFrameId.DustExplosionGreenOrbSmall,
        EnemyProjectileFrameId.DustExplosionGreenOrb,
        EnemyProjectileFrameId.DustExplosionGreenRingCored,
        EnemyProjectileFrameId.DustExplosionGreenRingThick,
        EnemyProjectileFrameId.DustExplosionGreenRing,
        EnemyProjectileFrameId.DustExplosionFlareGlint,
        EnemyProjectileFrameId.DustExplosionFlareGlow,
        EnemyProjectileFrameId.DustExplosionFlareBall,
        EnemyProjectileFrameId.DustExplosionFlareRingCored,
        EnemyProjectileFrameId.DustExplosionFlareRing,
        EnemyProjectileFrameId.DustExplosionFlareRingFading,
        EnemyProjectileFrameId.DustExplosionPuff,
        EnemyProjectileFrameId.DustExplosionCloud,
        EnemyProjectileFrameId.DustExplosionCloudThinning,
        EnemyProjectileFrameId.DustExplosionCloudThin,
        EnemyProjectileFrameId.DustExplosionCloudFaint,
        EnemyProjectileFrameId.DustExplosionPuffSmall,
        EnemyProjectileFrameId.DustExplosionPuffThin,
        EnemyProjectileFrameId.DustExplosionPuffFaint,
        EnemyProjectileFrameId.DustExplosionSmokePuffLow,
        EnemyProjectileFrameId.DustExplosionSmokePuffRising,
        EnemyProjectileFrameId.DustExplosionSmokePuffScattering,
        EnemyProjectileFrameId.DustExplosionSmokePuffFading,
        EnemyProjectileFrameId.DustExplosionFireCloud,
        EnemyProjectileFrameId.DustExplosionFireCloudSpreading,
        EnemyProjectileFrameId.DustExplosionFireCloudBreaking,
        EnemyProjectileFrameId.DustExplosionFireCloudEmbers,
        EnemyProjectileFrameId.DustExplosionFireballHuge,
        EnemyProjectileFrameId.DustExplosionWideRing,
        EnemyProjectileFrameId.DustExplosionWideRingFading,
        EnemyProjectileFrameId.DustExplosionPebblesBursting,
        EnemyProjectileFrameId.DustExplosionPebblesFlyingOut,
        EnemyProjectileFrameId.DustExplosionPebblesFlyingApart,
        EnemyProjectileFrameId.DustExplosionPebblesClimbing,
        EnemyProjectileFrameId.DustExplosionPebblesSlowing,
        EnemyProjectileFrameId.DustExplosionPebblesPeaking,
        EnemyProjectileFrameId.DustExplosionPebblesTurningOver,
        EnemyProjectileFrameId.DustExplosionPebblesStartingDown,
        EnemyProjectileFrameId.DustExplosionPebblesDropping,
        EnemyProjectileFrameId.DustExplosionPebblesFalling,
        EnemyProjectileFrameId.DustExplosionPebblesTurningAshen,
        EnemyProjectileFrameId.DustExplosionPebblesTumbling,
        EnemyProjectileFrameId.DustExplosionPebblesPlunging,
        EnemyProjectileFrameId.DustExplosionPebblesPlummeting,
        EnemyProjectileFrameId.DustExplosionPebblesScattering,
        EnemyProjectileFrameId.DustExplosionPebblesFallingAway,
        EnemyProjectileFrameId.DustExplosionPuffUpLeft,
        EnemyProjectileFrameId.DustExplosionPuffPair,
        EnemyProjectileFrameId.DustExplosionPuffTrio,
        EnemyProjectileFrameId.DustExplosionCloudsDense,
        EnemyProjectileFrameId.DustExplosionCloudsThinning,
        EnemyProjectileFrameId.DustExplosionCloudsThin,
        EnemyProjectileFrameId.DustExplosionCloudsFaint,
        EnemyProjectileFrameId.DustExplosionCloudFaintRight,
        EnemyProjectileFrameId.DustExplosionBubbleSmall,
        EnemyProjectileFrameId.DustExplosionBubbles,
        EnemyProjectileFrameId.DustExplosionBubblesRising,
        EnemyProjectileFrameId.DustExplosionBubblesSpreading,
        EnemyProjectileFrameId.DustExplosionBubblesScattered,
        EnemyProjectileFrameId.DustExplosionBubblesSparse,
        EnemyProjectileFrameId.DustExplosionBubblesFew,
        EnemyProjectileFrameId.DustExplosionBubblesLast,
        EnemyProjectileFrameId.DustExplosionSpeck,
        EnemyProjectileFrameId.EnemyDeathFireballSmall,
        EnemyProjectileFrameId.EnemyDeathFireballLarge,
        EnemyProjectileFrameId.EnemyDeathFlamingRing,
        EnemyProjectileFrameId.EnemyDeathFireballHuge,
        EnemyProjectileFrameId.EnemyDeathWideRing,
        EnemyProjectileFrameId.EnemyDeathWideRingFading,
        EnemyProjectileFrameId.EnemyDeathSpark,
        EnemyProjectileFrameId.EnemyDeathStarburst,
        EnemyProjectileFrameId.EnemyDeathEmbers,
        EnemyProjectileFrameId.EnemyPickupEnergySmallDark,
        EnemyProjectileFrameId.EnemyPickupEnergySmallRinged,
        EnemyProjectileFrameId.EnemyPickupEnergySmallBright,
        EnemyProjectileFrameId.EnemyPickupEnergyScattered,
        EnemyProjectileFrameId.EnemyPickupEnergyBigBright,
        EnemyProjectileFrameId.EnemyPickupEnergyBigRinged,
        EnemyProjectileFrameId.EnemyPickupEnergyBigDark,
        EnemyProjectileFrameId.EnemyPickupMissile,
        EnemyProjectileFrameId.EnemyPickupMissileFlashing,
        EnemyProjectileFrameId.EnemyPickupSuperMissile,
        EnemyProjectileFrameId.EnemyPickupSuperMissileFlashing,
        EnemyProjectileFrameId.EnemyPickupPowerBombBright,
        EnemyProjectileFrameId.EnemyPickupPowerBombFading,
        EnemyProjectileFrameId.EnemyPickupPowerBombDim,
        EnemyProjectileFrameId.EnemyPickupPowerBombFlashing,
        EnemyProjectileFrameId.EnemyDeathFireballCluster,
        EnemyProjectileFrameId.EnemyDeathStarburstCluster,
        EnemyProjectileFrameId.EnemyDeathEmbersCluster,
        EnemyProjectileFrameId.EnemyDeathFourFireballsAmidEmbers,
        EnemyProjectileFrameId.EnemyDeathFourFireballsAmidAsh,
        EnemyProjectileFrameId.EnemyDeathFourEmbersFlyingApart,
        EnemyProjectileFrameId.EnemyDeathFourEmbersSpreading,
        EnemyProjectileFrameId.EnemyDeathFourEmbersCooling,
        EnemyProjectileFrameId.EnemyDeathFourEmbersDimming,
        EnemyProjectileFrameId.EnemyDeathFourAshDrifting,
        EnemyProjectileFrameId.EnemyDeathFourAshSpreading,
        EnemyProjectileFrameId.EnemyDeathFourAshSinking,
        EnemyProjectileFrameId.EnemyDeathFourAshFalling,
        EnemyProjectileFrameId.EnemyDeathFourAshSettling,
        EnemyProjectileFrameId.EnemyDeathFourAshDispersing,
    ];

    /// <summary>The room effect actors <c>Bosses/SporeSpawn/SporeSpawnBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] SporeSpawnSharedEffectActors = KraidSharedEffectActors;

    /// <summary>The projectile frames <c>Bosses/Torizo/TorizoBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] TorizoSharedProjectiles =
    [
        EnemyProjectileFrameId.TorizoExplosionSpark,
        EnemyProjectileFrameId.TorizoExplosionFlareBall,
        EnemyProjectileFrameId.TorizoExplosionFlareRingCored,
        EnemyProjectileFrameId.TorizoExplosionFlareRing,
        EnemyProjectileFrameId.TorizoExplosionFlareRingFading,
        EnemyProjectileFrameId.TorizoExplosionFireballSmall,
        EnemyProjectileFrameId.TorizoExplosionFireballLarge,
        EnemyProjectileFrameId.TorizoExplosionFlamingRing,
        EnemyProjectileFrameId.TorizoExplosionStarburst,
        EnemyProjectileFrameId.TorizoExplosionEmbers,
        EnemyProjectileFrameId.TorizoExplosionFireCloud,
        EnemyProjectileFrameId.TorizoExplosionFireCloudSpreading,
        EnemyProjectileFrameId.TorizoExplosionFireCloudBreaking,
        EnemyProjectileFrameId.TorizoExplosionFireCloudEmbers,
        EnemyProjectileFrameId.TorizoSwipeSpark,
        EnemyProjectileFrameId.TorizoSwipeFlareBall,
        EnemyProjectileFrameId.TorizoSwipeFlareRingCored,
        EnemyProjectileFrameId.TorizoSwipeFlareRing,
        EnemyProjectileFrameId.TorizoSwipeFlareRingFading,
        EnemyProjectileFrameId.BombTorizoStatueMossClump,
        EnemyProjectileFrameId.BombTorizoStatueRubbleLitRight,
        EnemyProjectileFrameId.BombTorizoStatueRubbleLitCenter,
        EnemyProjectileFrameId.BombTorizoStatueRubbleLitSpot,
        EnemyProjectileFrameId.BombTorizoStatueRubbleMossyTop,
        EnemyProjectileFrameId.BombTorizoStatueRubbleMossSpeck,
        EnemyProjectileFrameId.BombTorizoStatueRubbleLitBase,
        EnemyProjectileFrameId.BombTorizoStatueSlab,
        EnemyProjectileFrameId.TorizoLandingDustRightDustPuff,
        EnemyProjectileFrameId.TorizoLandingDustRightDustPuffSmall,
        EnemyProjectileFrameId.TorizoLandingDustRightDustPuffThin,
        EnemyProjectileFrameId.TorizoLandingDustRightDustPuffFaint,
        EnemyProjectileFrameId.TorizoLandingDustLeftDustPuff,
        EnemyProjectileFrameId.TorizoLandingDustLeftDustPuffSmall,
        EnemyProjectileFrameId.TorizoLandingDustLeftDustPuffThin,
        EnemyProjectileFrameId.TorizoLandingDustLeftDustPuffFaint,
        EnemyProjectileFrameId.EnemyDeathFireballSmall,
        EnemyProjectileFrameId.EnemyDeathFireballLarge,
        EnemyProjectileFrameId.EnemyDeathFlamingRing,
        EnemyProjectileFrameId.EnemyDeathFireballHuge,
        EnemyProjectileFrameId.EnemyDeathWideRing,
        EnemyProjectileFrameId.EnemyDeathWideRingFading,
        EnemyProjectileFrameId.EnemyDeathSpark,
        EnemyProjectileFrameId.EnemyDeathStarburst,
        EnemyProjectileFrameId.EnemyDeathEmbers,
        EnemyProjectileFrameId.EnemyPickupEnergySmallDark,
        EnemyProjectileFrameId.EnemyPickupEnergySmallRinged,
        EnemyProjectileFrameId.EnemyPickupEnergySmallBright,
        EnemyProjectileFrameId.EnemyPickupEnergyScattered,
        EnemyProjectileFrameId.EnemyPickupEnergyBigBright,
        EnemyProjectileFrameId.EnemyPickupEnergyBigRinged,
        EnemyProjectileFrameId.EnemyPickupEnergyBigDark,
        EnemyProjectileFrameId.EnemyPickupMissile,
        EnemyProjectileFrameId.EnemyPickupMissileFlashing,
        EnemyProjectileFrameId.EnemyPickupSuperMissile,
        EnemyProjectileFrameId.EnemyPickupSuperMissileFlashing,
        EnemyProjectileFrameId.EnemyPickupPowerBombBright,
        EnemyProjectileFrameId.EnemyPickupPowerBombFading,
        EnemyProjectileFrameId.EnemyPickupPowerBombDim,
        EnemyProjectileFrameId.EnemyPickupPowerBombFlashing,
        EnemyProjectileFrameId.EnemyDeathFireballCluster,
        EnemyProjectileFrameId.EnemyDeathStarburstCluster,
        EnemyProjectileFrameId.EnemyDeathEmbersCluster,
        EnemyProjectileFrameId.EnemyDeathFourFireballsAmidEmbers,
        EnemyProjectileFrameId.EnemyDeathFourFireballsAmidAsh,
        EnemyProjectileFrameId.EnemyDeathFourEmbersFlyingApart,
        EnemyProjectileFrameId.EnemyDeathFourEmbersSpreading,
        EnemyProjectileFrameId.EnemyDeathFourEmbersCooling,
        EnemyProjectileFrameId.EnemyDeathFourEmbersDimming,
        EnemyProjectileFrameId.EnemyDeathFourAshDrifting,
        EnemyProjectileFrameId.EnemyDeathFourAshSpreading,
        EnemyProjectileFrameId.EnemyDeathFourAshSinking,
        EnemyProjectileFrameId.EnemyDeathFourAshFalling,
        EnemyProjectileFrameId.EnemyDeathFourAshSettling,
        EnemyProjectileFrameId.EnemyDeathFourAshDispersing,
    ];

    /// <summary>The room effect actors <c>Bosses/Torizo/TorizoBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] TorizoSharedEffectActors = KraidSharedEffectActors;

    /// <summary>The projectile frames <c>Enemies/Beetom/BeetomBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] BeetomSharedProjectiles = CeresSteamSharedProjectiles;

    /// <summary>The room effect actors <c>Enemies/Beetom/BeetomBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] BeetomSharedEffectActors = CeresSteamSharedEffectActors;

    /// <summary>The projectile frames <c>Enemies/Boulder/BoulderBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] BoulderSharedProjectiles = CeresElevatorDoorSharedProjectiles;

    /// <summary>The room effect actors <c>Enemies/Boulder/BoulderBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] BoulderSharedEffectActors = [];

    /// <summary>The projectile frames <c>Enemies/Chozo/ChozoStatueBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] ChozoStatueSharedProjectiles =
    [
        EnemyProjectileFrameId.ChozoDustPuff,
        EnemyProjectileFrameId.ChozoDustPuffPair,
        EnemyProjectileFrameId.ChozoDustPuffTrio,
        EnemyProjectileFrameId.ChozoDustCloudsDense,
    ];

    /// <summary>The room effect actors <c>Enemies/Chozo/ChozoStatueBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] ChozoStatueSharedEffectActors = [];

    /// <summary>The projectile frames <c>Enemies/Corpses/DeadSidehopperBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] DeadSidehopperSharedProjectiles = BotwoonSharedProjectiles;

    /// <summary>The room effect actors <c>Enemies/Corpses/DeadSidehopperBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] DeadSidehopperSharedEffectActors = CeresSteamSharedEffectActors;

    /// <summary>The projectile frames <c>Enemies/Corpses/DeadTorizoBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] DeadTorizoSharedProjectiles = BotwoonSharedProjectiles;

    /// <summary>The room effect actors <c>Enemies/Corpses/DeadTorizoBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] DeadTorizoSharedEffectActors = CeresSteamSharedEffectActors;

    /// <summary>The projectile frames <c>Enemies/Corpses/DeadTourianBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] DeadTourianSharedProjectiles = BotwoonSharedProjectiles;

    /// <summary>The room effect actors <c>Enemies/Corpses/DeadTourianBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] DeadTourianSharedEffectActors = CeresSteamSharedEffectActors;

    /// <summary>The projectile frames <c>Enemies/Dragon/DragonBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] DragonSharedProjectiles = CeresSteamSharedProjectiles;

    /// <summary>The room effect actors <c>Enemies/Dragon/DragonBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] DragonSharedEffectActors = CeresSteamSharedEffectActors;

    /// <summary>The projectile frames <c>Enemies/Evir/EvirBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] EvirSharedProjectiles = CeresSteamSharedProjectiles;

    /// <summary>The room effect actors <c>Enemies/Evir/EvirBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] EvirSharedEffectActors = CeresSteamSharedEffectActors;

    /// <summary>The projectile frames <c>Enemies/Fireflea/FirefleaBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] FirefleaSharedProjectiles = CeresSteamSharedProjectiles;

    /// <summary>The room effect actors <c>Enemies/Fireflea/FirefleaBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] FirefleaSharedEffectActors = CeresSteamSharedEffectActors;

    /// <summary>The projectile frames <c>Enemies/Gunship/GunshipBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] GunshipSharedProjectiles =
    [
        EnemyProjectileFrameId.GunshipDustRightInnerPuff,
        EnemyProjectileFrameId.GunshipDustRightInnerBillowing,
        EnemyProjectileFrameId.GunshipDustRightInnerSpreading,
        EnemyProjectileFrameId.GunshipDustRightInnerDrifting,
        EnemyProjectileFrameId.GunshipDustRightInnerThinning,
        EnemyProjectileFrameId.GunshipDustRightInnerFading,
        EnemyProjectileFrameId.GunshipDustRightInnerWisps,
        EnemyProjectileFrameId.GunshipDustRightInnerFaint,
        EnemyProjectileFrameId.GunshipDustRightMiddlePuff,
        EnemyProjectileFrameId.GunshipDustRightMiddleBillowing,
        EnemyProjectileFrameId.GunshipDustRightMiddleSpreading,
        EnemyProjectileFrameId.GunshipDustRightMiddleDrifting,
        EnemyProjectileFrameId.GunshipDustRightMiddleThinning,
        EnemyProjectileFrameId.GunshipDustRightMiddleFading,
        EnemyProjectileFrameId.GunshipDustRightMiddleWisps,
        EnemyProjectileFrameId.GunshipDustRightMiddleFaint,
        EnemyProjectileFrameId.GunshipDustRightOuterPuff,
        EnemyProjectileFrameId.GunshipDustRightOuterBillowing,
        EnemyProjectileFrameId.GunshipDustRightOuterSpreading,
        EnemyProjectileFrameId.GunshipDustRightOuterDrifting,
        EnemyProjectileFrameId.GunshipDustRightOuterThinning,
        EnemyProjectileFrameId.GunshipDustRightOuterFading,
        EnemyProjectileFrameId.GunshipDustRightOuterFaint,
        EnemyProjectileFrameId.GunshipDustLeftInnerPuff,
        EnemyProjectileFrameId.GunshipDustLeftInnerBillowing,
        EnemyProjectileFrameId.GunshipDustLeftInnerSpreading,
        EnemyProjectileFrameId.GunshipDustLeftInnerDrifting,
        EnemyProjectileFrameId.GunshipDustLeftInnerThinning,
        EnemyProjectileFrameId.GunshipDustLeftInnerFading,
        EnemyProjectileFrameId.GunshipDustLeftInnerWisps,
        EnemyProjectileFrameId.GunshipDustLeftInnerFaint,
        EnemyProjectileFrameId.GunshipDustLeftMiddlePuff,
        EnemyProjectileFrameId.GunshipDustLeftMiddleBillowing,
        EnemyProjectileFrameId.GunshipDustLeftMiddleSpreading,
        EnemyProjectileFrameId.GunshipDustLeftMiddleDrifting,
        EnemyProjectileFrameId.GunshipDustLeftMiddleThinning,
        EnemyProjectileFrameId.GunshipDustLeftMiddleFading,
        EnemyProjectileFrameId.GunshipDustLeftMiddleWisps,
        EnemyProjectileFrameId.GunshipDustLeftMiddleFaint,
        EnemyProjectileFrameId.GunshipDustLeftOuterPuff,
        EnemyProjectileFrameId.GunshipDustLeftOuterBillowing,
        EnemyProjectileFrameId.GunshipDustLeftOuterSpreading,
        EnemyProjectileFrameId.GunshipDustLeftOuterDrifting,
        EnemyProjectileFrameId.GunshipDustLeftOuterThinning,
        EnemyProjectileFrameId.GunshipDustLeftOuterFading,
        EnemyProjectileFrameId.GunshipDustLeftOuterFaint,
    ];

    /// <summary>The room effect actors <c>Enemies/Gunship/GunshipBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] GunshipSharedEffectActors = [];

    /// <summary>The projectile frames <c>Enemies/Kago/KagoBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] KagoSharedProjectiles =
    [
        EnemyProjectileFrameId.KagoBugSpark,
        EnemyProjectileFrameId.KagoBugFlareBall,
        EnemyProjectileFrameId.KagoBugFlareRingCored,
        EnemyProjectileFrameId.KagoBugFlareRing,
        EnemyProjectileFrameId.KagoBugFlareRingFading,
        EnemyProjectileFrameId.EnemyDeathFireballSmall,
        EnemyProjectileFrameId.EnemyDeathFireballLarge,
        EnemyProjectileFrameId.EnemyDeathFlamingRing,
        EnemyProjectileFrameId.EnemyDeathFireballHuge,
        EnemyProjectileFrameId.EnemyDeathWideRing,
        EnemyProjectileFrameId.EnemyDeathWideRingFading,
        EnemyProjectileFrameId.EnemyDeathSpark,
        EnemyProjectileFrameId.EnemyDeathStarburst,
        EnemyProjectileFrameId.EnemyDeathEmbers,
        EnemyProjectileFrameId.EnemyPickupEnergySmallDark,
        EnemyProjectileFrameId.EnemyPickupEnergySmallRinged,
        EnemyProjectileFrameId.EnemyPickupEnergySmallBright,
        EnemyProjectileFrameId.EnemyPickupEnergyScattered,
        EnemyProjectileFrameId.EnemyPickupEnergyBigBright,
        EnemyProjectileFrameId.EnemyPickupEnergyBigRinged,
        EnemyProjectileFrameId.EnemyPickupEnergyBigDark,
        EnemyProjectileFrameId.EnemyPickupMissile,
        EnemyProjectileFrameId.EnemyPickupMissileFlashing,
        EnemyProjectileFrameId.EnemyPickupSuperMissile,
        EnemyProjectileFrameId.EnemyPickupSuperMissileFlashing,
        EnemyProjectileFrameId.EnemyPickupPowerBombBright,
        EnemyProjectileFrameId.EnemyPickupPowerBombFading,
        EnemyProjectileFrameId.EnemyPickupPowerBombDim,
        EnemyProjectileFrameId.EnemyPickupPowerBombFlashing,
        EnemyProjectileFrameId.EnemyDeathFireballCluster,
        EnemyProjectileFrameId.EnemyDeathStarburstCluster,
        EnemyProjectileFrameId.EnemyDeathEmbersCluster,
        EnemyProjectileFrameId.EnemyDeathFourFireballsAmidEmbers,
        EnemyProjectileFrameId.EnemyDeathFourFireballsAmidAsh,
        EnemyProjectileFrameId.EnemyDeathFourEmbersFlyingApart,
        EnemyProjectileFrameId.EnemyDeathFourEmbersSpreading,
        EnemyProjectileFrameId.EnemyDeathFourEmbersCooling,
        EnemyProjectileFrameId.EnemyDeathFourEmbersDimming,
        EnemyProjectileFrameId.EnemyDeathFourAshDrifting,
        EnemyProjectileFrameId.EnemyDeathFourAshSpreading,
        EnemyProjectileFrameId.EnemyDeathFourAshSinking,
        EnemyProjectileFrameId.EnemyDeathFourAshFalling,
        EnemyProjectileFrameId.EnemyDeathFourAshSettling,
        EnemyProjectileFrameId.EnemyDeathFourAshDispersing,
    ];

    /// <summary>The room effect actors <c>Enemies/Kago/KagoBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] KagoSharedEffectActors = CeresSteamSharedEffectActors;

    /// <summary>The projectile frames <c>Enemies/Magdollite/MagdolliteBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] MagdolliteSharedProjectiles = CeresSteamSharedProjectiles;

    /// <summary>The room effect actors <c>Enemies/Magdollite/MagdolliteBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] MagdolliteSharedEffectActors = CeresSteamSharedEffectActors;

    /// <summary>The projectile frames <c>Enemies/MamaTurtles/MamaTurtleBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] MamaTurtleSharedProjectiles = CeresSteamSharedProjectiles;

    /// <summary>The room effect actors <c>Enemies/MamaTurtles/MamaTurtleBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] MamaTurtleSharedEffectActors = CeresSteamSharedEffectActors;

    /// <summary>The projectile frames <c>Enemies/MaridiaSnails/OumBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] OumSharedProjectiles = CeresSteamSharedProjectiles;

    /// <summary>The room effect actors <c>Enemies/MaridiaSnails/OumBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] OumSharedEffectActors = CeresSteamSharedEffectActors;

    /// <summary>The projectile frames <c>Enemies/Metroids/MetroidBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] MetroidSharedProjectiles = CeresSteamSharedProjectiles;

    /// <summary>The room effect actors <c>Enemies/Metroids/MetroidBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] MetroidSharedEffectActors = CeresSteamSharedEffectActors;

    /// <summary>The projectile frames <c>Enemies/Metroids/MochtroidBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] MochtroidSharedProjectiles = CeresSteamSharedProjectiles;

    /// <summary>The room effect actors <c>Enemies/Metroids/MochtroidBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] MochtroidSharedEffectActors = CeresSteamSharedEffectActors;

    /// <summary>The projectile frames <c>Enemies/NoobTube/NoobTubeBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] NoobTubeSharedProjectiles =
    [
        EnemyProjectileFrameId.NoobTubeCrackStarting,
        EnemyProjectileFrameId.NoobTubeCrackSpreading,
        EnemyProjectileFrameId.NoobTubeCrackWidening,
        EnemyProjectileFrameId.NoobTubeCrackReaching,
        EnemyProjectileFrameId.NoobTubeCrackSpanning,
        EnemyProjectileFrameId.NoobTubeCrackBranching,
        EnemyProjectileFrameId.NoobTubeShardsBursting,
        EnemyProjectileFrameId.NoobTubeShardsFlyingOut,
        EnemyProjectileFrameId.NoobTubeShardsFlyingApart,
        EnemyProjectileFrameId.NoobTubeShardsSpreading,
        EnemyProjectileFrameId.NoobTubeShardsSlowing,
        EnemyProjectileFrameId.NoobTubeShardsPeaking,
        EnemyProjectileFrameId.NoobTubeShardsTurning,
        EnemyProjectileFrameId.NoobTubeShardsCoasting,
        EnemyProjectileFrameId.NoobTubeShardsTipping,
        EnemyProjectileFrameId.NoobTubeShardsAtWidest,
        EnemyProjectileFrameId.NoobTubeShardsDipping,
        EnemyProjectileFrameId.NoobTubeShardsDropping,
        EnemyProjectileFrameId.NoobTubeShardsSinking,
        EnemyProjectileFrameId.NoobTubeShardsLowering,
        EnemyProjectileFrameId.NoobTubeShardsDescending,
        EnemyProjectileFrameId.NoobTubeShardsFalling,
        EnemyProjectileFrameId.NoobTubeShardsDrifting,
        EnemyProjectileFrameId.NoobTubeShardsFloating,
        EnemyProjectileFrameId.NoobTubeShardsWafting,
        EnemyProjectileFrameId.NoobTubeShardsGliding,
        EnemyProjectileFrameId.NoobTubeShardsSailing,
        EnemyProjectileFrameId.NoobTubeShardsFluttering,
        EnemyProjectileFrameId.NoobTubeShardsSubsiding,
        EnemyProjectileFrameId.NoobTubeShardsSlipping,
        EnemyProjectileFrameId.NoobTubeShardsSifting,
        EnemyProjectileFrameId.NoobTubeShardsSettling,
        EnemyProjectileFrameId.NoobTubeShardsSettlingLower,
        EnemyProjectileFrameId.NoobTubeShardsNearlySettled,
        EnemyProjectileFrameId.NoobTubeShardsComingToRest,
        EnemyProjectileFrameId.NoobTubeShardsLanded,
        EnemyProjectileFrameId.NoobTubeShardDownOutward,
        EnemyProjectileFrameId.NoobTubeShardDownOutwardMirrored,
        EnemyProjectileFrameId.NoobTubeShardUpOutward,
        EnemyProjectileFrameId.NoobTubeShardUpOutwardMirrored,
        EnemyProjectileFrameId.NoobTubeShardUpSteep,
        EnemyProjectileFrameId.NoobTubeShardUpSteepMirrored,
        EnemyProjectileFrameId.NoobTubeShardUpDiagonalInner,
        EnemyProjectileFrameId.NoobTubeShardUpDiagonalInnerMirrored,
        EnemyProjectileFrameId.NoobTubeShardUpDiagonalOuter,
        EnemyProjectileFrameId.NoobTubeShardUpDiagonalOuterMirrored,
        EnemyProjectileFrameId.NoobTubeShardDownDiagonal,
        EnemyProjectileFrameId.NoobTubeShardDownDiagonalMirrored,
        EnemyProjectileFrameId.NoobTubeShardDownOuter,
        EnemyProjectileFrameId.NoobTubeShardDownOuterMirrored,
        EnemyProjectileFrameId.NoobTubeShardOutward,
        EnemyProjectileFrameId.NoobTubeShardOutwardMirrored,
        EnemyProjectileFrameId.NoobTubeShardUp,
        EnemyProjectileFrameId.NoobTubeShardDownInner,
        EnemyProjectileFrameId.NoobTubeShardDownInnerMirrored,
        EnemyProjectileFrameId.NoobTubeDustTrailForming,
        EnemyProjectileFrameId.NoobTubeDustTrailGrowing,
        EnemyProjectileFrameId.NoobTubeDustTrailLengthening,
        EnemyProjectileFrameId.NoobTubeDustTrailFullLength,
        EnemyProjectileFrameId.NoobTubeDustTrailThinning,
        EnemyProjectileFrameId.NoobTubeDustTrailThin,
        EnemyProjectileFrameId.NoobTubeDustTrailBreaking,
        EnemyProjectileFrameId.NoobTubeDustBubblesForming,
        EnemyProjectileFrameId.NoobTubeDustBubbles,
        EnemyProjectileFrameId.NoobTubeDustBubblesDrifting,
        EnemyProjectileFrameId.NoobTubeDustBubblesSparse,
        EnemyProjectileFrameId.NoobTubeDustBubblesBursting,
        EnemyProjectileFrameId.NoobTubeDustSpecks,
        EnemyProjectileFrameId.NoobTubeDustSpecksFaint,
    ];

    /// <summary>The room effect actors <c>Enemies/NoobTube/NoobTubeBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] NoobTubeSharedEffectActors = [];

    /// <summary>The projectile frames <c>Enemies/Powamp/PowampBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] PowampSharedProjectiles = CeresSteamSharedProjectiles;

    /// <summary>The room effect actors <c>Enemies/Powamp/PowampBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] PowampSharedEffectActors = CeresSteamSharedEffectActors;

    /// <summary>The projectile frames <c>Enemies/Shaktool/ShaktoolBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] ShaktoolSharedProjectiles = CeresSteamSharedProjectiles;

    /// <summary>The room effect actors <c>Enemies/Shaktool/ShaktoolBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] ShaktoolSharedEffectActors = CeresSteamSharedEffectActors;

    /// <summary>The projectile frames <c>Enemies/Shutters/ShuttersBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] ShuttersSharedProjectiles = CeresSteamSharedProjectiles;

    /// <summary>The room effect actors <c>Enemies/Shutters/ShuttersBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] ShuttersSharedEffectActors = CeresSteamSharedEffectActors;

    /// <summary>The projectile frames <c>Enemies/Skree/SkreeBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] SkreeSharedProjectiles = CeresSteamSharedProjectiles;

    /// <summary>The room effect actors <c>Enemies/Skree/SkreeBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] SkreeSharedEffectActors = CeresSteamSharedEffectActors;

    /// <summary>The projectile frames <c>Enemies/SpacePirates/NinjaSpacePirateBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] NinjaSpacePirateSharedProjectiles = CeresSteamSharedProjectiles;

    /// <summary>The room effect actors <c>Enemies/SpacePirates/NinjaSpacePirateBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] NinjaSpacePirateSharedEffectActors =
    [
        RoomEffectActorKind.BigDustCloud,
        RoomEffectActorKind.RisingDustPuff,
        RoomEffectActorKind.SmallExplosion,
        RoomEffectActorKind.Smoke,
    ];

    /// <summary>The projectile frames <c>Enemies/SpacePirates/WalkingSpacePirateBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] WalkingSpacePirateSharedProjectiles =
    [
        EnemyProjectileFrameId.SpacePirateLaserFlareGlint,
        EnemyProjectileFrameId.SpacePirateLaserFlareGlow,
        EnemyProjectileFrameId.SpacePirateLaserFlareBall,
        EnemyProjectileFrameId.SpacePirateLaserEmergingBright,
        EnemyProjectileFrameId.SpacePirateLaserEmergingDim,
        EnemyProjectileFrameId.SpacePirateLaserExtendingBright,
        EnemyProjectileFrameId.SpacePirateLaserExtendingDim,
        EnemyProjectileFrameId.SpacePirateLaserLengtheningBright,
        EnemyProjectileFrameId.SpacePirateLaserLengtheningDim,
        EnemyProjectileFrameId.SpacePirateLaserFullLengthBright,
        EnemyProjectileFrameId.SpacePirateLaserFullLengthDim,
        EnemyProjectileFrameId.EnemyDeathFireballSmall,
        EnemyProjectileFrameId.EnemyDeathFireballLarge,
        EnemyProjectileFrameId.EnemyDeathFlamingRing,
        EnemyProjectileFrameId.EnemyDeathFireballHuge,
        EnemyProjectileFrameId.EnemyDeathWideRing,
        EnemyProjectileFrameId.EnemyDeathWideRingFading,
        EnemyProjectileFrameId.EnemyDeathSpark,
        EnemyProjectileFrameId.EnemyDeathStarburst,
        EnemyProjectileFrameId.EnemyDeathEmbers,
        EnemyProjectileFrameId.EnemyPickupEnergySmallDark,
        EnemyProjectileFrameId.EnemyPickupEnergySmallRinged,
        EnemyProjectileFrameId.EnemyPickupEnergySmallBright,
        EnemyProjectileFrameId.EnemyPickupEnergyScattered,
        EnemyProjectileFrameId.EnemyPickupEnergyBigBright,
        EnemyProjectileFrameId.EnemyPickupEnergyBigRinged,
        EnemyProjectileFrameId.EnemyPickupEnergyBigDark,
        EnemyProjectileFrameId.EnemyPickupMissile,
        EnemyProjectileFrameId.EnemyPickupMissileFlashing,
        EnemyProjectileFrameId.EnemyPickupSuperMissile,
        EnemyProjectileFrameId.EnemyPickupSuperMissileFlashing,
        EnemyProjectileFrameId.EnemyPickupPowerBombBright,
        EnemyProjectileFrameId.EnemyPickupPowerBombFading,
        EnemyProjectileFrameId.EnemyPickupPowerBombDim,
        EnemyProjectileFrameId.EnemyPickupPowerBombFlashing,
        EnemyProjectileFrameId.EnemyDeathFireballCluster,
        EnemyProjectileFrameId.EnemyDeathStarburstCluster,
        EnemyProjectileFrameId.EnemyDeathEmbersCluster,
        EnemyProjectileFrameId.EnemyDeathFourFireballsAmidEmbers,
        EnemyProjectileFrameId.EnemyDeathFourFireballsAmidAsh,
        EnemyProjectileFrameId.EnemyDeathFourEmbersFlyingApart,
        EnemyProjectileFrameId.EnemyDeathFourEmbersSpreading,
        EnemyProjectileFrameId.EnemyDeathFourEmbersCooling,
        EnemyProjectileFrameId.EnemyDeathFourEmbersDimming,
        EnemyProjectileFrameId.EnemyDeathFourAshDrifting,
        EnemyProjectileFrameId.EnemyDeathFourAshSpreading,
        EnemyProjectileFrameId.EnemyDeathFourAshSinking,
        EnemyProjectileFrameId.EnemyDeathFourAshFalling,
        EnemyProjectileFrameId.EnemyDeathFourAshSettling,
        EnemyProjectileFrameId.EnemyDeathFourAshDispersing,
    ];

    /// <summary>The room effect actors <c>Enemies/SpacePirates/WalkingSpacePirateBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] WalkingSpacePirateSharedEffectActors = CeresSteamSharedEffectActors;

    /// <summary>The projectile frames <c>Enemies/SpacePirates/WallSpacePirateBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] WallSpacePirateSharedProjectiles = WalkingSpacePirateSharedProjectiles;

    /// <summary>The room effect actors <c>Enemies/SpacePirates/WallSpacePirateBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] WallSpacePirateSharedEffectActors = CeresSteamSharedEffectActors;

    /// <summary>The projectile frames <c>Enemies/WorkRobots/WorkRobotBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] WorkRobotSharedProjectiles =
    [
        EnemyProjectileFrameId.WorkRobotLaserFlareGlint,
        EnemyProjectileFrameId.WorkRobotLaserFlareGlow,
        EnemyProjectileFrameId.WorkRobotLaserFlareBall,
        EnemyProjectileFrameId.WorkRobotLaserFlareRingCored,
        EnemyProjectileFrameId.WorkRobotLaserFlareRing,
        EnemyProjectileFrameId.WorkRobotLaserFlareRingFading,
        EnemyProjectileFrameId.EnemyDeathFireballSmall,
        EnemyProjectileFrameId.EnemyDeathFireballLarge,
        EnemyProjectileFrameId.EnemyDeathFlamingRing,
        EnemyProjectileFrameId.EnemyDeathFireballHuge,
        EnemyProjectileFrameId.EnemyDeathWideRing,
        EnemyProjectileFrameId.EnemyDeathWideRingFading,
        EnemyProjectileFrameId.EnemyDeathSpark,
        EnemyProjectileFrameId.EnemyDeathStarburst,
        EnemyProjectileFrameId.EnemyDeathEmbers,
        EnemyProjectileFrameId.EnemyPickupEnergySmallDark,
        EnemyProjectileFrameId.EnemyPickupEnergySmallRinged,
        EnemyProjectileFrameId.EnemyPickupEnergySmallBright,
        EnemyProjectileFrameId.EnemyPickupEnergyScattered,
        EnemyProjectileFrameId.EnemyPickupEnergyBigBright,
        EnemyProjectileFrameId.EnemyPickupEnergyBigRinged,
        EnemyProjectileFrameId.EnemyPickupEnergyBigDark,
        EnemyProjectileFrameId.EnemyPickupMissile,
        EnemyProjectileFrameId.EnemyPickupMissileFlashing,
        EnemyProjectileFrameId.EnemyPickupSuperMissile,
        EnemyProjectileFrameId.EnemyPickupSuperMissileFlashing,
        EnemyProjectileFrameId.EnemyPickupPowerBombBright,
        EnemyProjectileFrameId.EnemyPickupPowerBombFading,
        EnemyProjectileFrameId.EnemyPickupPowerBombDim,
        EnemyProjectileFrameId.EnemyPickupPowerBombFlashing,
        EnemyProjectileFrameId.EnemyDeathFireballCluster,
        EnemyProjectileFrameId.EnemyDeathStarburstCluster,
        EnemyProjectileFrameId.EnemyDeathEmbersCluster,
        EnemyProjectileFrameId.EnemyDeathFourFireballsAmidEmbers,
        EnemyProjectileFrameId.EnemyDeathFourFireballsAmidAsh,
        EnemyProjectileFrameId.EnemyDeathFourEmbersFlyingApart,
        EnemyProjectileFrameId.EnemyDeathFourEmbersSpreading,
        EnemyProjectileFrameId.EnemyDeathFourEmbersCooling,
        EnemyProjectileFrameId.EnemyDeathFourEmbersDimming,
        EnemyProjectileFrameId.EnemyDeathFourAshDrifting,
        EnemyProjectileFrameId.EnemyDeathFourAshSpreading,
        EnemyProjectileFrameId.EnemyDeathFourAshSinking,
        EnemyProjectileFrameId.EnemyDeathFourAshFalling,
        EnemyProjectileFrameId.EnemyDeathFourAshSettling,
        EnemyProjectileFrameId.EnemyDeathFourAshDispersing,
    ];

    /// <summary>The room effect actors <c>Enemies/WorkRobots/WorkRobotBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] WorkRobotSharedEffectActors = CeresSteamSharedEffectActors;

    /// <summary>The projectile frames <c>Enemies/YappingMaws/YappingMawBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] YappingMawSharedProjectiles = CeresSteamSharedProjectiles;

    /// <summary>The room effect actors <c>Enemies/YappingMaws/YappingMawBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] YappingMawSharedEffectActors = CeresSteamSharedEffectActors;

    /// <summary>The projectile frames <c>Enemies/Yard/YardEnemyBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly EnemyProjectileFrameId[] YardEnemySharedProjectiles = CeresSteamSharedProjectiles;

    /// <summary>The room effect actors <c>Enemies/Yard/YardEnemyBehavior.cs</c> spawns onto its sets' shared sheet.</summary>
    private static readonly RoomEffectActorKind[] YardEnemySharedEffectActors = CeresSteamSharedEffectActors;

    /// <summary>The projectile frames <see cref="SpawnedObjectOwner.MotherBrainTankGlass"/>'s sheet holds.</summary>
    private static readonly EnemyProjectileFrameId[] MotherBrainTankGlassSheetProjectiles =
    [
        EnemyProjectileFrameId.MotherBrainGlassShardUpTurned0,
        EnemyProjectileFrameId.MotherBrainGlassShardUpTurned45,
        EnemyProjectileFrameId.MotherBrainGlassShardUpTurned90,
        EnemyProjectileFrameId.MotherBrainGlassShardUpTurned135,
        EnemyProjectileFrameId.MotherBrainGlassShardUpTurned180,
        EnemyProjectileFrameId.MotherBrainGlassShardUpTurned225,
        EnemyProjectileFrameId.MotherBrainGlassShardUpTurned270,
        EnemyProjectileFrameId.MotherBrainGlassShardUpTurned315,
        EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned0,
        EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned45,
        EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned90,
        EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned135,
        EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned180,
        EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned225,
        EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned270,
        EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned315,
        EnemyProjectileFrameId.MotherBrainGlassShardRightTurned0,
        EnemyProjectileFrameId.MotherBrainGlassShardRightTurned45,
        EnemyProjectileFrameId.MotherBrainGlassShardRightTurned90,
        EnemyProjectileFrameId.MotherBrainGlassShardRightTurned135,
        EnemyProjectileFrameId.MotherBrainGlassShardRightTurned180,
        EnemyProjectileFrameId.MotherBrainGlassShardRightTurned225,
        EnemyProjectileFrameId.MotherBrainGlassShardRightTurned270,
        EnemyProjectileFrameId.MotherBrainGlassShardRightTurned315,
        EnemyProjectileFrameId.MotherBrainGlassShardDownRightTurned0,
        EnemyProjectileFrameId.MotherBrainGlassShardDownRightTurned45,
        EnemyProjectileFrameId.MotherBrainGlassShardDownRightTurned90,
        EnemyProjectileFrameId.MotherBrainGlassShardDownRightTurned135,
        EnemyProjectileFrameId.MotherBrainGlassShardDownRightTurned180,
        EnemyProjectileFrameId.MotherBrainGlassShardDownRightTurned225,
        EnemyProjectileFrameId.MotherBrainGlassShardDownRightTurned270,
        EnemyProjectileFrameId.MotherBrainGlassShardDownRightTurned315,
        EnemyProjectileFrameId.MotherBrainGlassShardDownTurned0,
        EnemyProjectileFrameId.MotherBrainGlassShardDownTurned45,
        EnemyProjectileFrameId.MotherBrainGlassShardDownTurned90,
        EnemyProjectileFrameId.MotherBrainGlassShardDownTurned135,
        EnemyProjectileFrameId.MotherBrainGlassShardDownTurned180,
        EnemyProjectileFrameId.MotherBrainGlassShardDownTurned225,
        EnemyProjectileFrameId.MotherBrainGlassShardDownTurned270,
        EnemyProjectileFrameId.MotherBrainGlassShardDownTurned315,
        EnemyProjectileFrameId.MotherBrainGlassShardDownLeftTurned0,
        EnemyProjectileFrameId.MotherBrainGlassShardDownLeftTurned45,
        EnemyProjectileFrameId.MotherBrainGlassShardDownLeftTurned90,
        EnemyProjectileFrameId.MotherBrainGlassShardDownLeftTurned135,
        EnemyProjectileFrameId.MotherBrainGlassShardDownLeftTurned180,
        EnemyProjectileFrameId.MotherBrainGlassShardDownLeftTurned225,
        EnemyProjectileFrameId.MotherBrainGlassShardDownLeftTurned270,
        EnemyProjectileFrameId.MotherBrainGlassShardDownLeftTurned315,
        EnemyProjectileFrameId.MotherBrainGlassShardLeftTurned0,
        EnemyProjectileFrameId.MotherBrainGlassShardLeftTurned45,
        EnemyProjectileFrameId.MotherBrainGlassShardLeftTurned90,
        EnemyProjectileFrameId.MotherBrainGlassShardLeftTurned135,
        EnemyProjectileFrameId.MotherBrainGlassShardLeftTurned180,
        EnemyProjectileFrameId.MotherBrainGlassShardLeftTurned225,
        EnemyProjectileFrameId.MotherBrainGlassShardLeftTurned270,
        EnemyProjectileFrameId.MotherBrainGlassShardLeftTurned315,
        EnemyProjectileFrameId.MotherBrainGlassShardUpLeftTurned0,
        EnemyProjectileFrameId.MotherBrainGlassShardUpLeftTurned45,
        EnemyProjectileFrameId.MotherBrainGlassShardUpLeftTurned90,
        EnemyProjectileFrameId.MotherBrainGlassShardUpLeftTurned135,
        EnemyProjectileFrameId.MotherBrainGlassShardUpLeftTurned180,
        EnemyProjectileFrameId.MotherBrainGlassShardUpLeftTurned225,
        EnemyProjectileFrameId.MotherBrainGlassShardUpLeftTurned270,
        EnemyProjectileFrameId.MotherBrainGlassShardUpLeftTurned315,
        EnemyProjectileFrameId.MotherBrainGlassSparkleCross,
        EnemyProjectileFrameId.MotherBrainGlassSparkleCrossSmall,
        EnemyProjectileFrameId.MotherBrainGlassSparkleDiagonal,
        EnemyProjectileFrameId.MotherBrainGlassSparkleDiagonalFaint,
    ];

    /// <summary>The room effect actors <see cref="SpawnedObjectOwner.MotherBrainTankGlass"/>'s sheet holds.</summary>
    private static readonly RoomEffectActorKind[] MotherBrainTankGlassSheetEffectActors = [];

    /// <summary>The projectile frames <c>CeresFallingDebrisMain</c>'s code spawns onto the shared sheet of the sets whose rooms run it.</summary>
    private static readonly EnemyProjectileFrameId[] CommonCeresFallingDebrisMainProjectiles =
    [
        EnemyProjectileFrameId.CeresFallingDebrisLight,
        EnemyProjectileFrameId.CeresFallingDebrisDark,
    ];

    /// <summary>The room effect actors <c>CeresFallingDebrisMain</c>'s code spawns onto the shared sheet of the sets whose rooms run it.</summary>
    private static readonly RoomEffectActorKind[] CommonCeresFallingDebrisMainEffectActors = [];

    /// <summary>The projectile frames <c>DownwardGateBehavior</c>'s code spawns onto the shared sheet of the sets whose rooms run it.</summary>
    private static readonly EnemyProjectileFrameId[] CommonDownwardGateBehaviorProjectiles =
    [
        EnemyProjectileFrameId.DownwardGateOneBlockTall,
        EnemyProjectileFrameId.DownwardGateTwoBlocksTall,
        EnemyProjectileFrameId.DownwardGateThreeBlocksTall,
        EnemyProjectileFrameId.DownwardGateFourBlocksTall,
    ];

    /// <summary>The room effect actors <c>DownwardGateBehavior</c>'s code spawns onto the shared sheet of the sets whose rooms run it.</summary>
    private static readonly RoomEffectActorKind[] CommonDownwardGateBehaviorEffectActors = [];

    /// <summary>The projectile frames <c>EnemyDeathsAndDrops</c>'s code spawns onto the shared sheet of the sets whose rooms run it.</summary>
    private static readonly EnemyProjectileFrameId[] CommonEnemyDeathsAndDropsProjectiles = CeresSteamSharedProjectiles;

    /// <summary>The room effect actors <c>EnemyDeathsAndDrops</c>'s code spawns onto the shared sheet of the sets whose rooms run it.</summary>
    private static readonly RoomEffectActorKind[] CommonEnemyDeathsAndDropsEffectActors = CeresSteamSharedEffectActors;

    /// <summary>The projectile frames <c>EnemyProjectileEngine.CreateBombDudShot</c>'s code spawns onto the shared sheet of the sets whose rooms run it.</summary>
    private static readonly EnemyProjectileFrameId[] CommonEnemyProjectileEngineCreateBombDudShotProjectiles = [];

    /// <summary>The room effect actors <c>EnemyProjectileEngine.CreateBombDudShot</c>'s code spawns onto the shared sheet of the sets whose rooms run it.</summary>
    private static readonly RoomEffectActorKind[] CommonEnemyProjectileEngineCreateBombDudShotEffectActors =
    [
        RoomEffectActorKind.DudShot,
    ];

    /// <summary>The projectile frames <c>EnemyProjectileEngine.CreateEnemyProjectileDudShot</c>'s code spawns onto the shared sheet of the sets whose rooms run it.</summary>
    private static readonly EnemyProjectileFrameId[] CommonEnemyProjectileEngineCreateEnemyProjectileDudShotProjectiles = [];

    /// <summary>The room effect actors <c>EnemyProjectileEngine.CreateEnemyProjectileDudShot</c>'s code spawns onto the shared sheet of the sets whose rooms run it.</summary>
    private static readonly RoomEffectActorKind[] CommonEnemyProjectileEngineCreateEnemyProjectileDudShotEffectActors =
    [
        RoomEffectActorKind.DudShot,
    ];

    /// <summary>The projectile frames <c>EnemyProjectileEngine.SpawnDustOrExplosion</c>'s code spawns onto the shared sheet of the sets whose rooms run it.</summary>
    private static readonly EnemyProjectileFrameId[] CommonEnemyProjectileEngineSpawnDustOrExplosionProjectiles = CeresElevatorDoorSharedProjectiles;

    /// <summary>The room effect actors <c>EnemyProjectileEngine.SpawnDustOrExplosion</c>'s code spawns onto the shared sheet of the sets whose rooms run it.</summary>
    private static readonly RoomEffectActorKind[] CommonEnemyProjectileEngineSpawnDustOrExplosionEffectActors = [];

    /// <summary>The projectile frames <c>EscapeExplosionsBehavior</c>'s code spawns onto the shared sheet of the sets whose rooms run it.</summary>
    private static readonly EnemyProjectileFrameId[] CommonEscapeExplosionsBehaviorProjectiles = [];

    /// <summary>The room effect actors <c>EscapeExplosionsBehavior</c>'s code spawns onto the shared sheet of the sets whose rooms run it.</summary>
    private static readonly RoomEffectActorKind[] CommonEscapeExplosionsBehaviorEffectActors =
    [
        RoomEffectActorKind.BigDustCloud,
        RoomEffectActorKind.ShortBigDustCloud,
        RoomEffectActorKind.SmallExplosion,
        RoomEffectActorKind.Smoke,
        RoomEffectActorKind.SmokePuff,
    ];

    /// <summary>The projectile frames <c>EyeDoorBehavior</c>'s code spawns onto the shared sheet of the sets whose rooms run it.</summary>
    private static readonly EnemyProjectileFrameId[] CommonEyeDoorBehaviorProjectiles =
    [
        EnemyProjectileFrameId.EyeDoorBallRingFaint,
        EnemyProjectileFrameId.EyeDoorBallRing,
        EnemyProjectileFrameId.EyeDoorBallRingCored,
        EnemyProjectileFrameId.EyeDoorBallWhole,
        EnemyProjectileFrameId.EyeDoorBallFireCloud,
        EnemyProjectileFrameId.EyeDoorBallFireCloudSpreading,
        EnemyProjectileFrameId.EyeDoorBallFireCloudBreaking,
        EnemyProjectileFrameId.EyeDoorBallFireCloudEmbers,
        EnemyProjectileFrameId.EyeDoorSweatDrop,
        EnemyProjectileFrameId.EyeDoorSweatSmokePuffLow,
        EnemyProjectileFrameId.EyeDoorSweatSmokePuffRising,
        EnemyProjectileFrameId.EyeDoorSweatSmokePuffScattering,
        EnemyProjectileFrameId.DustExplosionSparksGathering,
        EnemyProjectileFrameId.DustExplosionSparksDrawingIn,
        EnemyProjectileFrameId.DustExplosionSparksClosingIn,
        EnemyProjectileFrameId.DustExplosionSparksConverging,
        EnemyProjectileFrameId.DustExplosionSparksNearing,
        EnemyProjectileFrameId.DustExplosionSparksMeeting,
        EnemyProjectileFrameId.DustExplosionSparksTwistedGathering,
        EnemyProjectileFrameId.DustExplosionSparksTwistedDrawingIn,
        EnemyProjectileFrameId.DustExplosionSparksTwistedClosingIn,
        EnemyProjectileFrameId.DustExplosionSparksTwistedConverging,
        EnemyProjectileFrameId.DustExplosionSparksTwistedNearing,
        EnemyProjectileFrameId.DustExplosionSparksTwistedMeeting,
        EnemyProjectileFrameId.DustExplosionSpark,
        EnemyProjectileFrameId.DustExplosionFireballSmall,
        EnemyProjectileFrameId.DustExplosionFireballLarge,
        EnemyProjectileFrameId.DustExplosionFlamingRing,
        EnemyProjectileFrameId.DustExplosionStarburst,
        EnemyProjectileFrameId.DustExplosionEmbers,
        EnemyProjectileFrameId.DustExplosionGreenOrbSmall,
        EnemyProjectileFrameId.DustExplosionGreenOrb,
        EnemyProjectileFrameId.DustExplosionGreenRingCored,
        EnemyProjectileFrameId.DustExplosionGreenRingThick,
        EnemyProjectileFrameId.DustExplosionGreenRing,
        EnemyProjectileFrameId.DustExplosionFlareGlint,
        EnemyProjectileFrameId.DustExplosionFlareGlow,
        EnemyProjectileFrameId.DustExplosionFlareBall,
        EnemyProjectileFrameId.DustExplosionFlareRingCored,
        EnemyProjectileFrameId.DustExplosionFlareRing,
        EnemyProjectileFrameId.DustExplosionFlareRingFading,
        EnemyProjectileFrameId.DustExplosionPuff,
        EnemyProjectileFrameId.DustExplosionCloud,
        EnemyProjectileFrameId.DustExplosionCloudThinning,
        EnemyProjectileFrameId.DustExplosionCloudThin,
        EnemyProjectileFrameId.DustExplosionCloudFaint,
        EnemyProjectileFrameId.DustExplosionPuffSmall,
        EnemyProjectileFrameId.DustExplosionPuffThin,
        EnemyProjectileFrameId.DustExplosionPuffFaint,
        EnemyProjectileFrameId.DustExplosionSmokePuffLow,
        EnemyProjectileFrameId.DustExplosionSmokePuffRising,
        EnemyProjectileFrameId.DustExplosionSmokePuffScattering,
        EnemyProjectileFrameId.DustExplosionSmokePuffFading,
        EnemyProjectileFrameId.DustExplosionFireCloud,
        EnemyProjectileFrameId.DustExplosionFireCloudSpreading,
        EnemyProjectileFrameId.DustExplosionFireCloudBreaking,
        EnemyProjectileFrameId.DustExplosionFireCloudEmbers,
        EnemyProjectileFrameId.DustExplosionFireballHuge,
        EnemyProjectileFrameId.DustExplosionWideRing,
        EnemyProjectileFrameId.DustExplosionWideRingFading,
        EnemyProjectileFrameId.DustExplosionPebblesBursting,
        EnemyProjectileFrameId.DustExplosionPebblesFlyingOut,
        EnemyProjectileFrameId.DustExplosionPebblesFlyingApart,
        EnemyProjectileFrameId.DustExplosionPebblesClimbing,
        EnemyProjectileFrameId.DustExplosionPebblesSlowing,
        EnemyProjectileFrameId.DustExplosionPebblesPeaking,
        EnemyProjectileFrameId.DustExplosionPebblesTurningOver,
        EnemyProjectileFrameId.DustExplosionPebblesStartingDown,
        EnemyProjectileFrameId.DustExplosionPebblesDropping,
        EnemyProjectileFrameId.DustExplosionPebblesFalling,
        EnemyProjectileFrameId.DustExplosionPebblesTurningAshen,
        EnemyProjectileFrameId.DustExplosionPebblesTumbling,
        EnemyProjectileFrameId.DustExplosionPebblesPlunging,
        EnemyProjectileFrameId.DustExplosionPebblesPlummeting,
        EnemyProjectileFrameId.DustExplosionPebblesScattering,
        EnemyProjectileFrameId.DustExplosionPebblesFallingAway,
        EnemyProjectileFrameId.DustExplosionPuffUpLeft,
        EnemyProjectileFrameId.DustExplosionPuffPair,
        EnemyProjectileFrameId.DustExplosionPuffTrio,
        EnemyProjectileFrameId.DustExplosionCloudsDense,
        EnemyProjectileFrameId.DustExplosionCloudsThinning,
        EnemyProjectileFrameId.DustExplosionCloudsThin,
        EnemyProjectileFrameId.DustExplosionCloudsFaint,
        EnemyProjectileFrameId.DustExplosionCloudFaintRight,
        EnemyProjectileFrameId.DustExplosionBubbleSmall,
        EnemyProjectileFrameId.DustExplosionBubbles,
        EnemyProjectileFrameId.DustExplosionBubblesRising,
        EnemyProjectileFrameId.DustExplosionBubblesSpreading,
        EnemyProjectileFrameId.DustExplosionBubblesScattered,
        EnemyProjectileFrameId.DustExplosionBubblesSparse,
        EnemyProjectileFrameId.DustExplosionBubblesFew,
        EnemyProjectileFrameId.DustExplosionBubblesLast,
        EnemyProjectileFrameId.DustExplosionSpeck,
    ];

    /// <summary>The room effect actors <c>EyeDoorBehavior</c>'s code spawns onto the shared sheet of the sets whose rooms run it.</summary>
    private static readonly RoomEffectActorKind[] CommonEyeDoorBehaviorEffectActors = [];

    /// <summary>The projectile frames <c>OldTourianEscapeShaftWallBehavior</c>'s code spawns onto the shared sheet of the sets whose rooms run it.</summary>
    private static readonly EnemyProjectileFrameId[] CommonOldTourianEscapeShaftWallBehaviorProjectiles =
    [
        EnemyProjectileFrameId.OldTourianWallExplosionSpark,
        EnemyProjectileFrameId.OldTourianWallExplosionFireballSmall,
        EnemyProjectileFrameId.OldTourianWallExplosionFireballLarge,
        EnemyProjectileFrameId.OldTourianWallExplosionFlamingRing,
        EnemyProjectileFrameId.OldTourianWallExplosionStarburst,
        EnemyProjectileFrameId.OldTourianWallExplosionEmbers,
    ];

    /// <summary>The room effect actors <c>OldTourianEscapeShaftWallBehavior</c>'s code spawns onto the shared sheet of the sets whose rooms run it.</summary>
    private static readonly RoomEffectActorKind[] CommonOldTourianEscapeShaftWallBehaviorEffectActors = [];

    /// <summary>The projectile frames <c>RidleyProjectiles</c>'s code spawns onto the shared sheet of the sets whose rooms run it.</summary>
    private static readonly EnemyProjectileFrameId[] CommonRidleyProjectilesProjectiles =
    [
        EnemyProjectileFrameId.RidleyFireballSpark,
        EnemyProjectileFrameId.RidleyFireballFireCloud,
        EnemyProjectileFrameId.RidleyFireballFireCloudSpreading,
        EnemyProjectileFrameId.RidleyFireballFireCloudBreaking,
        EnemyProjectileFrameId.RidleyFireballFireCloudEmbers,
    ];

    /// <summary>The room effect actors <c>RidleyProjectiles</c>'s code spawns onto the shared sheet of the sets whose rooms run it.</summary>
    private static readonly RoomEffectActorKind[] CommonRidleyProjectilesEffectActors = [];

    /// <summary>The projectile frames <c>SaveStationElectricity</c>'s code spawns onto the shared sheet of the sets whose rooms run it.</summary>
    private static readonly EnemyProjectileFrameId[] CommonSaveStationElectricityProjectiles =
    [
        EnemyProjectileFrameId.SaveStationElectricityBand,
    ];

    /// <summary>The room effect actors <c>SaveStationElectricity</c>'s code spawns onto the shared sheet of the sets whose rooms run it.</summary>
    private static readonly RoomEffectActorKind[] CommonSaveStationElectricityEffectActors = [];

    /// <summary>What each enemy definition can spawn onto its own spawned-object sheet.</summary>
    internal static IReadOnlyDictionary<EnemyDefinitionId, SpawnedObjects> ByEnemy { get; } =
        new Dictionary<EnemyDefinitionId, SpawnedObjects>
        {
            [EnemyDefinitionId.Botwoon] = new(BotwoonProjectiles, BotwoonEffectActors),
            [EnemyDefinitionId.Crocomire] = new(CrocomireProjectiles, CrocomireEffectActors),
            [EnemyDefinitionId.CrocomireTongue] = new(CrocomireProjectiles, CrocomireEffectActors),
            [EnemyDefinitionId.FakeKraid] = new(FakeKraidProjectiles, FakeKraidEffectActors),
            [EnemyDefinitionId.PhantoonBody] = new(PhantoonProjectiles, PhantoonEffectActors),
            [EnemyDefinitionId.PhantoonEye] = new(PhantoonProjectiles, PhantoonEffectActors),
            [EnemyDefinitionId.PhantoonTentacles] = new(PhantoonProjectiles, PhantoonEffectActors),
            [EnemyDefinitionId.PhantoonMouth] = new(PhantoonProjectiles, PhantoonEffectActors),
            [EnemyDefinitionId.SporeSpawn] = new(SporeSpawnProjectiles, SporeSpawnEffectActors),
            [EnemyDefinitionId.BombTorizo] = new(TorizoProjectiles, TorizoEffectActors),
            [EnemyDefinitionId.GoldenTorizo] = new(TorizoProjectiles, TorizoEffectActors),
            [EnemyDefinitionId.Alcoon] = new(AlcoonProjectiles, AlcoonEffectActors),
            [EnemyDefinitionId.Cacatac] = new(CacatacProjectiles, CacatacEffectActors),
            [EnemyDefinitionId.Dragon] = new(DragonProjectiles, DragonEffectActors),
            [EnemyDefinitionId.Namihe] = new(FuneNamiheProjectiles, FuneNamiheEffectActors),
            [EnemyDefinitionId.Fune] = new(FuneNamiheProjectiles, FuneNamiheEffectActors),
            [EnemyDefinitionId.Kago] = new(KagoProjectiles, KagoEffectActors),
            [EnemyDefinitionId.KihunterGreen] = new(KiHunterProjectiles, KiHunterEffectActors),
            [EnemyDefinitionId.KihunterRed] = new(KiHunterProjectiles, KiHunterEffectActors),
            [EnemyDefinitionId.KihunterYellow] = new(KiHunterProjectiles, KiHunterEffectActors),
            [EnemyDefinitionId.KihunterGreenWings] = new(KiHunterProjectiles, KiHunterEffectActors),
            [EnemyDefinitionId.KihunterRedWings] = new(KiHunterProjectiles, KiHunterEffectActors),
            [EnemyDefinitionId.KihunterYellowWings] = new(KiHunterProjectiles, KiHunterEffectActors),
            [EnemyDefinitionId.Magdollite] = new(MagdolliteProjectiles, MagdolliteEffectActors),
            [EnemyDefinitionId.Metaree] = new(MetareeProjectiles, MetareeEffectActors),
            [EnemyDefinitionId.Metroid] = new(MetroidProjectiles, MetroidEffectActors),
            [EnemyDefinitionId.NuclearWaffle] = new(NuclearWaffleProjectiles, NuclearWaffleEffectActors),
            [EnemyDefinitionId.Polyp] = new(PolypProjectiles, PolypEffectActors),
            [EnemyDefinitionId.Powamp] = new(PowampProjectiles, PowampEffectActors),
            [EnemyDefinitionId.Skree] = new(SkreeProjectiles, SkreeEffectActors),
            [EnemyDefinitionId.PirateGoldNinja] = new(NinjaSpacePirateProjectiles, NinjaSpacePirateEffectActors),
            [EnemyDefinitionId.PirateSilverNinja] = new(NinjaSpacePirateProjectiles, NinjaSpacePirateEffectActors),
            [EnemyDefinitionId.Spark] = new(SparkProjectiles, SparkEffectActors),
            [EnemyDefinitionId.YappingMaw] = new(YappingMawProjectiles, YappingMawEffectActors),
        };

    /// <summary>What each enemy definition can spawn onto the shared sheet of the graphics sets holding it.</summary>
    internal static IReadOnlyDictionary<EnemyDefinitionId, SpawnedObjects> SharedByEnemy { get; } =
        new Dictionary<EnemyDefinitionId, SpawnedObjects>
        {
            [EnemyDefinitionId.CeresElevatorDoor] = new(CeresElevatorDoorSharedProjectiles, CeresElevatorDoorSharedEffectActors),
            [EnemyDefinitionId.SteamVent] = new(CeresSteamSharedProjectiles, CeresSteamSharedEffectActors),
            [EnemyDefinitionId.TourianStatue] = new(TourianEntranceStatueSharedProjectiles, TourianEntranceStatueSharedEffectActors),
            [EnemyDefinitionId.Botwoon] = new(BotwoonSharedProjectiles, BotwoonSharedEffectActors),
            [EnemyDefinitionId.Crocomire] = new(CrocomireSharedProjectiles, CrocomireSharedEffectActors),
            [EnemyDefinitionId.CrocomireTongue] = new(CrocomireSharedProjectiles, CrocomireSharedEffectActors),
            [EnemyDefinitionId.DraygonBody] = new(DraygonSharedProjectiles, DraygonSharedEffectActors),
            [EnemyDefinitionId.DraygonEye] = new(DraygonSharedProjectiles, DraygonSharedEffectActors),
            [EnemyDefinitionId.DraygonTail] = new(DraygonSharedProjectiles, DraygonSharedEffectActors),
            [EnemyDefinitionId.DraygonArms] = new(DraygonSharedProjectiles, DraygonSharedEffectActors),
            [EnemyDefinitionId.FakeKraid] = new(FakeKraidSharedProjectiles, FakeKraidSharedEffectActors),
            [EnemyDefinitionId.Kraid] = new(KraidSharedProjectiles, KraidSharedEffectActors),
            [EnemyDefinitionId.KraidArm] = new(KraidSharedProjectiles, KraidSharedEffectActors),
            [EnemyDefinitionId.KraidFoot] = new(KraidSharedProjectiles, KraidSharedEffectActors),
            [EnemyDefinitionId.KraidLintBottom] = new(KraidSharedProjectiles, KraidSharedEffectActors),
            [EnemyDefinitionId.KraidLintMiddle] = new(KraidSharedProjectiles, KraidSharedEffectActors),
            [EnemyDefinitionId.KraidLintTop] = new(KraidSharedProjectiles, KraidSharedEffectActors),
            [EnemyDefinitionId.KraidFirstNail] = new(KraidSharedProjectiles, KraidSharedEffectActors),
            [EnemyDefinitionId.KraidSecondNail] = new(KraidSharedProjectiles, KraidSharedEffectActors),
            [EnemyDefinitionId.BabyMetroid] = new(BabyMetroidSharedProjectiles, BabyMetroidSharedEffectActors),
            [EnemyDefinitionId.BabyMetroidCutscene] = new(BabyMetroidCutsceneSharedProjectiles, BabyMetroidCutsceneSharedEffectActors),
            [EnemyDefinitionId.MotherBrainBody] = new(MotherBrainBodySharedProjectiles, MotherBrainBodySharedEffectActors),
            [EnemyDefinitionId.MotherBrainHead] = new(MotherBrainBodySharedProjectiles, MotherBrainBodySharedEffectActors),
            [EnemyDefinitionId.MotherBrainTubes] = new(MotherBrainBodySharedProjectiles, MotherBrainBodySharedEffectActors),
            [EnemyDefinitionId.Rinka] = new(RinkaSharedProjectiles, RinkaSharedEffectActors),
            [EnemyDefinitionId.Zebetite] = new(ZebetiteSharedProjectiles, ZebetiteSharedEffectActors),
            [EnemyDefinitionId.PhantoonBody] = new(PhantoonSharedProjectiles, PhantoonSharedEffectActors),
            [EnemyDefinitionId.PhantoonEye] = new(PhantoonSharedProjectiles, PhantoonSharedEffectActors),
            [EnemyDefinitionId.PhantoonTentacles] = new(PhantoonSharedProjectiles, PhantoonSharedEffectActors),
            [EnemyDefinitionId.PhantoonMouth] = new(PhantoonSharedProjectiles, PhantoonSharedEffectActors),
            [EnemyDefinitionId.RidleyCeres] = new(CeresRidleyEntitySharedProjectiles, CeresRidleyEntitySharedEffectActors),
            [EnemyDefinitionId.Ridley] = new(RidleyEntitySharedProjectiles, RidleyEntitySharedEffectActors),
            [EnemyDefinitionId.RidleyBreakupPiece] = new(RidleyEntitySharedProjectiles, RidleyEntitySharedEffectActors),
            [EnemyDefinitionId.SporeSpawn] = new(SporeSpawnSharedProjectiles, SporeSpawnSharedEffectActors),
            [EnemyDefinitionId.BombTorizo] = new(TorizoSharedProjectiles, TorizoSharedEffectActors),
            [EnemyDefinitionId.GoldenTorizo] = new(TorizoSharedProjectiles, TorizoSharedEffectActors),
            [EnemyDefinitionId.Beetom] = new(BeetomSharedProjectiles, BeetomSharedEffectActors),
            [EnemyDefinitionId.Boulder] = new(BoulderSharedProjectiles, BoulderSharedEffectActors),
            [EnemyDefinitionId.Chozo] = new(ChozoStatueSharedProjectiles, ChozoStatueSharedEffectActors),
            [EnemyDefinitionId.CorpseSidehopper] = new(DeadSidehopperSharedProjectiles, DeadSidehopperSharedEffectActors),
            [EnemyDefinitionId.CorpseTorizo] = new(DeadTorizoSharedProjectiles, DeadTorizoSharedEffectActors),
            [EnemyDefinitionId.CorpseRipper] = new(DeadTourianSharedProjectiles, DeadTourianSharedEffectActors),
            [EnemyDefinitionId.CorpseSkree] = new(DeadTourianSharedProjectiles, DeadTourianSharedEffectActors),
            [EnemyDefinitionId.CorpseZoomer] = new(DeadTourianSharedProjectiles, DeadTourianSharedEffectActors),
            [EnemyDefinitionId.Dragon] = new(DragonSharedProjectiles, DragonSharedEffectActors),
            [EnemyDefinitionId.Evir] = new(EvirSharedProjectiles, EvirSharedEffectActors),
            [EnemyDefinitionId.EvirProjectile] = new(EvirSharedProjectiles, EvirSharedEffectActors),
            [EnemyDefinitionId.Fireflea] = new(FirefleaSharedProjectiles, FirefleaSharedEffectActors),
            [EnemyDefinitionId.ShipTop] = new(GunshipSharedProjectiles, GunshipSharedEffectActors),
            [EnemyDefinitionId.ShipBottomEntrance] = new(GunshipSharedProjectiles, GunshipSharedEffectActors),
            [EnemyDefinitionId.Kago] = new(KagoSharedProjectiles, KagoSharedEffectActors),
            [EnemyDefinitionId.Magdollite] = new(MagdolliteSharedProjectiles, MagdolliteSharedEffectActors),
            [EnemyDefinitionId.MamaTurtle] = new(MamaTurtleSharedProjectiles, MamaTurtleSharedEffectActors),
            [EnemyDefinitionId.BabyTurtle] = new(MamaTurtleSharedProjectiles, MamaTurtleSharedEffectActors),
            [EnemyDefinitionId.Oum] = new(OumSharedProjectiles, OumSharedEffectActors),
            [EnemyDefinitionId.Metroid] = new(MetroidSharedProjectiles, MetroidSharedEffectActors),
            [EnemyDefinitionId.Mochtroid] = new(MochtroidSharedProjectiles, MochtroidSharedEffectActors),
            [EnemyDefinitionId.NoobTubeCrack] = new(NoobTubeSharedProjectiles, NoobTubeSharedEffectActors),
            [EnemyDefinitionId.Powamp] = new(PowampSharedProjectiles, PowampSharedEffectActors),
            [EnemyDefinitionId.Shaktool] = new(ShaktoolSharedProjectiles, ShaktoolSharedEffectActors),
            [EnemyDefinitionId.ShutterGrowing] = new(ShuttersSharedProjectiles, ShuttersSharedEffectActors),
            [EnemyDefinitionId.ShutterShootable] = new(ShuttersSharedProjectiles, ShuttersSharedEffectActors),
            [EnemyDefinitionId.Kamer] = new(ShuttersSharedProjectiles, ShuttersSharedEffectActors),
            [EnemyDefinitionId.ShutterDestroyable] = new(ShuttersSharedProjectiles, ShuttersSharedEffectActors),
            [EnemyDefinitionId.Skree] = new(SkreeSharedProjectiles, SkreeSharedEffectActors),
            [EnemyDefinitionId.PirateGoldNinja] = new(NinjaSpacePirateSharedProjectiles, NinjaSpacePirateSharedEffectActors),
            [EnemyDefinitionId.PirateSilverNinja] = new(NinjaSpacePirateSharedProjectiles, NinjaSpacePirateSharedEffectActors),
            [EnemyDefinitionId.PirateGoldWalking] = new(WalkingSpacePirateSharedProjectiles, WalkingSpacePirateSharedEffectActors),
            [EnemyDefinitionId.PirateGreenWalking] = new(WalkingSpacePirateSharedProjectiles, WalkingSpacePirateSharedEffectActors),
            [EnemyDefinitionId.PirateGreyWalking] = new(WalkingSpacePirateSharedProjectiles, WalkingSpacePirateSharedEffectActors),
            [EnemyDefinitionId.PirateMagentaWalking] = new(WalkingSpacePirateSharedProjectiles, WalkingSpacePirateSharedEffectActors),
            [EnemyDefinitionId.PirateRedWalking] = new(WalkingSpacePirateSharedProjectiles, WalkingSpacePirateSharedEffectActors),
            [EnemyDefinitionId.PirateSilverWalking] = new(WalkingSpacePirateSharedProjectiles, WalkingSpacePirateSharedEffectActors),
            [EnemyDefinitionId.PirateGoldWall] = new(WallSpacePirateSharedProjectiles, WallSpacePirateSharedEffectActors),
            [EnemyDefinitionId.PirateGreyWall] = new(WallSpacePirateSharedProjectiles, WallSpacePirateSharedEffectActors),
            [EnemyDefinitionId.PirateMagentaWall] = new(WallSpacePirateSharedProjectiles, WallSpacePirateSharedEffectActors),
            [EnemyDefinitionId.PirateSilverWall] = new(WallSpacePirateSharedProjectiles, WallSpacePirateSharedEffectActors),
            [EnemyDefinitionId.WorkRobot] = new(WorkRobotSharedProjectiles, WorkRobotSharedEffectActors),
            [EnemyDefinitionId.UnpoweredWorkRobot] = new(WorkRobotSharedProjectiles, WorkRobotSharedEffectActors),
            [EnemyDefinitionId.YappingMaw] = new(YappingMawSharedProjectiles, YappingMawSharedEffectActors),
            [EnemyDefinitionId.Yard] = new(YardEnemySharedProjectiles, YardEnemySharedEffectActors),
        };

    /// <summary>What each sheet-specific owner's sheet holds: only what is spawned with graphics naming that owner.</summary>
    internal static IReadOnlyDictionary<SpawnedObjectOwner, SpawnedObjects> BySheet { get; } =
        new Dictionary<SpawnedObjectOwner, SpawnedObjects>
        {
            [SpawnedObjectOwner.MotherBrainTankGlass] = new(MotherBrainTankGlassSheetProjectiles, MotherBrainTankGlassSheetEffectActors),
        };

    /// <summary>
    /// What each common spawner, code outside the enemy behaviours, spawns onto the shared sheet, by the spawner the import
    /// places in the rooms whose data runs it, named for a type, or for a type and member where a type's members serve unrelated rooms.
    /// </summary>
    internal static IReadOnlyList<CommonSpawnedObjects> Common { get; } =
    [
        new(CommonSpawner.CeresFallingDebrisMain, new(CommonCeresFallingDebrisMainProjectiles, CommonCeresFallingDebrisMainEffectActors)),
        new(CommonSpawner.DownwardGateBehavior, new(CommonDownwardGateBehaviorProjectiles, CommonDownwardGateBehaviorEffectActors)),
        new(CommonSpawner.EnemyDeathsAndDrops, new(CommonEnemyDeathsAndDropsProjectiles, CommonEnemyDeathsAndDropsEffectActors)),
        new(CommonSpawner.EnemyProjectileEngineCreateBombDudShot, new(CommonEnemyProjectileEngineCreateBombDudShotProjectiles, CommonEnemyProjectileEngineCreateBombDudShotEffectActors)),
        new(CommonSpawner.EnemyProjectileEngineCreateEnemyProjectileDudShot, new(CommonEnemyProjectileEngineCreateEnemyProjectileDudShotProjectiles, CommonEnemyProjectileEngineCreateEnemyProjectileDudShotEffectActors)),
        new(CommonSpawner.EnemyProjectileEngineSpawnDustOrExplosion, new(CommonEnemyProjectileEngineSpawnDustOrExplosionProjectiles, CommonEnemyProjectileEngineSpawnDustOrExplosionEffectActors)),
        new(CommonSpawner.EscapeExplosionsBehavior, new(CommonEscapeExplosionsBehaviorProjectiles, CommonEscapeExplosionsBehaviorEffectActors)),
        new(CommonSpawner.EyeDoorBehavior, new(CommonEyeDoorBehaviorProjectiles, CommonEyeDoorBehaviorEffectActors)),
        new(CommonSpawner.OldTourianEscapeShaftWallBehavior, new(CommonOldTourianEscapeShaftWallBehaviorProjectiles, CommonOldTourianEscapeShaftWallBehaviorEffectActors)),
        new(CommonSpawner.RidleyProjectiles, new(CommonRidleyProjectilesProjectiles, CommonRidleyProjectilesEffectActors)),
        new(CommonSpawner.SaveStationElectricity, new(CommonSaveStationElectricityProjectiles, CommonSaveStationElectricityEffectActors)),
    ];
}
