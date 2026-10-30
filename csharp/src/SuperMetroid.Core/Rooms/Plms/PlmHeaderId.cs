namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The PLMs a room population, door, item or scripted spawner can install. The values are arbitrary and nothing
/// reads them (#1414): a family recognises its PLMs by member, and verification translates the cartridge's header
/// words through its native-word table. <see cref="None"/> marks an empty PLM slot.
/// </summary>
/// <remarks>
/// The exposed, Chozo-orb and concealed shot-block permanent items each name one PLM per
/// <see cref="InWorldCollectibleKind"/>; doors name one PLM per color and facing. Mother Brain's fake-death sequence
/// requests its background-row and tube PLMs by these identities.
/// </remarks>
public enum PlmHeaderId
{
    /// <summary>An empty PLM slot.</summary>
    None = 0,

    /// <summary>The <c>nothing</c> PLM.</summary>
    Nothing,

    /// <summary>The <c>collisionReactionClearCarry</c> PLM.</summary>
    CollisionReactionClearCarry,

    /// <summary>Extend a horizontal block chain toward increasing X.</summary>
    RightwardsScrollExtension,

    /// <summary>Extend a horizontal block chain toward decreasing X.</summary>
    LeftwardsScrollExtension,

    /// <summary>Extend a vertical block chain toward increasing Y.</summary>
    DownwardsScrollExtension,

    /// <summary>Extend a vertical block chain toward decreasing Y.</summary>
    UpwardsScrollExtension,

    /// <summary>Wrecked Ship entrance treadmill entered from the west.</summary>
    WreckedShipEntranceTreadmillFromWest,

    /// <summary>Wrecked Ship entrance treadmill entered from the east.</summary>
    WreckedShipEntranceTreadmillFromEast,

    /// <summary>The first of three inside reactions that do nothing.</summary>
    InsideReactionNothingFirst,

    /// <summary>The second of three inside reactions that do nothing.</summary>
    InsideReactionNothingSecond,

    /// <summary>The third of three inside reactions that do nothing.</summary>
    InsideReactionNothingThird,

    /// <summary>Fill Mother Brain's wall.</summary>
    FillMotherBrainsWall,

    /// <summary>Open Mother Brain's escape door.</summary>
    MotherBrainsRoomEscapeDoor,

    /// <summary>The <c>motherBrainsBackgroundRow2</c> PLM.</summary>
    MotherBrainsBackgroundRow2,

    /// <summary>The <c>motherBrainsBackgroundRow3</c> PLM.</summary>
    MotherBrainsBackgroundRow3,

    /// <summary>The <c>motherBrainsBackgroundRow4</c> PLM.</summary>
    MotherBrainsBackgroundRow4,

    /// <summary>The <c>motherBrainsBackgroundRow5</c> PLM.</summary>
    MotherBrainsBackgroundRow5,

    /// <summary>The <c>motherBrainsBackgroundRow6</c> PLM.</summary>
    MotherBrainsBackgroundRow6,

    /// <summary>The <c>motherBrainsBackgroundRow7</c> PLM.</summary>
    MotherBrainsBackgroundRow7,

    /// <summary>The <c>motherBrainsBackgroundRow8</c> PLM.</summary>
    MotherBrainsBackgroundRow8,

    /// <summary>The <c>motherBrainsBackgroundRow9</c> PLM.</summary>
    MotherBrainsBackgroundRow9,

    /// <summary>The <c>motherBrainsBackgroundRowA</c> PLM.</summary>
    MotherBrainsBackgroundRowA,

    /// <summary>The <c>motherBrainsBackgroundRowB</c> PLM.</summary>
    MotherBrainsBackgroundRowB,

    /// <summary>The <c>motherBrainsBackgroundRowC</c> PLM.</summary>
    MotherBrainsBackgroundRowC,

    /// <summary>The <c>motherBrainsBackgroundRowD</c> PLM.</summary>
    MotherBrainsBackgroundRowD,

    /// <summary>The <c>clearCeilingBlockInMotherBrainsRoom</c> PLM.</summary>
    ClearMotherBrainCeilingBlock,

    /// <summary>The <c>clearCeilingTubeInMotherBrainsRoom</c> PLM.</summary>
    ClearMotherBrainCeilingTube,

    /// <summary>The <c>clearMotherBrainsBottomMiddleSideTube</c> PLM.</summary>
    ClearMotherBrainBottomMiddleSideTube,

    /// <summary>The <c>clearMotherBrainsBottomMiddleTubes</c> PLM.</summary>
    ClearMotherBrainBottomMiddleTubes,

    /// <summary>The <c>clearMotherBrainsBottomLeftTube</c> PLM.</summary>
    ClearMotherBrainBottomLeftTube,

    /// <summary>The <c>clearMotherBrainsBottomRightTube</c> PLM.</summary>
    ClearMotherBrainBottomRightTube,

    /// <summary>The <c>insideReactionBrinstarFloorPlant</c> PLM.</summary>
    InsideReactionBrinstarFloorPlant,

    /// <summary>The <c>insideReactionBrinstarCeilingPlant</c> PLM.</summary>
    InsideReactionBrinstarCeilingPlant,

    /// <summary>Main map-station room actor.</summary>
    MapStation,

    /// <summary>The <c>mapStationRightAccess</c> PLM.</summary>
    MapStationRightAccess,

    /// <summary>The <c>mapStationLeftAccess</c> PLM.</summary>
    MapStationLeftAccess,

    /// <summary>The <c>energyStation</c> PLM.</summary>
    EnergyStation,

    /// <summary>The <c>energyStationRightAccess</c> PLM.</summary>
    EnergyStationRightAccess,

    /// <summary>The <c>energyStationLeftAccess</c> PLM.</summary>
    EnergyStationLeftAccess,

    /// <summary>The <c>missileStation</c> PLM.</summary>
    MissileStation,

    /// <summary>The <c>missileStationRightAccess</c> PLM.</summary>
    MissileStationRightAccess,

    /// <summary>The <c>missileStationLeftAccess</c> PLM.</summary>
    MissileStationLeftAccess,

    /// <summary>A second PLM that does nothing.</summary>
    NothingSecond,

    /// <summary>A third PLM that does nothing.</summary>
    NothingThird,

    /// <summary>The <c>scrollPLMTrigger</c> PLM.</summary>
    ScrollTriggerCollision,

    /// <summary>Resident special-air scroll trigger.</summary>
    ScrollTrigger,

    /// <summary>The <c>unusedSolidScrollPLM</c> PLM.</summary>
    UnusedSolidScrollPLM,

    /// <summary>Ordinary elevator-platform room actor.</summary>
    ElevatorPlatform,

    /// <summary>The <c>insideReactionCrateria80</c> PLM.</summary>
    InsideReactionCrateria80,

    /// <summary>The <c>insideReactionQuicksandSurface</c> PLM.</summary>
    InsideReactionQuicksandSurface,

    /// <summary>The <c>insideReactionSubmergingQuicksand</c> PLM.</summary>
    InsideReactionSubmergingQuicksand,

    /// <summary>The <c>insideReactionSandFallsSlow</c> PLM.</summary>
    InsideReactionSandFallsSlow,

    /// <summary>The <c>insideReactionSandFallsFast</c> PLM.</summary>
    InsideReactionSandFallsFast,

    /// <summary>The <c>collisionReactionQuicksandSurface</c> PLM.</summary>
    CollisionReactionQuicksandSurface,

    /// <summary>The <c>collisionReactionSubmergingQuicksand</c> PLM.</summary>
    CollisionReactionSubmergingQuicksand,

    /// <summary>The <c>collisionReactionSandFallsSlow</c> PLM.</summary>
    CollisionReactionSandFallsSlow,

    /// <summary>The <c>collisionReactionSandFallsFast</c> PLM.</summary>
    CollisionReactionSandFallsFast,

    /// <summary>Clear Crocomire bridge PLM.</summary>
    ClearCrocomireBridge,

    /// <summary>Crumble one Crocomire bridge block PLM.</summary>
    CrumbleCrocomireBridgeBlock,

    /// <summary>Clear one Crocomire bridge block PLM.</summary>
    ClearCrocomireBridgeBlock,

    /// <summary>Clear Crocomire's invisible wall PLM.</summary>
    ClearCrocomireInvisibleWall,

    /// <summary>Create Crocomire's invisible wall PLM.</summary>
    CreateCrocomireInvisibleWall,

    /// <summary>The <c>unusedDraw13BlankAirTiles</c> PLM.</summary>
    UnusedDraw13BlankAirTiles,

    /// <summary>The <c>unusedDraw13BlankSolidTiles</c> PLM.</summary>
    UnusedDraw13BlankSolidTiles,

    /// <summary>Clear the Baby Metroid encounter's invisible wall.</summary>
    ClearBabyMetroidInvisibleWall,

    /// <summary>Create the Baby Metroid encounter's invisible wall.</summary>
    CreateBabyMetroidInvisibleWall,

    /// <summary>The <c>saveStationTrigger</c> PLM.</summary>
    SaveStationTrigger,

    /// <summary>Main save-station room actor.</summary>
    SaveStation,

    /// <summary>The <c>crumbleAccessToTourianElevator</c> PLM.</summary>
    CrumbleAccessToTourianElevator,

    /// <summary>The <c>clearAccessToTourianElevator</c> PLM.</summary>
    ClearAccessToTourianElevator,

    /// <summary>Draw Phantoon's closed arena door.</summary>
    DrawPhantoonDoorDuringBossFight,

    /// <summary>Restore Phantoon's door after the fight.</summary>
    RestorePhantoonDoorAfterBossFight,

    /// <summary>Crumble Spore Spawn's ceiling PLM.</summary>
    CrumbleSporeSpawnCeiling,

    /// <summary>Clear Spore Spawn's ceiling PLM.</summary>
    ClearSporeSpawnCeiling,

    /// <summary>Clear Botwoon's wall PLM.</summary>
    ClearBotwoonWall,

    /// <summary>Crumble Botwoon's wall PLM.</summary>
    CrumbleBotwoonWall,

    /// <summary>The <c>unusedSetKraidCeilingBlockToBackground1</c> PLM.</summary>
    UnusedSetKraidCeilingBlockToBackground1,

    /// <summary>Kraid ceiling crumble using background variant one.</summary>
    CrumbleKraidCeilingIntoBackground1,

    /// <summary>Kraid platform crumble using the first floor variant.</summary>
    CrumbleKraidPlatformVariant1,

    /// <summary>Kraid ceiling crumble using background variant two.</summary>
    CrumbleKraidCeilingIntoBackground2,

    /// <summary>Kraid platform crumble using the second floor variant.</summary>
    CrumbleKraidPlatformVariant2,

    /// <summary>Kraid ceiling crumble using background variant three.</summary>
    CrumbleKraidCeilingIntoBackground3,

    /// <summary>Clear Kraid's ceiling after an already-defeated room load.</summary>
    ClearKraidCeiling,

    /// <summary>Clear Kraid-room spikes after an already-defeated room load.</summary>
    ClearKraidSpikes,

    /// <summary>The <c>crumbleKraidSpikeBlocks</c> PLM: animated live-death floor sweep.</summary>
    CrumbleKraidSpikes,

    /// <summary>The <c>enableSoundsIn20Frames_F0FramesIfCeres</c> PLM.</summary>
    EnableSoundsIn20FramesF0FramesIfCeres,

    /// <summary>Speed Booster escape lavaquake controller.</summary>
    SpeedBoosterEscape,

    /// <summary>The <c>shaktoolsRoom</c> PLM.</summary>
    ShaktoolsRoom,

    /// <summary>Maridia elevatube delay/sound PLM.</summary>
    MaridiaElevatube,

    /// <summary>
    /// Old Tourian escape shaft fake wall, spawned by its room's setup during
    /// the escape; explodes once Samus passes below and right of it.
    /// </summary>
    OldTourianEscapeShaftFakeWall,

    /// <summary>The <c>RaiseAcidInEscapeRoomBeforeOldTourianEscapeShaft</c> PLM.</summary>
    RaiseAcidInEscapeRoomBeforeOldTourianEscapeShaft,

    /// <summary>The <c>gateBlock</c> PLM.</summary>
    GateBlock,

    /// <summary>The <c>Reaction_CrittersEscapeBlock</c> PLM.</summary>
    ReactionCrittersEscapeBlock,

    /// <summary>The <c>CrittersEscapeBlock</c> PLM.</summary>
    CrittersEscapeBlock,

    /// <summary>The <c>turnCeresElevatorDoorToSolidBlocksDuringEscape</c> PLM.</summary>
    TurnCeresElevatorDoorToSolidBlocksDuringEscape,

    /// <summary>Bomb Torizo's exceptional right-facing grey door.</summary>
    BombTorizoGreyDoor,

    /// <summary>Resident Wrecked Ship attic no-op observer.</summary>
    WreckedShipAttic,

    /// <summary>
    /// Crateria mainstreet escape-passage clearer, spawned by its room's setup;
    /// its setup deletes it unless the critters escaped.
    /// </summary>
    CrateriaMainstreetEscapePassage,

    /// <summary>The <c>leftGreenGateTrigger</c> PLM.</summary>
    LeftGreenGateTrigger,

    /// <summary>The <c>rightGreenGateTrigger</c> PLM.</summary>
    RightGreenGateTrigger,

    /// <summary>The <c>leftRedGateTrigger</c> PLM.</summary>
    LeftRedGateTrigger,

    /// <summary>The <c>rightRedGateTrigger</c> PLM.</summary>
    RightRedGateTrigger,

    /// <summary>The <c>leftBlueGateTrigger</c> PLM.</summary>
    LeftBlueGateTrigger,

    /// <summary>The <c>rightBlueGateTrigger</c> PLM.</summary>
    RightBlueGateTrigger,

    /// <summary>The <c>leftYellowGateTrigger</c> PLM.</summary>
    LeftYellowGateTrigger,

    /// <summary>The <c>rightYellowGateTrigger</c> PLM.</summary>
    RightYellowGateTrigger,

    /// <summary>The <c>downwardsOpenGate</c> PLM.</summary>
    DownwardsOpenGate,

    /// <summary>Resident five-block downward gate.</summary>
    DownwardGate,

    /// <summary>The <c>upwardsOpenGate</c> PLM.</summary>
    UpwardsOpenGate,

    /// <summary>The <c>upwardsClosedGate</c> PLM.</summary>
    UpwardsClosedGate,

    /// <summary>Room-authored downward-gate shot block.</summary>
    DownwardGateShotBlock,

    /// <summary>The <c>upwardsGateShotblock</c> PLM.</summary>
    UpwardsGateShotblock,

    /// <summary>The <c>genericShotTriggerForAPLM</c> PLM.</summary>
    GenericShotTriggerForAPLM,

    /// <summary>The <c>greyDoorFacingLeft</c> PLM.</summary>
    GreyDoorFacingLeft,

    /// <summary>The <c>greyDoorFacingRight</c> PLM.</summary>
    GreyDoorFacingRight,

    /// <summary>The <c>greyDoorFacingUp</c> PLM.</summary>
    GreyDoorFacingUp,

    /// <summary>The <c>greyDoorFacingDown</c> PLM.</summary>
    GreyDoorFacingDown,

    /// <summary>The <c>yellowDoorFacingLeft</c> PLM.</summary>
    YellowDoorFacingLeft,

    /// <summary>The <c>yellowDoorFacingRight</c> PLM.</summary>
    YellowDoorFacingRight,

    /// <summary>The <c>yellowDoorFacingUp</c> PLM.</summary>
    YellowDoorFacingUp,

    /// <summary>The <c>yellowDoorFacingDown</c> PLM.</summary>
    YellowDoorFacingDown,

    /// <summary>The <c>greenDoorFacingLeft</c> PLM.</summary>
    GreenDoorFacingLeft,

    /// <summary>The <c>greenDoorFacingRight</c> PLM.</summary>
    GreenDoorFacingRight,

    /// <summary>The <c>greenDoorFacingUp</c> PLM.</summary>
    GreenDoorFacingUp,

    /// <summary>The <c>greenDoorFacingDown</c> PLM.</summary>
    GreenDoorFacingDown,

    /// <summary>The <c>redDoorFacingLeft</c> PLM.</summary>
    RedDoorFacingLeft,

    /// <summary>The <c>redDoorFacingRight</c> PLM.</summary>
    RedDoorFacingRight,

    /// <summary>The <c>redDoorFacingUp</c> PLM.</summary>
    RedDoorFacingUp,

    /// <summary>The <c>redDoorFacingDown</c> PLM.</summary>
    RedDoorFacingDown,

    /// <summary>The <c>blueDoorFacingLeft</c> PLM.</summary>
    BlueDoorFacingLeft,

    /// <summary>The <c>blueDoorFacingRight</c> PLM.</summary>
    BlueDoorFacingRight,

    /// <summary>The <c>blueDoorFacingUp</c> PLM.</summary>
    BlueDoorFacingUp,

    /// <summary>The <c>blueDoorFacingDown</c> PLM.</summary>
    BlueDoorFacingDown,

    /// <summary>Door-transition-only blue-door closer facing left.</summary>
    BlueDoorClosingFacingLeft,

    /// <summary>Door-transition-only blue-door closer facing right.</summary>
    BlueDoorClosingFacingRight,

    /// <summary>Door-transition-only blue-door closer facing up.</summary>
    BlueDoorClosingFacingUp,

    /// <summary>Door-transition-only blue-door closer facing down.</summary>
    BlueDoorClosingFacingDown,

    /// <summary>
    /// Resident gate in Tourian escape room one. Its second header list is
    /// selected by the shared door-transition closer when entering from Mother Brain.
    /// </summary>
    MotherBrainEscapeRoomGate,

    /// <summary>
    /// Door-transition-only fallback gate closer, used when a special
    /// direction-$8..$B door has no resident cap at its authored block coordinate.
    /// </summary>
    MotherBrainEscapeRoomGateClosing,

    /// <summary>The <c>1x1RespawningCrumbleBlock</c> PLM.</summary>
    Plm1x1RespawningCrumbleBlock,

    /// <summary>The <c>2x1RespawningCrumbleBlock</c> PLM.</summary>
    Plm2x1RespawningCrumbleBlock,

    /// <summary>The <c>1x2RespawningCrumbleBlock</c> PLM.</summary>
    Plm1x2RespawningCrumbleBlock,

    /// <summary>The <c>2x2RespawningCrumbleBlock</c> PLM.</summary>
    Plm2x2RespawningCrumbleBlock,

    /// <summary>The <c>BombReaction_SpeedBoostBlock</c> PLM.</summary>
    BombReactionSpeedBoostBlock,

    /// <summary>
    /// The <c>Collision_BTS82</c> PLM: Brinstar behaviour <c>$82</c>, a
    /// respawning Speed Booster block with the slower crumble animation.
    /// </summary>
    SpeedBlockBrinstarSlowRespawning,

    /// <summary>
    /// The <c>Collision_BTS83</c> PLM: Brinstar behaviour <c>$83</c>, a
    /// permanent Speed Booster block with the slower crumble animation.
    /// </summary>
    SpeedBlockBrinstarSlowPermanent,

    /// <summary>
    /// The <c>Collision_RespawningSpeedBoostBlock</c> PLM: the
    /// area-independent behaviour <c>$0E</c> respawning Speed Booster block.
    /// </summary>
    SpeedBlockRespawning,

    /// <summary>
    /// The <c>Collision_DachoraRespawningSpeedBoostBlock</c> PLM:
    /// Brinstar behaviour <c>$84</c>, the Dachora-room respawning Speed Booster block.
    /// </summary>
    SpeedBlockDachoraRespawning,

    /// <summary>
    /// The <c>Collision_SpeedBoostBlock</c> PLM: area-independent
    /// Behaviour <c>$0F</c> and Brinstar behaviour <c>$85</c>, a permanent Speed Booster block.
    /// </summary>
    SpeedBlockPermanent,

    /// <summary>Respawning 1x1 Samus-contact crumble block.</summary>
    ContactCrumble1x1Respawning,

    /// <summary>Respawning 2x1 Samus-contact crumble block.</summary>
    ContactCrumble2x1Respawning,

    /// <summary>Respawning 1x2 Samus-contact crumble block.</summary>
    ContactCrumble1x2Respawning,

    /// <summary>Respawning 2x2 Samus-contact crumble block.</summary>
    ContactCrumble2x2Respawning,

    /// <summary>Permanent 1x1 Samus-contact crumble block.</summary>
    ContactCrumble1x1Permanent,

    /// <summary>Permanent 2x1 Samus-contact crumble block.</summary>
    ContactCrumble2x1Permanent,

    /// <summary>Permanent 1x2 Samus-contact crumble block.</summary>
    ContactCrumble1x2Permanent,

    /// <summary>Permanent 2x2 Samus-contact crumble block.</summary>
    ContactCrumble2x2Permanent,

    /// <summary>The <c>Reaction_1x1RespawningShotBlock</c> PLM.</summary>
    Reaction1x1RespawningShotBlock,

    /// <summary>The <c>Reaction_2x1RespawningShotBlock</c> PLM.</summary>
    Reaction2x1RespawningShotBlock,

    /// <summary>The <c>Reaction_1x2RespawningShotBlock</c> PLM.</summary>
    Reaction1x2RespawningShotBlock,

    /// <summary>The <c>Reaction_2x2RespawningShotBlock</c> PLM.</summary>
    Reaction2x2RespawningShotBlock,

    /// <summary>The <c>Reaction_1x1ShotBlock</c> PLM.</summary>
    Reaction1x1ShotBlock,

    /// <summary>The <c>Reaction_2x1ShotBlock</c> PLM.</summary>
    Reaction2x1ShotBlock,

    /// <summary>The <c>Reaction_1x2ShotBlock</c> PLM.</summary>
    Reaction1x2ShotBlock,

    /// <summary>The <c>Reaction_2x2ShotBlock</c> PLM.</summary>
    Reaction2x2ShotBlock,

    /// <summary>The <c>Reaction_RespawningPowerBombBlock</c> PLM.</summary>
    ReactionRespawningPowerBombBlock,

    /// <summary>The <c>Reaction_PowerBombBlock</c> PLM.</summary>
    ReactionPowerBombBlock,

    /// <summary>The <c>Reaction_RespawningSuperMissileBlock</c> PLM.</summary>
    ReactionRespawningSuperMissileBlock,

    /// <summary>The <c>Reaction_SuperMissileBlock</c> PLM.</summary>
    ReactionSuperMissileBlock,

    /// <summary>The <c>EnemyBreakableBlock</c> PLM.</summary>
    EnemyBreakableBlock,

    /// <summary>The <c>Collision_1x1RespawningBombBlock</c> PLM.</summary>
    Collision1x1RespawningBombBlock,

    /// <summary>The <c>Collision_2x1RespawningBombBlock</c> PLM.</summary>
    Collision2x1RespawningBombBlock,

    /// <summary>The <c>Collision_1x2RespawningBombBlock</c> PLM.</summary>
    Collision1x2RespawningBombBlock,

    /// <summary>The <c>Collision_2x2RespawningBombBlock</c> PLM.</summary>
    Collision2x2RespawningBombBlock,

    /// <summary>The <c>Collision_1x1BombBlock</c> PLM.</summary>
    Collision1x1BombBlock,

    /// <summary>The <c>Collision_2x1BombBlock</c> PLM.</summary>
    Collision2x1BombBlock,

    /// <summary>The <c>Collision_1x2BombBlock</c> PLM.</summary>
    Collision1x2BombBlock,

    /// <summary>The <c>Collision_2x2BombBlock</c> PLM.</summary>
    Collision2x2BombBlock,

    /// <summary>The <c>Reaction_1x1RespawningBombBlock</c> PLM.</summary>
    Reaction1x1RespawningBombBlock,

    /// <summary>The <c>Reaction_2x1RespawningBombBlock</c> PLM.</summary>
    Reaction2x1RespawningBombBlock,

    /// <summary>The <c>Reaction_1x2RespawningBombBlock</c> PLM.</summary>
    Reaction1x2RespawningBombBlock,

    /// <summary>The <c>Reaction_2x2RespawningBombBlock</c> PLM.</summary>
    Reaction2x2RespawningBombBlock,

    /// <summary>The <c>Reaction_1x1BombBlock</c> PLM.</summary>
    Reaction1x1BombBlock,

    /// <summary>The <c>Reaction_2x1BombBlock</c> PLM.</summary>
    Reaction2x1BombBlock,

    /// <summary>The <c>Reaction_1x2BombBlock</c> PLM.</summary>
    Reaction1x2BombBlock,

    /// <summary>The <c>Reaction_2x2BombBlock</c> PLM.</summary>
    Reaction2x2BombBlock,

    /// <summary>The <c>Grappled_GrappleBlock</c> PLM.</summary>
    GrappledGrappleBlock,

    /// <summary>The <c>Grappled_RespawningBreakableGrappleBlock</c> PLM.</summary>
    GrappledRespawningBreakableGrappleBlock,

    /// <summary>The <c>Grappled_BreakableGrappleBlock</c> PLM.</summary>
    GrappledBreakableGrappleBlock,

    /// <summary>The <c>Grappled_GenericSpikeBlock</c> PLM.</summary>
    GrappledGenericSpikeBlock,

    /// <summary>The <c>Grappled_DraygonsBrokenTurret</c> PLM.</summary>
    GrappledDraygonsBrokenTurret,

    /// <summary>The <c>UnusedBlueBrinstarFaceBlock</c> PLM.</summary>
    UnusedBlueBrinstarFaceBlock,

    /// <summary>The <c>CrumbleLowerNorfairChozoRoomPlug</c> PLM.</summary>
    CrumbleLowerNorfairChozoRoomPlug,

    /// <summary>The <c>UnusedShotBlock</c> PLM.</summary>
    UnusedShotBlock,

    /// <summary>The <c>UnusedGrappleBlock</c> PLM.</summary>
    UnusedGrappleBlock,

    /// <summary>The <c>LowerNorfairChozoHand</c> PLM.</summary>
    LowerNorfairChozoHand,

    /// <summary>The <c>Collision_LowerNorfairChozoHandCheck</c> PLM.</summary>
    CollisionLowerNorfairChozoHandCheck,

    /// <summary>Mother Brain's missile-reactive glass actor.</summary>
    MotherBrainGlass,

    /// <summary>Bomb Torizo's Chozo-hand synchronization actor.</summary>
    BombTorizoHand,

    /// <summary>The <c>WreckedShipChozoHand</c> PLM.</summary>
    WreckedShipChozoHand,

    /// <summary>The <c>Collision_WreckedShipChozoHandCheck</c> PLM.</summary>
    CollisionWreckedShipChozoHandCheck,

    /// <summary>The <c>ClearSlopeAccessForWreckedShipChozo</c> PLM.</summary>
    ClearSlopeAccessForWreckedShipChozo,

    /// <summary>The <c>BlockSlopeAccessForWreckedShipChozo</c> PLM.</summary>
    BlockSlopeAccessForWreckedShipChozo,

    /// <summary>Maridia's power-bomb-reactive n00b tube actor.</summary>
    NoobTube,

    /// <summary>
    /// Resident room-kill observer which marks the four Tourian Metroid-room
    /// events when their authored enemy death quotas have been reached.
    /// </summary>
    SetMetroidsClearedStatesWhenRequired,

    /// <summary>Right-facing eye-door eye controller.</summary>
    EyeDoorEyeFacingRight,

    /// <summary>Right-facing eye-door middle/door component.</summary>
    EyeDoorFacingRight,

    /// <summary>Right-facing eye-door bottom component.</summary>
    EyeDoorBottomFacingRight,

    /// <summary>Left-facing eye-door eye controller.</summary>
    EyeDoorEyeFacingLeft,

    /// <summary>Left-facing eye-door middle/door component.</summary>
    EyeDoorFacingLeft,

    /// <summary>Left-facing eye-door bottom component.</summary>
    EyeDoorBottomFacingLeft,

    /// <summary>Draygon-room right-facing shielded cannon.</summary>
    DraygonCannonFacingRight,

    /// <summary>
    /// Draygon-room right-facing cannon. Its authored list enters the
    /// destroyed state immediately, disabling the unused upper-left firing position.
    /// </summary>
    DraygonCannonFacingRightDestroyed,

    /// <summary>Draygon-room left-facing shielded cannon.</summary>
    DraygonCannonFacingLeft,

    /// <summary>The <c>DraygonCannonFacingLeft</c> PLM.</summary>
    DraygonCannonFacingLeftUnshielded,

    /// <summary>The <c>ItemCollisionDetection</c> PLM.</summary>
    ItemCollisionDetection,

    /// <summary>The <c>EnergyTank</c> PLM.</summary>
    ExposedEnergyTank,

    /// <summary>The <c>MissileTank</c> PLM.</summary>
    ExposedMissileTank,

    /// <summary>The <c>SuperMissileTank</c> PLM.</summary>
    ExposedSuperMissileTank,

    /// <summary>The <c>PowerBombTank</c> PLM.</summary>
    ExposedPowerBombTank,

    /// <summary>The <c>Bombs</c> PLM.</summary>
    ExposedBombs,

    /// <summary>The <c>ChargeBeam</c> PLM.</summary>
    ExposedChargeBeam,

    /// <summary>The <c>IceBeam</c> PLM.</summary>
    ExposedIceBeam,

    /// <summary>The <c>HiJumpBoots</c> PLM.</summary>
    ExposedHiJumpBoots,

    /// <summary>The <c>SpeedBooster</c> PLM.</summary>
    ExposedSpeedBooster,

    /// <summary>The <c>WaveBeam</c> PLM.</summary>
    ExposedWaveBeam,

    /// <summary>The <c>Spazer</c> PLM.</summary>
    ExposedSpazerBeam,

    /// <summary>The <c>SpringBall</c> PLM.</summary>
    ExposedSpringBall,

    /// <summary>The <c>VariaSuit</c> PLM.</summary>
    ExposedVariaSuit,

    /// <summary>The <c>GravitySuit</c> PLM.</summary>
    ExposedGravitySuit,

    /// <summary>The <c>XrayScope</c> PLM.</summary>
    ExposedXrayScope,

    /// <summary>The <c>PlasmaBeam</c> PLM.</summary>
    ExposedPlasmaBeam,

    /// <summary>The <c>GrappleBeam</c> PLM.</summary>
    ExposedGrappleBeam,

    /// <summary>The <c>SpaceJump</c> PLM.</summary>
    ExposedSpaceJump,

    /// <summary>The <c>ScrewAttack</c> PLM.</summary>
    ExposedScrewAttack,

    /// <summary>The <c>MorphBall</c> PLM.</summary>
    ExposedMorphBall,

    /// <summary>The <c>ReserveTank</c> PLM.</summary>
    ExposedReserveTank,

    /// <summary>The <c>EnergyTankChozoOrb</c> PLM.</summary>
    ChozoEnergyTank,

    /// <summary>The <c>MissileTankChozoOrb</c> PLM.</summary>
    ChozoMissileTank,

    /// <summary>The <c>SuperMissileTankChozoOrb</c> PLM.</summary>
    ChozoSuperMissileTank,

    /// <summary>The <c>PowerBombTankChozoOrb</c> PLM.</summary>
    ChozoPowerBombTank,

    /// <summary>The <c>BombsChozoOrb</c> PLM.</summary>
    ChozoBombs,

    /// <summary>The <c>ChargeBeamChozoOrb</c> PLM.</summary>
    ChozoChargeBeam,

    /// <summary>The <c>IceBeamChozoOrb</c> PLM.</summary>
    ChozoIceBeam,

    /// <summary>The <c>HiJumpBootsChozoOrb</c> PLM.</summary>
    ChozoHiJumpBoots,

    /// <summary>The <c>SpeedBoosterChozoOrb</c> PLM.</summary>
    ChozoSpeedBooster,

    /// <summary>The <c>WaveBeamChozoOrb</c> PLM.</summary>
    ChozoWaveBeam,

    /// <summary>The <c>SpazerChozoOrb</c> PLM.</summary>
    ChozoSpazerBeam,

    /// <summary>The <c>SpringBallChozoOrb</c> PLM.</summary>
    ChozoSpringBall,

    /// <summary>The <c>VariaSuitChozoOrb</c> PLM.</summary>
    ChozoVariaSuit,

    /// <summary>The <c>GravitySuitChozoOrb</c> PLM.</summary>
    ChozoGravitySuit,

    /// <summary>The <c>XrayScopeChozoOrb</c> PLM.</summary>
    ChozoXrayScope,

    /// <summary>The <c>PlasmaBeamChozoOrb</c> PLM.</summary>
    ChozoPlasmaBeam,

    /// <summary>The <c>GrappleBeamChozoOrb</c> PLM.</summary>
    ChozoGrappleBeam,

    /// <summary>The <c>SpaceJumpChozoOrb</c> PLM.</summary>
    ChozoSpaceJump,

    /// <summary>The <c>ScrewAttackChozoOrb</c> PLM.</summary>
    ChozoScrewAttack,

    /// <summary>The <c>MorphBallChozoOrb</c> PLM.</summary>
    ChozoMorphBall,

    /// <summary>The <c>ReserveTankChozoOrb</c> PLM.</summary>
    ChozoReserveTank,

    /// <summary>The <c>EnergyTankShotBlock</c> PLM.</summary>
    ShotBlockEnergyTank,

    /// <summary>The <c>MissileTankShotBlock</c> PLM.</summary>
    ShotBlockMissileTank,

    /// <summary>The <c>SuperMissileTankShotBlock</c> PLM.</summary>
    ShotBlockSuperMissileTank,

    /// <summary>The <c>PowerBombTankShotBlock</c> PLM.</summary>
    ShotBlockPowerBombTank,

    /// <summary>The <c>BombsShotBlock</c> PLM.</summary>
    ShotBlockBombs,

    /// <summary>The <c>ChargeBeamShotBlock</c> PLM.</summary>
    ShotBlockChargeBeam,

    /// <summary>The <c>IceBeamShotBlock</c> PLM.</summary>
    ShotBlockIceBeam,

    /// <summary>The <c>HiJumpBootsShotBlock</c> PLM.</summary>
    ShotBlockHiJumpBoots,

    /// <summary>The <c>SpeedBoosterShotBlock</c> PLM.</summary>
    ShotBlockSpeedBooster,

    /// <summary>The <c>WaveBeamShotBlock</c> PLM.</summary>
    ShotBlockWaveBeam,

    /// <summary>The <c>SpazerShotBlock</c> PLM.</summary>
    ShotBlockSpazerBeam,

    /// <summary>The <c>SpringBallShotBlock</c> PLM.</summary>
    ShotBlockSpringBall,

    /// <summary>The <c>VariaSuitShotBlock</c> PLM.</summary>
    ShotBlockVariaSuit,

    /// <summary>The <c>GravitySuitShotBlock</c> PLM.</summary>
    ShotBlockGravitySuit,

    /// <summary>The <c>XrayScopeShotBlock</c> PLM.</summary>
    ShotBlockXrayScope,

    /// <summary>The <c>PlasmaBeamShotBlock</c> PLM.</summary>
    ShotBlockPlasmaBeam,

    /// <summary>The <c>GrappleBeamShotBlock</c> PLM.</summary>
    ShotBlockGrappleBeam,

    /// <summary>The <c>SpaceJumpShotBlock</c> PLM.</summary>
    ShotBlockSpaceJump,

    /// <summary>The <c>ScrewAttackShotBlock</c> PLM.</summary>
    ShotBlockScrewAttack,

    /// <summary>The <c>MorphBallShotBlock</c> PLM.</summary>
    ShotBlockMorphBall,

    /// <summary>The <c>ReserveTankShotBlock</c> PLM.</summary>
    ShotBlockReserveTank,
}
