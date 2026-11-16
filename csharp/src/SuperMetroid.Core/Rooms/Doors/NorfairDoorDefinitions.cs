namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The Norfair rooms' doors, each named for the room it leads from and the room it leads to.</summary>
public static class NorfairDoorDefinitions
{
    /// <summary>The door from the Ice Beam Acid room to the Ice Beam Gate room.</summary>
    public static DoorDefinition IceBeamAcidToIceBeamGate { get; } = DoorDefinitions.Find(nameof(IceBeamAcidToIceBeamGate));

    /// <summary>The door from the Ice Beam Acid room to the Ice Beam Snake room.</summary>
    public static DoorDefinition IceBeamAcidToIceBeamSnake { get; } = DoorDefinitions.Find(nameof(IceBeamAcidToIceBeamSnake));

    /// <summary>The door from the Cathedral room to the Cathedral Entrance room.</summary>
    public static DoorDefinition CathedralToCathedralEntrance { get; } = DoorDefinitions.Find(nameof(CathedralToCathedralEntrance));

    /// <summary>The door from the Cathedral room to the Rising Tide room.</summary>
    public static DoorDefinition CathedralToRisingTide { get; } = DoorDefinitions.Find(nameof(CathedralToRisingTide));

    /// <summary>The door from the Cathedral Entrance room to the Business Center room.</summary>
    public static DoorDefinition CathedralEntranceToBusinessCenter { get; } = DoorDefinitions.Find(nameof(CathedralEntranceToBusinessCenter));

    /// <summary>The door from the Cathedral Entrance room to the Cathedral room.</summary>
    public static DoorDefinition CathedralEntranceToCathedral { get; } = DoorDefinitions.Find(nameof(CathedralEntranceToCathedral));

    /// <summary>The door from the Business Center room to the Ice Beam Gate room.</summary>
    public static DoorDefinition BusinessCenterToIceBeamGate { get; } = DoorDefinitions.Find(nameof(BusinessCenterToIceBeamGate));

    /// <summary>The door from the Business Center room to the Cathedral Entrance room.</summary>
    public static DoorDefinition BusinessCenterToCathedralEntrance { get; } = DoorDefinitions.Find(nameof(BusinessCenterToCathedralEntrance));

    /// <summary>The door from the Business Center room to the Hi Jump Energy Tank room.</summary>
    public static DoorDefinition BusinessCenterToHiJumpEnergyTank { get; } = DoorDefinitions.Find(nameof(BusinessCenterToHiJumpEnergyTank));

    /// <summary>The door from the Business Center room to the Crocomire Escape room.</summary>
    public static DoorDefinition BusinessCenterToCrocomireEscape { get; } = DoorDefinitions.Find(nameof(BusinessCenterToCrocomireEscape));

    /// <summary>The door from the Business Center room to the Warehouse Entrance room.</summary>
    public static DoorDefinition BusinessCenterToWarehouseEntrance { get; } = DoorDefinitions.Find(nameof(BusinessCenterToWarehouseEntrance));

    /// <summary>The door from the Business Center room to the Frog Save room.</summary>
    public static DoorDefinition BusinessCenterToFrogSave { get; } = DoorDefinitions.Find(nameof(BusinessCenterToFrogSave));

    /// <summary>The door from the Business Center room to the Norfair Map room.</summary>
    public static DoorDefinition BusinessCenterToNorfairMap { get; } = DoorDefinitions.Find(nameof(BusinessCenterToNorfairMap));

    /// <summary>The door from the Ice Beam Gate room to the Ice Beam Tutorial room.</summary>
    public static DoorDefinition IceBeamGateToIceBeamTutorial { get; } = DoorDefinitions.Find(nameof(IceBeamGateToIceBeamTutorial));

    /// <summary>The door from the Ice Beam Gate room to the Ice Beam Acid room.</summary>
    public static DoorDefinition IceBeamGateToIceBeamAcid { get; } = DoorDefinitions.Find(nameof(IceBeamGateToIceBeamAcid));

    /// <summary>The door from the Ice Beam Gate room to the Business Center room.</summary>
    public static DoorDefinition IceBeamGateToBusinessCenter { get; } = DoorDefinitions.Find(nameof(IceBeamGateToBusinessCenter));

    /// <summary>The door from the Ice Beam Gate room to the Crumble Shaft room.</summary>
    public static DoorDefinition IceBeamGateToCrumbleShaft { get; } = DoorDefinitions.Find(nameof(IceBeamGateToCrumbleShaft));

    /// <summary>The door from the Ice Beam Tutorial room to the Ice Beam Snake room.</summary>
    public static DoorDefinition IceBeamTutorialToIceBeamSnake { get; } = DoorDefinitions.Find(nameof(IceBeamTutorialToIceBeamSnake));

    /// <summary>The door from the Ice Beam Tutorial room to the Ice Beam Gate room.</summary>
    public static DoorDefinition IceBeamTutorialToIceBeamGate { get; } = DoorDefinitions.Find(nameof(IceBeamTutorialToIceBeamGate));

    /// <summary>The door from the Ice Beam room to the Ice Beam Snake room.</summary>
    public static DoorDefinition IceBeamToIceBeamSnake { get; } = DoorDefinitions.Find(nameof(IceBeamToIceBeamSnake));

    /// <summary>The door from the Ice Beam Snake room to the Ice Beam Acid room.</summary>
    public static DoorDefinition IceBeamSnakeToIceBeamAcid { get; } = DoorDefinitions.Find(nameof(IceBeamSnakeToIceBeamAcid));

    /// <summary>The door from the Ice Beam Snake room to the Ice Beam Tutorial room.</summary>
    public static DoorDefinition IceBeamSnakeToIceBeamTutorial { get; } = DoorDefinitions.Find(nameof(IceBeamSnakeToIceBeamTutorial));

    /// <summary>The door from the Ice Beam Snake room to the Ice Beam room.</summary>
    public static DoorDefinition IceBeamSnakeToIceBeam { get; } = DoorDefinitions.Find(nameof(IceBeamSnakeToIceBeam));

    /// <summary>The door from the Crumble Shaft room to the Ice Beam Gate room.</summary>
    public static DoorDefinition CrumbleShaftToIceBeamGate { get; } = DoorDefinitions.Find(nameof(CrumbleShaftToIceBeamGate));

    /// <summary>The door from the Crumble Shaft room to the Crocomire Speedway room.</summary>
    public static DoorDefinition CrumbleShaftToCrocomireSpeedway { get; } = DoorDefinitions.Find(nameof(CrumbleShaftToCrocomireSpeedway));

    /// <summary>The door from the Crocomire Speedway room to the Crumble Shaft room.</summary>
    public static DoorDefinition CrocomireSpeedwayToCrumbleShaft { get; } = DoorDefinitions.Find(nameof(CrocomireSpeedwayToCrumbleShaft));

    /// <summary>The door from the Crocomire Speedway room to the Crocomire Escape room.</summary>
    public static DoorDefinition CrocomireSpeedwayToCrocomireEscape { get; } = DoorDefinitions.Find(nameof(CrocomireSpeedwayToCrocomireEscape));

    /// <summary>The door from the Crocomire Speedway room to the Crocomire Save room.</summary>
    public static DoorDefinition CrocomireSpeedwayToCrocomireSave { get; } = DoorDefinitions.Find(nameof(CrocomireSpeedwayToCrocomireSave));

    /// <summary>The door from the Crocomire Speedway room to the Acid Snakes Tunnel room.</summary>
    public static DoorDefinition CrocomireSpeedwayToAcidSnakesTunnel { get; } = DoorDefinitions.Find(nameof(CrocomireSpeedwayToAcidSnakesTunnel));

    /// <summary>The door from the Crocomire Speedway room to the Crocomire room.</summary>
    public static DoorDefinition CrocomireSpeedwayToCrocomire { get; } = DoorDefinitions.Find(nameof(CrocomireSpeedwayToCrocomire));

    /// <summary>The door from the Crocomire room to the Post Crocomire Farming room.</summary>
    public static DoorDefinition CrocomireToPostCrocomireFarming { get; } = DoorDefinitions.Find(nameof(CrocomireToPostCrocomireFarming));

    /// <summary>The door from the Crocomire room to the Crocomire Speedway room.</summary>
    public static DoorDefinition CrocomireToCrocomireSpeedway { get; } = DoorDefinitions.Find(nameof(CrocomireToCrocomireSpeedway));

    /// <summary>The door from the Hi Jump Boots room to the Hi Jump Energy Tank room.</summary>
    public static DoorDefinition HiJumpBootsToHiJumpEnergyTank { get; } = DoorDefinitions.Find(nameof(HiJumpBootsToHiJumpEnergyTank));

    /// <summary>The door from the Crocomire Escape room to the Business Center room.</summary>
    public static DoorDefinition CrocomireEscapeToBusinessCenter { get; } = DoorDefinitions.Find(nameof(CrocomireEscapeToBusinessCenter));

    /// <summary>The door from the Crocomire Escape room to the Crocomire Speedway room.</summary>
    public static DoorDefinition CrocomireEscapeToCrocomireSpeedway { get; } = DoorDefinitions.Find(nameof(CrocomireEscapeToCrocomireSpeedway));

    /// <summary>The door from the Hi Jump Energy Tank room to the Business Center room.</summary>
    public static DoorDefinition HiJumpEnergyTankToBusinessCenter { get; } = DoorDefinitions.Find(nameof(HiJumpEnergyTankToBusinessCenter));

    /// <summary>The door from the Hi Jump Energy Tank room to the Hi Jump Boots room.</summary>
    public static DoorDefinition HiJumpEnergyTankToHiJumpBoots { get; } = DoorDefinitions.Find(nameof(HiJumpEnergyTankToHiJumpBoots));

    /// <summary>The door from the Post Crocomire Farming room to the Crocomire room.</summary>
    public static DoorDefinition PostCrocomireFarmingToCrocomire { get; } = DoorDefinitions.Find(nameof(PostCrocomireFarmingToCrocomire));

    /// <summary>The door from the Post Crocomire Farming room to the Post Crocomire Power Bombs room.</summary>
    public static DoorDefinition PostCrocomireFarmingToPostCrocomirePowerBombs { get; } = DoorDefinitions.Find(nameof(PostCrocomireFarmingToPostCrocomirePowerBombs));

    /// <summary>The door from the Post Crocomire Farming room to the Post Crocomire Shaft room.</summary>
    public static DoorDefinition PostCrocomireFarmingToPostCrocomireShaft { get; } = DoorDefinitions.Find(nameof(PostCrocomireFarmingToPostCrocomireShaft));

    /// <summary>The door from the Post Crocomire Farming room to the Post Crocomire Save room.</summary>
    public static DoorDefinition PostCrocomireFarmingToPostCrocomireSave { get; } = DoorDefinitions.Find(nameof(PostCrocomireFarmingToPostCrocomireSave));

    /// <summary>The door from the Post Crocomire Save room to the Post Crocomire Farming room.</summary>
    public static DoorDefinition PostCrocomireSaveToPostCrocomireFarming { get; } = DoorDefinitions.Find(nameof(PostCrocomireSaveToPostCrocomireFarming));

    /// <summary>The door from the Post Crocomire Power Bombs room to the Post Crocomire Farming room.</summary>
    public static DoorDefinition PostCrocomirePowerBombsToPostCrocomireFarming { get; } = DoorDefinitions.Find(nameof(PostCrocomirePowerBombsToPostCrocomireFarming));

    /// <summary>The door from the Post Crocomire Shaft room to the Post Crocomire Farming room.</summary>
    public static DoorDefinition PostCrocomireShaftToPostCrocomireFarming { get; } = DoorDefinitions.Find(nameof(PostCrocomireShaftToPostCrocomireFarming));

    /// <summary>The door from the Post Crocomire Shaft room to the Grapple Tutorial 3 room.</summary>
    public static DoorDefinition PostCrocomireShaftToGrappleTutorial3 { get; } = DoorDefinitions.Find(nameof(PostCrocomireShaftToGrappleTutorial3));

    /// <summary>The door from the Post Crocomire Shaft room to the Post Crocomire Missile room.</summary>
    public static DoorDefinition PostCrocomireShaftToPostCrocomireMissile { get; } = DoorDefinitions.Find(nameof(PostCrocomireShaftToPostCrocomireMissile));

    /// <summary>The door from the Post Crocomire Shaft room to the Post Crocomire Jump room.</summary>
    public static DoorDefinition PostCrocomireShaftToPostCrocomireJump { get; } = DoorDefinitions.Find(nameof(PostCrocomireShaftToPostCrocomireJump));

    /// <summary>The door from the Post Crocomire Missile room to the Post Crocomire Shaft room.</summary>
    public static DoorDefinition PostCrocomireMissileToPostCrocomireShaft { get; } = DoorDefinitions.Find(nameof(PostCrocomireMissileToPostCrocomireShaft));

    /// <summary>The door from the Grapple Tutorial 3 room to the Grapple Tutorial 2 room.</summary>
    public static DoorDefinition GrappleTutorial3ToGrappleTutorial2 { get; } = DoorDefinitions.Find(nameof(GrappleTutorial3ToGrappleTutorial2));

    /// <summary>The door from the Grapple Tutorial 3 room to the Post Crocomire Shaft room.</summary>
    public static DoorDefinition GrappleTutorial3ToPostCrocomireShaft { get; } = DoorDefinitions.Find(nameof(GrappleTutorial3ToPostCrocomireShaft));

    /// <summary>The door from the Post Crocomire Jump room to the Post Crocomire Shaft room.</summary>
    public static DoorDefinition PostCrocomireJumpToPostCrocomireShaft { get; } = DoorDefinitions.Find(nameof(PostCrocomireJumpToPostCrocomireShaft));

    /// <summary>The door from the Post Crocomire Jump room to the Grapple Beam room.</summary>
    public static DoorDefinition PostCrocomireJumpToGrappleBeam { get; } = DoorDefinitions.Find(nameof(PostCrocomireJumpToGrappleBeam));

    /// <summary>The door from the Grapple Tutorial 2 room to the Grapple Tutorial 1 room.</summary>
    public static DoorDefinition GrappleTutorial2ToGrappleTutorial1 { get; } = DoorDefinitions.Find(nameof(GrappleTutorial2ToGrappleTutorial1));

    /// <summary>The door from the Grapple Tutorial 2 room to the Grapple Tutorial 3 room.</summary>
    public static DoorDefinition GrappleTutorial2ToGrappleTutorial3 { get; } = DoorDefinitions.Find(nameof(GrappleTutorial2ToGrappleTutorial3));

    /// <summary>The door from the Grapple Tutorial 1 room to the Grapple Beam room.</summary>
    public static DoorDefinition GrappleTutorial1ToGrappleBeam { get; } = DoorDefinitions.Find(nameof(GrappleTutorial1ToGrappleBeam));

    /// <summary>The door from the Grapple Tutorial 1 room to the Grapple Tutorial 2 room.</summary>
    public static DoorDefinition GrappleTutorial1ToGrappleTutorial2 { get; } = DoorDefinitions.Find(nameof(GrappleTutorial1ToGrappleTutorial2));

    /// <summary>The door from the Grapple Beam room to the Post Crocomire Jump room.</summary>
    public static DoorDefinition GrappleBeamToPostCrocomireJump { get; } = DoorDefinitions.Find(nameof(GrappleBeamToPostCrocomireJump));

    /// <summary>The door from the Grapple Beam room to the Grapple Tutorial 1 room.</summary>
    public static DoorDefinition GrappleBeamToGrappleTutorial1 { get; } = DoorDefinitions.Find(nameof(GrappleBeamToGrappleTutorial1));

    /// <summary>The Norfair Reserve Tank room's door.</summary>
    public static DoorDefinition NorfairReserveTank { get; } = DoorDefinitions.Find(nameof(NorfairReserveTank));

    /// <summary>The door from the Green Bubbles Missiles room to the Bubble Mountain room.</summary>
    public static DoorDefinition GreenBubblesMissilesToBubbleMountain { get; } = DoorDefinitions.Find(nameof(GreenBubblesMissilesToBubbleMountain));

    /// <summary>The door from the Green Bubbles Missiles room to the Norfair Reserve Tank room.</summary>
    public static DoorDefinition GreenBubblesMissilesToNorfairReserveTank { get; } = DoorDefinitions.Find(nameof(GreenBubblesMissilesToNorfairReserveTank));

    /// <summary>The door from the Bubble Mountain room to the Green Bubbles Missiles room.</summary>
    public static DoorDefinition BubbleMountainToGreenBubblesMissiles { get; } = DoorDefinitions.Find(nameof(BubbleMountainToGreenBubblesMissiles));

    /// <summary>The door from the Bubble Mountain room to the Rising Tide room.</summary>
    public static DoorDefinition BubbleMountainToRisingTide { get; } = DoorDefinitions.Find(nameof(BubbleMountainToRisingTide));

    /// <summary>The door from the Bubble Mountain room to the Upper Norfair Farming room.</summary>
    public static DoorDefinition BubbleMountainToUpperNorfairFarming { get; } = DoorDefinitions.Find(nameof(BubbleMountainToUpperNorfairFarming));

    /// <summary>The door from the Bubble Mountain room to the Purple Shaft room.</summary>
    public static DoorDefinition BubbleMountainToPurpleShaft { get; } = DoorDefinitions.Find(nameof(BubbleMountainToPurpleShaft));

    /// <summary>The door from the Bubble Mountain room to the Single Chamber room.</summary>
    public static DoorDefinition BubbleMountainToSingleChamber { get; } = DoorDefinitions.Find(nameof(BubbleMountainToSingleChamber));

    /// <summary>The door from the Bubble Mountain room to the Bat Cave room.</summary>
    public static DoorDefinition BubbleMountainToBatCave { get; } = DoorDefinitions.Find(nameof(BubbleMountainToBatCave));

    /// <summary>The door from the Bubble Mountain room to the Bubble Mountain Save room.</summary>
    public static DoorDefinition BubbleMountainToBubbleMountainSave { get; } = DoorDefinitions.Find(nameof(BubbleMountainToBubbleMountainSave));

    /// <summary>The door from the Speed Booster Hall room to the Bat Cave room.</summary>
    public static DoorDefinition SpeedBoosterHallToBatCave { get; } = DoorDefinitions.Find(nameof(SpeedBoosterHallToBatCave));

    /// <summary>The door from the Speed Booster Hall room to the Speed Booster room.</summary>
    public static DoorDefinition SpeedBoosterHallToSpeedBooster { get; } = DoorDefinitions.Find(nameof(SpeedBoosterHallToSpeedBooster));

    /// <summary>The door from the Speed Booster room to the Speed Booster Hall room.</summary>
    public static DoorDefinition SpeedBoosterToSpeedBoosterHall { get; } = DoorDefinitions.Find(nameof(SpeedBoosterToSpeedBoosterHall));

    /// <summary>The door from the Single Chamber room to the Bubble Mountain room.</summary>
    public static DoorDefinition SingleChamberToBubbleMountain { get; } = DoorDefinitions.Find(nameof(SingleChamberToBubbleMountain));

    /// <summary>The door from the Single Chamber room to the Double Chamber room, arriving at its upper doorway.</summary>
    public static DoorDefinition SingleChamberToDoubleChamberUpper { get; } = DoorDefinitions.Find(nameof(SingleChamberToDoubleChamberUpper));

    /// <summary>The door from the Single Chamber room to the Double Chamber room, arriving at its lower doorway.</summary>
    public static DoorDefinition SingleChamberToDoubleChamberLower { get; } = DoorDefinitions.Find(nameof(SingleChamberToDoubleChamberLower));

    /// <summary>The door from the Single Chamber room to the Spiky Platforms Tunnel room.</summary>
    public static DoorDefinition SingleChamberToSpikyPlatformsTunnel { get; } = DoorDefinitions.Find(nameof(SingleChamberToSpikyPlatformsTunnel));

    /// <summary>The door from the Single Chamber room to the Three Musketeers room.</summary>
    public static DoorDefinition SingleChamberToThreeMusketeers { get; } = DoorDefinitions.Find(nameof(SingleChamberToThreeMusketeers));

    /// <summary>The door from the Double Chamber room to the Single Chamber room, arriving at its upper doorway.</summary>
    public static DoorDefinition DoubleChamberToSingleChamberUpper { get; } = DoorDefinitions.Find(nameof(DoubleChamberToSingleChamberUpper));

    /// <summary>The door from the Double Chamber room to the Single Chamber room, arriving at its lower doorway.</summary>
    public static DoorDefinition DoubleChamberToSingleChamberLower { get; } = DoorDefinitions.Find(nameof(DoubleChamberToSingleChamberLower));

    /// <summary>The door from the Double Chamber room to the Wave Beam room.</summary>
    public static DoorDefinition DoubleChamberToWaveBeam { get; } = DoorDefinitions.Find(nameof(DoubleChamberToWaveBeam));

    /// <summary>The door from the Wave Beam room to the Double Chamber room.</summary>
    public static DoorDefinition WaveBeamToDoubleChamber { get; } = DoorDefinitions.Find(nameof(WaveBeamToDoubleChamber));

    /// <summary>The door from the Spiky Platforms Tunnel room to the Single Chamber room.</summary>
    public static DoorDefinition SpikyPlatformsTunnelToSingleChamber { get; } = DoorDefinitions.Find(nameof(SpikyPlatformsTunnelToSingleChamber));

    /// <summary>The door from the Spiky Platforms Tunnel room to the Volcano room.</summary>
    public static DoorDefinition SpikyPlatformsTunnelToVolcano { get; } = DoorDefinitions.Find(nameof(SpikyPlatformsTunnelToVolcano));

    /// <summary>The door from the Volcano room to the Spiky Platforms Tunnel room.</summary>
    public static DoorDefinition VolcanoToSpikyPlatformsTunnel { get; } = DoorDefinitions.Find(nameof(VolcanoToSpikyPlatformsTunnel));

    /// <summary>The door from the Volcano room to the Kronic Boost room.</summary>
    public static DoorDefinition VolcanoToKronicBoost { get; } = DoorDefinitions.Find(nameof(VolcanoToKronicBoost));

    /// <summary>The door from the Kronic Boost room to the Magdollite Tunnel room.</summary>
    public static DoorDefinition KronicBoostToMagdolliteTunnel { get; } = DoorDefinitions.Find(nameof(KronicBoostToMagdolliteTunnel));

    /// <summary>The door from the Kronic Boost room to the Volcano room.</summary>
    public static DoorDefinition KronicBoostToVolcano { get; } = DoorDefinitions.Find(nameof(KronicBoostToVolcano));

    /// <summary>The door from the Kronic Boost room to the Lava Dive room.</summary>
    public static DoorDefinition KronicBoostToLavaDive { get; } = DoorDefinitions.Find(nameof(KronicBoostToLavaDive));

    /// <summary>The door from the Kronic Boost room to the Spiky Acid Snakes Tunnel room.</summary>
    public static DoorDefinition KronicBoostToSpikyAcidSnakesTunnel { get; } = DoorDefinitions.Find(nameof(KronicBoostToSpikyAcidSnakesTunnel));

    /// <summary>The door from the Magdollite Tunnel room to the Purple Shaft room.</summary>
    public static DoorDefinition MagdolliteTunnelToPurpleShaft { get; } = DoorDefinitions.Find(nameof(MagdolliteTunnelToPurpleShaft));

    /// <summary>The door from the Magdollite Tunnel room to the Kronic Boost room.</summary>
    public static DoorDefinition MagdolliteTunnelToKronicBoost { get; } = DoorDefinitions.Find(nameof(MagdolliteTunnelToKronicBoost));

    /// <summary>The door from the Purple Shaft room to the Bubble Mountain room.</summary>
    public static DoorDefinition PurpleShaftToBubbleMountain { get; } = DoorDefinitions.Find(nameof(PurpleShaftToBubbleMountain));

    /// <summary>The door from the Purple Shaft room to the Magdollite Tunnel room.</summary>
    public static DoorDefinition PurpleShaftToMagdolliteTunnel { get; } = DoorDefinitions.Find(nameof(PurpleShaftToMagdolliteTunnel));

    /// <summary>The door from the Purple Shaft room to the Purple Farming room.</summary>
    public static DoorDefinition PurpleShaftToPurpleFarming { get; } = DoorDefinitions.Find(nameof(PurpleShaftToPurpleFarming));

    /// <summary>The door from the Lava Dive room to the Kronic Boost room.</summary>
    public static DoorDefinition LavaDiveToKronicBoost { get; } = DoorDefinitions.Find(nameof(LavaDiveToKronicBoost));

    /// <summary>The door from the Lava Dive room to the Lower Norfair Elevator room.</summary>
    public static DoorDefinition LavaDiveToLowerNorfairElevator { get; } = DoorDefinitions.Find(nameof(LavaDiveToLowerNorfairElevator));

    /// <summary>The door from the Lower Norfair Elevator room to the Lava Dive room.</summary>
    public static DoorDefinition LowerNorfairElevatorToLavaDive { get; } = DoorDefinitions.Find(nameof(LowerNorfairElevatorToLavaDive));

    /// <summary>The door from the Lower Norfair Elevator room to the Main Hall room.</summary>
    public static DoorDefinition LowerNorfairElevatorToMainHall { get; } = DoorDefinitions.Find(nameof(LowerNorfairElevatorToMainHall));

    /// <summary>The door from the Lower Norfair Elevator room to the Lower Norfair Elevator Save room.</summary>
    public static DoorDefinition LowerNorfairElevatorToLowerNorfairElevatorSave { get; } = DoorDefinitions.Find(nameof(LowerNorfairElevatorToLowerNorfairElevatorSave));

    /// <summary>The door from the Upper Norfair Farming room to the Frog Speedway room.</summary>
    public static DoorDefinition UpperNorfairFarmingToFrogSpeedway { get; } = DoorDefinitions.Find(nameof(UpperNorfairFarmingToFrogSpeedway));

    /// <summary>The door from the Upper Norfair Farming room to the Bubble Mountain room.</summary>
    public static DoorDefinition UpperNorfairFarmingToBubbleMountain { get; } = DoorDefinitions.Find(nameof(UpperNorfairFarmingToBubbleMountain));

    /// <summary>The door from the Upper Norfair Farming room to the Red Pirate Shaft room.</summary>
    public static DoorDefinition UpperNorfairFarmingToRedPirateShaft { get; } = DoorDefinitions.Find(nameof(UpperNorfairFarmingToRedPirateShaft));

    /// <summary>The door from the Rising Tide room to the Cathedral room.</summary>
    public static DoorDefinition RisingTideToCathedral { get; } = DoorDefinitions.Find(nameof(RisingTideToCathedral));

    /// <summary>The door from the Rising Tide room to the Bubble Mountain room.</summary>
    public static DoorDefinition RisingTideToBubbleMountain { get; } = DoorDefinitions.Find(nameof(RisingTideToBubbleMountain));

    /// <summary>The door from the Acid Snakes Tunnel room to the Crocomire Speedway room.</summary>
    public static DoorDefinition AcidSnakesTunnelToCrocomireSpeedway { get; } = DoorDefinitions.Find(nameof(AcidSnakesTunnelToCrocomireSpeedway));

    /// <summary>The door from the Acid Snakes Tunnel room to the Upper Norfair Refill room.</summary>
    public static DoorDefinition AcidSnakesTunnelToUpperNorfairRefill { get; } = DoorDefinitions.Find(nameof(AcidSnakesTunnelToUpperNorfairRefill));

    /// <summary>The door from the Acid Snakes Tunnel room to the Red Pirate Shaft room.</summary>
    public static DoorDefinition AcidSnakesTunnelToRedPirateShaft { get; } = DoorDefinitions.Find(nameof(AcidSnakesTunnelToRedPirateShaft));

    /// <summary>The door from the Spiky Acid Snakes Tunnel room to the Upper Norfair Refill room.</summary>
    public static DoorDefinition SpikyAcidSnakesTunnelToUpperNorfairRefill { get; } = DoorDefinitions.Find(nameof(SpikyAcidSnakesTunnelToUpperNorfairRefill));

    /// <summary>The door from the Spiky Acid Snakes Tunnel room to the Kronic Boost room.</summary>
    public static DoorDefinition SpikyAcidSnakesTunnelToKronicBoost { get; } = DoorDefinitions.Find(nameof(SpikyAcidSnakesTunnelToKronicBoost));

    /// <summary>The door from the Upper Norfair Refill room to the Acid Snakes Tunnel room.</summary>
    public static DoorDefinition UpperNorfairRefillToAcidSnakesTunnel { get; } = DoorDefinitions.Find(nameof(UpperNorfairRefillToAcidSnakesTunnel));

    /// <summary>The door from the Upper Norfair Refill room to the Spiky Acid Snakes Tunnel room.</summary>
    public static DoorDefinition UpperNorfairRefillToSpikyAcidSnakesTunnel { get; } = DoorDefinitions.Find(nameof(UpperNorfairRefillToSpikyAcidSnakesTunnel));

    /// <summary>The door from the Purple Farming room to the Purple Shaft room.</summary>
    public static DoorDefinition PurpleFarmingToPurpleShaft { get; } = DoorDefinitions.Find(nameof(PurpleFarmingToPurpleShaft));

    /// <summary>The door from the Bat Cave room to the Bubble Mountain room.</summary>
    public static DoorDefinition BatCaveToBubbleMountain { get; } = DoorDefinitions.Find(nameof(BatCaveToBubbleMountain));

    /// <summary>The door from the Bat Cave room to the Speed Booster Hall room.</summary>
    public static DoorDefinition BatCaveToSpeedBoosterHall { get; } = DoorDefinitions.Find(nameof(BatCaveToSpeedBoosterHall));

    /// <summary>The door from the Norfair Map room to the Business Center room.</summary>
    public static DoorDefinition NorfairMapToBusinessCenter { get; } = DoorDefinitions.Find(nameof(NorfairMapToBusinessCenter));

    /// <summary>The door from the Bubble Mountain Save room to the Bubble Mountain room.</summary>
    public static DoorDefinition BubbleMountainSaveToBubbleMountain { get; } = DoorDefinitions.Find(nameof(BubbleMountainSaveToBubbleMountain));

    /// <summary>The door from the Frog Speedway room to the Frog Save room.</summary>
    public static DoorDefinition FrogSpeedwayToFrogSave { get; } = DoorDefinitions.Find(nameof(FrogSpeedwayToFrogSave));

    /// <summary>The door from the Frog Speedway room to the Upper Norfair Farming room.</summary>
    public static DoorDefinition FrogSpeedwayToUpperNorfairFarming { get; } = DoorDefinitions.Find(nameof(FrogSpeedwayToUpperNorfairFarming));

    /// <summary>The door from the Red Pirate Shaft room to the Upper Norfair Farming room.</summary>
    public static DoorDefinition RedPirateShaftToUpperNorfairFarming { get; } = DoorDefinitions.Find(nameof(RedPirateShaftToUpperNorfairFarming));

    /// <summary>The door from the Red Pirate Shaft room to the Acid Snakes Tunnel room.</summary>
    public static DoorDefinition RedPirateShaftToAcidSnakesTunnel { get; } = DoorDefinitions.Find(nameof(RedPirateShaftToAcidSnakesTunnel));

    /// <summary>The door from the Frog Save room to the Frog Speedway room.</summary>
    public static DoorDefinition FrogSaveToFrogSpeedway { get; } = DoorDefinitions.Find(nameof(FrogSaveToFrogSpeedway));

    /// <summary>The door from the Frog Save room to the Business Center room.</summary>
    public static DoorDefinition FrogSaveToBusinessCenter { get; } = DoorDefinitions.Find(nameof(FrogSaveToBusinessCenter));

    /// <summary>The door from the Crocomire Save room to the Crocomire Speedway room.</summary>
    public static DoorDefinition CrocomireSaveToCrocomireSpeedway { get; } = DoorDefinitions.Find(nameof(CrocomireSaveToCrocomireSpeedway));

    /// <summary>The door from the Lower Norfair Elevator Save room to the Lower Norfair Elevator room.</summary>
    public static DoorDefinition LowerNorfairElevatorSaveToLowerNorfairElevator { get; } = DoorDefinitions.Find(nameof(LowerNorfairElevatorSaveToLowerNorfairElevator));

    /// <summary>The door from the Acid Statue room to the Golden Torizo room.</summary>
    public static DoorDefinition AcidStatueToGoldenTorizo { get; } = DoorDefinitions.Find(nameof(AcidStatueToGoldenTorizo));

    /// <summary>The door from the Acid Statue room to the Main Hall room.</summary>
    public static DoorDefinition AcidStatueToMainHall { get; } = DoorDefinitions.Find(nameof(AcidStatueToMainHall));

    /// <summary>The door from the Main Hall room to the Acid Statue room.</summary>
    public static DoorDefinition MainHallToAcidStatue { get; } = DoorDefinitions.Find(nameof(MainHallToAcidStatue));

    /// <summary>The door from the Main Hall room to the Fast Pillars Setup room.</summary>
    public static DoorDefinition MainHallToFastPillarsSetup { get; } = DoorDefinitions.Find(nameof(MainHallToFastPillarsSetup));

    /// <summary>The door from the Main Hall room to the Lower Norfair Elevator room.</summary>
    public static DoorDefinition MainHallToLowerNorfairElevator { get; } = DoorDefinitions.Find(nameof(MainHallToLowerNorfairElevator));

    /// <summary>The door from the Golden Torizo room to the Acid Statue room.</summary>
    public static DoorDefinition GoldenTorizoToAcidStatue { get; } = DoorDefinitions.Find(nameof(GoldenTorizoToAcidStatue));

    /// <summary>The door from the Golden Torizo room to the Screw Attack room.</summary>
    public static DoorDefinition GoldenTorizoToScrewAttack { get; } = DoorDefinitions.Find(nameof(GoldenTorizoToScrewAttack));

    /// <summary>The door from the Fast Ripper room to the Screw Attack room.</summary>
    public static DoorDefinition FastRipperToScrewAttack { get; } = DoorDefinitions.Find(nameof(FastRipperToScrewAttack));

    /// <summary>The door from the Fast Ripper room to the Fast Pillars Setup room.</summary>
    public static DoorDefinition FastRipperToFastPillarsSetup { get; } = DoorDefinitions.Find(nameof(FastRipperToFastPillarsSetup));

    /// <summary>The door from the Golden Torizo Energy Refill room to the Screw Attack room.</summary>
    public static DoorDefinition GoldenTorizoEnergyRefillToScrewAttack { get; } = DoorDefinitions.Find(nameof(GoldenTorizoEnergyRefillToScrewAttack));

    /// <summary>The door from the Ridley room to the Ridley Energy Tank room.</summary>
    public static DoorDefinition RidleyToRidleyEnergyTank { get; } = DoorDefinitions.Find(nameof(RidleyToRidleyEnergyTank));

    /// <summary>The door from the Ridley room to the Lower Norfair Farming room.</summary>
    public static DoorDefinition RidleyToLowerNorfairFarming { get; } = DoorDefinitions.Find(nameof(RidleyToLowerNorfairFarming));

    /// <summary>The door from the Lower Norfair Farming room to the Ridley room.</summary>
    public static DoorDefinition LowerNorfairFarmingToRidley { get; } = DoorDefinitions.Find(nameof(LowerNorfairFarmingToRidley));

    /// <summary>The door from the Lower Norfair Farming room to the Plowerhouse room.</summary>
    public static DoorDefinition LowerNorfairFarmingToPlowerhouse { get; } = DoorDefinitions.Find(nameof(LowerNorfairFarmingToPlowerhouse));

    /// <summary>The door from the Fast Pillars Setup room to the Main Hall room.</summary>
    public static DoorDefinition FastPillarsSetupToMainHall { get; } = DoorDefinitions.Find(nameof(FastPillarsSetupToMainHall));

    /// <summary>The door from the Fast Pillars Setup room to the Mickey Mouse room.</summary>
    public static DoorDefinition FastPillarsSetupToMickeyMouse { get; } = DoorDefinitions.Find(nameof(FastPillarsSetupToMickeyMouse));

    /// <summary>The door from the Fast Pillars Setup room to the Main Hall room: a second door the room lists to the Main Hall, leading where the first does.</summary>
    public static DoorDefinition FastPillarsSetupToMainHallAlternate { get; } = DoorDefinitions.Find(nameof(FastPillarsSetupToMainHallAlternate));

    /// <summary>The door from the Fast Pillars Setup room to the Fast Ripper room.</summary>
    public static DoorDefinition FastPillarsSetupToFastRipper { get; } = DoorDefinitions.Find(nameof(FastPillarsSetupToFastRipper));

    /// <summary>The door from the Fast Pillars Setup room to the Pillar room.</summary>
    public static DoorDefinition FastPillarsSetupToPillar { get; } = DoorDefinitions.Find(nameof(FastPillarsSetupToPillar));

    /// <summary>The door from the Mickey Mouse room to the Fast Pillars Setup room.</summary>
    public static DoorDefinition MickeyMouseToFastPillarsSetup { get; } = DoorDefinitions.Find(nameof(MickeyMouseToFastPillarsSetup));

    /// <summary>The door from the Mickey Mouse room to the Worst Room In The Game room.</summary>
    public static DoorDefinition MickeyMouseToWorstRoomInTheGame { get; } = DoorDefinitions.Find(nameof(MickeyMouseToWorstRoomInTheGame));

    /// <summary>The door from the Pillar room to the Fast Pillars Setup room.</summary>
    public static DoorDefinition PillarToFastPillarsSetup { get; } = DoorDefinitions.Find(nameof(PillarToFastPillarsSetup));

    /// <summary>The door from the Pillar room to the Worst Room In The Game room.</summary>
    public static DoorDefinition PillarToWorstRoomInTheGame { get; } = DoorDefinitions.Find(nameof(PillarToWorstRoomInTheGame));

    /// <summary>The door from the Plowerhouse room to the Lower Norfair Farming room.</summary>
    public static DoorDefinition PlowerhouseToLowerNorfairFarming { get; } = DoorDefinitions.Find(nameof(PlowerhouseToLowerNorfairFarming));

    /// <summary>The door from the Plowerhouse room to the Metal Pirates room.</summary>
    public static DoorDefinition PlowerhouseToMetalPirates { get; } = DoorDefinitions.Find(nameof(PlowerhouseToMetalPirates));

    /// <summary>The door from the Worst Room In The Game room to the Mickey Mouse room.</summary>
    public static DoorDefinition WorstRoomInTheGameToMickeyMouse { get; } = DoorDefinitions.Find(nameof(WorstRoomInTheGameToMickeyMouse));

    /// <summary>The door from the Worst Room In The Game room to the Amphitheatre room.</summary>
    public static DoorDefinition WorstRoomInTheGameToAmphitheatre { get; } = DoorDefinitions.Find(nameof(WorstRoomInTheGameToAmphitheatre));

    /// <summary>The door from the Worst Room In The Game room to the Pillar room.</summary>
    public static DoorDefinition WorstRoomInTheGameToPillar { get; } = DoorDefinitions.Find(nameof(WorstRoomInTheGameToPillar));

    /// <summary>The door from the Amphitheatre room to the Worst Room In The Game room.</summary>
    public static DoorDefinition AmphitheatreToWorstRoomInTheGame { get; } = DoorDefinitions.Find(nameof(AmphitheatreToWorstRoomInTheGame));

    /// <summary>The door from the Amphitheatre room to the Red Kihunter Shaft room.</summary>
    public static DoorDefinition AmphitheatreToRedKihunterShaft { get; } = DoorDefinitions.Find(nameof(AmphitheatreToRedKihunterShaft));

    /// <summary>The door from the Lower Norfair Spring Ball Maze room to the Three Musketeers room.</summary>
    public static DoorDefinition LowerNorfairSpringBallMazeToThreeMusketeers { get; } = DoorDefinitions.Find(nameof(LowerNorfairSpringBallMazeToThreeMusketeers));

    /// <summary>The door from the Lower Norfair Spring Ball Maze room to the Lower Norfair Fireflea room.</summary>
    public static DoorDefinition LowerNorfairSpringBallMazeToLowerNorfairFireflea { get; } = DoorDefinitions.Find(nameof(LowerNorfairSpringBallMazeToLowerNorfairFireflea));

    /// <summary>The door from the Lower Norfair Spring Ball Maze room to the Lower Norfair Escape Power Bombs room.</summary>
    public static DoorDefinition LowerNorfairSpringBallMazeToLowerNorfairEscapePowerBombs { get; } = DoorDefinitions.Find(nameof(LowerNorfairSpringBallMazeToLowerNorfairEscapePowerBombs));

    /// <summary>The door from the Lower Norfair Escape Power Bombs room to the Lower Norfair Fireflea room.</summary>
    public static DoorDefinition LowerNorfairEscapePowerBombsToLowerNorfairFireflea { get; } = DoorDefinitions.Find(nameof(LowerNorfairEscapePowerBombsToLowerNorfairFireflea));

    /// <summary>The door from the Lower Norfair Escape Power Bombs room to the Lower Norfair Spring Ball Maze room.</summary>
    public static DoorDefinition LowerNorfairEscapePowerBombsToLowerNorfairSpringBallMaze { get; } = DoorDefinitions.Find(nameof(LowerNorfairEscapePowerBombsToLowerNorfairSpringBallMaze));

    /// <summary>The door from the Red Kihunter Shaft room to the Wasteland room.</summary>
    public static DoorDefinition RedKihunterShaftToWasteland { get; } = DoorDefinitions.Find(nameof(RedKihunterShaftToWasteland));

    /// <summary>The door from the Red Kihunter Shaft room to the Amphitheatre room.</summary>
    public static DoorDefinition RedKihunterShaftToAmphitheatre { get; } = DoorDefinitions.Find(nameof(RedKihunterShaftToAmphitheatre));

    /// <summary>The door from the Red Kihunter Shaft room to the Lower Norfair Fireflea room.</summary>
    public static DoorDefinition RedKihunterShaftToLowerNorfairFireflea { get; } = DoorDefinitions.Find(nameof(RedKihunterShaftToLowerNorfairFireflea));

    /// <summary>The door from the Red Kihunter Shaft room to the Lower Norfair Save room.</summary>
    public static DoorDefinition RedKihunterShaftToLowerNorfairSave { get; } = DoorDefinitions.Find(nameof(RedKihunterShaftToLowerNorfairSave));

    /// <summary>The door from the Wasteland room to the Metal Pirates room.</summary>
    public static DoorDefinition WastelandToMetalPirates { get; } = DoorDefinitions.Find(nameof(WastelandToMetalPirates));

    /// <summary>The door from the Wasteland room to the Red Kihunter Shaft room.</summary>
    public static DoorDefinition WastelandToRedKihunterShaft { get; } = DoorDefinitions.Find(nameof(WastelandToRedKihunterShaft));

    /// <summary>The door from the Metal Pirates room to the Plowerhouse room.</summary>
    public static DoorDefinition MetalPiratesToPlowerhouse { get; } = DoorDefinitions.Find(nameof(MetalPiratesToPlowerhouse));

    /// <summary>The door from the Metal Pirates room to the Wasteland room.</summary>
    public static DoorDefinition MetalPiratesToWasteland { get; } = DoorDefinitions.Find(nameof(MetalPiratesToWasteland));

    /// <summary>The door from the Three Musketeers room to the Single Chamber room.</summary>
    public static DoorDefinition ThreeMusketeersToSingleChamber { get; } = DoorDefinitions.Find(nameof(ThreeMusketeersToSingleChamber));

    /// <summary>The door from the Three Musketeers room to the Lower Norfair Spring Ball Maze room.</summary>
    public static DoorDefinition ThreeMusketeersToLowerNorfairSpringBallMaze { get; } = DoorDefinitions.Find(nameof(ThreeMusketeersToLowerNorfairSpringBallMaze));

    /// <summary>The door from the Ridley Energy Tank room to the Ridley room.</summary>
    public static DoorDefinition RidleyEnergyTankToRidley { get; } = DoorDefinitions.Find(nameof(RidleyEnergyTankToRidley));

    /// <summary>The door from the Screw Attack room to the Fast Ripper room.</summary>
    public static DoorDefinition ScrewAttackToFastRipper { get; } = DoorDefinitions.Find(nameof(ScrewAttackToFastRipper));

    /// <summary>The door from the Screw Attack room to the Golden Torizo Energy Refill room.</summary>
    public static DoorDefinition ScrewAttackToGoldenTorizoEnergyRefill { get; } = DoorDefinitions.Find(nameof(ScrewAttackToGoldenTorizoEnergyRefill));

    /// <summary>The door from the Screw Attack room to the Golden Torizo room.</summary>
    public static DoorDefinition ScrewAttackToGoldenTorizo { get; } = DoorDefinitions.Find(nameof(ScrewAttackToGoldenTorizo));

    /// <summary>The door from the Lower Norfair Fireflea room to the Lower Norfair Spring Ball Maze room.</summary>
    public static DoorDefinition LowerNorfairFirefleaToLowerNorfairSpringBallMaze { get; } = DoorDefinitions.Find(nameof(LowerNorfairFirefleaToLowerNorfairSpringBallMaze));

    /// <summary>The door from the Lower Norfair Fireflea room to the Lower Norfair Escape Power Bombs room.</summary>
    public static DoorDefinition LowerNorfairFirefleaToLowerNorfairEscapePowerBombs { get; } = DoorDefinitions.Find(nameof(LowerNorfairFirefleaToLowerNorfairEscapePowerBombs));

    /// <summary>The door from the Lower Norfair Fireflea room to the Red Kihunter Shaft room.</summary>
    public static DoorDefinition LowerNorfairFirefleaToRedKihunterShaft { get; } = DoorDefinitions.Find(nameof(LowerNorfairFirefleaToRedKihunterShaft));

    /// <summary>The door from the Lower Norfair Save room to the Red Kihunter Shaft room.</summary>
    public static DoorDefinition LowerNorfairSaveToRedKihunterShaft { get; } = DoorDefinitions.Find(nameof(LowerNorfairSaveToRedKihunterShaft));
}
