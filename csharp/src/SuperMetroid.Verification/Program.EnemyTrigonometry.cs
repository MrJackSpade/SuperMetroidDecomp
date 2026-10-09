using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend;

internal static partial class Program
{
    /// <summary>Checks compiled enemy trigonometry and related vector calculations against the pinned retail sine tables across their authored byte and word inputs.</summary>
    private static void VerifyCompiledEnemyTrigonometry()
    {
        Suite(nameof(VerifyRidleyPowerBombNeutralTail), () => VerifyRidleyPowerBombNeutralTail());
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var bytes = new byte[128];
        var words = new ushort[128];
        for (int i = 0; i < 128; i++)
        {
            bytes[i] = rom.ReadByte(EnemyMathReferenceData.ByteSine + i);
            int address = EnemyMathReferenceData.UnsignedSine + i * 2;
            words[i] = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        }
        Suite(nameof(VerifyEightBitHalfWaveAlgorithm), () => VerifyEightBitHalfWaveAlgorithm(rom));
        Suite(nameof(VerifyUnsignedHalfWaveAlgorithm), () => VerifyUnsignedHalfWaveAlgorithm(rom));
        Suite(nameof(VerifyCompiledSignedTrigonometry), () => VerifyCompiledSignedTrigonometry(rom));
        Suite(nameof(VerifyBombTorizoDroolSine), () => VerifyBombTorizoDroolSine(rom));
        Suite(nameof(VerifyMotherBrainNeckSine), () => VerifyMotherBrainNeckSine(rom));
        Suite(nameof(VerifyCompiledGrappleMath), () => VerifyCompiledGrappleMath(rom));
        Suite(nameof(VerifyCompiledProjectileMath), () => VerifyCompiledProjectileMath(rom));
        Suite(nameof(VerifyBeamCallbackTables), () => VerifyBeamCallbackTables());
        Suite(nameof(VerifyCompiledFamilyTrigonometry), () => VerifyCompiledFamilyTrigonometry(rom));
        Suite(nameof(VerifyPhantoonWaveMath), () => VerifyPhantoonWaveMath(rom));
        Suite(nameof(VerifyCompiledLinearEnemySpeeds), () => VerifyCompiledLinearEnemySpeeds(rom));
        Suite(nameof(VerifyFirefleaMovementDefinitions), () => VerifyFirefleaMovementDefinitions(rom));
        Suite(nameof(VerifyCacatacMovementDefinitions), () => VerifyCacatacMovementDefinitions(rom));
        Suite(nameof(VerifyCacatacProjectileDefinitions), () => VerifyCacatacProjectileDefinitions(rom));
        Suite(nameof(VerifyCacatacProjectileInstructionProgramDefinitions), () => VerifyCacatacProjectileInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyFallingSparkInstructionProgramDefinitions), () => VerifyFallingSparkInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyFuneNamiheFireballInstructionProgramDefinitions), () => VerifyFuneNamiheFireballInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyMagdolliteLavaInstructionProgramDefinitions), () => VerifyMagdolliteLavaInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyDragonFireballInstructionProgramDefinitions), () => VerifyDragonFireballInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyEyeDoorProjectileInstructionProgramDefinitions), () => VerifyEyeDoorProjectileInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyEyeDoorSweatInstructionProgramDefinitions), () => VerifyEyeDoorSweatInstructionProgramDefinitions(rom));
        Suite(nameof(VerifySkreeMetareeParticleInstructionProgramDefinitions), () => VerifySkreeMetareeParticleInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyKraidRockProjectileInstructionProgramDefinitions), () => VerifyKraidRockProjectileInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyFakeKraidProjectileInstructionProgramDefinitions), () => VerifyFakeKraidProjectileInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyAlcoonFireballInstructionProgramDefinitions), () => VerifyAlcoonFireballInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyWorkRobotLaserInstructionProgramDefinitions), () => VerifyWorkRobotLaserInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyPowampSpikeInstructionProgramDefinitions), () => VerifyPowampSpikeInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyPolypRockInstructionProgramDefinitions), () => VerifyPolypRockInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyKiHunterAcidSpitInstructionProgramDefinitions), () => VerifyKiHunterAcidSpitInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyStokeProjectileInstructionProgramDefinitions), () => VerifyStokeProjectileInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyNuclearWaffleProjectileInstructionProgramDefinitions), () => VerifyNuclearWaffleProjectileInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyKagoBugProjectileInstructionProgramDefinitions), () => VerifyKagoBugProjectileInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyYappingMawBodyProjectileInstructionProgramDefinitions), () => VerifyYappingMawBodyProjectileInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyCrocomireProjectileInstructionProgramDefinitions), () => VerifyCrocomireProjectileInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyPhantoonProjectileInstructionProgramDefinitions), () => VerifyPhantoonProjectileInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyDraygonProjectileInstructionProgramDefinitions), () => VerifyDraygonProjectileInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyCeresRidleyProjectileInstructionProgramDefinitions), () => VerifyCeresRidleyProjectileInstructionProgramDefinitions(rom));
        Suite(nameof(VerifySpacePirateProjectileInstructionProgramDefinitions), () => VerifySpacePirateProjectileInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyCeresFallingDebrisInstructionProgramDefinitions), () => VerifyCeresFallingDebrisInstructionProgramDefinitions(rom));
        Suite(nameof(VerifySaveStationElectricityInstructionProgramDefinitions), () => VerifySaveStationElectricityInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyEnemyPickupInstructionProgramDefinitions), () => VerifyEnemyPickupInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyEnemyDeathInstructionProgramDefinitions), () => VerifyEnemyDeathInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyShaktoolProjectileInstructionProgramDefinitions), () => VerifyShaktoolProjectileInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyChozoTourianDustInstructionProgramDefinitions), () => VerifyChozoTourianDustInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyTourianStatueProjectileInstructionProgramDefinitions), () => VerifyTourianStatueProjectileInstructionProgramDefinitions(rom));
        Suite(nameof(VerifySporeSpawnProjectileInstructionProgramDefinitions), () => VerifySporeSpawnProjectileInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyBotwoonProjectileInstructionProgramDefinitions), () => VerifyBotwoonProjectileInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyTorizoLandingDustInstructionProgramDefinitions), () => VerifyTorizoLandingDustInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyTorizoExplosiveSwipeInstructionProgramDefinitions), () => VerifyTorizoExplosiveSwipeInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyBombTorizoDroolInstructionProgramDefinitions), () => VerifyBombTorizoDroolInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyTorizoExplosionInstructionProgramDefinitions), () => VerifyTorizoExplosionInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyTorizoChozoOrbInstructionProgramDefinitions), () => VerifyTorizoChozoOrbInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyTorizoSonicBoomInstructionProgramDefinitions), () => VerifyTorizoSonicBoomInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyBombTorizoStatueInstructionProgramDefinitions), () => VerifyBombTorizoStatueInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyGoldenTorizoEggInstructionProgramDefinitions), () => VerifyGoldenTorizoEggInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyGoldenTorizoSuperMissileInstructionProgramDefinitions), () => VerifyGoldenTorizoSuperMissileInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyGoldenTorizoEyeBeamInstructionProgramDefinitions), () => VerifyGoldenTorizoEyeBeamInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyEnemyProjectileInstructionOwnerCoverage), () => VerifyEnemyProjectileInstructionOwnerCoverage());
        Suite(nameof(VerifyGunshipInstructionProgramDefinitions), () => VerifyGunshipInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyRidleyInstructionProgramDefinitions), () => VerifyRidleyInstructionProgramDefinitions());
        Suite(nameof(VerifyDraygonInstructionProgramDefinitions), () => VerifyDraygonInstructionProgramDefinitions());
        Suite(nameof(VerifyWalkingSpacePirateInstructionProgramDefinitions), () => VerifyWalkingSpacePirateInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyWallSpacePirateInstructionProgramDefinitions), () => VerifyWallSpacePirateInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyNinjaSpacePirateInstructionProgramDefinitions), () => VerifyNinjaSpacePirateInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyMotherBrainBodyInstructionPrograms), () => VerifyMotherBrainBodyInstructionPrograms());
        Suite(nameof(VerifyMotherBrainRoomPaletteProgramDefinitions), () => VerifyMotherBrainRoomPaletteProgramDefinitions());
        Suite(nameof(VerifyEnemyInstructionOwnerCoverage), () => VerifyEnemyInstructionOwnerCoverage());
        Suite(nameof(VerifyDownwardGateProjectileInstructionProgramDefinitions), () => VerifyDownwardGateProjectileInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyNoobTubeProjectileInstructionProgramDefinitions), () => VerifyNoobTubeProjectileInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyMotherBrainTopTubeInstructionProgramDefinitions), () => VerifyMotherBrainTopTubeInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyMotherBrainGlassInstructionProgramDefinitions), () => VerifyMotherBrainGlassInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyMotherBrainTurretInstructionProgramDefinitions), () => VerifyMotherBrainTurretInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyGunshipDustInstructionProgramDefinitions), () => VerifyGunshipDustInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyCacatacInstructionProgramDefinitions), () => VerifyCacatacInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyMagdolliteInstructionProgramDefinitions), () => VerifyMagdolliteInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyKiHunterInstructionProgramDefinitions), () => VerifyKiHunterInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyOwtchInstructionProgramDefinitions), () => VerifyOwtchInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyStokeInstructionProgramDefinitions), () => VerifyStokeInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyRipperInstructionProgramDefinitions), () => VerifyRipperInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyKzanInstructionProgramDefinitions), () => VerifyKzanInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyFlyInstructionProgramDefinitions), () => VerifyFlyInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyBullInstructionProgramDefinitions), () => VerifyBullInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyKagoInstructionProgramDefinitions), () => VerifyKagoInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyHorizontalShutterInstructionProgramDefinitions), () => VerifyHorizontalShutterInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyGrowingShutterInstructionProgramDefinitions), () => VerifyGrowingShutterInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyVerticalShutterInstructionProgramDefinitions), () => VerifyVerticalShutterInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyChootInstructionProgramDefinitions), () => VerifyChootInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyNorfairLavaJumperInstructionProgramDefinitions), () => VerifyNorfairLavaJumperInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyBeetomInstructionProgramDefinitions), () => VerifyBeetomInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyAlcoonInstructionProgramDefinitions), () => VerifyAlcoonInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyMultiviolaInstructionProgramDefinitions), () => VerifyMultiviolaInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyPolypInstructionProgramDefinitions), () => VerifyPolypInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyPowampInstructionProgramDefinitions), () => VerifyPowampInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyWreckedShipGhostInstructionProgramDefinitions), () => VerifyWreckedShipGhostInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyPuyoInstructionProgramDefinitions), () => VerifyPuyoInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyDeadTorizoInstructionProgramDefinitions), () => VerifyDeadTorizoInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyDeadSidehopperInstructionProgramDefinitions), () => VerifyDeadSidehopperInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyDeadTourianCorpseInstructionProgramDefinitions), () => VerifyDeadTourianCorpseInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyShitroidInstructionProgramDefinitions), () => VerifyShitroidInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyRioInstructionProgramDefinitions), () => VerifyRioInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyAtomicMovementDefinitions), () => VerifyAtomicMovementDefinitions(rom));
        Suite(nameof(VerifySbugMovementDefinitions), () => VerifySbugMovementDefinitions(rom));
        Suite(nameof(VerifySparkMovementDefinitions), () => VerifySparkMovementDefinitions(rom));
        Suite(nameof(VerifyElevatorInputDefinitions), () => VerifyElevatorInputDefinitions(rom));
        Suite(nameof(VerifyOwtchMovementDefinitions), () => VerifyOwtchMovementDefinitions(rom));
        Suite(nameof(VerifyNuclearWaffleDefinitions), () => VerifyNuclearWaffleDefinitions(rom));
        Suite(nameof(VerifyHibashiDefinitions), () => VerifyHibashiDefinitions(rom));
        Suite(nameof(VerifyBlueBrinstarFaceBlockInstructionProgramDefinitions), () => VerifyBlueBrinstarFaceBlockInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyFuneNamiheDefinitions), () => VerifyFuneNamiheDefinitions(rom));
        Suite(nameof(VerifyFuneNamiheInstructionProgramDefinitions), () => VerifyFuneNamiheInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyMiscDustProjectileDefinitions), () => VerifyMiscDustProjectileDefinitions(rom));
        Suite(nameof(VerifyHopperAnimationDefinitions), () => VerifyHopperAnimationDefinitions(rom));
        Suite(nameof(VerifyChootPatternDefinitions), () => VerifyChootPatternDefinitions(rom));
        Suite(nameof(VerifyCrawlerAnimationDefinitions), () => VerifyCrawlerAnimationDefinitions(rom));
        Suite(nameof(VerifyHZoomerInstructionProgramDefinitions), () => VerifyHZoomerInstructionProgramDefinitions(rom));
        Suite(nameof(VerifySciserInstructionProgramDefinitions), () => VerifySciserInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyZeroInstructionProgramDefinitions), () => VerifyZeroInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyViolaInstructionProgramDefinitions), () => VerifyViolaInstructionProgramDefinitions(rom));
        Suite(nameof(VerifySharedCrawlerInstructionProgramDefinitions), () => VerifySharedCrawlerInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyDachoraInstructionProgramDefinitions), () => VerifyDachoraInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyFirefleaInstructionProgramDefinitions), () => VerifyFirefleaInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyZebetiteInstructionProgramDefinitions), () => VerifyZebetiteInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyEvirInstructionProgramDefinitions), () => VerifyEvirInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyMorphBallEyeInstructionProgramDefinitions), () => VerifyMorphBallEyeInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyYappingMawInstructionProgramDefinitions), () => VerifyYappingMawInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyMetroidInstructionProgramDefinitions), () => VerifyMetroidInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyNorfairRioInstructionProgramDefinitions), () => VerifyNorfairRioInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyLowerNorfairRioInstructionProgramDefinitions), () => VerifyLowerNorfairRioInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyMamaTurtleInstructionProgramDefinitions), () => VerifyMamaTurtleInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyTourianEntranceStatueInstructionProgramDefinitions), () => VerifyTourianEntranceStatueInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyKraidNailInstructionProgramDefinitions), () => VerifyKraidNailInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyKraidArmInstructionProgramDefinitions), () => VerifyKraidArmInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyKraidFootInstructionProgramDefinitions), () => VerifyKraidFootInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyKraidLintInstructionProgramDefinitions), () => VerifyKraidLintInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyPhantoonInstructionProgramDefinitions), () => VerifyPhantoonInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyWorkRobotInstructionProgramDefinitions), () => VerifyWorkRobotInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyYardInstructionProgramDefinitions), () => VerifyYardInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyNorfairLavaJumpDefinitions), () => VerifyNorfairLavaJumpDefinitions(rom));
        Suite(nameof(VerifyCompiledQuadraticEnemySpeeds), () => VerifyCompiledQuadraticEnemySpeeds(rom));
        Suite(nameof(VerifyCompiledBullMovement), () => VerifyCompiledBullMovement(rom));
        Suite(nameof(VerifyPowerBombCallbackDefinitions), () => VerifyPowerBombCallbackDefinitions(rom));
        Suite(nameof(VerifyShotCallbackDefinitions), () => VerifyShotCallbackDefinitions(rom));
        Suite(nameof(VerifyCompiledPuyoHops), () => VerifyCompiledPuyoHops(rom));
        Suite(nameof(VerifyCompiledBotwoonSpeeds), () => VerifyCompiledBotwoonSpeeds(rom));
        Suite(nameof(VerifyBotwoonNavigationDefinitions), () => VerifyBotwoonNavigationDefinitions(rom));
        Suite(nameof(VerifyBotwoonInstructionDefinitions), () => VerifyBotwoonInstructionDefinitions(rom));
        Suite(nameof(VerifyBotwoonInstructionProgramDefinitions), () => VerifyBotwoonInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyBotwoonHealthPaletteDefinitions), () => VerifyBotwoonHealthPaletteDefinitions(rom));
        Suite(nameof(VerifyCompiledCrawlerSpeeds), () => VerifyCompiledCrawlerSpeeds(rom));
        Suite(nameof(VerifyCompiledPolypLaunchDefinitions), () => VerifyCompiledPolypLaunchDefinitions(rom));
        Suite(nameof(VerifyShaktoolSegmentDefinitions), () => VerifyShaktoolSegmentDefinitions(rom));
        Suite(nameof(VerifyShaktoolInstructionDefinitions), () => VerifyShaktoolInstructionDefinitions(rom));
        Suite(nameof(VerifyShaktoolInstructionProgramDefinitions), () => VerifyShaktoolInstructionProgramDefinitions(rom));
        Suite(nameof(VerifySporeSpawnProjectileDefinitions), () => VerifySporeSpawnProjectileDefinitions(rom));
        Suite(nameof(VerifySamusAtmosphericEffectDefinitions), () => VerifySamusAtmosphericEffectDefinitions(rom));
        Suite(nameof(VerifySamusAtmosphericAnimationDefinitions), () => VerifySamusAtmosphericAnimationDefinitions(rom));
        Suite(nameof(VerifyCrystalFlashPaletteTimingDefinitions), () => VerifyCrystalFlashPaletteTimingDefinitions(rom));
        Suite(nameof(VerifyWorkRobotPaletteTimingDefinitions), () => VerifyWorkRobotPaletteTimingDefinitions(rom));
        Suite(nameof(VerifySamusDeathExplosionTimingDefinitions), () => VerifySamusDeathExplosionTimingDefinitions(rom));
        Suite(nameof(VerifyHyperBeamPaletteFxProgramDefinitions), () => VerifyHyperBeamPaletteFxProgramDefinitions(rom));
        Suite(nameof(VerifySuitPickupBeamCurveDefinitions), () => VerifySuitPickupBeamCurveDefinitions(rom));
        Suite(nameof(VerifyQuicksandDefinitions), () => VerifyQuicksandDefinitions(rom));
        Suite(nameof(VerifySaveStationAnimationDefinitions), () => VerifySaveStationAnimationDefinitions(rom));
        Suite(nameof(VerifyZebesEscapeExplosionDefinitions), () => VerifyZebesEscapeExplosionDefinitions(rom));
        Suite(nameof(VerifyMetroidBehaviorDefinitions), () => VerifyMetroidBehaviorDefinitions(rom));
        Suite(nameof(VerifyMotherBrainDoorFragmentDefinitions), () => VerifyMotherBrainDoorFragmentDefinitions(rom));
        Suite(nameof(VerifyMotherBrainTurretDefinitions), () => VerifyMotherBrainTurretDefinitions(rom));
        Suite(nameof(VerifyMotherBrainGlassShardDefinitions), () => VerifyMotherBrainGlassShardDefinitions(rom));
        Suite(nameof(VerifyTourianStatueUnlockDefinitions), () => VerifyTourianStatueUnlockDefinitions(rom));
        Suite(nameof(VerifyTourianAccessPlmDefinitions), () => VerifyTourianAccessPlmDefinitions(rom));
        Suite(nameof(VerifyChozoStatuePlmDefinitions), () => VerifyChozoStatuePlmDefinitions(rom));
        Suite(nameof(VerifySamusEaterPlmDefinitions), () => VerifySamusEaterPlmDefinitions(rom));
        Suite(nameof(VerifyStationAccessPlmDefinitions), () => VerifyStationAccessPlmDefinitions(rom));
        Suite(nameof(VerifyMaridiaLargeSnailInstructionDefinitions), () => VerifyMaridiaLargeSnailInstructionDefinitions(rom));
        Suite(nameof(VerifyEtecoonInstructionProgramDefinitions), () => VerifyEtecoonInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyElevatorInstructionProgramDefinitions), () => VerifyElevatorInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyMochtroidInstructionProgramDefinitions), () => VerifyMochtroidInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyPlatformInstructionProgramDefinitions), () => VerifyPlatformInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyEnemyBreakableTerrainDefinitions), () => VerifyEnemyBreakableTerrainDefinitions(rom));
        Suite(nameof(VerifyMotherBrainDeathExplosionDefinitions), () => VerifyMotherBrainDeathExplosionDefinitions(rom));
        Suite(nameof(VerifyWaverAnimationDefinitions), () => VerifyWaverAnimationDefinitions(rom));
        Suite(nameof(VerifyWaverInstructionProgramDefinitions), () => VerifyWaverInstructionProgramDefinitions(rom));
        Suite(nameof(VerifySkreeMetareeAnimationDefinitions), () => VerifySkreeMetareeAnimationDefinitions(rom));
        Suite(nameof(VerifySkreeMetareeInstructionProgramDefinitions), () => VerifySkreeMetareeInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyZoaAnimationDefinitions), () => VerifyZoaAnimationDefinitions(rom));
        Suite(nameof(VerifyZoaInstructionProgramDefinitions), () => VerifyZoaInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyDragonAnimationDefinitions), () => VerifyDragonAnimationDefinitions(rom));
        Suite(nameof(VerifyDragonInstructionProgramDefinitions), () => VerifyDragonInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyEnemyPickupDefinitions), () => VerifyEnemyPickupDefinitions(rom));
        Suite(nameof(VerifyEnemyDropChanceDefinitions), () => VerifyEnemyDropChanceDefinitions(rom));
        Suite(nameof(VerifyEnemyVulnerabilityDefinitions), () => VerifyEnemyVulnerabilityDefinitions(rom));
        Suite(nameof(VerifyDownwardGateShotBlockDefinitions), () => VerifyDownwardGateShotBlockDefinitions(rom));
        Suite(nameof(VerifySpeedBoosterEscapeStageDefinitions), () => VerifySpeedBoosterEscapeStageDefinitions(rom));
        Suite(nameof(VerifyDoorClosingPlmDefinitions), () => VerifyDoorClosingPlmDefinitions(rom));
        Suite(nameof(VerifyBlueDoorPlmDrawDefinitions), () => VerifyBlueDoorPlmDrawDefinitions(rom));
        Suite(nameof(VerifyColoredDoorPlmDrawDefinitions), () => VerifyColoredDoorPlmDrawDefinitions(rom));
        Suite(nameof(VerifyGreyDoorPlmDrawDefinitions), () => VerifyGreyDoorPlmDrawDefinitions(rom));
        Suite(nameof(VerifyEyeDoorPlmDrawDefinitions), () => VerifyEyeDoorPlmDrawDefinitions(rom));
        Suite(nameof(VerifyMotherBrainGlassPlmDrawDefinitions), () => VerifyMotherBrainGlassPlmDrawDefinitions(rom));
        Suite(nameof(VerifyNoobTubePlmDrawDefinitions), () => VerifyNoobTubePlmDrawDefinitions(rom));
        Suite(nameof(VerifySamusArmCannonDefinitions), () => VerifySamusArmCannonDefinitions(rom));
        Suite(nameof(VerifyEnemyDeathExplosionDefinitions), () => VerifyEnemyDeathExplosionDefinitions(rom));
        Suite(nameof(VerifyEnemyProjectileDefinitions), () => VerifyEnemyProjectileDefinitions(rom));
        Suite(nameof(VerifyRoomPaletteFxDefinitions), () => VerifyRoomPaletteFxDefinitions(rom));
        Suite(nameof(VerifyRoomSpriteObjectDefinitions), () => VerifyRoomSpriteObjectDefinitions(rom));
        Suite(nameof(VerifyCeresSteamDefinitions), () => VerifyCeresSteamDefinitions(rom));
        Suite(nameof(VerifyCeresSteamInstructionProgramDefinitions), () => VerifyCeresSteamInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyCeresDoorInstructionProgramDefinitions), () => VerifyCeresDoorInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyPipeBugAnimationDefinitions), () => VerifyPipeBugAnimationDefinitions(rom));
        Suite(nameof(VerifyBrinstarPipeBugInstructionProgramDefinitions), () => VerifyBrinstarPipeBugInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyNorfairPipeBugInstructionProgramDefinitions), () => VerifyNorfairPipeBugInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyYellowPipeBugInstructionProgramDefinitions), () => VerifyYellowPipeBugInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyDraygonBurialEvirDefinitions), () => VerifyDraygonBurialEvirDefinitions(rom));
        Suite(nameof(VerifyDraygonHealthPaletteDefinitions), () => VerifyDraygonHealthPaletteDefinitions(rom));
        Suite(nameof(VerifyFakeKraidProjectileDefinitions), () => VerifyFakeKraidProjectileDefinitions(rom));
        Suite(nameof(VerifyFakeKraidInstructionProgramDefinitions), () => VerifyFakeKraidInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyChozoStatueInstructionProgramDefinitions), () => VerifyChozoStatueInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyCrocomireTongueInstructionProgramDefinitions), () => VerifyCrocomireTongueInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyCrocomireInstructionProgramDefinitions), () => VerifyCrocomireInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyMotherBrainBabyInstructionProgramDefinitions), () => VerifyMotherBrainBabyInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyEscapeEtecoonDefinitions), () => VerifyEscapeEtecoonDefinitions(rom));
        Suite(nameof(VerifyEscapeEtecoonInstructionProgramDefinitions), () => VerifyEscapeEtecoonInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyEscapeDachoraInstructionProgramDefinitions), () => VerifyEscapeDachoraInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyZebetiteDefinitions), () => VerifyZebetiteDefinitions(rom));
        Suite(nameof(VerifyMotherBrainBabyMetroidDefinitions), () => VerifyMotherBrainBabyMetroidDefinitions(rom));
        Suite(nameof(VerifyYardDirectionDefinitions), () => VerifyYardDirectionDefinitions(rom));
        Suite(nameof(VerifyYardTurnDefinitions), () => VerifyYardTurnDefinitions(rom));
        Suite(nameof(VerifyBombTorizoStatueFragmentDefinitions), () => VerifyBombTorizoStatueFragmentDefinitions(rom));
        Suite(nameof(VerifyTorizoRandomizedProjectileDefinitions), () => VerifyTorizoRandomizedProjectileDefinitions(rom));
        Suite(nameof(VerifyBombTorizoAttackDefinitions), () => VerifyBombTorizoAttackDefinitions(rom));
        Suite(nameof(VerifyBombTorizoMovementDefinitions), () => VerifyBombTorizoMovementDefinitions(rom));
        Suite(nameof(VerifyCrocomireRumbleDefinitions), () => VerifyCrocomireRumbleDefinitions(rom));
        Suite(nameof(VerifyCrocomireBridgeFragmentDefinitions), () => VerifyCrocomireBridgeFragmentDefinitions(rom));
        Suite(nameof(VerifyRoomShakeDefinitions), () => VerifyRoomShakeDefinitions(rom));
        Suite(nameof(VerifyCeresDoorQuakeDefinitions), () => VerifyCeresDoorQuakeDefinitions(rom));
        Suite(nameof(VerifyCeresDoorInitializationDefinitions), () => VerifyCeresDoorInitializationDefinitions(rom));
        Suite(nameof(VerifyCeresRidleyEyeFadeDefinitions), () => VerifyCeresRidleyEyeFadeDefinitions(rom));
        Suite(nameof(VerifyCompiledCeresRidleyGetaway), () => VerifyCompiledCeresRidleyGetaway(rom));
        Suite(nameof(VerifyRidleyExplosionDefinitions), () => VerifyRidleyExplosionDefinitions(rom));
        Suite(nameof(VerifyCrocomireMeltingDefinitions), () => VerifyCrocomireMeltingDefinitions(rom));
        Suite(nameof(VerifyDraygonIntroDanceDefinitions), () => VerifyDraygonIntroDanceDefinitions(rom));
        Suite(nameof(VerifyCompiledBoyonSpeeds), () => VerifyCompiledBoyonSpeeds(rom));
        Suite(nameof(VerifyBoyonInstructionProgramDefinitions), () => VerifyBoyonInstructionProgramDefinitions(rom));
        Suite(nameof(VerifySkulteraInstructionProgramDefinitions), () => VerifySkulteraInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyCompiledSurfaceMotion), () => VerifyCompiledSurfaceMotion(rom));
        Suite(nameof(VerifyCompiledEnemyFireballLaunches), () => VerifyCompiledEnemyFireballLaunches(rom));
        Suite(nameof(VerifyCompiledRioLaunches), () => VerifyCompiledRioLaunches(rom));
        Suite(nameof(VerifyCompiledBoulderBounces), () => VerifyCompiledBoulderBounces(rom));
        Suite(nameof(VerifyBoulderInstructionProgramDefinitions), () => VerifyBoulderInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyCompiledZoaSpeeds), () => VerifyCompiledZoaSpeeds(rom));
        Suite(nameof(VerifyCompiledGrowingShutters), () => VerifyCompiledGrowingShutters(rom));
        Suite(nameof(VerifyCompiledIntroEggMotion), () => VerifyCompiledIntroEggMotion(rom));
        Suite(nameof(VerifyCompiledPowerBombShape), () => VerifyCompiledPowerBombShape(rom));
        Suite(nameof(VerifyCompiledAbsoluteTangent), () => VerifyCompiledAbsoluteTangent(rom));
        Suite(nameof(VerifyCompiledStatueWalking), () => VerifyCompiledStatueWalking(rom));
        Suite(nameof(VerifyCompiledRidleyInertia), () => VerifyCompiledRidleyInertia(rom));
        Suite(nameof(VerifyRidleyDeathAcceleration), () => VerifyRidleyDeathAcceleration(rom));
        Suite(nameof(VerifyCompiledPhantoonMotion), () => VerifyCompiledPhantoonMotion(rom));
        Suite(nameof(VerifyCompiledPhantoonFlameSpawns), () => VerifyCompiledPhantoonFlameSpawns(rom));
        Suite(nameof(VerifyCompiledPhantoonTimers), () => VerifyCompiledPhantoonTimers(rom));
        Suite(nameof(VerifyCompiledPhantoonCasualFlames), () => VerifyCompiledPhantoonCasualFlames(rom));
        Suite(nameof(VerifyCompiledPhantoonPatterns), () => VerifyCompiledPhantoonPatterns(rom));
        Suite(nameof(VerifyCompiledPhantoonPath), () => VerifyCompiledPhantoonPath(rom));
        Suite(nameof(VerifyCompiledPhantoonDeathExplosions), () => VerifyCompiledPhantoonDeathExplosions(rom));
        Suite(nameof(VerifyPhantoonSoundDefinitions), () => VerifyPhantoonSoundDefinitions(rom));
        Suite(nameof(VerifyCompiledSlopeHeights), () => VerifyCompiledSlopeHeights(rom));
        Suite(nameof(VerifyCompiledSlopeSpeeds), () => VerifyCompiledSlopeSpeeds(rom));
        Suite(nameof(VerifyCompiledSquareSlopes), () => VerifyCompiledSquareSlopes(rom));
        Suite(nameof(VerifyCompiledTorizoInitialization), () => VerifyCompiledTorizoInitialization(rom));
        Suite(nameof(VerifyCompiledGunshipDust), () => VerifyCompiledGunshipDust(rom));
        Suite(nameof(VerifyGunshipMotionDefinitions), () => VerifyGunshipMotionDefinitions(rom));
        Suite(nameof(VerifyCompiledRidleyPogo), () => VerifyCompiledRidleyPogo(rom));
        Suite(nameof(VerifyRidleyMovementTargets), () => VerifyRidleyMovementTargets(rom));
        Suite(nameof(VerifyRidleyAttackChoices), () => VerifyRidleyAttackChoices(rom));
        Suite(nameof(VerifyRidleyClawOffsets), () => VerifyRidleyClawOffsets(rom));
        Suite(nameof(VerifyFallingSparkLaunchDefinitions), () => VerifyFallingSparkLaunchDefinitions(rom));
        Suite(nameof(VerifyDeadSidehopperLaunchDefinitions), () => VerifyDeadSidehopperLaunchDefinitions(rom));
        Suite(nameof(VerifyDeadSidehopperCorpseDefinitions), () => VerifyDeadSidehopperCorpseDefinitions(rom));
        Suite(nameof(VerifyDeadTourianCorpseDefinitions), () => VerifyDeadTourianCorpseDefinitions(rom));
        Suite(nameof(VerifyDeadTorizoCorpseDefinitions), () => VerifyDeadTorizoCorpseDefinitions(rom));
        Suite(nameof(VerifyKiHunterMotionDefinitions), () => VerifyKiHunterMotionDefinitions(rom));
        Suite(nameof(VerifyKraidRockLaunchDefinitions), () => VerifyKraidRockLaunchDefinitions(rom));
        Suite(nameof(VerifyKraidMovementChoices), () => VerifyKraidMovementChoices(rom));
        Suite(nameof(VerifyKraidBodyContour), () => VerifyKraidBodyContour(rom));
        Suite(nameof(VerifyKraidNailSibling), () => VerifyKraidNailSibling(rom));
        Suite(nameof(VerifyKraidNailBounce), () => VerifyKraidNailBounce(rom));
        Suite(nameof(VerifyKraidCeilingRockPositions), () => VerifyKraidCeilingRockPositions(rom));
        Suite(nameof(VerifyKraidSinkSchedule), () => VerifyKraidSinkSchedule(rom));
        Suite(nameof(VerifyKraidHeadInstructionDefinitions), () => VerifyKraidHeadInstructionDefinitions(rom));
        Suite(nameof(VerifyKraidMouthHitboxes), () => VerifyKraidMouthHitboxes(rom));
        Suite(nameof(VerifyKraidNailContour), () => VerifyKraidNailContour(rom));
        Suite(nameof(VerifySamusVerticalDefinitions), () => VerifySamusVerticalDefinitions(rom));
        Suite(nameof(VerifySamusImpulseDefinitions), () => VerifySamusImpulseDefinitions(rom));
        Suite(nameof(VerifySamusBallBounceDefinitions), () => VerifySamusBallBounceDefinitions(rom));
        Suite(nameof(VerifySamusStandaloneSpeeds), () => VerifySamusStandaloneSpeeds(rom));
        Suite(nameof(VerifySamusIndexedSpeeds), () => VerifySamusIndexedSpeeds(rom));
        Suite(nameof(VerifyGrappleFiringDefinitions), () => VerifyGrappleFiringDefinitions(rom));
        Suite(nameof(VerifyGrappleConnectionDefinitions), () => VerifyGrappleConnectionDefinitions(rom));
        Suite(nameof(VerifySamusHudDefinitions), () => VerifySamusHudDefinitions(rom));
        Suite(nameof(VerifyGrappleBodyPlacement), () => VerifyGrappleBodyPlacement(rom));
        Suite(nameof(VerifyRunningCadence), () => VerifyRunningCadence(rom));
        Suite(nameof(VerifyPoseProjectileOrigin), () => VerifyPoseProjectileOrigin(rom));
        Suite(nameof(VerifyPoseCollisionDefinitions), () => VerifyPoseCollisionDefinitions(rom));
        Suite(nameof(VerifyPoseDispatchDefinitions), () => VerifyPoseDispatchDefinitions(rom));
        Suite(nameof(VerifyPoseInputDefinitions), () => VerifyPoseInputDefinitions(rom));
        Suite(nameof(VerifyBombSpreadLaunchDefinitions), () => VerifyBombSpreadLaunchDefinitions(rom));
        Suite(nameof(VerifyProjectileDamage), () => VerifyProjectileDamage(rom));
        Suite(nameof(VerifyProjectileRadii), () => VerifyProjectileRadii(rom));
        Suite(nameof(VerifyProjectileInstructions), () => VerifyProjectileInstructions(rom));
        Suite(nameof(VerifyProjectileFrameBindings), () => VerifyProjectileFrameBindings(rom));
        Suite(nameof(VerifyChargeFlareDefinitions), () => VerifyChargeFlareDefinitions(rom));
        Suite(nameof(VerifyChargeFlarePlacement), () => VerifyChargeFlarePlacement(rom));
        Suite(nameof(VerifyProjectileVisualParts), () => VerifyProjectileVisualParts());
        Suite(nameof(VerifyProjectileCompositions), () => VerifyProjectileCompositions(rom));
        Suite(nameof(VerifyProjectileOrigins), () => VerifyProjectileOrigins(rom));

        T Method<T>(string name) where T : Delegate => typeof(RoomEnemySystem)
            .GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic)!.CreateDelegate<T>();
        var pixel = Method<Func<ushort, ushort, int>>("ReadEightBitSineProduct");
        var fixedProduct = Method<Func<ushort, ushort, (short Whole, ushort Fraction)>>("ReadEightBitSineFixedProduct");
        var cosine = Method<Func<ushort, ushort, int>>("ReadEightBitCosineProduct");
        var negative = Method<Func<ushort, ushort, int>>("ReadEightBitNegativeSineProduct");
        var fixedCosine = Method<Func<ushort, ushort, (short Whole, ushort Fraction)>>("ReadEightBitCosineFixedProduct");
        var fixedNegative = Method<Func<ushort, ushort, (short Whole, ushort Fraction)>>("ReadEightBitNegativeSineFixedProduct");
        var sbugSigned = Method<Func<byte, byte, int, SbugVelocityWords>>("CalculateSignedSbugComponent");
        var sbugUnsigned = Method<Func<byte, byte, int, SbugVelocityWords>>("CalculateUnsignedSbugMagnitude");
        var unsigned = Method<Func<ushort, ushort, ushort, int>>("ReadUnsignedSineMagnitudeProduct");

        int ExpectedPixel(int angle, int radius)
        {
            angle &= 255;
            int magnitude = bytes[angle & 127] * (radius & 255) >> 8;
            return angle < 128 ? magnitude : -magnitude;
        }
        for (int angle = 0; angle < 256; angle++)
        for (int radius = 0; radius < 256; radius++)
        {
            // Poison high bytes to prove native byte truncation remains in the
            // production helpers. The fractional negation deliberately has no carry.
            ushort a = (ushort)(0xff00 | angle), r = (ushort)(0xab00 | radius);
            int product = bytes[angle & 127] * radius;
            var expected = ((short)(angle < 128 ? product >> 8 : -(product >> 8)),
                unchecked((ushort)(angle < 128 ? product << 8 : -(product << 8))));
            if (pixel(a, r) != ExpectedPixel(angle, radius) || fixedProduct(a, r) != expected ||
                cosine(a, r) != ExpectedPixel(angle + 64, radius) || negative(a, r) != ExpectedPixel(angle + 128, radius))
                throw new InvalidDataException($"Compiled byte sine result differs at angle={angle}, radius={radius}.");
            foreach (int phase in new[] { 64, 128 })
            {
                int shiftedAngle = (angle + phase) & 255;
                int shiftedProduct = bytes[shiftedAngle & 127] * radius;
                short whole = (short)(shiftedAngle < 128 ? shiftedProduct >> 8 : -(shiftedProduct >> 8));
                ushort fraction = unchecked((ushort)(shiftedAngle < 128 ? shiftedProduct << 8 : -(shiftedProduct << 8)));
                var fixedActual = phase == 64 ? fixedCosine(a, r) : fixedNegative(a, r);
                var signedActual = sbugSigned((byte)angle, (byte)radius, phase);
                var unsignedActual = sbugUnsigned((byte)angle, (byte)radius, phase);
                uint magnitude = (uint)words[shiftedAngle & 127] * (uint)radius;
                if (fixedActual != (whole, fraction) || signedActual != new SbugVelocityWords(unchecked((ushort)whole), fraction) ||
                    unsignedActual.RawFixed != magnitude)
                    throw new InvalidDataException($"Compiled vector differs at angle={angle}, radius={radius}, phase={phase}.");
            }
        }
        for (int index = 0; index < 128; index++)
        for (int magnitude = 0; magnitude <= ushort.MaxValue; magnitude++)
        {
            int expected = unchecked((int)((uint)words[index] * magnitude));
            if (unsigned((ushort)(0xff80 | index), (ushort)magnitude, 128) != expected ||
                unsigned((ushort)(0xffc0 + index), (ushort)magnitude, 64) != expected)
                throw new InvalidDataException($"Compiled unsigned sine differs at index={index}, magnitude={magnitude}.");
        }
        Console.WriteLine("Compiled enemy sine: 256 exact samples, 65,536 byte inputs including both Sbug vector phases, and 8,388,608 unsigned products (two wrapped offsets) pass without any production bus dependency.");
    }

    /// <summary>Compares the compiled signed sine data and its production readers with cartridge words, optionally stopping before the exhaustive liquid-tide phase check.</summary>
    /// <param name="rom">Retail address space supplying the pinned signed negative-cosine reference words.</param>
    /// <param name="definitionsOnly">When <see langword="true"/>, skips the subsequent liquid-tide phase sweep.</param>
    private static void VerifyCompiledSignedTrigonometry(SuperMetroidAddressSpace rom, bool definitionsOnly = false)
    {
        short Reference(int index)
        {
            int address = EnemyMathReferenceData.SignedNegativeCosine + index * 2;
            return unchecked((short)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));
        }
        var cinematicReaders = new[] { typeof(CeresDestructionCinematicState), typeof(EndingCreditsState), typeof(IntroCeresFlightState) }
            .Select(type => type.GetMethod("ReadSine", BindingFlags.NonPublic | BindingFlags.Static)!
                .CreateDelegate<Func<byte, short>>()).ToArray();
        for (int index = 0; index < 320; index++)
        {
            short expected = Reference(index);
            AssertEqual(expected, EnemyTrigonometryTables.SignedNegativeCosineWord(index), "all 320 native prefix/full-wave words");
            if (index >= 64)
            {
                byte angle = (byte)(index - 64);
                AssertEqual(expected, EnemyTrigonometryTables.SignedSine(angle), "all signed sine quadrants match cartridge");
                foreach (var reader in cinematicReaders)
                    AssertEqual(expected, reader(angle), "cinematic matrix sample matches cartridge without bus");
            }
        }
        foreach (int invalid in new[] { -1, 320, int.MinValue, int.MaxValue })
        {
            bool rejected = false;
            try { _ = EnemyTrigonometryTables.SignedNegativeCosineWord(invalid); }
            catch (ArgumentOutOfRangeException) { rejected = true; }
            AssertTrue(rejected, "signed table does not silently wrap invalid prefix indexes");
        }
        Console.WriteLine("Compiled signed sine: all 320 native words and three cinematic readers match, including +/-256 peaks and prefix bounds.");
        if (definitionsOnly) return;
        var tide = new RoomLayer3FxState();
        var phaseField = typeof(RoomLayer3FxState).GetField("tidePhase", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var offsetField = typeof(RoomLayer3FxState).GetField("tideFixedOffset", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var options = typeof(RoomLayer3FxState).GetProperty(nameof(RoomLayer3FxState.LiquidOptions))!;
        var step = typeof(RoomLayer3FxState).GetMethod("StepLiquidTide", BindingFlags.Instance | BindingFlags.NonPublic)!
            .CreateDelegate<Action>(tide);
        foreach (ushort option in new ushort[] { 0, 0x40, 0x80, 0xc0 })
        {
            options.SetValue(tide, option);
            for (int phase = 0; phase <= ushort.MaxValue; phase++)
            {
                phaseField.SetValue(tide, (ushort)phase);
                offsetField.SetValue(tide, 12345);
                step();
                short sample = Reference(phase >> 8);
                bool small = (option & 0x80) != 0;
                int scale = option == 0 ? 0 : small ? 8 : 32;
                int delta = option == 0 ? 0 : small ? (sample >= 0 ? 288 : 192) : (sample >= 0 ? 224 : 128);
                if ((int)offsetField.GetValue(tide)! != (sample * scale << 8) ||
                    (ushort)phaseField.GetValue(tide)! != unchecked((ushort)(phase + delta)))
                    throw new InvalidDataException($"Native tide differs at options={option:X2}, phase={phase:X4}.");
            }
        }
        Console.WriteLine("Compiled tide: all 65,536 phases in four option combinations preserve exact offset, phase advance and small-tide precedence without a bus.");
    }

}

/// <summary>Pinned retail-ROM addresses for enemy and cinematic sine tables used as independent arithmetic references.</summary>
internal static class EnemyMathReferenceData
{
    /// <summary>Pinned $A0:B443-$B642 sign-extended 8.8 sine words.</summary>
    public const int SignedSine = 0xa0b443;
    /// <summary>Pinned $A0:B1C3 signed 16-bit sine/cosine quadrants.</summary>
    public const int SignedSixteenBitSine = 0xa0b1c3;
    /// <summary>Pinned $AA:E03D Shaktool negative-cosine prefix and sine quadrants.</summary>
    public const int ShaktoolOrbit = 0xaae03d;
    /// <summary>Pinned $A0:B3C3 negative-cosine prefix followed by the full signed sine wave.</summary>
    public const int SignedNegativeCosine = 0xa0b3c3;
    /// <summary>Pinned $A0:B143 positive byte sine/cosine sample range.</summary>
    public const int ByteSine = 0xa0b143;
    /// <summary>Pinned $A0:B7EE UnsignedSineTable.</summary>
    public const int UnsignedSine = 0xa0b7ee;
}
