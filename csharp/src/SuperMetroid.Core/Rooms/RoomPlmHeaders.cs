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
    public const ushort MotherBrainsBackgroundRow2 = 0xb67b;
    public const ushort MotherBrainsBackgroundRow3 = 0xb67f;
    public const ushort MotherBrainsBackgroundRow4 = 0xb683;
    public const ushort MotherBrainsBackgroundRow5 = 0xb687;
    public const ushort MotherBrainsBackgroundRow6 = 0xb68b;
    public const ushort MotherBrainsBackgroundRow7 = 0xb68f;
    public const ushort MotherBrainsBackgroundRow8 = 0xb693;
    public const ushort MotherBrainsBackgroundRow9 = 0xb697;
    public const ushort MotherBrainsBackgroundRowA = 0xb69b;
    public const ushort MotherBrainsBackgroundRowB = 0xb69f;
    public const ushort MotherBrainsBackgroundRowC = 0xb6a3;
    public const ushort MotherBrainsBackgroundRowD = 0xb6a7;
    public const ushort ClearMotherBrainCeilingBlock = 0xb6b3;
    public const ushort ClearMotherBrainCeilingTube = 0xb6b7;
    public const ushort ClearMotherBrainBottomMiddleSideTube = 0xb6bb;
    public const ushort ClearMotherBrainBottomMiddleTubes = 0xb6bf;
    public const ushort ClearMotherBrainBottomLeftTube = 0xb6c3;
    public const ushort ClearMotherBrainBottomRightTube = 0xb6c7;

    /// <summary>Main map-station room actor at $84:B6D3.</summary>
    public const ushort MapStation = 0xb6d3;
    public const ushort MapStationRightAccess = 0xb6d7;
    public const ushort MapStationLeftAccess = 0xb6db;
    public const ushort EnergyStation = 0xb6df;
    public const ushort EnergyStationRightAccess = 0xb6e3;
    public const ushort EnergyStationLeftAccess = 0xb6e7;
    public const ushort MissileStation = 0xb6eb;
    public const ushort MissileStationRightAccess = 0xb6ef;
    public const ushort MissileStationLeftAccess = 0xb6f3;
    public const ushort ScrollTriggerCollision = 0xb6ff;

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
    /// <summary>Collision-side save-station trigger at $84:B76B.</summary>
    public const ushort SaveStationTrigger = 0xb76b;
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

    /// <summary>Speed Booster escape lavaquake controller at $84:B8AC.</summary>
    public const ushort SpeedBoosterEscape = 0xb8ac;

    /// <summary>Maridia elevatube delay/sound PLM at $84:B8F9.</summary>
    public const ushort MaridiaElevatube = 0xb8f9;

    /// <summary>Resident Wrecked Ship attic no-op observer at $84:BB05.</summary>
    public const ushort WreckedShipAttic = 0xbb05;

    /// <summary>Bomb Torizo's exceptional right-facing grey door at $84:BAF4.</summary>
    public const ushort BombTorizoGreyDoor = 0xbaf4;

    // Door families use a six-byte header stride in left/right/up/down order. Naming each
    // address prevents orientation arithmetic from silently accepting an in-between word.
    public const ushort GreyDoorFacingLeft = 0xc842;
    public const ushort GreyDoorFacingRight = 0xc848;
    public const ushort GreyDoorFacingUp = 0xc84e;
    public const ushort GreyDoorFacingDown = 0xc854;

    public const ushort YellowDoorFacingLeft = 0xc85a;
    public const ushort YellowDoorFacingRight = 0xc860;
    public const ushort YellowDoorFacingUp = 0xc866;
    public const ushort YellowDoorFacingDown = 0xc86c;
    public const ushort GreenDoorFacingLeft = 0xc872;
    public const ushort GreenDoorFacingRight = 0xc878;
    public const ushort GreenDoorFacingUp = 0xc87e;
    public const ushort GreenDoorFacingDown = 0xc884;
    public const ushort RedDoorFacingLeft = 0xc88a;
    public const ushort RedDoorFacingRight = 0xc890;
    public const ushort RedDoorFacingUp = 0xc896;
    public const ushort RedDoorFacingDown = 0xc89c;

    public const ushort BlueDoorFacingLeft = 0xc8a2;
    public const ushort BlueDoorFacingRight = 0xc8a8;
    public const ushort BlueDoorFacingUp = 0xc8ae;
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

    /// <summary>Collision-side permanent-item detector at $84:EED3.</summary>
    public const ushort ItemCollisionDetection = 0xeed3;

    // Exposed permanent-item headers. The order matches InWorldCollectibleKind exactly.
    public const ushort ExposedEnergyTank = 0xeed7;
    public const ushort ExposedMissileTank = 0xeedb;
    public const ushort ExposedSuperMissileTank = 0xeedf;
    public const ushort ExposedPowerBombTank = 0xeee3;
    public const ushort ExposedBombs = 0xeee7;
    public const ushort ExposedChargeBeam = 0xeeeb;
    public const ushort ExposedIceBeam = 0xeeef;
    public const ushort ExposedHiJumpBoots = 0xeef3;
    public const ushort ExposedSpeedBooster = 0xeef7;
    public const ushort ExposedWaveBeam = 0xeefb;
    public const ushort ExposedSpazerBeam = 0xeeff;
    public const ushort ExposedSpringBall = 0xef03;
    public const ushort ExposedVariaSuit = 0xef07;
    public const ushort ExposedGravitySuit = 0xef0b;
    public const ushort ExposedXrayScope = 0xef0f;
    public const ushort ExposedPlasmaBeam = 0xef13;
    public const ushort ExposedGrappleBeam = 0xef17;
    public const ushort ExposedSpaceJump = 0xef1b;
    public const ushort ExposedScrewAttack = 0xef1f;
    public const ushort ExposedMorphBall = 0xef23;
    public const ushort ExposedReserveTank = 0xef27;

    // Chozo-orb permanent-item headers. The order matches InWorldCollectibleKind exactly.
    public const ushort ChozoEnergyTank = 0xef2b;
    public const ushort ChozoMissileTank = 0xef2f;
    public const ushort ChozoSuperMissileTank = 0xef33;
    public const ushort ChozoPowerBombTank = 0xef37;
    public const ushort ChozoBombs = 0xef3b;
    public const ushort ChozoChargeBeam = 0xef3f;
    public const ushort ChozoIceBeam = 0xef43;
    public const ushort ChozoHiJumpBoots = 0xef47;
    public const ushort ChozoSpeedBooster = 0xef4b;
    public const ushort ChozoWaveBeam = 0xef4f;
    public const ushort ChozoSpazerBeam = 0xef53;
    public const ushort ChozoSpringBall = 0xef57;
    public const ushort ChozoVariaSuit = 0xef5b;
    public const ushort ChozoGravitySuit = 0xef5f;
    public const ushort ChozoXrayScope = 0xef63;
    public const ushort ChozoPlasmaBeam = 0xef67;
    public const ushort ChozoGrappleBeam = 0xef6b;
    public const ushort ChozoSpaceJump = 0xef6f;
    public const ushort ChozoScrewAttack = 0xef73;
    public const ushort ChozoMorphBall = 0xef77;
    public const ushort ChozoReserveTank = 0xef7b;

    // Concealed shot-block permanent-item headers. The order matches
    // InWorldCollectibleKind exactly.
    public const ushort ShotBlockEnergyTank = 0xef7f;
    public const ushort ShotBlockMissileTank = 0xef83;
    public const ushort ShotBlockSuperMissileTank = 0xef87;
    public const ushort ShotBlockPowerBombTank = 0xef8b;
    public const ushort ShotBlockBombs = 0xef8f;
    public const ushort ShotBlockChargeBeam = 0xef93;
    public const ushort ShotBlockIceBeam = 0xef97;
    public const ushort ShotBlockHiJumpBoots = 0xef9b;
    public const ushort ShotBlockSpeedBooster = 0xef9f;
    public const ushort ShotBlockWaveBeam = 0xefa3;
    public const ushort ShotBlockSpazerBeam = 0xefa7;
    public const ushort ShotBlockSpringBall = 0xefab;
    public const ushort ShotBlockVariaSuit = 0xefaf;
    public const ushort ShotBlockGravitySuit = 0xefb3;
    public const ushort ShotBlockXrayScope = 0xefb7;
    public const ushort ShotBlockPlasmaBeam = 0xefbb;
    public const ushort ShotBlockGrappleBeam = 0xefbf;
    public const ushort ShotBlockSpaceJump = 0xefc3;
    public const ushort ShotBlockScrewAttack = 0xefc7;
    public const ushort ShotBlockMorphBall = 0xefcb;
    public const ushort ShotBlockReserveTank = 0xefcf;
}
