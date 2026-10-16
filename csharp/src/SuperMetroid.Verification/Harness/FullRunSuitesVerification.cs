using SuperMetroid.Verification.Enemies.Projectiles;
using SuperMetroid.Verification.Bosses.MotherBrain;
using static SuperMetroid.Verification.Runtime.GameplayDisplayFrameTestsVerification;
using static SuperMetroid.Verification.Samus.Movement.AerialSpreadTransitionVerification;
using static SuperMetroid.Verification.Samus.Movement.AirSpikeAlphaRadiusVerification;
using static SuperMetroid.Verification.Frontend.AndroidHostVerification;
using static SuperMetroid.Verification.Fx.AnimatedTileInstructionCodesVerification;
using static SuperMetroid.Verification.Frontend.Map.AreaMapAssetsVerification;
using static SuperMetroid.Verification.Runtime.Attract.AttractCaptureTestsVerification;
using static SuperMetroid.Verification.Runtime.Attract.AttractDemoControllerVerification;
using static SuperMetroid.Verification.Runtime.Attract.AttractDemoSceneVerification;
using static SuperMetroid.Verification.Audio.AudioVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.BabyMetroidDeathExplosionSoundVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.BabyMetroidFatalBlowShakeVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.BabyMetroidHeadTargetVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.BabyMetroidInheritedFractionsVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.BabyMetroidVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.BabyMetroidWallClearTimingVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.BabyMetroidWrongWaySpeedVerification;
using static SuperMetroid.Verification.Rooms.BackgroundMosaicVerification;
using static SuperMetroid.Verification.Rooms.BackgroundSamplerVerification;
using static SuperMetroid.Verification.Samus.Projectiles.BeamCallbackTablesVerification;
using static SuperMetroid.Verification.Samus.Projectiles.BeamImpactSoundVerification;
using static SuperMetroid.Verification.Samus.Projectiles.BeamSpeedRowsVerification;
using static SuperMetroid.Verification.Hardware.Bgr555DomainVerification;
using static SuperMetroid.Verification.Rooms.LevelData.BombBlockProgramsVerification;
using static SuperMetroid.Verification.Samus.Projectiles.BombChargeRejectionVerification;
using static SuperMetroid.Verification.Samus.Movement.BombJumpRunMomentumVerification;
using static SuperMetroid.Verification.Samus.Movement.BombJumpWallVerification;
using static SuperMetroid.Verification.Bosses.Torizo.BombTorizoHandVerification;
using static SuperMetroid.Verification.Samus.Abilities.BoostFloorAuditVerification;
using static SuperMetroid.Verification.Samus.Abilities.BoostFloorScrollVerification;
using static SuperMetroid.Verification.Bosses.BossMapMarkersVerification;
using static SuperMetroid.Verification.Bosses.BossResetOnLoadVerification;
using static SuperMetroid.Verification.Bosses.Botwoon.BotwoonPlmIdentityVerification;
using static SuperMetroid.Verification.Bosses.Botwoon.BotwoonPositionHistoryVerification;
using static SuperMetroid.Verification.Bosses.Botwoon.BotwoonWallPlmsVerification;
using static SuperMetroid.Verification.Bosses.Botwoon.BotwoonWallVisualsVerification;
using static SuperMetroid.Verification.Samus.Movement.CeilingWrapRoomVerification;
using static SuperMetroid.Verification.Samus.Movement.CeilingWrapVerification;
using static SuperMetroid.Verification.Frontend.CeresCinematics.CeresDestructionActorDefinitionsVerification;
using static SuperMetroid.Verification.Frontend.CeresCinematics.CeresDestructionVerification;
using static SuperMetroid.Verification.Areas.Ceres.CeresElevatorArrivalVerification;
using static SuperMetroid.Verification.Areas.Ceres.CeresEscapeHandoffVerification;
using static SuperMetroid.Verification.Areas.Ceres.CeresEscapeVramTransferDefinitionsVerification;
using static SuperMetroid.Verification.Frontend.CeresCinematics.CeresExplosionDefinitionsVerification;
using static SuperMetroid.Verification.Frontend.CeresCinematics.CeresFlightActorDefinitionsVerification;
using static SuperMetroid.Verification.Areas.Ceres.CeresHazeVerification;
using static SuperMetroid.Verification.Bosses.Ridley.CeresRidleyWallVerification;
using static SuperMetroid.Verification.Areas.Ceres.CeresSteamCollisionDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Projectiles.ChainsawFiringVerification;
using static SuperMetroid.Verification.Frontend.Intro.CinematicCrystalFlashVerification;
using static SuperMetroid.Verification.Frontend.Intro.CinematicSnapshotTestsVerification;
using static SuperMetroid.Verification.Frontend.Intro.CinematicTextGlowVerification;
using static SuperMetroid.Verification.Rooms.Plms.CollectibleMessageTimingVerification;
using static SuperMetroid.Verification.Rooms.Plms.CollectibleVisualsVerification;
using static SuperMetroid.Verification.Rooms.Plms.CollectiblesVerification;
using static SuperMetroid.Verification.Hardware.ColorWindowSnapshotTestsVerification;
using static SuperMetroid.Verification.Samus.Movement.CompactWalkOffCollisionVerification;
using static SuperMetroid.Verification.Hardware.CompressionVerification;
using static SuperMetroid.Verification.Rooms.Plms.ContactCrumbleProgramsVerification;
using static SuperMetroid.Verification.Audio.ContactDamageSoundVerification;
using static SuperMetroid.Verification.Frontend.Recording.ControllerInputRecordingVerification;
using static SuperMetroid.Verification.Hardware.CpuBusBoundaryVerification;
using static SuperMetroid.Verification.Hardware.CpuOperandOpenBusVerification;
using static SuperMetroid.Verification.Samus.Movement.CrampedAerialLandingVerification;
using static SuperMetroid.Verification.Rooms.Plms.CrateriaMainstreetEscapePassageVerification;
using static SuperMetroid.Verification.Bosses.Crocomire.CrocomireArenaPlmsVerification;
using static SuperMetroid.Verification.Bosses.Crocomire.CrocomireArenaVisualsVerification;
using static SuperMetroid.Verification.Bosses.Crocomire.CrocomireBodyCollisionDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Crocomire.CrocomireMeltingDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Crocomire.CrocomireTongueCollisionDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Abilities.CrystalFlashCleanupVerification;
using static SuperMetroid.Verification.Samus.Movement.DamageBoostJumpInitializerVerification;
using static SuperMetroid.Verification.Runtime.DebuggerStateLayoutVerification;
using static SuperMetroid.Verification.Rooms.Doors.DoorAlignmentVerification;
using static SuperMetroid.Verification.Rooms.Doors.DoorAnimatedTilesVerification;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.Doors.DoorEntryEnemySoundVerification;
using static SuperMetroid.Verification.Rooms.Doors.DoorEntryRoomFxSoundVerification;
using static SuperMetroid.Verification.Rooms.Doors.DoorEntryShotBindingVerification;
using static SuperMetroid.Verification.Bosses.Draygon.DraygonGoopTouchOrderVerification;
using static SuperMetroid.Verification.Bosses.Draygon.DraygonGrabVerification;
using static SuperMetroid.Verification.Frontend.Options.GameOptionsStartGameVerification;
using static SuperMetroid.Verification.Frontend.FileSelect.FileSelectMapLoadTimingVerification;
using static SuperMetroid.Verification.Rooms.Doors.DoorMusicTimingVerification;
using static SuperMetroid.Verification.Rooms.Doors.DoorOrientationDomainVerification;
using static SuperMetroid.Verification.Rooms.Doors.DoorSoundsDuringPowerBombVerification;
using static SuperMetroid.Verification.Rooms.Doors.DoorWaitsForMessageBoxVerification;
using static SuperMetroid.Verification.Rooms.Plms.DownwardGatePlmsVerification;
using static SuperMetroid.Verification.Samus.Abilities.DrainedSparkCrashVerification;
using static SuperMetroid.Verification.Bosses.Draygon.DraygonCannonPlmsVerification;
using static SuperMetroid.Verification.Bosses.Draygon.DraygonEscapeDragVerification;
using static SuperMetroid.Verification.Bosses.Draygon.DraygonEyeEffectsVerification;
using static SuperMetroid.Verification.Bosses.Draygon.DraygonIntroDanceDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Draygon.DraygonPrebattleXrayVerification;
using static SuperMetroid.Verification.Bosses.Draygon.DraygonScrollingSpeedCapVerification;
using static SuperMetroid.Verification.Bosses.Draygon.DraygonTilemapProductionVerification;
using static SuperMetroid.Verification.Bosses.Draygon.DraygonTurretCadenceVerification;
using static SuperMetroid.Verification.Audio.DspRegisterBoundaryVerification;
using static SuperMetroid.Verification.Enemies.Elevator.ElevatorStandUpContactVerification;
using static SuperMetroid.Verification.Areas.Maridia.ElevatubeScrollingVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EmptyExtendedFrameShotsVerification;
using static SuperMetroid.Verification.Frontend.Ending.EndingSetupNmiWaitsVerification;
using static SuperMetroid.Verification.Frontend.Ending.EndingSnapshotTestsVerification;
using static SuperMetroid.Verification.Enemies.EnemyAngleDivisionVerification;
using static SuperMetroid.Verification.Enemies.EnemyDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.EnemyDropsVerification;
using static SuperMetroid.Verification.Enemies.EnemyInstructionCodePointersVerification;
using static SuperMetroid.Verification.Enemies.Projectiles.EnemyProjectileCodePointersVerification;
using static SuperMetroid.Verification.Enemies.EnemyRomTablePointersVerification;
using static SuperMetroid.Verification.Enemies.EnemyRoomListsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkVerification;
using static SuperMetroid.Verification.Enemies.EnemyTrigonometryVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyVisualSelectorInventoryVerification;
using static SuperMetroid.Verification.Areas.Escape.EscapeGateVisualsVerification;
using static SuperMetroid.Verification.Areas.Escape.EscapeTimerFloorVerification;
using static SuperMetroid.Verification.Enemies.Evir.EvirInitTimerVerification;
using static SuperMetroid.Verification.Map.ExploredMapPackingDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.Doors.EyeDoorPlmsVerification;
using static SuperMetroid.Verification.Rooms.Doors.EyeDoorSweatSpanUnderflowVerification;
using static SuperMetroid.Verification.Hardware.EyeWindowSnapshotTestsVerification;
using static SuperMetroid.Verification.Samus.Movement.FallingSpeedRecurrenceVerification;
using static SuperMetroid.Verification.Frontend.FileSelect.FileMapSnapshotTestsVerification;
using static SuperMetroid.Verification.Frontend.FileSelect.FileMenuSnapshotTestsVerification;
using static SuperMetroid.Verification.Frontend.FileSelect.FileSelectMapWindowVerification;
using static SuperMetroid.Verification.Enemies.Fireflea.FirefleaDoubleDeathVerification;
using static SuperMetroid.Verification.Enemies.Fireflea.FirefleaFxVerification;
using static SuperMetroid.Verification.Frontend.FrontendCompletionVerification;
using static SuperMetroid.Verification.Frontend.FrontendRenderCaptureTestsVerification;
using static SuperMetroid.Verification.Frontend.FrontendVerification;
using static SuperMetroid.Verification.Rooms.Plms.FrozenGateGlitchVerification;
using static SuperMetroid.Verification.Enemies.Projectiles.FrozenTimeEnemyProjectilesVerification;
using static SuperMetroid.Verification.Rooms.Plms.GModeGateGlitchVerification;
using static SuperMetroid.Verification.Frontend.GameConfigurationVerification;
using static SuperMetroid.Verification.Frontend.GameOver.GameOverRomDataVerification;
using static SuperMetroid.Verification.Saves.GameSaveJsonVerification;
using static SuperMetroid.Verification.Progression.GameTimeVerification;
using static SuperMetroid.Verification.Frontend.GamepadInputVerification;
using static SuperMetroid.Verification.Runtime.GameplayCaptureIntegrationTestsVerification;
using static SuperMetroid.Verification.Runtime.GameplaySnapshotTestsVerification;
using static SuperMetroid.Verification.Rooms.Plms.GateBeamCollisionVerification;
using static SuperMetroid.Verification.Rooms.Plms.GateJumpTraceVerification;
using static SuperMetroid.Verification.Enemies.SpacePirates.GoldNinjaDeathDropsVerification;
using static SuperMetroid.Verification.Bosses.Torizo.GoldenTorizoAttackChoiceVerification;
using static SuperMetroid.Verification.Samus.Grapple.GrappleDemoTrajectoryVerification;
using static SuperMetroid.Verification.Samus.Grapple.GrappleDoorsVerification;
using static SuperMetroid.Verification.Samus.Grapple.GrappleEnemyDeathVerification;
using static SuperMetroid.Verification.Samus.Grapple.GrappleGatesVerification;
using static SuperMetroid.Verification.Samus.Grapple.GrapplePoseRefireVerification;
using static SuperMetroid.Verification.Samus.Grapple.GrappleSoundsVerification;
using static SuperMetroid.Verification.Samus.Grapple.GrappleSpinVerification;
using static SuperMetroid.Verification.Rooms.Plms.GreenHillGateGlitchVerification;
using static SuperMetroid.Verification.Samus.Movement.GroundedBombSpreadVerification;
using static SuperMetroid.Verification.Samus.Movement.GroundedSpreadTransitionVerification;
using static SuperMetroid.Verification.Enemies.Gunship.GunshipLandingVerification;
using static SuperMetroid.Verification.Hardware.HardwareVerification;
using static SuperMetroid.Verification.Hardware.HardwareWindowsVerification;
using static SuperMetroid.Verification.Samus.Abilities.HealthWarningVerification;
using static SuperMetroid.Verification.Runtime.HeroShotRuntimeVerification;
using static SuperMetroid.Verification.Runtime.HeroShotsVerification;
using static SuperMetroid.Verification.Audio.HostIndependentAudioAcknowledgementsVerification;
using static SuperMetroid.Verification.Rooms.Scrolling.ImplicitScrollResidueVerification;
using static SuperMetroid.Verification.Frontend.IniEditingVerification;
using static SuperMetroid.Verification.Frontend.Intro.IntroBabyActorDefinitionsVerification;
using static SuperMetroid.Verification.Frontend.Intro.IntroCinematicArtworkVerification;
using static SuperMetroid.Verification.Frontend.Intro.IntroCinematicRomDataVerification;
using static SuperMetroid.Verification.Frontend.Intro.IntroEggEffectDefinitionsVerification;
using static SuperMetroid.Verification.Frontend.Intro.IntroMotherBrainDefinitionsVerification;
using static SuperMetroid.Verification.Frontend.Intro.IntroPoseHistoryVerification;
using static SuperMetroid.Verification.Frontend.Intro.IntroRinkaDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Projectiles.InvalidBeamGraphicsVerification;
using static SuperMetroid.Verification.Samus.Projectiles.InvalidBeamSelectionVerification;
using static SuperMetroid.Verification.Samus.Projectiles.ItemCancelClearsChargeVerification;
using static SuperMetroid.Verification.Samus.Movement.JumpNoXMovementVerification;
using static SuperMetroid.Verification.Enemies.KiHunters.KiHunterSpitAudioVerification;
using static SuperMetroid.Verification.Samus.Movement.KnockbackHorizontalStopVerification;
using static SuperMetroid.Verification.Samus.Movement.KnockbackShinesparkLaunchVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidArmSamusContactVerification;
using static SuperMetroid.Verification.Enemies.Shaktool.ShaktoolSamusContactVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidRoomPlmsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidRoomVisualsVerification;
using static SuperMetroid.Verification.Rooms.Backgrounds.LibraryBackgroundVerification;
using static SuperMetroid.Verification.Rooms.Plms.LoadStationDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Magdollite.MagdolliteApexThresholdVerification;
using static SuperMetroid.Verification.Harness.MagicNumberAuditVerification;
using static SuperMetroid.Verification.Runtime.MainGameLoopCarryVerification;
using static SuperMetroid.Verification.Frontend.BootDemoSetCountVerification;
using static SuperMetroid.Verification.Enemies.MamaTurtles.MamaTurtleEnemyDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.MamaTurtles.MamaTurtleShellContourVerification;
using static SuperMetroid.Verification.Audio.ManagedAudioVerification;
using static SuperMetroid.Verification.Frontend.Map.MapPresentationVerification;
using static SuperMetroid.Verification.Areas.Maridia.MaridiaElevatubePlmVerification;
using static SuperMetroid.Verification.Areas.Maridia.MaridiaElevatubeVisualsVerification;
using static SuperMetroid.Verification.Enemies.MaridiaSnails.MaridiaLargeSnailCollisionDefinitionsVerification;
using static SuperMetroid.Verification.Hud.MessageSnapshotTestsVerification;
using static SuperMetroid.Verification.Enemies.Metroids.MetroidDeathDropsVerification;
using static SuperMetroid.Verification.Samus.Projectiles.MissileEdgeVerification;
using static SuperMetroid.Verification.Enemies.Metroids.MochtroidBotwoonClipVerification;
using static SuperMetroid.Verification.Hardware.Mode7GameplaySnapshotTestsVerification;
using static SuperMetroid.Verification.Samus.Movement.MoonwalkRanIntoWallVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainBeamWindowVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainBodyHitboxesVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainContactHitboxesVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainDeathHandoffVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainFakeDeathLockVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainFakeDeathPlmsVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainFakeDeathVisualsVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainGlassSuperMissileVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainGlitchMechanicsVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainHeadHitboxVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainInheritedExplosionIndexVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainMissileWalkResetVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainRainbowVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainRaiseCounterVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainRingBabyHealthVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainRinkaDoorSpawnVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainSmallPurpleBreathVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainTubeHdmaDeletionVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainTubeTimingVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainWalkBackwardsPoseVerification;
using static SuperMetroid.Verification.Samus.Projectiles.MurderBeamVerification;
using static SuperMetroid.Verification.Rooms.Plms.NoobTubePlmVerification;
using static SuperMetroid.Verification.Rooms.Backgrounds.OceanSkyVerification;
using static SuperMetroid.Verification.Rooms.Plms.OldTourianEscapeShaftWallVerification;
using static SuperMetroid.Verification.Harness.OperationContractsVerification;
using static SuperMetroid.Verification.Harness.NullContractsVerification;
using static SuperMetroid.Verification.Harness.SpanQueriesVerification;
using static SuperMetroid.Verification.Fx.PaletteFx.PaletteFxInstructionCodesVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseMapAreaLabelsVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseMapArrowsVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseMapPositionVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseMenuVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseMomentumVerification;
using static SuperMetroid.Verification.Frontend.Pause.PausePaletteSoundVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseReserveArrowRebindVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseReserveArrowVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseReserveHudVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseReserveManualVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseReserveTanksVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseSnapshotTestsVerification;
using static SuperMetroid.Verification.Bosses.Phantoon.PhantoonPositionVerification;
using static SuperMetroid.Verification.Bosses.Phantoon.PhantoonWaveVerification;
using static SuperMetroid.Verification.Rooms.Plms.PlmDrawCloneVerification;
using static SuperMetroid.Verification.Rooms.Plms.PlmsAndPostureVerification;
using static SuperMetroid.Verification.Enemies.Powamp.PowampDeathSequenceVerification;
using static SuperMetroid.Verification.Samus.Projectiles.PowerBombBoundaryVerification;
using static SuperMetroid.Verification.Samus.Projectiles.PowerBombDeathRadiusVerification;
using static SuperMetroid.Verification.Samus.Palettes.PowerBombFixedColorsVerification;
using static SuperMetroid.Verification.Samus.Projectiles.PowerBombFuseVerification;
using static SuperMetroid.Verification.Samus.Projectiles.PowerBombRuntimeIntegrationVerification;
using static SuperMetroid.Verification.Hardware.PpuLayoutBoundaryVerification;
using static SuperMetroid.Verification.Hardware.PpuMemorySnapshotTestsVerification;
using static SuperMetroid.Verification.Samus.Projectiles.ProjectileContactPhaseVerification;
using static SuperMetroid.Verification.Samus.Projectiles.ProjectileCooldownsVerification;
using static SuperMetroid.Verification.Samus.Projectiles.ProjectileInheritanceProbeVerification;
using static SuperMetroid.Verification.Enemies.Puromi.PuromiArcPositionVerification;
using static SuperMetroid.Verification.Fx.QuicksandVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.RainbowReleaseKnockbackVerification;
using static SuperMetroid.Verification.Samus.Projectiles.RedTowerHeroVerification;
using static SuperMetroid.Verification.Rooms.Plms.RefillStationLockVerification;
using static SuperMetroid.Verification.Bosses.Draygon.ReleasedSamusFallsOffDraygonVerification;
using static SuperMetroid.Verification.Rendering.RenderFrameHandoffTestsVerification;
using static SuperMetroid.Verification.Rendering.RenderPacketCodecTestsVerification;
using static SuperMetroid.Verification.Rendering.RenderPresentationGateTestsVerification;
using static SuperMetroid.Verification.Rendering.RenderingAndRoomsVerification;
using static SuperMetroid.Verification.Harness.ReplayTraceCompactionVerification;
using static SuperMetroid.Verification.Harness.ReplaySnapshotVerification;
using static SuperMetroid.Verification.Hud.ReserveAutoFrontendVerification;
using static SuperMetroid.Verification.Hud.ReserveModeVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.ReserveRefillRainbowBeamVerification;
using static SuperMetroid.Verification.Enemies.RespawnedEnemyContactVerification;
using static SuperMetroid.Verification.Bosses.Ridley.RidleyCollisionDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Ridley.RidleyExplosionDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Ridley.RidleyShotHealthStageVerification;
using static SuperMetroid.Verification.Rooms.Plms.RightFacingGateGlitchVerification;
using static SuperMetroid.Verification.Rooms.RoomAssetRomDataVerification;
using static SuperMetroid.Verification.Rooms.RoomCallbackDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.RoomDefinitionIntegrationVerification;
using static SuperMetroid.Verification.Rooms.RoomEnemiesVerification;
using static SuperMetroid.Verification.Enemies.RoomEnemyLoadContractsVerification;
using static SuperMetroid.Verification.Fx.RoomFxRomDataVerification;
using static SuperMetroid.Verification.Fx.RoomFxSnapshotTestsVerification;
using static SuperMetroid.Verification.Rooms.RoomHeaderDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.RoomMainCodeCatalogVerification;
using static SuperMetroid.Verification.Rooms.Plms.RoomPlmHeaderCatalogVerification;
using static SuperMetroid.Verification.Rooms.Plms.RoomPlmInstructionListCatalogVerification;
using static SuperMetroid.Verification.Rooms.Plms.RoomPlmPopulationDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.Plms.RoomPlmPopulationVerification;
using static SuperMetroid.Verification.Rooms.Scrolling.RoomScrollDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.RoomSetupCodeCatalogVerification;
using static SuperMetroid.Verification.Rooms.RoomStateDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.RoomStateSelectionDefinitionsVerification;
using static SuperMetroid.Verification.Runtime.RuntimeCartridgeBoundaryVerification;
using static SuperMetroid.Verification.Samus.Movement.SamusAerialAndLiquidsVerification;
using static SuperMetroid.Verification.Samus.SamusAimVerification;
using static SuperMetroid.Verification.Samus.SamusDamageAndGrabVerification;
using static SuperMetroid.Verification.Rooms.Plms.SamusEaterVisualsVerification;
using static SuperMetroid.Verification.Samus.Grapple.SamusGrappleRomDataVerification;
using static SuperMetroid.Verification.Samus.Grapple.SamusGrappleVerification;
using static SuperMetroid.Verification.Samus.Hud.SamusHudSelectionVerification;
using static SuperMetroid.Verification.Samus.Palettes.SamusHurtColorsVerification;
using static SuperMetroid.Verification.Samus.Palettes.SamusHyperBeamColorsVerification;
using static SuperMetroid.Verification.Samus.Abilities.SamusMorphBallVerification;
using static SuperMetroid.Verification.Samus.SamusMotionCoreVerification;
using static SuperMetroid.Verification.Samus.Movement.SamusMovementRomDataVerification;
using static SuperMetroid.Verification.Samus.Palettes.SamusPaletteRomDataVerification;
using static SuperMetroid.Verification.Samus.Poses.SamusPoseHistoryVerification;
using static SuperMetroid.Verification.Samus.Projectiles.SamusProjectileRomDataVerification;
using static SuperMetroid.Verification.Samus.Projectiles.SamusProjectilesVerification;
using static SuperMetroid.Verification.Samus.Drawing.SamusRenderingRomDataVerification;
using static SuperMetroid.Verification.Samus.Artwork.SamusRenderingVerification;
using static SuperMetroid.Verification.Samus.Abilities.SamusShinesparkVerification;
using static SuperMetroid.Verification.Samus.Abilities.SamusSpecialSequenceRomDataVerification;
using static SuperMetroid.Verification.Samus.Abilities.SamusSpecialStatesVerification;
using static SuperMetroid.Verification.Samus.Movement.SamusTerrainMovementVerification;
using static SuperMetroid.Verification.Samus.SamusTurnsAndKnockbackVerification;
using static SuperMetroid.Verification.Samus.Palettes.SamusVisorColorsVerification;
using static SuperMetroid.Verification.Samus.Abilities.SamusXrayRomDataVerification;
using static SuperMetroid.Verification.Fx.SandAnimatedTilesVerification;
using static SuperMetroid.Verification.Rooms.Plms.SaveConfirmationCadenceVerification;
using static SuperMetroid.Verification.Saves.SaveRamLayoutVerification;
using static SuperMetroid.Verification.Rooms.Scrolling.ScrollPlmsVerification;
using static SuperMetroid.Verification.Samus.Movement.ShallowWaterJumpVerification;
using static SuperMetroid.Verification.Samus.Abilities.ShinesparkEnemyStopVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.ShitroidGradualAccelerationVerification;
using static SuperMetroid.Verification.Rooms.Plms.ShotBlockPlmProgramsVerification;
using static SuperMetroid.Verification.Enemies.Shutters.ShutterRidingVerification;
using static SuperMetroid.Verification.Enemies.Shutters.ShutterScrewContactVerification;
using static SuperMetroid.Verification.Enemies.SpacePirates.SpacePirateCollisionDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Projectiles.SpacetimeBeamVerification;
using static SuperMetroid.Verification.Enemies.Sparks.SparkCrashAlignmentVerification;
using static SuperMetroid.Verification.Spatial.SpatialValuesVerification;
using static SuperMetroid.Verification.Audio.SpcMemoryBoundaryVerification;
using static SuperMetroid.Verification.Audio.SpcSoundLibrary2PointersVerification;
using static SuperMetroid.Verification.Samus.Abilities.SpeedBoosterCollisionBlocksVerification;
using static SuperMetroid.Verification.Samus.Abilities.SpeedBoosterVisualsVerification;
using static SuperMetroid.Verification.Samus.Abilities.SpeedPaletteOverrunVerification;
using static SuperMetroid.Verification.Enemies.Spike.SpikeDamageBoostVerification;
using static SuperMetroid.Verification.Enemies.Spike.SpikeShinesparkSuitVerification;
using static SuperMetroid.Verification.Bosses.SporeSpawn.SporeSpawnCeilingPlmsVerification;
using static SuperMetroid.Verification.Bosses.SporeSpawn.SporeSpawnCeilingVisualsVerification;
using static SuperMetroid.Verification.Samus.Movement.SpringBallBounceVerification;
using static SuperMetroid.Verification.Samus.Movement.SpringBallFallingFallbackVerification;
using static SuperMetroid.Verification.Samus.Movement.SquareSlopeBeamCollisionVerification;
using static SuperMetroid.Verification.Rooms.Plms.StationAnimationProgramDefinitionsVerification;
using static SuperMetroid.Verification.Harness.StrictFailuresVerification;
using static SuperMetroid.Verification.Samus.Abilities.SuitPickupPeriodicDamageVerification;
using static SuperMetroid.Verification.Samus.Projectiles.SuperMissileDeathAnimationVerification;
using static SuperMetroid.Verification.Samus.Projectiles.SuperMissileEnemyHitQuakeVerification;
using static SuperMetroid.Verification.Frontend.Title.TitleSequenceRomDataVerification;
using static SuperMetroid.Verification.Frontend.Title.TitleSnapshotTestsVerification;
using static SuperMetroid.Verification.Areas.Tourian.TourianAccessPlmProgramsVerification;
using static SuperMetroid.Verification.Areas.Tourian.TourianAccessVisualsVerification;
using static SuperMetroid.Verification.Areas.Tourian.TourianStatueDescentRoundingVerification;
using static SuperMetroid.Verification.Areas.Tourian.TourianStatueXrayFreezeVerification;
using static SuperMetroid.Verification.Fx.TreadmillPhysicsVerification;
using static SuperMetroid.Verification.Hardware.TypedWordsVerification;
using static SuperMetroid.Verification.Frontend.Pause.UnpauseElevatorFlagsVerification;
using static SuperMetroid.Verification.Frontend.Pause.UnpauseReserveBlackoutVerification;
using static SuperMetroid.Verification.Frontend.Pause.UnpauseStationLockReleaseVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseRemappedShoulderVerification;
using static SuperMetroid.Verification.Enemies.VerticalOffScreenDeletionVerification;
using static SuperMetroid.Verification.Rooms.Scrolling.ViewportParityVerification;
using static SuperMetroid.Verification.Samus.Movement.WallJumpDustVerification;
using static SuperMetroid.Verification.Samus.Movement.WallJumpSpinExitVerification;
using static SuperMetroid.Verification.Rooms.LevelData.WallProbeBombBlockVerification;
using static SuperMetroid.Verification.Rendering.WindowPixelsVerification;
using static SuperMetroid.Verification.Hardware.WordReadsVerification;
using static SuperMetroid.Verification.Samus.Projectiles.WrapShotEnemiesVerification;
using static SuperMetroid.Verification.Samus.Projectiles.WrapShotRoomsVerification;
using static SuperMetroid.Verification.Samus.Projectiles.WrapShotWidthsVerification;
using static SuperMetroid.Verification.Samus.Projectiles.WrapShotsVerification;
using static SuperMetroid.Verification.Samus.Abilities.XrayControlsVerification;
using static SuperMetroid.Verification.Samus.Abilities.XrayExtensionsVerification;
using static SuperMetroid.Verification.Samus.Abilities.XrayGameStateVerification;
using static SuperMetroid.Verification.Samus.Abilities.SpeedBoosterStageFiveResidueVerification;
using static SuperMetroid.Verification.Samus.Abilities.XrayInputVerification;
using static SuperMetroid.Verification.Samus.Abilities.XrayOverlaysVerification;
using static SuperMetroid.Verification.Samus.Abilities.XrayRevealVerification;
using static SuperMetroid.Verification.Samus.Abilities.XrayRoomDisplayVerification;
using static SuperMetroid.Verification.Samus.Abilities.XraySetupStageDomainVerification;
using static SuperMetroid.Verification.Samus.Abilities.XraySetupVerification;
using static SuperMetroid.Verification.Samus.Abilities.XrayTilemapVerification;
using static SuperMetroid.Verification.Samus.Abilities.XrayWindowGeometryVerification;
using static SuperMetroid.Verification.Enemies.YappingMaws.YappingMawGrappleReleaseVerification;
using static SuperMetroid.Verification.Areas.Escape.ZebesEscapeFadeVerification;
using static SuperMetroid.Verification.Enemies.Crawlers.ZeroDistanceJumpContactVerification;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.VerificationOracles;

namespace SuperMetroid.Verification.Harness;
/// <summary>Registers the portable render-contract checks and the independent suites of the full verifier run.</summary>
internal static class FullRunSuitesVerification
{
    /// <summary>One outermost suite of the full no-argument run.</summary>
    internal sealed record RegisteredSuite(string Name, Action Body);

    /// <summary>
    /// The portable render contract (<c>--render-contract</c>): every snapshot, codec, publication,
    /// scene-capture and software parity check. It deliberately excludes unrelated gameplay audits,
    /// and never loads the Windows desktop or a graphics backend.
    /// </summary>
    internal static RegisteredSuite[] RenderContractSuites() =>
    [
        new(nameof(VerifyPlmDrawClone), () => VerifyPlmDrawClone()),
        new(nameof(VerifyGameConfigurationIni), () => VerifyGameConfigurationIni()),
        new(nameof(VerifyViewportTileRowParity), () => VerifyViewportTileRowParity()),
        new(nameof(VerifyPpuMemorySnapshotOwnership), () => VerifyPpuMemorySnapshotOwnership()),
        new(nameof(VerifyTitleRenderSnapshots), () => VerifyTitleRenderSnapshots()),
        new(nameof(VerifyPauseRenderSnapshots), () => VerifyPauseRenderSnapshots()),
        new(nameof(VerifyPauseBossMarkers), () => VerifyPauseBossMarkers()),
        new(nameof(VerifyFileMenuRenderSnapshots), () => VerifyFileMenuRenderSnapshots()),
        new(nameof(VerifyRenderFrameHandoff), () => VerifyRenderFrameHandoff()),
        new(nameof(VerifyRenderPresentationGate), () => VerifyRenderPresentationGate()),
        new(nameof(VerifyRenderPacketCodec), () => VerifyRenderPacketCodec()),
        new(nameof(VerifyFrontendRenderCapture), () => VerifyFrontendRenderCapture()),
        new(nameof(VerifyCinematicRenderSnapshots), () => VerifyCinematicRenderSnapshots()),
        new(nameof(VerifyGameplaySnapshots), () => VerifyGameplaySnapshots()),
        new(nameof(VerifyWindowPixels), () => VerifyWindowPixels()),
        new(nameof(VerifyColorWindowSnapshots), () => VerifyColorWindowSnapshots()),
        new(nameof(VerifyMessageSnapshots), () => VerifyMessageSnapshots()),
        new(nameof(VerifyEyeWindowSnapshots), () => VerifyEyeWindowSnapshots()),
        new(nameof(VerifyRoomFxSnapshots), () => VerifyRoomFxSnapshots()),
        new(nameof(VerifyGameplayCaptureIntegration), () => VerifyGameplayCaptureIntegration()),
        new(nameof(VerifyGameplayDisplayFrame), () => VerifyGameplayDisplayFrame()),
        new(nameof(VerifyMode7GameplaySnapshots), () => VerifyMode7GameplaySnapshots()),
        new(nameof(VerifyEndingRenderSnapshots), () => VerifyEndingRenderSnapshots()),
        new(nameof(VerifyFileMapSnapshots), () => VerifyFileMapSnapshots()),
        new(nameof(VerifyAttractCapture), () => VerifyAttractCapture()),
    ];

    /// <summary>
    /// Every outermost suite of the full no-argument run, each exactly once. Suites are independent:
    /// the parallel runner executes them in any order across worker processes.
    /// </summary>
    internal static RegisteredSuite[] FullRunSuites() =>
    [
        new(nameof(VerifyRuntimeAddressSpaceHasNoCartridgeApi), () => VerifyRuntimeAddressSpaceHasNoCartridgeApi()),
        new(nameof(VerifyDebuggerStateRejectsStaleLayouts), () => VerifyDebuggerStateRejectsStaleLayouts()),
        new(nameof(VerifyCpuOperandOpenBus), () => VerifyCpuOperandOpenBus()),
        new(nameof(VerifyCpuBusBoundary), () => VerifyCpuBusBoundary()),
        new(nameof(VerifyCanonicalWordReads), () => VerifyCanonicalWordReads()),
        new(nameof(VerifyPpuLayoutBoundary), () => VerifyPpuLayoutBoundary()),
        new(nameof(VerifyDspRegisterBoundary), () => VerifyDspRegisterBoundary()),
        new(nameof(VerifySpcMemoryBoundary), () => VerifySpcMemoryBoundary()),
        new(nameof(VerifyRoomEnemyLoadContracts), () => VerifyRoomEnemyLoadContracts()),
        new(nameof(VerifyOperationContracts), () => VerifyOperationContracts()),
        new(nameof(VerifyNullContracts), () => VerifyNullContracts()),
        new(nameof(VerifySpanQueries), () => VerifySpanQueries()),
        new(nameof(VerifyAndroidHostPolicies), () => VerifyAndroidHostPolicies()),
        new(nameof(VerifyBoostFloorScroll), () => VerifyBoostFloorScroll()),
        new(nameof(VerifyXrayControls), () => VerifyXrayControls()),
        new(nameof(VerifyIntroPoseHistory), () => VerifyIntroPoseHistory()),
        new(nameof(VerifyCinematicTextGlow), () => VerifyCinematicTextGlow()),
        new(nameof(VerifyProjectileContactPhase), () => VerifyProjectileContactPhase(verifyPhase: true)),
        new(nameof(VerifyProjectileRuntimePhase), () => VerifyProjectileRuntimePhase()),
        new(nameof(VerifyMorphedSpikeRelease), () => VerifyMorphedSpikeRelease()),
        new(nameof(VerifySpikeShinesparkSuit), () => VerifySpikeShinesparkSuit()),
        new(nameof(VerifyReserveMode), () => VerifyReserveMode("csharp/test-fixtures/movement-release/reserve-mode-433.csv")),
        new(nameof(VerifyCinematicCrystalFlash), () => VerifyCinematicCrystalFlash("csharp/test-fixtures/movement-release/cinematic-flash-432.csv")),
        new(nameof(VerifyElevatubeScrolling), () => VerifyElevatubeScrolling()),
        new(nameof(VerifyIniEditing), () => VerifyIniEditing()),
        new(nameof(VerifyBackgroundSampler), () => VerifyBackgroundSampler()),
        .. RenderContractSuites(),
        new(nameof(VerifyRandomNumberGeneratorExhaustively), () => VerifyRandomNumberGeneratorExhaustively()),
        new(nameof(SaveLoadRandomAudit), () => SaveLoadRandomAudit.Run(Path.GetFullPath("Super Metroid.smc"), InstalledFixtures.BindGame)),
        new(nameof(VerifySandAnimatedTiles), () => VerifySandAnimatedTiles()),
        new(nameof(VerifyQuicksand), () => VerifyQuicksand()),
        new(nameof(VerifyTreadmillPhysics), () => VerifyTreadmillPhysics()),
        new(nameof(VerifyPhantoonPosition), () => VerifyPhantoonPosition()),
        new(nameof(VerifyPausePaletteSound), () => VerifyPausePaletteSound()),
        new(nameof(VerifyKnownRandomSequence), () => VerifyKnownRandomSequence()),
        new(nameof(VerifyTimedHeldInputTimeline), () => VerifyTimedHeldInputTimeline()),
        new(nameof(VerifyEventBitfield), () => VerifyEventBitfield()),
        new(nameof(VerifyBossBitfield), () => VerifyBossBitfield()),
        new(nameof(VerifyMultiplicationExhaustively), () => VerifyMultiplicationExhaustively()),
        new(nameof(VerifySmCompressionFormat), () => VerifySmCompressionFormat()),
        new(nameof(VerifyVramWriteQueue), () => VerifyVramWriteQueue()),
        new(nameof(VerifyEscapeTimerBcd), () => VerifyEscapeTimerBcd()),
        new(nameof(VerifyEscapeTimerStateMachine), () => VerifyEscapeTimerStateMachine()),
        new(nameof(VerifyEscapeTimerFloor), () => VerifyEscapeTimerFloor()),
        new(nameof(VerifyControllerInputLatch), () => VerifyControllerInputLatch()),
        new(nameof(VerifyGameOptionsRomDataCatalog), () => VerifyGameOptionsRomDataCatalog()),
        new(nameof(VerifyControllerBindingsAndOptionsSubmenus), () => VerifyControllerBindingsAndOptionsSubmenus()),
        new(nameof(VerifyGameOverRomData), () => VerifyGameOverRomData()),
        new(nameof(VerifyTitleSequenceRomData), () => VerifyTitleSequenceRomData()),
        new(nameof(VerifyStrictFailureBoundaries), () => VerifyStrictFailureBoundaries()),
        new(nameof(VerifyReserveAutoRecovery), () => VerifyReserveAutoRecovery()),
        new(nameof(VerifyHealthWarning), () => VerifyHealthWarning()),
        new(nameof(VerifyReserveAutoFrontend), () => VerifyReserveAutoFrontend()),
        new(nameof(VerifyPauseReserveManual), () => VerifyPauseReserveManual()),
        new(nameof(VerifyPauseReserveArrow), () => VerifyPauseReserveArrow()),
        new(nameof(VerifyPauseReserveArrowRebindKeepsLatch), () => VerifyPauseReserveArrowRebindKeepsLatch()),
        new(nameof(VerifyPauseReserveTanks), () => VerifyPauseReserveTanks()),
        new(nameof(VerifyPauseReserveHud), () => VerifyPauseReserveHud()),
        new(nameof(VerifyDoorMusicTiming), () => VerifyDoorMusicTiming()),
        new(nameof(VerifyDoorAlignmentParity), () => VerifyDoorAlignmentParity()),
        new(nameof(VerifyDoorOpeningTrajectories), () => VerifyDoorOpeningTrajectories()),
        new(nameof(VerifyCreditsObjectInterpreter), () => VerifyCreditsObjectInterpreter()),
        new(nameof(VerifyEndingCreditsState), () => VerifyEndingCreditsState()),
        new(nameof(VerifyGenericGamepadInput), () => VerifyGenericGamepadInput()),
        new(nameof(VerifyDemoInputObject), () => VerifyDemoInputObject()),
        new(nameof(VerifyAttractDemoScene), () => VerifyAttractDemoScene()),
        new(nameof(VerifyAttractDemoControllerOverride), () => VerifyAttractDemoControllerOverride()),
        new(nameof(VerifyGrappleDemoTrajectory), () => VerifyGrappleDemoTrajectory()),
        new(nameof(VerifyFrameRuntime), () => VerifyFrameRuntime()),
        new(nameof(VerifyGameTimeState), () => VerifyGameTimeState()),
        new(nameof(VerifySuperMetroidAddressSpace), () => VerifySuperMetroidAddressSpace()),
        new(nameof(VerifyCartridgeAudioQueues), () => VerifyCartridgeAudioQueues()),
        new(nameof(VerifyManagedSnesDsp), () => VerifyManagedSnesDsp()),
        new(nameof(VerifyTypedNativeWords), () => VerifyTypedNativeWords()),
        new(nameof(VerifySamusSpatialValues), () => VerifySamusSpatialValues()),
        new(nameof(VerifyProductionMagicNumberAudit), () => VerifyProductionMagicNumberAudit()),
        new(nameof(VerifyPhantoonWaveLifecycle), () => VerifyPhantoonWaveLifecycle()),
        new(nameof(VerifyBackgroundMosaicSampling), () => VerifyBackgroundMosaicSampling()),
        new(nameof(VerifyOamSpritemapPacking), () => VerifyOamSpritemapPacking()),
        new(nameof(VerifyCeresElevatorArrivalGraphicsIndex), () => VerifyCeresElevatorArrivalGraphicsIndex()),
        new(nameof(VerifySamusRenderingSlice), () => VerifySamusRenderingSlice()),
        new(nameof(VerifySamusMovementRomData), () => VerifySamusMovementRomData()),
        new(nameof(VerifySamusRenderingRomData), () => VerifySamusRenderingRomData()),
        new(nameof(VerifySamusPaletteRomData), () => VerifySamusPaletteRomData()),
        new(nameof(VerifySamusSpecialSequenceRomData), () => VerifySamusSpecialSequenceRomData()),
        new(nameof(VerifySamusArmCannon), () => VerifySamusArmCannon()),
        new(nameof(VerifySamusProjectileRomData), () => VerifySamusProjectileRomData()),
        new(nameof(VerifySamusGrappleRomData), () => VerifySamusGrappleRomData()),
        new(nameof(VerifySamusXrayRomData), () => VerifySamusXrayRomData()),
        new(nameof(VerifySamusHudSelection), () => VerifySamusHudSelection()),
        new(nameof(VerifySamusVisorPalette), () => VerifySamusVisorPalette()),
        new(nameof(VerifySamusHurtFlashPalette), () => VerifySamusHurtFlashPalette()),
        new(nameof(VerifySamusPoseTransitionMatching), () => VerifySamusPoseTransitionMatching()),
        new(nameof(VerifySamusHorizontalSpeed), () => VerifySamusHorizontalSpeed()),
        new(nameof(VerifySamusExtraDisplacement), () => VerifySamusExtraDisplacement()),
        new(nameof(VerifySamusStoredShineAndShinespark), () => VerifySamusStoredShineAndShinespark()),
        new(nameof(VerifySamusCrystalFlash), () => VerifySamusCrystalFlash()),
        new(nameof(VerifyCrystalFlashRuntime), () => VerifyCrystalFlashRuntime()),
        new(nameof(VerifySamusXray), () => VerifySamusXray()),
        new(nameof(VerifySamusDeathSequence), () => VerifySamusDeathSequence()),
        new(nameof(VerifyMotherBrainBeamWindow), () => VerifyMotherBrainBeamWindow()),
        new(nameof(VerifyMotherBrainDeathHandoff), () => VerifyMotherBrainDeathHandoff()),
        new(nameof(VerifySamusDrainedController), () => VerifySamusDrainedController()),
        new(nameof(VerifySamusGrabbedByDraygon), () => VerifySamusGrabbedByDraygon()),
        new(nameof(VerifyMotherBrainRainbowBeamSamusMovement), () => VerifyMotherBrainRainbowBeamSamusMovement()),
        new(nameof(VerifyMotherBrainRainbowBeamAttackSequence), () => VerifyMotherBrainRainbowBeamAttackSequence()),
        new(nameof(MotherBrainEffectAnimationsVerification.VerifyMotherBrainEffectAnimations), () => MotherBrainEffectAnimationsVerification.VerifyMotherBrainEffectAnimations(SuperMetroid.Verification.Harness.RepositoryInputsVerification.LoadRepositoryRom())),
        new(nameof(MotherBrainEffectBehaviorVerification.VerifyMotherBrainEffectBehavior), () => MotherBrainEffectBehaviorVerification.VerifyMotherBrainEffectBehavior(SuperMetroid.Verification.Harness.RepositoryInputsVerification.LoadRepositoryRom())),
        new(nameof(MiscDustProjectileAnimationsVerification.VerifyMiscDustProjectileAnimations), () => MiscDustProjectileAnimationsVerification.VerifyMiscDustProjectileAnimations(SuperMetroid.Verification.Harness.RepositoryInputsVerification.LoadRepositoryRom())),
        new(nameof(MiscDustProjectileBehaviorVerification.VerifyMiscDustProjectileBehavior), () => MiscDustProjectileBehaviorVerification.VerifyMiscDustProjectileBehavior(SuperMetroid.Verification.Harness.RepositoryInputsVerification.LoadRepositoryRom())),
        new(nameof(VerifyBabyMetroidCutsceneEntrance), () => VerifyBabyMetroidCutsceneEntrance()),
        new(nameof(VerifySamusSolidEnemyCollision), () => VerifySamusSolidEnemyCollision()),
        new(nameof(VerifySamusAerialMovement), () => VerifySamusAerialMovement()),
        new(nameof(VerifyZeroDistanceJumpContact), () => VerifyZeroDistanceJumpContact()),
        new(nameof(VerifyShinesparkEnemyStop), () => VerifyShinesparkEnemyStop()),
        new(nameof(VerifyKnockbackHorizontalStop), () => VerifyKnockbackHorizontalStop()),
        new(nameof(VerifyRetailFallingSpeedRecurrence), () => VerifyRetailFallingSpeedRecurrence()),
        new(nameof(VerifyCrampedAerialLandingPoseCollision), () => VerifyCrampedAerialLandingPoseCollision()),
        new(nameof(VerifySamusSpaceJumpAndScrewAttack), () => VerifySamusSpaceJumpAndScrewAttack()),
        new(nameof(VerifySamusLiquidPhysics), () => VerifySamusLiquidPhysics()),
        new(nameof(VerifyShallowWaterJump), () => VerifyShallowWaterJump()),
        new(nameof(VerifySamusAtmosphericEffects), () => VerifySamusAtmosphericEffects()),
        new(nameof(VerifySamusAerialTurnsAndWallJump), () => VerifySamusAerialTurnsAndWallJump()),
        new(nameof(VerifySamusPoseHistory), () => VerifySamusPoseHistory()),
        new(nameof(VerifyWallJumpDust), () => VerifyWallJumpDust()),
        new(nameof(VerifyCeresHazeLifecycle), () => VerifyCeresHazeLifecycle()),
        new(nameof(VerifyCeresRidleyWallImpact), () => VerifyCeresRidleyWallImpact()),
        new(nameof(VerifySamusKnockbackAndDamageBoost), () => VerifySamusKnockbackAndDamageBoost()),
        new(nameof(VerifyYappingMawGrappleRelease), () => VerifyYappingMawGrappleRelease()),
        new(nameof(VerifyKiHunterSpitAudio), () => VerifyKiHunterSpitAudio()),
        new(nameof(VerifySamusGrappleSwingAndRelease), () => VerifySamusGrappleSwingAndRelease()),
        new(nameof(VerifyGrappleBlueDoors), () => VerifyGrappleBlueDoors()),
        new(nameof(VerifyGrappleSounds), () => VerifyGrappleSounds()),
        new(nameof(VerifyGrapplePoseRefire), () => VerifyGrapplePoseRefire()),
        new(nameof(VerifyGrappleSpinInput), () => VerifyGrappleSpinInput()),
        new(nameof(VerifyGrappleGreenGateVisibility), () => VerifyGrappleGreenGateVisibility()),
        new(nameof(VerifyGrappleEnemyDeath), () => VerifyGrappleEnemyDeath()),
        new(nameof(VerifyShutterRiding), () => VerifyShutterRiding()),
        new(nameof(VerifyXrayInput), () => VerifyXrayInput()),
        new(nameof(VerifyXrayGameState), () => VerifyXrayGameState()),
        new(nameof(VerifySpeedBoosterStageFiveResidue), () => VerifySpeedBoosterStageFiveResidue()),
        new(nameof(VerifyDraygonPrebattleXray), () => VerifyDraygonPrebattleXray()),
        new(nameof(VerifyXrayWindowGeometry), () => VerifyXrayWindowGeometry()),
        new(nameof(VerifyXraySetupBuffers), () => VerifyXraySetupBuffers()),
        new(nameof(VerifyFirefleaFx), () => VerifyFirefleaFx()),
        new(nameof(VerifyBombJumpWallContact), () => VerifyBombJumpWallContact()),
        new(nameof(VerifyXrayRoomDisplayRules), () => VerifyXrayRoomDisplayRules()),
        new(nameof(VerifyXrayRevealTable), () => VerifyXrayRevealTable()),
        new(nameof(VerifyXrayExtensions), () => VerifyXrayExtensions()),
        new(nameof(VerifyXrayTilemap), () => VerifyXrayTilemap()),
        new(nameof(VerifyXrayOverlays), () => VerifyXrayOverlays()),
        new(nameof(VerifyBreakableGrapplePlms), () => VerifyBreakableGrapplePlms()),
        new(nameof(VerifyBombBlockPrograms), () => VerifyBombBlockPrograms()),
        new(nameof(VerifyContactCrumblePrograms), () => VerifyContactCrumblePrograms()),
        new(nameof(VerifyStationAnimationProgramDefinitions), () => VerifyStationAnimationProgramDefinitions()),
        new(nameof(VerifyCollectibleMessageTiming), () => VerifyCollectibleMessageTiming()),
        new(nameof(VerifyPermanentCollectibles), () => VerifyPermanentCollectibles()),
        new(nameof(VerifyCollectibleVisuals), () => VerifyCollectibleVisuals()),
        new(nameof(VerifyEnemyDrops), () => VerifyEnemyDrops()),
        new(nameof(VerifySamusPostureMovement), () => VerifySamusPostureMovement()),
        new(nameof(VerifySamusPowerBeamProjectiles), () => VerifySamusPowerBeamProjectiles()),
        new(nameof(ProbeProjectileVelocityInheritance), () => ProbeProjectileVelocityInheritance()),
        new(nameof(VerifyProjectileCooldowns), () => VerifyProjectileCooldowns()),
        new(nameof(VerifyWrapShotTrace), () => VerifyWrapShotTrace("csharp/test-fixtures/movement-release/wrap-shot-409.csv")),
        new(nameof(VerifyRetailWrapShotDoors), () => VerifyRetailWrapShotDoors()),
        new(nameof(VerifyWrapShotEnemySeparation), () => VerifyWrapShotEnemySeparation()),
        new(nameof(VerifyWrapShotWidths), () => VerifyWrapShotWidths("csharp/test-fixtures/movement-release/wrap-width-409.csv")),
        new(nameof(VerifyCeilingWrapPlmTrace), () => VerifyCeilingWrapPlmTrace("csharp/test-fixtures/movement-release/ceiling-plm-410.csv")),
        new(nameof(VerifyKronicGateBeamCollision), () => VerifyKronicGateBeamCollision()),
        new(nameof(VerifyGateJumpTraces), () => VerifyGateJumpTraces()),
        new(nameof(VerifyRightFacingGateGlitches), () => VerifyRightFacingGateGlitches()),
        new(nameof(VerifyGreenHillGrappleSpeedGateGlitch), () => VerifyGreenHillGrappleSpeedGateGlitch()),
        new(nameof(VerifyGModeGateGlitch), () => VerifyGModeGateGlitch()),
        new(nameof(VerifyFrozenEnemyGateGlitch), () => VerifyFrozenEnemyGateGlitch()),
        new(nameof(VerifyMochtroidBotwoonPipeClip), () => VerifyMochtroidBotwoonPipeClip()),
        new(nameof(VerifySparkCrashAlignment), () => VerifySparkCrashAlignment()),
        new(nameof(VerifyEnemyAngleDivision), () => VerifyEnemyAngleDivision()),
        new(nameof(VerifyDraygonEyeEffects), () => VerifyDraygonEyeEffects()),
        new(nameof(VerifyDraygonTilemapProduction), () => VerifyDraygonTilemapProduction()),
        new(nameof(VerifyFrogSpeedwayPoolCollision), () => VerifyFrogSpeedwayPoolCollision()),
        new(nameof(VerifyFrogSpeedwayRuntimeTrace), () => VerifyFrogSpeedwayRuntimeTrace("csharp/test-fixtures/movement-release/frog-runtime-410.csv")),
        new(nameof(VerifyFrogSpeedwayRuntimeTrace), () => VerifyFrogSpeedwayRuntimeTrace("csharp/test-fixtures/movement-release/frog-runtime-410.csv", 9)),
        new(nameof(VerifyFrogSpeedwayRuntimeTrace), () => VerifyFrogSpeedwayRuntimeTrace("csharp/test-fixtures/movement-release/frog-success-run-410.csv", 11)),
        new(nameof(VerifyFrogSpeedwayRuntimeTrace), () => VerifyFrogSpeedwayRuntimeTrace("csharp/test-fixtures/movement-release/frog-success-walk-410.csv", 11, false)),
        new(nameof(VerifyHeroShotCameraLifetime), () => VerifyHeroShotCameraLifetime("csharp/test-fixtures/movement-release/hero-shot-411.csv")),
        new(nameof(VerifyHeroShotRuntimeCamera), () => VerifyHeroShotRuntimeCamera("csharp/test-fixtures/movement-release/hero-runtime-603.csv")),
        new(nameof(VerifyMissileImpactCameraEdge), () => VerifyMissileImpactCameraEdge("csharp/test-fixtures/movement-release/missile-edge-602.csv")),
        new(nameof(VerifyControlledRedTowerHeroShot), () => VerifyControlledRedTowerHeroShot()),
        new(nameof(VerifyBeamSpeedRows), () => VerifyBeamSpeedRows()),
        new(nameof(VerifyBeamCallbackTables), () => VerifyBeamCallbackTables()),
        new(nameof(VerifySamusMorphBallMovement), () => VerifySamusMorphBallMovement()),
        new(nameof(VerifyShotBlockPlmPrograms), () => VerifyShotBlockPlmPrograms()),
        new(nameof(VerifyCompactWalkOffCollision), () => VerifyCompactWalkOffCollision()),
        new(nameof(VerifyPauseMomentumReconciliation), () => VerifyPauseMomentumReconciliation()),
        new(nameof(VerifyBombChargeRejection), () => VerifyBombChargeRejection()),
        new(nameof(VerifyGroundedBombSpread), () => VerifyGroundedBombSpread()),
        new(nameof(VerifyGroundedSpreadTransition), () => VerifyGroundedSpreadTransition()),
        new(nameof(VerifyAerialSpreadTransitions), () => VerifyAerialSpreadTransitions()),
        new(nameof(VerifyAerialSpreadTransitions), () => VerifyAerialSpreadTransitions(wallRoute: true)),
        new(nameof(VerifySamusStandingAimMovement), () => VerifySamusStandingAimMovement()),
        new(nameof(VerifySamusAimedAerialMovement), () => VerifySamusAimedAerialMovement()),
        new(nameof(VerifySamusGunExtendedMovement), () => VerifySamusGunExtendedMovement()),
        new(nameof(VerifySamusSlopePhysics), () => VerifySamusSlopePhysics()),
        new(nameof(VerifySamusBlockCollision), () => VerifySamusBlockCollision()),
        new(nameof(VerifySpeedBoosterCollisionBlocks), () => VerifySpeedBoosterCollisionBlocks()),
        new(nameof(VerifyMaridiaElevatubePlm), () => VerifyMaridiaElevatubePlm()),
        new(nameof(VerifyCompiledTourianAccessPlmPrograms), () => VerifyCompiledTourianAccessPlmPrograms()),
        new(nameof(VerifyTourianAccessVisuals), () => VerifyTourianAccessVisuals()),
        new(nameof(VerifySpeedBoosterVisuals), () => VerifySpeedBoosterVisuals()),
        new(nameof(VerifyMaridiaElevatubeVisuals), () => VerifyMaridiaElevatubeVisuals()),
        new(nameof(VerifyCompiledSporeSpawnCeilingPlms), () => VerifyCompiledSporeSpawnCeilingPlms()),
        new(nameof(VerifyCompiledBotwoonWallPlms), () => VerifyCompiledBotwoonWallPlms()),
        new(nameof(VerifyBotwoonWallVisuals), () => VerifyBotwoonWallVisuals()),
        new(nameof(VerifyCompiledKraidRoomPlms), () => VerifyCompiledKraidRoomPlms()),
        new(nameof(VerifyCompiledCrocomireArenaPlms), () => VerifyCompiledCrocomireArenaPlms()),
        new(nameof(VerifyCompiledMotherBrainFakeDeathPlms), () => VerifyCompiledMotherBrainFakeDeathPlms()),
        new(nameof(VerifyMotherBrainFakeDeathVisuals), () => VerifyMotherBrainFakeDeathVisuals()),
        new(nameof(VerifyCrocomireArenaVisuals), () => VerifyCrocomireArenaVisuals()),
        new(nameof(VerifyKraidRoomVisuals), () => VerifyKraidRoomVisuals()),
        new(nameof(VerifySporeSpawnCeilingVisuals), () => VerifySporeSpawnCeilingVisuals()),
        new(nameof(VerifySamusEaterVisuals), () => VerifySamusEaterVisuals()),
        new(nameof(VerifySamusGroundedMovement), () => VerifySamusGroundedMovement()),
        new(nameof(VerifySamusGroundedReversal), () => VerifySamusGroundedReversal()),
        new(nameof(VerifySamusMoonwalking), () => VerifySamusMoonwalking()),
        new(nameof(VerifySamusRanIntoWall), () => VerifySamusRanIntoWall()),
        new(nameof(VerifyObjRendering), () => VerifyObjRendering()),
        new(nameof(VerifyHudStateAndBg3Rendering), () => VerifyHudStateAndBg3Rendering()),
        new(nameof(VerifyRoomScrollGridAndBoundaryCamera), () => VerifyRoomScrollGridAndBoundaryCamera()),
        new(nameof(VerifyRoomScrollPlms), () => VerifyRoomScrollPlms()),
        new(nameof(VerifyRoomPlmHeaderCatalog), () => VerifyRoomPlmHeaderCatalog()),
        new(nameof(VerifyRoomPlmInstructionListCatalog), () => VerifyRoomPlmInstructionListCatalog()),
        new(nameof(VerifySequentialRoomPlmPopulationLoader), () => VerifySequentialRoomPlmPopulationLoader()),
        new(nameof(VerifyMotherBrainEscapeGateCompiledDefinitions), () => VerifyMotherBrainEscapeGateCompiledDefinitions(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")))),
        new(nameof(VerifyEscapeGateVisuals), () => VerifyEscapeGateVisuals(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")))),
        new(nameof(VerifyCompiledRoomPlmPopulationDefinitions), () => VerifyCompiledRoomPlmPopulationDefinitions(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")))),
        new(nameof(VerifyNoobTubePlm), () => VerifyNoobTubePlm()),
        new(nameof(VerifyDownwardGatePlms), () => VerifyDownwardGatePlms()),
        new(nameof(VerifyEyeDoorPlms), () => VerifyEyeDoorPlms()),
        new(nameof(VerifyDraygonCannonPlms), () => VerifyDraygonCannonPlms()),
        new(nameof(VerifyBombTorizoHandPlm), () => VerifyBombTorizoHandPlm()),
        new(nameof(VerifyPauseMenuEquipmentInteraction), () => VerifyPauseMenuEquipmentInteraction()),
        new(nameof(VerifyPauseMapArrows), () => VerifyPauseMapArrows()),
        new(nameof(VerifyPauseMapAreaLabels), () => VerifyPauseMapAreaLabels()),
        new(nameof(VerifyPauseMapPosition), () => VerifyPauseMapPosition()),
        new(nameof(VerifySpeedPaletteOverrun), () => VerifySpeedPaletteOverrun()),
        new(nameof(VerifyInvalidBeamSelection), () => VerifyInvalidBeamSelection()),
        new(nameof(VerifyInvalidBeamGraphics), () => VerifyInvalidBeamGraphics()),
        new(nameof(VerifyMovedSamusCameraTracking), () => VerifyMovedSamusCameraTracking()),
        new(nameof(VerifyBackgroundScrollState), () => VerifyBackgroundScrollState()),
        new(nameof(VerifyLevelBlockTilemapExpansion), () => VerifyLevelBlockTilemapExpansion()),
        new(nameof(VerifyRoomLevelData), () => VerifyRoomLevelData()),
        new(nameof(VerifyCartridgeRoomStateSelection), () => VerifyCartridgeRoomStateSelection()),
        new(nameof(VerifyCompiledRoomHeaderDefinitions), () => VerifyCompiledRoomHeaderDefinitions()),
        new(nameof(VerifyCompiledRoomStateDefinitions), () => VerifyCompiledRoomStateDefinitions()),
        new(nameof(VerifyCompiledRoomStateSelectionDefinitions), () => VerifyCompiledRoomStateSelectionDefinitions()),
        new(nameof(VerifyCompiledLoadStationDefinitions), () => VerifyCompiledLoadStationDefinitions()),
        new(nameof(VerifyCompiledDoorDefinitions), () => VerifyCompiledDoorDefinitions()),
        new(nameof(VerifyCompiledEnemyDefinitions), () => VerifyCompiledEnemyDefinitions()),
        new(nameof(VerifyCompiledEnemyRoomLists), () => VerifyCompiledEnemyRoomLists()),
        new(nameof(VerifyCompiledRoomScrollDefinitions), () => VerifyCompiledRoomScrollDefinitions()),
        new(nameof(VerifyCompiledRoomCallbackDefinitions), () => VerifyCompiledRoomCallbackDefinitions()),
        new(nameof(VerifyCompiledRoomDefinitionIntegration), () => VerifyCompiledRoomDefinitionIntegration()),
        new(nameof(VerifyRoomMainCodeCatalog), () => VerifyRoomMainCodeCatalog()),
        new(nameof(VerifyRoomSetupCodeCatalog), () => VerifyRoomSetupCodeCatalog()),
        new(nameof(VerifyRoomAssetRomData), () => VerifyRoomAssetRomData()),
        new(nameof(VerifyAreaMapAssets), () => VerifyAreaMapAssets()),
        new(nameof(VerifyReplayTraceCompaction), () => VerifyReplayTraceCompaction()),
        new(nameof(VerifyReplaySnapshotSaveRamSizes), () => VerifyReplaySnapshotSaveRamSizes()),
        .. MapPresentationSuites(),
        new(nameof(VerifyBackgroundTilemapStreamer), () => VerifyBackgroundTilemapStreamer()),
        new(nameof(VerifyFourBitBackgroundRendering), () => VerifyFourBitBackgroundRendering()),
        new(nameof(VerifyLoRomCrossBankCompressedData), () => VerifyLoRomCrossBankCompressedData()),
        new(nameof(VerifyMode7Rendering), () => VerifyMode7Rendering()),
        new(nameof(VerifyLayerCompositorBackdrop), () => VerifyLayerCompositorBackdrop()),
        new(nameof(VerifyBgPriorityPlaneRendering), () => VerifyBgPriorityPlaneRendering()),
        new(nameof(VerifyLibraryBackgroundLoader), () => VerifyLibraryBackgroundLoader()),
        new(nameof(VerifyControllerInputRecording), () => VerifyControllerInputRecording()),
        new(nameof(VerifySaveRamLayout), () => VerifySaveRamLayout()),
        new(nameof(VerifyExploredMapPackingDefinitions), () => VerifyExploredMapPackingDefinitions()),
        new(nameof(VerifyGameSaveJsonPersistence), () => VerifyGameSaveJsonPersistence()),
        new(nameof(VerifySelectedSaveSlotDecode), () => VerifySelectedSaveSlotDecode()),
        new(nameof(VerifyFileSelectFreshSaveTilemap), () => VerifyFileSelectFreshSaveTilemap()),
        new(nameof(VerifyFileSelectMapWindow), () => VerifyFileSelectMapWindow()),
        new(nameof(VerifySavedGameLoadAppearance), () => VerifySavedGameLoadAppearance()),
        new(nameof(VerifyBossResetOnLoad), () => VerifyBossResetOnLoad()),
        new(nameof(VerifyIntroCinematicRomData), () => VerifyIntroCinematicRomData()),
        .. IntroCinematicArtworkSuites(),
        new(nameof(VerifyIntroGameplayFlashbackVerticalScroll), () => VerifyIntroGameplayFlashbackVerticalScroll()),
        new(nameof(VerifyIntroMotherBrainDefinitions), () => VerifyIntroMotherBrainDefinitions()),
        new(nameof(VerifyIntroRinkaDefinitions), () => VerifyIntroRinkaDefinitions()),
        new(nameof(VerifyIntroBabyActorDefinitions), () => VerifyIntroBabyActorDefinitions()),
        new(nameof(VerifyIntroEggEffectDefinitions), () => VerifyIntroEggEffectDefinitions()),
        new(nameof(VerifyCeresExplosionDefinitions), () => VerifyCeresExplosionDefinitions()),
        new(nameof(VerifyCeresFlightActorDefinitions), () => VerifyCeresFlightActorDefinitions()),
        new(nameof(VerifyCeresDestructionActorDefinitions), () => VerifyCeresDestructionActorDefinitions()),
        new(nameof(VerifyCinematicPaletteFader), () => VerifyCinematicPaletteFader()),
        new(nameof(VerifyHostRoomViewportAlignment), () => VerifyHostRoomViewportAlignment()),
        new(nameof(VerifyPowerBombColorMathWindow), () => VerifyPowerBombColorMathWindow()),
        new(nameof(VerifyHardwareWindows), () => VerifyHardwareWindows()),
        new(nameof(VerifyChainsawFiring), () => VerifyChainsawFiring()),
        new(nameof(VerifySpacetimeBeam), () => VerifySpacetimeBeam()),
        new(nameof(VerifyMurderBeam), () => VerifyMurderBeam()),
        new(nameof(VerifyPowerBombRuntimeRendererIntegration), () => VerifyPowerBombRuntimeRendererIntegration()),
        new(nameof(VerifyPowerBombFuse), () => VerifyPowerBombFuse()),
        new(nameof(VerifyPowerBombBoundary), () => VerifyPowerBombBoundary()),
        new(nameof(VerifyPowerBombDeathRadius), () => VerifyPowerBombDeathRadius()),
        new(nameof(VerifyRespawnedEnemyContact), () => VerifyRespawnedEnemyContact()),
        new(nameof(VerifyVerticalOffScreenDeletion), () => VerifyVerticalOffScreenDeletion()),
        new(nameof(VerifyBotwoonPositionHistory), () => VerifyBotwoonPositionHistory()),
        new(nameof(VerifyDraygonTurretCadence), () => VerifyDraygonTurretCadence()),
        new(nameof(VerifyWallJumpSpinExit), () => VerifyWallJumpSpinExit()),
        new(nameof(VerifyEvirInitTimer), () => VerifyEvirInitTimer()),
        new(nameof(VerifyRefillStationLock), () => VerifyRefillStationLock()),
        new(nameof(VerifyMagdolliteApexThreshold), () => VerifyMagdolliteApexThreshold()),
        new(nameof(VerifyGoldenTorizoAttackChoice), () => VerifyGoldenTorizoAttackChoice()),
        new(nameof(VerifyWallProbeBombBlock), () => VerifyWallProbeBombBlock()),
        new(nameof(VerifyPuromiArcPosition), () => VerifyPuromiArcPosition()),
        new(nameof(VerifySpringBallBounce), () => VerifySpringBallBounce()),
        new(nameof(VerifyMainGameLoopCarry), () => VerifyMainGameLoopCarry()),
        new(nameof(VerifyBootDemoSetCount), () => VerifyBootDemoSetCount()),
        new(nameof(VerifyDoorEntryRoomFxSound), () => VerifyDoorEntryRoomFxSound()),
        new(nameof(VerifyDoorEntryShotBinding), () => VerifyDoorEntryShotBinding()),
        new(nameof(VerifyDraygonGoopTouchOrder), () => VerifyDraygonGoopTouchOrder()),
        new(nameof(VerifyDraygonGrabPoseRequests), () => VerifyDraygonGrabPoseRequests()),
        new(nameof(VerifyDraygonGrabSpiralFraction), () => VerifyDraygonGrabSpiralFraction()),
        new(nameof(VerifyGameOptionsStartGame), () => VerifyGameOptionsStartGame()),
        new(nameof(VerifyFileSelectMapLoadTiming), () => VerifyFileSelectMapLoadTiming()),
        new(nameof(VerifyGoldNinjaDeathDrops), () => VerifyGoldNinjaDeathDrops()),
        new(nameof(VerifyDoorEntryEnemySound), () => VerifyDoorEntryEnemySound()),
        new(nameof(VerifySpringBallFallingFallback), () => VerifySpringBallFallingFallback()),
        new(nameof(VerifyFirefleaDoubleDeath), () => VerifyFirefleaDoubleDeath()),
        new(nameof(VerifyShutterScrewContact), () => VerifyShutterScrewContact()),
        new(nameof(VerifyJumpNoXMovement), () => VerifyJumpNoXMovement()),
        new(nameof(VerifySaveConfirmationCadence), () => VerifySaveConfirmationCadence()),
        new(nameof(VerifyTourianStatueXrayFreeze), () => VerifyTourianStatueXrayFreeze()),
        new(nameof(VerifyDoorAnimatedTiles), () => VerifyDoorAnimatedTiles()),
        new(nameof(VerifyTourianStatueDescentRounding), () => VerifyTourianStatueDescentRounding()),
        new(nameof(VerifyMetroidDeathDrops), () => VerifyMetroidDeathDrops()),
        new(nameof(VerifyShitroidGradualAcceleration), () => VerifyShitroidGradualAcceleration()),
        new(nameof(VerifyShitroidDrainCarry), () => VerifyShitroidDrainCarry()),
        new(nameof(VerifyMotherBrainRinkaDoorSpawn), () => VerifyMotherBrainRinkaDoorSpawn()),
        new(nameof(VerifyMotherBrainGlassSuperMissile), () => VerifyMotherBrainGlassSuperMissile()),
        new(nameof(VerifyMotherBrainTubeTiming), () => VerifyMotherBrainTubeTiming()),
        new(nameof(VerifyMotherBrainTubeHdmaDeletion), () => VerifyMotherBrainTubeHdmaDeletion()),
        new(nameof(VerifyMotherBrainRaiseCounter), () => VerifyMotherBrainRaiseCounter()),
        new(nameof(VerifyMotherBrainHeadHitbox), () => VerifyMotherBrainHeadHitbox()),
        new(nameof(VerifyMotherBrainSmallPurpleBreath), () => VerifyMotherBrainSmallPurpleBreath()),
        new(nameof(VerifyRainbowReleaseKnockback), () => VerifyRainbowReleaseKnockback()),
        new(nameof(VerifyBabyMetroidInheritedFractions), () => VerifyBabyMetroidInheritedFractions()),
        new(nameof(VerifyBabyMetroidHeadTarget), () => VerifyBabyMetroidHeadTarget()),
        new(nameof(VerifyMotherBrainWalkBackwardsPose), () => VerifyMotherBrainWalkBackwardsPose()),
        new(nameof(VerifyMotherBrainRingBabyHealth), () => VerifyMotherBrainRingBabyHealth()),
        new(nameof(VerifyBabyMetroidWrongWaySpeed), () => VerifyBabyMetroidWrongWaySpeed()),
        new(nameof(VerifyMotherBrainGlitchMechanics), () => VerifyMotherBrainGlitchMechanics()),
        new(nameof(VerifyXraySetupStageDomain), () => VerifyXraySetupStageDomain()),
        new(nameof(VerifyDoorOrientationDomain), () => VerifyDoorOrientationDomain()),
        new(nameof(VerifyBgr555Domain), () => VerifyBgr555Domain()),
        new(nameof(VerifyBabyMetroidFatalBlowShake), () => VerifyBabyMetroidFatalBlowShake()),
        new(nameof(VerifyMotherBrainMissileWalkReset), () => VerifyMotherBrainMissileWalkReset()),
        new(nameof(VerifyMotherBrainBodyHitboxes), () => VerifyMotherBrainBodyHitboxes()),
        new(nameof(VerifyMotherBrainInheritedExplosionIndex), () => VerifyMotherBrainInheritedExplosionIndex()),
        new(nameof(VerifyOldTourianEscapeShaftWall), () => VerifyOldTourianEscapeShaftWall()),
        new(nameof(VerifyCrateriaMainstreetEscapePassage), () => VerifyCrateriaMainstreetEscapePassage()),
        new(nameof(VerifyZebesEscapeFade), () => VerifyZebesEscapeFade()),
        new(nameof(VerifyEndingSetupNmiWaits), () => VerifyEndingSetupNmiWaits()),
        new(nameof(VerifyBeamImpactSound), () => VerifyBeamImpactSound()),
        new(nameof(VerifySquareSlopeBeamCollision), () => VerifySquareSlopeBeamCollision()),
        new(nameof(VerifyAirSpikeAlphaRadius), () => VerifyAirSpikeAlphaRadius()),
        new(nameof(VerifySuitPickupPeriodicDamage), () => VerifySuitPickupPeriodicDamage()),
        new(nameof(VerifyKraidArmSamusContact), () => VerifyKraidArmSamusContact()),
        new(nameof(VerifyShaktoolSamusContact), () => VerifyShaktoolSamusContact()),
        new(nameof(VerifyEmptyExtendedFrameShots), () => VerifyEmptyExtendedFrameShots()),
        new(nameof(VerifyElevatorStandUpContact), () => VerifyElevatorStandUpContact()),
        new(nameof(VerifyImplicitScrollResidue), () => VerifyImplicitScrollResidue()),
        new(nameof(VerifySuperMissileDeathAnimation), () => VerifySuperMissileDeathAnimation()),
        new(nameof(VerifyDoorSoundsDuringPowerBomb), () => VerifyDoorSoundsDuringPowerBomb()),
        new(nameof(VerifyHostIndependentAudioAcknowledgements), () => VerifyHostIndependentAudioAcknowledgements()),
        new(nameof(VerifyKnockbackShinesparkLaunch), () => VerifyKnockbackShinesparkLaunch()),
        new(nameof(VerifyUnpauseElevatorFlags), () => VerifyUnpauseElevatorFlags()),
        new(nameof(VerifyRidleyShotHealthStage), () => VerifyRidleyShotHealthStage()),
        new(nameof(VerifyPowampDeathSequence), () => VerifyPowampDeathSequence()),
        new(nameof(VerifyUnpauseReserveBlackout), () => VerifyUnpauseReserveBlackout()),
        new(nameof(VerifyFrozenTimeEnemyProjectiles), () => VerifyFrozenTimeEnemyProjectiles()),
        new(nameof(VerifyItemCancelClearsCharge), () => VerifyItemCancelClearsCharge()),
        new(nameof(VerifyDraygonScrollingSpeedCap), () => VerifyDraygonScrollingSpeedCap()),
        new(nameof(VerifyDraygonEscapeDrag), () => VerifyDraygonEscapeDrag()),
        new(nameof(VerifyReleasedSamusFallsOffDraygon), () => VerifyReleasedSamusFallsOffDraygon()),
        new(nameof(VerifySuperMissileEnemyHitQuake), () => VerifySuperMissileEnemyHitQuake()),
        new(nameof(VerifyContactDamageSound), () => VerifyContactDamageSound()),
        new(nameof(VerifyBombJumpRunMomentum), () => VerifyBombJumpRunMomentum()),
        new(nameof(VerifyBabyMetroidWallClearTiming), () => VerifyBabyMetroidWallClearTiming()),
        new(nameof(VerifyDrainedSparkCrash), () => VerifyDrainedSparkCrash()),
        new(nameof(VerifyEyeDoorSweatSpanUnderflow), () => VerifyEyeDoorSweatSpanUnderflow()),
        new(nameof(VerifyUnpauseStationLockRelease), () => VerifyUnpauseStationLockRelease()),
        new(nameof(VerifyPauseRemappedShoulder), () => VerifyPauseRemappedShoulder()),
        new(nameof(VerifyDoorWaitsForMessageBox), () => VerifyDoorWaitsForMessageBox()),
        new(nameof(VerifyMotherBrainFakeDeathLock), () => VerifyMotherBrainFakeDeathLock()),
        new(nameof(VerifyMoonwalkRanIntoWall), () => VerifyMoonwalkRanIntoWall()),
        new(nameof(VerifyReserveRefillRainbowBeam), () => VerifyReserveRefillRainbowBeam()),
        new(nameof(VerifyDamageBoostJumpInitializer), () => VerifyDamageBoostJumpInitializer()),
        new(nameof(VerifyBabyMetroidDeathExplosionSound), () => VerifyBabyMetroidDeathExplosionSound()),
        new(nameof(VerifyRoomFxRomData), () => VerifyRoomFxRomData()),
        new(nameof(VerifyPowerBombFixedColors), () => VerifyPowerBombFixedColors()),
        new(nameof(VerifySamusVisorColors), () => VerifySamusVisorColors()),
        new(nameof(VerifySamusHurtColors), () => VerifySamusHurtColors()),
        new(nameof(VerifySamusHyperBeamColors), () => VerifySamusHyperBeamColors()),
        new(nameof(VerifySpcSoundLibrary2Pointers), () => VerifySpcSoundLibrary2Pointers()),
        new(nameof(VerifyScrollingSkyState), () => VerifyScrollingSkyState()),
        new(nameof(VerifyOceanSky), () => VerifyOceanSky()),
        new(nameof(AuditBoostFloor), () => AuditBoostFloor()),
        new(nameof(VerifyEnemyInstructionCodePointerCatalogs), () => VerifyEnemyInstructionCodePointerCatalogs()),
        new(nameof(VerifyEnemyRomTablePointerCatalog), () => VerifyEnemyRomTablePointerCatalog()),
        new(nameof(VerifyMotherBrainContactHitboxes), () => VerifyMotherBrainContactHitboxes()),
        new(nameof(VerifyMamaTurtleShellContourDefinitions), () => VerifyMamaTurtleShellContourDefinitions()),
        new(nameof(VerifyMamaTurtleEnemyDefinitions), () => VerifyMamaTurtleEnemyDefinitions()),
        new(nameof(VerifyPaletteFxInstructionCodeCatalogs), () => VerifyPaletteFxInstructionCodeCatalogs()),
        new(nameof(VerifyAnimatedTileInstructionCodeCatalog), () => VerifyAnimatedTileInstructionCodeCatalog()),
        new(nameof(VerifyEnemyProjectileCodePointerCatalog), () => VerifyEnemyProjectileCodePointerCatalog()),
        new(nameof(VerifyEnemyMappedSourceRouting), () => VerifyEnemyMappedSourceRouting()),
        new(nameof(VerifyRoomEnemyLoading), () => VerifyRoomEnemyLoading()),
        .. EnemyTileArtworkSuites(),
        new(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors()),
        new(nameof(VerifySpacePirateCollisionDefinitions), () => VerifySpacePirateCollisionDefinitions()),
        new(nameof(VerifyRidleyCollisionDefinitions), () => VerifyRidleyCollisionDefinitions()),
        new(nameof(VerifyCeresSteamCollisionDefinitions), () => VerifyCeresSteamCollisionDefinitions()),
        new(nameof(VerifyMaridiaLargeSnailCollisionDefinitions), () => VerifyMaridiaLargeSnailCollisionDefinitions()),
        new(nameof(VerifyCrocomireTongueCollisionDefinitions), () => VerifyCrocomireTongueCollisionDefinitions()),
        new(nameof(VerifyCrocomireBodyCollisionDefinitions), () => VerifyCrocomireBodyCollisionDefinitions()),
        new(nameof(VerifyBotwoonPlmIdentity), () => VerifyBotwoonPlmIdentity()),
        .. CompiledEnemyTrigonometrySuites(),
        new(nameof(VerifyRidleyExplosionDefinitions), () => VerifyRidleyExplosionDefinitions(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")))),
        new(nameof(VerifyCrocomireMeltingDefinitions), () => VerifyCrocomireMeltingDefinitions(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")))),
        new(nameof(VerifyDraygonIntroDanceDefinitions), () => VerifyDraygonIntroDanceDefinitions(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")))),
        new(nameof(VerifyEnemyProjectileCollisionLifecycle), () => VerifyEnemyProjectileCollisionLifecycle()),
        new(nameof(VerifyRipperEnemy), () => VerifyRipperEnemy()),
        new(nameof(VerifyRipperEnemy), () => VerifyRipperEnemy(verifyXrayTimers: true)),
        new(nameof(VerifyPostCeresGunshipLanding), () => VerifyPostCeresGunshipLanding()),
        new(nameof(VerifyCeresElevatorPlatformAnimation), () => VerifyCeresElevatorPlatformAnimation()),
        new(nameof(VerifyCeresDoorBossBranch), () => VerifyCeresDoorBossBranch()),
        new(nameof(VerifyCeresRidleyRoomEntry), () => VerifyCeresRidleyRoomEntry()),
        new(nameof(VerifyCeresEscapeVramTransferDefinitions), () => VerifyCeresEscapeVramTransferDefinitions(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")))),
        new(nameof(VerifyCeresEscapeHandoff), () => VerifyCeresEscapeHandoff()),
        new(nameof(VerifyCeresDestructionCinematic), () => VerifyCeresDestructionCinematic()),
    ];
}
