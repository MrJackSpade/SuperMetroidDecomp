namespace SuperMetroid.Core.Rooms;

/// <summary>Named bank-$84 PLM instruction-list offsets used by translated room actors.</summary>
/// <remarks>
/// Values are native low-word pointers in fixed bank $84. List identities remain separate
/// from executable opcode identities in <see cref="RoomPlmInstructionCodes"/> and from
/// draw-list pointers, even though all three domains share the same 16-bit storage type.
/// </remarks>
public static class RoomPlmInstructionLists
{
    /// <summary>Downward gate's close-and-wait loop at <c>$84:BC13</c>.</summary>
    public const ushort DownwardGateClosing = 0xbc13;

    /// <summary>Downward gate's open-and-wait loop at <c>$84:BC3A</c>.</summary>
    public const ushort DownwardGateOpening = 0xbc3a;

    /// <summary><c>$84:BCAF InstList_PLM_DownwardsGateShotblock_BlueLeft</c>.</summary>
    public const ushort DownwardGateShotBlockBlueLeft = 0xbcaf;
    /// <summary><c>$84:BCB5 InstList_PLM_DownwardsGateShotblock_BlueRight</c>.</summary>
    public const ushort DownwardGateShotBlockBlueRight = 0xbcb5;
    /// <summary><c>$84:BCBB InstList_PLM_DownwardsGateShotblock_RedLeft</c>.</summary>
    public const ushort DownwardGateShotBlockRedLeft = 0xbcbb;
    /// <summary><c>$84:BCC1 InstList_PLM_DownwardsGateShotblock_RedRight</c>.</summary>
    public const ushort DownwardGateShotBlockRedRight = 0xbcc1;
    /// <summary><c>$84:BCC7 InstList_PLM_DownwardsGateShotblock_GreenLeft</c>.</summary>
    public const ushort DownwardGateShotBlockGreenLeft = 0xbcc7;
    /// <summary><c>$84:BCCD InstList_PLM_DownwardsGateShotblock_GreenRight</c>.</summary>
    public const ushort DownwardGateShotBlockGreenRight = 0xbccd;
    /// <summary><c>$84:BCD3 InstList_PLM_DownwardsGateShotblock_YellowLeft</c>.</summary>
    public const ushort DownwardGateShotBlockYellowLeft = 0xbcd3;
    /// <summary><c>$84:BCD9 InstList_PLM_DownwardsGateShotblock_YellowRight</c>.</summary>
    public const ushort DownwardGateShotBlockYellowRight = 0xbcd9;
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

    public const ushort FillMotherBrainsWall = 0xac05;
    /// <summary>$84:AC0B draws the opened escape door then deletes the one-shot PLM.</summary>
    public const ushort MotherBrainsRoomEscapeDoor = 0xac0b;
    public const ushort MotherBrainsBackgroundRow2 = 0xac11;
    public const ushort MotherBrainsBackgroundRow3 = 0xac17;
    public const ushort MotherBrainsBackgroundRow4 = 0xac1d;
    public const ushort MotherBrainsBackgroundRow5 = 0xac23;
    public const ushort MotherBrainsBackgroundRow6 = 0xac29;
    public const ushort MotherBrainsBackgroundRow7 = 0xac2f;
    public const ushort MotherBrainsBackgroundRow8 = 0xac35;
    public const ushort MotherBrainsBackgroundRow9 = 0xac3b;
    public const ushort MotherBrainsBackgroundRowA = 0xac41;
    public const ushort MotherBrainsBackgroundRowB = 0xac47;
    public const ushort MotherBrainsBackgroundRowC = 0xac4d;
    public const ushort MotherBrainsBackgroundRowD = 0xac53;
    public const ushort ClearMotherBrainCeilingBlock = 0xac65;
    public const ushort ClearMotherBrainCeilingTube = 0xac6b;
    public const ushort ClearMotherBrainBottomMiddleSideTube = 0xac71;
    public const ushort ClearMotherBrainBottomMiddleTubes = 0xac77;
    public const ushort ClearMotherBrainBottomLeftTube = 0xac7d;
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

    /// <summary>Entry two bytes into the scroll list after collision wakes it.</summary>
    public const ushort ScrollTriggerActivated = 0xaf8c;

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

    public const ushort BlueDoorFacingLeftOpening = 0xc489;
    public const ushort BlueDoorFacingRightOpening = 0xc4ba;
    public const ushort BlueDoorFacingUpOpening = 0xc4eb;
    public const ushort BlueDoorFacingDownOpening = 0xc51c;

    /// <summary>
    /// <c>$84:BB34 InstList_PLM_GateThatClosesDuringEscapeAfterMotherBrain_0</c>:
    /// draw the already-closed gate for six frames, then release the resident PLM slot.
    /// </summary>
    public const ushort MotherBrainEscapeRoomGateClosed = 0xbb34;

    /// <summary>
    /// <c>$84:BB44 InstList_PLM_GateThatClosesDuringEscapeAfterMotherBrain_1</c>:
    /// animate open, half-closed, and closed at two frames apiece, then release the slot.
    /// </summary>
    public const ushort MotherBrainEscapeRoomGateClosing = 0xbb44;

    public const ushort CrumbleReveal1x1 = 0xc8ec;
    public const ushort CrumbleReveal2x1 = 0xc8f2;
    public const ushort CrumbleReveal1x2 = 0xc8f8;
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

    public const ushort BombedPowerBombBlockUnused = 0xc91c;
    public const ushort BombedSuperMissileBlockUnused = 0xc922;
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

    public const ushort RespawningShotBlock1x1 = 0xcadf;
    public const ushort RespawningShotBlock2x1 = 0xcb02;
    public const ushort RespawningShotBlock1x2 = 0xcb27;
    public const ushort RespawningShotBlock2x2 = 0xcb4c;
    public const ushort RespawningSuperMissileBlock = 0xcb71;
    public const ushort RespawningPowerBombBlock = 0xcb94;

    public const ushort PermanentShotBlock1x1 = 0xcbb7;
    public const ushort PermanentShotBlock2x1 = 0xcbcc;
    public const ushort PermanentShotBlock1x2 = 0xcbe1;
    public const ushort PermanentShotBlock2x2 = 0xcbf6;
    public const ushort PermanentSuperMissileBlock = 0xcc0b;
    public const ushort PermanentPowerBombBlock = 0xcc20;

    public const ushort CollisionBombBlock1x1Respawning = 0xcc35;
    public const ushort ReactionBombBlock1x1Respawning = 0xcc3c;
    public const ushort CollisionBombBlock2x1Respawning = 0xcc5f;
    public const ushort ReactionBombBlock2x1Respawning = 0xcc66;
    public const ushort CollisionBombBlock1x2Respawning = 0xcc8b;
    public const ushort ReactionBombBlock1x2Respawning = 0xcc92;
    public const ushort CollisionBombBlock2x2Respawning = 0xccb7;
    public const ushort ReactionBombBlock2x2Respawning = 0xccbe;
    public const ushort CollisionBombBlock1x1Permanent = 0xcce3;
    public const ushort ReactionBombBlock1x1Permanent = 0xccea;
    public const ushort CollisionBombBlock2x1Permanent = 0xccff;
    public const ushort ReactionBombBlock2x1Permanent = 0xcd06;
    public const ushort CollisionBombBlock1x2Permanent = 0xcd1b;
    public const ushort ReactionBombBlock1x2Permanent = 0xcd22;
    public const ushort CollisionBombBlock2x2Permanent = 0xcd37;
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

    private static readonly ushort[] CollisionBombLists =
    [
        CollisionBombBlock1x1Respawning,
        CollisionBombBlock2x1Respawning,
        CollisionBombBlock1x2Respawning,
        CollisionBombBlock2x2Respawning,
        CollisionBombBlock1x1Permanent,
        CollisionBombBlock2x1Permanent,
        CollisionBombBlock1x2Permanent,
        CollisionBombBlock2x2Permanent,
    ];

    private static readonly ushort[] ReactionBombLists =
    [
        ReactionBombBlock1x1Respawning,
        ReactionBombBlock2x1Respawning,
        ReactionBombBlock1x2Respawning,
        ReactionBombBlock2x2Respawning,
        ReactionBombBlock1x1Permanent,
        ReactionBombBlock2x1Permanent,
        ReactionBombBlock1x2Permanent,
        ReactionBombBlock2x2Permanent,
    ];

    private static readonly ushort[] RespawningShotLists =
    [
        RespawningShotBlock1x1,
        RespawningShotBlock2x1,
        RespawningShotBlock1x2,
        RespawningShotBlock2x2,
    ];

    private static readonly ushort[] PermanentShotLists =
    [
        PermanentShotBlock1x1,
        PermanentShotBlock2x1,
        PermanentShotBlock1x2,
        PermanentShotBlock2x2,
    ];

    private static readonly ushort[] CrumbleRevealLists =
    [
        CrumbleReveal1x1,
        CrumbleReveal2x1,
        CrumbleReveal1x2,
        CrumbleReveal2x2,
    ];

    private static readonly ushort[] ContactCrumbleLists =
    [
        ContactCrumble1x1Respawning,
        ContactCrumble2x1Respawning,
        ContactCrumble1x2Respawning,
        ContactCrumble2x2Respawning,
        ContactCrumble1x1Permanent,
        ContactCrumble2x1Permanent,
        ContactCrumble1x2Permanent,
        ContactCrumble2x2Permanent,
    ];

    public static ReadOnlySpan<ushort> CollisionBombByReactionIndex => CollisionBombLists;
    public static ReadOnlySpan<ushort> ReactionBombByReactionIndex => ReactionBombLists;
    public static ReadOnlySpan<ushort> RespawningShotBySize => RespawningShotLists;
    public static ReadOnlySpan<ushort> PermanentShotBySize => PermanentShotLists;
    public static ReadOnlySpan<ushort> CrumbleRevealBySize => CrumbleRevealLists;
    public static ReadOnlySpan<ushort> ContactCrumbleByReactionIndex => ContactCrumbleLists;
}
