namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Bank-$84 PLM header entry points: every <c>PLMEntries_*</c> record a room population,
/// door, item or scripted spawner can install. The value is the header's low word in fixed
/// bank $84; <see cref="None"/> marks an empty PLM slot.
/// </summary>
public enum PlmHeaderId : ushort
{
    /// <summary>An empty PLM slot.</summary>
    None = 0,

    /// <summary><c>PLMEntries_nothing</c> at $84:B62F.</summary>
    Nothing = 0xb62f,

    /// <summary><c>PLMEntries_collisionReactionClearCarry</c> at $84:B633.</summary>
    CollisionReactionClearCarry = 0xb633,

    /// <summary>Extend a horizontal block chain toward increasing X at $84:B63B.</summary>
    RightwardsScrollExtension = 0xb63b,

    /// <summary>Extend a horizontal block chain toward decreasing X at $84:B63F.</summary>
    LeftwardsScrollExtension = 0xb63f,

    /// <summary>Extend a vertical block chain toward increasing Y at $84:B643.</summary>
    DownwardsScrollExtension = 0xb643,

    /// <summary>Extend a vertical block chain toward decreasing Y at $84:B647.</summary>
    UpwardsScrollExtension = 0xb647,

    /// <summary>Wrecked Ship entrance treadmill entered from the west at $84:B64B.</summary>
    WreckedShipEntranceTreadmillFromWest = 0xb64b,

    /// <summary>Wrecked Ship entrance treadmill entered from the east at $84:B64F.</summary>
    WreckedShipEntranceTreadmillFromEast = 0xb64f,

    /// <summary><c>PLMEntries_insideReactionNothing_B653</c> at $84:B653.</summary>
    InsideReactionNothingB653 = 0xb653,

    /// <summary><c>PLMEntries_insideReactionNothing_B657</c> at $84:B657.</summary>
    InsideReactionNothingB657 = 0xb657,

    /// <summary><c>PLMEntries_insideReactionNothing_B65B</c> at $84:B65B.</summary>
    InsideReactionNothingB65B = 0xb65b,

    /// <summary>Fill Mother Brain's wall at $84:B673.</summary>
    FillMotherBrainsWall = 0xb673,

    /// <summary>Open Mother Brain's escape door at $84:B677.</summary>
    MotherBrainsRoomEscapeDoor = 0xb677,

    // The fake-death sequence requests these row/tube actors by exact identity. Their
    // descriptive names keep the boss state machine free of an opaque address matrix.
    /// <summary><c>PLMEntries_motherBrainsBackgroundRow2</c> at $84:B67B.</summary>
    MotherBrainsBackgroundRow2 = 0xb67b,

    /// <summary><c>PLMEntries_motherBrainsBackgroundRow3</c> at $84:B67F.</summary>
    MotherBrainsBackgroundRow3 = 0xb67f,

    /// <summary><c>PLMEntries_motherBrainsBackgroundRow4</c> at $84:B683.</summary>
    MotherBrainsBackgroundRow4 = 0xb683,

    /// <summary><c>PLMEntries_motherBrainsBackgroundRow5</c> at $84:B687.</summary>
    MotherBrainsBackgroundRow5 = 0xb687,

    /// <summary><c>PLMEntries_motherBrainsBackgroundRow6</c> at $84:B68B.</summary>
    MotherBrainsBackgroundRow6 = 0xb68b,

    /// <summary><c>PLMEntries_motherBrainsBackgroundRow7</c> at $84:B68F.</summary>
    MotherBrainsBackgroundRow7 = 0xb68f,

    /// <summary><c>PLMEntries_motherBrainsBackgroundRow8</c> at $84:B693.</summary>
    MotherBrainsBackgroundRow8 = 0xb693,

    /// <summary><c>PLMEntries_motherBrainsBackgroundRow9</c> at $84:B697.</summary>
    MotherBrainsBackgroundRow9 = 0xb697,

    /// <summary><c>PLMEntries_motherBrainsBackgroundRowA</c> at $84:B69B.</summary>
    MotherBrainsBackgroundRowA = 0xb69b,

    /// <summary><c>PLMEntries_motherBrainsBackgroundRowB</c> at $84:B69F.</summary>
    MotherBrainsBackgroundRowB = 0xb69f,

    /// <summary><c>PLMEntries_motherBrainsBackgroundRowC</c> at $84:B6A3.</summary>
    MotherBrainsBackgroundRowC = 0xb6a3,

    /// <summary><c>PLMEntries_motherBrainsBackgroundRowD</c> at $84:B6A7.</summary>
    MotherBrainsBackgroundRowD = 0xb6a7,

    /// <summary><c>PLMEntries_clearCeilingBlockInMotherBrainsRoom</c> at $84:B6B3.</summary>
    ClearMotherBrainCeilingBlock = 0xb6b3,

    /// <summary><c>PLMEntries_clearCeilingTubeInMotherBrainsRoom</c> at $84:B6B7.</summary>
    ClearMotherBrainCeilingTube = 0xb6b7,

    /// <summary><c>PLMEntries_clearMotherBrainsBottomMiddleSideTube</c> at $84:B6BB.</summary>
    ClearMotherBrainBottomMiddleSideTube = 0xb6bb,

    /// <summary><c>PLMEntries_clearMotherBrainsBottomMiddleTubes</c> at $84:B6BF.</summary>
    ClearMotherBrainBottomMiddleTubes = 0xb6bf,

    /// <summary><c>PLMEntries_clearMotherBrainsBottomLeftTube</c> at $84:B6C3.</summary>
    ClearMotherBrainBottomLeftTube = 0xb6c3,

    /// <summary><c>PLMEntries_clearMotherBrainsBottomRightTube</c> at $84:B6C7.</summary>
    ClearMotherBrainBottomRightTube = 0xb6c7,

    /// <summary><c>PLMEntries_insideReactionBrinstarFloorPlant</c> at $84:B6CB.</summary>
    InsideReactionBrinstarFloorPlant = 0xb6cb,

    /// <summary><c>PLMEntries_insideReactionBrinstarCeilingPlant</c> at $84:B6CF.</summary>
    InsideReactionBrinstarCeilingPlant = 0xb6cf,

    /// <summary>Main map-station room actor at $84:B6D3.</summary>
    MapStation = 0xb6d3,

    /// <summary><c>PLMEntries_mapStationRightAccess</c> at $84:B6D7.</summary>
    MapStationRightAccess = 0xb6d7,

    /// <summary><c>PLMEntries_mapStationLeftAccess</c> at $84:B6DB.</summary>
    MapStationLeftAccess = 0xb6db,

    /// <summary><c>PLMEntries_energyStation</c> at $84:B6DF.</summary>
    EnergyStation = 0xb6df,

    /// <summary><c>PLMEntries_energyStationRightAccess</c> at $84:B6E3.</summary>
    EnergyStationRightAccess = 0xb6e3,

    /// <summary><c>PLMEntries_energyStationLeftAccess</c> at $84:B6E7.</summary>
    EnergyStationLeftAccess = 0xb6e7,

    /// <summary><c>PLMEntries_missileStation</c> at $84:B6EB.</summary>
    MissileStation = 0xb6eb,

    /// <summary><c>PLMEntries_missileStationRightAccess</c> at $84:B6EF.</summary>
    MissileStationRightAccess = 0xb6ef,

    /// <summary><c>PLMEntries_missileStationLeftAccess</c> at $84:B6F3.</summary>
    MissileStationLeftAccess = 0xb6f3,

    /// <summary><c>PLMEntries_nothing_84B6F7</c> at $84:B6F7.</summary>
    Nothing84B6F7 = 0xb6f7,

    /// <summary><c>PLMEntries_nothing_84B6FB</c> at $84:B6FB.</summary>
    Nothing84B6FB = 0xb6fb,

    /// <summary><c>PLMEntries_scrollPLMTrigger</c> at $84:B6FF.</summary>
    ScrollTriggerCollision = 0xb6ff,

    /// <summary>Resident special-air scroll trigger at $84:B703.</summary>
    ScrollTrigger = 0xb703,

    /// <summary><c>PLMEntries_unusedSolidScrollPLM</c> at $84:B707.</summary>
    UnusedSolidScrollPLM = 0xb707,

    /// <summary>Ordinary elevator-platform room actor at $84:B70B.</summary>
    ElevatorPlatform = 0xb70b,

    /// <summary><c>PLMEntries_insideReactionCrateria80</c> at $84:B70F.</summary>
    InsideReactionCrateria80 = 0xb70f,

    /// <summary><c>PLMEntries_insideReactionQuicksandSurface</c> at $84:B713.</summary>
    InsideReactionQuicksandSurface = 0xb713,

    /// <summary><c>PLMEntries_insideReactionSubmergingQuicksand</c> at $84:B71F.</summary>
    InsideReactionSubmergingQuicksand = 0xb71f,

    /// <summary><c>PLMEntries_insideReactionSandFallsSlow</c> at $84:B723.</summary>
    InsideReactionSandFallsSlow = 0xb723,

    /// <summary><c>PLMEntries_insideReactionSandFallsFast</c> at $84:B727.</summary>
    InsideReactionSandFallsFast = 0xb727,

    /// <summary><c>PLMEntries_collisionReactionQuicksandSurface</c> at $84:B72B.</summary>
    CollisionReactionQuicksandSurface = 0xb72b,

    /// <summary><c>PLMEntries_collisionReactionSubmergingQuicksand</c> at $84:B737.</summary>
    CollisionReactionSubmergingQuicksand = 0xb737,

    /// <summary><c>PLMEntries_collisionReactionSandFallsSlow</c> at $84:B73B.</summary>
    CollisionReactionSandFallsSlow = 0xb73b,

    /// <summary><c>PLMEntries_collisionReactionSandFallsFast</c> at $84:B73F.</summary>
    CollisionReactionSandFallsFast = 0xb73f,

    /// <summary>Clear Crocomire bridge PLM at $84:B747.</summary>
    ClearCrocomireBridge = 0xb747,

    /// <summary>Crumble one Crocomire bridge block PLM at $84:B74B.</summary>
    CrumbleCrocomireBridgeBlock = 0xb74b,

    /// <summary>Clear one Crocomire bridge block PLM at $84:B74F.</summary>
    ClearCrocomireBridgeBlock = 0xb74f,

    /// <summary>Clear Crocomire's invisible wall PLM at $84:B753.</summary>
    ClearCrocomireInvisibleWall = 0xb753,

    /// <summary>Create Crocomire's invisible wall PLM at $84:B757.</summary>
    CreateCrocomireInvisibleWall = 0xb757,

    /// <summary><c>PLMEntries_unusedDraw13BlankAirTiles</c> at $84:B75B.</summary>
    UnusedDraw13BlankAirTiles = 0xb75b,

    /// <summary><c>PLMEntries_unusedDraw13BlankSolidTiles</c> at $84:B75F.</summary>
    UnusedDraw13BlankSolidTiles = 0xb75f,

    /// <summary>Clear the Baby Metroid encounter's invisible wall at $84:B763.</summary>
    ClearBabyMetroidInvisibleWall = 0xb763,

    /// <summary>Create the Baby Metroid encounter's invisible wall at $84:B767.</summary>
    CreateBabyMetroidInvisibleWall = 0xb767,

    /// <summary><c>PLMEntries_saveStationTrigger</c> at $84:B76B.</summary>
    SaveStationTrigger = 0xb76b,

    /// <summary>Main save-station room actor at $84:B76F.</summary>
    SaveStation = 0xb76f,

    /// <summary><c>PLMEntries_crumbleAccessToTourianElevator</c> at $84:B773.</summary>
    CrumbleAccessToTourianElevator = 0xb773,

    /// <summary><c>PLMEntries_clearAccessToTourianElevator</c> at $84:B777.</summary>
    ClearAccessToTourianElevator = 0xb777,

    /// <summary>Draw Phantoon's closed arena door at $84:B781.</summary>
    DrawPhantoonDoorDuringBossFight = 0xb781,

    /// <summary>Restore Phantoon's door after the fight at $84:B78B.</summary>
    RestorePhantoonDoorAfterBossFight = 0xb78b,

    /// <summary>Crumble Spore Spawn's ceiling PLM at $84:B78F.</summary>
    CrumbleSporeSpawnCeiling = 0xb78f,

    /// <summary>Clear Spore Spawn's ceiling PLM at $84:B793.</summary>
    ClearSporeSpawnCeiling = 0xb793,

    /// <summary>Clear Botwoon's wall PLM at $84:B797.</summary>
    ClearBotwoonWall = 0xb797,

    /// <summary>Crumble Botwoon's wall PLM at $84:B79B.</summary>
    CrumbleBotwoonWall = 0xb79b,

    /// <summary><c>PLMEntries_unusedSetKraidCeilingBlockToBackground1</c> at $84:B79F.</summary>
    UnusedSetKraidCeilingBlockToBackground1 = 0xb79f,

    /// <summary>Kraid ceiling crumble using background variant one at $84:B7A3.</summary>
    CrumbleKraidCeilingIntoBackground1 = 0xb7a3,

    /// <summary>Kraid platform crumble using the first floor variant at $84:B7A7.</summary>
    CrumbleKraidPlatformVariant1 = 0xb7a7,

    /// <summary>Kraid ceiling crumble using background variant two at $84:B7AB.</summary>
    CrumbleKraidCeilingIntoBackground2 = 0xb7ab,

    /// <summary>Kraid platform crumble using the second floor variant at $84:B7AF.</summary>
    CrumbleKraidPlatformVariant2 = 0xb7af,

    /// <summary>Kraid ceiling crumble using background variant three at $84:B7B3.</summary>
    CrumbleKraidCeilingIntoBackground3 = 0xb7b3,

    /// <summary>Clear Kraid's ceiling after an already-defeated room load at $84:B7B7.</summary>
    ClearKraidCeiling = 0xb7b7,

    /// <summary>Clear Kraid-room spikes after an already-defeated room load at $84:B7BB.</summary>
    ClearKraidSpikes = 0xb7bb,

    /// <summary><c>$84:B7BF PLMEntries_crumbleKraidSpikeBlocks</c>: animated live-death floor sweep.</summary>
    CrumbleKraidSpikes = 0xb7bf,

    /// <summary><c>PLMEntries_enableSoundsIn20Frames_F0FramesIfCeres</c> at $84:B7EB.</summary>
    EnableSoundsIn20FramesF0FramesIfCeres = 0xb7eb,

    /// <summary>Speed Booster escape lavaquake controller at $84:B8AC.</summary>
    SpeedBoosterEscape = 0xb8ac,

    /// <summary><c>PLMEntries_shaktoolsRoom</c> at $84:B8EB.</summary>
    ShaktoolsRoom = 0xb8eb,

    /// <summary>Maridia elevatube delay/sound PLM at $84:B8F9.</summary>
    MaridiaElevatube = 0xb8f9,

    /// <summary>
    /// Old Tourian escape shaft fake wall at $84:B964, spawned by room setup $8F:91A9 during
    /// the escape; explodes once Samus passes below and right of it.
    /// </summary>
    OldTourianEscapeShaftFakeWall = 0xb964,

    /// <summary><c>PLMEntries_RaiseAcidInEscapeRoomBeforeOldTourianEscapeShaft</c> at $84:B968.</summary>
    RaiseAcidInEscapeRoomBeforeOldTourianEscapeShaft = 0xb968,

    /// <summary><c>PLMEntries_gateBlock</c> at $84:B974.</summary>
    GateBlock = 0xb974,

    /// <summary><c>PLMEntries_Reaction_CrittersEscapeBlock</c> at $84:B9C1.</summary>
    ReactionCrittersEscapeBlock = 0xb9c1,

    /// <summary><c>PLMEntries_CrittersEscapeBlock</c> at $84:B9ED.</summary>
    CrittersEscapeBlock = 0xb9ed,

    /// <summary><c>PLMEntries_turnCeresElevatorDoorToSolidBlocksDuringEscape</c> at $84:BA48.</summary>
    TurnCeresElevatorDoorToSolidBlocksDuringEscape = 0xba48,

    /// <summary>Bomb Torizo's exceptional right-facing grey door at $84:BAF4.</summary>
    BombTorizoGreyDoor = 0xbaf4,

    /// <summary>Resident Wrecked Ship attic no-op observer at $84:BB05.</summary>
    WreckedShipAttic = 0xbb05,

    /// <summary>
    /// Crateria mainstreet escape-passage clearer at $84:BB30, spawned by room setup $8F:9194;
    /// its setup deletes it unless the critters escaped.
    /// </summary>
    CrateriaMainstreetEscapePassage = 0xbb30,

    /// <summary><c>PLMEntries_leftGreenGateTrigger</c> at $84:C806.</summary>
    LeftGreenGateTrigger = 0xc806,

    /// <summary><c>PLMEntries_rightGreenGateTrigger</c> at $84:C80A.</summary>
    RightGreenGateTrigger = 0xc80a,

    /// <summary><c>PLMEntries_leftRedGateTrigger</c> at $84:C80E.</summary>
    LeftRedGateTrigger = 0xc80e,

    /// <summary><c>PLMEntries_rightRedGateTrigger</c> at $84:C812.</summary>
    RightRedGateTrigger = 0xc812,

    /// <summary><c>PLMEntries_leftBlueGateTrigger</c> at $84:C816.</summary>
    LeftBlueGateTrigger = 0xc816,

    /// <summary><c>PLMEntries_rightBlueGateTrigger</c> at $84:C81A.</summary>
    RightBlueGateTrigger = 0xc81a,

    /// <summary><c>PLMEntries_leftYellowGateTrigger</c> at $84:C81E.</summary>
    LeftYellowGateTrigger = 0xc81e,

    /// <summary><c>PLMEntries_rightYellowGateTrigger</c> at $84:C822.</summary>
    RightYellowGateTrigger = 0xc822,

    /// <summary><c>PLMEntries_downwardsOpenGate</c> at $84:C826.</summary>
    DownwardsOpenGate = 0xc826,

    /// <summary>Resident five-block downward gate at <c>$84:C82A</c>.</summary>
    DownwardGate = 0xc82a,

    /// <summary><c>PLMEntries_upwardsOpenGate</c> at $84:C82E.</summary>
    UpwardsOpenGate = 0xc82e,

    /// <summary><c>PLMEntries_upwardsClosedGate</c> at $84:C832.</summary>
    UpwardsClosedGate = 0xc832,

    /// <summary>Room-authored downward-gate shot block at <c>$84:C836</c>.</summary>
    DownwardGateShotBlock = 0xc836,

    /// <summary><c>PLMEntries_upwardsGateShotblock</c> at $84:C83A.</summary>
    UpwardsGateShotblock = 0xc83a,

    /// <summary><c>PLMEntries_genericShotTriggerForAPLM</c> at $84:C83E.</summary>
    GenericShotTriggerForAPLM = 0xc83e,

    // Door families use a six-byte header stride in left/right/up/down order. Naming each
    // address prevents orientation arithmetic from silently accepting an in-between word.
    /// <summary><c>PLMEntries_greyDoorFacingLeft</c> at $84:C842.</summary>
    GreyDoorFacingLeft = 0xc842,

    /// <summary><c>PLMEntries_greyDoorFacingRight</c> at $84:C848.</summary>
    GreyDoorFacingRight = 0xc848,

    /// <summary><c>PLMEntries_greyDoorFacingUp</c> at $84:C84E.</summary>
    GreyDoorFacingUp = 0xc84e,

    /// <summary><c>PLMEntries_greyDoorFacingDown</c> at $84:C854.</summary>
    GreyDoorFacingDown = 0xc854,

    /// <summary><c>PLMEntries_yellowDoorFacingLeft</c> at $84:C85A.</summary>
    YellowDoorFacingLeft = 0xc85a,

    /// <summary><c>PLMEntries_yellowDoorFacingRight</c> at $84:C860.</summary>
    YellowDoorFacingRight = 0xc860,

    /// <summary><c>PLMEntries_yellowDoorFacingUp</c> at $84:C866.</summary>
    YellowDoorFacingUp = 0xc866,

    /// <summary><c>PLMEntries_yellowDoorFacingDown</c> at $84:C86C.</summary>
    YellowDoorFacingDown = 0xc86c,

    /// <summary><c>PLMEntries_greenDoorFacingLeft</c> at $84:C872.</summary>
    GreenDoorFacingLeft = 0xc872,

    /// <summary><c>PLMEntries_greenDoorFacingRight</c> at $84:C878.</summary>
    GreenDoorFacingRight = 0xc878,

    /// <summary><c>PLMEntries_greenDoorFacingUp</c> at $84:C87E.</summary>
    GreenDoorFacingUp = 0xc87e,

    /// <summary><c>PLMEntries_greenDoorFacingDown</c> at $84:C884.</summary>
    GreenDoorFacingDown = 0xc884,

    /// <summary><c>PLMEntries_redDoorFacingLeft</c> at $84:C88A.</summary>
    RedDoorFacingLeft = 0xc88a,

    /// <summary><c>PLMEntries_redDoorFacingRight</c> at $84:C890.</summary>
    RedDoorFacingRight = 0xc890,

    /// <summary><c>PLMEntries_redDoorFacingUp</c> at $84:C896.</summary>
    RedDoorFacingUp = 0xc896,

    /// <summary><c>PLMEntries_redDoorFacingDown</c> at $84:C89C.</summary>
    RedDoorFacingDown = 0xc89c,

    /// <summary><c>PLMEntries_blueDoorFacingLeft</c> at $84:C8A2.</summary>
    BlueDoorFacingLeft = 0xc8a2,

    /// <summary><c>PLMEntries_blueDoorFacingRight</c> at $84:C8A8.</summary>
    BlueDoorFacingRight = 0xc8a8,

    /// <summary><c>PLMEntries_blueDoorFacingUp</c> at $84:C8AE.</summary>
    BlueDoorFacingUp = 0xc8ae,

    /// <summary><c>PLMEntries_blueDoorFacingDown</c> at $84:C8B4.</summary>
    BlueDoorFacingDown = 0xc8b4,

    /// <summary>Door-transition-only blue-door closer facing left at $84:C8BA.</summary>
    BlueDoorClosingFacingLeft = 0xc8ba,

    /// <summary>Door-transition-only blue-door closer facing right at $84:C8BE.</summary>
    BlueDoorClosingFacingRight = 0xc8be,

    /// <summary>Door-transition-only blue-door closer facing up at $84:C8C2.</summary>
    BlueDoorClosingFacingUp = 0xc8c2,

    /// <summary>Door-transition-only blue-door closer facing down at $84:C8C6.</summary>
    BlueDoorClosingFacingDown = 0xc8c6,

    /// <summary>
    /// Resident gate at $84:C8CA in Tourian escape room one. Its second header list is
    /// selected by the shared door-transition closer when entering from Mother Brain.
    /// </summary>
    MotherBrainEscapeRoomGate = 0xc8ca,

    /// <summary>
    /// Door-transition-only fallback gate closer at $84:C8D0, used when a special
    /// direction-$8..$B door has no resident cap at its authored block coordinate.
    /// </summary>
    MotherBrainEscapeRoomGateClosing = 0xc8d0,

    /// <summary><c>PLMEntries_1x1RespawningCrumbleBlock</c> at $84:CFFC.</summary>
    Plm1x1RespawningCrumbleBlock = 0xcffc,

    /// <summary><c>PLMEntries_2x1RespawningCrumbleBlock</c> at $84:D000.</summary>
    Plm2x1RespawningCrumbleBlock = 0xd000,

    /// <summary><c>PLMEntries_1x2RespawningCrumbleBlock</c> at $84:D004.</summary>
    Plm1x2RespawningCrumbleBlock = 0xd004,

    /// <summary><c>PLMEntries_2x2RespawningCrumbleBlock</c> at $84:D008.</summary>
    Plm2x2RespawningCrumbleBlock = 0xd008,

    /// <summary><c>PLMEntries_BombReaction_SpeedBoostBlock</c> at $84:D024.</summary>
    BombReactionSpeedBoostBlock = 0xd024,

    /// <summary>
    /// <c>$84:D030 PLMEntries_Collision_BTS82</c>: Brinstar BTS <c>$82</c>, a
    /// respawning Speed Booster block with the slower crumble animation.
    /// </summary>
    SpeedBlockBrinstarSlowRespawning = 0xd030,

    /// <summary>
    /// <c>$84:D034 PLMEntries_Collision_BTS83</c>: Brinstar BTS <c>$83</c>, a
    /// permanent Speed Booster block with the slower crumble animation.
    /// </summary>
    SpeedBlockBrinstarSlowPermanent = 0xd034,

    /// <summary>
    /// <c>$84:D038 PLMEntries_Collision_RespawningSpeedBoostBlock</c>: the
    /// area-independent BTS <c>$0E</c> respawning Speed Booster block.
    /// </summary>
    SpeedBlockRespawning = 0xd038,

    /// <summary>
    /// <c>$84:D03C PLMEntries_Collision_DachoraRespawningSpeedBoostBlock</c>:
    /// Brinstar BTS <c>$84</c>, the Dachora-room respawning Speed Booster block.
    /// </summary>
    SpeedBlockDachoraRespawning = 0xd03c,

    /// <summary>
    /// <c>$84:D040 PLMEntries_Collision_SpeedBoostBlock</c>: area-independent
    /// BTS <c>$0F</c> and Brinstar BTS <c>$85</c>, a permanent Speed Booster block.
    /// </summary>
    SpeedBlockPermanent = 0xd040,

    /// <summary>Respawning 1x1 Samus-contact crumble block at <c>$84:D044</c>.</summary>
    ContactCrumble1x1Respawning = 0xd044,

    /// <summary>Respawning 2x1 Samus-contact crumble block at <c>$84:D048</c>.</summary>
    ContactCrumble2x1Respawning = 0xd048,

    /// <summary>Respawning 1x2 Samus-contact crumble block at <c>$84:D04C</c>.</summary>
    ContactCrumble1x2Respawning = 0xd04c,

    /// <summary>Respawning 2x2 Samus-contact crumble block at <c>$84:D050</c>.</summary>
    ContactCrumble2x2Respawning = 0xd050,

    /// <summary>Permanent 1x1 Samus-contact crumble block at <c>$84:D054</c>.</summary>
    ContactCrumble1x1Permanent = 0xd054,

    /// <summary>Permanent 2x1 Samus-contact crumble block at <c>$84:D058</c>.</summary>
    ContactCrumble2x1Permanent = 0xd058,

    /// <summary>Permanent 1x2 Samus-contact crumble block at <c>$84:D05C</c>.</summary>
    ContactCrumble1x2Permanent = 0xd05c,

    /// <summary>Permanent 2x2 Samus-contact crumble block at <c>$84:D060</c>.</summary>
    ContactCrumble2x2Permanent = 0xd060,

    /// <summary><c>PLMEntries_Reaction_1x1RespawningShotBlock</c> at $84:D064.</summary>
    Reaction1x1RespawningShotBlock = 0xd064,

    /// <summary><c>PLMEntries_Reaction_2x1RespawningShotBlock</c> at $84:D068.</summary>
    Reaction2x1RespawningShotBlock = 0xd068,

    /// <summary><c>PLMEntries_Reaction_1x2RespawningShotBlock</c> at $84:D06C.</summary>
    Reaction1x2RespawningShotBlock = 0xd06c,

    /// <summary><c>PLMEntries_Reaction_2x2RespawningShotBlock</c> at $84:D070.</summary>
    Reaction2x2RespawningShotBlock = 0xd070,

    /// <summary><c>PLMEntries_Reaction_1x1ShotBlock</c> at $84:D074.</summary>
    Reaction1x1ShotBlock = 0xd074,

    /// <summary><c>PLMEntries_Reaction_2x1ShotBlock</c> at $84:D078.</summary>
    Reaction2x1ShotBlock = 0xd078,

    /// <summary><c>PLMEntries_Reaction_1x2ShotBlock</c> at $84:D07C.</summary>
    Reaction1x2ShotBlock = 0xd07c,

    /// <summary><c>PLMEntries_Reaction_2x2ShotBlock</c> at $84:D080.</summary>
    Reaction2x2ShotBlock = 0xd080,

    /// <summary><c>PLMEntries_Reaction_RespawningPowerBombBlock</c> at $84:D084.</summary>
    ReactionRespawningPowerBombBlock = 0xd084,

    /// <summary><c>PLMEntries_Reaction_PowerBombBlock</c> at $84:D088.</summary>
    ReactionPowerBombBlock = 0xd088,

    /// <summary><c>PLMEntries_Reaction_RespawningSuperMissileBlock</c> at $84:D08C.</summary>
    ReactionRespawningSuperMissileBlock = 0xd08c,

    /// <summary><c>PLMEntries_Reaction_SuperMissileBlock</c> at $84:D090.</summary>
    ReactionSuperMissileBlock = 0xd090,

    /// <summary><c>PLMEntries_EnemyBreakableBlock</c> at $84:D094.</summary>
    EnemyBreakableBlock = 0xd094,

    /// <summary><c>PLMEntries_Collision_1x1RespawningBombBlock</c> at $84:D098.</summary>
    Collision1x1RespawningBombBlock = 0xd098,

    /// <summary><c>PLMEntries_Collision_2x1RespawningBombBlock</c> at $84:D09C.</summary>
    Collision2x1RespawningBombBlock = 0xd09c,

    /// <summary><c>PLMEntries_Collision_1x2RespawningBombBlock</c> at $84:D0A0.</summary>
    Collision1x2RespawningBombBlock = 0xd0a0,

    /// <summary><c>PLMEntries_Collision_2x2RespawningBombBlock</c> at $84:D0A4.</summary>
    Collision2x2RespawningBombBlock = 0xd0a4,

    /// <summary><c>PLMEntries_Collision_1x1BombBlock</c> at $84:D0A8.</summary>
    Collision1x1BombBlock = 0xd0a8,

    /// <summary><c>PLMEntries_Collision_2x1BombBlock</c> at $84:D0AC.</summary>
    Collision2x1BombBlock = 0xd0ac,

    /// <summary><c>PLMEntries_Collision_1x2BombBlock</c> at $84:D0B0.</summary>
    Collision1x2BombBlock = 0xd0b0,

    /// <summary><c>PLMEntries_Collision_2x2BombBlock</c> at $84:D0B4.</summary>
    Collision2x2BombBlock = 0xd0b4,

    /// <summary><c>PLMEntries_Reaction_1x1RespawningBombBlock</c> at $84:D0B8.</summary>
    Reaction1x1RespawningBombBlock = 0xd0b8,

    /// <summary><c>PLMEntries_Reaction_2x1RespawningBombBlock</c> at $84:D0BC.</summary>
    Reaction2x1RespawningBombBlock = 0xd0bc,

    /// <summary><c>PLMEntries_Reaction_1x2RespawningBombBlock</c> at $84:D0C0.</summary>
    Reaction1x2RespawningBombBlock = 0xd0c0,

    /// <summary><c>PLMEntries_Reaction_2x2RespawningBombBlock</c> at $84:D0C4.</summary>
    Reaction2x2RespawningBombBlock = 0xd0c4,

    /// <summary><c>PLMEntries_Reaction_1x1BombBlock</c> at $84:D0C8.</summary>
    Reaction1x1BombBlock = 0xd0c8,

    /// <summary><c>PLMEntries_Reaction_2x1BombBlock</c> at $84:D0CC.</summary>
    Reaction2x1BombBlock = 0xd0cc,

    /// <summary><c>PLMEntries_Reaction_1x2BombBlock</c> at $84:D0D0.</summary>
    Reaction1x2BombBlock = 0xd0d0,

    /// <summary><c>PLMEntries_Reaction_2x2BombBlock</c> at $84:D0D4.</summary>
    Reaction2x2BombBlock = 0xd0d4,

    /// <summary><c>PLMEntries_Grappled_GrappleBlock</c> at $84:D0D8.</summary>
    GrappledGrappleBlock = 0xd0d8,

    /// <summary><c>PLMEntries_Grappled_RespawningBreakableGrappleBlock</c> at $84:D0DC.</summary>
    GrappledRespawningBreakableGrappleBlock = 0xd0dc,

    /// <summary><c>PLMEntries_Grappled_BreakableGrappleBlock</c> at $84:D0E0.</summary>
    GrappledBreakableGrappleBlock = 0xd0e0,

    /// <summary><c>PLMEntries_Grappled_GenericSpikeBlock</c> at $84:D0E4.</summary>
    GrappledGenericSpikeBlock = 0xd0e4,

    /// <summary><c>PLMEntries_Grappled_DraygonsBrokenTurret</c> at $84:D0E8.</summary>
    GrappledDraygonsBrokenTurret = 0xd0e8,

    /// <summary><c>PLMEntries_UnusedBlueBrinstarFaceBlock</c> at $84:D0F2.</summary>
    UnusedBlueBrinstarFaceBlock = 0xd0f2,

    /// <summary><c>PLMEntries_CrumbleLowerNorfairChozoRoomPlug</c> at $84:D113.</summary>
    CrumbleLowerNorfairChozoRoomPlug = 0xd113,

    /// <summary><c>PLMEntries_UnusedShotBlock</c> at $84:D127.</summary>
    UnusedShotBlock = 0xd127,

    /// <summary><c>PLMEntries_UnusedGrappleBlock</c> at $84:D13B.</summary>
    UnusedGrappleBlock = 0xd13b,

    /// <summary><c>PLMEntries_LowerNorfairChozoHand</c> at $84:D6D6.</summary>
    LowerNorfairChozoHand = 0xd6d6,

    /// <summary><c>PLMEntries_Collision_LowerNorfairChozoHandCheck</c> at $84:D6DA.</summary>
    CollisionLowerNorfairChozoHandCheck = 0xd6da,

    /// <summary>Mother Brain's missile-reactive glass actor at $84:D6DE.</summary>
    MotherBrainGlass = 0xd6de,

    /// <summary>Bomb Torizo's Chozo-hand synchronization actor at $84:D6EA.</summary>
    BombTorizoHand = 0xd6ea,

    /// <summary><c>PLMEntries_WreckedShipChozoHand</c> at $84:D6EE.</summary>
    WreckedShipChozoHand = 0xd6ee,

    /// <summary><c>PLMEntries_Collision_WreckedShipChozoHandCheck</c> at $84:D6F2.</summary>
    CollisionWreckedShipChozoHandCheck = 0xd6f2,

    /// <summary><c>PLMEntries_ClearSlopeAccessForWreckedShipChozo</c> at $84:D6F8.</summary>
    ClearSlopeAccessForWreckedShipChozo = 0xd6f8,

    /// <summary><c>PLMEntries_BlockSlopeAccessForWreckedShipChozo</c> at $84:D6FC.</summary>
    BlockSlopeAccessForWreckedShipChozo = 0xd6fc,

    /// <summary>Maridia's power-bomb-reactive n00b tube actor at $84:D70C.</summary>
    NoobTube = 0xd70c,

    /// <summary>
    /// Resident room-kill observer at $84:DB44 which marks the four Tourian Metroid-room
    /// events when their authored enemy death quotas have been reached.
    /// </summary>
    SetMetroidsClearedStatesWhenRequired = 0xdb44,

    /// <summary>Right-facing eye-door eye controller at <c>$84:DB48</c>.</summary>
    EyeDoorEyeFacingRight = 0xdb48,

    /// <summary>Right-facing eye-door middle/door component at <c>$84:DB4C</c>.</summary>
    EyeDoorFacingRight = 0xdb4c,

    /// <summary>Right-facing eye-door bottom component at <c>$84:DB52</c>.</summary>
    EyeDoorBottomFacingRight = 0xdb52,

    /// <summary>Left-facing eye-door eye controller at <c>$84:DB56</c>.</summary>
    EyeDoorEyeFacingLeft = 0xdb56,

    /// <summary>Left-facing eye-door middle/door component at <c>$84:DB5A</c>.</summary>
    EyeDoorFacingLeft = 0xdb5a,

    /// <summary>Left-facing eye-door bottom component at <c>$84:DB60</c>.</summary>
    EyeDoorBottomFacingLeft = 0xdb60,

    /// <summary>Draygon-room right-facing shielded cannon at <c>$84:DF59</c>.</summary>
    DraygonCannonFacingRight = 0xdf59,

    /// <summary>
    /// Draygon-room right-facing cannon at <c>$84:DF65</c>. Its authored list enters the
    /// destroyed state immediately, disabling the unused upper-left firing position.
    /// </summary>
    DraygonCannonFacingRightDestroyed = 0xdf65,

    /// <summary>Draygon-room left-facing shielded cannon at <c>$84:DF71</c>.</summary>
    DraygonCannonFacingLeft = 0xdf71,

    /// <summary><c>PLMEntries_DraygonCannonFacingLeft</c> at $84:DF7D.</summary>
    DraygonCannonFacingLeftUnshielded = 0xdf7d,

    /// <summary><c>PLMEntries_ItemCollisionDetection</c> at $84:EED3.</summary>
    ItemCollisionDetection = 0xeed3,

    // Exposed permanent-item headers. The order matches InWorldCollectibleKind exactly.
    /// <summary><c>PLMEntries_EnergyTank</c> at $84:EED7.</summary>
    ExposedEnergyTank = 0xeed7,

    /// <summary><c>PLMEntries_MissileTank</c> at $84:EEDB.</summary>
    ExposedMissileTank = 0xeedb,

    /// <summary><c>PLMEntries_SuperMissileTank</c> at $84:EEDF.</summary>
    ExposedSuperMissileTank = 0xeedf,

    /// <summary><c>PLMEntries_PowerBombTank</c> at $84:EEE3.</summary>
    ExposedPowerBombTank = 0xeee3,

    /// <summary><c>PLMEntries_Bombs</c> at $84:EEE7.</summary>
    ExposedBombs = 0xeee7,

    /// <summary><c>PLMEntries_ChargeBeam</c> at $84:EEEB.</summary>
    ExposedChargeBeam = 0xeeeb,

    /// <summary><c>PLMEntries_IceBeam</c> at $84:EEEF.</summary>
    ExposedIceBeam = 0xeeef,

    /// <summary><c>PLMEntries_HiJumpBoots</c> at $84:EEF3.</summary>
    ExposedHiJumpBoots = 0xeef3,

    /// <summary><c>PLMEntries_SpeedBooster</c> at $84:EEF7.</summary>
    ExposedSpeedBooster = 0xeef7,

    /// <summary><c>PLMEntries_WaveBeam</c> at $84:EEFB.</summary>
    ExposedWaveBeam = 0xeefb,

    /// <summary><c>PLMEntries_Spazer</c> at $84:EEFF.</summary>
    ExposedSpazerBeam = 0xeeff,

    /// <summary><c>PLMEntries_SpringBall</c> at $84:EF03.</summary>
    ExposedSpringBall = 0xef03,

    /// <summary><c>PLMEntries_VariaSuit</c> at $84:EF07.</summary>
    ExposedVariaSuit = 0xef07,

    /// <summary><c>PLMEntries_GravitySuit</c> at $84:EF0B.</summary>
    ExposedGravitySuit = 0xef0b,

    /// <summary><c>PLMEntries_XrayScope</c> at $84:EF0F.</summary>
    ExposedXrayScope = 0xef0f,

    /// <summary><c>PLMEntries_PlasmaBeam</c> at $84:EF13.</summary>
    ExposedPlasmaBeam = 0xef13,

    /// <summary><c>PLMEntries_GrappleBeam</c> at $84:EF17.</summary>
    ExposedGrappleBeam = 0xef17,

    /// <summary><c>PLMEntries_SpaceJump</c> at $84:EF1B.</summary>
    ExposedSpaceJump = 0xef1b,

    /// <summary><c>PLMEntries_ScrewAttack</c> at $84:EF1F.</summary>
    ExposedScrewAttack = 0xef1f,

    /// <summary><c>PLMEntries_MorphBall</c> at $84:EF23.</summary>
    ExposedMorphBall = 0xef23,

    /// <summary><c>PLMEntries_ReserveTank</c> at $84:EF27.</summary>
    ExposedReserveTank = 0xef27,

    // Chozo-orb permanent-item headers. The order matches InWorldCollectibleKind exactly.
    /// <summary><c>PLMEntries_EnergyTankChozoOrb</c> at $84:EF2B.</summary>
    ChozoEnergyTank = 0xef2b,

    /// <summary><c>PLMEntries_MissileTankChozoOrb</c> at $84:EF2F.</summary>
    ChozoMissileTank = 0xef2f,

    /// <summary><c>PLMEntries_SuperMissileTankChozoOrb</c> at $84:EF33.</summary>
    ChozoSuperMissileTank = 0xef33,

    /// <summary><c>PLMEntries_PowerBombTankChozoOrb</c> at $84:EF37.</summary>
    ChozoPowerBombTank = 0xef37,

    /// <summary><c>PLMEntries_BombsChozoOrb</c> at $84:EF3B.</summary>
    ChozoBombs = 0xef3b,

    /// <summary><c>PLMEntries_ChargeBeamChozoOrb</c> at $84:EF3F.</summary>
    ChozoChargeBeam = 0xef3f,

    /// <summary><c>PLMEntries_IceBeamChozoOrb</c> at $84:EF43.</summary>
    ChozoIceBeam = 0xef43,

    /// <summary><c>PLMEntries_HiJumpBootsChozoOrb</c> at $84:EF47.</summary>
    ChozoHiJumpBoots = 0xef47,

    /// <summary><c>PLMEntries_SpeedBoosterChozoOrb</c> at $84:EF4B.</summary>
    ChozoSpeedBooster = 0xef4b,

    /// <summary><c>PLMEntries_WaveBeamChozoOrb</c> at $84:EF4F.</summary>
    ChozoWaveBeam = 0xef4f,

    /// <summary><c>PLMEntries_SpazerChozoOrb</c> at $84:EF53.</summary>
    ChozoSpazerBeam = 0xef53,

    /// <summary><c>PLMEntries_SpringBallChozoOrb</c> at $84:EF57.</summary>
    ChozoSpringBall = 0xef57,

    /// <summary><c>PLMEntries_VariaSuitChozoOrb</c> at $84:EF5B.</summary>
    ChozoVariaSuit = 0xef5b,

    /// <summary><c>PLMEntries_GravitySuitChozoOrb</c> at $84:EF5F.</summary>
    ChozoGravitySuit = 0xef5f,

    /// <summary><c>PLMEntries_XrayScopeChozoOrb</c> at $84:EF63.</summary>
    ChozoXrayScope = 0xef63,

    /// <summary><c>PLMEntries_PlasmaBeamChozoOrb</c> at $84:EF67.</summary>
    ChozoPlasmaBeam = 0xef67,

    /// <summary><c>PLMEntries_GrappleBeamChozoOrb</c> at $84:EF6B.</summary>
    ChozoGrappleBeam = 0xef6b,

    /// <summary><c>PLMEntries_SpaceJumpChozoOrb</c> at $84:EF6F.</summary>
    ChozoSpaceJump = 0xef6f,

    /// <summary><c>PLMEntries_ScrewAttackChozoOrb</c> at $84:EF73.</summary>
    ChozoScrewAttack = 0xef73,

    /// <summary><c>PLMEntries_MorphBallChozoOrb</c> at $84:EF77.</summary>
    ChozoMorphBall = 0xef77,

    /// <summary><c>PLMEntries_ReserveTankChozoOrb</c> at $84:EF7B.</summary>
    ChozoReserveTank = 0xef7b,

    // Concealed shot-block permanent-item headers. The order matches
    // InWorldCollectibleKind exactly.
    /// <summary><c>PLMEntries_EnergyTankShotBlock</c> at $84:EF7F.</summary>
    ShotBlockEnergyTank = 0xef7f,

    /// <summary><c>PLMEntries_MissileTankShotBlock</c> at $84:EF83.</summary>
    ShotBlockMissileTank = 0xef83,

    /// <summary><c>PLMEntries_SuperMissileTankShotBlock</c> at $84:EF87.</summary>
    ShotBlockSuperMissileTank = 0xef87,

    /// <summary><c>PLMEntries_PowerBombTankShotBlock</c> at $84:EF8B.</summary>
    ShotBlockPowerBombTank = 0xef8b,

    /// <summary><c>PLMEntries_BombsShotBlock</c> at $84:EF8F.</summary>
    ShotBlockBombs = 0xef8f,

    /// <summary><c>PLMEntries_ChargeBeamShotBlock</c> at $84:EF93.</summary>
    ShotBlockChargeBeam = 0xef93,

    /// <summary><c>PLMEntries_IceBeamShotBlock</c> at $84:EF97.</summary>
    ShotBlockIceBeam = 0xef97,

    /// <summary><c>PLMEntries_HiJumpBootsShotBlock</c> at $84:EF9B.</summary>
    ShotBlockHiJumpBoots = 0xef9b,

    /// <summary><c>PLMEntries_SpeedBoosterShotBlock</c> at $84:EF9F.</summary>
    ShotBlockSpeedBooster = 0xef9f,

    /// <summary><c>PLMEntries_WaveBeamShotBlock</c> at $84:EFA3.</summary>
    ShotBlockWaveBeam = 0xefa3,

    /// <summary><c>PLMEntries_SpazerShotBlock</c> at $84:EFA7.</summary>
    ShotBlockSpazerBeam = 0xefa7,

    /// <summary><c>PLMEntries_SpringBallShotBlock</c> at $84:EFAB.</summary>
    ShotBlockSpringBall = 0xefab,

    /// <summary><c>PLMEntries_VariaSuitShotBlock</c> at $84:EFAF.</summary>
    ShotBlockVariaSuit = 0xefaf,

    /// <summary><c>PLMEntries_GravitySuitShotBlock</c> at $84:EFB3.</summary>
    ShotBlockGravitySuit = 0xefb3,

    /// <summary><c>PLMEntries_XrayScopeShotBlock</c> at $84:EFB7.</summary>
    ShotBlockXrayScope = 0xefb7,

    /// <summary><c>PLMEntries_PlasmaBeamShotBlock</c> at $84:EFBB.</summary>
    ShotBlockPlasmaBeam = 0xefbb,

    /// <summary><c>PLMEntries_GrappleBeamShotBlock</c> at $84:EFBF.</summary>
    ShotBlockGrappleBeam = 0xefbf,

    /// <summary><c>PLMEntries_SpaceJumpShotBlock</c> at $84:EFC3.</summary>
    ShotBlockSpaceJump = 0xefc3,

    /// <summary><c>PLMEntries_ScrewAttackShotBlock</c> at $84:EFC7.</summary>
    ShotBlockScrewAttack = 0xefc7,

    /// <summary><c>PLMEntries_MorphBallShotBlock</c> at $84:EFCB.</summary>
    ShotBlockMorphBall = 0xefcb,

    /// <summary><c>PLMEntries_ReserveTankShotBlock</c> at $84:EFCF.</summary>
    ShotBlockReserveTank = 0xefcf,
}
