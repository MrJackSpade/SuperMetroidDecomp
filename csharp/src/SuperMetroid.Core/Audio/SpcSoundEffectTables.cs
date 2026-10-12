namespace SuperMetroid.Core.Audio;

/// <summary>
/// Sound-effect stream pointers and voice-allocation classes from the resident SPC driver.
/// Stream and policy dispatch use the one-based CPU command.
/// </summary>
internal static class SpcSoundEffectTables
{
    /// <summary>Number of defined one-based commands in each native SPC sound library.</summary>
    internal static int CommandCount(int libraryIndex) => libraryIndex switch
    {
        0 => 66,
        1 => 127,
        2 => 47,
        _ => throw new IndexOutOfRangeException(),
    };

    /// <summary>Selects the native instruction-list set for a one-based sound command.</summary>
    internal static ushort StreamPointer(int libraryIndex, int command) => libraryIndex switch
    {
        0 => Library1Stream(command),
        1 => Library2Stream(command),
        2 => Library3Stream(command),
        _ => throw new IndexOutOfRangeException(),
    };
    private static ushort Library1Stream(int command) => command switch
    {
        0x01 => SpcSoundStreamDefinitions.Library1.Sound01PowerBombExplosion,
        0x02 => SpcSoundStreamDefinitions.Library1.Sound02Silence,
        0x03 => SpcSoundStreamDefinitions.Library1.Sound03Missile,
        0x04 => SpcSoundStreamDefinitions.Library1.Sound04SuperMissile,
        0x05 => SpcSoundStreamDefinitions.Library1.Sound05GrappleStart,
        0x06 => SpcSoundStreamDefinitions.Library1.Sound06Grappling,
        0x07 => SpcSoundStreamDefinitions.Library1.Sound07GrappleEnd,
        0x08 => SpcSoundStreamDefinitions.Library1.Sound08ChargingBeam,
        0x09 => SpcSoundStreamDefinitions.Library1.Sound09XRay,
        0x0a => SpcSoundStreamDefinitions.Library1.Sound0AXRayEnd,
        0x0b => SpcSoundStreamDefinitions.Library1.Sound0BUnchargedPowerBeam,
        0x0c => SpcSoundStreamDefinitions.Library1.Sound0CUnchargedIceBeam,
        0x0d => SpcSoundStreamDefinitions.Library1.Sound0DUnchargedWaveBeam,
        0x0e => SpcSoundStreamDefinitions.Library1.Sound0EUnchargedIceWaveBeam,
        0x0f => SpcSoundStreamDefinitions.Library1.Sound0FUnchargedSpazerBeam,
        0x10 => SpcSoundStreamDefinitions.Library1.Sound10UnchargedSpazerIceBeam,
        0x11 => SpcSoundStreamDefinitions.Library1.Sound11UnchargedSpazerIceWaveBeam,
        0x12 => SpcSoundStreamDefinitions.Library1.Sound12UnchargedSpazerWaveBeam,
        0x13 => SpcSoundStreamDefinitions.Library1.Sound13UnchargedPlasmaBeam,
        0x14 => SpcSoundStreamDefinitions.Library1.Sound14UnchargedPlasmaIceBeam,
        0x15 => SpcSoundStreamDefinitions.Library1.Sound15UnchargedPlasmaIceWaveBeam,
        0x16 => SpcSoundStreamDefinitions.Library1.Sound16UnchargedPlasmaWaveBeam,
        0x17 => SpcSoundStreamDefinitions.Library1.Sound17ChargedPowerBeam,
        0x18 => SpcSoundStreamDefinitions.Library1.Sound18ChargedIceBeam,
        0x19 => SpcSoundStreamDefinitions.Library1.Sound19ChargedWaveBeam,
        0x1a => SpcSoundStreamDefinitions.Library1.Sound1AChargedIceWaveBeam,
        0x1b => SpcSoundStreamDefinitions.Library1.Sound1BChargedSpazerBeam,
        0x1c => SpcSoundStreamDefinitions.Library1.Sound1CChargedSpazerIceBeam,
        0x1d => SpcSoundStreamDefinitions.Library1.Sound1DChargedSpazerIceWaveBeam,
        0x1e => SpcSoundStreamDefinitions.Library1.Sound1EChargedSpazerWaveBeam,
        0x1f => SpcSoundStreamDefinitions.Library1.Sound1FChargedPlasmaBeamHyperBeam,
        0x20 => SpcSoundStreamDefinitions.Library1.Sound20ChargedPlasmaIceBeam,
        0x21 => SpcSoundStreamDefinitions.Library1.Sound21ChargedPlasmaIceWaveBeam,
        0x22 => SpcSoundStreamDefinitions.Library1.Sound22ChargedPlasmaWaveBeam,
        0x23 => SpcSoundStreamDefinitions.Library1.Sound23IceSBA,
        0x24 => SpcSoundStreamDefinitions.Library1.Sound24IceSBAEnd,
        0x25 => SpcSoundStreamDefinitions.Library1.Sound25SpazerSBA,
        0x26 => SpcSoundStreamDefinitions.Library1.Sound26SpazerSBAEnd,
        0x27 => SpcSoundStreamDefinitions.Library1.Sound27PlasmaSBA,
        0x28 => SpcSoundStreamDefinitions.Library1.Sound28WaveSBA,
        0x29 => SpcSoundStreamDefinitions.Library1.Sound29WaveSBAEnd,
        0x2a => SpcSoundStreamDefinitions.Library1.Sound2ASelectedSaveFile,
        0x2b => SpcSoundStreamDefinitions.Library1.Sound2BEmpty,
        0x2c => SpcSoundStreamDefinitions.Library1.Sound2CEmpty,
        0x2d => SpcSoundStreamDefinitions.Library1.Sound2DEmpty,
        0x2e => SpcSoundStreamDefinitions.Library1.Sound2ESaving,
        0x2f => SpcSoundStreamDefinitions.Library1.Sound2FUnderwaterSpaceJumpWithoutGravitySuit,
        0x30 => SpcSoundStreamDefinitions.Library1.Sound30ResumedSpinJump,
        0x31 => SpcSoundStreamDefinitions.Library1.Sound31SpinJump,
        0x32 => SpcSoundStreamDefinitions.Library1.Sound32SpinJumpEnd,
        0x33 => SpcSoundStreamDefinitions.Library1.Sound33ScrewAttack,
        0x34 => SpcSoundStreamDefinitions.Library1.Sound34ScrewAttackEnd,
        0x35 => SpcSoundStreamDefinitions.Library1.Sound35SamusDamaged,
        0x36 => SpcSoundStreamDefinitions.Library1.Sound36ScrollingMap,
        0x37 => SpcSoundStreamDefinitions.Library1.Sound37ToggleReserveModeMovedCursor,
        0x38 => SpcSoundStreamDefinitions.Library1.Sound38PauseMenuTransitionToggledEquipment,
        0x39 => SpcSoundStreamDefinitions.Library1.Sound39SwitchHUDItem,
        0x3a => SpcSoundStreamDefinitions.Library1.Sound3AEmpty,
        0x3b => SpcSoundStreamDefinitions.Library1.Sound3BHexagonMapSquareMapTransition,
        0x3c => SpcSoundStreamDefinitions.Library1.Sound3CSquareMapHexagonMapTransition,
        0x3d => SpcSoundStreamDefinitions.Library1.Sound3DDudShot,
        0x3e => SpcSoundStreamDefinitions.Library1.Sound3ESpaceJump,
        0x3f => SpcSoundStreamDefinitions.Library1.Sound3FResumedSpaceJump,
        0x40 => SpcSoundStreamDefinitions.Library1.Sound40MotherBrainSRainbowBeam,
        0x41 => SpcSoundStreamDefinitions.Library1.Sound41ResumeChargingBeam,
        0x42 => SpcSoundStreamDefinitions.Library1.Sound42,
        _ => throw new IndexOutOfRangeException(),
    };

    private static ushort Library2Stream(int command) => command switch
    {
        0x01 => SpcSoundStreamDefinitions.Library2.Sound01CollectedSmallHealthDrop,
        0x02 => SpcSoundStreamDefinitions.Library2.Sound02CollectedBigHealthDrop,
        0x03 => SpcSoundStreamDefinitions.Library2.Sound03CollectedMissileDrop,
        0x04 => SpcSoundStreamDefinitions.Library2.Sound04CollectedSuperMissileDrop,
        0x05 => SpcSoundStreamDefinitions.Library2.Sound05CollectedPowerBombDrop,
        0x06 => SpcSoundStreamDefinitions.Library2.Sound06BlockDestroyedByContactDamage,
        0x07 => SpcSoundStreamDefinitions.Library2.Sound07SuperMissileHitWall,
        0x08 => SpcSoundStreamDefinitions.Library2.Sound08BombExplosion,
        0x09 => SpcSoundStreamDefinitions.Library2.Sound09EnemyKilled,
        0x0a => SpcSoundStreamDefinitions.Library2.Sound0ABlockCrumbledOrDestroyedByShot,
        0x0b => SpcSoundStreamDefinitions.Library2.Sound0BEnemyKilledByContactDamage,
        0x0c => SpcSoundStreamDefinitions.Library2.Sound0CBeamHitWall,
        0x0d => SpcSoundStreamDefinitions.Library2.Sound0DSplashedIntoWater,
        0x0e => SpcSoundStreamDefinitions.Library2.Sound0ESplashedOutOfWater,
        0x0f => SpcSoundStreamDefinitions.Library2.Sound0FLowPitchedAirBubbles,
        0x10 => SpcSoundStreamDefinitions.Library2.Sound10LavaAcidDamagingSamus,
        0x11 => SpcSoundStreamDefinitions.Library2.Sound11HighPitchedAirBubbles,
        0x12 => SpcSoundStreamDefinitions.Library2.Sound12PlaysAtRandomInHeatedRooms,
        0x13 => SpcSoundStreamDefinitions.Library2.Sound13PlaysAtRandomInHeatedRooms,
        0x14 => SpcSoundStreamDefinitions.Library2.Sound14PlaysAtRandomInHeatedRooms,
        0x15 => SpcSoundStreamDefinitions.Library2.Sound15MaridiaElevatube,
        0x16 => SpcSoundStreamDefinitions.Library2.Sound16,
        0x17 => SpcSoundStreamDefinitions.Library2.Sound17MorphBallEyeSRay,
        0x18 => SpcSoundStreamDefinitions.Library2.Sound18Beacon,
        0x19 => SpcSoundStreamDefinitions.Library2.Sound19,
        0x1a => SpcSoundStreamDefinitions.Library2.Sound1AN00bTubeShattering,
        0x1b => SpcSoundStreamDefinitions.Library2.Sound1B,
        0x1c => SpcSoundStreamDefinitions.Library2.Sound1C,
        0x1d => SpcSoundStreamDefinitions.Library2.Sound1DDachoraCry,
        0x1e => SpcSoundStreamDefinitions.Library2.Sound1E,
        0x1f => SpcSoundStreamDefinitions.Library2.Sound1F,
        0x20 => SpcSoundStreamDefinitions.Library2.Sound20ShotFly,
        0x21 => SpcSoundStreamDefinitions.Library2.Sound21ShotSkreeWallNinjaSpacePirate,
        0x22 => SpcSoundStreamDefinitions.Library2.Sound22ShotPipeBugHighRisingSlowFallingEnemy,
        0x23 => SpcSoundStreamDefinitions.Library2.Sound23ShotSlugSidehopperZoomer,
        0x24 => SpcSoundStreamDefinitions.Library2.Sound24SmallExplosionEnemyDeath,
        0x25 => SpcSoundStreamDefinitions.Library2.Sound25CeresDoorExplosionAlsoUsedByMotherBrain,
        0x26 => SpcSoundStreamDefinitions.Library2.Sound26,
        0x27 => SpcSoundStreamDefinitions.Library2.Sound27ShotTorizo,
        0x28 => SpcSoundStreamDefinitions.Library2.Sound28,
        0x29 => SpcSoundStreamDefinitions.Library2.Sound29MotherBrainRisingIntoPhase2,
        0x2a => SpcSoundStreamDefinitions.Library2.Sound2A,
        0x2b => SpcSoundStreamDefinitions.Library2.Sound2BRidleySFireballHitSurface,
        0x2c => SpcSoundStreamDefinitions.Library2.Sound2CShotSporeSpawn,
        0x2d => SpcSoundStreamDefinitions.Library2.Sound2D,
        0x2e => SpcSoundStreamDefinitions.Library2.Sound2E,
        0x2f => SpcSoundStreamDefinitions.Library2.Sound2FYappingMaw,
        0x30 => SpcSoundStreamDefinitions.Library2.Sound30ShotSuperDesgeega,
        0x31 => SpcSoundStreamDefinitions.Library2.Sound31BrinstarPlantChewing,
        0x32 => SpcSoundStreamDefinitions.Library2.Sound32EtecoonWallJump,
        0x33 => SpcSoundStreamDefinitions.Library2.Sound33EtecoonCry,
        0x34 => SpcSoundStreamDefinitions.Library2.Sound34SpikeShootingPlantSpikes,
        0x35 => SpcSoundStreamDefinitions.Library2.Sound35EtecoonSTheme,
        0x36 => SpcSoundStreamDefinitions.Library2.Sound36ShotRioNorfairLavaJumpingEnemyLavaSeahorse,
        0x37 => SpcSoundStreamDefinitions.Library2.Sound37RefillMapStationEngaged,
        0x38 => SpcSoundStreamDefinitions.Library2.Sound38RefillMapStationDisengaged,
        0x39 => SpcSoundStreamDefinitions.Library2.Sound39DachoraSpeedBooster,
        0x3a => SpcSoundStreamDefinitions.Library2.Sound3A,
        0x3b => SpcSoundStreamDefinitions.Library2.Sound3BDachoraShinespark,
        0x3c => SpcSoundStreamDefinitions.Library2.Sound3CDachoraShinesparkEnded,
        0x3d => SpcSoundStreamDefinitions.Library2.Sound3DDachoraStoredShinespark,
        0x3e => SpcSoundStreamDefinitions.Library2.Sound3EShotMaridiaSpikeyShellsNorfairErraticFireballRippedKamerMaridiaSnailYappingMawWreckedShipOrbs,
        0x3f => SpcSoundStreamDefinitions.Library2.Sound3F,
        0x40 => SpcSoundStreamDefinitions.Library2.Sound40,
        0x41 => SpcSoundStreamDefinitions.Library2.Sound41,
        0x42 => SpcSoundStreamDefinitions.Library2.Sound42,
        0x43 => SpcSoundStreamDefinitions.Library2.Sound43,
        0x44 => SpcSoundStreamDefinitions.Library2.Sound44,
        0x45 => SpcSoundStreamDefinitions.Library2.Sound45TypewriterStrokeCeresSelfDestructSequence,
        0x46 => SpcSoundStreamDefinitions.Library2.Sound46,
        0x47 => SpcSoundStreamDefinitions.Library2.Sound47ShotWaver,
        0x48 => SpcSoundStreamDefinitions.Library2.Sound48,
        0x49 => SpcSoundStreamDefinitions.Library2.Sound49ShotFishCrabMaridiaRefillCandy,
        0x4a => SpcSoundStreamDefinitions.Library2.Sound4AShotMiniDraygon,
        0x4b => SpcSoundStreamDefinitions.Library2.Sound4B,
        0x4c => SpcSoundStreamDefinitions.Library2.Sound4CKiHunterEyeDoorAcidSpit,
        0x4d => SpcSoundStreamDefinitions.Library2.Sound4DGunshipHover,
        0x4e => SpcSoundStreamDefinitions.Library2.Sound4ECeresRidleyGetaway,
        0x4f => SpcSoundStreamDefinitions.Library2.Sound4F,
        0x50 => SpcSoundStreamDefinitions.Library2.Sound50,
        0x51 => SpcSoundStreamDefinitions.Library2.Sound51ShotWreckedShipGhost,
        0x52 => SpcSoundStreamDefinitions.Library2.Sound52,
        0x53 => SpcSoundStreamDefinitions.Library2.Sound53ShotMiniCrocomire,
        0x54 => SpcSoundStreamDefinitions.Library2.Sound54,
        0x55 => SpcSoundStreamDefinitions.Library2.Sound55ShotBeetom,
        0x56 => SpcSoundStreamDefinitions.Library2.Sound56AcquiredSuit,
        0x57 => SpcSoundStreamDefinitions.Library2.Sound57ShotDoorGateWithDudShotShotReflec,
        0x58 => SpcSoundStreamDefinitions.Library2.Sound58ShotMochtroid,
        0x59 => SpcSoundStreamDefinitions.Library2.Sound59RidleySRoar,
        0x5a => SpcSoundStreamDefinitions.Library2.Sound5AShotMetroid,
        0x5b => SpcSoundStreamDefinitions.Library2.Sound5BSkreeLaunchesAttack,
        0x5c => SpcSoundStreamDefinitions.Library2.Sound5CSkreeHitsTheGround,
        0x5d => SpcSoundStreamDefinitions.Library2.Sound5DSidehopperJumped,
        0x5e => SpcSoundStreamDefinitions.Library2.Sound5ESidehopperLanded,
        0x5f => SpcSoundStreamDefinitions.Library2.Sound5FShotLowerNorfairRioDesgeegaNorfairSlowFireballWalkingLavaSeahorseBotwoon,
        0x60 => SpcSoundStreamDefinitions.Library2.Sound60,
        0x61 => SpcSoundStreamDefinitions.Library2.Sound61,
        0x62 => SpcSoundStreamDefinitions.Library2.Sound62,
        0x63 => SpcSoundStreamDefinitions.Library2.Sound63MotherBrainSKetchupBeam,
        0x64 => SpcSoundStreamDefinitions.Library2.Sound64,
        0x65 => SpcSoundStreamDefinitions.Library2.Sound65,
        0x66 => SpcSoundStreamDefinitions.Library2.Sound66ShotKiHunterWalkingSpacePirate,
        0x67 => SpcSoundStreamDefinitions.Library2.Sound67SpacePirateMotherBrainLaser,
        0x68 => SpcSoundStreamDefinitions.Library2.Sound68ShotWreckedShipRobot,
        0x69 => SpcSoundStreamDefinitions.Library2.Sound69ShotShaktool,
        0x6a => SpcSoundStreamDefinitions.Library2.Sound6AShotMaridiaFloater,
        0x6b => SpcSoundStreamDefinitions.Library2.Sound6B,
        0x6c => SpcSoundStreamDefinitions.Library2.Sound6C,
        0x6d => SpcSoundStreamDefinitions.Library2.Sound6DCeresTilesFallingFromCeiling,
        0x6e => SpcSoundStreamDefinitions.Library2.Sound6EShotMotherBrainPhase1,
        0x6f => SpcSoundStreamDefinitions.Library2.Sound6FMotherBrainSCryLowPitch,
        0x70 => SpcSoundStreamDefinitions.Library2.Sound70,
        0x71 => SpcSoundStreamDefinitions.Library2.Sound71Silence,
        0x72 => SpcSoundStreamDefinitions.Library2.Sound72,
        0x73 => SpcSoundStreamDefinitions.Library2.Sound73,
        0x74 => SpcSoundStreamDefinitions.Library2.Sound74,
        0x75 => SpcSoundStreamDefinitions.Library2.Sound75,
        0x76 => SpcSoundStreamDefinitions.Library2.Sound76,
        0x77 => SpcSoundStreamDefinitions.Library2.Sound77,
        0x78 => SpcSoundStreamDefinitions.Library2.Sound78,
        0x79 => SpcSoundStreamDefinitions.Library2.Sound79,
        0x7a => SpcSoundStreamDefinitions.Library2.Sound7A,
        0x7b => SpcSoundStreamDefinitions.Library2.Sound7B,
        0x7c => SpcSoundStreamDefinitions.Library2.Sound7C,
        0x7d => SpcSoundStreamDefinitions.Library2.Sound7D,
        0x7e => SpcSoundStreamDefinitions.Library2.Sound7EMotherBrainSCryHighPitch,
        0x7f => SpcSoundStreamDefinitions.Library2.Sound7FMotherBrainChargingHerRainbow,
        _ => throw new IndexOutOfRangeException(),
    };

    private static ushort Library3Stream(int command) => command switch
    {
        0x01 => SpcSoundStreamDefinitions.Library3.Sound01Silence,
        0x02 => SpcSoundStreamDefinitions.Library3.Sound02LowHealthBeep,
        0x03 => SpcSoundStreamDefinitions.Library3.Sound03SpeedBooster,
        0x04 => SpcSoundStreamDefinitions.Library3.Sound04SamusLandedHard,
        0x05 => SpcSoundStreamDefinitions.Library3.Sound05SamusLandedWallJumped,
        0x06 => SpcSoundStreamDefinitions.Library3.Sound06SamusFootsteps,
        0x07 => SpcSoundStreamDefinitions.Library3.Sound07DoorOpened,
        0x08 => SpcSoundStreamDefinitions.Library3.Sound08DoorClosed,
        0x09 => SpcSoundStreamDefinitions.Library3.Sound09MissileDoorShotWithMissile,
        0x0a => SpcSoundStreamDefinitions.Library3.Sound0AEnemyFrozen,
        0x0b => SpcSoundStreamDefinitions.Library3.Sound0BElevator,
        0x0c => SpcSoundStreamDefinitions.Library3.Sound0CStoredShinespark,
        0x0d => SpcSoundStreamDefinitions.Library3.Sound0DTypewriterStrokeIntro,
        0x0e => SpcSoundStreamDefinitions.Library3.Sound0EGateOpeningClosing,
        0x0f => SpcSoundStreamDefinitions.Library3.Sound0FShinespark,
        0x10 => SpcSoundStreamDefinitions.Library3.Sound10ShinesparkEnded,
        0x11 => SpcSoundStreamDefinitions.Library3.Sound11,
        0x12 => SpcSoundStreamDefinitions.Library3.Sound12Empty,
        0x13 => SpcSoundStreamDefinitions.Library3.Sound13MotherBrainSProjectileHitsSurface,
        0x14 => SpcSoundStreamDefinitions.Library3.Sound14GunshipElevatorActivated,
        0x15 => SpcSoundStreamDefinitions.Library3.Sound15GunshipElevatorDeactivated,
        0x16 => SpcSoundStreamDefinitions.Library3.Sound16,
        0x17 => SpcSoundStreamDefinitions.Library3.Sound17MotherBrainSBlueRings,
        0x18 => SpcSoundStreamDefinitions.Library3.Sound18Empty,
        0x19 => SpcSoundStreamDefinitions.Library3.Sound19,
        0x1a => SpcSoundStreamDefinitions.Library3.Sound1AEmpty,
        0x1b => SpcSoundStreamDefinitions.Library3.Sound1B,
        0x1c => SpcSoundStreamDefinitions.Library3.Sound1C,
        0x1d => SpcSoundStreamDefinitions.Library3.Sound1D,
        0x1e => SpcSoundStreamDefinitions.Library3.Sound1EEarthquakeKraid,
        0x1f => SpcSoundStreamDefinitions.Library3.Sound1F,
        0x20 => SpcSoundStreamDefinitions.Library3.Sound20Empty,
        0x21 => SpcSoundStreamDefinitions.Library3.Sound21RidleyWhipsItsTail,
        0x22 => SpcSoundStreamDefinitions.Library3.Sound22,
        0x23 => SpcSoundStreamDefinitions.Library3.Sound23BabyMetroidCry1,
        0x24 => SpcSoundStreamDefinitions.Library3.Sound24BabyMetroidCryCeres,
        0x25 => SpcSoundStreamDefinitions.Library3.Sound25SilenceClearSpeedBoosterElevatorSound,
        0x26 => SpcSoundStreamDefinitions.Library3.Sound26BabyMetroidCry2,
        0x27 => SpcSoundStreamDefinitions.Library3.Sound27BabyMetroidCry3,
        0x28 => SpcSoundStreamDefinitions.Library3.Sound28,
        0x29 => SpcSoundStreamDefinitions.Library3.Sound29PhantoonRelated,
        0x2a => SpcSoundStreamDefinitions.Library3.Sound2APauseMenuAmbientBeep,
        0x2b => SpcSoundStreamDefinitions.Library3.Sound2B,
        0x2c => SpcSoundStreamDefinitions.Library3.Sound2CCeresDoorOpening,
        0x2d => SpcSoundStreamDefinitions.Library3.Sound2DGainingLosingIncrementalHealth,
        0x2e => SpcSoundStreamDefinitions.Library3.Sound2EMotherBrainSGlassShattering,
        0x2f => SpcSoundStreamDefinitions.Library3.Sound2FEmpty,
        _ => throw new IndexOutOfRangeException(),
    };

    /// <summary>
    /// Dispatches one-based CPU sound commands to their native voice-allocation policy.
    /// Native jump tables at SPC $1F4D/$31B1/$4776 choose handlers, not numeric samples.
    /// The byte result preserves the extracted audio manifest's library-relative policy identity.
    /// </summary>
    internal static byte Configuration(int libraryIndex, int command) => libraryIndex switch
    {
        0 => (byte)Library1Configuration(command),
        1 => (byte)Library2Configuration(command),
        2 => (byte)Library3Configuration(command),
        _ => throw new IndexOutOfRangeException(),
    };
    private static SpcLibrary1Policy Library1Configuration(int command) => command switch
    {
        0x02 or 0x03 or 0x04 or 0x05 or 0x06 or 0x07 or 0x09 or 0x0a or
        0x0b or 0x0c or 0x0d or 0x0e or 0x0f or 0x10 or 0x11 or 0x12 or
        0x13 or 0x14 or 0x15 or 0x16 or 0x17 or 0x18 or 0x19 or 0x1a or
        0x1b or 0x1c or 0x1d or 0x1e or 0x1f or 0x20 or 0x21 or 0x22 or
        0x23 or 0x25 or 0x26 or 0x28 or 0x29 or 0x2a or 0x2b or 0x2c or
        0x2d or 0x2f or 0x30 or 0x31 or 0x32 or 0x34 or 0x36 or 0x37 or
        0x38 or 0x39 or 0x3a or 0x3b or 0x3c or 0x3d or 0x3e or 0x3f => SpcLibrary1Policy.OneVoiceLowPriority,
        0x35 => SpcLibrary1Policy.OneVoiceHighPriority,
        0x08 or 0x24 or 0x27 or 0x33 or 0x41 or 0x42 => SpcLibrary1Policy.TwoVoicesLowPriority,
        0x40 => SpcLibrary1Policy.ThreeVoicesHighPriority,
        0x2e => SpcLibrary1Policy.FourVoicesLowPriority,
        0x01 => SpcLibrary1Policy.PowerBombFourVoices,
        _ => throw new IndexOutOfRangeException(),
    };

    private static SpcLibrary2Policy Library2Configuration(int command) => command switch
    {
        0x06 or 0x07 or 0x08 or 0x09 or 0x0a or 0x0b or 0x0c or 0x0d or
        0x0e or 0x0f or 0x10 or 0x11 or 0x12 or 0x13 or 0x14 or 0x15 or
        0x18 or 0x1b or 0x1d or 0x20 or 0x21 or 0x22 or 0x23 or 0x24 or
        0x25 or 0x26 or 0x28 or 0x29 or 0x2a or 0x2b or 0x2f or 0x30 or
        0x31 or 0x32 or 0x33 or 0x34 or 0x36 or 0x39 or 0x3a or 0x3b or
        0x3c or 0x3d or 0x3e or 0x3f or 0x40 or 0x41 or 0x42 or 0x43 or
        0x44 or 0x45 or 0x47 or 0x48 or 0x49 or 0x4a or 0x4b or 0x4c or
        0x4d or 0x4f or 0x52 or 0x53 or 0x55 or 0x57 or 0x5b or 0x5c or
        0x5d or 0x5e or 0x5f or 0x60 or 0x61 or 0x62 or 0x64 or 0x65 or
        0x66 or 0x67 or 0x68 or 0x69 or 0x6a or 0x6b or 0x6c or 0x6d or
        0x70 or 0x71 or 0x76 => SpcLibrary2Policy.OneVoiceLowPriority,
        0x01 or 0x02 or 0x03 or 0x04 or 0x05 or 0x16 or 0x17 or 0x1c or
        0x1f or 0x2d or 0x35 or 0x46 or 0x54 or 0x7c or 0x7d or 0x7e => SpcLibrary2Policy.OneVoiceHighPriority,
        0x19 or 0x1a or 0x37 or 0x38 or 0x56 or 0x58 or 0x5a or 0x63 or
        0x78 or 0x79 or 0x7a or 0x7b or 0x7f => SpcLibrary2Policy.TwoVoicesLowPriority,
        0x1e or 0x27 or 0x2c or 0x2e or 0x4e or 0x50 or 0x51 or 0x59 or
        0x6e or 0x6f or 0x72 or 0x73 or 0x74 or 0x75 or 0x77 => SpcLibrary2Policy.TwoVoicesHighPriority,
        _ => throw new IndexOutOfRangeException(),
    };

    private static SpcLibrary3Policy Library3Configuration(int command) => command switch
    {
        0x01 => SpcLibrary3Policy.CancelAndClearLowHealthMode,
        0x02 => SpcLibrary3Policy.LowHealthModePreservePriority,
        0x03 or 0x06 or 0x09 or 0x0c or 0x0d or 0x10 or 0x11 or 0x13 or
        0x16 or 0x17 or 0x18 or 0x1a or 0x1c or 0x1d or 0x1e or 0x1f or
        0x22 or 0x23 or 0x25 or 0x26 or 0x27 or 0x28 or 0x29 or 0x2a or
        0x2b or 0x2d or 0x2f => SpcLibrary3Policy.OneVoiceLowPriority,
        0x04 or 0x05 or 0x0b or 0x0f => SpcLibrary3Policy.TwoVoicesLowPriority,
        0x07 or 0x08 or 0x0e or 0x14 or 0x15 or 0x19 or 0x1b or 0x2c or
        0x2e => SpcLibrary3Policy.TwoVoicesHighPriority,
        0x0a or 0x12 or 0x20 or 0x21 or 0x24 => SpcLibrary3Policy.OneVoiceHighPriority,
        _ => throw new IndexOutOfRangeException(),
    };

    /// <summary>
    /// SPC allocation structure layout: contiguous per-channel bitsets and masks,
    /// then one intervening byte before the per-channel voice indices.
    /// Library bases are $03A6, $0446 and $047E; channel counts are four, two and two.
    /// </summary>
    internal static ushort AllocationStateAddress(int libraryIndex, SpcAllocationField field)
    {
        (int baseAddress, int channelCount) = libraryIndex switch
        {
            0 => (0x03a6, 4),
            1 => (0x0446, 2),
            2 => (0x047e, 2),
            _ => throw new IndexOutOfRangeException(),
        };
        int offset = field switch
        {
            SpcAllocationField.VoiceBitset => 0,
            SpcAllocationField.ChannelMask => channelCount,
            SpcAllocationField.VoiceIndex => 2 * channelCount + 1,
            _ => throw new IndexOutOfRangeException(),
        };
        return (ushort)(baseAddress + offset);
    }

    /// <summary>Number of channel programs required by a named native allocation policy.</summary>
    /// <remarks>#1165: matches the voice-count writes in the handlers catalogued by
    /// SpcLibrary1Policy/SpcLibrary2Policy/SpcLibrary3Policy. Library-three cancellation
    /// and low-health mode both allocate one voice. Unknown manifest bytes are rejected,
    /// and unsupported library indices preserve ArgumentOutOfRangeException.</remarks>
    internal static int GetVoiceCount(int libraryIndex, byte configuration) => libraryIndex switch
    {
        0 => (SpcLibrary1Policy)configuration switch
        {
            SpcLibrary1Policy.OneVoiceLowPriority or SpcLibrary1Policy.OneVoiceHighPriority => 1,
            SpcLibrary1Policy.TwoVoicesLowPriority => 2,
            SpcLibrary1Policy.ThreeVoicesHighPriority => 3,
            SpcLibrary1Policy.FourVoicesLowPriority or SpcLibrary1Policy.PowerBombFourVoices => 4,
            _ => throw new InvalidDataException($"Unknown SPC SFX1 configuration {configuration}."),
        },
        1 => (SpcLibrary2Policy)configuration switch
        {
            SpcLibrary2Policy.OneVoiceLowPriority or SpcLibrary2Policy.OneVoiceHighPriority => 1,
            SpcLibrary2Policy.TwoVoicesLowPriority or SpcLibrary2Policy.TwoVoicesHighPriority => 2,
            _ => throw new InvalidDataException($"Unknown SPC SFX2 configuration {configuration}."),
        },
        2 => (SpcLibrary3Policy)configuration switch
        {
            SpcLibrary3Policy.CancelAndClearLowHealthMode or SpcLibrary3Policy.LowHealthModePreservePriority or
                SpcLibrary3Policy.OneVoiceLowPriority or SpcLibrary3Policy.OneVoiceHighPriority => 1,
            SpcLibrary3Policy.TwoVoicesLowPriority or SpcLibrary3Policy.TwoVoicesHighPriority => 2,
            _ => throw new InvalidDataException($"Unknown SPC SFX3 configuration {configuration}."),
        },
        _ => throw new ArgumentOutOfRangeException(nameof(libraryIndex)),
    };
}

/// <summary>Distinct per-channel fields in the native SPC voice-allocation structures.</summary>
internal enum SpcAllocationField
{
    VoiceBitset,
    ChannelMask,
    VoiceIndex,
}
