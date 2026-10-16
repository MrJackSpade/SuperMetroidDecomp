using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Enemies.Alcoon;
using SuperMetroid.Verification.Enemies.Powamp;
using SuperMetroid.Verification.Enemies.Metroids;
using SuperMetroid.Verification.Enemies.Crawlers;
using SuperMetroid.Verification.Enemies.Dragon;
using SuperMetroid.Verification.Enemies.Cacatac;
using SuperMetroid.Verification.Enemies.Stoke;
using SuperMetroid.Verification.Enemies.Owtch;
using SuperMetroid.Verification.Enemies.PipeBugs;
using SuperMetroid.Verification.Enemies.Elevator;
using SuperMetroid.Verification.Enemies.Shutters;
using SuperMetroid.Verification.Enemies.Boulder;
using SuperMetroid.Verification.Enemies.Bull;
using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Samus.Movement.AbsoluteTangentDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Alcoon.AlcoonInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Fx.AnimatedTileInstructionCodesVerification;
using static SuperMetroid.Verification.Enemies.Atomic.AtomicMovementDefinitionsVerification;
using static SuperMetroid.Verification.Audio.AudioUploadCatalogVerification;
using static SuperMetroid.Verification.Samus.Projectiles.BeamCallbackTablesVerification;
using static SuperMetroid.Verification.Enemies.Beetom.BeetomInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.LevelData.BlockReactionSelectorsVerification;
using static SuperMetroid.Verification.Rooms.Doors.BlueDoorPlmDrawDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.LevelData.BombBlockProgramsVerification;
using static SuperMetroid.Verification.Bosses.Torizo.BombTorizoAttackDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Torizo.BombTorizoStatueFragmentDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.BossOamRootGeometryVerification;
using static SuperMetroid.Verification.Bosses.Botwoon.BotwoonAnimationsVerification;
using static SuperMetroid.Verification.Bosses.Botwoon.BotwoonHealthPaletteDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Botwoon.BotwoonNavigationDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Botwoon.BotwoonSpeedsVerification;
using static SuperMetroid.Verification.Bosses.Botwoon.BotwoonWallPlmsVerification;
using static SuperMetroid.Verification.Bosses.Botwoon.BotwoonWallVisualsVerification;
using static SuperMetroid.Verification.Frontend.CeresCinematics.CeresBlastPlacementVerification;
using static SuperMetroid.Verification.Frontend.CeresCinematics.CeresDestructionFrameCatalogVerification;
using static SuperMetroid.Verification.Areas.Ceres.CeresEscapeVramTransferDefinitionsVerification;
using static SuperMetroid.Verification.Frontend.CeresCinematics.CeresExplosionDefinitionsVerification;
using static SuperMetroid.Verification.Frontend.CeresCinematics.CeresFlightActorMetadataVerification;
using static SuperMetroid.Verification.Frontend.CeresCinematics.CeresFlightFrameCatalogVerification;
using static SuperMetroid.Verification.Areas.Ceres.CeresHazeVerification;
using static SuperMetroid.Verification.Frontend.CeresCinematics.CeresInitialActorMetadataVerification;
using static SuperMetroid.Verification.Frontend.CeresCinematics.Artwork.CeresLargeAsteroidPartsVerification;
using static SuperMetroid.Verification.Frontend.CeresCinematics.Artwork.CeresLargeBlastPartsVerification;
using static SuperMetroid.Verification.Frontend.CeresCinematics.CeresPlacementSelectorsVerification;
using static SuperMetroid.Verification.Frontend.CeresCinematics.Artwork.CeresReflectedStarPartsVerification;
using static SuperMetroid.Verification.Bosses.Ridley.CeresRidleyGetawayDefinitionsVerification;
using static SuperMetroid.Verification.Frontend.CeresCinematics.Artwork.CeresSmallAsteroidPartsVerification;
using static SuperMetroid.Verification.Frontend.CeresCinematics.CeresSpawnerScheduleVerification;
using static SuperMetroid.Verification.Frontend.CeresCinematics.Artwork.CeresStarPointPartsVerification;
using static SuperMetroid.Verification.Frontend.CeresCinematics.Artwork.CeresStationBlastPartsVerification;
using static SuperMetroid.Verification.Frontend.CeresCinematics.Artwork.CeresStationPartsVerification;
using static SuperMetroid.Verification.Frontend.CeresCinematics.Artwork.CeresVortexPartsVerification;
using static SuperMetroid.Verification.Frontend.CeresCinematics.CeresZebesActorMetadataVerification;
using static SuperMetroid.Verification.Samus.Projectiles.ChargeFlareDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Chozo.ChozoStatuePlmDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.Doors.ColoredDoorPlmDrawDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.Plms.CompiledDynamicCollectibleGraphicsVerification;
using static SuperMetroid.Verification.Rooms.Plms.CompiledScrollPlmProgramsVerification;
using static SuperMetroid.Verification.Rooms.Plms.ContactCrumbleProgramsVerification;
using static SuperMetroid.Verification.Frontend.ControllerButtonLookupsVerification;
using static SuperMetroid.Verification.Enemies.Crawlers.CrawlerAnimationDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Crocomire.CrocomireBg2ScrollVerification;
using static SuperMetroid.Verification.Bosses.Crocomire.CrocomireBodyVisualsVerification;
using static SuperMetroid.Verification.Bosses.Crocomire.CrocomireBridgeFragmentDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Crocomire.CrocomireMeltingDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Crocomire.CrocomireAnimationsVerification;
using static SuperMetroid.Verification.Bosses.Crocomire.CrocomireSkeletonFrameGeometryVerification;
using static SuperMetroid.Verification.Bosses.Crocomire.CrocomireSpikeMotionDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Crocomire.CrocomireTongueCollisionDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.Plms.DownwardGatePlmsVerification;
using static SuperMetroid.Verification.Enemies.Dragon.DragonFireballAnimationsVerification;
using static SuperMetroid.Verification.Bosses.Draygon.DraygonHealthPaletteDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Draygon.DraygonIntroDanceDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Draygon.DraygonAnimationsVerification;
using static SuperMetroid.Verification.Enemies.Elevator.ElevatorInputDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Elevator.ElevatorPlatformVisualsVerification;
using static SuperMetroid.Verification.Frontend.Ending.EndingCompletionTextInstructionsVerification;
using static SuperMetroid.Verification.Frontend.Ending.EndingExplosionSpriteArtworkVerification;
using static SuperMetroid.Verification.Frontend.Ending.EndingGlyphMappingVerification;
using static SuperMetroid.Verification.Frontend.Ending.EndingLogoTablesVerification;
using static SuperMetroid.Verification.Frontend.Ending.EndingLogoVerification;
using static SuperMetroid.Verification.Frontend.Ending.EndingObjectArtworkVerification;
using static SuperMetroid.Verification.Frontend.Ending.Artwork.EndingPaletteArtworkVerification;
using static SuperMetroid.Verification.Frontend.Ending.EndingRewardGestureVerification;
using static SuperMetroid.Verification.Frontend.Ending.EndingRewardInstructionsVerification;
using static SuperMetroid.Verification.Enemies.EnemyFireballLaunchDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.EnemyLinearSpeedsVerification;
using static SuperMetroid.Verification.Enemies.EnemyQuadraticSpeedsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkDraygonBg2FramesVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkDraygonOamCollisionVerification;
using static SuperMetroid.Verification.Enemies.EnemyTrigonometryVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyVisualSelectorInventoryVerification;
using static SuperMetroid.Verification.Enemies.Animals.EscapeEtecoonDefinitionsVerification;
using static SuperMetroid.Verification.Areas.Escape.EscapeGateVisualsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.ExtendedFrameSequenceVerification;
using static SuperMetroid.Verification.Rooms.Doors.EyeDoorPlmDrawDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.Doors.EyeDoorProjectileAnimationsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.FakeKraidProjectileDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.FakeKraidProjectileProgramMappingsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.FakeKraidSpitVelocitiesVerification;
using static SuperMetroid.Verification.Enemies.FamilyTrigonometryVerification;
using static SuperMetroid.Verification.Frontend.FileSelect.FileSelectHelmetAnimationVerification;
using static SuperMetroid.Verification.Frontend.FileSelect.FileSelectNameCasesVerification;
using static SuperMetroid.Verification.Frontend.FileSelect.FileSelectNavigationLookupVerification;
using static SuperMetroid.Verification.Enemies.Fireflea.FirefleaFxVerification;
using static SuperMetroid.Verification.Enemies.Fireflea.FirefleaMovementDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.FuneNamihe.FuneNamiheDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.FuneNamihe.FuneNamiheFireballAnimationsVerification;
using static SuperMetroid.Verification.Enemies.FuneNamihe.FuneNamiheInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Frontend.GameOver.GameOverBabyAnimationVerification;
using static SuperMetroid.Verification.Frontend.GameOver.GameOverBabyColorsVerification;
using static SuperMetroid.Verification.Frontend.GameOver.GameOverTextLookupsVerification;
using static SuperMetroid.Verification.Bosses.GeneratedBossBg2CatalogsVerification;
using static SuperMetroid.Verification.Samus.Grapple.GrappleBlockProgramsVerification;
using static SuperMetroid.Verification.Samus.Grapple.GrappleFiringDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.Doors.GreyDoorPlmDrawDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Shutters.GrowingShutterDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Gunship.GunshipMotionDefinitionsVerification;
using static SuperMetroid.Verification.Hardware.HalfWaveAlgorithmsVerification;
using static SuperMetroid.Verification.Hardware.HighlightTilePixelsVerification;
using static SuperMetroid.Verification.Samus.Palettes.HyperBeamFxColorArtworkVerification;
using static SuperMetroid.Verification.Frontend.Intro.Artwork.IntroConfusedBabyPartsVerification;
using static SuperMetroid.Verification.Frontend.Intro.IntroDiscoveryCollisionVerification;
using static SuperMetroid.Verification.Frontend.Intro.IntroDiscoveryFrameCatalogVerification;
using static SuperMetroid.Verification.Frontend.Intro.Artwork.IntroEggCrackingPartsVerification;
using static SuperMetroid.Verification.Frontend.Intro.Artwork.IntroEggEffectPartsVerification;
using static SuperMetroid.Verification.Frontend.Intro.IntroEggEffectProgramsVerification;
using static SuperMetroid.Verification.Frontend.Intro.Artwork.IntroEggRemnantPartsVerification;
using static SuperMetroid.Verification.Frontend.Intro.Artwork.IntroEggRockingPartsVerification;
using static SuperMetroid.Verification.Frontend.Intro.IntroEyeInstructionsVerification;
using static SuperMetroid.Verification.Frontend.Intro.IntroMotherBrainCollisionSourceVerification;
using static SuperMetroid.Verification.Frontend.Intro.Artwork.IntroMotherBrainExplosionPartsVerification;
using static SuperMetroid.Verification.Frontend.Intro.IntroMotherBrainExplosionProgramsVerification;
using static SuperMetroid.Verification.Frontend.Intro.IntroMotherBrainFrameCatalogVerification;
using static SuperMetroid.Verification.Frontend.Intro.Artwork.IntroMotherBrainPartsVerification;
using static SuperMetroid.Verification.Frontend.Intro.Artwork.IntroRinkaPartsVerification;
using static SuperMetroid.Verification.Frontend.Intro.IntroRinkaProgramsVerification;
using static SuperMetroid.Verification.Frontend.Intro.IntroScientistFrameCatalogVerification;
using static SuperMetroid.Verification.Frontend.Intro.Artwork.IntroScientistPartsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidArmCollisionDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidArmComponentHitboxesVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidArmPhysicalFrameMappingsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidCameraMappingsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidCeilingRockPositionsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidContourCaseMappingsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidDefeatedPlmMappingsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidDrawMappingsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidFootCoordinateMappingsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidFootFrameGeometryVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidGrowthPlmMappingsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidHeadCommandMappingVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidHealthThresholdsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidMouthMappingsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidMovementChoicesVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidNailContourMappingsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidNailSiblingVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidPaletteDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidRockLaunchDefinitionsVerification;
using SuperMetroid.Verification.Rooms.Plms;
using static SuperMetroid.Verification.Bosses.Kraid.KraidRoomVisualSelectionVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidSinkScheduleVerification;
using static SuperMetroid.Verification.Areas.Ceres.LookupCeresHazeRampVerification;
using static SuperMetroid.Verification.Enemies.Animals.LookupDachoraColorsVerification;
using static SuperMetroid.Verification.Fx.LookupHorizontalHeatWaveVerification;
using static SuperMetroid.Verification.Fx.LookupLiquidWaveVerification;
using static SuperMetroid.Verification.Fx.PaletteFx.LookupPaletteFxAreasVerification;
using static SuperMetroid.Verification.Fx.PaletteFx.LookupPaletteFxDispatchVerification;
using static SuperMetroid.Verification.Fx.LookupQuakeSoundSelectionVerification;
using static SuperMetroid.Verification.Fx.LookupQuakeSoundSuppressionVerification;
using static SuperMetroid.Verification.Fx.LookupRainVelocityVerification;
using static SuperMetroid.Verification.Rooms.Doors.LookupRetailDoorListsVerification;
using static SuperMetroid.Verification.Rooms.Plms.LookupRetailPlmHeadersVerification;
using static SuperMetroid.Verification.Rooms.LookupRetailPopulationsVerification;
using static SuperMetroid.Verification.Harness.LookupRomOraclesVerification;
using static SuperMetroid.Verification.Rooms.Backgrounds.LookupSkyChunkPointersVerification;
using static SuperMetroid.Verification.Rooms.Backgrounds.LookupSkySectionsVerification;
using static SuperMetroid.Verification.Enemies.LookupSmallGameTablesVerification;
using static SuperMetroid.Verification.Harness.LookupStream1Verification;
using static SuperMetroid.Verification.Harness.LookupStream2Verification;
using static SuperMetroid.Verification.Harness.LookupStream3Verification;
using static SuperMetroid.Verification.Harness.LookupStream4Verification;
using static SuperMetroid.Verification.Harness.LookupStream5Verification;
using static SuperMetroid.Verification.Areas.Tourian.LookupTourianArtworkVerification;
using static SuperMetroid.Verification.Areas.Tourian.LookupTourianDescriptorsVerification;
using static SuperMetroid.Verification.Areas.Tourian.LookupTourianProgramVerification;
using static SuperMetroid.Verification.Areas.Tourian.LookupTourianSpawnVerification;
using static SuperMetroid.Verification.Fx.LookupTreadmillMechanicsVerification;
using static SuperMetroid.Verification.Enemies.Rios.LowerNorfairRioInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.MamaTurtles.MamaTurtleInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Frontend.Map.MapArrowCasesVerification;
using static SuperMetroid.Verification.Frontend.Map.MapArrowDurationsVerification;
using static SuperMetroid.Verification.Frontend.Map.MapIndicatorAnimationVerification;
using static SuperMetroid.Verification.Frontend.Map.MapLoadAnchorsVerification;
using static SuperMetroid.Verification.Frontend.Map.MapMarkerPixelsVerification;
using static SuperMetroid.Verification.Frontend.Map.MapSaveMarkerCoordinatesVerification;
using static SuperMetroid.Verification.Frontend.Map.MapSaveMarkersVerification;
using static SuperMetroid.Verification.Frontend.Map.MapSpriteCasesVerification;
using static SuperMetroid.Verification.Frontend.Map.MapWindowMotionDefinitionsVerification;
using static SuperMetroid.Verification.Frontend.Menus.MenuBeveledSquarePixelsVerification;
using static SuperMetroid.Verification.Frontend.Menus.MenuCompactLetteringPixelsVerification;
using static SuperMetroid.Verification.Frontend.Menus.MenuLargeFontPixelsVerification;
using static SuperMetroid.Verification.Frontend.Menus.MenuMissileLookupsVerification;
using static SuperMetroid.Verification.Frontend.Menus.MenuPanelPixelsVerification;
using static SuperMetroid.Verification.Frontend.Menus.MenuShoulderButtonPixelsVerification;
using static SuperMetroid.Verification.Frontend.Menus.MenuShoulderHighlightPixelsVerification;
using static SuperMetroid.Verification.Frontend.Menus.MenuSmallFontPixelsVerification;
using static SuperMetroid.Verification.Frontend.Menus.MenuThinBorderPixelsVerification;
using static SuperMetroid.Verification.Enemies.Metroids.MetroidBehaviorDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Projectiles.MiscDustProjectileDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainGeneratedVisualCatalogsVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainGlassPlmDrawDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainGlassShardDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.Plms.NoobTubePlmDrawDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Nuclear.NuclearWaffleDefinitionsVerification;
using static SuperMetroid.Verification.Frontend.Menus.OptionsGeometryLookupsVerification;
using static SuperMetroid.Verification.Enemies.Owtch.OwtchMovementDefinitionsVerification;
using static SuperMetroid.Verification.Fx.PaletteFx.PaletteFxInstructionCodesVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseButtonSpansVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseCategoryCasesVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentRulesVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseReserveAnchorsVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseReserveFramesVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseSelectorAnchorsVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseSelectorCompositionsVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseSelectorDurationsVerification;
using static SuperMetroid.Verification.Bosses.Phantoon.PhantoonTimersVerification;
using static SuperMetroid.Verification.Bosses.Phantoon.PhantoonWaveMathVerification;
using static SuperMetroid.Verification.Enemies.PipeBugs.PipeBugAnimationDefinitionsVerification;
using static SuperMetroid.Verification.Frontend.Title.Artwork.PlanetZebesTitlePartsVerification;
using static SuperMetroid.Verification.Enemies.Platforms.PlatformInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.Plms.PlmPopulationInputBoundaryVerification;
using static SuperMetroid.Verification.Samus.Poses.PoseCollisionDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Poses.PoseDispatchDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Poses.PoseInputDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Poses.PoseProjectileOriginVerification;
using static SuperMetroid.Verification.Samus.Palettes.PowerBombFixedColorsVerification;
using static SuperMetroid.Verification.Rendering.PowerBombShapeDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Projectiles.ProjectileDamageVerification;
using static SuperMetroid.Verification.Samus.Projectiles.ProjectileOriginsVerification;
using static SuperMetroid.Verification.Samus.Projectiles.ProjectileSoundRoutingVerification;
using static SuperMetroid.Verification.Enemies.Puyo.PuyoInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Fx.QuicksandVerification;
using static SuperMetroid.Verification.Rendering.RenderingAndRoomsVerification;
using static SuperMetroid.Verification.Hud.ReserveTilePixelsVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Bosses.Ridley.RidleyAttackChoicesVerification;
using static SuperMetroid.Verification.Bosses.Ridley.RidleyClawOffsetsVerification;
using static SuperMetroid.Verification.Bosses.Ridley.RidleyCollisionDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Ridley.RidleyExplosionDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Ridley.RidleyMovementTargetsVerification;
using static SuperMetroid.Verification.Fx.Artwork.RoomPaletteFxPresentationVerification;
using static SuperMetroid.Verification.Rooms.Plms.RoomPlmPopulationDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.Plms.RoomPlmPopulationVerification;
using static SuperMetroid.Verification.Fx.RoomShakeDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Movement.RunningCadenceVerification;
using static SuperMetroid.Verification.Samus.Poses.SamusAnimationDelayDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Drawing.SamusArmCannonDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Drawing.SamusAtmosphericEffectDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Palettes.SamusChargeColorsVerification;
using static SuperMetroid.Verification.Rooms.Plms.SamusEaterPlmDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Palettes.SamusFullBodyCycleColorsVerification;
using static SuperMetroid.Verification.Samus.Hud.SamusHudDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Palettes.SamusHurtColorsVerification;
using static SuperMetroid.Verification.Samus.SamusIndexedSpeedsVerification;
using static SuperMetroid.Verification.Samus.Abilities.SamusShinesparkVerification;
using static SuperMetroid.Verification.Samus.Abilities.SamusSpecialStatesVerification;
using static SuperMetroid.Verification.Saves.SaveRamLayoutVerification;
using static SuperMetroid.Verification.Rooms.Scrolling.ScrollPlmsVerification;
using static SuperMetroid.Verification.Enemies.Shaktool.ShaktoolOrbitAlgorithmVerification;
using static SuperMetroid.Verification.Enemies.Shaktool.ShaktoolSegmentDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.LevelData.SharedSlopeHeightsVerification;
using static SuperMetroid.Verification.Rooms.Plms.ShotBlockPlmProgramsVerification;
using static SuperMetroid.Verification.Rooms.LevelData.SlopeSpeedDefinitionsVerification;
using static SuperMetroid.Verification.Frontend.CeresCinematics.SpaceColonyCaptionVerification;
using static SuperMetroid.Verification.Audio.SpcDspPublicationVerification;
using static SuperMetroid.Verification.Audio.SpcNotePercentageAlgorithmsVerification;
using static SuperMetroid.Verification.Audio.SpcPanSamplesVerification;
using static SuperMetroid.Verification.Audio.SpcPitchBasisAlgorithmVerification;
using static SuperMetroid.Verification.Audio.SpcSoundLibrary2PointersVerification;
using static SuperMetroid.Verification.Audio.SpcSoundPoliciesVerification;
using static SuperMetroid.Verification.Samus.Abilities.SpeedBoosterPlmProgramsVerification;
using static SuperMetroid.Verification.Samus.Movement.SquareSlopeDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.Plms.StationAccessPlmDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Chozo.StatueWalkDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Samus.Projectiles.SuitPickupBeamCurveDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Waver.WaverAnimationDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.WorkRobots.WorkRobotInitialSelectionVerification;
using static SuperMetroid.Verification.Enemies.WorkRobots.WorkRobotPaletteTimingDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Yard.YardDirectionDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Yard.YardTurnDefinitionsVerification;
using static SuperMetroid.Verification.Frontend.CeresCinematics.Artwork.ZebesPlanetBandPartsVerification;
using static SuperMetroid.Verification.Enemies.Zoa.ZoaAnimationDefinitionsVerification;
using SuperMetroid.Verification.Enemies.Zoa;
using static SuperMetroid.Verification.Enemies.Zoa.ZoaSpeedDefinitionsVerification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Samus;

namespace SuperMetroid.Verification.Harness;

/// <summary>
/// Dependency-free assertions shared by every verifier source file.
/// </summary>
/// <remarks>
/// Native-state properties are intentionally narrow (<see cref="byte"/>,
/// <see cref="ushort"/>, and <see cref="short"/>), while C# numeric literals and loop
/// expressions naturally begin as <see cref="int"/>. The numeric overloads centralize
/// that checked narrowing so tests can say <c>AssertEqual(0x8000, state.Word, ...)</c>
/// without repeating casts at hundreds of call sites. An out-of-range expectation fails
/// as bad fixture data instead of silently wrapping before the comparison.
/// </remarks>
internal static class VerificationAssert
{
    /// <summary>Fails a verification with its supplied context when a required condition is false.</summary>
    internal static void AssertTrue(bool condition, string context)
    {
        if (!condition)
            throw new InvalidOperationException($"Verification failed: {context}.");
    }

    /// <summary>Compares arbitrary values with the default equality comparer and reports both values on failure.</summary>
    internal static void AssertEqual<T>(T expected, T actual, string context)
    {
        // EqualityComparer<T>.Default handles primitives, records, nullable values, and
        // enums. Constraining T to IEquatable<T> looks attractive but excludes C# enums.
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException(
                $"Verification failed: {context}; expected {expected}, got {actual}.");
        }
    }

    /// <summary>Checks that an integer expectation fits a byte before comparing it.</summary>
    internal static void AssertEqual(int expected, byte actual, string context) =>
        AssertEqual<byte>(checked((byte)expected), actual, context);

    /// <summary>Checks that an integer expectation fits a signed byte before comparing it.</summary>
    internal static void AssertEqual(int expected, sbyte actual, string context) =>
        AssertEqual<sbyte>(checked((sbyte)expected), actual, context);

    /// <summary>Checks that an integer expectation fits a native word before comparing it.</summary>
    internal static void AssertEqual(int expected, ushort actual, string context) =>
        AssertEqual<ushort>(checked((ushort)expected), actual, context);

    /// <summary>Checks that an integer expectation fits a signed word before comparing it.</summary>
    internal static void AssertEqual(int expected, short actual, string context) =>
        AssertEqual<short>(checked((short)expected), actual, context);

    /// <summary>Checks a nullable native word against a safely narrowed integer expectation.</summary>
    internal static void AssertEqual(int expected, ushort? actual, string context) =>
        AssertEqual<ushort?>(checked((ushort)expected), actual, context);

    /// <summary>Compares a color with an expected cartridge word, decoded as a 15-bit color.</summary>
    internal static void AssertEqual(int expectedWord, Bgr555 actual, string context) =>
        AssertEqual(Bgr555.FromWord(checked((ushort)expectedWord)), actual, context);

    /// <summary>Compares an expected color with an actual cartridge word, decoded as a 15-bit color.</summary>
    internal static void AssertEqual(Bgr555 expected, ushort actualWord, string context) =>
        AssertEqual(expected, Bgr555.FromWord(actualWord), context);

    /// <summary>Compares a nullable color with an expected cartridge word.</summary>
    internal static void AssertEqual(int expectedWord, Bgr555? actual, string context) =>
        AssertEqual<Bgr555?>(Bgr555.FromWord(checked((ushort)expectedWord)), actual, context);

    /// <summary>Returns the expected exception, or fails when the action completes without it.</summary>
    internal static TException AssertThrows<TException>(Action action, string context)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException exception)
        {
            return exception;
        }

        throw new InvalidOperationException(
            $"Verification failed: {context}; expected {typeof(TException).Name}.");
    }

    /// <summary>Checks the imported HUD icon tiles against the pinned cartridge image.</summary>
    [VerificationCommand("--lookup-stream-4-hud-icons")]
    internal static int LookupStream4HudIconsCommand()
    {
        var rom=CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)),"HUD icon source revision");
        Suite(nameof(VerifyLookupStream4HudIcons), () => VerifyLookupStream4HudIcons(rom));
        return 0;
    }

    /// <summary>Checks native HUD template cells and their selected tile words.</summary>
    [VerificationCommand("--lookup-stream-4-hud-template")]
    internal static int LookupStream4HudTemplateCommand()
    {
        var rom=CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)),"HUD template source revision");
        Suite(nameof(VerifyLookupStream4HudTemplate), () => VerifyLookupStream4HudTemplate(rom));
        return 0;
    }

    /// <summary>Checks the installed top HUD row against native VRAM placement.</summary>
    [VerificationCommand("--lookup-stream-4-hud-top-row")]
    internal static int LookupStream4HudTopRowCommand()
    {
        var rom=CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)),"HUD top-row source revision");
        Suite(nameof(VerifyLookupStream4HudTopRow), () => VerifyLookupStream4HudTopRow(rom));
        return 0;
    }

    /// <summary>Checks the completed auto-reserve HUD cells and their source layout.</summary>
    [VerificationCommand("--lookup-stream-4-hud-auto")]
    internal static int LookupStream4HudAutoCommand()
    {
        var rom=CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)),"AUTO source revision");
        Suite(nameof(VerifyLookupStream4HudAutoComplete), () => VerifyLookupStream4HudAutoComplete(rom));
        return 0;
    }

    /// <summary>Checks Draygon presentation table boundaries against native addresses.</summary>
    [VerificationCommand("--lookup-stream-4-draygon-layout")]
    internal static int LookupStream4DraygonLayoutCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Draygon layout oracle revision");
        Suite(nameof(VerifyLookupStream4DraygonPresentationLayout), () => VerifyLookupStream4DraygonPresentationLayout(rom));
        return 0;
    }

    /// <summary>Checks Draygon's intro palette phases and CGRAM exposure.</summary>
    [VerificationCommand("--lookup-stream-4-draygon-intro")]
    internal static int LookupStream4DraygonIntroCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Draygon intro oracle revision");
        Suite(nameof(VerifyLookupStream4DraygonIntro), () => VerifyLookupStream4DraygonIntro(rom));
        return 0;
    }

    /// <summary>Checks Draygon sprite palette stages against native color rows.</summary>
    [VerificationCommand("--lookup-stream-4-draygon-sprite")]
    internal static int LookupStream4DraygonSpriteCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Draygon sprite oracle revision");
        Suite(nameof(VerifyLookupStream4DraygonSprite), () => VerifyLookupStream4DraygonSprite(rom));
        return 0;
    }

    /// <summary>Checks Draygon background palette stages and their CGRAM destinations.</summary>
    [VerificationCommand("--lookup-stream-4-draygon-background")]
    internal static int LookupStream4DraygonBackgroundCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Draygon background oracle revision");
        Suite(nameof(VerifyLookupStream4DraygonBackground), () => VerifyLookupStream4DraygonBackground(rom));
        Suite(nameof(VerifyLookupStream4DraygonHealth), () => VerifyLookupStream4DraygonHealth(rom));
        return 0;
    }

    /// <summary>Checks health-dependent Draygon palette selections against native rows.</summary>
    [VerificationCommand("--lookup-stream-4-draygon-health")]
    internal static int LookupStream4DraygonHealthCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Draygon health oracle revision");
        Suite(nameof(VerifyLookupStream4DraygonHealth), () => VerifyLookupStream4DraygonHealth(rom));
        return 0;
    }

    /// <summary>Checks the Norfair reveal room palette and its native timing.</summary>
    [VerificationCommand("--lookup-stream-4-norfair-reveal")]
    internal static int LookupStream4NorfairRevealCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Norfair reveal oracle revision");
        Suite(nameof(VerifyLookupStream4NorfairReveal), () => VerifyLookupStream4NorfairReveal(rom));
        return 0;
    }

    /// <summary>Checks both normal and escape Ceres door paint against cartridge colors.</summary>
    [VerificationCommand("--lookup-stream5-ceres-door-materials")]
    internal static int LookupStream5CeresDoorMaterialsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres door material oracle revision");
        Suite(nameof(VerifyLookupStream5CeresNormalPaint), () => VerifyLookupStream5CeresNormalPaint(rom));
        Suite(nameof(VerifyLookupStream5CeresEscapePaint), () => VerifyLookupStream5CeresEscapePaint(rom));
        return 0;
    }

    /// <summary>Checks Mother Brain room fade colors used after the exploded door.</summary>
    [VerificationCommand("--lookup-stream3-exploded-door-paints")]
    internal static int LookupStream3ExplodedDoorPaintsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Exploded door palette oracle revision");
        Suite(nameof(VerifyStream3MotherBrainFades), () => VerifyStream3MotherBrainFades(rom));
        Console.WriteLine("Exploded door:14 native colors,42 independent channel edits,no stock array,hashes,bounds and instance isolation pass; other death fades preserved.");
        return 0;
    }

    /// <summary>Checks auxiliary Mother Brain colors and Baby Metroid pulse output.</summary>
    [VerificationCommand("--lookup-stream3-auxiliary-complete")]
    internal static int LookupStream3AuxiliaryCompleteCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Auxiliary palette oracle revision");
        Suite(nameof(VerifyStream3AuxiliaryPalettes), () => VerifyStream3AuxiliaryPalettes(rom));
        Suite(nameof(VerifyStream3ShitroidPulse), () => VerifyStream3ShitroidPulse(rom));
        Console.WriteLine("Auxiliary palettes:393 native colors,1179 independent RGB edits,zero stock endpoint/corpse arrays,identities,bounds and standalone target isolation pass.");
        return 0;
    }

    /// <summary>Checks Ceres platform composition and installed visual words.</summary>
    [VerificationCommand("--lookup-stream5-ceres-platform")]
    internal static int LookupStream5CeresPlatformCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres platform oracle revision");
        Suite(nameof(VerifyLookupStream5CeresPlatform), () => VerifyLookupStream5CeresPlatform(rom));
        return 0;
    }

    /// <summary>Checks Ceres source pages, overlay words, and escape VRAM transfers together.</summary>
    [VerificationCommand("--lookup-stream5-ceres-overlay-complete")]
    internal static int LookupStream5CeresOverlayCompleteCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres overlay native revision");
        Suite(nameof(VerifyLookupStream5CeresSourcePages), () => VerifyLookupStream5CeresSourcePages());
        Suite(nameof(VerifyLookupStream5CeresOverlayWords), () => VerifyLookupStream5CeresOverlayWords(rom));
        Suite(nameof(VerifyCeresEscapeVramTransferDefinitions), () => VerifyCeresEscapeVramTransferDefinitions(rom));
        return 0;
    }

    /// <summary>Checks imported projectile instruction programs against native words.</summary>
    [VerificationCommand("--lookup-stream-1-projectile-programs")]
    internal static int LookupStream1ProjectileProgramsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Projectile program oracle revision");
        Suite(nameof(VerifyLookupStream1ProjectilePrograms), () => VerifyLookupStream1ProjectilePrograms(rom));
        return 0;
    }

    /// <summary>Checks projectile radius records and their native table layout.</summary>
    [VerificationCommand("--lookup-stream-1-projectile-radii")]
    internal static int LookupStream1ProjectileRadiiCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Projectile radius oracle revision");
        Suite(nameof(VerifyLookupStream1ProjectileRadiusLayout), () => VerifyLookupStream1ProjectileRadiusLayout(rom));
        return 0;
    }

    /// <summary>Checks world-map foreground characters and placement against native data.</summary>
    [VerificationCommand("--lookup-stream-1-world-foreground")]
    internal static int LookupStream1WorldForegroundCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "World foreground oracle revision");
        Suite(nameof(VerifyLookupStream1WorldForeground), () => VerifyLookupStream1WorldForeground(rom));
        return 0;
    }

    /// <summary>Checks world-map background characters and placement against native data.</summary>
    [VerificationCommand("--lookup-stream-1-world-background")]
    internal static int LookupStream1WorldBackgroundCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "World BG3 oracle revision");
        Suite(nameof(VerifyLookupStream1WorldBackground), () => VerifyLookupStream1WorldBackground(rom));
    
        return 0;
    }

    /// <summary>Checks Samus death sprite pixels against the pinned artwork source.</summary>
    [VerificationCommand("--lookup-stream-1-death-pixels")]
    internal static int LookupStream1DeathPixelsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Death pixel oracle revision");
        Suite(nameof(VerifyLookupStream1DeathPixels), () => VerifyLookupStream1DeathPixels(rom));
    
        return 0;
    }

    /// <summary>Checks ending explosion foreground palette holds and star handoff timing.</summary>
    [VerificationCommand("--lookup-stream-4-foreground-cadence")]
    internal static int LookupStream4ForegroundCadenceCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Foreground cadence oracle revision");
        Suite(nameof(VerifyLookupStream4ForegroundCadence), () => VerifyLookupStream4ForegroundCadence(rom));
        return 0;
    }

    /// <summary>Checks title ambient palette loops together with their installed colors.</summary>
    [VerificationCommand("--lookup-stream-4-title-ambient-complete")]
    internal static int LookupStream4TitleAmbientCompleteCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Title ambient oracle revision");
        Suite(nameof(VerifyLookupStream4TitleAmbientMechanics), () => VerifyLookupStream4TitleAmbientMechanics(rom));
        Suite(nameof(VerifyLookupStream4TitleAmbientComplete), () => VerifyLookupStream4TitleAmbientComplete(rom));
        return 0;
    }

    /// <summary>Checks Baby Metroid pulse and initial paint targets used by the normal phase.</summary>
    [VerificationCommand("--lookup-stream3-baby-normal-target")]
    internal static int LookupStream3BabyNormalTargetCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Baby normal/target oracle revision");
        Suite(nameof(VerifyStream3ShitroidPulse), () => VerifyStream3ShitroidPulse(rom));
        Suite(nameof(VerifyStream3BabyInitialPaints), () => VerifyStream3BabyInitialPaints(rom));
        Console.WriteLine("Baby normal/target: native outputs, independent pulse/target/initial edits, bidirectional isolation, identities and bounds pass.");
        return 0;
    }

    /// <summary>Checks the drained Samus pose geometry against cartridge offsets.</summary>
    [VerificationCommand("--lookup-stream-1-drained-geometry")]
    internal static int LookupStream1DrainedGeometryCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Drained geometry oracle revision");
        Suite(nameof(VerifyLookupStream1DrainedGeometry), () => VerifyLookupStream1DrainedGeometry(rom));
        return 0;
    }

    /// <summary>Checks Baby Metroid initial paint and its fade sequence.</summary>
    [VerificationCommand("--lookup-stream3-baby-initial-fade")]
    internal static int LookupStream3BabyInitialFadeCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Baby palette oracle revision");
        Suite(nameof(VerifyStream3BabyInitialPaints), () => VerifyStream3BabyInitialPaints(rom));
        Suite(nameof(VerifyStream3BabyFade), () => VerifyStream3BabyFade(rom));
        return 0;
    }

    /// <summary>Checks Samus posture hitbox dimensions and offsets against native records.</summary>
    [VerificationCommand("--lookup-stream-1-posture-geometry")]
    internal static int LookupStream1PostureGeometryCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Posture geometry oracle revision");
        Suite(nameof(VerifyLookupStream1PostureGeometry), () => VerifyLookupStream1PostureGeometry(rom));
        return 0;
    }

    /// <summary>Checks Samus body facing offsets that determine landing placement.</summary>
    [VerificationCommand("--lookup-stream-1-landing-placement")]
    internal static int LookupStream1LandingPlacementCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Landing placement oracle revision");
        Suite(nameof(VerifyLookupStream1BodyFacingOffsets), () => VerifyLookupStream1BodyFacingOffsets(rom));
        return 0;
    }

    /// <summary>Checks Metroid color pulse frames and their native cadence.</summary>
    [VerificationCommand("--lookup-stream-1-metroid-pulse")]
    internal static int LookupStream1MetroidPulseCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Metroid pulse oracle revision");
        Suite(nameof(VerifyLookupStream1MetroidPulse), () => VerifyLookupStream1MetroidPulse(rom));
        return 0;
    }

    /// <summary>Checks atmospheric animation frame durations against cartridge timing.</summary>
    [VerificationCommand("--lookup-stream-1-atmospheric-cadence")]
    internal static int LookupStream1AtmosphericCadenceCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Atmospheric cadence oracle revision");
        Suite(nameof(VerifyLookupStream1AtmosphericCadence), () => VerifyLookupStream1AtmosphericCadence(rom));
        Console.WriteLine("Atmospheric cadence:37 native holds, seven domains,74 actual expiry/delayed-start cases and rejection contracts pass.");
        return 0;
    }

    /// <summary>Checks rainbow beam and drain fade material colors against source artwork.</summary>
    [VerificationCommand("--lookup-stream3-rainbow-materials")]
    internal static int LookupStream3RainbowMaterialsCommand()
    {
        var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(), Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Rainbow palette oracle revision");
        Suite(nameof(VerifyStream3DrainFades), () => VerifyStream3DrainFades(oracle));
        Console.WriteLine("Rainbow material palettes: native colors, routing, independent edits and reduced material basis pass.");
        return 0;
    }

    /// <summary>Checks ghost palette rows and their native color selection.</summary>
    [VerificationCommand("--lookup-stream2-ghost-palette")]
    internal static int LookupStream2GhostPaletteCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(), Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ghost palette oracle revision");
        Suite(nameof(VerifyLookupStream2GhostPalette), () => VerifyLookupStream2GhostPalette(rom));
        return 0;
    }

    /// <summary>Checks escape timer update cadence against the authored timing program.</summary>
    [VerificationCommand("--lookup-stream-1-timer-cadence")]
    internal static int LookupStream1TimerCadenceCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Timer cadence oracle revision");
        Suite(nameof(VerifyLookupStream1TimerCadence), () => VerifyLookupStream1TimerCadence(rom));
        return 0;
    }

    /// <summary>Checks timer digits and glyph tile identities against native artwork.</summary>
    [VerificationCommand("--lookup-stream-1-timer-glyphs")]
    internal static int LookupStream1TimerGlyphsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(), Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Timer glyph oracle revision");
        Suite(nameof(VerifyLookupStream1TimerGlyphs), () => VerifyLookupStream1TimerGlyphs(rom));
        return 0;
    }

    /// <summary>Checks compiled slope movement speeds against native fixed-point values.</summary>
    [VerificationCommand("--lookup-stream5-slope-speeds")]
    internal static int LookupStream5SlopeSpeedsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(), Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Slope speed oracle revision");
        Suite(nameof(VerifyCompiledSlopeSpeeds), () => VerifyCompiledSlopeSpeeds(rom));
        AssertThrows<ArgumentOutOfRangeException>(() => SlopeSpeedDefinitions.HorizontalMultiplier(-1), "Slope family lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => SlopeSpeedDefinitions.HorizontalMultiplier(32), "Slope family upper bound");
        return 0;
    }

    /// <summary>Checks Crocomire shared paint and palette reuse against native colors.</summary>
    [VerificationCommand("--lookup-stream5-crocomire-paint")]
    internal static int LookupStream5CrocomirePaintCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(), Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Crocomire palette oracle revision");
        Suite(nameof(VerifyLookupStream5CrocomireSharedPaint), () => VerifyLookupStream5CrocomireSharedPaint(rom));
        return 0;
    }

    /// <summary>Checks Mother Brain falling-tube population records and slot layout.</summary>
    [VerificationCommand("--lookup-stream3-falling-tube-population-layout")]
    internal static int LookupStream3FallingTubePopulationLayoutCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(), Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Falling tube oracle revision");
        Suite(nameof(VerifyStream3FallingTubePopulationLayout), () => VerifyStream3FallingTubePopulationLayout(rom));
        return 0;
    }

    /// <summary>Checks intro palette rows against the pinned cartridge colors.</summary>
    [VerificationCommand("--lookup-stream3-intro-palette-rows")]
    internal static int LookupStream3IntroPaletteRowsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(), Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Intro palette oracle revision");
        Suite(nameof(VerifyStream3IntroPaletteRows), () => VerifyStream3IntroPaletteRows(rom));
        return 0;
    }

    /// <summary>Checks intro eye rectangles and divider placement against native layout data.</summary>
    [VerificationCommand("--lookup-stream3-intro-layouts")]
    internal static int LookupStream3IntroLayoutsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(), Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Intro layout oracle revision");
        Suite(nameof(VerifyStream3IntroEyeRectangles), () => VerifyStream3IntroEyeRectangles(rom));
        Suite(nameof(VerifyStream3IntroDivider), () => VerifyStream3IntroDivider(rom));
        return 0;
    }

    /// <summary>Checks Ceres graphics source pages used by escape VRAM transfers.</summary>
    [VerificationCommand("--lookup-stream5-ceres-source-pages")]
    internal static int LookupStream5CeresSourcePagesCommand()
    {
        var ceresPageOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(), Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ceresPageOracle.Rom)), "Ceres source-page oracle revision");
        Suite(nameof(VerifyLookupStream5CeresSourcePages), () => VerifyLookupStream5CeresSourcePages());
        Suite(nameof(VerifyCeresEscapeVramTransferDefinitions), () => VerifyCeresEscapeVramTransferDefinitions(ceresPageOracle));
        return 0;
    }

    /// <summary>Checks X-ray Samus body frames before final presentation overrides.</summary>
    [VerificationCommand("--lookup-stream-1-body-frame-basis")]
    internal static int LookupStream1BodyFrameBasisCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Body frame oracle revision");
        Suite(nameof(VerifyLookupStream1XrayBodyFrames), () => VerifyLookupStream1XrayBodyFrames(rom, true));
        return 0;
    }

    /// <summary>Checks final X-ray body frame composition after presentation selection.</summary>
    [VerificationCommand("--lookup-stream-1-body-frame-final")]
    internal static int LookupStream1BodyFrameFinalCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Body frame oracle revision");
        Suite(nameof(VerifyLookupStream1XrayBodyFrames), () => VerifyLookupStream1XrayBodyFrames(rom, false, true));
        return 0;
    }

    /// <summary>Checks Samus body origins, pose pointers, and facing offsets together.</summary>
    [VerificationCommand("--lookup-stream-1-body-facing")]
    [VerificationCommand("--lookup-stream-1-xray-frames")]
    internal static int LookupStream1BodyFacingCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Body oracle revision");
        Suite(nameof(VerifyLookupStream1BodyFacingOffsets), () => VerifyLookupStream1BodyFacingOffsets(rom));
        Suite(nameof(VerifyLookupStream1BodyGraphicsOrigins), () => VerifyLookupStream1BodyGraphicsOrigins(rom));
        Suite(nameof(VerifyLookupStream1BodySetPointers), () => VerifyLookupStream1BodySetPointers(rom));
        Suite(nameof(VerifyLookupStream1BodyPosePointers), () => VerifyLookupStream1BodyPosePointers(rom));
        Suite(nameof(VerifyLookupStream1XrayBodyFrames), () => VerifyLookupStream1XrayBodyFrames(rom));
        return 0;
    }

    /// <summary>Checks body OAM base positions and tile indexes against cartridge tables.</summary>
    [VerificationCommand("--lookup-stream-1-body-oam-bases")]
    internal static int LookupStream1BodyOamBasesCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "OAM base oracle revision");
        Suite(nameof(VerifyLookupStream1BodyOamBases), () => VerifyLookupStream1BodyOamBases(rom));
        return 0;
    }

    /// <summary>Checks Samus hurt blend colors and their installed palette output.</summary>
    [VerificationCommand("--lookup-stream-1-hurt-blend")]
    internal static int LookupStream1HurtBlendCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Hurt palette oracle revision");
        Suite(nameof(VerifyLookupStream1HurtBlend), () => VerifyLookupStream1HurtBlend(rom));
        Suite(nameof(VerifySamusHurtColors), () => VerifySamusHurtColors());
        return 0;
    }

    /// <summary>Checks Mother Brain hand-beam body geometry against native layout.</summary>
    [VerificationCommand("--lookup-stream3-hand-beam-body-layout")]
    internal static int LookupStream3HandBeamBodyLayoutCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Hand-beam body oracle revision");
        return 0;
    }

    /// <summary>Checks map landmark case selection and authored anchor positions.</summary>
    [VerificationCommand("--lookup-stream5-map-landmarks")]
    internal static int LookupStream5MapLandmarksCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Map landmark oracle revision");
        Suite(nameof(VerifyLookupStream5MapLandmarkCases), () => VerifyLookupStream5MapLandmarkCases(rom));
        return 0;
    }

    /// <summary>Checks Baby Metroid instruction words and their native program layout.</summary>
    [VerificationCommand("--lookup-stream3-baby-instruction-layout")]
    internal static int LookupStream3BabyInstructionLayoutCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Baby instruction oracle revision");
        Console.WriteLine("Baby command layout: twelve native words, nine visual addresses and rejection domains pass; six hold roles remain required.");
        return 0;
    }

    /// <summary>Checks Baby Metroid sprite reflection and shared body frame ownership.</summary>
    [VerificationCommand("--lookup-stream3-baby-body-sharing")]
    internal static int LookupStream3BabyBodySharingCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Baby OAM oracle revision");
        Suite(nameof(VerifyStream3BabySpriteReflection), () => VerifyStream3BabySpriteReflection(rom));
        Console.WriteLine("Baby upper-body sharing: native OAM/order/hash, independent symmetric edits and legacy schemas pass; geometry/artwork remain required.");
        return 0;
    }

    /// <summary>Checks hand-beam geometry and projectile instruction mechanics together.</summary>
    [VerificationCommand("--lookup-stream3-hand-beam-layout")]
    internal static int LookupStream3HandBeamLayoutCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Hand-beam oracle revision");
        Suite(nameof(MotherBrainEffectBehaviorVerification.VerifyMotherBrainEffectBehavior), () => MotherBrainEffectBehaviorVerification.VerifyMotherBrainEffectBehavior(rom));
        Console.WriteLine("Hand-beam command layout: stage/callback/visual order, exact byte ownership and production checks pass; selected holds remain required.");
        return 0;
    }

    /// <summary>Checks Phantoon's authored phase delays and flame timers against native words.</summary>
    [VerificationCommand("--lookup-stream5-phantoon-timers")]
    internal static int LookupStream5PhantoonTimersCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Phantoon timer oracle revision");
        Suite(nameof(VerifyCompiledPhantoonTimers), () => VerifyCompiledPhantoonTimers(rom));
        Suite(nameof(VerifyLookupStream5PhantoonExposure), () => VerifyLookupStream5PhantoonExposure(rom));
        Suite(nameof(VerifyLookupStream5PhantoonClosedEye), () => VerifyLookupStream5PhantoonClosedEye(rom));
        Suite(nameof(VerifyLookupStream5PhantoonRainWait), () => VerifyLookupStream5PhantoonRainWait(rom));
        return 0;
    }

    /// <summary>Checks power-bomb program words and selector order against cartridge data.</summary>
    [VerificationCommand("--lookup-stream-1-power-programs")]
    internal static int LookupStream1PowerProgramsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Projectile program oracle revision");
        return 0;
    }

    /// <summary>Checks Phantoon's collision geometry and hitbox dispatch against native records.</summary>
    [VerificationCommand("--lookup-stream5-phantoon-collision")]
    internal static int LookupStream5PhantoonCollisionCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Phantoon collision oracle revision");
        Suite(nameof(VerifyLookupStream5PhantoonCollision), () => VerifyLookupStream5PhantoonCollision(rom));
        return 0;
    }

    /// <summary>Checks Phantoon's fade phases and RGB5 transition values.</summary>
    [VerificationCommand("--lookup-stream5-phantoon-fade")]
    internal static int LookupStream5PhantoonFadeCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Phantoon colors oracle revision");
        Suite(nameof(VerifyLookupStream5PhantoonFade), () => VerifyLookupStream5PhantoonFade(rom));
        return 0;
    }

    /// <summary>Checks ending-font space glyphs and their layout boundaries.</summary>
    [VerificationCommand("--lookup-stream-4-ending-font-spaces")]
    [VerificationCommand("--lookup-stream-4-ending-font-outline")]
    internal static int LookupStream4EndingFontSpacesCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending font oracle revision");
        Suite(nameof(VerifyLookupStream4EndingFontSpaces), () => VerifyLookupStream4EndingFontSpaces(rom));
        return 0;
    }

    /// <summary>Checks world-map button graphic identities and placement.</summary>
    [VerificationCommand("--lookup-stream5-map-buttons")]
    internal static int LookupStream5MapButtonsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Map buttons oracle revision");
        Suite(nameof(VerifyLookupStream5MapButtons), () => VerifyLookupStream5MapButtons(rom));
        return 0;
    }

    /// <summary>Checks Mode-7 source pages and transfer destinations against native DMA data.</summary>
    [VerificationCommand("--lookup-stream5-mode7-transfers")]
    internal static int LookupStream5Mode7TransfersCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres Mode7 oracle revision");
        Suite(nameof(VerifyCompiledCeresRidleyMode7Transfers), () => VerifyCompiledCeresRidleyMode7Transfers(rom));
        return 0;
    }

    /// <summary>Checks pause-menu border tiles and their drawing layout.</summary>
    [VerificationCommand("--lookup-stream3-menu-borders")]
    internal static int LookupStream3MenuBordersCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Menu borders oracle revision");
        Suite(nameof(VerifyStream3FileSelectBorders), () => VerifyStream3FileSelectBorders(rom, FileSelectPresentationExtractor.Extract(rom)));
        Suite(nameof(VerifyStream3OptionsBorders), () => VerifyStream3OptionsBorders(rom, GameOptionsPresentationExtractor.Extract(rom)));
        Console.WriteLine("Menu borders:258native parts,1032independent edits,six reordered/expanded compositions pass.");
        return 0;
    }

    /// <summary>Checks Maridia room palette words and installed color selection.</summary>
    [VerificationCommand("--lookup-stream-4-maridia-colors")]
    internal static int LookupStream4MaridiaColorsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Maridia colors oracle revision");
        Suite(nameof(VerifyLookupStream4MaridiaColors), () => VerifyLookupStream4MaridiaColors(rom));
        return 0;
    }

    /// <summary>Checks planet title glyphs and tile positions against source data.</summary>
    [VerificationCommand("--lookup-stream-4-planet-text")]
    internal static int LookupStream4PlanetTextCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Planet text oracle revision");
        Suite(nameof(VerifyLookupStream4PlanetText), () => VerifyLookupStream4PlanetText(rom));
        return 0;
    }

    /// <summary>Checks Ceres palette changes driven by Ridley's health phase.</summary>
    [VerificationCommand("--lookup-stream-4-ceres-health")]
    internal static int LookupStream4CeresHealthCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres health oracle revision");
        Suite(nameof(VerifyLookupStream4CeresHealth), () => VerifyLookupStream4CeresHealth(rom));
        return 0;
    }

    /// <summary>Checks Ceres zoom and retreat palette stages in their native order.</summary>
    [VerificationCommand("--lookup-stream-4-ceres-zoom-retreat")]
    internal static int LookupStream4CeresZoomRetreatCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres zoom oracle revision");
        Suite(nameof(VerifyLookupStream4CeresZoomAndRetreat), () => VerifyLookupStream4CeresZoomAndRetreat(rom));
        return 0;
    }

    /// <summary>Checks colors reused by the Ceres retreat stages.</summary>
    [VerificationCommand("--lookup-stream-4-ceres-retreat-shared")]
    internal static int LookupStream4CeresRetreatSharedCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres retreat oracle revision");
        Suite(nameof(VerifyLookupStream4CeresRetreatShared), () => VerifyLookupStream4CeresRetreatShared(rom));
        return 0;
    }

    /// <summary>Checks the initial Ceres room colors loaded before Ridley moves.</summary>
    [VerificationCommand("--lookup-stream-4-ceres-start")]
    [VerificationCommand("--lookup-stream-4-ceres-baby")]
    internal static int LookupStream4CeresStartCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres start oracle revision");
        Suite(nameof(VerifyLookupStream4CeresStart), () => VerifyLookupStream4CeresStart(rom));
        Suite(nameof(VerifyLookupStream4CeresBaby), () => VerifyLookupStream4CeresBaby(rom));
        return 0;
    }

    /// <summary>Checks Ceres fade timing, rows, and destination colors.</summary>
    [VerificationCommand("--lookup-stream-4-ceres-fades")]
    internal static int LookupStream4CeresFadesCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres fades oracle revision");
        Suite(nameof(VerifyLookupStream4CeresFades), () => VerifyLookupStream4CeresFades(rom));
        return 0;
    }

    /// <summary>Checks beam palette paint across its selected variants.</summary>
    [VerificationCommand("--lookup-stream-4-beam-paint")]
    internal static int LookupStream4BeamPaintCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(), Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Beam paint oracle revision");
        Suite(nameof(VerifyLookupStream4BeamColorRelations), () => VerifyLookupStream4BeamColorRelations(rom));
        return 0;
    }

    /// <summary>Checks the Norfair initial room palette and CGRAM exposure.</summary>
    [VerificationCommand("--lookup-stream-4-norfair-initial")]
    internal static int LookupStream4NorfairInitialCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(), Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Norfair initial oracle revision");
        Suite(nameof(VerifyLookupStream4NorfairInitial), () => VerifyLookupStream4NorfairInitial(rom));
        return 0;
    }

    /// <summary>Checks Botwoon health-dependent color rows against the pinned image.</summary>
    [VerificationCommand("--lookup-stream-4-botwoon-health")]
    internal static int LookupStream4BotwoonHealthCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(), Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Botwoon palette oracle revision");
        Suite(nameof(VerifyLookupStream4BotwoonColors), () => VerifyLookupStream4BotwoonColors(rom));
        return 0;
    }

    /// <summary>Checks Ceres alarm palette cadence and room colors.</summary>
    [VerificationCommand("--lookup-stream-4-ceres-alarm")]
    internal static int LookupStream4CeresAlarmCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres alarm oracle revision");
        Suite(nameof(VerifyLookupStream4CeresAlarm), () => VerifyLookupStream4CeresAlarm(rom));
        return 0;
    }

    /// <summary>Checks Maridia palette definition addresses and selected color rows.</summary>
    [VerificationCommand("--lookup-stream-4-maridia-palette")]
    internal static int LookupStream4MaridiaPaletteCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Maridia palette oracle revision");
        Suite(nameof(VerifyLookupStream4MaridiaPaletteDefinitions), () => VerifyLookupStream4MaridiaPaletteDefinitions(rom));
        return 0;
    }

    /// <summary>Checks ending shake displacements and wait lengths against native instructions.</summary>
    [VerificationCommand("--lookup-stream-4-ending-shake")]
    internal static int LookupStream4EndingShakeCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending shake oracle revision");
        Suite(nameof(VerifyLookupStream4EndingShake), () => VerifyLookupStream4EndingShake(rom));
        return 0;
    }

    /// <summary>Checks OAM pointer tables used to select Samus body frames.</summary>
    [VerificationCommand("--lookup-stream-1-oam-pointers")]
    internal static int LookupStream1OamPointersCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(), Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "OAM selector oracle revision");
        Suite(nameof(VerifyLookupStream1OamPointers), () => VerifyLookupStream1OamPointers(rom));
        return 0;
    }

    /// <summary>Checks Samus body graphics DMA sources and destination layout.</summary>
    [VerificationCommand("--lookup-stream-1-body-transfers")]
    internal static int LookupStream1BodyTransfersCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Body transfer oracle revision");
        Suite(nameof(VerifyLookupStream1BodyTransfers), () => VerifyLookupStream1BodyTransfers(rom));
        return 0;
    }

    /// <summary>Checks arm-cannon origins relative to Samus poses.</summary>
    [VerificationCommand("--lookup-stream-1-cannon-placement")]
    internal static int LookupStream1CannonPlacementCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Cannon placement oracle revision");
        Suite(nameof(VerifyLookupStream1CannonPlacement), () => VerifyLookupStream1CannonPlacement(rom));
        return 0;
    }

    /// <summary>Checks Samus animation pointer and frame-table identities.</summary>
    [VerificationCommand("--lookup-stream-1-animation-pointers")]
    internal static int LookupStream1AnimationPointersCommand()
    {
        string sourceRom = Path.GetFullPath("Super Metroid.smc");
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Animation pointer oracle revision");
        Suite(nameof(VerifySamusAnimationDelayDefinitions), () => VerifySamusAnimationDelayDefinitions(rom));
        Suite(nameof(VerifyLookupStream1AnimationAliases), () => VerifyLookupStream1AnimationAliases(rom));
        return 0;
    }

    /// <summary>Checks Powamp instruction programs and their frame selections.</summary>
    [VerificationCommand("--lookup-stream-1-powamp-programs")]
    internal static int LookupStream1PowampProgramsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Powamp program oracle revision");
        Suite(nameof(PowampBehaviorVerification.VerifyPowampBehavior), () => PowampBehaviorVerification.VerifyPowampBehavior(rom));
        return 0;
    }

    /// <summary>Checks escape timer tile and layout definitions.</summary>
    [VerificationCommand("--lookup-stream-1-timer-layout")]
    internal static int LookupStream1TimerLayoutCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Timer layout oracle revision");
        Suite(nameof(VerifyLookupStream1TimerLayout), () => VerifyLookupStream1TimerLayout(rom));
        return 0;
    }

    /// <summary>Checks charge-flare offsets relative to Samus's body frames.</summary>
    [VerificationCommand("--lookup-stream-1-flare-placement")]
    internal static int LookupStream1FlarePlacementCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Flare placement oracle revision");
        Suite(nameof(VerifyLookupStream1FlarePlacement), () => VerifyLookupStream1FlarePlacement(rom));
        return 0;
    }

    /// <summary>Checks Mochtroid sprite composition and frame selection against cartridge data.</summary>
    [VerificationCommand("--lookup-mochtroid-visuals")]
    internal static int LookupMochtroidVisualsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Mochtroid oracle revision");
        Suite(nameof(VerifyStream3MochtroidVisuals), () => VerifyStream3MochtroidVisuals(rom));
        Suite(nameof(MochtroidBehaviorVerification.VerifyMochtroidBehavior), () => MochtroidBehaviorVerification.VerifyMochtroidBehavior());
        Console.WriteLine("Mochtroid: six native registrations and the production animation checks pass.");
        return 0;
    }

    /// <summary>Checks compiled environmental visual catalogs against source artwork.</summary>
    [VerificationCommand("--lookup-stream2-environmental-catalogs")]
    internal static int LookupStream2EnvironmentalCatalogsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Environmental catalog oracle revision");
        Suite(nameof(VerifyLookupStream2EnvironmentalCatalogs), () => VerifyLookupStream2EnvironmentalCatalogs(rom));
        return 0;
    }

    /// <summary>Checks background tilemap geometry and screen placement.</summary>
    [VerificationCommand("--lookup-stream2-backdrop-geometry")]
    internal static int LookupStream2BackdropGeometryCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Backdrop oracle revision");
        Suite(nameof(VerifyLookupStream2BackdropGeometry), () => VerifyLookupStream2BackdropGeometry(rom));
        return 0;
    }

    /// <summary>Checks menu sprite dimensions and OAM coordinates.</summary>
    [VerificationCommand("--lookup-menu-sprite-geometry")]
    internal static int LookupMenuSpriteGeometryCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Menu geometry oracle revision");
        Suite(nameof(VerifyStream3MenuSpriteGeometry), () => VerifyStream3MenuSpriteGeometry(rom));
        Console.WriteLine("Menu geometry: native compositions, calculated stock storage, independent part edits, reordering and three loader bindings pass.");
        return 0;
    }

    /// <summary>Checks equipment-screen base tiles and their VRAM geometry.</summary>
    [VerificationCommand("--lookup-stream2-equipment-base-geometry")]
    internal static int LookupStream2EquipmentBaseGeometryCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Equipment template oracle revision");
        Suite(nameof(VerifyLookupStream2EquipmentBaseGeometry), () => VerifyLookupStream2EquipmentBaseGeometry(rom));
        return 0;
    }

    /// <summary>Checks mirrored pause-menu wireframe tiles against native output.</summary>
    [VerificationCommand("--lookup-stream2-wireframe-mirrors")]
    internal static int LookupStream2WireframeMirrorsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Wireframe oracle revision");
        Suite(nameof(VerifyLookupStream2WireframeMirrors), () => VerifyLookupStream2WireframeMirrors(rom));
        return 0;
    }

    /// <summary>Checks intro caret actor registration and its native draw identity.</summary>
    [VerificationCommand("--lookup-intro-caret-registration")]
    internal static int LookupIntroCaretRegistrationCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(1, IntroCaretSpriteDefinitions.Frames.Count, "Caret registration count");
        VerifyIntroCaretNativePartCount(rom);
        AssertEqual(new IntroCaretFrameDefinition(0x8d68, "caret-visible", 1), IntroCaretSpriteDefinitions.Frames.Single(), "Caret identity and enumeration");
        foreach (int invalid in new[] { -1, 1, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => _ = IntroCaretSpriteDefinitions.Frames[invalid], "Caret index bounds");
        Console.WriteLine("Caret registration: native part count, identity, enumeration and bounds pass.");
        return 0;
    }

    /// <summary>Checks enemy frame selectors are registered to the intended visual owners.</summary>
    [VerificationCommand("--lookup-enemy-frame-registration")]
    internal static int LookupEnemyFrameRegistrationCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Enemy registration oracle revision");
        Suite(nameof(VerifyStream3EnemyFrameRegistration), () => VerifyStream3EnemyFrameRegistration(rom));
        return 0;
    }

    /// <summary>Checks compact gameplay tables against their cartridge words.</summary>
    [VerificationCommand("--lookup-small-game-tables")]
    internal static int LookupSmallGameTablesCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Small game tables oracle revision");
        Suite(nameof(VerifyLookupSmallGameTables), () => VerifyLookupSmallGameTables(rom));
        return 0;
    }

    /// <summary>Checks Dachora palette colors and color-row identities.</summary>
    [VerificationCommand("--lookup-dachora-colors")]
    internal static int LookupDachoraColorsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Dachora colors oracle revision");
        Suite(nameof(VerifyLookupDachoraColors), () => VerifyLookupDachoraColors(rom));
        return 0;
    }

    /// <summary>Checks Spore Spawn health color progression against native rows.</summary>
    [VerificationCommand("--lookup-stream-4-spore-health-ramp")]
    internal static int LookupStream4SporeHealthRampCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Spore health ramp oracle revision");
        Suite(nameof(VerifyLookupStream4SporeHealthRamp), () => VerifyLookupStream4SporeHealthRamp(rom));
        return 0;
    }

    /// <summary>Checks Spore Spawn level, background, and sprite fade phases.</summary>
    [VerificationCommand("--lookup-stream-4-spore-fade")]
    internal static int LookupStream4SporeFadeCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Spore fade oracle revision");
        Suite(nameof(VerifyLookupStream4SporeSpriteFade), () => VerifyLookupStream4SporeSpriteFade(rom));
        Suite(nameof(VerifyLookupStream4SporeBackgroundFade), () => VerifyLookupStream4SporeBackgroundFade(rom));
        Suite(nameof(VerifyLookupStream4SporeLevelFade), () => VerifyLookupStream4SporeLevelFade(rom));
        Suite(nameof(VerifyLookupStream4SporeHealthyAlias), () => VerifyLookupStream4SporeHealthyAlias(rom));
        return 0;
    }

    /// <summary>Checks beam color basis inputs and their shared palette relationships.</summary>
    [VerificationCommand("--lookup-stream-4-beam-basis")]
    internal static int LookupStream4BeamBasisCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Beam basis oracle revision");
        Suite(nameof(VerifyLookupStream4BeamColorRelations), () => VerifyLookupStream4BeamColorRelations(rom));
        Console.WriteLine("Beam basis: exactly43 required stored colors and149 calculated values; all native outputs, independent edits and CGRAM isolation pass.");
        return 0;
    }

    /// <summary>Checks arm-cannon base tiles and origins used by selected poses.</summary>
    [VerificationCommand("--lookup-stream-1-cannon-basis")]
    internal static int LookupStream1CannonBasisCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Cannon basis oracle revision");
        Suite(nameof(VerifyLookupStream1ArmCannonTileSources), () => VerifyLookupStream1ArmCannonTileSources(rom));
        Console.WriteLine("Cannon calculated defaults, zero stock overrides, native selectors, edits and DMA checks passed.");
        return 0;
    }

    /// <summary>Checks ending subtitle characters and placement against native artwork.</summary>
    [VerificationCommand("--lookup-stream-4-ending-subtitle")]
    internal static int LookupStream4EndingSubtitleCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Subtitle oracle revision");
        Suite(nameof(VerifyLookupStream4EndingSubtitle), () => VerifyLookupStream4EndingSubtitle(rom));
        return 0;
    }

    /// <summary>Checks ending result-panel tile composition and text space.</summary>
    [VerificationCommand("--lookup-stream-4-ending-panel")]
    internal static int LookupStream4EndingPanelCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Producer panel oracle revision");
        Suite(nameof(VerifyLookupStream4EndingResultPanel), () => VerifyLookupStream4EndingResultPanel(rom));
        Suite(nameof(VerifyLookupStream4EndingSubtitle), () => VerifyLookupStream4EndingSubtitle(rom));
        return 0;
    }

    /// <summary>Checks Ridley's tail-rest coordinates against native geometry.</summary>
    [VerificationCommand("--lookup-stream-4-tail-rest")]
    internal static int LookupStream4TailRestCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Tail rest oracle revision");
        Suite(nameof(VerifyLookupStream4TailRestGeometry), () => VerifyLookupStream4TailRestGeometry(rom));
        return 0;
    }

    /// <summary>Checks Baby Metroid transfer-phase tiles at their native VRAM positions.</summary>
    [VerificationCommand("--lookup-stream-4-baby-phase")]
    internal static int LookupStream4BabyPhaseCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Baby phase oracle revision");
        Suite(nameof(VerifyLookupStream4BabyTransferPhase), () => VerifyLookupStream4BabyTransferPhase(rom));
        return 0;
    }

    /// <summary>Checks Samus body sprite pixel data against the pinned cartridge.</summary>
    [VerificationCommand("--lookup-stream-1-body-pixels")]
    internal static int LookupStream1BodyPixelsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Body pixel oracle revision");
        Suite(nameof(VerifyLookupStream1BodyPixels), () => VerifyLookupStream1BodyPixels(rom));
        return 0;
    }

    /// <summary>Checks Brinstar pipe bug instruction lists and frame selection.</summary>
    [VerificationCommand("--lookup-stream2-pipe-programs")]
    internal static int LookupStream2PipeProgramsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Stream 2 pipe-program oracle revision");
        Suite(nameof(VerifyPipeBugAnimationDefinitions), () => VerifyPipeBugAnimationDefinitions(rom));
        Suite(nameof(PipeBugAnimationsVerification.VerifyPipeBugAnimations), () => PipeBugAnimationsVerification.VerifyPipeBugAnimations(rom));
        return 0;
    }

    /// <summary>Checks enemy body OAM placement and animation pointers.</summary>
    [VerificationCommand("--lookup-stream2-body-placements")]
    internal static int LookupStream2BodyPlacementsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Stream 2 body-placement oracle revision");
        Suite(nameof(VerifyBombTorizoAttackDefinitions), () => VerifyBombTorizoAttackDefinitions(rom));
        Suite(nameof(VerifyCompiledStatueWalking), () => VerifyCompiledStatueWalking(rom));
        return 0;
    }

    /// <summary>Checks crawler instruction timing and selected poses.</summary>
    [VerificationCommand("--lookup-stream2-crawler-animations")]
    internal static int LookupStream2CrawlerAnimationsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Stream 2 crawler original oracle revision");
        Suite(nameof(VerifyCrawlerAnimationDefinitions), () => VerifyCrawlerAnimationDefinitions(rom));
        Suite(nameof(VerifyWaverAnimationDefinitions), () => VerifyWaverAnimationDefinitions(rom));
        return 0;
    }

    /// <summary>Checks gunship tile transfer source pages using the selected cartridge path.</summary>
    [VerificationCommand("--lookup-stream2-gunship-transfers")]
    [VerificationCommand("--lookup-stream2-reserve-geometry")]
    [VerificationCommand("--lookup-stream2-ghost-norfair")]
    [VerificationCommand("--lookup-stream2-palette-mechanics")]
    [VerificationCommand("--lookup-stream2-yard-directions")]
    [VerificationCommand("--lookup-stream2-crystal-body")]
    [VerificationCommand("--lookup-stream2-kraid-ramps")]
    internal static int LookupStream2GunshipTransfersCommand(string[] args)
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Stream 2 original oracle revision");
        if (args[0] == "--lookup-stream2-gunship-transfers")
            VerifyLookupStream2GunshipTransfers(rom);
        else if (args[0] == "--lookup-stream2-reserve-geometry")
            VerifyLookupStream2ReserveGeometry(rom);
        else if (args[0] == "--lookup-stream2-ghost-norfair")
            VerifyLookupStream2GhostAndNorfair(rom);
        else if (args[0] == "--lookup-stream2-palette-mechanics")
            VerifyLookupStream2PaletteMechanics();
        else if (args[0] == "--lookup-stream2-kraid-ramps")
            VerifyLookupStream2KraidRamps(rom);
        else if (args[0] == "--lookup-stream2-crystal-body")
            VerifyLookupStream2CrystalBody(rom);
        else
        {
            VerifyYardDirectionDefinitions(rom);
            VerifyYardTurnDefinitions(rom);
        }
        return 0;
    }

    /// <summary>Runs the grouped stream-one import checks against the pinned cartridge revision.</summary>
    [VerificationCommand("--lookup-stream-1")]
    internal static int LookupStream1Command()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Stream 1 oracle revision");
        Suite(nameof(CrawlerBehaviorVerification.VerifyCrawlerBehavior), () => CrawlerBehaviorVerification.VerifyCrawlerBehavior());
        Suite(nameof(VerifySamusDeathSequence), () => VerifySamusDeathSequence());
        Suite(nameof(VerifyNuclearWaffleDefinitions), () => VerifyNuclearWaffleDefinitions(rom));
        Suite(nameof(OwtchBehaviorVerification.VerifyOwtchBehavior), () => OwtchBehaviorVerification.VerifyOwtchBehavior());
        Suite(nameof(StokeBehaviorVerification.VerifyStokeBehavior), () => StokeBehaviorVerification.VerifyStokeBehavior());
        Suite(nameof(VerifyPuyoInstructionProgramDefinitions), () => VerifyPuyoInstructionProgramDefinitions());
        Suite(nameof(VerifyMiscDustProjectileDefinitions), () => VerifyMiscDustProjectileDefinitions(rom));
        Suite(nameof(VerifyMetroidsClearedStatePlm), () => VerifyMetroidsClearedStatePlm(new TestAddressSpace()));
        Suite(nameof(MetroidBehaviorVerification.VerifyMetroidBehavior), () => MetroidBehaviorVerification.VerifyMetroidBehavior());
        Suite(nameof(VerifySamusAtmosphericEffectDefinitions), () => VerifySamusAtmosphericEffectDefinitions(rom));
        Suite(nameof(VerifySamusHudDefinitions), () => VerifySamusHudDefinitions(rom));
        Suite(nameof(VerifySamusStoredShineAndShinespark), () => VerifySamusStoredShineAndShinespark());
        Suite(nameof(VerifySamusArmCannonDefinitions), () => VerifySamusArmCannonDefinitions(rom));
        Suite(nameof(VerifyPoseDispatchDefinitions), () => VerifyPoseDispatchDefinitions(rom));
        Suite(nameof(VerifyPoseCollisionDefinitions), () => VerifyPoseCollisionDefinitions(rom));
        Suite(nameof(VerifyPoseProjectileOrigin), () => VerifyPoseProjectileOrigin(rom));
        Suite(nameof(VerifyProjectileOrigins), () => VerifyProjectileOrigins(rom));
        Suite(nameof(VerifyCompiledSpeedBoosterPlmPrograms), () => VerifyCompiledSpeedBoosterPlmPrograms());
        Suite(nameof(VerifyLookupStream1), () => VerifyLookupStream1(rom));
        Suite(nameof(PipeBugBehaviorVerification.VerifyPipeBugBehavior), () => PipeBugBehaviorVerification.VerifyPipeBugBehavior());
        Suite(nameof(VerifyMetroidBehaviorDefinitions), () => VerifyMetroidBehaviorDefinitions(rom));
        Suite(nameof(DragonBehaviorVerification.VerifyDragonBehavior), () => DragonBehaviorVerification.VerifyDragonBehavior());
        Suite(nameof(CacatacBehaviorVerification.VerifyCacatacBehavior), () => CacatacBehaviorVerification.VerifyCacatacBehavior());
        Suite(nameof(BullBehaviorVerification.VerifyBullBehavior), () => BullBehaviorVerification.VerifyBullBehavior());
        Suite(nameof(BullAnimationsVerification.VerifyBullAnimations), () => BullAnimationsVerification.VerifyBullAnimations(rom));
        Suite(nameof(VerifyChargeFlareDefinitions), () => VerifyChargeFlareDefinitions(rom));
        Suite(nameof(VerifySamusIndexedSpeeds), () => VerifySamusIndexedSpeeds(rom));
        Suite(nameof(VerifyLookupStream1ProjectileMotion), () => VerifyLookupStream1ProjectileMotion(rom));
        Suite(nameof(VerifyBeamCallbackTables), () => VerifyBeamCallbackTables(initializeOnly: true));
        Suite(nameof(VerifyLookupStream1Selection), () => VerifyLookupStream1Selection(rom));
        Suite(nameof(VerifyLookupStream1LaunchRoles), () => VerifyLookupStream1LaunchRoles(rom));
        Suite(nameof(VerifyPoseInputDefinitions), () => VerifyPoseInputDefinitions(rom));
        Suite(nameof(VerifyProjectileDamage), () => VerifyProjectileDamage(rom));
        Suite(nameof(VerifyProjectileSoundRoutingDefinitions), () => VerifyProjectileSoundRoutingDefinitions(rom));
        Console.WriteLine("Stream 1 lookup conversions: focused original-source and domain checks pass.");
        return 0;
    }

    /// <summary>Runs the grouped stream-four palette, layout, and instruction checks on the pinned image.</summary>
    [VerificationCommand("--lookup-stream-4")]
    internal static int LookupStream4Command()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Stream 4 oracle revision");
        Suite(nameof(VerifyMamaTurtleInstructionProgramDefinitions), () => VerifyMamaTurtleInstructionProgramDefinitions());
        Suite(nameof(VerifyCompiledDraygonBg2Collision), () => VerifyCompiledDraygonBg2Collision(rom));
        Suite(nameof(VerifyRidleyCollisionDefinitions), () => VerifyRidleyCollisionDefinitions());
        Suite(nameof(VerifyLookupStream4), () => VerifyLookupStream4(rom));
        Suite(nameof(VerifyPowerBombFixedColors), () => VerifyPowerBombFixedColors());
        Suite(nameof(VerifyRidleyAttackChoices), () => VerifyRidleyAttackChoices(rom));
        Suite(nameof(VerifyRidleyMovementTargets), () => VerifyRidleyMovementTargets(rom));
        Suite(nameof(VerifyRidleyClawOffsets), () => VerifyRidleyClawOffsets(rom));
        Suite(nameof(VerifySaveRamLayout), () => VerifySaveRamLayout());
        Suite(nameof(VerifyLowerNorfairRioInstructionProgramDefinitions), () => VerifyLowerNorfairRioInstructionProgramDefinitions());
        Suite(nameof(VerifyDraygonProjectileAnimations), () => VerifyDraygonProjectileAnimations(rom));
        Suite(nameof(VerifyBotwoonHoleRightBounds), () => VerifyBotwoonHoleRightBounds(rom));
        Suite(nameof(VerifyBotwoonHoleBottomBounds), () => VerifyBotwoonHoleBottomBounds(rom));
        Suite(nameof(VerifyBotwoonWallStockMapping), () => VerifyBotwoonWallStockMapping(rom));
        Suite(nameof(VerifyCompiledDraygonOamCollision), () => VerifyCompiledDraygonOamCollision(rom));
        Console.WriteLine("Stream 4 lookup conversions: focused original-source and domain checks pass.");
        return 0;
    }

    /// <summary>Checks Crocomire's authored phase ordering against native control words.</summary>
    [VerificationCommand("--lookup-stream5-crocomire-order")]
    internal static int LookupStream5CrocomireOrderCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Crocomire destruction-order oracle revision");
        Suite(nameof(VerifyCrocomireBridgeFragmentDefinitions), () => VerifyCrocomireBridgeFragmentDefinitions(rom));
        return 0;
    }

    /// <summary>Checks selected actor layouts using the requested cartridge source.</summary>
    [VerificationCommand("--lookup-stream5-actor-layouts")]
    [VerificationCommand("--lookup-stream5-initialization")]
    [VerificationCommand("--lookup-stream5-palette-entries")]
    internal static int LookupStream5ActorLayoutsCommand(string[] args)
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Stream 5 original oracle revision");
        if (args[0] == "--lookup-stream5-actor-layouts")
            VerifyLookupStream5ActorLayouts(rom);
        else if (args[0] == "--lookup-stream5-initialization")
            VerifyLookupStream5Initialization(rom);
        else
            VerifyLookupStream5PaletteEntries(rom);
        return 0;
    }

    /// <summary>Checks file-select name glyphs and their tile placements.</summary>
    [VerificationCommand("--lookup-file-select-names")]
    internal static int LookupFileSelectNamesCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "File-select identity fixture revision");
        byte[] source = FileSelectPresentationExtractor.Extract(rom);
        Suite(nameof(VerifyFileSelectPageNames), () => VerifyFileSelectPageNames(source));
        Suite(nameof(VerifyFileSelectPatchNames), () => VerifyFileSelectPatchNames(source));
        Suite(nameof(VerifyFileSelectBorderNames), () => VerifyFileSelectBorderNames(source));
        Suite(nameof(VerifyFileSelectDynamicAnchorNames), () => VerifyFileSelectDynamicAnchorNames(source));
        Suite(nameof(VerifyFileSelectSpriteNames), () => VerifyFileSelectSpriteNames(source));
        Console.WriteLine("File-select names: all five original identity domains, ordered cases, bounds and exact loader membership pass.");
        return 0;
    }

    /// <summary>Checks map sprite case selection and composed OAM output.</summary>
    [VerificationCommand("--lookup-map-sprite-cases")]
    internal static int LookupMapSpriteCasesCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Map sprite case oracle revision");
        Suite(nameof(VerifyMapSpriteCompositionCases), () => VerifyMapSpriteCompositionCases(rom));
        Console.WriteLine("Map sprite cases: all original OAM, independent role edits, required membership and invalid identities pass.");
        return 0;
    }

    /// <summary>Checks pause-menu title glyph pixels against source tiles.</summary>
    [VerificationCommand("--lookup-menu-title-font")]
    [VerificationCommand("--lookup-menu-large-font")]
    internal static int LookupMenuTitleFontCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Large font pixel oracle revision");
        Suite(nameof(VerifyMenuLargeFontPixels), () => VerifyMenuLargeFontPixels(rom));
        Console.WriteLine("Large font pixels: all original pixels, complete native uploads, independent edits, full custom region and bounds pass.");
        return 0;
    }

    /// <summary>Checks thin menu border tiles and their pixel edges.</summary>
    [VerificationCommand("--lookup-menu-thin-border-pixels")]
    internal static int LookupMenuThinBorderPixelsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Thin border pixel oracle revision");
        Suite(nameof(VerifyMenuThinBorderPixels), () => VerifyMenuThinBorderPixels(rom));
        Console.WriteLine("Thin border pixels: all original pixels, complete native uploads, independent edits, full custom region and bounds pass.");
        return 0;
    }

    /// <summary>Checks beveled menu square tiles and corner pixels.</summary>
    [VerificationCommand("--lookup-menu-beveled-square-pixels")]
    internal static int LookupMenuBeveledSquarePixelsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Beveled square pixel oracle revision");
        Suite(nameof(VerifyMenuBeveledSquarePixels), () => VerifyMenuBeveledSquarePixels(rom));
        Console.WriteLine("Beveled square pixels: all original pixels, complete native uploads, independent edits, full custom region and bounds pass.");
        return 0;
    }

    /// <summary>Checks map arrow artwork using the requested cartridge source.</summary>
    [VerificationCommand("--lookup-map-arrow-pixels")]
    [VerificationCommand("--lookup-map-pulse-pixels")]
    [VerificationCommand("--lookup-defeated-boss-pixels")]
    internal static int LookupMapArrowPixelsCommand(string[] args)
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Map marker pixel oracle revision");
        if (args[0] == "--lookup-map-arrow-pixels") VerifyMapArrowPixels(rom);
        else if (args[0] == "--lookup-map-pulse-pixels") VerifyMapPulsePixels(rom);
        else VerifyDefeatedBossPixels(rom);
        Console.WriteLine($"{args[0]}: all original pixels, complete native uploads, independent edits, full custom region and bounds pass.");
        return 0;
    }

    /// <summary>Checks shoulder-button highlight tiles and pixel colors.</summary>
    [VerificationCommand("--lookup-menu-shoulder-highlight-pixels")]
    internal static int LookupMenuShoulderHighlightPixelsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Shoulder highlight pixel oracle revision");
        Suite(nameof(VerifyMenuShoulderHighlightPixels), () => VerifyMenuShoulderHighlightPixels(rom));
        Console.WriteLine("Shoulder highlight pixels: all original pixels, complete native uploads, independent edits, full custom region and bounds pass.");
        return 0;
    }

    /// <summary>Checks shoulder-button label and outline tiles.</summary>
    [VerificationCommand("--lookup-menu-shoulder-button-pixels")]
    internal static int LookupMenuShoulderButtonPixelsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Shoulder button pixel oracle revision");
        Suite(nameof(VerifyMenuShoulderButtonPixels), () => VerifyMenuShoulderButtonPixels(rom));
        Console.WriteLine("Shoulder button pixels: all original pixels, complete native uploads, independent edits, full custom region and bounds pass.");
        return 0;
    }

    /// <summary>Checks menu panel tile pixels against native artwork.</summary>
    [VerificationCommand("--lookup-menu-panel-pixels")]
    internal static int LookupMenuPanelPixelsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Menu panel pixel oracle revision");
        Suite(nameof(VerifyMenuPanelPixels), () => VerifyMenuPanelPixels(rom));
        Console.WriteLine("Menu panel pixels: all original pixels, complete native uploads, independent edits, full custom region and bounds pass.");
        return 0;
    }

    /// <summary>Checks elevator-lettering glyph shapes and tile order.</summary>
    [VerificationCommand("--lookup-elevator-lettering")]
    [VerificationCommand("--lookup-menu-outlined-lettering")]
    [VerificationCommand("--lookup-menu-compact-lettering")]
    internal static int LookupElevatorLetteringCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Elevator lettering pixel oracle revision");
        Suite(nameof(VerifyMenuCompactLetteringPixels), () => VerifyMenuCompactLetteringPixels(rom));
        Console.WriteLine("Elevator lettering pixels: all original pixels, complete native uploads, independent edits, full custom region and bounds pass.");
        return 0;
    }

    /// <summary>Checks menu small-font glyph pixels and spacing.</summary>
    [VerificationCommand("--lookup-menu-small-font")]
    internal static int LookupMenuSmallFontCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Small font pixel oracle revision");
        Suite(nameof(VerifyMenuSmallFontPixels), () => VerifyMenuSmallFontPixels(rom));
        Console.WriteLine("Small font pixels: all original pixels, complete native uploads, independent edits, full custom region and bounds pass.");
        return 0;
    }

    /// <summary>Checks highlighted map tile pixels against source artwork.</summary>
    [VerificationCommand("--lookup-highlight-tile-pixels")]
    internal static int LookupHighlightTilePixelsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Highlight pixel oracle revision");
        Suite(nameof(VerifyHighlightTilePixels), () => VerifyHighlightTilePixels(rom));
        Console.WriteLine("Highlight pixels: all original pixels, complete native uploads, independent edits, full custom region and bounds pass.");
        return 0;
    }

    /// <summary>Checks reserve-tank UI tile pixels against native data.</summary>
    [VerificationCommand("--lookup-reserve-tile-pixels")]
    internal static int LookupReserveTilePixelsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Reserve pixel oracle revision");
        Suite(nameof(VerifyReserveTilePixels), () => VerifyReserveTilePixels(rom));
        Console.WriteLine("Reserve pixels: all original pixels, complete native uploads, independent edits, full custom region and bounds pass.");
        return 0;
    }

    /// <summary>Checks reserve-tank frame selection in the pause menu.</summary>
    [VerificationCommand("--lookup-pause-reserve-frames")]
    internal static int LookupPauseReserveFramesCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Reserve frame oracle revision");
        Suite(nameof(VerifyPauseReserveFrameCases), () => VerifyPauseReserveFrameCases(rom));
        Console.WriteLine("Reserve frames: all native role identities and OAM, independent edits, required membership and invalid inputs pass.");
        return 0;
    }

    /// <summary>Checks reserve-tank labels and drawing anchors.</summary>
    [VerificationCommand("--lookup-pause-reserve-anchors")]
    internal static int LookupPauseReserveAnchorsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Reserve anchor oracle revision");
        Suite(nameof(VerifyPauseReserveAnchors), () => VerifyPauseReserveAnchors(rom));
        Console.WriteLine("Reserve anchors: native coordinate fields, independent edits, actual sprite output and bounds pass.");
        return 0;
    }

    /// <summary>Checks ending explosion frame identities and artwork records.</summary>
    [VerificationCommand("--lookup-ending-explosion-frame-catalog")]
    internal static int LookupEndingExplosionFrameCatalogCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending explosion frame catalog oracle revision");
        Suite(nameof(VerifyEndingExplosionFrameCatalog), () => VerifyEndingExplosionFrameCatalog(rom));
        Console.WriteLine("Ending explosion frame catalog: all16 native pointers/counts, published keys, enumeration and bounds pass.");
        return 0;
    }

    /// <summary>Checks ending text region bounds and tilemap locations.</summary>
    [VerificationCommand("--lookup-ending-text-regions")]
    internal static int LookupEndingTextRegionsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending region oracle revision");
        Suite(nameof(VerifyEndingTextRegions), () => VerifyEndingTextRegions(rom));
        Console.WriteLine("Ending text regions: all six native text spans, positions and styles pass.");
        return 0;
    }

    /// <summary>Checks ending font glyph mapping against cartridge characters.</summary>
    [VerificationCommand("--lookup-ending-glyphs")]
    internal static int LookupEndingGlyphsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending glyph oracle revision");
        Suite(nameof(VerifyEndingGlyphMapping), () => VerifyEndingGlyphMapping(rom));
        Console.WriteLine("Ending glyphs: complete four-style alphabet/digit/blank mappings, both halves and all ushort decoder inputs pass.");
        return 0;
    }

    /// <summary>Checks ending gunship animation program words and timing.</summary>
    [VerificationCommand("--lookup-ending-gunship-program")]
    internal static int LookupEndingGunshipProgramCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending gunship program oracle revision");
        Suite(nameof(VerifyZebesExplosionGunshipPaletteFxColorDefinitions), () => VerifyZebesExplosionGunshipPaletteFxColorDefinitions(rom));
        Console.WriteLine("Ending gunship program:35 original control words,16 frame pointers,256 color pointers, complete ownership and384-frame lifetime pass.");
        return 0;
    }

    /// <summary>Checks ending gunship palette rows against source colors.</summary>
    [VerificationCommand("--lookup-ending-gunship-colors")]
    internal static int LookupEndingGunshipColorsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending gunship color oracle revision");
        byte[] json = RoomPaletteFxPresentationExtractor.Extract(rom);
        Suite(nameof(VerifyExtractedEndingGunshipPaletteFxPresentation), () => VerifyExtractedEndingGunshipPaletteFxPresentation(rom, RoomPaletteFxPresentation.Load(new MemoryStream(json))));
        Console.WriteLine("Ending gunship colors: all256 original words,239 calculated colors, complete pointer domain, guarded effect and independent edits pass.");
        return 0;
    }

    /// <summary>Checks ending logo glare instruction cadence and control flow.</summary>
    [VerificationCommand("--lookup-ending-logo-glare-program")]
    internal static int LookupEndingLogoGlareProgramCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Logo glare program oracle revision");
        Suite(nameof(VerifyPostCreditsIconGlarePaletteFxColorDefinitions), () => VerifyPostCreditsIconGlarePaletteFxColorDefinitions(rom));
        Console.WriteLine("Logo glare program: decoded31 control words,14 frame pointers,224 color pointers, complete ownership and lifetime pass.");
        return 0;
    }

    /// <summary>Checks visible logo glare color frames against native values.</summary>
    [VerificationCommand("--lookup-ending-logo-glare")]
    internal static int LookupEndingLogoGlareCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Logo glare oracle revision");
        byte[] json = RoomPaletteFxPresentationExtractor.Extract(rom);
        Suite(nameof(VerifyExtractedLogoGlarePaletteFxPresentation), () => VerifyExtractedLogoGlarePaletteFxPresentation(rom, RoomPaletteFxPresentation.Load(new MemoryStream(json))));
        Console.WriteLine("Logo glare: all224 native colors, full pointer domain, guarded program and independent edits pass.");
        return 0;
    }

    /// <summary>Checks logo fade stages and their palette destinations.</summary>
    [VerificationCommand("--lookup-ending-logo-fade")]
    internal static int LookupEndingLogoFadeCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending logo fade oracle revision");
        Suite(nameof(VerifyEndingLogoPaletteFade), () => VerifyEndingLogoPaletteFade(rom));
        Console.WriteLine("Ending logo fade: all512 original colors, computed transfers, independent edits and bounds pass.");
        return 0;
    }

    /// <summary>Checks ending palette source addresses and frame metadata.</summary>
    [VerificationCommand("--lookup-ending-palette-metadata")]
    internal static int LookupEndingPaletteMetadataCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending palette metadata oracle revision");
        Suite(nameof(VerifyEndingPaletteMetadata), () => VerifyEndingPaletteMetadata(rom));
        Console.WriteLine("Ending palettes: six native sources, seven allocation sizes, published filenames and invalid roles pass.");
        return 0;
    }

    /// <summary>Checks ending fragment identities and spawn metadata.</summary>
    [VerificationCommand("--lookup-ending-fragment-metadata")]
    internal static int LookupEndingFragmentMetadataCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending fragment oracle revision");
        Suite(nameof(VerifyEndingObjectFragmentMetadata), () => VerifyEndingObjectFragmentMetadata(rom));
        Console.WriteLine("Ending fragments: four native compressed sources, published filenames and bounds pass.");
        return 0;
    }

    /// <summary>Checks ending cloud actor definitions and frame selection.</summary>
    [VerificationCommand("--lookup-ending-cloud-definitions")]
    internal static int LookupEndingCloudDefinitionsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending cloud oracle revision");
        Suite(nameof(VerifyEndingCloudDefinitions), () => VerifyEndingCloudDefinitions(rom));
        Console.WriteLine("Ending clouds: six catalog records,24 original program words, six actor streams and bounds pass.");
        return 0;
    }

    /// <summary>Checks reward actor positions and presentation identities.</summary>
    [VerificationCommand("--lookup-ending-reward-actors")]
    internal static int LookupEndingRewardActorsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending reward actor oracle revision");
        Suite(nameof(VerifyEndingRewardActorDefinitions), () => VerifyEndingRewardActorDefinitions(rom));
        return 0;
    }

    /// <summary>Checks reward actor instruction words and timing.</summary>
    [VerificationCommand("--lookup-ending-reward-programs")]
    internal static int LookupEndingRewardProgramsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending reward oracle revision");
        Suite(nameof(VerifyEndingRewardInstructions), () => VerifyEndingRewardInstructions(rom));
        Console.WriteLine("Ending reward programs: all160 native words,17 actor streams, callback timing, head record layout and bounds pass.");
        return 0;
    }

    /// <summary>Checks ending explosion actor programs and transitions.</summary>
    [VerificationCommand("--lookup-ending-explosion-programs")]
    internal static int LookupEndingExplosionProgramsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending explosion oracle revision");
        Console.WriteLine("Ending explosion programs: all65 native words, eight actor streams, frame record layout and bounds pass.");
        return 0;
    }

    /// <summary>Checks ending logo actor geometry and draw identities.</summary>
    [VerificationCommand("--lookup-ending-logo-actors")]
    internal static int LookupEndingLogoActorsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending logo actor oracle revision");
        Suite(nameof(VerifyEndingLogoDefinitions), () => VerifyEndingLogoDefinitions(rom));
        return 0;
    }

    /// <summary>Checks ending logo actor programs and frame selectors.</summary>
    [VerificationCommand("--lookup-ending-logo-programs")]
    internal static int LookupEndingLogoProgramsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending logo program oracle revision");
        Suite(nameof(VerifyEndingLogoPrograms), () => VerifyEndingLogoPrograms(rom));
        Console.WriteLine("Ending logo programs: all31 native words, four actor streams, callback timing and bounds pass.");
        return 0;
    }

    /// <summary>Checks completion text program words and timing.</summary>
    [VerificationCommand("--lookup-ending-completion-programs")]
    internal static int LookupEndingCompletionProgramsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending completion oracle revision");
        Suite(nameof(VerifyEndingCompletionTextInstructions), () => VerifyEndingCompletionTextInstructions(rom));
        Console.WriteLine("Ending completion programs: all164 original words and all14 actor streams pass.");
        return 0;
    }

    /// <summary>Checks ending logo lookup tables against the native image.</summary>
    [VerificationCommand("--lookup-ending-logo-tables")]
    internal static int LookupEndingLogoTablesCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending logo table oracle revision");
        Suite(nameof(VerifyEndingLogoPaletteSources), () => VerifyEndingLogoPaletteSources(rom));
        Suite(nameof(VerifyEndingPostShotTransferFields), () => VerifyEndingPostShotTransferFields(rom));
        Console.WriteLine("Ending logo tables: all32 original palette sources, six transfer records and invalid bounds pass.");
        return 0;
    }

    /// <summary>Checks SPC upload destinations and block metadata against cartridge data.</summary>
    [VerificationCommand("--lookup-audio-upload-catalog")]
    internal static int LookupAudioUploadCatalogCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Audio upload oracle revision");
        Suite(nameof(VerifyAudioUploadCatalog), () => VerifyAudioUploadCatalog(rom));
        Console.WriteLine("Audio upload catalog: all25 original pointers, names, views and byte rejection domain pass.");
        return 0;
    }

    /// <summary>Checks initial Ceres actor IDs, positions, and frame metadata.</summary>
    [VerificationCommand("--lookup-ceres-initial-metadata")]
    internal static int LookupCeresInitialMetadataCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres destruction metadata oracle revision");
        Suite(nameof(VerifyCeresInitialActorMetadata), () => VerifyCeresInitialActorMetadata(rom));
        Console.WriteLine("Ceres destruction metadata: native spawn identities, asteroid aliases, stationary-vortex overrides and bounds pass.");
        return 0;
    }

    /// <summary>Checks Ceres flight actor metadata against native records.</summary>
    [VerificationCommand("--lookup-ceres-flight-metadata")]
    internal static int LookupCeresFlightMetadataCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres flight metadata oracle revision");
        Suite(nameof(VerifyCeresFlightActorMetadata), () => VerifyCeresFlightActorMetadata(rom));
        Console.WriteLine("Ceres flight metadata: native spawn identities, initializer branches, motion operands, star aliases and bounds pass.");
        return 0;
    }

    /// <summary>Checks Ceres-to-Zebes actor metadata and placement.</summary>
    [VerificationCommand("--lookup-ceres-zebes-metadata")]
    internal static int LookupCeresZebesMetadataCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Zebes metadata oracle revision");
        Suite(nameof(VerifyCeresZebesActorMetadata), () => VerifyCeresZebesActorMetadata(rom));
        Console.WriteLine("Zebes actor metadata: all six native spawn identities, definitions, placements, motion policies and bounds pass.");
        return 0;
    }

    /// <summary>Checks Ceres actor placement selector tables against cartridge words.</summary>
    [VerificationCommand("--lookup-ceres-placement-selectors")]
    internal static int LookupCeresPlacementSelectorsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres placement oracle revision");
        Suite(nameof(VerifyCeresRearPlacementSelector), () => VerifyCeresRearPlacementSelector(rom));
        Suite(nameof(VerifyCeresRevealPlacementSelector), () => VerifyCeresRevealPlacementSelector(rom));
        Suite(nameof(VerifyCeresInitialPlacementSelector), () => VerifyCeresInitialPlacementSelector());
        Console.WriteLine("Ceres placement selectors: all 14 original entries, native operands and invalid-index contracts pass.");
        return 0;
    }

    /// <summary>Checks Ceres spawner delays and actor order.</summary>
    [VerificationCommand("--lookup-ceres-spawner-schedule")]
    internal static int LookupCeresSpawnerScheduleCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres spawner oracle revision");
        Suite(nameof(VerifyCeresSpawnerSchedule), () => VerifyCeresSpawnerSchedule(rom));
        Console.WriteLine("Ceres spawner: native list timing, late countdown resets, simultaneous waves and departure boundaries pass.");
        return 0;
    }

    /// <summary>Checks Ceres flight animation programs and branch words.</summary>
    [VerificationCommand("--lookup-ceres-flight-programs")]
    internal static int LookupCeresFlightProgramsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres flight program oracle revision");
        Suite(nameof(VerifyCeresFlightFrameCatalog), () => VerifyCeresFlightFrameCatalog(rom));
        Suite(nameof(VerifySpaceColonyCaption), () => VerifySpaceColonyCaption(rom));
        Suite(nameof(VerifyCeresStationParts), () => VerifyCeresStationParts(rom));
        Suite(nameof(VerifyCeresSmallAsteroidParts), () => VerifyCeresSmallAsteroidParts(rom));
        Suite(nameof(VerifyCeresVortexParts), () => VerifyCeresVortexParts(rom));
        Suite(nameof(VerifyCeresReflectedStarParts), () => VerifyCeresReflectedStarParts(rom));
        Suite(nameof(VerifyCeresStarPointParts), () => VerifyCeresStarPointParts(rom));
        Console.WriteLine("Ceres flight programs: five original streams, byte/word boundaries, shared aliases and interpreter loops pass.");
        return 0;
    }

    /// <summary>Checks Ceres backdrop instruction programs against native data.</summary>
    [VerificationCommand("--lookup-ceres-backdrop-programs")]
    internal static int LookupCeresBackdropProgramsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres backdrop program oracle revision");
        Suite(nameof(VerifyCeresDestructionFrameCatalog), () => VerifyCeresDestructionFrameCatalog(rom));
        Suite(nameof(VerifyPlanetZebesTitleParts), () => VerifyPlanetZebesTitleParts(rom));
        Suite(nameof(VerifyZebesPlanetBandParts), () => VerifyZebesPlanetBandParts(rom));
        Console.WriteLine("Ceres backdrop programs: seven original streams, word boundaries, loops and title callback timing pass.");
        return 0;
    }

    /// <summary>Checks Ceres explosion instruction lists and transitions.</summary>
    [VerificationCommand("--lookup-ceres-explosion-programs")]
    internal static int LookupCeresExplosionProgramsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres explosion program oracle revision");
        Suite(nameof(VerifyCeresStationBlastParts), () => VerifyCeresStationBlastParts(rom));
        Suite(nameof(VerifyCeresLargeBlastParts), () => VerifyCeresLargeBlastParts(rom));
        Console.WriteLine("Ceres explosion programs: four original streams, byte/word bounds and actual interpreter timing/deletion pass.");
        return 0;
    }

    /// <summary>Checks Ceres burst OAM layout and part offsets.</summary>
    [VerificationCommand("--lookup-ceres-burst-layout")]
    internal static int LookupCeresBurstLayoutCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres burst layout oracle revision");
        Suite(nameof(VerifyCeresBurstLayout), () => VerifyCeresBurstLayout(rom));
        Console.WriteLine("Ceres burst layout: all original repeating X/Y and final Y words, common delay and bounds pass.");
        return 0;
    }

    /// <summary>Checks blast spawn coordinates and selected source frames.</summary>
    [VerificationCommand("--lookup-ceres-blast-placement")]
    internal static int LookupCeresBlastPlacementCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres blast oracle revision");
        Suite(nameof(VerifyCeresInitialBlastX), () => VerifyCeresInitialBlastX(rom));
        Suite(nameof(VerifyCeresInitialBlastY), () => VerifyCeresInitialBlastY(rom));
        Suite(nameof(VerifyCeresInitialBlastDelay), () => VerifyCeresInitialBlastDelay(rom));
        Suite(nameof(VerifyCeresFinalBlastX), () => VerifyCeresFinalBlastX(rom));
        Suite(nameof(VerifyCeresFinalBlastDelay), () => VerifyCeresFinalBlastDelay(rom));
        Console.WriteLine("Ceres blast placement: five original geometry/delay mappings and invalid boundaries pass.");
        return 0;
    }

    /// <summary>Checks decoded SPC sound effect byte streams against uploads.</summary>
    [VerificationCommand("--lookup-spc-sound-streams")]
    internal static int LookupSpcSoundStreamsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "SPC stream oracle revision");
        Suite(nameof(VerifySpcSoundStream1), () => VerifySpcSoundStream1(rom));
        Suite(nameof(VerifySpcSoundStream2), () => VerifySpcSoundStream2(rom));
        Suite(nameof(VerifySpcSoundStream3), () => VerifySpcSoundStream3(rom));
        Console.WriteLine("SPC sound streams: all240 original pointers, counts, invalid inputs and runtime command boundaries pass.");
        return 0;
    }

    /// <summary>Checks SPC sound selection and channel policies.</summary>
    [VerificationCommand("--lookup-spc-sound-policies")]
    internal static int LookupSpcSoundPoliciesCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "SPC policy oracle revision");
        Suite(nameof(VerifySpcSoundPolicy1), () => VerifySpcSoundPolicy1(rom));
        Suite(nameof(VerifySpcSoundPolicy2), () => VerifySpcSoundPolicy2(rom));
        Suite(nameof(VerifySpcSoundPolicy3), () => VerifySpcSoundPolicy3(rom));
        Console.WriteLine("SPC sound policies: all240 native dispatches, handler writes, preserved fields, voice counts and invalid inputs pass.");
        return 0;
    }

    /// <summary>Checks intro egg-effect instruction lists against cartridge words.</summary>
    [VerificationCommand("--lookup-intro-egg-effect-programs")]
    internal static int LookupIntroEggEffectProgramsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Intro egg-effect oracle revision");
        Suite(nameof(VerifyIntroEggEffectFrameCatalog), () => VerifyIntroEggEffectFrameCatalog(rom));
        Suite(nameof(VerifyIntroEggEffectParts), () => VerifyIntroEggEffectParts(rom));
        Console.WriteLine("Intro egg effects: all76 bytes, eleven frame records/calculated parts, native OAM, independent edits and boundaries pass.");
        return 0;
    }

    /// <summary>Checks intro Rinka animation lists and timing.</summary>
    [VerificationCommand("--lookup-intro-rinka-programs")]
    internal static int LookupIntroRinkaProgramsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Intro Rinka oracle revision");
        Suite(nameof(VerifyIntroRinkaFrameCatalog), () => VerifyIntroRinkaFrameCatalog(rom));
        Suite(nameof(VerifyIntroRinkaParts), () => VerifyIntroRinkaParts(rom));
        Console.WriteLine("Intro Rinka: program/catalog, twelve calculated parts, native OAM, independent edits and bounds pass.");
        return 0;
    }

    /// <summary>Checks Baby discovery actor instructions and frame order.</summary>
    [VerificationCommand("--lookup-intro-baby-discovery-instructions")]
    internal static int LookupIntroBabyDiscoveryInstructionsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Intro discovery oracle revision");
        Suite(nameof(VerifyIntroDiscoveryFrameCatalog), () => VerifyIntroDiscoveryFrameCatalog(rom));
        Suite(nameof(VerifyIntroEggRemnantParts), () => VerifyIntroEggRemnantParts(rom));
        Suite(nameof(VerifyIntroEggRockingParts), () => VerifyIntroEggRockingParts(rom));
        Suite(nameof(VerifyIntroEggCrackingParts), () => VerifyIntroEggCrackingParts(rom));
        Suite(nameof(VerifyIntroConfusedBabyParts), () => VerifyIntroConfusedBabyParts(rom));
        Suite(nameof(VerifyCeresLargeAsteroidParts), () => VerifyCeresLargeAsteroidParts(rom));
        Suite(nameof(VerifyIntroDiscoveryCollision), () => VerifyIntroDiscoveryCollision(rom));
        Console.WriteLine("Intro discovery programs: all138 bytes, overlapping words, operations and boundaries pass.");
        return 0;
    }

    /// <summary>Checks Ceres scientist actor instruction programs.</summary>
    [VerificationCommand("--lookup-intro-scientist-instructions")]
    internal static int LookupIntroScientistInstructionsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Intro scientist oracle revision");
        Suite(nameof(VerifyIntroScientistFrameCatalog), () => VerifyIntroScientistFrameCatalog(rom));
        Suite(nameof(VerifyIntroScientistParts), () => VerifyIntroScientistParts(rom));
        Console.WriteLine("Intro scientist programs/catalog: all142 bytes, overlapping views, ten original frame records, stable asset names and boundaries pass.");
        return 0;
    }

    /// <summary>Checks intro eye actor frame programs and selectors.</summary>
    [VerificationCommand("--lookup-intro-eye-instructions")]
    internal static int LookupIntroEyeInstructionsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Intro eye oracle revision");
        Suite(nameof(VerifyIntroEyeInstructions), () => VerifyIntroEyeInstructions(rom));
        Console.WriteLine("Intro eye programs: all74 bytes, overlapping words, loop targets and read boundaries pass.");
        return 0;
    }

    /// <summary>Checks intro Mother Brain actor instruction words.</summary>
    [VerificationCommand("--lookup-intro-mother-brain-instructions")]
    internal static int LookupIntroMotherBrainInstructionsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Mother Brain instruction oracle revision");
        Suite(nameof(VerifyIntroMotherBrainFrameCatalog), () => VerifyIntroMotherBrainFrameCatalog(rom));
        Suite(nameof(VerifyIntroMotherBrainParts), () => VerifyIntroMotherBrainParts(rom));
        Console.WriteLine("Mother Brain programs/catalog: all46 bytes,45 word views,three frames and boundaries pass.");
        return 0;
    }

    /// <summary>Checks intro Mother Brain collision geometry and response.</summary>
    [VerificationCommand("--lookup-intro-mother-brain-collision")]
    internal static int LookupIntroMotherBrainCollisionCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Mother Brain collision oracle revision");
        Suite(nameof(VerifyIntroMotherBrainCollisionSource), () => VerifyIntroMotherBrainCollisionSource(rom));
        Console.WriteLine("Mother Brain collision source: all448 bytes and independent mutable allocations pass.");
        return 0;
    }

    /// <summary>Checks intro Mother Brain explosion instructions and timing.</summary>
    [VerificationCommand("--lookup-intro-mother-brain-explosion-programs")]
    internal static int LookupIntroMotherBrainExplosionProgramsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Intro explosion oracle revision");
        Suite(nameof(VerifyIntroMotherBrainExplosionFrameCatalog), () => VerifyIntroMotherBrainExplosionFrameCatalog(rom));
        Suite(nameof(VerifyIntroMotherBrainExplosionParts), () => VerifyIntroMotherBrainExplosionParts(rom));
        Console.WriteLine("Intro Mother Brain explosions: both native programs, word views, delete, twelve frame identities and bounds pass.");
        return 0;
    }

    /// <summary>Checks intro caret actor instruction cadence and frame order.</summary>
    [VerificationCommand("--lookup-intro-caret-instructions")]
    internal static int LookupIntroCaretInstructionsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Intro caret oracle revision");
        Console.WriteLine("Intro caret instructions: all20 bytes, overlapping word views and boundary behavior pass.");
        return 0;
    }

    /// <summary>Checks SPC DSP register publication against the decoded music program.</summary>
    [VerificationCommand("--lookup-spc-dsp-publication")]
    internal static int LookupSpcDspPublicationCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "SPC publication oracle revision");
        Suite(nameof(VerifySpcDspPublication), () => VerifySpcDspPublication(rom));
        Console.WriteLine("SPC DSP publication: native destination/source maps, echo gates and pending-key reset pass.");
        return 0;
    }

    /// <summary>Checks SPC left/right pan samples and native channel values.</summary>
    [VerificationCommand("--lookup-spc-pan-samples")]
    internal static int LookupSpcPanSamplesCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "SPC pan sample oracle revision");
        Suite(nameof(VerifySpcPanSamples), () => VerifySpcPanSamples(rom));
        Console.WriteLine("SPC pan sample boundary: all21 curve bytes, the adjacent FIR coefficient and rejected indices pass; curve conversion remains pending.");
        return 0;
    }

    /// <summary>Checks pause wireframe variants chosen for equipment state.</summary>
    [VerificationCommand("--lookup-pause-wireframe-selection")]
    internal static int LookupPauseWireframeSelectionCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Pause wireframe selection oracle revision");
        Suite(nameof(VerifyPauseWireframeSelection), () => VerifyPauseWireframeSelection(rom));
        Console.WriteLine("Pause wireframe selection: native mask and comparison-table operands and all65536 input words pass.");
        return 0;
    }

    /// <summary>Checks equipment icon masks and their native bit assignments.</summary>
    [VerificationCommand("--lookup-pause-equipment-masks")]
    internal static int LookupPauseEquipmentMasksCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Pause equipment mask oracle revision");
        Suite(nameof(VerifyPauseBeamMasks), () => VerifyPauseBeamMasks(rom));
        Suite(nameof(VerifyPauseSuitMasks), () => VerifyPauseSuitMasks(rom));
        Suite(nameof(VerifyPauseBootMasks), () => VerifyPauseBootMasks(rom));
        Console.WriteLine("Pause equipment masks: all fourteen native flags and category/item rejection contracts pass.");
        return 0;
    }

    /// <summary>Checks pause selector positions in the equipment panels.</summary>
    [VerificationCommand("--lookup-pause-selector-anchors")]
    internal static int LookupPauseSelectorAnchorsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Pause selector anchor oracle revision");
        Suite(nameof(VerifyPauseSelectorAnchors), () => VerifyPauseSelectorAnchors(rom));
        Console.WriteLine("Pause selector anchors: original names and coordinates, independent axis edits, zero stock storage and bounds pass.");
        return 0;
    }

    /// <summary>Checks selector OAM parts and palette bits.</summary>
    [VerificationCommand("--lookup-pause-selector-compositions")]
    internal static int LookupPauseSelectorCompositionsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Pause selector composition oracle revision");
        Suite(nameof(VerifyPauseSelectorCompositions), () => VerifyPauseSelectorCompositions(rom));
        Console.WriteLine("Pause selector compositions: native OAM, all groups/phases, independent edits, cyclic indices and capacity pass.");
        return 0;
    }

    /// <summary>Checks selector animation hold lengths against native lists.</summary>
    [VerificationCommand("--lookup-pause-selector-durations")]
    internal static int LookupPauseSelectorDurationsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Pause selector duration oracle revision");
        Suite(nameof(VerifyPauseSelectorDurations), () => VerifyPauseSelectorDurations(rom));
        Console.WriteLine("Pause selector durations: native program, sparse edits, custom lengths, cyclic indices and initial timer pass.");
        return 0;
    }

    /// <summary>Checks map-scroll arrow animation timing.</summary>
    [VerificationCommand("--lookup-map-arrow-durations")]
    internal static int LookupMapArrowDurationsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Map arrow duration oracle revision");
        Suite(nameof(VerifyMapArrowDurations), () => VerifyMapArrowDurations(rom));
        Console.WriteLine("Map arrow durations: all original phases, sparse edits, custom lengths, bounds and actual timing pass.");
        return 0;
    }

    /// <summary>Checks arrow frame selection at map-scroll boundaries.</summary>
    [VerificationCommand("--lookup-map-arrow-cases")]
    internal static int LookupMapArrowCasesCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Map arrow oracle revision");
        Suite(nameof(VerifyMapArrowCases), () => VerifyMapArrowCases(rom));
        Console.WriteLine("Map arrow cases: original anchors and shapes, independent direction edits, phase selection, membership and bounds pass.");
        return 0;
    }

    /// <summary>Checks pause button label widths and drawing anchors.</summary>
    [VerificationCommand("--lookup-pause-button-spans")]
    internal static int LookupPauseButtonSpansCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Pause button span oracle revision");
        Suite(nameof(VerifyPauseButtonSpanWords), () => VerifyPauseButtonSpanWords(rom));
        Suite(nameof(VerifyPauseButtonSpanCounts), () => VerifyPauseButtonSpanCounts(rom));
        Console.WriteLine("Pause buttons: all six native row destinations and widths, enumeration order and invalid bounds pass.");
        return 0;
    }

    /// <summary>Checks pause-menu category IDs and their native ordering.</summary>
    [VerificationCommand("--lookup-pause-categories")]
    internal static int LookupPauseCategoriesCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Pause category oracle revision");
        Suite(nameof(VerifyPauseCategoryCases), () => VerifyPauseCategoryCases(rom));
        Console.WriteLine("Pause categories: all native pointer fields, item counts, dispatched copy lengths, reserve contract and invalid bounds pass.");
        return 0;
    }

    /// <summary>Checks map indicator art and placement rules.</summary>
    [VerificationCommand("--lookup-map-indicator")]
    internal static int LookupMapIndicatorCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Map indicator oracle revision");
        Suite(nameof(VerifyMapIndicatorSprites), () => VerifyMapIndicatorSprites(rom));
        Suite(nameof(VerifyMapIndicatorDelays), () => VerifyMapIndicatorDelays(rom));
        Console.WriteLine("Map indicator: all native sprite/delay words, input bounds, first-tick order and two loop transitions pass.");
        return 0;
    }

    /// <summary>Checks save marker coordinates in file-select maps.</summary>
    [VerificationCommand("--lookup-save-marker-coordinates")]
    internal static int LookupSaveMarkerCoordinatesCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Save-marker coordinate oracle revision");
        Suite(nameof(VerifyMapSaveMarkerCoordinates), () => VerifyMapSaveMarkerCoordinates(rom));
        Console.WriteLine("Save-marker coordinates: all 68 native components, independent edits, JSON identity, actual marker binding and bounds pass.");
        return 0;
    }

    /// <summary>Checks area-specific map case selection against native tables.</summary>
    [VerificationCommand("--lookup-map-area-cases")]
    internal static int LookupMapAreaCasesCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Map-area selector oracle revision");
        Suite(nameof(VerifyFileSelectMapAreaCases), () => VerifyFileSelectMapAreaCases(rom));
        Console.WriteLine("Map-area selection: all six native identity cases and rejected input boundaries pass.");
        return 0;
    }

    /// <summary>Checks when file-select save markers are drawn for a slot.</summary>
    [VerificationCommand("--lookup-save-marker-eligibility")]
    internal static int LookupSaveMarkerEligibilityCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Save-marker eligibility oracle revision");
        Suite(nameof(VerifySaveMarkerEligibility), () => VerifySaveMarkerEligibility(rom));
        Console.WriteLine("Save-marker eligibility: all 96 native slots, 34 identities, all used masks and input boundaries pass.");
        return 0;
    }

    /// <summary>Checks map load and display anchor coordinates.</summary>
    [VerificationCommand("--lookup-map-load-anchors")]
    internal static int LookupMapLoadAnchorsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Map-load anchor oracle revision");
        Suite(nameof(VerifyMapLoadAnchorX), () => VerifyMapLoadAnchorX(rom));
        Suite(nameof(VerifyMapLoadAnchorY), () => VerifyMapLoadAnchorY(rom));
        Console.WriteLine("Map-load anchors: all 34 original X/Y projections, unused stations and invalid input boundaries pass.");
        return 0;
    }

    /// <summary>Checks file-select helmet frames and their native animation cadence.</summary>
    [VerificationCommand("--lookup-file-select-helmet")]
    internal static int LookupFileSelectHelmetCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Helmet animation oracle revision");
        Suite(nameof(VerifyFileSelectHelmetAnimation), () => VerifyFileSelectHelmetAnimation(rom));
        Console.WriteLine("File-select helmet: all nine original sprite IDs, native cadence/clamp and bounded selectors pass.");
        return 0;
    }

    /// <summary>Checks Game Over Baby Metroid frames and timing.</summary>
    [VerificationCommand("--lookup-game-over-baby-animation")]
    internal static int LookupGameOverBabyAnimationCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Baby animation oracle revision");
        Suite(nameof(VerifyGameOverBabyAnimation), () => VerifyGameOverBabyAnimation(rom));
        Console.WriteLine("Baby animation: original durations, frames, palettes, sound/control handoffs, lazy enumeration and all ushort pointers pass.");
        return 0;
    }

    /// <summary>Checks Game Over Baby Metroid palette colors.</summary>
    [VerificationCommand("--lookup-game-over-baby-colors")]
    internal static int LookupGameOverBabyColorsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Baby colors oracle revision");
        Suite(nameof(VerifyGameOverBabyColors), () => VerifyGameOverBabyColors(rom));
        Console.WriteLine("Baby colors: all 64 original words, CGRAM application, independent channel edits and bounds pass.");
        return 0;
    }

    /// <summary>Checks Game Over text glyphs and tilemap positions.</summary>
    [VerificationCommand("--lookup-game-over-text")]
    internal static int LookupGameOverTextCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Game-over text oracle revision");
        Suite(nameof(VerifyGameOverTextSources), () => VerifyGameOverTextSources(rom));
        Suite(nameof(VerifyGameOverTextDestinations), () => VerifyGameOverTextDestinations(rom));
        Console.WriteLine("Game-over text: all five original sources and destinations, enumeration order and rejected selectors pass.");
        return 0;
    }

    /// <summary>Checks file-select cursor transitions and selectable entries.</summary>
    [VerificationCommand("--lookup-file-select-navigation")]
    internal static int LookupFileSelectNavigationCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "File-select navigation oracle revision");
        Suite(nameof(VerifyFileSelectMainNavigation), () => VerifyFileSelectMainNavigation());
        Suite(nameof(VerifyFileSelectSourceNavigation), () => VerifyFileSelectSourceNavigation(rom));
        Suite(nameof(VerifyFileSelectDestinationNavigation), () => VerifyFileSelectDestinationNavigation(rom));
        return 0;
    }

    /// <summary>Checks world-map window offsets and scrolling cadence.</summary>
    [VerificationCommand("--lookup-map-window-motion")]
    internal static int LookupMapWindowMotionCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Map window motion oracle revision");
        var labels = RetailPresentationFixture().Labels;
        Suite(nameof(VerifyMapWindowOriginX), () => VerifyMapWindowOriginX(rom, labels));
        Suite(nameof(VerifyMapWindowOriginY), () => VerifyMapWindowOriginY(rom, labels));
        Suite(nameof(VerifyMapWindowLeftVelocities), () => VerifyMapWindowLeftVelocities(rom));
        Suite(nameof(VerifyMapWindowRightVelocities), () => VerifyMapWindowRightVelocities(rom));
        Suite(nameof(VerifyMapWindowTopVelocities), () => VerifyMapWindowTopVelocities(rom));
        Suite(nameof(VerifyMapWindowBottomVelocities), () => VerifyMapWindowBottomVelocities(rom));
        Suite(nameof(VerifyMapWindowTimers), () => VerifyMapWindowTimers(rom));
        Console.WriteLine("Map window motion: all 24 original signed16.16 velocities, six timers, 12 named origin fields, independent label edits and rejected areas pass.");
        return 0;
    }

    /// <summary>Checks missile icon and count display in the menu.</summary>
    [VerificationCommand("--lookup-menu-missile")]
    internal static int LookupMenuMissileCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Menu missile oracle revision");
        Suite(nameof(VerifyMenuMissileSpritemapIds), () => VerifyMenuMissileSpritemapIds(rom));
        Suite(nameof(VerifyMenuMissileDurations), () => VerifyMenuMissileDurations(rom));
        Console.WriteLine("Menu missile: all four original frame IDs, four duration words, wrap mask and rejected indices pass.");
        return 0;
    }

    /// <summary>Checks options-screen panel and cursor geometry.</summary>
    [VerificationCommand("--lookup-options-geometry")]
    internal static int LookupOptionsGeometryCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Options geometry oracle revision");
        Suite(nameof(VerifyOptionsPrimaryCursorY), () => VerifyOptionsPrimaryCursorY(rom));
        Suite(nameof(VerifyOptionsControllerCursorY), () => VerifyOptionsControllerCursorY(rom));
        Suite(nameof(VerifyOptionsSpecialCursorY), () => VerifyOptionsSpecialCursorY(rom));
        Suite(nameof(VerifyOptionsLabelDestinations), () => VerifyOptionsLabelDestinations(rom));
        Suite(nameof(VerifyOptionsLabelSources), () => VerifyOptionsLabelSources(rom));
        Console.WriteLine("Options geometry: all 31 native words across five logical mappings and their bounds pass.");
        return 0;
    }

    /// <summary>Checks controller button glyphs and binding cases.</summary>
    [VerificationCommand("--lookup-controller-buttons")]
    internal static int LookupControllerButtonsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Controller button oracle revision");
        Suite(nameof(VerifyAssignableControllerButtons), () => VerifyAssignableControllerButtons(rom));
        Suite(nameof(VerifyDefaultControllerButtons), () => VerifyDefaultControllerButtons(rom));
        Console.WriteLine("Controller buttons: native choices, inverse domain, swaps, rejection and default action mappings pass.");
        return 0;
    }

    /// <summary>Checks Hyper Beam effect color frames against pinned artwork.</summary>
    [VerificationCommand("--hyper-beam-fx-colors")]
    internal static int HyperBeamFxColorsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Hyper Beam FX oracle revision");
        Suite(nameof(VerifyHyperBeamFxColorArtwork), () => VerifyHyperBeamFxColorArtwork(rom));
        return 0;
    }

    /// <summary>Checks door-closing behavior when no resident room PLM owns the door.</summary>
    [VerificationCommand("--lookup-fallback-door-closing")]
    internal static int LookupFallbackDoorClosingCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Fallback closing oracle revision");
        Suite(nameof(VerifyDoorClosingPlmDefinitions), () => VerifyDoorClosingPlmDefinitions(rom, fallbackOnly: true));
        Console.WriteLine("Fallback closing: all twelve original header/list selections, complete byte domain and production spawning pass.");
        return 0;
    }

    /// <summary>Checks the resident door-closing program and its tile updates.</summary>
    [VerificationCommand("--lookup-resident-door-closing")]
    internal static int LookupResidentDoorClosingCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Resident closing oracle revision");
        Suite(nameof(VerifyResidentDoorClosingDefinitions), () => VerifyResidentDoorClosingDefinitions(rom));
        Console.WriteLine("Resident door closing: eighteen original header fields, complete selector domain and production redirect pass.");
        return 0;
    }

    /// <summary>Checks collectible tile graphics assembled from dynamic sources.</summary>
    [VerificationCommand("--dynamic-collectible-graphics")]
    internal static int DynamicCollectibleGraphicsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Dynamic collectible oracle revision");
        Suite(nameof(VerifyCompiledDynamicCollectibleGraphics), () => VerifyCompiledDynamicCollectibleGraphics(rom));
        Console.WriteLine("Dynamic collectible graphics: native palette selectors, tiles, pointers, guarded upload and installed artwork pass.");
        return 0;
    }

    /// <summary>Checks Tourian enemy instruction lists and control words.</summary>
    [VerificationCommand("--lookup-tourian-program")]
    internal static int LookupTourianProgramCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Statue program oracle revision");
        Suite(nameof(VerifyTourianStatueProgramMappings), () => VerifyTourianStatueProgramMappings(rom));
        Suite(nameof(VerifyTourianStatueSpawnOrder), () => VerifyTourianStatueSpawnOrder(rom));
        Suite(nameof(VerifyTourianStatueDescriptorFields), () => VerifyTourianStatueDescriptorFields(rom));
        Suite(nameof(VerifyTourianStatueArtworkSources), () => VerifyTourianStatueArtworkSources(rom));
        Console.WriteLine("Tourian statue programs: all 36 operand positions, 184 mechanics words and full pointer domains pass.");
        return 0;
    }

    /// <summary>Checks Tourian actor descriptor identities and layout.</summary>
    [VerificationCommand("--lookup-tourian-descriptors")]
    internal static int LookupTourianDescriptorsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Statue descriptor oracle revision");
        Suite(nameof(VerifyTourianStatueDescriptorFields), () => VerifyTourianStatueDescriptorFields(rom));
        Suite(nameof(VerifyTourianStatueArtworkSources), () => VerifyTourianStatueArtworkSources(rom));
        Console.WriteLine("Tourian statue descriptors: all eleven native fields, mechanics aliases, full object domain and artwork sources pass.");
        return 0;
    }

    /// <summary>Checks Tourian visual tiles and palette sources.</summary>
    [VerificationCommand("--lookup-tourian-artwork")]
    internal static int LookupTourianArtworkCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Statue artwork oracle revision");
        Suite(nameof(VerifyTourianStatueArtworkSources), () => VerifyTourianStatueArtworkSources(rom));
        Console.WriteLine("Tourian statue artwork: all 36 original sources, decoded operand views and complete ushort rejection domains pass.");
        return 0;
    }

    /// <summary>Checks treadmill scroll and movement mechanics against native values.</summary>
    [VerificationCommand("--lookup-treadmill-mechanics")]
    internal static int LookupTreadmillMechanicsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Treadmill oracle revision");
        Suite(nameof(VerifyRetailTreadmillMechanics), () => VerifyRetailTreadmillMechanics(rom));
        foreach (var program in OriginalTreadmillPrograms(rom))
            VerifyRetailTreadmillStream(rom, TreadmillDefinition(program.Header).Direction,
                program.Frames.Select(pointer => 0x870000 | ReadVerificationWord(rom, 0x870002 + pointer)).ToArray());
        Console.WriteLine("Treadmill: native header/control fields, calculated cursors, full ushort domains and both guarded loops pass.");
        return 0;
    }

    /// <summary>Checks charge-beam phases, flare timing, and frame selection.</summary>
    [VerificationCommand("--samus-charge-phases")]
    internal static int SamusChargePhasesCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Charge phase oracle revision");
        Suite(nameof(VerifySamusChargeColorPhases), () => VerifySamusChargeColorPhases(rom));
        return 0;
    }

    /// <summary>Checks full-body Samus palette rows across suit variants.</summary>
    [VerificationCommand("--lookup-full-body-colors")]
    internal static int LookupFullBodyColorsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Full-body color oracle revision");
        Suite(nameof(VerifyFullBodyPaletteColorData), () => VerifyFullBodyPaletteColorData(rom, SamusFullBodyCycleColorExtractor.Extract(rom)));
        Console.WriteLine("Full-body palettes:48 native identities, all768 colors, complete pointer domain, edits and CGRAM copies pass.");
        return 0;
    }

    /// <summary>Checks loading-screen tilemap and text placement.</summary>
    [VerificationCommand("--lookup-loading-layout")]
    internal static int LookupLoadingLayoutCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Loading layout oracle revision");
        Suite(nameof(VerifySamusLoadingSuitPaletteFxColorDefinitions), () => VerifySamusLoadingSuitPaletteFxColorDefinitions());
        Console.WriteLine("Suit loading: original decoded group/frame layout, complete pointer ownership and guarded programs pass.");
        return 0;
    }

    /// <summary>Checks loading-screen palette rows against native colors.</summary>
    [VerificationCommand("--lookup-loading-colors")]
    internal static int LookupLoadingColorsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Loading colors oracle revision");
        byte[] json = RoomPaletteFxPresentationExtractor.Extract(rom);
        Suite(nameof(VerifyExtractedSamusLoadingPaletteFxPresentation), () => VerifyExtractedSamusLoadingPaletteFxPresentation(rom, RoomPaletteFxPresentation.Load(new MemoryStream(json))));
        foreach (int editMode in new[] { 0, 1, 2 })
        {
            var document = RequiredJson.Deserialize<RoomPaletteFxPresentationDocument>(json, ArtworkJson.Options);
            int ordinal = 1000;
            foreach (var frames in new[] { document.SamusLoadingPowerSuit, document.SamusLoadingVariaSuit, document.SamusLoadingGravitySuit })
            for (int frame = 0; frame < 9; frame++)
            for (int color = 0; color < 16; color++, ordinal++)
                if (editMode == 0 || (editMode == 1 && frame is 0 or 1) || (editMode == 2 && frame == 7))
                    frames[frame][color] = new PaletteRgb5 { Red = ordinal & 31, Green = ordinal >> 5 & 31, Blue = ordinal >> 10 & 31 };
            var edited = RoomPaletteFxPresentation.Load(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options)));
            ordinal = 1000;
            foreach (var program in SamusLoadingSuitPaletteFxColorDefinitions.All)
            for (int frame = 0; frame < 9; frame++)
            for (int color = 0; color < 16; color++, ordinal++)
            {
                ushort pointer = program.ColorPointer(frame, color);
                AssertTrue(edited.TryReadColor(pointer, out Bgr555 actual), "Edited loading color remains owned");
                ushort expected = editMode == 0 || (editMode == 1 && frame is 0 or 1) || (editMode == 2 && frame == 7) ? (ushort)ordinal : ReadVerificationWord(rom, 0x8d0000 | pointer);
                AssertEqual(expected, actual, "Independent loading row edits survive alias compilation");
            }
        }
        Console.WriteLine("Loading colors: all432 native colors, full pointer domain,25 stored words plus9 endpoint components, independent edits and guarded consumers pass.");
        return 0;
    }

    /// <summary>Checks heat-effect palette shades and their CGRAM destinations.</summary>
    [VerificationCommand("--lookup-heat-colors")]
    internal static int LookupHeatColorsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Heat color oracle revision");
        byte[] json = RoomPaletteFxPresentationExtractor.Extract(rom);
        var presentation = RoomPaletteFxPresentation.Load(new MemoryStream(json));
        Suite(nameof(VerifyExtractedSamusHeatPaletteFxPresentation), () => VerifyExtractedSamusHeatPaletteFxPresentation(rom, presentation));
        var document = RequiredJson.Deserialize<RoomPaletteFxPresentationDocument>(json,
            ArtworkJson.Options);
        int ordinal = 1000;
        foreach (var frames in new[] { document.SamusHeatPowerSuit, document.SamusHeatVariaSuit, document.SamusHeatGravitySuit })
        foreach (var row in (frames ?? throw new InvalidOperationException("Verification requires frames.")))
        for (int color = 0; color < row.Length; color++, ordinal++)
            row[color] = new PaletteRgb5 { Red = ordinal & 31, Green = ordinal >> 5 & 31, Blue = ordinal >> 10 & 31 };
        var edited = RoomPaletteFxPresentation.Load(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options)));
        ordinal = 1000;
        foreach (int first in new[] { 0xe468, 0xe694, 0xe8c0 })
        for (int phase = 0; phase < 16; phase++)
        for (int color = 0; color < 15; color++, ordinal++)
        {
            AssertTrue(edited.TryReadColor((ushort)(first + phase * 34 + color * 2), out Bgr555 actual), "Edited heat color owned");
            AssertEqual((ushort)ordinal, actual, "Every separately edited heat color survives alias compilation");
        }
        document = System.Text.Json.JsonSerializer.Deserialize<RoomPaletteFxPresentationDocument>(json,
            ArtworkJson.Options);
        foreach (var frames in new[] { (document ?? throw new InvalidOperationException("Verification requires document.")).SamusLoadingPowerSuit, document.SamusLoadingVariaSuit, document.SamusLoadingGravitySuit })
        for (int color = 0; color < 16; color++)
            frames[0][color] = new PaletteRgb5 { Red = 1, Green = 2, Blue = 3 };
        var loadingEdited = RoomPaletteFxPresentation.Load(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options)));
        foreach (int first in new[] { 0xe468, 0xe694, 0xe8c0 })
        for (int phase = 0; phase < 16; phase++)
        for (int color = 0; color < 15; color++)
        {
            ushort pointer = (ushort)(first + phase * 34 + color * 2);
            AssertTrue(loadingEdited.TryReadColor(pointer, out Bgr555 actual), "Heat color survives independent loading-palette edit");
            AssertEqual(ReadVerificationWord(rom, 0x8d0000 | pointer), actual, "Changing loading art cannot rewrite supplied heat art");
        }
        Console.WriteLine("Heat color aliases: original repeated rows, all pointer boundaries, stock storage, edited frames and guarded consumers pass.");
        return 0;
    }

    /// <summary>Checks heat palette selector tables and timing.</summary>
    [VerificationCommand("--lookup-heat-selectors")]
    internal static int LookupHeatSelectorsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Heat selector oracle revision");
        Suite(nameof(VerifyPaletteFxHeatInstructionListDefinitions), () => VerifyPaletteFxHeatInstructionListDefinitions(rom));
        Console.WriteLine("Heat selectors: all48 native words, program layout, bounds and full equipment selection pass.");
        return 0;
    }

    /// <summary>Checks Ceres haze color phases and update cadence.</summary>
    [VerificationCommand("--lookup-ceres-haze")]
    internal static int LookupCeresHazeCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres haze oracle revision");
        Suite(nameof(VerifyCeresHazeNativeRamp), () => VerifyCeresHazeNativeRamp(rom));
        Suite(nameof(VerifyCeresHazeTintScaling), () => VerifyCeresHazeTintScaling(rom));
        Suite(nameof(VerifyCeresHazePhaseControl), () => VerifyCeresHazePhaseControl(rom));
        Console.WriteLine("Ceres haze: original HDMA bands, all17 native counters, both channels, captured/software views and RGB5 tint scaling pass.");
        return 0;
    }

    /// <summary>Checks area-dependent palette-FX definition selection.</summary>
    [VerificationCommand("--lookup-palette-fx-areas")]
    internal static int LookupPaletteFxAreasCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Palette-FX area oracle revision");
        Suite(nameof(VerifyPaletteFxAreaListPointers), () => VerifyPaletteFxAreaListPointers(rom));
        Suite(nameof(VerifyPaletteFxAreaSelections), () => VerifyPaletteFxAreaSelections(rom));
        Console.WriteLine("Palette-FX areas: eight original list identities, all64 selections and rejection domains pass.");
        return 0;
    }

    /// <summary>Checks palette-FX opcode dispatch and authored control flow.</summary>
    [VerificationCommand("--lookup-palette-fx-dispatch")]
    internal static int LookupPaletteFxDispatchCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Palette-FX dispatch oracle revision");
        Suite(nameof(VerifyPaletteFxDispatch), () => VerifyPaletteFxDispatch(rom));
        Console.WriteLine("Palette-FX dispatch: all 63 native setup/list pairs, full identity domain and guarded spawning pass.");
        return 0;
    }

    /// <summary>Checks sky tile chunk pointers and transfer order.</summary>
    [VerificationCommand("--lookup-sky-chunk-pointers")]
    internal static int LookupSkyChunkPointersCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Sky chunk oracle revision");
        Suite(nameof(VerifyLandSkyChunkPointers), () => VerifyLandSkyChunkPointers(rom));
        Suite(nameof(VerifyOceanSkyChunkPointers), () => VerifyOceanSkyChunkPointers(rom));
        Console.WriteLine("Sky chunks: both native mappings, compatibility reads, rejections and all ushort camera inputs pass.");
        return 0;
    }

    /// <summary>Checks sky section layouts and source tile ranges.</summary>
    [VerificationCommand("--lookup-sky-sections")]
    internal static int LookupSkySectionsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Sky sections oracle revision");
        Suite(nameof(VerifySkySectionTopPositions), () => VerifySkySectionTopPositions(rom));
        Suite(nameof(VerifySkySectionSubspeeds), () => VerifySkySectionSubspeeds(rom));
        Suite(nameof(VerifySkySectionSpeeds), () => VerifySkySectionSpeeds(rom));
        Suite(nameof(VerifySkySectionDataSlots), () => VerifySkySectionDataSlots(rom));
        Suite(nameof(VerifyScrollingSkyState), () => VerifyScrollingSkyState());
        Console.WriteLine("Sky sections: four native fields, bounds, complete world-Y projection and existing integration pass.");
        return 0;
    }

    /// <summary>Checks conditions that suppress room earthquake effects.</summary>
    [VerificationCommand("--lookup-quake-suppression")]
    internal static int LookupQuakeSuppressionCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Quake suppression oracle revision");
        Suite(nameof(VerifyQuakeSoundSuppression), () => VerifyQuakeSoundSuppression(rom));
        Console.WriteLine("Quake suppression: native branches, all 65536 room identities and sound consumer pass.");
        return 0;
    }

    /// <summary>Checks earthquake sound selection and queue limits.</summary>
    [VerificationCommand("--lookup-quake-sound-selection")]
    internal static int LookupQuakeSoundSelectionCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Quake sound oracle revision");
        Suite(nameof(VerifyQuakeSoundSelection), () => VerifyQuakeSoundSelection(rom));
        Console.WriteLine("Quake sound selection: all eight original identities, loop marker and production queue requests pass.");
        return 0;
    }

    /// <summary>Checks liquid wave motion and native amplitude values.</summary>
    [VerificationCommand("--lookup-liquid-wave")]
    internal static int LookupLiquidWaveCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Liquid wave oracle revision");
        Suite(nameof(VerifyMirroredLiquidWave), () => VerifyMirroredLiquidWave(rom));
        Suite(nameof(VerifyHorizontalHeatWave), () => VerifyHorizontalHeatWave(rom));
        Console.WriteLine("Liquid waves: original mirrored and horizontal pulse samples, bounds, all phases and projection consumers pass.");
        return 0;
    }

    /// <summary>Checks rain particle speeds and signed motion.</summary>
    [VerificationCommand("--lookup-rain-velocity")]
    internal static int LookupRainVelocityCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Rain velocity oracle revision");
        Suite(nameof(VerifyRainHorizontalVelocity), () => VerifyRainHorizontalVelocity(rom));
        Console.WriteLine("Rain velocity: four signed 8.8 values, all 65536 RNG selections and invalid bounds pass.");
        return 0;
    }

    /// <summary>Checks retail door list pointers and their room ordering.</summary>
    [VerificationCommand("--lookup-retail-door-lists")]
    internal static int LookupRetailDoorListsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Door-list oracle revision");
        Suite(nameof(VerifyRetailDoorListMapping), () => VerifyRetailDoorListMapping(rom));
        Suite(nameof(VerifyCompiledDoorListCollision), () => VerifyCompiledDoorListCollision());
        return 0;
    }

    /// <summary>Checks Kraid arm hitbox instruction lists.</summary>
    [VerificationCommand("--lookup-kraid-arm-hitbox-lists")]
    internal static int LookupKraidArmHitboxListsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Kraid arm list oracle revision");
        Suite(nameof(VerifyKraidArmHitboxListSelection), () => VerifyKraidArmHitboxListSelection(rom));
        Console.WriteLine("Kraid arm hitbox selection: all 16 native lists, 24 ordered rectangles, full ushort rejection domain and slice bounds pass.");
        return 0;
    }

    /// <summary>Checks room PLM population parsing and native record boundaries.</summary>
    [VerificationCommand("--room-plm-populations")]
    internal static int RoomPlmPopulationsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Retail population oracle revision");
        Suite(nameof(VerifyRetailPopulationMappings), () => VerifyRetailPopulationMappings(rom));
        Suite(nameof(VerifyCompiledRoomPlmHeaderLoad), () => VerifyCompiledRoomPlmHeaderLoad(rom));
        Suite(nameof(VerifyPlmPopulationInputBoundary), () => VerifyPlmPopulationInputBoundary());
        Console.WriteLine("Retail populations: all 284 identities, 941 ordered placements and native terminators, sequential setup and historical state schemas pass.");
        return 0;
    }

    /// <summary>Checks room scroll PLM instruction lists and arguments.</summary>
    [VerificationCommand("--lookup-scroll-programs")]
    internal static int LookupScrollProgramsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Scroll program oracle revision");
        Suite(nameof(VerifyCompiledRoomScrollPrograms), () => VerifyCompiledRoomScrollPrograms(rom));
        Suite(nameof(VerifyRoomScrollPlms), () => VerifyRoomScrollPlms());
        Console.WriteLine("Scroll programs: all 173 identities, 285 ordered writes and terminators, guarded retail execution and constructed-room behavior pass.");
        return 0;
    }

    /// <summary>Checks eye-door PLM tile draw programs.</summary>
    [VerificationCommand("--eye-door-plm-draws")]
    internal static int EyeDoorPlmDrawsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Eye door oracle revision");
        Suite(nameof(VerifyEyeDoorPlmDrawDefinitions), () => VerifyEyeDoorPlmDrawDefinitions(rom));
        return 0;
    }

    /// <summary>Checks station access PLM headers and instruction selection.</summary>
    [VerificationCommand("--station-access-plm-definitions")]
    internal static int StationAccessPlmDefinitionsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Station access oracle revision");
        Suite(nameof(VerifyStationAccessPlmDefinitions), () => VerifyStationAccessPlmDefinitions(rom));
        return 0;
    }

    /// <summary>Checks elevator platform tiles and sprite placement.</summary>
    [VerificationCommand("--elevator-platform-visuals")]
    internal static int ElevatorPlatformVisualsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Elevator platform oracle revision");
        Suite(nameof(VerifyElevatorPlatformPlmDefinitions), () => VerifyElevatorPlatformPlmDefinitions(rom));
        Suite(nameof(VerifyElevatorPlatformVisuals), () => VerifyElevatorPlatformVisuals());
        return 0;
    }

    /// <summary>Checks Chozo statue PLM headers and activation programs.</summary>
    [VerificationCommand("--chozo-plm-definitions")]
    internal static int ChozoPlmDefinitionsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Chozo program oracle revision");
        Suite(nameof(VerifyChozoStatuePlmDefinitions), () => VerifyChozoStatuePlmDefinitions(rom));
        return 0;
    }

    /// <summary>Checks glass-shard projectile definitions and visual identities.</summary>
    [VerificationCommand("--mother-brain-glass-shard-definitions")]
    internal static int MotherBrainGlassShardDefinitionsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Glass shard oracle revision");
        Suite(nameof(VerifyMotherBrainGlassShardDefinitions), () => VerifyMotherBrainGlassShardDefinitions(rom));
        return 0;
    }

    /// <summary>Checks Mother Brain room glass PLM draw words.</summary>
    [VerificationCommand("--mother-brain-glass-plm-draws")]
    internal static int MotherBrainGlassPlmDrawsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Glass oracle revision");
        Suite(nameof(VerifyMotherBrainGlassPlmDrawDefinitions), () => VerifyMotherBrainGlassPlmDrawDefinitions(rom));
        return 0;
    }

    /// <summary>Checks Glass Tunnel tube PLM draw lists and tile words.</summary>
    [VerificationCommand("--noob-tube-plm-draws")]
    internal static int NoobTubePlmDrawsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Tube draw oracle revision");
        Suite(nameof(VerifyNoobTubePlmDrawDefinitions), () => VerifyNoobTubePlmDrawDefinitions(rom));
        return 0;
    }

    /// <summary>Checks colored door PLM tile variants.</summary>
    [VerificationCommand("--colored-door-plm-draws")]
    internal static int ColoredDoorPlmDrawsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Colored door oracle revision");
        Suite(nameof(VerifyColoredDoorPlmDrawDefinitions), () => VerifyColoredDoorPlmDrawDefinitions(rom));
        return 0;
    }

    /// <summary>Checks blue door PLM open and close tile lists.</summary>
    [VerificationCommand("--blue-door-plm-draws")]
    internal static int BlueDoorPlmDrawsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Blue door oracle revision");
        Suite(nameof(VerifyBlueDoorPlmDrawDefinitions), () => VerifyBlueDoorPlmDrawDefinitions(rom));
        return 0;
    }

    /// <summary>Checks gray door PLM draw lists and lock graphics.</summary>
    [VerificationCommand("--grey-door-plm-draws")]
    internal static int GreyDoorPlmDrawsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Grey door oracle revision");
        Suite(nameof(VerifyGreyDoorPlmDrawDefinitions), () => VerifyGreyDoorPlmDrawDefinitions(rom));
        Console.WriteLine("Grey door definitions and guarded lifecycle pass, including all Bomb Torizo program fields.");
        return 0;
    }

    /// <summary>Checks escape gate visual and instruction selection.</summary>
    [VerificationCommand("--lookup-escape-gate")]
    internal static int LookupEscapeGateCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Escape gate oracle revision");
        Suite(nameof(VerifyMotherBrainEscapeGateCompiledDefinitions), () => VerifyMotherBrainEscapeGateCompiledDefinitions(rom));
        Suite(nameof(VerifyMotherBrainEscapeRoomGate), () => VerifyMotherBrainEscapeRoomGate(new TestAddressSpace()));
        Suite(nameof(VerifyEscapeGateVisuals), () => VerifyEscapeGateVisuals(rom));
        Console.WriteLine("Escape gate: all original draw/program fields and complete domains pass; production door handoff and custom artwork pass.");
        return 0;
    }

    /// <summary>Checks native PLM headers and their bank-$84 pointers.</summary>
    [VerificationCommand("--lookup-retail-plm-headers")]
    internal static int LookupRetailPlmHeadersCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Retail header oracle revision");
        Suite(nameof(VerifyRetailPlmHeaderSetups), () => VerifyRetailPlmHeaderSetups(rom));
        Suite(nameof(VerifyRetailPlmHeaderInstructions), () => VerifyRetailPlmHeaderInstructions(rom));
        Suite(nameof(VerifyCompiledRoomPlmHeaderLoad), () => VerifyCompiledRoomPlmHeaderLoad(rom));
        Console.WriteLine("Retail PLM headers: all seventy setup/list cases and full input domain match ROM; guarded population load passes.");
        return 0;
    }

    /// <summary>Checks downward gate PLM programs and tile updates.</summary>
    [VerificationCommand("--downward-gate-definitions")]
    internal static int DownwardGateDefinitionsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Downward gate oracle revision");
        Suite(nameof(VerifyDownwardGateShotBlockDefinitions), () => VerifyDownwardGateShotBlockDefinitions(rom));
        return 0;
    }

    /// <summary>Checks speed-booster escape PLM identities and timing.</summary>
    [VerificationCommand("--speed-booster-escape-definitions")]
    internal static int SpeedBoosterEscapeDefinitionsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Speed escape stage oracle revision");
        Suite(nameof(VerifySpeedBoosterEscapeStageDefinitions), () => VerifySpeedBoosterEscapeStageDefinitions(rom));
        return 0;
    }

    /// <summary>Checks speed-booster escape instruction words and branch targets.</summary>
    [VerificationCommand("--lookup-speed-escape-program")]
    internal static int LookupSpeedEscapeProgramCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Speed escape oracle revision");
        Suite(nameof(RoomPlmProgramsVerification.VerifySpeedBoosterEscapePrograms), () => RoomPlmProgramsVerification.VerifySpeedBoosterEscapePrograms(rom));
        Suite(nameof(VerifySpeedBoosterEscapePlm), () => VerifySpeedBoosterEscapePlm(new TestAddressSpace()));
        Console.WriteLine("Speed escape program: all native fields and complete address domain pass; production handoffs and lava completion pass.");
        return 0;
    }

    /// <summary>Checks Samus-eater PLM headers and capture program words.</summary>
    [VerificationCommand("--samus-eater-plm-definitions")]
    internal static int SamusEaterPlmDefinitionsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "plant program NTSC J/U oracle");
        Suite(nameof(VerifySamusEaterPlmDefinitions), () => VerifySamusEaterPlmDefinitions(rom));
        return 0;
    }

    /// <summary>Checks special-air movement cases and native update constants.</summary>
    [VerificationCommand("--lookup-special-air")]
    internal static int LookupSpecialAirCommand()
    {
        var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Special-air NTSC J/U v1.0 oracle");
        Suite(nameof(VerifyQuicksandDefinitions), () => VerifyQuicksandDefinitions(oracle));
        Suite(nameof(VerifyQuicksand), () => VerifyQuicksand());
        return 0;
    }

    /// <summary>Checks speed-block collision selectors and PLM transitions.</summary>
    [VerificationCommand("--lookup-speed-blocks")]
    internal static int LookupSpeedBlocksCommand()
    {
        var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Speed blocks NTSC J/U v1.0 oracle");
        Suite(nameof(VerifyCompiledSpeedBoosterPlmPrograms), () => VerifyCompiledSpeedBoosterPlmPrograms());
        return 0;
    }

    /// <summary>Checks grapple-block behavior and its tile reactions.</summary>
    [VerificationCommand("--lookup-grapple-blocks")]
    internal static int LookupGrappleBlocksCommand()
    {
        var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Grapple blocks NTSC J/U v1.0 oracle");
        Suite(nameof(VerifyGrappleBlockPrograms), () => VerifyGrappleBlockPrograms());
        return 0;
    }

    /// <summary>Checks block reaction dispatch identities against native tables.</summary>
    [VerificationCommand("--lookup-block-reaction-selectors")]
    internal static int LookupBlockReactionSelectorsCommand()
    {
        var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Block reactions NTSC J/U v1.0 oracle");
        Suite(nameof(VerifySharedBreakAnimationSelection), () => VerifySharedBreakAnimationSelection(oracle));
        Suite(nameof(VerifyBombedRevealPhysicalDrawMapping), () => VerifyBombedRevealPhysicalDrawMapping(oracle));
        Suite(nameof(VerifyBombedRevealControlMapping), () => VerifyBombedRevealControlMapping(oracle));
        Suite(nameof(VerifyBombedRevealDrawMapping), () => VerifyBombedRevealDrawMapping(oracle));
        Suite(nameof(VerifyContactCrumbleHeaderSelection), () => VerifyContactCrumbleHeaderSelection(oracle));
        Suite(nameof(VerifyCollisionBombInstructionSelection), () => VerifyCollisionBombInstructionSelection(oracle));
        Suite(nameof(VerifyReactionBombInstructionSelection), () => VerifyReactionBombInstructionSelection(oracle));
        Suite(nameof(VerifyCrumbleRevealInstructionSelection), () => VerifyCrumbleRevealInstructionSelection(oracle));
        Suite(nameof(VerifyContactCrumbleInstructionSelection), () => VerifyContactCrumbleInstructionSelection(oracle));
        Suite(nameof(VerifyBombSpecialInstructionSelection), () => VerifyBombSpecialInstructionSelection(oracle));
        Suite(nameof(VerifyBombBlockPrograms), () => VerifyBombBlockPrograms());
        Suite(nameof(VerifyContactCrumblePrograms), () => VerifyContactCrumblePrograms());
        Console.WriteLine("Block reaction selectors: six complete native mappings and bomb/crumble production fixtures pass.");
        return 0;
    }

    /// <summary>Checks Mother Brain phase-two rear-leg palette colors.</summary>
    [VerificationCommand("--lookup-phase-two-rear-leg")]
    internal static int LookupPhaseTwoRearLegCommand()
    {
        var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Phase-two rear-leg NTSC J/U v1.0 oracle");
        Suite(nameof(VerifyStream3PhaseTwoRearLeg), () => VerifyStream3PhaseTwoRearLeg(oracle));
        Console.WriteLine("Phase-two rear leg: 15 native colors/destinations and 45 independent RGB edits pass.");
        return 0;
    }

    /// <summary>Checks auxiliary boss and room palette rows against source colors.</summary>
    [VerificationCommand("--lookup-auxiliary-palettes")]
    internal static int LookupAuxiliaryPalettesCommand()
    {
        var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Auxiliary palette NTSC J/U v1.0 oracle");
        Suite(nameof(VerifyStream3AuxiliaryPalettes), () => VerifyStream3AuxiliaryPalettes(oracle));
        Console.WriteLine("Auxiliary palettes: 393 native colors, 1179 independent edits, identities and bounds pass.");
        return 0;
    }

    /// <summary>Checks door-triggered quake records and encoded parameters.</summary>
    [VerificationCommand("--lookup-stream5-door-quake-decoding")]
    internal static int LookupStream5DoorQuakeDecodingCommand()
    {
        var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Door quake NTSC J/U v1.0 oracle");
        Suite(nameof(VerifyLookupStream5DoorQuakeDecoding), () => VerifyLookupStream5DoorQuakeDecoding(oracle));
        return 0;
    }

    /// <summary>Checks arm-cannon sprite selector cases by Samus pose.</summary>
    [VerificationCommand("--lookup-arm-cannon-selectors")]
    internal static int LookupArmCannonSelectorsCommand()
    {
        var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Arm cannon NTSC J/U v1.0 oracle");
        Suite(nameof(VerifyLookupStream1ArmCannonTileSources), () => VerifyLookupStream1ArmCannonTileSources(oracle));
        Console.WriteLine("Arm cannon selectors: native directions/frames, full reverse domain, independent edits/hash, DMA pixels and boundaries pass.");
        return 0;
    }

    /// <summary>Checks Ceres flight palette rows and animation selection.</summary>
    [VerificationCommand("--lookup-stream5-ceres-flight-palette")]
    internal static int LookupStream5CeresFlightPaletteCommand()
    {
        var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Ceres flight NTSC J/U v1.0 oracle");
        Suite(nameof(VerifyLookupStream5CeresFlightPalette), () => VerifyLookupStream5CeresFlightPalette(oracle));
        return 0;
    }

    /// <summary>Checks Oum instruction-list pointers and native table boundaries.</summary>
    [VerificationCommand("--lookup-stream-4-oum-layout")]
    internal static int LookupStream4OumLayoutCommand()
    {
        var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Oum NTSC J/U v1.0 oracle");
        Suite(nameof(VerifyLookupStream4OumListLayout), () => VerifyLookupStream4OumListLayout(oracle));
        return 0;
    }

    /// <summary>Checks scripted breakup phase order against cartridge controls.</summary>
    [VerificationCommand("--lookup-stream-4-breakup-order")]
    internal static int LookupStream4BreakupOrderCommand()
    {
        var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Ridley breakup NTSC J/U v1.0 oracle");
        Suite(nameof(VerifyRidleyExplosionDefinitions), () => VerifyRidleyExplosionDefinitions(oracle));
        Suite(nameof(VerifyLookupStream4BreakupOrder), () => VerifyLookupStream4BreakupOrder(oracle));
        return 0;
    }

    /// <summary>Runs the grouped stream-three visual and instruction checks.</summary>
    [VerificationCommand("--lookup-stream-3")]
    internal static int LookupStream3Command()
    {
        var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Stream 3 NTSC J/U v1.0 oracle");
        Suite(nameof(VerifyLookupStream3), () => VerifyLookupStream3(oracle));
        return 0;
    }

    /// <summary>Checks block art drawn for each shot reaction.</summary>
    [VerificationCommand("--lookup-shot-block-draws")]
    internal static int LookupShotBlockDrawsCommand()
    {
        var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Shot-block NTSC J/U v1.0 oracle");
        Suite(nameof(VerifyShotBlockPlmPrograms), () => VerifyShotBlockPlmPrograms());
        Suite(nameof(VerifyCompiledBotwoonWallPlms), () => VerifyCompiledBotwoonWallPlms());
        return 0;
    }

    /// <summary>Checks Botwoon wall PLM identities and room tile changes.</summary>
    [VerificationCommand("--lookup-botwoon-wall")]
    internal static int LookupBotwoonWallCommand()
    {
        var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Botwoon wall NTSC J/U v1.0 oracle");
        Suite(nameof(VerifyCompiledBotwoonWallPlms), () => VerifyCompiledBotwoonWallPlms());
        Suite(nameof(VerifyBotwoonWallStockMapping), () => VerifyBotwoonWallStockMapping(oracle));
        Suite(nameof(VerifyBotwoonWallVisualIdMapping), () => VerifyBotwoonWallVisualIdMapping());
        Suite(nameof(VerifyBotwoonWallVisualSeparation), () => VerifyBotwoonWallVisualSeparation(new SuperMetroid.Core.Rooms.Plms.RoomPlmBotwoonWallVisualCatalog(
            [new("clear-wall", [0xff,0xff,0xff,0x58,0xff,0xff,0xff,0xff,0xff])])));
        return 0;
    }

    /// <summary>Checks Botwoon hole collision bounds and entrance positions.</summary>
    [VerificationCommand("--lookup-botwoon-hole-bounds")]
    internal static int LookupBotwoonHoleBoundsCommand()
    {
        var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Botwoon hole NTSC J/U v1.0 oracle");
        Suite(nameof(VerifyBotwoonHoleRightBounds), () => VerifyBotwoonHoleRightBounds(oracle));
        Suite(nameof(VerifyBotwoonHoleBottomBounds), () => VerifyBotwoonHoleBottomBounds(oracle));
        Console.WriteLine("Botwoon hole bounds: all native right/bottom edges and production inclusion/exclusion checks pass.");
        return 0;
    }

    /// <summary>Checks Botwoon path table entries and movement targets.</summary>
    [VerificationCommand("--lookup-botwoon-path-descriptors")]
    internal static int LookupBotwoonPathDescriptorsCommand()
    {
        var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Botwoon navigation NTSC J/U v1.0 oracle");
        Suite(nameof(VerifyBotwoonPathDescriptorMappings), () => VerifyBotwoonPathDescriptorMappings(oracle));
        return 0;
    }

    /// <summary>Checks Botwoon projectile instruction lists and frame selection.</summary>
    [VerificationCommand("--lookup-botwoon-projectile-programs")]
    internal static int LookupBotwoonProjectileProgramsCommand()
    {
        var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Botwoon projectile NTSC J/U v1.0 oracle");
        Suite(nameof(VerifyBotwoonProjectileAnimations), () => VerifyBotwoonProjectileAnimations(oracle));
        Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
        return 0;
    }

    /// <summary>Checks Botwoon animation program chosen by phase and direction.</summary>
    [VerificationCommand("--lookup-botwoon-program-selection")]
    internal static int LookupBotwoonProgramSelectionCommand()
    {
        var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Botwoon selection NTSC J/U v1.0 oracle");
        Suite(nameof(VerifyBotwoonAnimationSelection), () => VerifyBotwoonAnimationSelection(oracle));
        return 0;
    }

    /// <summary>Checks Botwoon head instruction words and timing.</summary>
    [VerificationCommand("--lookup-botwoon-head-programs")]
    internal static int LookupBotwoonHeadProgramsCommand()
    {
        var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Botwoon NTSC J/U v1.0 oracle");
        Suite(nameof(VerifyBotwoonHeadAnimations), () => VerifyBotwoonHeadAnimations(oracle));
        Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
        return 0;
    }

    /// <summary>Checks Torizo statue PLM and enemy animation programs.</summary>
    [VerificationCommand("--lookup-torizo-statue-programs")]
    internal static int LookupTorizoStatueProgramsCommand()
    {
        var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Bomb Torizo statue NTSC J/U v1.0 oracle");
        Suite(nameof(VerifyBombTorizoStatueFragmentDefinitions), () => VerifyBombTorizoStatueFragmentDefinitions(oracle));
        Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
        return 0;
    }

    /// <summary>Checks dormant Torizo pose and drool projectile programs.</summary>
    [VerificationCommand("--lookup-torizo-dormant-and-drool")]
    internal static int LookupTorizoDormantAndDroolCommand()
    {
        var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Bomb Torizo NTSC J/U v1.0 oracle");
        Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
        return 0;
    }

    /// <summary>Checks Beetom instruction controls and presentation frames.</summary>
    [VerificationCommand("--lookup-beetom-programs")]
    internal static int LookupBeetomProgramsCommand()
    {
        var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Beetom NTSC J/U v1.0 oracle");
        Suite(nameof(VerifyBeetomInstructionProgramDefinitions), () => VerifyBeetomInstructionProgramDefinitions(oracle));
        Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
        return 0;
    }

    /// <summary>Checks moving-platform instruction lists and waits.</summary>
    [VerificationCommand("--lookup-platform-programs")]
    internal static int LookupPlatformProgramsCommand()
    {
        var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Platform NTSC J/U v1.0 oracle");
        Suite(nameof(VerifyPlatformInstructionProgramDefinitions), () => VerifyPlatformInstructionProgramDefinitions(oracle));
        Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
        return 0;
    }

    /// <summary>Checks elevator program commands and transition timing.</summary>
    [VerificationCommand("--lookup-elevator-programs")]
    internal static int LookupElevatorProgramsCommand()
    {
        var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Elevator NTSC J/U v1.0 oracle");
        Suite(nameof(VerifyElevatorInputDefinitions), () => VerifyElevatorInputDefinitions(oracle));
        Suite(nameof(ElevatorBehaviorVerification.VerifyElevatorBehavior), () => ElevatorBehaviorVerification.VerifyElevatorBehavior());
        Suite(nameof(ElevatorAnimationsVerification.VerifyElevatorAnimations), () => ElevatorAnimationsVerification.VerifyElevatorAnimations(oracle));
        Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
        return 0;
    }

    /// <summary>Checks growing-shutter definitions and frame table bounds.</summary>
    [VerificationCommand("--lookup-growing-shutter-definitions")]
    internal static int LookupGrowingShutterDefinitionsCommand()
    {
        var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Growing shutter NTSC J/U v1.0 oracle");
        Suite(nameof(VerifyCompiledGrowingShutters), () => VerifyCompiledGrowingShutters(oracle));
        return 0;
    }

    /// <summary>Checks shutter sprite poses and tile composition.</summary>
    [VerificationCommand("--lookup-shutter-visuals")]
    internal static int LookupShutterVisualsCommand()
    {
        var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Shutter NTSC J/U v1.0 oracle");
        Suite(nameof(ShutterBehaviorVerification.VerifyShutterExportFrames), () => ShutterBehaviorVerification.VerifyShutterExportFrames(oracle));
        Suite(nameof(ShutterBehaviorVerification.VerifyShutterBehavior), () => ShutterBehaviorVerification.VerifyShutterBehavior());
        Suite(nameof(ShutterAnimationsVerification.VerifyShutterAnimations), () => ShutterAnimationsVerification.VerifyShutterAnimations(oracle));
        Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
        return 0;
    }

    /// <summary>Checks shutter pose instruction lists and frame selection.</summary>
    [VerificationCommand("--lookup-shutter-pose-programs")]
    internal static int LookupShutterPoseProgramsCommand()
    {
        var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Shutter NTSC J/U v1.0 oracle");
        Suite(nameof(ShutterBehaviorVerification.VerifyShutterBehavior), () => ShutterBehaviorVerification.VerifyShutterBehavior());
        Suite(nameof(ShutterAnimationsVerification.VerifyShutterAnimations), () => ShutterAnimationsVerification.VerifyShutterAnimations(oracle));
        return 0;
    }

    /// <summary>Checks shutter initial AI function pointers and direction.</summary>
    [VerificationCommand("--lookup-shutter-initial-functions")]
    internal static int LookupShutterInitialFunctionsCommand()
    {
        var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Shutter NTSC J/U v1.0 oracle");
        Suite(nameof(ShutterBehaviorVerification.VerifyVerticalShutterInitialFunctionSelection), () => ShutterBehaviorVerification.VerifyVerticalShutterInitialFunctionSelection(oracle));
        Suite(nameof(ShutterBehaviorVerification.VerifyHorizontalShutterInitialFunctionSelection), () => ShutterBehaviorVerification.VerifyHorizontalShutterInitialFunctionSelection(oracle));
        Console.WriteLine("Shutter initial function selection: both native five-entry mappings, state semantics and invalid offsets pass.");
        return 0;
    }

    /// <summary>Checks vertical shutter program words and native delays.</summary>
    [VerificationCommand("--lookup-vertical-shutter-programs")]
    internal static int LookupVerticalShutterProgramsCommand()
    {
        var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Vertical shutter NTSC J/U v1.0 oracle");
        Suite(nameof(ShutterBehaviorVerification.VerifyShutterBehavior), () => ShutterBehaviorVerification.VerifyShutterBehavior());
        Suite(nameof(ShutterAnimationsVerification.VerifyShutterAnimations), () => ShutterAnimationsVerification.VerifyShutterAnimations(oracle));
        Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
        return 0;
    }

    /// <summary>Checks Fune and Namihe instruction programs against source words.</summary>
    [VerificationCommand("--lookup-fune-namihe-programs")]
    internal static int LookupFuneNamiheProgramsCommand()
    {
        var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)),
            "Fune/Namihe actor oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyFuneNamiheDefinitions), () => VerifyFuneNamiheDefinitions(oracle));
        Suite(nameof(VerifyFuneNamiheInstructionProgramDefinitions), () => VerifyFuneNamiheInstructionProgramDefinitions(oracle));
        Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
        return 0;
    }

    /// <summary>Checks their fireball projectile programs and frame selectors.</summary>
    [VerificationCommand("--lookup-fune-namihe-fireball-programs")]
    internal static int LookupFuneNamiheFireballProgramsCommand()
    {
        var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)),
            "Fune/Namihe fireball oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
        return 0;
    }

    /// <summary>Checks fireball launch positions and velocity cases.</summary>
    [VerificationCommand("--lookup-enemy-fireball-launches")]
    internal static int LookupEnemyFireballLaunchesCommand()
    {
        var launchOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(launchOracle.Rom)),
            "Enemy fireball launch oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyCompiledEnemyFireballLaunches), () => VerifyCompiledEnemyFireballLaunches(launchOracle));
        return 0;
    }

    /// <summary>Checks boulder instruction lists and terminal controls.</summary>
    [VerificationCommand("--lookup-boulder-programs")]
    internal static int LookupBoulderProgramsCommand()
    {
        var boulderOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(boulderOracle.Rom)),
            "Boulder oracle is NTSC J/U v1.0");
        Suite(nameof(BoulderBehaviorVerification.VerifyBoulderBehavior), () => BoulderBehaviorVerification.VerifyBoulderBehavior());
        Suite(nameof(BoulderAnimationsVerification.VerifyBoulderAnimations), () => BoulderAnimationsVerification.VerifyBoulderAnimations(boulderOracle));
        Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
        return 0;
    }

    /// <summary>Checks Alcoon walking, firing, and airborne program words.</summary>
    [VerificationCommand("--lookup-alcoon-programs")]
    internal static int LookupAlcoonProgramsCommand()
    {
        var alcoonOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(alcoonOracle.Rom)),
            "Alcoon oracle is NTSC J/U v1.0");
        Suite(nameof(AlcoonAnimationsVerification.VerifyAlcoonAnimations), () => AlcoonAnimationsVerification.VerifyAlcoonAnimations(alcoonOracle));
        Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
        Console.WriteLine("Alcoon programs: the typed programs match bank $A8.");
        return 0;
    }

    /// <summary>Checks Atomic enemy movement and animation programs.</summary>
    [VerificationCommand("--lookup-atomic-programs")]
    internal static int LookupAtomicProgramsCommand()
    {
        var atomicOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(atomicOracle.Rom)),
            "Atomic oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyAtomicMovementDefinitions), () => VerifyAtomicMovementDefinitions(atomicOracle));
        Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
        return 0;
    }

    /// <summary>Checks Draygon intro command sequence and timing.</summary>
    [VerificationCommand("--lookup-draygon-intro-commands")]
    internal static int LookupDraygonIntroCommandsCommand()
    {
        var danceOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(danceOracle.Rom)),
            "Draygon intro oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyDraygonIntroMovementDefinitions), () => VerifyDraygonIntroMovementDefinitions(danceOracle));
        Console.WriteLine("Draygon intro: explicit delete selection and all 1104 original X/Y results and bounds pass.");
        return 0;
    }

    /// <summary>Checks Kraid body and part OAM drawing against cartridge compositions.</summary>
    [VerificationCommand("--lookup-kraid-draws")]
    internal static int LookupKraidDrawsCommand()
    {
        var drawOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(drawOracle.Rom)),
            "Kraid draw oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyKraidDrawAddresses), () => VerifyKraidDrawAddresses());
        Suite(nameof(VerifyKraidDrawOwnerClassification), () => VerifyKraidDrawOwnerClassification(drawOracle));
        Suite(nameof(VerifyKraidDrawShapes), () => VerifyKraidDrawShapes(drawOracle));
        Suite(nameof(VerifyKraidDrawWords), () => VerifyKraidDrawWords(drawOracle));
        Suite(nameof(VerifyKraidDrawVisualIds), () => VerifyKraidDrawVisualIds());
        Suite(nameof(VerifyKraidRoomVisualSelection), () => VerifyKraidRoomVisualSelection());
        Console.WriteLine("Kraid draw definitions: ten identities, eight owners, native shapes, all 45 words, visual IDs and bounds pass.");
        return 0;
    }

    /// <summary>Checks Kraid room palette and tilemap transition programs.</summary>
    [VerificationCommand("--lookup-kraid-room-programs")]
    internal static int LookupKraidRoomProgramsCommand()
    {
        var roomProgramOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(roomProgramOracle.Rom)),
            "Kraid room program oracle is NTSC J/U v1.0");
        Suite(nameof(BossRoomPlmProgramsVerification.VerifyKraidRoomPrograms), () => BossRoomPlmProgramsVerification.VerifyKraidRoomPrograms(roomProgramOracle));
        return 0;
    }

    /// <summary>Checks Fake Kraid spike row tile data and placement.</summary>
    [VerificationCommand("--lookup-fake-kraid-spike-rows")]
    internal static int LookupFakeKraidSpikeRowsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Fake Kraid spike-row oracle revision");
        Suite(nameof(VerifyFakeKraidSpikeRowSelection), () => VerifyFakeKraidSpikeRowSelection(rom));
        Console.WriteLine("Fake Kraid spike rows: all three original signed launch positions and invalid selectors pass.");
        return 0;
    }

    /// <summary>Checks Fake Kraid spit trajectories against native signed values.</summary>
    [VerificationCommand("--lookup-fake-kraid-spit-velocities")]
    internal static int LookupFakeKraidSpitVelocitiesCommand()
    {
        var velocityOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(velocityOracle.Rom)),
            "Fake Kraid velocity oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyFakeKraidSpitHorizontalVelocity), () => VerifyFakeKraidSpitHorizontalVelocity(velocityOracle));
        Suite(nameof(VerifyFakeKraidSpitVerticalVelocity), () => VerifyFakeKraidSpitVerticalVelocity(velocityOracle));
        Console.WriteLine("Fake Kraid spit: both velocity fields match all four native launches; invalid ordinals pass.");
        return 0;
    }

    /// <summary>Checks Fake Kraid projectile lists and visual frames.</summary>
    [VerificationCommand("--lookup-fake-kraid-projectile-programs")]
    internal static int LookupFakeKraidProjectileProgramsCommand()
    {
        var projectileProgramOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(projectileProgramOracle.Rom)),
            "Fake Kraid projectile program oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyFakeKraidProjectileMechanicsMapping), () => VerifyFakeKraidProjectileMechanicsMapping(projectileProgramOracle));
        Suite(nameof(VerifyFakeKraidProjectilePresentationAddresses), () => VerifyFakeKraidProjectilePresentationAddresses());
        Console.WriteLine("Fake Kraid projectile programs: six native control words, complete bank ownership, three visual operand positions and bounds pass.");
        return 0;
    }

    /// <summary>Checks Kraid part frame selectors against native spritemap pointers.</summary>
    [VerificationCommand("--lookup-kraid-visual-selectors")]
    internal static int LookupKraidVisualSelectorsCommand()
    {
        var visualOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(visualOracle.Rom)),
            "Kraid visual oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
        Console.WriteLine("Kraid visual selectors: the compiled selector inventory passes; Mini-Kraid's frames come from its typed programs.");
        return 0;
    }

    /// <summary>Checks spit-rock speed table values against cartridge words.</summary>
    [VerificationCommand("--lookup-kraid-spit-speeds")]
    internal static int LookupKraidSpitSpeedsCommand()
    {
        var spitOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(spitOracle.Rom)),
            "Kraid spit oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyKraidRockLaunchDefinitions), () => VerifyKraidRockLaunchDefinitions(spitOracle, definitionsOnly: true));
        return 0;
    }

    /// <summary>Checks Kraid palette sources and destination bands.</summary>
    [VerificationCommand("--lookup-kraid-palette-definitions")]
    internal static int LookupKraidPaletteDefinitionsCommand()
    {
        var paletteOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(paletteOracle.Rom)),
            "Kraid palette oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyKraidPaletteSourceAddresses), () => VerifyKraidPaletteSourceAddresses(paletteOracle));
        Suite(nameof(VerifyKraidPaletteSourceLengths), () => VerifyKraidPaletteSourceLengths());
        Console.WriteLine("Kraid palette definitions: five native source operands, complete extents and invalid enums pass.");
        return 0;
    }

    /// <summary>Checks health-band thresholds used for Kraid palette changes.</summary>
    [VerificationCommand("--lookup-kraid-health-thresholds")]
    internal static int LookupKraidHealthThresholdsCommand()
    {
        var healthOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(healthOracle.Rom)),
            "Kraid health oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyKraidHealthEighths), () => VerifyKraidHealthEighths(healthOracle));
        Suite(nameof(VerifyKraidHealthQuarters), () => VerifyKraidHealthQuarters(healthOracle));
        Console.WriteLine("Kraid health thresholds: both native recurrences across every initial health and ordinal, plus bounds, pass.");
        return 0;
    }

    /// <summary>Checks falling ceiling-rock spawn order and positions.</summary>
    [VerificationCommand("--lookup-kraid-ceiling-order")]
    internal static int LookupKraidCeilingOrderCommand()
    {
        var ceilingOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ceilingOracle.Rom)),
            "Kraid ceiling oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyKraidCeilingRockCoordinates), () => VerifyKraidCeilingRockCoordinates(ceilingOracle));
        Suite(nameof(VerifyKraidGrowthPlmColumns), () => VerifyKraidGrowthPlmColumns(ceilingOracle));
        Suite(nameof(VerifyKraidGrowthPlmRows), () => VerifyKraidGrowthPlmRows(ceilingOracle));
        Suite(nameof(VerifyKraidGrowthPlmHeaders), () => VerifyKraidGrowthPlmHeaders(ceilingOracle));
        Console.WriteLine("Kraid ceiling order: every native byte window and all derived PLM fields match; bounds pass.");
        return 0;
    }

    /// <summary>Checks death-sink table entries and BG2 row mapping.</summary>
    [VerificationCommand("--lookup-kraid-sink-mappings")]
    internal static int LookupKraidSinkMappingsCommand()
    {
        var sinkOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(sinkOracle.Rom)),
            "Kraid sinking oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyKraidSinkSchedule), () => VerifyKraidSinkSchedule(sinkOracle, definitionsOnly: true));
        return 0;
    }

    /// <summary>Checks Kraid defeated-room PLM requests and tile repairs.</summary>
    [VerificationCommand("--lookup-kraid-defeated-plms")]
    internal static int LookupKraidDefeatedPlmsCommand()
    {
        var defeatedPlmOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(defeatedPlmOracle.Rom)),
            "Kraid defeated PLM oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyKraidDefeatedPlmColumns), () => VerifyKraidDefeatedPlmColumns(defeatedPlmOracle));
        Suite(nameof(VerifyKraidDefeatedPlmRows), () => VerifyKraidDefeatedPlmRows(defeatedPlmOracle));
        Suite(nameof(VerifyKraidDefeatedPlmHeaders), () => VerifyKraidDefeatedPlmHeaders(defeatedPlmOracle));
        Console.WriteLine("Kraid defeated-room PLMs: both native requests, three fields, order and bounds pass.");
        return 0;
    }

    /// <summary>Checks PLM tile changes during the growth transition.</summary>
    [VerificationCommand("--lookup-kraid-growth-plms")]
    internal static int LookupKraidGrowthPlmsCommand()
    {
        var growthPlmOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(growthPlmOracle.Rom)),
            "Kraid growth PLM oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyKraidGrowthPlmColumns), () => VerifyKraidGrowthPlmColumns(growthPlmOracle));
        Suite(nameof(VerifyKraidGrowthPlmRows), () => VerifyKraidGrowthPlmRows(growthPlmOracle));
        Suite(nameof(VerifyKraidGrowthPlmHeaders), () => VerifyKraidGrowthPlmHeaders(growthPlmOracle));
        Console.WriteLine("Kraid growth PLMs: all nine native columns, rows and headers, enumeration and bounds pass.");
        return 0;
    }

    /// <summary>Checks Kraid body and foot movement choices against native data.</summary>
    [VerificationCommand("--lookup-kraid-movement")]
    internal static int LookupKraidMovementCommand()
    {
        var movementOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(movementOracle.Rom)),
            "Kraid movement oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyKraidMovementChoices), () => VerifyKraidMovementChoices(movementOracle, definitionsOnly: true));
        return 0;
    }

    /// <summary>Checks fingernail launch speeds and direction selection.</summary>
    [VerificationCommand("--lookup-kraid-nail-launch")]
    internal static int LookupKraidNailLaunchCommand()
    {
        var launchOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(launchOracle.Rom)),
            "Kraid launch oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyKraidNailLaunchXFraction), () => VerifyKraidNailLaunchXFraction(launchOracle));
        Suite(nameof(VerifyKraidNailLaunchXWhole), () => VerifyKraidNailLaunchXWhole(launchOracle));
        Suite(nameof(VerifyKraidNailLaunchYFraction), () => VerifyKraidNailLaunchYFraction(launchOracle));
        Suite(nameof(VerifyKraidNailLaunchYWhole), () => VerifyKraidNailLaunchYWhole(launchOracle));
        Console.WriteLine("Kraid nail launch: four fields, all sibling words and four RNG choices match native indirect records.");
        return 0;
    }

    /// <summary>Checks camera scroll changes during Kraid's growth and death.</summary>
    [VerificationCommand("--lookup-kraid-camera")]
    internal static int LookupKraidCameraCommand()
    {
        var cameraOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(cameraOracle.Rom)),
            "Kraid camera oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyKraidInitialScrollMapping), () => VerifyKraidInitialScrollMapping(cameraOracle));
        Suite(nameof(VerifyKraidGrownScrollMapping), () => VerifyKraidGrownScrollMapping(cameraOracle));
        Console.WriteLine("Kraid camera: both four-screen mappings match native instruction operands; bounds pass.");
        return 0;
    }

    /// <summary>Checks arm component frame selector table entries.</summary>
    [VerificationCommand("--lookup-kraid-arm-component-selectors")]
    internal static int LookupKraidArmComponentSelectorsCommand()
    {
        var armComponentOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(armComponentOracle.Rom)),
            "Kraid arm component oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyKraidArmCollisionDefinitions), () => VerifyKraidArmCollisionDefinitions(armComponentOracle));
        return 0;
    }

    /// <summary>Checks arm hitbox callback pointers and shot responses.</summary>
    [VerificationCommand("--lookup-kraid-arm-callbacks")]
    internal static int LookupKraidArmCallbacksCommand()
    {
        var armCallbackOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(armCallbackOracle.Rom)),
            "Kraid arm callback oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyKraidArmCollisionDefinitions), () => VerifyKraidArmCollisionDefinitions(armCallbackOracle));
        return 0;
    }

    /// <summary>Checks physical arm frame composition against cartridge art.</summary>
    [VerificationCommand("--lookup-kraid-arm-physical-frames")]
    internal static int LookupKraidArmPhysicalFramesCommand()
    {
        var armFrameOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(armFrameOracle.Rom)),
            "Kraid arm physical-frame oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyKraidArmComponentHitboxes), () => VerifyKraidArmComponentHitboxes(armFrameOracle));
        Suite(nameof(VerifyKraidArmPhysicalFramePointers), () => VerifyKraidArmPhysicalFramePointers(armFrameOracle));
        _ = VerifyKraidArmPhysicalLayoutSelection(armFrameOracle);
        Console.WriteLine("Kraid arm physical frames: all 22 roots, ordered native layouts, complete pointer domain and ordinal bounds pass.");
        return 0;
    }

    /// <summary>Checks body-relative foot coordinates for each pose.</summary>
    [VerificationCommand("--lookup-kraid-foot-coordinates")]
    internal static int LookupKraidFootCoordinatesCommand()
    {
        var footOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(footOracle.Rom)),
            "Kraid foot coordinate oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyKraidFootFirstX), () => VerifyKraidFootFirstX(footOracle));
        Suite(nameof(VerifyKraidFootFirstY), () => VerifyKraidFootFirstY(footOracle));
        Suite(nameof(VerifyKraidFootSecondX), () => VerifyKraidFootSecondX(footOracle));
        Suite(nameof(VerifyKraidFootSecondY), () => VerifyKraidFootSecondY(footOracle));
        Suite(nameof(VerifyKraidFootSharedHitbox), () => VerifyKraidFootSharedHitbox(footOracle));
        Console.WriteLine("Kraid foot coordinates: four fields across 35 frames plus initial alias, shared geometry/callbacks, membership and bounds pass.");
        return 0;
    }

    /// <summary>Checks body contour collision cases across vertical positions.</summary>
    [VerificationCommand("--lookup-kraid-contour-cases")]
    internal static int LookupKraidContourCasesCommand()
    {
        var contourOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(contourOracle.Rom)),
            "Kraid contour oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyKraidBodyContourCases), () => VerifyKraidBodyContourCases(contourOracle));
        Suite(nameof(VerifyKraidGrowthResumeCases), () => VerifyKraidGrowthResumeCases(contourOracle));
        Console.WriteLine("Kraid body contour and growth resume: full signed-Y and tilemap domains match native records and instruction operands.");
        return 0;
    }

    /// <summary>Checks fingernail reflection against the body contour.</summary>
    [VerificationCommand("--lookup-kraid-nail-contour")]
    internal static int LookupKraidNailContourCommand()
    {
        var nailOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(nailOracle.Rom)),
            "Kraid nail oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyKraidNailLeftOffsets), () => VerifyKraidNailLeftOffsets(nailOracle));
        Suite(nameof(VerifyKraidNailTopBoundaries), () => VerifyKraidNailTopBoundaries(nailOracle));
        Console.WriteLine("Kraid nail contour: all 12 original words, all 65536 wrapped relative-Y selections and bounds pass.");
        return 0;
    }

    /// <summary>Checks mouth hitbox pointer and frame mappings.</summary>
    [VerificationCommand("--lookup-kraid-mouth-mappings")]
    internal static int LookupKraidMouthMappingsCommand()
    {
        var mouthOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(mouthOracle.Rom)),
            "Kraid mouth oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyKraidMouthShapeCases), () => VerifyKraidMouthShapeCases(mouthOracle));
        Console.WriteLine("Kraid mouth mappings: 32 geometry words, 7 instruction bytes, head/mouth boundary crossings and bounds pass.");
        return 0;
    }

    /// <summary>Checks Kraid head instruction lists and branch control words.</summary>
    [VerificationCommand("--lookup-kraid-head-programs")]
    internal static int LookupKraidHeadProgramsCommand()
    {
        var headOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(headOracle.Rom)),
            "Kraid head oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyKraidHeadCommandMapping), () => VerifyKraidHeadCommandMapping(headOracle));
        Console.WriteLine("Kraid head programs: all 28 native commands, frame fields, sound IDs, enumeration, exact cursors and bounds pass.");
        return 0;
    }

    /// <summary>Checks Crocomire skeleton OAM frames and part geometry.</summary>
    [VerificationCommand("--lookup-crocomire-skeleton-frames")]
    internal static int LookupCrocomireSkeletonFramesCommand()
    {
        var skeletonOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(skeletonOracle.Rom)),
            "Skeleton frame oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyCrocomireSkeletonFrameGeometry), () => VerifyCrocomireSkeletonFrameGeometry(skeletonOracle));
        Suite(nameof(VerifyExtendedFrameSequence), () => VerifyExtendedFrameSequence());
        Console.WriteLine("Crocomire skeleton catalog: 33 native roots, geometry, names, enumeration, membership and bounds pass.");
        return 0;
    }

    /// <summary>Checks foot frame identities and body-relative placement.</summary>
    [VerificationCommand("--lookup-kraid-foot-frames")]
    internal static int LookupKraidFootFramesCommand()
    {
        var footOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(footOracle.Rom)),
            "Kraid foot oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyKraidFootFrameGeometry), () => VerifyKraidFootFrameGeometry(footOracle));
        Suite(nameof(VerifyExtendedFrameSequence), () => VerifyExtendedFrameSequence());
        Console.WriteLine("Kraid foot catalog: 35 native roots, names, geometry, enumeration and bounds pass.");
        return 0;
    }

    /// <summary>Checks root OAM coordinates shared by boss compositions.</summary>
    [VerificationCommand("--lookup-boss-oam-roots")]
    internal static int LookupBossOamRootsCommand()
    {
        var rootOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rootOracle.Rom)),
            "Boss OAM root oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyRidleyOamRootGeometry), () => VerifyRidleyOamRootGeometry(rootOracle));
        Suite(nameof(VerifyDraygonOamRootGeometry), () => VerifyDraygonOamRootGeometry(rootOracle));
        Suite(nameof(VerifySporeSpawnOamRootGeometry), () => VerifySporeSpawnOamRootGeometry(rootOracle));
        Suite(nameof(VerifyExtendedFrameSequence), () => VerifyExtendedFrameSequence());
        Console.WriteLine("Boss OAM roots: 71 original identities, native geometry and bounds pass.");
        return 0;
    }

    /// <summary>Checks Mother Brain head, body, and room visual catalog identities.</summary>
    [VerificationCommand("--lookup-mother-brain-visual-catalogs")]
    internal static int LookupMotherBrainVisualCatalogsCommand()
    {
        var catalogOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(catalogOracle.Rom)),
            "Mother Brain catalog oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyMotherBrainGeneratedOamCatalog), () => VerifyMotherBrainGeneratedOamCatalog(catalogOracle));
        Suite(nameof(VerifyMotherBrainGeneratedBg2Catalog), () => VerifyMotherBrainGeneratedBg2Catalog(catalogOracle));
        Console.WriteLine("Mother Brain catalogs: 17 OAM/16 BG2 identities and names, geometry, membership, exact JSON and loading pass.");
        return 0;
    }

    /// <summary>Checks Phantoon and Draygon BG2 frame catalogs against source tiles.</summary>
    [VerificationCommand("--lookup-phantoon-draygon-bg2-catalogs")]
    internal static int LookupPhantoonDraygonBg2CatalogsCommand()
    {
        var catalogOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(catalogOracle.Rom)),
            "Boss BG2 catalog oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyPhantoonGeneratedBg2Catalog), () => VerifyPhantoonGeneratedBg2Catalog(catalogOracle));
        Suite(nameof(VerifyDraygonGeneratedBg2Catalog), () => VerifyDraygonGeneratedBg2Catalog(catalogOracle));
        Console.WriteLine("Phantoon/Draygon BG2: 56 original identities/names, geometry, selector domains, exact JSON and loading pass.");
        return 0;
    }

    /// <summary>Checks Crocomire BG2 tilemap frames and source data.</summary>
    [VerificationCommand("--lookup-crocomire-bg2-catalog")]
    internal static int LookupCrocomireBg2CatalogCommand()
    {
        var catalogOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(catalogOracle.Rom)),
            "Crocomire BG2 catalog oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyCrocomireBg2GeneratedCatalog), () => VerifyCrocomireBg2GeneratedCatalog(catalogOracle));
        Console.WriteLine("Crocomire BG2 catalog: 42 native identities/names, exact extracted JSON, loading and bounds pass.");
        return 0;
    }

    /// <summary>Checks Crocomire body animation frame composition.</summary>
    [VerificationCommand("--lookup-crocomire-body-frames")]
    internal static int LookupCrocomireBodyFramesCommand()
    {
        var bodyOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bodyOracle.Rom)),
            "Crocomire body oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyCrocomireBodyFrameGeometry), () => VerifyCrocomireBodyFrameGeometry(bodyOracle));
        Console.WriteLine("Crocomire body: 50 original frame roots, native geometry, full BG2 membership domain and bounds pass.");
        return 0;
    }

    /// <summary>Checks tongue hitbox shape and contact behavior.</summary>
    [VerificationCommand("--lookup-crocomire-tongue-collision")]
    internal static int LookupCrocomireTongueCollisionCommand()
    {
        var tongueOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(tongueOracle.Rom)),
            "Crocomire tongue collision oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyCrocomireTongueFramePositions), () => VerifyCrocomireTongueFramePositions(tongueOracle));
        Suite(nameof(VerifyCrocomireTongueComponentCases), () => VerifyCrocomireTongueComponentCases(tongueOracle));
        Console.WriteLine("Crocomire tongue collision: nine native frame identities/components, empty hitbox cases and bounds pass.");
        return 0;
    }

    /// <summary>Checks BG2 pose selection during Crocomire's fight.</summary>
    [VerificationCommand("--lookup-crocomire-bg2-poses")]
    internal static int LookupCrocomireBg2PosesCommand()
    {
        var bg2Oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bg2Oracle.Rom)),
            "Crocomire BG2 oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyCrocomireBg2ScrollDefinitions), () => VerifyCrocomireBg2ScrollDefinitions(bg2Oracle));
        Console.WriteLine("Crocomire BG2: all 17 pointers and corrections, full selector domain and wrapped scroll values pass.");
        return 0;
    }

    /// <summary>Checks VRAM transfer order during Crocomire's melt sequence.</summary>
    [VerificationCommand("--lookup-crocomire-melt-transfers")]
    internal static int LookupCrocomireMeltTransfersCommand()
    {
        var meltOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(meltOracle.Rom)),
            "Crocomire melt oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyCrocomireMeltHeaders), () => VerifyCrocomireMeltHeaders(meltOracle));
        Suite(nameof(VerifyCrocomireMeltCopies), () => VerifyCrocomireMeltCopies(meltOracle));
        Suite(nameof(VerifyCrocomireMeltUploads), () => VerifyCrocomireMeltUploads(meltOracle));
        Console.WriteLine("Crocomire melt: two headers, 13 copies, 13 uploads, sentinels and bounds pass.");
        return 0;
    }

    /// <summary>Checks Crocomire projectile instruction lists and selectors.</summary>
    [VerificationCommand("--lookup-crocomire-projectile-programs")]
    internal static int LookupCrocomireProjectileProgramsCommand()
    {
        var projectileOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(projectileOracle.Rom)),
            "Crocomire projectile oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyCrocomireProjectileAnimations), () => VerifyCrocomireProjectileAnimations(projectileOracle));
        return 0;
    }

    /// <summary>Checks tongue animation commands and timing.</summary>
    [VerificationCommand("--lookup-crocomire-tongue-program")]
    internal static int LookupCrocomireTongueProgramCommand()
    {
        var tongueOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(tongueOracle.Rom)),
            "Crocomire tongue oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyCrocomireTongueAnimations), () => VerifyCrocomireTongueAnimations(tongueOracle));
        return 0;
    }

    /// <summary>Checks spike-wall motion and signed displacement.</summary>
    [VerificationCommand("--lookup-crocomire-spike-motion")]
    internal static int LookupCrocomireSpikeMotionCommand()
    {
        var spikeOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(spikeOracle.Rom)),
            "Crocomire spike oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyCrocomireSpikeAccelerationDelta), () => VerifyCrocomireSpikeAccelerationDelta(spikeOracle));
        Suite(nameof(VerifyCrocomireSpikeMaximumAcceleration), () => VerifyCrocomireSpikeMaximumAcceleration(spikeOracle));
        Suite(nameof(VerifyCrocomireSpikeMaximumVelocity), () => VerifyCrocomireSpikeMaximumVelocity(spikeOracle));
        Console.WriteLine("Crocomire spike motion: all 54 native slot values and invalid bounds pass.");
        return 0;
    }

    /// <summary>Checks shared sine samples used by combined attack motion.</summary>
    [VerificationCommand("--lookup-combo-sine-alias")]
    internal static int LookupComboSineAliasCommand()
    {
        var comboOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(comboOracle.Rom)),
            "Combo sine oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyComboSineOffsetAlias), () => VerifyComboSineOffsetAlias(comboOracle));
        Console.WriteLine("Combo sine alias: all 65,536 byte-angle/radius pairs match native split multiplication.");
        return 0;
    }

    /// <summary>Checks Zoa visual frame selection and animation cadence.</summary>
    [VerificationCommand("--lookup-zoa-animation")]
    internal static int LookupZoaAnimationCommand()
    {
        var animationOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(animationOracle.Rom)),
            "Zoa animation oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyZoaAnimationDefinitions), () => VerifyZoaAnimationDefinitions(animationOracle, definitionsOnly: true));
        return 0;
    }

    /// <summary>Checks Zoa enemy instruction lists and branch targets.</summary>
    [VerificationCommand("--lookup-zoa-program")]
    internal static int LookupZoaProgramCommand()
    {
        var programOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(programOracle.Rom)),
            "Zoa program oracle is NTSC J/U v1.0");
        Suite(nameof(ZoaAnimationsVerification.VerifyZoaAnimations), () => ZoaAnimationsVerification.VerifyZoaAnimations(programOracle));
        return 0;
    }

    /// <summary>Checks Zoa motion speeds for each directional case.</summary>
    [VerificationCommand("--lookup-zoa-speed-cases")]
    internal static int LookupZoaSpeedCasesCommand()
    {
        var zoaOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(zoaOracle.Rom)),
            "Zoa oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyCompiledZoaSpeeds), () => VerifyCompiledZoaSpeeds(zoaOracle, definitionsOnly: true));
        return 0;
    }

    /// <summary>Checks grapple beam launch origins across Samus poses.</summary>
    [VerificationCommand("--lookup-grapple-origin-cases")]
    internal static int LookupGrappleOriginCasesCommand()
    {
        var originOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(originOracle.Rom)),
            "Grapple origin oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyGrappleOriginXSelection), () => VerifyGrappleOriginXSelection(originOracle));
        Suite(nameof(VerifyGrappleOriginDefaultYSelection), () => VerifyGrappleOriginDefaultYSelection(originOracle));
        Suite(nameof(VerifyGrappleOriginRunningYSelection), () => VerifyGrappleOriginRunningYSelection(originOracle));
        Console.WriteLine("Grapple origin cases: all 40 original words across three mappings and invalid bounds pass.");
        return 0;
    }

    /// <summary>Checks suit and beam palette curve interpolation values.</summary>
    [VerificationCommand("--lookup-suit-beam-curve")]
    internal static int LookupSuitBeamCurveCommand()
    {
        var suitOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(suitOracle.Rom)),
            "Suit beam oracle is NTSC J/U v1.0");
        Suite(nameof(VerifySuitPickupBeamCurveDefinitions), () => VerifySuitPickupBeamCurveDefinitions(suitOracle));
        return 0;
    }

    /// <summary>Checks fixed-point tangent values against the native table.</summary>
    [VerificationCommand("--lookup-absolute-tangent")]
    internal static int LookupAbsoluteTangentCommand()
    {
        var tangentOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(tangentOracle.Rom)),
            "Tangent oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyCompiledAbsoluteTangent), () => VerifyCompiledAbsoluteTangent(tangentOracle, definitionsOnly: true));
        Console.WriteLine("Absolute tangent algorithm: all 129 original words, direct caller aliases and invalid bounds pass.");
        return 0;
    }

    /// <summary>Checks Shaktool joint positions and body-link motion.</summary>
    [VerificationCommand("--lookup-shaktool-joint-algorithms")]
    internal static int LookupShaktoolJointAlgorithmsCommand()
    {
        var jointOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(jointOracle.Rom)),
            "Shaktool joint oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyShaktoolInitialAngleAlgorithm), () => VerifyShaktoolInitialAngleAlgorithm(jointOracle));
        Console.WriteLine("Shaktool joint algorithms: all 14 original words, velocity alias and invalid bounds pass.");
        return 0;
    }

    /// <summary>Checks power-bomb expansion curve samples.</summary>
    [VerificationCommand("--lookup-power-bomb-base-curves")]
    internal static int LookupPowerBombBaseCurvesCommand()
    {
        var powerBombOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(powerBombOracle.Rom)),
            "Power Bomb oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyPowerBombWidthAlgorithm), () => VerifyPowerBombWidthAlgorithm(powerBombOracle));
        Suite(nameof(VerifyPowerBombTopOffsetAlgorithm), () => VerifyPowerBombTopOffsetAlgorithm(powerBombOracle));
        Console.WriteLine("Power Bomb base curves: all 64 original bytes and invalid bounds pass.");
        return 0;
    }

    /// <summary>Checks Shaktool orbit offsets and angle progression.</summary>
    [VerificationCommand("--lookup-shaktool-orbit")]
    internal static int LookupShaktoolOrbitCommand()
    {
        var orbitOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(orbitOracle.Rom)),
            "Shaktool oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyShaktoolOrbitAlgorithm), () => VerifyShaktoolOrbitAlgorithm(orbitOracle));
        Console.WriteLine("Shaktool orbit: all320 words, rounding intervals,256 displacement pairs and bounds pass.");
        return 0;
    }

    /// <summary>Checks signed sine samples used by movement and projectile routines.</summary>
    [VerificationCommand("--lookup-signed-sine-review")]
    internal static int LookupSignedSineReviewCommand()
    {
        var signedOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(signedOracle.Rom)),
            "Signed sine oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyCompiledSignedTrigonometry), () => VerifyCompiledSignedTrigonometry(signedOracle, definitionsOnly: true));
        Suite(nameof(VerifySignedSixteenBitSineDefinitions), () => VerifySignedSixteenBitSineDefinitions(signedOracle));
        Suite(nameof(VerifyPhantoonWaveMath), () => VerifyPhantoonWaveMath(signedOracle, definitionsOnly: true));
        Console.WriteLine("Signed sine review: 320 signed words, 256 sixteen-bit words and every Phantoon byte phase pass.");
        return 0;
    }

    /// <summary>Checks half-wave interpolation and mirrored sample rules.</summary>
    [VerificationCommand("--lookup-half-wave-algorithms")]
    internal static int LookupHalfWaveAlgorithmsCommand()
    {
        var sineOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(sineOracle.Rom)),
            "Sine oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyEightBitHalfWaveAlgorithm), () => VerifyEightBitHalfWaveAlgorithm(sineOracle));
        Suite(nameof(VerifyUnsignedHalfWaveAlgorithm), () => VerifyUnsignedHalfWaveAlgorithm(sineOracle));
        Console.WriteLine("Half-wave algorithms: all 256 original samples, rounding intervals and invalid bounds pass.");
        return 0;
    }

    /// <summary>Checks SPC pitch basis values and note scaling.</summary>
    [VerificationCommand("--lookup-spc-pitch-basis")]
    internal static int LookupSpcPitchBasisCommand()
    {
        var pitchOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(pitchOracle.Rom)),
            "SPC pitch oracle is NTSC J/U v1.0");
        Suite(nameof(VerifySpcPitchBasisAlgorithm), () => VerifySpcPitchBasisAlgorithm(pitchOracle));
        Console.WriteLine("SPC pitch basis: all13 original words, root bounds and invalid indices pass.");
        return 0;
    }

    /// <summary>Checks sound-effect program selection for SPC commands.</summary>
    [VerificationCommand("--lookup-spc-effect-selection")]
    internal static int LookupSpcEffectSelectionCommand()
    {
        var effectOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(effectOracle.Rom)),
            "SPC effect oracle is NTSC J/U v1.0");
        Suite(nameof(VerifySpcEffectOperandSelection), () => VerifySpcEffectOperandSelection(effectOracle));
        Console.WriteLine("SPC effects: all 31 original operand counts and invalid index bounds pass.");
        return 0;
    }

    /// <summary>Checks SPC note percentage scaling against native data.</summary>
    [VerificationCommand("--lookup-spc-note-percentages")]
    internal static int LookupSpcNotePercentagesCommand()
    {
        var noteOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(noteOracle.Rom)),
            "SPC percentage oracle is NTSC J/U v1.0");
        Suite(nameof(VerifySpcNoteVolumeAlgorithm), () => VerifySpcNoteVolumeAlgorithm(noteOracle));
        Suite(nameof(VerifySpcNoteGateAlgorithm), () => VerifySpcNoteGateAlgorithm(noteOracle));
        Console.WriteLine("SPC note percentages: all24 original bytes, every timing-command selector and invalid bounds pass.");
        return 0;
    }

    /// <summary>Checks HDMA window curve samples and bounds.</summary>
    [VerificationCommand("--lookup-window-curves")]
    internal static int LookupWindowCurvesCommand()
    {
        var curveOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(curveOracle.Rom)),
            "Window curve oracle is NTSC J/U v1.0");
        Suite(nameof(VerifySuitPickupBeamCurveDefinitions), () => VerifySuitPickupBeamCurveDefinitions(curveOracle));
        Suite(nameof(VerifyCompiledAbsoluteTangent), () => VerifyCompiledAbsoluteTangent(curveOracle, definitionsOnly: true));
        Console.WriteLine("Window curves: 128 suit bytes and129 tangent words, direct readers and bounds match original data.");
        return 0;
    }

    /// <summary>Checks Samus running frame cadence against native timers.</summary>
    [VerificationCommand("--lookup-running-cadence-review")]
    internal static int LookupRunningCadenceReviewCommand()
    {
        var cadenceOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(cadenceOracle.Rom)),
            "Cadence oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyRunningCadence), () => VerifyRunningCadence(cadenceOracle, definitionsOnly: true));
        Console.WriteLine("Running cadence: nine logical mappings, all 90 catalog bytes, selector bounds and mutable/wrapped aliases pass.");
        return 0;
    }

    /// <summary>Checks shared fixed-point speed values across movement consumers.</summary>
    [VerificationCommand("--lookup-shared-speed-review")]
    internal static int LookupSharedSpeedReviewCommand()
    {
        var speedOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(speedOracle.Rom)),
            "Shared speed oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyCompiledLinearEnemySpeeds), () => VerifyCompiledLinearEnemySpeeds(speedOracle, definitionsOnly: true));
        Suite(nameof(VerifyCompiledQuadraticEnemySpeeds), () => VerifyCompiledQuadraticEnemySpeeds(speedOracle, definitionsOnly: true));
        Console.WriteLine("Existing speed algorithms: 517 linear pairs, 759 quadratic words in both copies, 757 displacements and bounds match original bytes.");
        return 0;
    }

    /// <summary>Checks grapple launch direction and speed calculations.</summary>
    [VerificationCommand("--lookup-grapple-launch-algorithms")]
    internal static int LookupGrappleLaunchAlgorithmsCommand()
    {
        var launchOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(launchOracle.Rom)),
            "Grapple launch oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyGrappleLaunchXSelection), () => VerifyGrappleLaunchXSelection(launchOracle));
        Suite(nameof(VerifyGrappleLaunchYSelection), () => VerifyGrappleLaunchYSelection(launchOracle));
        Suite(nameof(VerifyGrappleLaunchAngleAlgorithm), () => VerifyGrappleLaunchAngleAlgorithm(launchOracle));
        Console.WriteLine("Grapple launch: all thirty original words and each field's bounds pass.");
        return 0;
    }

    /// <summary>Checks Fireflea melt color progression and timing.</summary>
    [VerificationCommand("--lookup-fireflea-melt-algorithms")]
    internal static int LookupFirefleaMeltAlgorithmsCommand()
    {
        var effectOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(effectOracle.Rom)),
            "Fireflea/melt oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyFirefleaFx), () => VerifyFirefleaFx(includeXrayCapture: false));
        Suite(nameof(VerifyCrocomireMaskAlgorithm), () => VerifyCrocomireMaskAlgorithm(effectOracle));
        Suite(nameof(VerifyCrocomireMeltingProductionSequence), () => VerifyCrocomireMeltingProductionSequence(effectOracle));
        return 0;
    }

    /// <summary>Checks Fireflea room gunship lighting calculations.</summary>
    [VerificationCommand("--lookup-fireflea-gunship-algorithms")]
    internal static int LookupFirefleaGunshipAlgorithmsCommand()
    {
        var motionOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(motionOracle.Rom)),
            "Fireflea/gunship oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyFirefleaMovementDefinitions), () => VerifyFirefleaMovementDefinitions(motionOracle));
        Suite(nameof(VerifyGunshipMotionDefinitions), () => VerifyGunshipMotionDefinitions(motionOracle));
        return 0;
    }

    /// <summary>Checks Owtch shake offsets and frame cadence.</summary>
    [VerificationCommand("--lookup-owtch-shake-algorithms")]
    internal static int LookupOwtchShakeAlgorithmsCommand()
    {
        var timingOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(timingOracle.Rom)),
            "Owtch/shake oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyOwtchMovementDefinitions), () => VerifyOwtchMovementDefinitions(timingOracle));
        Suite(nameof(VerifyRoomShakeDefinitions), () => VerifyRoomShakeDefinitions(timingOracle));
        return 0;
    }

    /// <summary>Checks slope movement adjustments and signed rounding.</summary>
    [VerificationCommand("--lookup-slope-algorithms")]
    internal static int LookupSlopeAlgorithmsCommand()
    {
        var slopeOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(slopeOracle.Rom)),
            "slope algorithm oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyCompiledSlopeHeights), () => VerifyCompiledSlopeHeights(slopeOracle));
        Suite(nameof(VerifyCompiledSquareSlopes), () => VerifyCompiledSquareSlopes(slopeOracle));
        return 0;
    }

    /// <summary>Checks segment interpolation along Shaktool's linked body.</summary>
    [VerificationCommand("--shaktool-segment-algorithms")]
    internal static int ShaktoolSegmentAlgorithmsCommand()
    {
        var segmentOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(segmentOracle.Rom)),
            "Shaktool segment oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyShaktoolSegmentDefinitions), () => VerifyShaktoolSegmentDefinitions(segmentOracle));
        return 0;
    }

    /// <summary>Checks Work Robot initial pose and program selection.</summary>
    [VerificationCommand("--work-robot-initial-selection")]
    internal static int WorkRobotInitialSelectionCommand()
    {
        var robotOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(robotOracle.Rom)),
            "Work Robot selection oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyWorkRobotInitialSelection), () => VerifyWorkRobotInitialSelection(robotOracle));
        return 0;
    }

    /// <summary>Runs the grouped phase and transition calculation checks.</summary>
    [VerificationCommand("--lookup-phase-algorithms")]
    internal static int LookupPhaseAlgorithmsCommand()
    {
        var phaseOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(phaseOracle.Rom)),
            "lookup phase oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyCompiledBotwoonSpeeds), () => VerifyCompiledBotwoonSpeeds(phaseOracle));
        Suite(nameof(VerifyComboPowerBombCostAlgorithm), () => VerifyComboPowerBombCostAlgorithm(address => ReadPhaseAlgorithmWord(phaseOracle, address)));
        Suite(nameof(VerifyComboOriginAngleAlgorithm), () => VerifyComboOriginAngleAlgorithm(address => ReadPhaseAlgorithmWord(phaseOracle, address)));
        Suite(nameof(VerifyDraygonIntroLatencyDefinitions), () => VerifyDraygonIntroLatencyDefinitions(phaseOracle));
        return 0;
    }

    /// <summary>Runs the grouped palette derivation and interpolation checks.</summary>
    [VerificationCommand("--lookup-palette-algorithms")]
    internal static int LookupPaletteAlgorithmsCommand()
    {
        var paletteOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(paletteOracle.Rom)),
            "lookup palette oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyDraygonHealthPaletteDefinitions), () => VerifyDraygonHealthPaletteDefinitions(paletteOracle));
        Suite(nameof(VerifyBotwoonHealthPaletteDefinitions), () => VerifyBotwoonHealthPaletteDefinitions(paletteOracle));
        Suite(nameof(VerifyWorkRobotPaletteTimingDefinitions), () => VerifyWorkRobotPaletteTimingDefinitions(paletteOracle));
        return 0;
    }
}
