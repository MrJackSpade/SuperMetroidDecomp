namespace SuperMetroid.Core.Rooms;

/// <summary>Named bank-$84 PLM instruction-list offsets used by translated room actors.</summary>
/// <remarks>
/// Values are native low-word pointers in fixed bank $84. List identities remain separate
/// from executable opcode identities in <see cref="RoomPlmInstruction"/> and from
/// draw-list pointers, even though all three domains share the same 16-bit storage type.
/// </remarks>
public static class RoomPlmInstructionLists
{

    /// <summary><c>$84:BCAF InstList_PLM_DownwardsGateShotblock_BlueLeft</c>.</summary>
    public const ushort DownwardGateShotBlockBlueLeft = 0xbcaf;
    /// <summary><c>$84:AAE3 InstList_PLM_Delete</c>.</summary>
    public const ushort Delete = 0xaae3;

    /// <summary><c>$84:AB12 InstList_PLM_CrumbleSporeSpawnCeiling</c>.</summary>
    public const ushort CrumbleSporeSpawnCeiling = 0xab12;

    /// <summary><c>$84:AB21 InstList_PLM_ClearSporeSpawnCeiling</c>.</summary>
    public const ushort ClearSporeSpawnCeiling = 0xab21;

    /// <summary><c>$84:AB31 InstList_PLM_CrumbleBotwoonWall_0</c>.</summary>
    public const ushort CrumbleBotwoonWall = 0xab31;

    /// <summary><c>$84:AB67 InstList_PLM_ClearBotwoonWall</c>.</summary>
    public const ushort ClearBotwoonWall = 0xab67;

    /// <summary>Kraid ceiling background-one crumble list at $84:AB6D.</summary>
    public const ushort CrumbleKraidCeilingIntoBackground1 = 0xab6d;
    /// <summary>Kraid ceiling background-two crumble list at $84:AB7F.</summary>
    public const ushort CrumbleKraidCeilingIntoBackground2 = 0xab7f;
    /// <summary>Kraid platform variant-one crumble list at $84:AB8B.</summary>
    public const ushort CrumbleKraidPlatformVariant1 = 0xab8b;
    /// <summary>Kraid ceiling background-three crumble list at $84:AB91.</summary>
    public const ushort CrumbleKraidCeilingIntoBackground3 = 0xab91;
    /// <summary>Kraid platform variant-two crumble list at $84:AB9D.</summary>
    public const ushort CrumbleKraidPlatformVariant2 = 0xab9d;
    /// <summary>Already-defeated Kraid ceiling clear list at $84:ABA3.</summary>
    public const ushort ClearKraidCeiling = 0xaba3;
    /// <summary>Already-defeated Kraid spike clear list at $84:ABDD.</summary>
    public const ushort ClearKraidSpikes = 0xabdd;

    /// <summary><c>$84:ABA9 InstList_PLM_CrumbleKraidSpikeBlocks_0</c>: eleven pairs of animated floor removals.</summary>
    public const ushort CrumbleKraidSpikes = 0xaba9;

    /// <summary><c>$84:AC05 InstList_PLM_FillMotherBrainsWall</c>: draws the wall for one frame, then deletes the mutation PLM.</summary>
    public const ushort FillMotherBrainsWall = 0xac05;
    /// <summary>$84:AC0B draws the opened escape door then deletes the one-shot PLM.</summary>
    public const ushort MotherBrainsRoomEscapeDoor = 0xac0b;
    /// <summary><c>$84:AC11 InstList_PLM_MotherBrainsBackgroundRow2</c>: installs row 2's thirteen-block layout during fake death, then deletes.</summary>
    public const ushort MotherBrainsBackgroundRow2 = 0xac11;
    /// <summary><c>$84:AC17 InstList_PLM_MotherBrainsBackgroundRow3</c>: installs row 3's thirteen-block layout during fake death, then deletes.</summary>
    public const ushort MotherBrainsBackgroundRow3 = 0xac17;
    /// <summary><c>$84:AC1D InstList_PLM_MotherBrainsBackgroundRow4</c>: installs row 4's thirteen-block layout during fake death, then deletes.</summary>
    public const ushort MotherBrainsBackgroundRow4 = 0xac1d;
    /// <summary><c>$84:AC23 InstList_PLM_MotherBrainsBackgroundRow5</c>: installs row 5's thirteen-block layout during fake death, then deletes.</summary>
    public const ushort MotherBrainsBackgroundRow5 = 0xac23;
    /// <summary><c>$84:AC29 InstList_PLM_MotherBrainsBackgroundRow6</c>: installs row 6's thirteen-block layout during fake death, then deletes.</summary>
    public const ushort MotherBrainsBackgroundRow6 = 0xac29;
    /// <summary><c>$84:AC2F InstList_PLM_MotherBrainsBackgroundRow7</c>: installs row 7's thirteen-block layout during fake death, then deletes.</summary>
    public const ushort MotherBrainsBackgroundRow7 = 0xac2f;
    /// <summary><c>$84:AC35 InstList_PLM_MotherBrainsBackgroundRow8</c>: installs row 8's thirteen-block layout during fake death, then deletes.</summary>
    public const ushort MotherBrainsBackgroundRow8 = 0xac35;
    /// <summary><c>$84:AC3B InstList_PLM_MotherBrainsBackgroundRow9</c>: installs row 9's thirteen-block layout during fake death, then deletes.</summary>
    public const ushort MotherBrainsBackgroundRow9 = 0xac3b;
    /// <summary><c>$84:AC41 InstList_PLM_MotherBrainsBackgroundRowA</c>: installs row $A's thirteen-block layout during fake death, then deletes.</summary>
    public const ushort MotherBrainsBackgroundRowA = 0xac41;
    /// <summary><c>$84:AC47 InstList_PLM_MotherBrainsBackgroundRowB</c>: installs row $B's thirteen-block layout during fake death, then deletes.</summary>
    public const ushort MotherBrainsBackgroundRowB = 0xac47;
    /// <summary><c>$84:AC4D InstList_PLM_MotherBrainsBackgroundRowC</c>: installs row $C's thirteen-block layout during fake death, then deletes.</summary>
    public const ushort MotherBrainsBackgroundRowC = 0xac4d;
    /// <summary><c>$84:AC53 InstList_PLM_MotherBrainsBackgroundRowD</c>: installs row $D's thirteen-block layout during fake death, then deletes.</summary>
    public const ushort MotherBrainsBackgroundRowD = 0xac53;
    /// <summary><c>$84:AC65 InstList_PLM_ClearCeilingBlockInMotherBrainsRoom</c>: applies a one-frame ceiling-block removal and deletes.</summary>
    public const ushort ClearMotherBrainCeilingBlock = 0xac65;
    /// <summary><c>$84:AC6B InstList_PLM_ClearCeilingTubeInMotherBrainsRoom</c>: applies a one-frame ceiling-tube removal and deletes.</summary>
    public const ushort ClearMotherBrainCeilingTube = 0xac6b;
    /// <summary><c>$84:AC71 InstList_PLM_ClearMotherBrainsBottomMiddleSideTube</c>: clears the lower middle side tube and deletes.</summary>
    public const ushort ClearMotherBrainBottomMiddleSideTube = 0xac71;
    /// <summary><c>$84:AC77 InstList_PLM_ClearMotherBrainsBottomMiddleTubes</c>: clears the lower middle tubes and deletes.</summary>
    public const ushort ClearMotherBrainBottomMiddleTubes = 0xac77;
    /// <summary><c>$84:AC7D InstList_PLM_ClearMotherBrainsBottomLeftTube</c>: clears the lower left tube and deletes.</summary>
    public const ushort ClearMotherBrainBottomLeftTube = 0xac7d;
    /// <summary><c>$84:AC83 InstList_PLM_ClearMotherBrainsBottomRightTube</c>: clears the lower right tube and deletes.</summary>
    public const ushort ClearMotherBrainBottomRightTube = 0xac83;

    /// <summary>Wrecked Ship west-entry treadmill list at $84:AD38.</summary>
    public const ushort WreckedShipEntranceTreadmillFromWest = 0xad38;

    /// <summary>Wrecked Ship east-entry treadmill list at $84:AD4D.</summary>
    public const ushort WreckedShipEntranceTreadmillFromEast = 0xad4d;

    /// <summary>Clear Crocomire's bridge list at $84:AFCA.</summary>
    public const ushort ClearCrocomireBridge = 0xafca;

    /// <summary>Crumble one Crocomire bridge block list at $84:AFD0.</summary>
    public const ushort CrumbleCrocomireBridgeBlock = 0xafd0;

    /// <summary>Clear one Crocomire bridge block list at $84:AFD6.</summary>
    public const ushort ClearCrocomireBridgeBlock = 0xafd6;

    /// <summary>Clear Crocomire's invisible wall list at $84:AFDC.</summary>
    public const ushort ClearCrocomireInvisibleWall = 0xafdc;

    /// <summary>Create Crocomire's invisible wall list at $84:AFE2.</summary>
    public const ushort CreateCrocomireInvisibleWall = 0xafe2;

    /// <summary><c>$84:AF8A InstList_PLM_ScrollPLM_1</c>, the sleeping trigger loop.</summary>
    public const ushort ScrollTriggerWaiting = 0xaf8a;

    /// <summary>Idle save-pod draw entry at $84:AFE8, before its trigger sleeps.</summary>
    public const ushort SaveStationIdleDraw = 0xafe8;

    /// <summary>First save-pod animation frame entry at $84:AFFA.</summary>
    public const ushort SaveStationAnimationFirstFrame = 0xaffa;

    /// <summary>Second save-pod animation frame entry at $84:AFFE.</summary>
    public const ushort SaveStationAnimationSecondFrame = 0xaffe;

    /// <summary>Speed Booster escape's three-pre-instruction coroutine at $84:B88A.</summary>
    public const ushort SpeedBoosterEscape = 0xb88a;

    /// <summary>Maridia elevatube's delay and sound list at $84:B8F0.</summary>
    public const ushort MaridiaElevatube = 0xb8f0;

    /// <summary>
    /// <c>$84:BAFF InstList_PLM_WreckedShipAttic</c>: install the cartridge's inert
    /// pre-instruction and sleep permanently.
    /// </summary>
    public const ushort WreckedShipAttic = 0xbaff;

    /// <summary><c>$84:C489 InstList_PLM_BlueDoorFacingLeftOpened_40</c>: plays the opening sound, animates the left-facing cap, clears it, and deletes.</summary>
    public const ushort BlueDoorFacingLeftOpening = 0xc489;
    /// <summary><c>$84:C4BA InstList_PLM_BlueDoorFacingLeftOpened_41</c>: opens the right-facing blue door; the pinned native symbol retains its left-facing spelling.</summary>
    public const ushort BlueDoorFacingRightOpening = 0xc4ba;
    /// <summary><c>$84:C4EB InstList_PLM_BlueDoorFacingUpOpened_42</c>: plays the opening sound, animates the upward-facing cap, clears it, and deletes.</summary>
    public const ushort BlueDoorFacingUpOpening = 0xc4eb;
    /// <summary><c>$84:C51C InstList_PLM_BlueDoorFacingUpOpened_43</c>: opens the downward-facing blue door; the pinned native symbol retains its upward-facing spelling.</summary>
    public const ushort BlueDoorFacingDownOpening = 0xc51c;

    /// <summary>
    /// <c>$84:BB44 InstList_PLM_GateThatClosesDuringEscapeAfterMotherBrain_1</c>:
    /// animate open, half-closed, and closed at two frames apiece, then release the slot.
    /// </summary>
    public const ushort MotherBrainEscapeRoomGateClosing = 0xbb44;

    /// <summary><c>$84:C8EC InstList_BombReaction_PLM_1x1RespawningCrumbleBlock</c>: reveals one crumble block for one frame, then deletes without breaking it.</summary>
    public const ushort CrumbleReveal1x1 = 0xc8ec;
    /// <summary><c>$84:C8F2 InstList_BombReaction_PLM_2x1RespawningCrumbleBlock</c>: reveals a horizontal two-block crumble parent, then deletes.</summary>
    public const ushort CrumbleReveal2x1 = 0xc8f2;
    /// <summary><c>$84:C8F8 InstList_BombReaction_PLM_1x2RespawningCrumbleBlock</c>: reveals a vertical two-block crumble parent, then deletes.</summary>
    public const ushort CrumbleReveal1x2 = 0xc8f8;
    /// <summary><c>$84:C8FE InstList_BombReaction_PLM_2x2RespawningCrumbleBlock</c>: reveals a square four-block crumble parent, then deletes.</summary>
    public const ushort CrumbleReveal2x2 = 0xc8fe;

    /// <summary>Respawning 1x1 contact-crumble list at <c>$84:C9F9</c>.</summary>
    public const ushort ContactCrumble1x1Respawning = 0xc9f9;
    /// <summary>Respawning 2x1 contact-crumble list at <c>$84:CA1C</c>.</summary>
    public const ushort ContactCrumble2x1Respawning = 0xca1c;
    /// <summary>Respawning 1x2 contact-crumble list at <c>$84:CA41</c>.</summary>
    public const ushort ContactCrumble1x2Respawning = 0xca41;
    /// <summary>Respawning 2x2 contact-crumble list at <c>$84:CA66</c>.</summary>
    public const ushort ContactCrumble2x2Respawning = 0xca66;
    /// <summary>Permanent 1x1 contact-crumble list at <c>$84:CA8B</c>.</summary>
    public const ushort ContactCrumble1x1Permanent = 0xca8b;
    /// <summary>Permanent 2x1 contact-crumble list at <c>$84:CAA0</c>.</summary>
    public const ushort ContactCrumble2x1Permanent = 0xcaa0;
    /// <summary>Permanent 1x2 contact-crumble list at <c>$84:CAB5</c>.</summary>
    public const ushort ContactCrumble1x2Permanent = 0xcab5;
    /// <summary>Permanent 2x2 contact-crumble list at <c>$84:CACA</c>.</summary>
    public const ushort ContactCrumble2x2Permanent = 0xcaca;

    /// <summary><c>$84:C91C UNUSED_InstList_PLM_PowerBombBlockBombed_84C91C</c>: bomb setup uses this one-frame reveal/delete list despite its unused native name.</summary>
    public const ushort BombedPowerBombBlockUnused = 0xc91c;
    /// <summary><c>$84:C922 UNUSED_InstList_PLM_SuperMissileBlockBombed_84C922</c>: bomb setup reveals the gated Super Missile parent without breaking it, then deletes.</summary>
    public const ushort BombedSuperMissileBlockUnused = 0xc922;
    /// <summary><c>$84:C928 InstList_PLM_BombReaction_SpeedBlock</c>: draws a revealed speed-block parent for one frame and deletes; it does not perform the speed-break animation.</summary>
    public const ushort BombReactionSpeedBlock = 0xc928;

    /// <summary>Brinstar BTS <c>$82</c> slow respawning speed-block list at <c>$84:C951</c>.</summary>
    public const ushort SpeedBlockBrinstarSlowRespawning = 0xc951;
    /// <summary>Area-independent BTS <c>$0E</c> respawning speed-block list at <c>$84:C974</c>.</summary>
    public const ushort SpeedBlockRespawning = 0xc974;
    /// <summary>Dachora-room BTS <c>$84</c> respawning speed-block list at <c>$84:C997</c>.</summary>
    public const ushort SpeedBlockDachoraRespawning = 0xc997;
    /// <summary>Brinstar BTS <c>$83</c> slow permanent speed-block list at <c>$84:C9CF</c>.</summary>
    public const ushort SpeedBlockBrinstarSlowPermanent = 0xc9cf;
    /// <summary>Area-independent BTS <c>$0F</c> permanent speed-block list at <c>$84:C9E4</c>.</summary>
    public const ushort SpeedBlockPermanent = 0xc9e4;

    /// <summary><c>$84:CADF InstList_PLM_1x1RespawningShotBlock</c>: sounds and animates a single block's break, waits 384 ticks, then restores its saved level word.</summary>
    public const ushort RespawningShotBlock1x1 = 0xcadf;
    /// <summary><c>$84:CB02 InstList_PLM_2x1RespawningShotBlock</c>: breaks a horizontal pair, waits 384 ticks, and restores its linked two-block layout.</summary>
    public const ushort RespawningShotBlock2x1 = 0xcb02;
    /// <summary><c>$84:CB27 InstList_PLM_1x2RespawningShotBlock</c>: breaks a vertical pair, waits 384 ticks, and restores its linked two-block layout.</summary>
    public const ushort RespawningShotBlock1x2 = 0xcb27;
    /// <summary><c>$84:CB4C InstList_PLM_2x2RespawningShotBlock</c>: breaks a four-block square, waits 384 ticks, and restores its linked layout.</summary>
    public const ushort RespawningShotBlock2x2 = 0xcb4c;
    /// <summary><c>$84:CB71 InstList_PLM_RespawningSuperMissileBlock</c>: breaks the accepted Super Missile parent with library-two maximum-six sound routing, then restores after the respawn wait.</summary>
    public const ushort RespawningSuperMissileBlock = 0xcb71;
    /// <summary><c>$84:CB94 InstList_PLM_RespawningPowerBombBlock</c>: animates the accepted Power Bomb parent's break and restores its saved level word after the respawn wait.</summary>
    public const ushort RespawningPowerBombBlock = 0xcb94;

    /// <summary><c>$84:CBB7 InstList_PLM_1x1ShotBlock</c>: sounds and animates a single block's permanent removal, then deletes without restoration.</summary>
    public const ushort PermanentShotBlock1x1 = 0xcbb7;
    /// <summary><c>$84:CBCC InstList_PLM_2x1ShotBlock</c>: animates permanent removal of a horizontal pair, then deletes without restoration.</summary>
    public const ushort PermanentShotBlock2x1 = 0xcbcc;
    /// <summary><c>$84:CBE1 InstList_PLM_1x2ShotBlock</c>: animates permanent removal of a vertical pair, then deletes without restoration.</summary>
    public const ushort PermanentShotBlock1x2 = 0xcbe1;
    /// <summary><c>$84:CBF6 InstList_PLM_2x2ShotBlock</c>: animates permanent removal of a four-block square, then deletes without restoration.</summary>
    public const ushort PermanentShotBlock2x2 = 0xcbf6;
    /// <summary><c>$84:CC0B InstList_PLM_SuperMissileBlock</c>: sounds and animates the accepted Super Missile parent's permanent removal.</summary>
    public const ushort PermanentSuperMissileBlock = 0xcc0b;
    /// <summary><c>$84:CC20 InstList_PLM_PowerBombBlock</c>: permanently removes the accepted Power Bomb parent with the native 3/2/1/1-tick draw sequence.</summary>
    public const ushort PermanentPowerBombBlock = 0xcc20;

    /// <summary><c>$84:CC35 InstList_PLM_CollisionReaction_1x1RespawningBombBlock</c>: collision sound $06, then the shared single-block break/wait/restore tail.</summary>
    public const ushort CollisionBombBlock1x1Respawning = 0xcc35;
    /// <summary><c>$84:CC3C InstList_PLM_Reaction_1x1RespawningBombBlock_0</c>: projectile sound $0A, then the shared single-block break/wait/restore tail.</summary>
    public const ushort ReactionBombBlock1x1Respawning = 0xcc3c;
    /// <summary><c>$84:CC5F InstList_PLM_Collision_2x1RespawningBombBlock</c>: collision sound $06, then the shared horizontal-pair respawn animation.</summary>
    public const ushort CollisionBombBlock2x1Respawning = 0xcc5f;
    /// <summary><c>$84:CC66 InstList_PLM_Reaction_2x1RespawningBombBlock</c>: projectile sound $0A, then the shared horizontal-pair respawn animation.</summary>
    public const ushort ReactionBombBlock2x1Respawning = 0xcc66;
    /// <summary><c>$84:CC8B InstList_PLM_Collision_1x2RespawningBombBlock</c>: collision sound $06, then the shared vertical-pair respawn animation.</summary>
    public const ushort CollisionBombBlock1x2Respawning = 0xcc8b;
    /// <summary><c>$84:CC92 InstList_PLM_Reaction_1x2RespawningBombBlock</c>: projectile sound $0A, then the shared vertical-pair respawn animation.</summary>
    public const ushort ReactionBombBlock1x2Respawning = 0xcc92;
    /// <summary><c>$84:CCB7 InstList_PLM_Collision_2x2RespawningBombBlock</c>: collision sound $06, then the shared four-block square respawn animation.</summary>
    public const ushort CollisionBombBlock2x2Respawning = 0xccb7;
    /// <summary><c>$84:CCBE InstList_PLM_Reaction_2x2RespawningBombBlock</c>: projectile sound $0A, then the shared four-block square respawn animation.</summary>
    public const ushort ReactionBombBlock2x2Respawning = 0xccbe;
    /// <summary><c>$84:CCE3 InstList_PLM_Collision_1x1RespawningBombBlock</c>: despite the native spelling, collision sound $06 enters the permanent single-block removal tail.</summary>
    public const ushort CollisionBombBlock1x1Permanent = 0xcce3;
    /// <summary><c>$84:CCEA InstList_PLM_Reaction_1x1RespawningBombBlock_4</c>: projectile sound $0A enters permanent single-block removal without restoration.</summary>
    public const ushort ReactionBombBlock1x1Permanent = 0xccea;
    /// <summary><c>$84:CCFF InstList_PLM_Collision_2x1BombBlock</c>: collision sound $06 enters permanent removal of a horizontal pair.</summary>
    public const ushort CollisionBombBlock2x1Permanent = 0xccff;
    /// <summary><c>$84:CD06 InstList_PLM_Reaction_2x1BombBlock</c>: projectile sound $0A enters permanent removal of a horizontal pair.</summary>
    public const ushort ReactionBombBlock2x1Permanent = 0xcd06;
    /// <summary><c>$84:CD1B InstList_PLM_Collision_1x2BombBlock</c>: collision sound $06 enters permanent removal of a vertical pair.</summary>
    public const ushort CollisionBombBlock1x2Permanent = 0xcd1b;
    /// <summary><c>$84:CD22 InstList_PLM_Reaction_1x2BombBlock</c>: projectile sound $0A enters permanent removal of a vertical pair.</summary>
    public const ushort ReactionBombBlock1x2Permanent = 0xcd22;
    /// <summary><c>$84:CD37 InstList_PLM_Collision_2x2BombBlock</c>: collision sound $06 enters permanent removal of a four-block square.</summary>
    public const ushort CollisionBombBlock2x2Permanent = 0xcd37;
    /// <summary><c>$84:CD3E InstList_PLM_Reaction_2x2BombBlock</c>: projectile sound $0A enters permanent removal of a four-block square.</summary>
    public const ushort ReactionBombBlock2x2Permanent = 0xcd3e;

    /// <summary><c>$84:CD6A InstList_PLM_RespawningBreakableGrappleBlock</c>.</summary>
    public const ushort RespawningBreakableGrappleBlock = 0xcd6a;

    /// <summary><c>$84:CDA9 InstList_PLM_BreakableGrappleBlock</c>.</summary>
    public const ushort PermanentBreakableGrappleBlock = 0xcda9;

    /// <summary><c>$84:D202 InstList_PLM_MotherBrainsGlass_0</c>.</summary>
    public const ushort MotherBrainGlass = 0xd202;

    /// <summary><c>$84:D368 InstList_PLM_BombTorizosCrumblingChozo</c>.</summary>
    public const ushort BombTorizoCrumblingChozo = 0xd368;

    /// <summary><c>$84:D4D4 InstList_PLM_NoobTube_0</c>.</summary>
    public const ushort NoobTube = 0xd4d4;

    /// <summary>
    /// <c>$84:DB42 InstList_PLM_SetsMetroidsClearedStatesWhenRequired</c>. The list is one
    /// permanent Sleep instruction; its selected pre-instruction owns all useful behavior.
    /// </summary>
    public const ushort SetMetroidsClearedStatesWhenRequired = 0xdb42;

    /// <summary>Selects the named collision bomb program from native $94:936B PLM headers.
    /// Indices encode 1x1, 2x1, 1x2, 2x2; where present, permanent forms follow respawning forms.</summary>
    public static ushort CollisionBombByReactionIndex(int index) => index switch
    {
        0 => CollisionBombBlock1x1Respawning,
        1 => CollisionBombBlock2x1Respawning,
        2 => CollisionBombBlock1x2Respawning,
        3 => CollisionBombBlock2x2Respawning,
        4 => CollisionBombBlock1x1Permanent,
        5 => CollisionBombBlock2x1Permanent,
        6 => CollisionBombBlock1x2Permanent,
        7 => CollisionBombBlock2x2Permanent,
        _ => throw new IndexOutOfRangeException(),
    };

    /// <summary>Selects the named bomb reaction program from native $94:A012 PLM headers.
    /// Indices encode 1x1, 2x1, 1x2, 2x2; where present, permanent forms follow respawning forms.</summary>
    public static ushort ReactionBombByReactionIndex(int index) => index switch
    {
        0 => ReactionBombBlock1x1Respawning,
        1 => ReactionBombBlock2x1Respawning,
        2 => ReactionBombBlock1x2Respawning,
        3 => ReactionBombBlock2x2Respawning,
        4 => ReactionBombBlock1x1Permanent,
        5 => ReactionBombBlock2x1Permanent,
        6 => ReactionBombBlock1x2Permanent,
        7 => ReactionBombBlock2x2Permanent,
        _ => throw new IndexOutOfRangeException(),
    };

    /// <summary>Selects the named crumble reveal program from native $94:9DA4 PLM headers.
    /// Indices encode 1x1, 2x1, 1x2, 2x2; where present, permanent forms follow respawning forms.</summary>
    public static ushort CrumbleRevealBySize(int index) => index switch
    {
        0 => CrumbleReveal1x1,
        1 => CrumbleReveal2x1,
        2 => CrumbleReveal1x2,
        3 => CrumbleReveal2x2,
        _ => throw new IndexOutOfRangeException(),
    };

    /// <summary>Selects the named contact crumble program from native $94:9139 PLM headers.
    /// Indices encode 1x1, 2x1, 1x2, 2x2; where present, permanent forms follow respawning forms.</summary>
    public static ushort ContactCrumbleByReactionIndex(int index) => index switch
    {
        0 => ContactCrumble1x1Respawning,
        1 => ContactCrumble2x1Respawning,
        2 => ContactCrumble1x2Respawning,
        3 => ContactCrumble2x2Respawning,
        4 => ContactCrumble1x1Permanent,
        5 => ContactCrumble2x1Permanent,
        6 => ContactCrumble1x2Permanent,
        7 => ContactCrumble2x2Permanent,
        _ => throw new IndexOutOfRangeException(),
    };

    /// <summary>Selects the native respawning-shot program for size 0=1x1,
    /// 1=2x1, 2=1x2, 3=2x2, from PLM instruction fields $84:D066/D06A/D06E/D072.</summary>
    public static ushort RespawningShotBySize(int sizeIndex) => sizeIndex switch
    {
        0 => RespawningShotBlock1x1,
        1 => RespawningShotBlock2x1,
        2 => RespawningShotBlock1x2,
        3 => RespawningShotBlock2x2,
        _ => throw new IndexOutOfRangeException(),
    };
    /// <summary>Selects the native permanent-shot program for size 0=1x1,
    /// 1=2x1, 2=1x2, 3=2x2, from PLM instruction fields $84:D076/D07A/D07E/D082.</summary>
    public static ushort PermanentShotBySize(int sizeIndex) => sizeIndex switch
    {
        0 => PermanentShotBlock1x1,
        1 => PermanentShotBlock2x1,
        2 => PermanentShotBlock1x2,
        3 => PermanentShotBlock2x2,
        _ => throw new IndexOutOfRangeException(),
    };
}
