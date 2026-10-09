namespace SuperMetroid.Core.Rooms;

/// <summary>Named bank-$84 PLM header pointers consumed by room integration code.</summary>
/// <remarks>
/// Each <see cref="ushort"/> value is the low word of a CPU address in fixed bank $84.
/// Names follow the cartridge's native entry labels. The three collectible groups retain
/// all 21 concrete headers rather than hiding their identities behind unchecked arithmetic.
/// </remarks>
internal static class RoomPlmHeaders
{
    /// <summary>Resident five-block downward gate at <c>$84:C82A</c>.</summary>
    public const ushort DownwardGate = 0xc82a;

    /// <summary>Room-authored downward-gate shot block at <c>$84:C836</c>.</summary>
    public const ushort DownwardGateShotBlock = 0xc836;
    /// <summary>Number of concrete headers in each permanent-collectible presentation.</summary>
    public const int PermanentCollectibleKindCount = 21;

    /// <summary>Extend a horizontal block chain toward increasing X at $84:B63B.</summary>
    public const ushort RightwardsScrollExtension = 0xb63b;
    /// <summary>Extend a horizontal block chain toward decreasing X at $84:B63F.</summary>
    public const ushort LeftwardsScrollExtension = 0xb63f;
    /// <summary>Extend a vertical block chain toward increasing Y at $84:B643.</summary>
    public const ushort DownwardsScrollExtension = 0xb643;
    /// <summary>Extend a vertical block chain toward decreasing Y at $84:B647.</summary>
    public const ushort UpwardsScrollExtension = 0xb647;

    /// <summary>Resident special-air scroll trigger at $84:B703.</summary>
    public const ushort ScrollTrigger = 0xb703;

    /// <summary>Wrecked Ship entrance treadmill entered from the west at $84:B64B.</summary>
    public const ushort WreckedShipEntranceTreadmillFromWest = 0xb64b;

    /// <summary>Wrecked Ship entrance treadmill entered from the east at $84:B64F.</summary>
    public const ushort WreckedShipEntranceTreadmillFromEast = 0xb64f;

    /// <summary>Fill Mother Brain's wall at $84:B673.</summary>
    public const ushort FillMotherBrainsWall = 0xb673;
    /// <summary>Open Mother Brain's escape door at $84:B677.</summary>
    public const ushort MotherBrainsRoomEscapeDoor = 0xb677;

    // The fake-death sequence requests these row/tube actors by exact identity. Their
    // descriptive names keep the boss state machine free of an opaque address matrix.
    /// <summary>Mother Brain background row 2 actor used while clearing the boss room at <c>$84:B67B</c>.</summary>
    public const ushort MotherBrainsBackgroundRow2 = 0xb67b;
    /// <summary>Mother Brain background row 3 actor used while clearing the boss room at <c>$84:B67F</c>.</summary>
    public const ushort MotherBrainsBackgroundRow3 = 0xb67f;
    /// <summary>Mother Brain background row 4 actor used while clearing the boss room at <c>$84:B683</c>.</summary>
    public const ushort MotherBrainsBackgroundRow4 = 0xb683;
    /// <summary>Mother Brain background row 5 actor used while clearing the boss room at <c>$84:B687</c>.</summary>
    public const ushort MotherBrainsBackgroundRow5 = 0xb687;
    /// <summary>Mother Brain background row 6 actor used while clearing the boss room at <c>$84:B68B</c>.</summary>
    public const ushort MotherBrainsBackgroundRow6 = 0xb68b;
    /// <summary>Mother Brain background row 7 actor used while clearing the boss room at <c>$84:B68F</c>.</summary>
    public const ushort MotherBrainsBackgroundRow7 = 0xb68f;
    /// <summary>Mother Brain background row 8 actor used while clearing the boss room at <c>$84:B693</c>.</summary>
    public const ushort MotherBrainsBackgroundRow8 = 0xb693;
    /// <summary>Mother Brain background row 9 actor used while clearing the boss room at <c>$84:B697</c>.</summary>
    public const ushort MotherBrainsBackgroundRow9 = 0xb697;
    /// <summary>Mother Brain background row A actor used while clearing the boss room at <c>$84:B69B</c>.</summary>
    public const ushort MotherBrainsBackgroundRowA = 0xb69b;
    /// <summary>Mother Brain background row B actor used while clearing the boss room at <c>$84:B69F</c>.</summary>
    public const ushort MotherBrainsBackgroundRowB = 0xb69f;
    /// <summary>Mother Brain background row C actor used while clearing the boss room at <c>$84:B6A3</c>.</summary>
    public const ushort MotherBrainsBackgroundRowC = 0xb6a3;
    /// <summary>Mother Brain background row D actor used while clearing the boss room at <c>$84:B6A7</c>.</summary>
    public const ushort MotherBrainsBackgroundRowD = 0xb6a7;
    /// <summary>Clears the ceiling block above Mother Brain's chamber at <c>$84:B6B3</c>.</summary>
    public const ushort ClearMotherBrainCeilingBlock = 0xb6b3;
    /// <summary>Clears the ceiling tube above Mother Brain's chamber at <c>$84:B6B7</c>.</summary>
    public const ushort ClearMotherBrainCeilingTube = 0xb6b7;
    /// <summary>Clears the side tube at the bottom middle of Mother Brain's chamber at <c>$84:B6BB</c>.</summary>
    public const ushort ClearMotherBrainBottomMiddleSideTube = 0xb6bb;
    /// <summary>Clears the paired bottom-middle tubes in Mother Brain's chamber at <c>$84:B6BF</c>.</summary>
    public const ushort ClearMotherBrainBottomMiddleTubes = 0xb6bf;
    /// <summary>Clears the bottom-left tube in Mother Brain's chamber at <c>$84:B6C3</c>.</summary>
    public const ushort ClearMotherBrainBottomLeftTube = 0xb6c3;
    /// <summary>Clears the bottom-right tube in Mother Brain's chamber at <c>$84:B6C7</c>.</summary>
    public const ushort ClearMotherBrainBottomRightTube = 0xb6c7;

    /// <summary>Main map-station room actor at $84:B6D3.</summary>
    public const ushort MapStation = 0xb6d3;
    /// <summary>Main energy recharge station actor that refills energy on interaction at <c>$84:B6DF</c>.</summary>
    public const ushort EnergyStation = 0xb6df;
    /// <summary>Main missile recharge station actor that refills missiles on interaction at <c>$84:B6EB</c>.</summary>
    public const ushort MissileStation = 0xb6eb;

    /// <summary>Ordinary elevator-platform room actor at $84:B70B.</summary>
    public const ushort ElevatorPlatform = 0xb70b;

    /// <summary>Clear Crocomire bridge PLM at $84:B747.</summary>
    public const ushort ClearCrocomireBridge = 0xb747;

    /// <summary>Crumble one Crocomire bridge block PLM at $84:B74B.</summary>
    public const ushort CrumbleCrocomireBridgeBlock = 0xb74b;

    /// <summary>Clear one Crocomire bridge block PLM at $84:B74F.</summary>
    public const ushort ClearCrocomireBridgeBlock = 0xb74f;

    /// <summary>Clear Crocomire's invisible wall PLM at $84:B753.</summary>
    public const ushort ClearCrocomireInvisibleWall = 0xb753;

    /// <summary>Create Crocomire's invisible wall PLM at $84:B757.</summary>
    public const ushort CreateCrocomireInvisibleWall = 0xb757;

    /// <summary>Clear the Baby Metroid encounter's invisible wall at $84:B763.</summary>
    public const ushort ClearBabyMetroidInvisibleWall = 0xb763;
    /// <summary>Create the Baby Metroid encounter's invisible wall at $84:B767.</summary>
    public const ushort CreateBabyMetroidInvisibleWall = 0xb767;
    /// <summary>Main save-station room actor at $84:B76F.</summary>
    public const ushort SaveStation = 0xb76f;
    /// <summary>Draw Phantoon's closed arena door at $84:B781.</summary>
    public const ushort DrawPhantoonDoorDuringBossFight = 0xb781;
    /// <summary>Restore Phantoon's door after the fight at $84:B78B.</summary>
    public const ushort RestorePhantoonDoorAfterBossFight = 0xb78b;

    /// <summary>Crumble Spore Spawn's ceiling PLM at $84:B78F.</summary>
    public const ushort CrumbleSporeSpawnCeiling = 0xb78f;

    /// <summary>Clear Spore Spawn's ceiling PLM at $84:B793.</summary>
    public const ushort ClearSporeSpawnCeiling = 0xb793;

    /// <summary>Clear Botwoon's wall PLM at $84:B797.</summary>
    public const ushort ClearBotwoonWall = 0xb797;

    /// <summary>Crumble Botwoon's wall PLM at $84:B79B.</summary>
    public const ushort CrumbleBotwoonWall = 0xb79b;

    /// <summary>Kraid ceiling crumble using background variant one at $84:B7A3.</summary>
    public const ushort CrumbleKraidCeilingIntoBackground1 = 0xb7a3;
    /// <summary>Kraid platform crumble using the first floor variant at $84:B7A7.</summary>
    public const ushort CrumbleKraidPlatformVariant1 = 0xb7a7;
    /// <summary>Kraid ceiling crumble using background variant two at $84:B7AB.</summary>
    public const ushort CrumbleKraidCeilingIntoBackground2 = 0xb7ab;
    /// <summary>Kraid platform crumble using the second floor variant at $84:B7AF.</summary>
    public const ushort CrumbleKraidPlatformVariant2 = 0xb7af;
    /// <summary>Kraid ceiling crumble using background variant three at $84:B7B3.</summary>
    public const ushort CrumbleKraidCeilingIntoBackground3 = 0xb7b3;
    /// <summary>Clear Kraid's ceiling after an already-defeated room load at $84:B7B7.</summary>
    public const ushort ClearKraidCeiling = 0xb7b7;
    /// <summary>Clear Kraid-room spikes after an already-defeated room load at $84:B7BB.</summary>
    public const ushort ClearKraidSpikes = 0xb7bb;

    /// <summary><c>$84:B7BF PLMEntries_crumbleKraidSpikeBlocks</c>: animated live-death floor sweep.</summary>
    public const ushort CrumbleKraidSpikes = 0xb7bf;

    /// <summary>Speed Booster escape lavaquake controller at $84:B8AC.</summary>
    public const ushort SpeedBoosterEscape = 0xb8ac;

    /// <summary>Maridia elevatube delay/sound PLM at $84:B8F9.</summary>
    public const ushort MaridiaElevatube = 0xb8f9;

    /// <summary>
    /// Old Tourian escape shaft fake wall at $84:B964, spawned by room setup $8F:91A9 during
    /// the escape; explodes once Samus passes below and right of it.
    /// </summary>
    public const ushort OldTourianEscapeShaftFakeWall = 0xb964;

    /// <summary>
    /// Crateria mainstreet escape-passage clearer at $84:BB30, spawned by room setup $8F:9194;
    /// its setup deletes it unless the critters escaped.
    /// </summary>
    public const ushort CrateriaMainstreetEscapePassage = 0xbb30;

    /// <summary>Resident Wrecked Ship attic no-op observer at $84:BB05.</summary>
    public const ushort WreckedShipAttic = 0xbb05;

    /// <summary>Bomb Torizo's exceptional right-facing grey door at $84:BAF4.</summary>
    public const ushort BombTorizoGreyDoor = 0xbaf4;

    // Door families use a six-byte header stride in left/right/up/down order. Naming each
    // address prevents orientation arithmetic from silently accepting an in-between word.
    /// <summary>Standard grey door header for a left-facing opening at <c>$84:C842</c>.</summary>
    public const ushort GreyDoorFacingLeft = 0xc842;
    /// <summary>Standard grey door header for a right-facing opening at <c>$84:C848</c>.</summary>
    public const ushort GreyDoorFacingRight = 0xc848;
    /// <summary>Standard grey door header for an upward-facing opening at <c>$84:C84E</c>.</summary>
    public const ushort GreyDoorFacingUp = 0xc84e;
    /// <summary>Standard grey door header for a downward-facing opening at <c>$84:C854</c>.</summary>
    public const ushort GreyDoorFacingDown = 0xc854;

    /// <summary>Standard yellow door header for a left-facing opening at <c>$84:C85A</c>.</summary>
    public const ushort YellowDoorFacingLeft = 0xc85a;
    /// <summary>Standard yellow door header for a right-facing opening at <c>$84:C860</c>.</summary>
    public const ushort YellowDoorFacingRight = 0xc860;
    /// <summary>Standard yellow door header for an upward-facing opening at <c>$84:C866</c>.</summary>
    public const ushort YellowDoorFacingUp = 0xc866;
    /// <summary>Standard yellow door header for a downward-facing opening at <c>$84:C86C</c>.</summary>
    public const ushort YellowDoorFacingDown = 0xc86c;
    /// <summary>Standard green door header for a left-facing opening at <c>$84:C872</c>.</summary>
    public const ushort GreenDoorFacingLeft = 0xc872;
    /// <summary>Standard green door header for a right-facing opening at <c>$84:C878</c>.</summary>
    public const ushort GreenDoorFacingRight = 0xc878;
    /// <summary>Standard green door header for an upward-facing opening at <c>$84:C87E</c>.</summary>
    public const ushort GreenDoorFacingUp = 0xc87e;
    /// <summary>Standard green door header for a downward-facing opening at <c>$84:C884</c>.</summary>
    public const ushort GreenDoorFacingDown = 0xc884;
    /// <summary>Standard red door header for a left-facing opening at <c>$84:C88A</c>.</summary>
    public const ushort RedDoorFacingLeft = 0xc88a;
    /// <summary>Standard red door header for a right-facing opening at <c>$84:C890</c>.</summary>
    public const ushort RedDoorFacingRight = 0xc890;
    /// <summary>Standard red door header for an upward-facing opening at <c>$84:C896</c>.</summary>
    public const ushort RedDoorFacingUp = 0xc896;
    /// <summary>Standard red door header for a downward-facing opening at <c>$84:C89C</c>.</summary>
    public const ushort RedDoorFacingDown = 0xc89c;

    /// <summary>Standard blue door header for a left-facing opening at <c>$84:C8A2</c>.</summary>
    public const ushort BlueDoorFacingLeft = 0xc8a2;
    /// <summary>Standard blue door header for a right-facing opening at <c>$84:C8A8</c>.</summary>
    public const ushort BlueDoorFacingRight = 0xc8a8;
    /// <summary>Standard blue door header for an upward-facing opening at <c>$84:C8AE</c>.</summary>
    public const ushort BlueDoorFacingUp = 0xc8ae;
    /// <summary>Standard blue door header for a downward-facing opening at <c>$84:C8B4</c>.</summary>
    public const ushort BlueDoorFacingDown = 0xc8b4;

    /// <summary>Door-transition-only blue-door closer facing left at $84:C8BA.</summary>
    public const ushort BlueDoorClosingFacingLeft = 0xc8ba;
    /// <summary>Door-transition-only blue-door closer facing right at $84:C8BE.</summary>
    public const ushort BlueDoorClosingFacingRight = 0xc8be;
    /// <summary>Door-transition-only blue-door closer facing up at $84:C8C2.</summary>
    public const ushort BlueDoorClosingFacingUp = 0xc8c2;
    /// <summary>Door-transition-only blue-door closer facing down at $84:C8C6.</summary>
    public const ushort BlueDoorClosingFacingDown = 0xc8c6;

    /// <summary>
    /// Resident gate at $84:C8CA in Tourian escape room one. Its second header list is
    /// selected by the shared door-transition closer when entering from Mother Brain.
    /// </summary>
    public const ushort MotherBrainEscapeRoomGate = 0xc8ca;

    /// <summary>
    /// Door-transition-only fallback gate closer at $84:C8D0, used when a special
    /// direction-$8..$B door has no resident cap at its authored block coordinate.
    /// </summary>
    public const ushort MotherBrainEscapeRoomGateClosing = 0xc8d0;

    /// <summary>
    /// <c>$84:D030 PLMEntries_Collision_BTS82</c>: Brinstar BTS <c>$82</c>, a
    /// respawning Speed Booster block with the slower crumble animation.
    /// </summary>
    public const ushort SpeedBlockBrinstarSlowRespawning = 0xd030;

    /// <summary>
    /// <c>$84:D034 PLMEntries_Collision_BTS83</c>: Brinstar BTS <c>$83</c>, a
    /// permanent Speed Booster block with the slower crumble animation.
    /// </summary>
    public const ushort SpeedBlockBrinstarSlowPermanent = 0xd034;

    /// <summary>
    /// <c>$84:D038 PLMEntries_Collision_RespawningSpeedBoostBlock</c>: the
    /// area-independent BTS <c>$0E</c> respawning Speed Booster block.
    /// </summary>
    public const ushort SpeedBlockRespawning = 0xd038;

    /// <summary>
    /// <c>$84:D03C PLMEntries_Collision_DachoraRespawningSpeedBoostBlock</c>:
    /// Brinstar BTS <c>$84</c>, the Dachora-room respawning Speed Booster block.
    /// </summary>
    public const ushort SpeedBlockDachoraRespawning = 0xd03c;

    /// <summary>
    /// <c>$84:D040 PLMEntries_Collision_SpeedBoostBlock</c>: area-independent
    /// BTS <c>$0F</c> and Brinstar BTS <c>$85</c>, a permanent Speed Booster block.
    /// </summary>
    public const ushort SpeedBlockPermanent = 0xd040;

    /// <summary>Respawning 1x1 Samus-contact crumble block at <c>$84:D044</c>.</summary>
    public const ushort ContactCrumble1x1Respawning = 0xd044;
    /// <summary>Respawning 2x1 Samus-contact crumble block at <c>$84:D048</c>.</summary>
    public const ushort ContactCrumble2x1Respawning = 0xd048;
    /// <summary>Respawning 1x2 Samus-contact crumble block at <c>$84:D04C</c>.</summary>
    public const ushort ContactCrumble1x2Respawning = 0xd04c;
    /// <summary>Respawning 2x2 Samus-contact crumble block at <c>$84:D050</c>.</summary>
    public const ushort ContactCrumble2x2Respawning = 0xd050;
    /// <summary>Permanent 1x1 Samus-contact crumble block at <c>$84:D054</c>.</summary>
    public const ushort ContactCrumble1x1Permanent = 0xd054;
    /// <summary>Permanent 2x1 Samus-contact crumble block at <c>$84:D058</c>.</summary>
    public const ushort ContactCrumble2x1Permanent = 0xd058;
    /// <summary>Permanent 1x2 Samus-contact crumble block at <c>$84:D05C</c>.</summary>
    public const ushort ContactCrumble1x2Permanent = 0xd05c;
    /// <summary>Permanent 2x2 Samus-contact crumble block at <c>$84:D060</c>.</summary>
    public const ushort ContactCrumble2x2Permanent = 0xd060;

    /// <summary>Selects the named contact-crumble header for bank-$94 special-block
    /// BTS $00..07 at $94:9139. Sizes 1x1, 2x1, 1x2, 2x2 appear first as
    /// respawning blocks, then as permanent blocks.</summary>
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

    /// <summary>Mother Brain's missile-reactive glass actor at $84:D6DE.</summary>
    public const ushort MotherBrainGlass = 0xd6de;
    /// <summary>Bomb Torizo's Chozo-hand synchronization actor at $84:D6EA.</summary>
    public const ushort BombTorizoHand = 0xd6ea;

    /// <summary>Maridia's power-bomb-reactive n00b tube actor at $84:D70C.</summary>
    public const ushort NoobTube = 0xd70c;

    /// <summary>
    /// Resident room-kill observer at $84:DB44 which marks the four Tourian Metroid-room
    /// events when their authored enemy death quotas have been reached.
    /// </summary>
    public const ushort SetMetroidsClearedStatesWhenRequired = 0xdb44;

    /// <summary>Right-facing eye-door eye controller at <c>$84:DB48</c>.</summary>
    public const ushort EyeDoorEyeFacingRight = 0xdb48;
    /// <summary>Right-facing eye-door middle/door component at <c>$84:DB4C</c>.</summary>
    public const ushort EyeDoorFacingRight = 0xdb4c;
    /// <summary>Right-facing eye-door bottom component at <c>$84:DB52</c>.</summary>
    public const ushort EyeDoorBottomFacingRight = 0xdb52;
    /// <summary>Left-facing eye-door eye controller at <c>$84:DB56</c>.</summary>
    public const ushort EyeDoorEyeFacingLeft = 0xdb56;
    /// <summary>Left-facing eye-door middle/door component at <c>$84:DB5A</c>.</summary>
    public const ushort EyeDoorFacingLeft = 0xdb5a;
    /// <summary>Left-facing eye-door bottom component at <c>$84:DB60</c>.</summary>
    public const ushort EyeDoorBottomFacingLeft = 0xdb60;

    /// <summary>Draygon-room right-facing shielded cannon at <c>$84:DF59</c>.</summary>
    public const ushort DraygonCannonFacingRight = 0xdf59;
    /// <summary>
    /// Draygon-room right-facing cannon at <c>$84:DF65</c>. Its authored list enters the
    /// destroyed state immediately, disabling the unused upper-left firing position.
    /// </summary>
    public const ushort DraygonCannonFacingRightDestroyed = 0xdf65;
    /// <summary>Draygon-room left-facing shielded cannon at <c>$84:DF71</c>.</summary>
    public const ushort DraygonCannonFacingLeft = 0xdf71;

    // Exposed permanent-item headers. The order matches InWorldCollectibleKind exactly.
    /// <summary>Exposed energy-tank collectible header used by the permanent-item order at <c>$84:EED7</c>.</summary>
    public const ushort ExposedEnergyTank = 0xeed7;
    /// <summary>Exposed missile-tank collectible header used by the permanent-item order at <c>$84:EEDB</c>.</summary>
    public const ushort ExposedMissileTank = 0xeedb;
    /// <summary>Exposed super-missile-tank collectible header used by the permanent-item order at <c>$84:EEDF</c>.</summary>
    public const ushort ExposedSuperMissileTank = 0xeedf;
    /// <summary>Exposed power-bomb-tank collectible header used by the permanent-item order at <c>$84:EEE3</c>.</summary>
    public const ushort ExposedPowerBombTank = 0xeee3;
    /// <summary>Exposed Morph Ball collectible header used by the permanent-item order at <c>$84:EF23</c>.</summary>
    public const ushort ExposedMorphBall = 0xef23;

    // Chozo-orb permanent-item headers. The order matches InWorldCollectibleKind exactly.
    /// <summary>Chozo-orb energy-tank collectible header at <c>$84:EF2B</c>.</summary>
    public const ushort ChozoEnergyTank = 0xef2b;
    /// <summary>Chozo-orb missile-tank collectible header at <c>$84:EF2F</c>.</summary>
    public const ushort ChozoMissileTank = 0xef2f;
    /// <summary>Chozo-orb super-missile-tank collectible header at <c>$84:EF33</c>.</summary>
    public const ushort ChozoSuperMissileTank = 0xef33;
    /// <summary>Chozo-orb power-bomb-tank collectible header at <c>$84:EF37</c>.</summary>
    public const ushort ChozoPowerBombTank = 0xef37;
    /// <summary>Chozo-orb bombs collectible header at <c>$84:EF3B</c>.</summary>
    public const ushort ChozoBombs = 0xef3b;
    /// <summary>Chozo-orb Charge Beam collectible header at <c>$84:EF3F</c>.</summary>
    public const ushort ChozoChargeBeam = 0xef3f;
    /// <summary>Chozo-orb Ice Beam collectible header at <c>$84:EF43</c>.</summary>
    public const ushort ChozoIceBeam = 0xef43;
    /// <summary>Chozo-orb Hi-Jump Boots collectible header at <c>$84:EF47</c>.</summary>
    public const ushort ChozoHiJumpBoots = 0xef47;
    /// <summary>Chozo-orb Speed Booster collectible header at <c>$84:EF4B</c>.</summary>
    public const ushort ChozoSpeedBooster = 0xef4b;
    /// <summary>Chozo-orb Wave Beam collectible header at <c>$84:EF4F</c>.</summary>
    public const ushort ChozoWaveBeam = 0xef4f;
    /// <summary>Chozo-orb Spazer Beam collectible header at <c>$84:EF53</c>.</summary>
    public const ushort ChozoSpazerBeam = 0xef53;
    /// <summary>Chozo-orb Spring Ball collectible header at <c>$84:EF57</c>.</summary>
    public const ushort ChozoSpringBall = 0xef57;
    /// <summary>Chozo-orb Varia Suit collectible header at <c>$84:EF5B</c>.</summary>
    public const ushort ChozoVariaSuit = 0xef5b;
    /// <summary>Chozo-orb Gravity Suit collectible header at <c>$84:EF5F</c>.</summary>
    public const ushort ChozoGravitySuit = 0xef5f;
    /// <summary>Chozo-orb X-Ray Scope collectible header at <c>$84:EF63</c>.</summary>
    public const ushort ChozoXrayScope = 0xef63;
    /// <summary>Chozo-orb Plasma Beam collectible header at <c>$84:EF67</c>.</summary>
    public const ushort ChozoPlasmaBeam = 0xef67;
    /// <summary>Chozo-orb Grapple Beam collectible header at <c>$84:EF6B</c>.</summary>
    public const ushort ChozoGrappleBeam = 0xef6b;
    /// <summary>Chozo-orb Space Jump collectible header at <c>$84:EF6F</c>.</summary>
    public const ushort ChozoSpaceJump = 0xef6f;
    /// <summary>Chozo-orb Screw Attack collectible header at <c>$84:EF73</c>.</summary>
    public const ushort ChozoScrewAttack = 0xef73;
    /// <summary>Chozo-orb reserve-tank collectible header at <c>$84:EF7B</c>.</summary>
    public const ushort ChozoReserveTank = 0xef7b;

    // Concealed shot-block permanent-item headers. The order matches
    // InWorldCollectibleKind exactly.
    /// <summary>Shot-block-concealed energy-tank collectible header at <c>$84:EF7F</c>.</summary>
    public const ushort ShotBlockEnergyTank = 0xef7f;
    /// <summary>Shot-block-concealed missile-tank collectible header at <c>$84:EF83</c>.</summary>
    public const ushort ShotBlockMissileTank = 0xef83;
    /// <summary>Shot-block-concealed super-missile-tank collectible header at <c>$84:EF87</c>.</summary>
    public const ushort ShotBlockSuperMissileTank = 0xef87;
}
