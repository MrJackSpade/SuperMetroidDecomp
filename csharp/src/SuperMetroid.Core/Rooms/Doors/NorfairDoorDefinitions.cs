namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The Norfair rooms' doors, each named for the room it leads from and the room it leads to.</summary>
public static class NorfairDoorDefinitions
{
    /// <summary>The door from the Ice Beam Acid room to the Ice Beam Gate room.</summary>
    public static DoorDefinition IceBeamAcidToIceBeamGate { get; } = new("IceBeamAcidToIceBeamGate", RoomDefinitions.IceBeamGate, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(49, 38), new(3, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Ice Beam Acid room to the Ice Beam Snake room.</summary>
    public static DoorDefinition IceBeamAcidToIceBeamSnake { get; } = new("IceBeamAcidToIceBeamSnake", RoomDefinitions.IceBeamSnake, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 38), new(0, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Cathedral room to the Cathedral Entrance room.</summary>
    public static DoorDefinition CathedralToCathedralEntrance { get; } = new("CathedralToCathedralEntrance", RoomDefinitions.CathedralEntrance, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(46, 6), new(2, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Cathedral room to the Rising Tide room.</summary>
    public static DoorDefinition CathedralToRisingTide { get; } = new("CathedralToRisingTide", RoomDefinitions.RisingTide, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Cathedral Entrance room to the Business Center room.</summary>
    public static DoorDefinition CathedralEntranceToBusinessCenter { get; } = new("CathedralEntranceToBusinessCenter", RoomDefinitions.BusinessCenter, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 54), new(0, 3), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Cathedral Entrance room to the Cathedral room.</summary>
    public static DoorDefinition CathedralEntranceToCathedral { get; } = new("CathedralEntranceToCathedral", RoomDefinitions.Cathedral, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Business Center room to the Ice Beam Gate room.</summary>
    public static DoorDefinition BusinessCenterToIceBeamGate { get; } = new("BusinessCenterToIceBeamGate", RoomDefinitions.IceBeamGate, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(110, 38), new(6, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Business Center room to the Cathedral Entrance room.</summary>
    public static DoorDefinition BusinessCenterToCathedralEntrance { get; } = new("BusinessCenterToCathedralEntrance", RoomDefinitions.CathedralEntrance, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Business Center room to the Hi Jump Energy Tank room.</summary>
    public static DoorDefinition BusinessCenterToHiJumpEnergyTank { get; } = new("BusinessCenterToHiJumpEnergyTank", RoomDefinitions.HiJumpEnergyTank, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 6), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Business Center room to the Crocomire Escape room.</summary>
    public static DoorDefinition BusinessCenterToCrocomireEscape { get; } = new("BusinessCenterToCrocomireEscape", RoomDefinitions.CrocomireEscape, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Business Center room to the Warehouse Entrance room.</summary>
    public static DoorDefinition BusinessCenterToWarehouseEntrance { get; } = new("BusinessCenterToWarehouseEntrance", RoomDefinitions.WarehouseEntrance, new(DoorDirection.Up, DoorClosingBehavior.None), new(0, 0), new(0, 0), DoorSamusSpeed.None, DoorSetupDefinitions.None);

    /// <summary>The door from the Business Center room to the Frog Save room.</summary>
    public static DoorDefinition BusinessCenterToFrogSave { get; } = new("BusinessCenterToFrogSave", RoomDefinitions.FrogSave, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Business Center room to the Norfair Map room.</summary>
    public static DoorDefinition BusinessCenterToNorfairMap { get; } = new("BusinessCenterToNorfairMap", RoomDefinitions.NorfairMap, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Ice Beam Gate room to the Ice Beam Tutorial room.</summary>
    public static DoorDefinition IceBeamGateToIceBeamTutorial { get; } = new("IceBeamGateToIceBeamTutorial", RoomDefinitions.IceBeamTutorial, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 6), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Ice Beam Gate room to the Ice Beam Acid room.</summary>
    public static DoorDefinition IceBeamGateToIceBeamAcid { get; } = new("IceBeamGateToIceBeamAcid", RoomDefinitions.IceBeamAcid, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 6), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Ice Beam Gate room to the Business Center room.</summary>
    public static DoorDefinition IceBeamGateToBusinessCenter { get; } = new("IceBeamGateToBusinessCenter", RoomDefinitions.BusinessCenter, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 54), new(0, 3), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Ice Beam Gate room to the Crumble Shaft room.</summary>
    public static DoorDefinition IceBeamGateToCrumbleShaft { get; } = new("IceBeamGateToCrumbleShaft", RoomDefinitions.CrumbleShaft, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Ice Beam Tutorial room to the Ice Beam Snake room.</summary>
    public static DoorDefinition IceBeamTutorialToIceBeamSnake { get; } = new("IceBeamTutorialToIceBeamSnake", RoomDefinitions.IceBeamSnake, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Ice Beam Tutorial room to the Ice Beam Gate room.</summary>
    public static DoorDefinition IceBeamTutorialToIceBeamGate { get; } = new("IceBeamTutorialToIceBeamGate", RoomDefinitions.IceBeamGate, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(49, 6), new(3, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.IceBeamGateOpensScreen3x1);

    /// <summary>The door from the Ice Beam room to the Ice Beam Snake room.</summary>
    public static DoorDefinition IceBeamToIceBeamSnake { get; } = new("IceBeamToIceBeamSnake", RoomDefinitions.IceBeamSnake, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 22), new(1, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.IceBeamSnakeClosesScreen0x1OpensScreen1x1TopAligned);

    /// <summary>The door from the Ice Beam Snake room to the Ice Beam Acid room.</summary>
    public static DoorDefinition IceBeamSnakeToIceBeamAcid { get; } = new("IceBeamSnakeToIceBeamAcid", RoomDefinitions.IceBeamAcid, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Ice Beam Snake room to the Ice Beam Tutorial room.</summary>
    public static DoorDefinition IceBeamSnakeToIceBeamTutorial { get; } = new("IceBeamSnakeToIceBeamTutorial", RoomDefinitions.IceBeamTutorial, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Ice Beam Snake room to the Ice Beam room.</summary>
    public static DoorDefinition IceBeamSnakeToIceBeam { get; } = new("IceBeamSnakeToIceBeam", RoomDefinitions.IceBeam, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Crumble Shaft room to the Ice Beam Gate room.</summary>
    public static DoorDefinition CrumbleShaftToIceBeamGate { get; } = new("CrumbleShaftToIceBeamGate", RoomDefinitions.IceBeamGate, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 54), new(0, 3), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.IceBeamGateOpensScreen3x3TopAligned);

    /// <summary>The door from the Crumble Shaft room to the Crocomire Speedway room.</summary>
    public static DoorDefinition CrumbleShaftToCrocomireSpeedway { get; } = new("CrumbleShaftToCrocomireSpeedway", RoomDefinitions.CrocomireSpeedway, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Crocomire Speedway room to the Crumble Shaft room.</summary>
    public static DoorDefinition CrocomireSpeedwayToCrumbleShaft { get; } = new("CrocomireSpeedwayToCrumbleShaft", RoomDefinitions.CrumbleShaft, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 54), new(0, 3), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Crocomire Speedway room to the Crocomire Escape room.</summary>
    public static DoorDefinition CrocomireSpeedwayToCrocomireEscape { get; } = new("CrocomireSpeedwayToCrocomireEscape", RoomDefinitions.CrocomireEscape, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(62, 22), new(3, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Crocomire Speedway room to the Crocomire Save room.</summary>
    public static DoorDefinition CrocomireSpeedwayToCrocomireSave { get; } = new("CrocomireSpeedwayToCrocomireSave", RoomDefinitions.CrocomireSave, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Crocomire Speedway room to the Acid Snakes Tunnel room.</summary>
    public static DoorDefinition CrocomireSpeedwayToAcidSnakesTunnel { get; } = new("CrocomireSpeedwayToAcidSnakesTunnel", RoomDefinitions.AcidSnakesTunnel, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Crocomire Speedway room to the Crocomire room.</summary>
    public static DoorDefinition CrocomireSpeedwayToCrocomire { get; } = new("CrocomireSpeedwayToCrocomire", RoomDefinitions.Crocomire, new(DoorDirection.Down, DoorClosingBehavior.BlueDoorCloses), new(54, 2), new(3, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Crocomire room to the Post Crocomire Farming room.</summary>
    public static DoorDefinition CrocomireToPostCrocomireFarming { get; } = new("CrocomireToPostCrocomireFarming", RoomDefinitions.PostCrocomireFarming, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 6), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Crocomire room to the Crocomire Speedway room.</summary>
    public static DoorDefinition CrocomireToCrocomireSpeedway { get; } = new("CrocomireToCrocomireSpeedway", RoomDefinitions.CrocomireSpeedway, new(DoorDirection.Up, DoorClosingBehavior.BlueDoorCloses), new(198, 45), new(12, 2), DoorSamusSpeed.PixelsPerStep(1, 3, 4), DoorSetupDefinitions.None);

    /// <summary>The door from the Hi Jump Boots room to the Hi Jump Energy Tank room.</summary>
    public static DoorDefinition HiJumpBootsToHiJumpEnergyTank { get; } = new("HiJumpBootsToHiJumpEnergyTank", RoomDefinitions.HiJumpEnergyTank, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.HiJumpEnergyTankOpensScreens0x0And0x1);

    /// <summary>The door from the Crocomire Escape room to the Business Center room.</summary>
    public static DoorDefinition CrocomireEscapeToBusinessCenter { get; } = new("CrocomireEscapeToBusinessCenter", RoomDefinitions.BusinessCenter, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 102), new(0, 6), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Crocomire Escape room to the Crocomire Speedway room.</summary>
    public static DoorDefinition CrocomireEscapeToCrocomireSpeedway { get; } = new("CrocomireEscapeToCrocomireSpeedway", RoomDefinitions.CrocomireSpeedway, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(193, 6), new(12, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Hi Jump Energy Tank room to the Business Center room.</summary>
    public static DoorDefinition HiJumpEnergyTankToBusinessCenter { get; } = new("HiJumpEnergyTankToBusinessCenter", RoomDefinitions.BusinessCenter, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 86), new(0, 5), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Hi Jump Energy Tank room to the Hi Jump Boots room.</summary>
    public static DoorDefinition HiJumpEnergyTankToHiJumpBoots { get; } = new("HiJumpEnergyTankToHiJumpBoots", RoomDefinitions.HiJumpBoots, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Post Crocomire Farming room to the Crocomire room.</summary>
    public static DoorDefinition PostCrocomireFarmingToCrocomire { get; } = new("PostCrocomireFarmingToCrocomire", RoomDefinitions.Crocomire, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.CrocomireOpensScreens0x0And1x0TopAligned);

    /// <summary>The door from the Post Crocomire Farming room to the Post Crocomire Power Bombs room.</summary>
    public static DoorDefinition PostCrocomireFarmingToPostCrocomirePowerBombs { get; } = new("PostCrocomireFarmingToPostCrocomirePowerBombs", RoomDefinitions.PostCrocomirePowerBombs, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Post Crocomire Farming room to the Post Crocomire Shaft room.</summary>
    public static DoorDefinition PostCrocomireFarmingToPostCrocomireShaft { get; } = new("PostCrocomireFarmingToPostCrocomireShaft", RoomDefinitions.PostCrocomireShaft, new(DoorDirection.Down, DoorClosingBehavior.BlueDoorCloses), new(6, 2), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Post Crocomire Farming room to the Post Crocomire Save room.</summary>
    public static DoorDefinition PostCrocomireFarmingToPostCrocomireSave { get; } = new("PostCrocomireFarmingToPostCrocomireSave", RoomDefinitions.PostCrocomireSave, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Post Crocomire Save room to the Post Crocomire Farming room.</summary>
    public static DoorDefinition PostCrocomireSaveToPostCrocomireFarming { get; } = new("PostCrocomireSaveToPostCrocomireFarming", RoomDefinitions.PostCrocomireFarming, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 22), new(1, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Post Crocomire Power Bombs room to the Post Crocomire Farming room.</summary>
    public static DoorDefinition PostCrocomirePowerBombsToPostCrocomireFarming { get; } = new("PostCrocomirePowerBombsToPostCrocomireFarming", RoomDefinitions.PostCrocomireFarming, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Post Crocomire Shaft room to the Post Crocomire Farming room.</summary>
    public static DoorDefinition PostCrocomireShaftToPostCrocomireFarming { get; } = new("PostCrocomireShaftToPostCrocomireFarming", RoomDefinitions.PostCrocomireFarming, new(DoorDirection.Up, DoorClosingBehavior.BlueDoorCloses), new(6, 29), new(0, 1), DoorSamusSpeed.PixelsPerStep(1, 3, 4), DoorSetupDefinitions.None);

    /// <summary>The door from the Post Crocomire Shaft room to the Grapple Tutorial 3 room.</summary>
    public static DoorDefinition PostCrocomireShaftToGrappleTutorial3 { get; } = new("PostCrocomireShaftToGrappleTutorial3", RoomDefinitions.GrappleTutorial3, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(46, 6), new(2, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Post Crocomire Shaft room to the Post Crocomire Missile room.</summary>
    public static DoorDefinition PostCrocomireShaftToPostCrocomireMissile { get; } = new("PostCrocomireShaftToPostCrocomireMissile", RoomDefinitions.PostCrocomireMissile, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Post Crocomire Shaft room to the Post Crocomire Jump room.</summary>
    public static DoorDefinition PostCrocomireShaftToPostCrocomireJump { get; } = new("PostCrocomireShaftToPostCrocomireJump", RoomDefinitions.PostCrocomireJump, new(DoorDirection.Down, DoorClosingBehavior.BlueDoorCloses), new(102, 34), new(6, 2), DoorSamusSpeed.PixelsPerStep(1, 1, 4), DoorSetupDefinitions.None);

    /// <summary>The door from the Post Crocomire Missile room to the Post Crocomire Shaft room.</summary>
    public static DoorDefinition PostCrocomireMissileToPostCrocomireShaft { get; } = new("PostCrocomireMissileToPostCrocomireShaft", RoomDefinitions.PostCrocomireShaft, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 54), new(0, 3), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Grapple Tutorial 3 room to the Grapple Tutorial 2 room.</summary>
    public static DoorDefinition GrappleTutorial3ToGrappleTutorial2 { get; } = new("GrappleTutorial3ToGrappleTutorial2", RoomDefinitions.GrappleTutorial2, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Grapple Tutorial 3 room to the Post Crocomire Shaft room.</summary>
    public static DoorDefinition GrappleTutorial3ToPostCrocomireShaft { get; } = new("GrappleTutorial3ToPostCrocomireShaft", RoomDefinitions.PostCrocomireShaft, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Post Crocomire Jump room to the Post Crocomire Shaft room.</summary>
    public static DoorDefinition PostCrocomireJumpToPostCrocomireShaft { get; } = new("PostCrocomireJumpToPostCrocomireShaft", RoomDefinitions.PostCrocomireShaft, new(DoorDirection.Up, DoorClosingBehavior.BlueDoorCloses), new(6, 77), new(0, 4), DoorSamusSpeed.PixelsPerStep(1, 3, 4), DoorSetupDefinitions.None);

    /// <summary>The door from the Post Crocomire Jump room to the Grapple Beam room.</summary>
    public static DoorDefinition PostCrocomireJumpToGrappleBeam { get; } = new("PostCrocomireJumpToGrappleBeam", RoomDefinitions.GrappleBeam, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 38), new(0, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Grapple Tutorial 2 room to the Grapple Tutorial 1 room.</summary>
    public static DoorDefinition GrappleTutorial2ToGrappleTutorial1 { get; } = new("GrappleTutorial2ToGrappleTutorial1", RoomDefinitions.GrappleTutorial1, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 6), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Grapple Tutorial 2 room to the Grapple Tutorial 3 room.</summary>
    public static DoorDefinition GrappleTutorial2ToGrappleTutorial3 { get; } = new("GrappleTutorial2ToGrappleTutorial3", RoomDefinitions.GrappleTutorial3, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Grapple Tutorial 1 room to the Grapple Beam room.</summary>
    public static DoorDefinition GrappleTutorial1ToGrappleBeam { get; } = new("GrappleTutorial1ToGrappleBeam", RoomDefinitions.GrappleBeam, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Grapple Tutorial 1 room to the Grapple Tutorial 2 room.</summary>
    public static DoorDefinition GrappleTutorial1ToGrappleTutorial2 { get; } = new("GrappleTutorial1ToGrappleTutorial2", RoomDefinitions.GrappleTutorial2, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 38), new(0, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Grapple Beam room to the Post Crocomire Jump room.</summary>
    public static DoorDefinition GrappleBeamToPostCrocomireJump { get; } = new("GrappleBeamToPostCrocomireJump", RoomDefinitions.PostCrocomireJump, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Grapple Beam room to the Grapple Tutorial 1 room.</summary>
    public static DoorDefinition GrappleBeamToGrappleTutorial1 { get; } = new("GrappleBeamToGrappleTutorial1", RoomDefinitions.GrappleTutorial1, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The Norfair Reserve Tank room's door.</summary>
    public static DoorDefinition NorfairReserveTankToGreenBubblesMissiles { get; } = new("NorfairReserveTankToGreenBubblesMissiles", RoomDefinitions.GreenBubblesMissiles, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.GreenBubblesMissilesOpensScreen0x0TopAligned);

    /// <summary>The door from the Green Bubbles Missiles room to the Bubble Mountain room.</summary>
    public static DoorDefinition GreenBubblesMissilesToBubbleMountain { get; } = new("GreenBubblesMissilesToBubbleMountain", RoomDefinitions.BubbleMountain, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Green Bubbles Missiles room to the Norfair Reserve Tank room.</summary>
    public static DoorDefinition GreenBubblesMissilesToNorfairReserveTank { get; } = new("GreenBubblesMissilesToNorfairReserveTank", RoomDefinitions.NorfairReserveTank, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 6), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Bubble Mountain room to the Green Bubbles Missiles room.</summary>
    public static DoorDefinition BubbleMountainToGreenBubblesMissiles { get; } = new("BubbleMountainToGreenBubblesMissiles", RoomDefinitions.GreenBubblesMissiles, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 6), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Bubble Mountain room to the Rising Tide room.</summary>
    public static DoorDefinition BubbleMountainToRisingTide { get; } = new("BubbleMountainToRisingTide", RoomDefinitions.RisingTide, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(78, 6), new(4, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Bubble Mountain room to the Upper Norfair Farming room.</summary>
    public static DoorDefinition BubbleMountainToUpperNorfairFarming { get; } = new("BubbleMountainToUpperNorfairFarming", RoomDefinitions.UpperNorfairFarming, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 6), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Bubble Mountain room to the Purple Shaft room.</summary>
    public static DoorDefinition BubbleMountainToPurpleShaft { get; } = new("BubbleMountainToPurpleShaft", RoomDefinitions.PurpleShaft, new(DoorDirection.Down, DoorClosingBehavior.BlueDoorCloses), new(6, 2), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.PurpleShaftClosesScreen0x1OpensScreen0x0TopAligned);

    /// <summary>The door from the Bubble Mountain room to the Single Chamber room.</summary>
    public static DoorDefinition BubbleMountainToSingleChamber { get; } = new("BubbleMountainToSingleChamber", RoomDefinitions.SingleChamber, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Bubble Mountain room to the Bat Cave room.</summary>
    public static DoorDefinition BubbleMountainToBatCave { get; } = new("BubbleMountainToBatCave", RoomDefinitions.BatCave, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Bubble Mountain room to the Bubble Mountain Save room.</summary>
    public static DoorDefinition BubbleMountainToBubbleMountainSave { get; } = new("BubbleMountainToBubbleMountainSave", RoomDefinitions.BubbleMountainSave, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Speed Booster Hall room to the Bat Cave room.</summary>
    public static DoorDefinition SpeedBoosterHallToBatCave { get; } = new("SpeedBoosterHallToBatCave", RoomDefinitions.BatCave, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.BatCaveClosesScreen0x1OpensScreen0x0TopAligned);

    /// <summary>The door from the Speed Booster Hall room to the Speed Booster room.</summary>
    public static DoorDefinition SpeedBoosterHallToSpeedBooster { get; } = new("SpeedBoosterHallToSpeedBooster", RoomDefinitions.SpeedBooster, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Speed Booster room to the Speed Booster Hall room.</summary>
    public static DoorDefinition SpeedBoosterToSpeedBoosterHall { get; } = new("SpeedBoosterToSpeedBoosterHall", RoomDefinitions.SpeedBoosterHall, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(190, 22), new(11, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Single Chamber room to the Bubble Mountain room.</summary>
    public static DoorDefinition SingleChamberToBubbleMountain { get; } = new("SingleChamberToBubbleMountain", RoomDefinitions.BubbleMountain, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 22), new(1, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Single Chamber room to the Double Chamber room, arriving at its upper doorway.</summary>
    public static DoorDefinition SingleChamberToDoubleChamberUpper { get; } = new("SingleChamberToDoubleChamberUpper", RoomDefinitions.DoubleChamber, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Single Chamber room to the Double Chamber room, arriving at its lower doorway.</summary>
    public static DoorDefinition SingleChamberToDoubleChamberLower { get; } = new("SingleChamberToDoubleChamberLower", RoomDefinitions.DoubleChamber, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Single Chamber room to the Spiky Platforms Tunnel room.</summary>
    public static DoorDefinition SingleChamberToSpikyPlatformsTunnel { get; } = new("SingleChamberToSpikyPlatformsTunnel", RoomDefinitions.SpikyPlatformsTunnel, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Single Chamber room to the Three Musketeers room.</summary>
    public static DoorDefinition SingleChamberToThreeMusketeers { get; } = new("SingleChamberToThreeMusketeers", RoomDefinitions.ThreeMusketeers, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(17, 6), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Double Chamber room to the Single Chamber room, arriving at its upper doorway.</summary>
    public static DoorDefinition DoubleChamberToSingleChamberUpper { get; } = new("DoubleChamberToSingleChamberUpper", RoomDefinitions.SingleChamber, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Double Chamber room to the Single Chamber room, arriving at its lower doorway.</summary>
    public static DoorDefinition DoubleChamberToSingleChamberLower { get; } = new("DoubleChamberToSingleChamberLower", RoomDefinitions.SingleChamber, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 38), new(0, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Double Chamber room to the Wave Beam room.</summary>
    public static DoorDefinition DoubleChamberToWaveBeam { get; } = new("DoubleChamberToWaveBeam", RoomDefinitions.WaveBeam, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Wave Beam room to the Double Chamber room.</summary>
    public static DoorDefinition WaveBeamToDoubleChamber { get; } = new("WaveBeamToDoubleChamber", RoomDefinitions.DoubleChamber, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(62, 6), new(3, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Spiky Platforms Tunnel room to the Single Chamber room.</summary>
    public static DoorDefinition SpikyPlatformsTunnelToSingleChamber { get; } = new("SpikyPlatformsTunnelToSingleChamber", RoomDefinitions.SingleChamber, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 54), new(0, 3), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Spiky Platforms Tunnel room to the Volcano room.</summary>
    public static DoorDefinition SpikyPlatformsTunnelToVolcano { get; } = new("SpikyPlatformsTunnelToVolcano", RoomDefinitions.Volcano, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(33, 6), new(2, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Volcano room to the Spiky Platforms Tunnel room.</summary>
    public static DoorDefinition VolcanoToSpikyPlatformsTunnel { get; } = new("VolcanoToSpikyPlatformsTunnel", RoomDefinitions.SpikyPlatformsTunnel, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(62, 6), new(3, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Volcano room to the Kronic Boost room.</summary>
    public static DoorDefinition VolcanoToKronicBoost { get; } = new("VolcanoToKronicBoost", RoomDefinitions.KronicBoost, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 6), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Kronic Boost room to the Magdollite Tunnel room.</summary>
    public static DoorDefinition KronicBoostToMagdolliteTunnel { get; } = new("KronicBoostToMagdolliteTunnel", RoomDefinitions.MagdolliteTunnel, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(46, 6), new(2, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Kronic Boost room to the Volcano room.</summary>
    public static DoorDefinition KronicBoostToVolcano { get; } = new("KronicBoostToVolcano", RoomDefinitions.Volcano, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 38), new(0, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.VolcanoClosesScreen2x2OpensScreens0x2And1x2TopAligned);

    /// <summary>The door from the Kronic Boost room to the Lava Dive room.</summary>
    public static DoorDefinition KronicBoostToLavaDive { get; } = new("KronicBoostToLavaDive", RoomDefinitions.LavaDive, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(62, 6), new(3, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Kronic Boost room to the Spiky Acid Snakes Tunnel room.</summary>
    public static DoorDefinition KronicBoostToSpikyAcidSnakesTunnel { get; } = new("KronicBoostToSpikyAcidSnakesTunnel", RoomDefinitions.SpikyAcidSnakesTunnel, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(62, 6), new(3, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Magdollite Tunnel room to the Purple Shaft room.</summary>
    public static DoorDefinition MagdolliteTunnelToPurpleShaft { get; } = new("MagdolliteTunnelToPurpleShaft", RoomDefinitions.PurpleShaft, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 38), new(0, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Magdollite Tunnel room to the Kronic Boost room.</summary>
    public static DoorDefinition MagdolliteTunnelToKronicBoost { get; } = new("MagdolliteTunnelToKronicBoost", RoomDefinitions.KronicBoost, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(17, 6), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Purple Shaft room to the Bubble Mountain room.</summary>
    public static DoorDefinition PurpleShaftToBubbleMountain { get; } = new("PurpleShaftToBubbleMountain", RoomDefinitions.BubbleMountain, new(DoorDirection.Up, DoorClosingBehavior.BlueDoorCloses), new(6, 61), new(0, 3), DoorSamusSpeed.PixelsPerStep(1, 3, 4), DoorSetupDefinitions.None);

    /// <summary>The door from the Purple Shaft room to the Magdollite Tunnel room.</summary>
    public static DoorDefinition PurpleShaftToMagdolliteTunnel { get; } = new("PurpleShaftToMagdolliteTunnel", RoomDefinitions.MagdolliteTunnel, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Purple Shaft room to the Purple Farming room.</summary>
    public static DoorDefinition PurpleShaftToPurpleFarming { get; } = new("PurpleShaftToPurpleFarming", RoomDefinitions.PurpleFarming, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Lava Dive room to the Kronic Boost room.</summary>
    public static DoorDefinition LavaDiveToKronicBoost { get; } = new("LavaDiveToKronicBoost", RoomDefinitions.KronicBoost, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(17, 38), new(1, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Lava Dive room to the Lower Norfair Elevator room.</summary>
    public static DoorDefinition LavaDiveToLowerNorfairElevator { get; } = new("LavaDiveToLowerNorfairElevator", RoomDefinitions.LowerNorfairElevator, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Lower Norfair Elevator room to the Lava Dive room.</summary>
    public static DoorDefinition LowerNorfairElevatorToLavaDive { get; } = new("LowerNorfairElevatorToLavaDive", RoomDefinitions.LavaDive, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Lower Norfair Elevator room to the Main Hall room.</summary>
    public static DoorDefinition LowerNorfairElevatorToMainHall { get; } = new("LowerNorfairElevatorToMainHall", RoomDefinitions.MainHall, new(DoorDirection.Down, DoorClosingBehavior.None), new(0, 0), new(4, 0), DoorSamusSpeed.None, DoorSetupDefinitions.None);

    /// <summary>The door from the Lower Norfair Elevator room to the Lower Norfair Elevator Save room.</summary>
    public static DoorDefinition LowerNorfairElevatorToLowerNorfairElevatorSave { get; } = new("LowerNorfairElevatorToLowerNorfairElevatorSave", RoomDefinitions.LowerNorfairElevatorSave, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Upper Norfair Farming room to the Frog Speedway room.</summary>
    public static DoorDefinition UpperNorfairFarmingToFrogSpeedway { get; } = new("UpperNorfairFarmingToFrogSpeedway", RoomDefinitions.FrogSpeedway, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(126, 6), new(7, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Upper Norfair Farming room to the Bubble Mountain room.</summary>
    public static DoorDefinition UpperNorfairFarmingToBubbleMountain { get; } = new("UpperNorfairFarmingToBubbleMountain", RoomDefinitions.BubbleMountain, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 54), new(0, 3), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Upper Norfair Farming room to the Red Pirate Shaft room.</summary>
    public static DoorDefinition UpperNorfairFarmingToRedPirateShaft { get; } = new("UpperNorfairFarmingToRedPirateShaft", RoomDefinitions.RedPirateShaft, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Rising Tide room to the Cathedral room.</summary>
    public static DoorDefinition RisingTideToCathedral { get; } = new("RisingTideToCathedral", RoomDefinitions.Cathedral, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(46, 22), new(2, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Rising Tide room to the Bubble Mountain room.</summary>
    public static DoorDefinition RisingTideToBubbleMountain { get; } = new("RisingTideToBubbleMountain", RoomDefinitions.BubbleMountain, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 38), new(0, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Acid Snakes Tunnel room to the Crocomire Speedway room.</summary>
    public static DoorDefinition AcidSnakesTunnelToCrocomireSpeedway { get; } = new("AcidSnakesTunnelToCrocomireSpeedway", RoomDefinitions.CrocomireSpeedway, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(206, 38), new(12, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Acid Snakes Tunnel room to the Upper Norfair Refill room.</summary>
    public static DoorDefinition AcidSnakesTunnelToUpperNorfairRefill { get; } = new("AcidSnakesTunnelToUpperNorfairRefill", RoomDefinitions.UpperNorfairRefill, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Acid Snakes Tunnel room to the Red Pirate Shaft room.</summary>
    public static DoorDefinition AcidSnakesTunnelToRedPirateShaft { get; } = new("AcidSnakesTunnelToRedPirateShaft", RoomDefinitions.RedPirateShaft, new(DoorDirection.Up, DoorClosingBehavior.BlueDoorCloses), new(6, 45), new(0, 2), DoorSamusSpeed.PixelsPerStep(2), DoorSetupDefinitions.None);

    /// <summary>The door from the Spiky Acid Snakes Tunnel room to the Upper Norfair Refill room.</summary>
    public static DoorDefinition SpikyAcidSnakesTunnelToUpperNorfairRefill { get; } = new("SpikyAcidSnakesTunnelToUpperNorfairRefill", RoomDefinitions.UpperNorfairRefill, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Spiky Acid Snakes Tunnel room to the Kronic Boost room.</summary>
    public static DoorDefinition SpikyAcidSnakesTunnelToKronicBoost { get; } = new("SpikyAcidSnakesTunnelToKronicBoost", RoomDefinitions.KronicBoost, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.KronicBoostClosesScreen1x1OpensScreen0x1TopAligned);

    /// <summary>The door from the Upper Norfair Refill room to the Acid Snakes Tunnel room.</summary>
    public static DoorDefinition UpperNorfairRefillToAcidSnakesTunnel { get; } = new("UpperNorfairRefillToAcidSnakesTunnel", RoomDefinitions.AcidSnakesTunnel, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(62, 6), new(3, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Upper Norfair Refill room to the Spiky Acid Snakes Tunnel room.</summary>
    public static DoorDefinition UpperNorfairRefillToSpikyAcidSnakesTunnel { get; } = new("UpperNorfairRefillToSpikyAcidSnakesTunnel", RoomDefinitions.SpikyAcidSnakesTunnel, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Purple Farming room to the Purple Shaft room.</summary>
    public static DoorDefinition PurpleFarmingToPurpleShaft { get; } = new("PurpleFarmingToPurpleShaft", RoomDefinitions.PurpleShaft, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Bat Cave room to the Bubble Mountain room.</summary>
    public static DoorDefinition BatCaveToBubbleMountain { get; } = new("BatCaveToBubbleMountain", RoomDefinitions.BubbleMountain, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 6), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Bat Cave room to the Speed Booster Hall room.</summary>
    public static DoorDefinition BatCaveToSpeedBoosterHall { get; } = new("BatCaveToSpeedBoosterHall", RoomDefinitions.SpeedBoosterHall, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Norfair Map room to the Business Center room.</summary>
    public static DoorDefinition NorfairMapToBusinessCenter { get; } = new("NorfairMapToBusinessCenter", RoomDefinitions.BusinessCenter, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 70), new(0, 4), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Bubble Mountain Save room to the Bubble Mountain room.</summary>
    public static DoorDefinition BubbleMountainSaveToBubbleMountain { get; } = new("BubbleMountainSaveToBubbleMountain", RoomDefinitions.BubbleMountain, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Frog Speedway room to the Frog Save room.</summary>
    public static DoorDefinition FrogSpeedwayToFrogSave { get; } = new("FrogSpeedwayToFrogSave", RoomDefinitions.FrogSave, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Frog Speedway room to the Upper Norfair Farming room.</summary>
    public static DoorDefinition FrogSpeedwayToUpperNorfairFarming { get; } = new("FrogSpeedwayToUpperNorfairFarming", RoomDefinitions.UpperNorfairFarming, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Red Pirate Shaft room to the Upper Norfair Farming room.</summary>
    public static DoorDefinition RedPirateShaftToUpperNorfairFarming { get; } = new("RedPirateShaftToUpperNorfairFarming", RoomDefinitions.UpperNorfairFarming, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Red Pirate Shaft room to the Acid Snakes Tunnel room.</summary>
    public static DoorDefinition RedPirateShaftToAcidSnakesTunnel { get; } = new("RedPirateShaftToAcidSnakesTunnel", RoomDefinitions.AcidSnakesTunnel, new(DoorDirection.Down, DoorClosingBehavior.BlueDoorCloses), new(54, 2), new(3, 0), DoorSamusSpeed.PixelsPerStep(1, 1, 4), DoorSetupDefinitions.None);

    /// <summary>The door from the Frog Save room to the Frog Speedway room.</summary>
    public static DoorDefinition FrogSaveToFrogSpeedway { get; } = new("FrogSaveToFrogSpeedway", RoomDefinitions.FrogSpeedway, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Frog Save room to the Business Center room.</summary>
    public static DoorDefinition FrogSaveToBusinessCenter { get; } = new("FrogSaveToBusinessCenter", RoomDefinitions.BusinessCenter, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 86), new(0, 5), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Crocomire Save room to the Crocomire Speedway room.</summary>
    public static DoorDefinition CrocomireSaveToCrocomireSpeedway { get; } = new("CrocomireSaveToCrocomireSpeedway", RoomDefinitions.CrocomireSpeedway, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(174, 22), new(12, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Lower Norfair Elevator Save room to the Lower Norfair Elevator room.</summary>
    public static DoorDefinition LowerNorfairElevatorSaveToLowerNorfairElevator { get; } = new("LowerNorfairElevatorSaveToLowerNorfairElevator", RoomDefinitions.LowerNorfairElevator, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Acid Statue room to the Golden Torizo room.</summary>
    public static DoorDefinition AcidStatueToGoldenTorizo { get; } = new("AcidStatueToGoldenTorizo", RoomDefinitions.GoldenTorizo, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.GoldenTorizoClosesScreens1x0And0x1And1x1OpensScreen0x0TopAligned);

    /// <summary>The door from the Acid Statue room to the Main Hall room.</summary>
    public static DoorDefinition AcidStatueToMainHall { get; } = new("AcidStatueToMainHall", RoomDefinitions.MainHall, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 38), new(0, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Main Hall room to the Acid Statue room.</summary>
    public static DoorDefinition MainHallToAcidStatue { get; } = new("MainHallToAcidStatue", RoomDefinitions.AcidStatue, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 6), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Main Hall room to the Fast Pillars Setup room.</summary>
    public static DoorDefinition MainHallToFastPillarsSetup { get; } = new("MainHallToFastPillarsSetup", RoomDefinitions.FastPillarsSetup, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Main Hall room to the Lower Norfair Elevator room.</summary>
    public static DoorDefinition MainHallToLowerNorfairElevator { get; } = new("MainHallToLowerNorfairElevator", RoomDefinitions.LowerNorfairElevator, new(DoorDirection.Up, DoorClosingBehavior.None), new(0, 0), new(0, 0), DoorSamusSpeed.None, DoorSetupDefinitions.None);

    /// <summary>The door from the Golden Torizo room to the Acid Statue room.</summary>
    public static DoorDefinition GoldenTorizoToAcidStatue { get; } = new("GoldenTorizoToAcidStatue", RoomDefinitions.AcidStatue, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(46, 38), new(2, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.AcidStatueClosesScreens0x1And1x1OpensScreens0x2And1x2And2x2TopAligned);

    /// <summary>The door from the Golden Torizo room to the Screw Attack room.</summary>
    public static DoorDefinition GoldenTorizoToScrewAttack { get; } = new("GoldenTorizoToScrewAttack", RoomDefinitions.ScrewAttack, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 38), new(0, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Fast Ripper room to the Screw Attack room.</summary>
    public static DoorDefinition FastRipperToScrewAttack { get; } = new("FastRipperToScrewAttack", RoomDefinitions.ScrewAttack, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Fast Ripper room to the Fast Pillars Setup room.</summary>
    public static DoorDefinition FastRipperToFastPillarsSetup { get; } = new("FastRipperToFastPillarsSetup", RoomDefinitions.FastPillarsSetup, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 38), new(0, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.FastPillarsSetupClosesScreen0x1OpensScreen0x2TopAligned);

    /// <summary>The door from the Golden Torizo Energy Refill room to the Screw Attack room.</summary>
    public static DoorDefinition GoldenTorizoEnergyRefillToScrewAttack { get; } = new("GoldenTorizoEnergyRefillToScrewAttack", RoomDefinitions.ScrewAttack, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Ridley room to the Ridley Energy Tank room.</summary>
    public static DoorDefinition RidleyToRidleyEnergyTank { get; } = new("RidleyToRidleyEnergyTank", RoomDefinitions.RidleyEnergyTank, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Ridley room to the Lower Norfair Farming room.</summary>
    public static DoorDefinition RidleyToLowerNorfairFarming { get; } = new("RidleyToLowerNorfairFarming", RoomDefinitions.LowerNorfairFarming, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Lower Norfair Farming room to the Ridley room.</summary>
    public static DoorDefinition LowerNorfairFarmingToRidley { get; } = new("LowerNorfairFarmingToRidley", RoomDefinitions.Ridley, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Lower Norfair Farming room to the Plowerhouse room.</summary>
    public static DoorDefinition LowerNorfairFarmingToPlowerhouse { get; } = new("LowerNorfairFarmingToPlowerhouse", RoomDefinitions.Plowerhouse, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Fast Pillars Setup room to the Main Hall room.</summary>
    public static DoorDefinition FastPillarsSetupToMainHall { get; } = new("FastPillarsSetupToMainHall", RoomDefinitions.MainHall, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(126, 38), new(7, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Fast Pillars Setup room to the Mickey Mouse room.</summary>
    public static DoorDefinition FastPillarsSetupToMickeyMouse { get; } = new("FastPillarsSetupToMickeyMouse", RoomDefinitions.MickeyMouse, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 54), new(0, 3), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Fast Pillars Setup room to the Main Hall room: a second door the room lists to the Main Hall, leading where the first does.</summary>
    public static DoorDefinition FastPillarsSetupToMainHallAlternate { get; } = new("FastPillarsSetupToMainHallAlternate", RoomDefinitions.MainHall, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(126, 38), new(7, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Fast Pillars Setup room to the Fast Ripper room.</summary>
    public static DoorDefinition FastPillarsSetupToFastRipper { get; } = new("FastPillarsSetupToFastRipper", RoomDefinitions.FastRipper, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(62, 6), new(3, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Fast Pillars Setup room to the Pillar room.</summary>
    public static DoorDefinition FastPillarsSetupToPillar { get; } = new("FastPillarsSetupToPillar", RoomDefinitions.Pillar, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Mickey Mouse room to the Fast Pillars Setup room.</summary>
    public static DoorDefinition MickeyMouseToFastPillarsSetup { get; } = new("MickeyMouseToFastPillarsSetup", RoomDefinitions.FastPillarsSetup, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Mickey Mouse room to the Worst Room In The Game room.</summary>
    public static DoorDefinition MickeyMouseToWorstRoomInTheGame { get; } = new("MickeyMouseToWorstRoomInTheGame", RoomDefinitions.WorstRoomInTheGame, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.WorstRoomInTheGameOpensScreen0x0);

    /// <summary>The door from the Pillar room to the Fast Pillars Setup room.</summary>
    public static DoorDefinition PillarToFastPillarsSetup { get; } = new("PillarToFastPillarsSetup", RoomDefinitions.FastPillarsSetup, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 38), new(0, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.FastPillarsSetupClosesScreen0x1OpensScreen0x2TopAligned);

    /// <summary>The door from the Pillar room to the Worst Room In The Game room.</summary>
    public static DoorDefinition PillarToWorstRoomInTheGame { get; } = new("PillarToWorstRoomInTheGame", RoomDefinitions.WorstRoomInTheGame, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 86), new(0, 5), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Plowerhouse room to the Lower Norfair Farming room.</summary>
    public static DoorDefinition PlowerhouseToLowerNorfairFarming { get; } = new("PlowerhouseToLowerNorfairFarming", RoomDefinitions.LowerNorfairFarming, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(46, 6), new(2, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Plowerhouse room to the Metal Pirates room.</summary>
    public static DoorDefinition PlowerhouseToMetalPirates { get; } = new("PlowerhouseToMetalPirates", RoomDefinitions.MetalPirates, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Worst Room In The Game room to the Mickey Mouse room.</summary>
    public static DoorDefinition WorstRoomInTheGameToMickeyMouse { get; } = new("WorstRoomInTheGameToMickeyMouse", RoomDefinitions.MickeyMouse, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(62, 6), new(3, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.MickeyMouseClosesScreen2x3);

    /// <summary>The door from the Worst Room In The Game room to the Amphitheatre room.</summary>
    public static DoorDefinition WorstRoomInTheGameToAmphitheatre { get; } = new("WorstRoomInTheGameToAmphitheatre", RoomDefinitions.Amphitheatre, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Worst Room In The Game room to the Pillar room.</summary>
    public static DoorDefinition WorstRoomInTheGameToPillar { get; } = new("WorstRoomInTheGameToPillar", RoomDefinitions.Pillar, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(62, 6), new(3, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Amphitheatre room to the Worst Room In The Game room.</summary>
    public static DoorDefinition AmphitheatreToWorstRoomInTheGame { get; } = new("AmphitheatreToWorstRoomInTheGame", RoomDefinitions.WorstRoomInTheGame, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Amphitheatre room to the Red Kihunter Shaft room.</summary>
    public static DoorDefinition AmphitheatreToRedKihunterShaft { get; } = new("AmphitheatreToRedKihunterShaft", RoomDefinitions.RedKihunterShaft, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.RedKihunterShaftClosesScreen0x1OpensScreen0x0TopAligned);

    /// <summary>The door from the Lower Norfair Spring Ball Maze room to the Three Musketeers room.</summary>
    public static DoorDefinition LowerNorfairSpringBallMazeToThreeMusketeers { get; } = new("LowerNorfairSpringBallMazeToThreeMusketeers", RoomDefinitions.ThreeMusketeers, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(62, 38), new(3, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Lower Norfair Spring Ball Maze room to the Lower Norfair Fireflea room.</summary>
    public static DoorDefinition LowerNorfairSpringBallMazeToLowerNorfairFireflea { get; } = new("LowerNorfairSpringBallMazeToLowerNorfairFireflea", RoomDefinitions.LowerNorfairFireflea, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.LowerNorfairFirefleaClosesScreen1x1OpensScreens0x0And1x0TopAligned);

    /// <summary>The door from the Lower Norfair Spring Ball Maze room to the Lower Norfair Escape Power Bombs room.</summary>
    public static DoorDefinition LowerNorfairSpringBallMazeToLowerNorfairEscapePowerBombs { get; } = new("LowerNorfairSpringBallMazeToLowerNorfairEscapePowerBombs", RoomDefinitions.LowerNorfairEscapePowerBombs, new(DoorDirection.Down, DoorClosingBehavior.BlueDoorCloses), new(6, 2), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Lower Norfair Escape Power Bombs room to the Lower Norfair Fireflea room.</summary>
    public static DoorDefinition LowerNorfairEscapePowerBombsToLowerNorfairFireflea { get; } = new("LowerNorfairEscapePowerBombsToLowerNorfairFireflea", RoomDefinitions.LowerNorfairFireflea, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 6), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.LowerNorfairFirefleaClosesScreen1x1OpensScreen1x0TopAligned);

    /// <summary>The door from the Lower Norfair Escape Power Bombs room to the Lower Norfair Spring Ball Maze room.</summary>
    public static DoorDefinition LowerNorfairEscapePowerBombsToLowerNorfairSpringBallMaze { get; } = new("LowerNorfairEscapePowerBombsToLowerNorfairSpringBallMaze", RoomDefinitions.LowerNorfairSpringBallMaze, new(DoorDirection.Up, DoorClosingBehavior.BlueDoorCloses), new(70, 13), new(4, 0), DoorSamusSpeed.PixelsPerStep(1, 3, 4), DoorSetupDefinitions.LowerNorfairSpringBallMazeClosesScreen1x1OpensScreen4x0OpensScreens1x0And2x0And3x0TopAligned);

    /// <summary>The door from the Red Kihunter Shaft room to the Wasteland room.</summary>
    public static DoorDefinition RedKihunterShaftToWasteland { get; } = new("RedKihunterShaftToWasteland", RoomDefinitions.Wasteland, new(DoorDirection.Down, DoorClosingBehavior.BlueDoorCloses), new(86, 2), new(5, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Red Kihunter Shaft room to the Amphitheatre room.</summary>
    public static DoorDefinition RedKihunterShaftToAmphitheatre { get; } = new("RedKihunterShaftToAmphitheatre", RoomDefinitions.Amphitheatre, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(62, 6), new(3, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Red Kihunter Shaft room to the Lower Norfair Fireflea room.</summary>
    public static DoorDefinition RedKihunterShaftToLowerNorfairFireflea { get; } = new("RedKihunterShaftToLowerNorfairFireflea", RoomDefinitions.LowerNorfairFireflea, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(17, 54), new(1, 3), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Red Kihunter Shaft room to the Lower Norfair Save room.</summary>
    public static DoorDefinition RedKihunterShaftToLowerNorfairSave { get; } = new("RedKihunterShaftToLowerNorfairSave", RoomDefinitions.LowerNorfairSave, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Wasteland room to the Metal Pirates room.</summary>
    public static DoorDefinition WastelandToMetalPirates { get; } = new("WastelandToMetalPirates", RoomDefinitions.MetalPirates, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(46, 6), new(2, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Wasteland room to the Red Kihunter Shaft room.</summary>
    public static DoorDefinition WastelandToRedKihunterShaft { get; } = new("WastelandToRedKihunterShaft", RoomDefinitions.RedKihunterShaft, new(DoorDirection.Up, DoorClosingBehavior.BlueDoorCloses), new(38, 77), new(2, 4), DoorSamusSpeed.PixelsPerStep(2), DoorSetupDefinitions.None);

    /// <summary>The door from the Metal Pirates room to the Plowerhouse room.</summary>
    public static DoorDefinition MetalPiratesToPlowerhouse { get; } = new("MetalPiratesToPlowerhouse", RoomDefinitions.Plowerhouse, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(46, 6), new(2, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Metal Pirates room to the Wasteland room.</summary>
    public static DoorDefinition MetalPiratesToWasteland { get; } = new("MetalPiratesToWasteland", RoomDefinitions.Wasteland, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(17, 38), new(1, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.WastelandOpensScreen1x1);

    /// <summary>The door from the Three Musketeers room to the Single Chamber room.</summary>
    public static DoorDefinition ThreeMusketeersToSingleChamber { get; } = new("ThreeMusketeersToSingleChamber", RoomDefinitions.SingleChamber, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(94, 6), new(5, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Three Musketeers room to the Lower Norfair Spring Ball Maze room.</summary>
    public static DoorDefinition ThreeMusketeersToLowerNorfairSpringBallMaze { get; } = new("ThreeMusketeersToLowerNorfairSpringBallMaze", RoomDefinitions.LowerNorfairSpringBallMaze, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Ridley Energy Tank room to the Ridley room.</summary>
    public static DoorDefinition RidleyEnergyTankToRidley { get; } = new("RidleyEnergyTankToRidley", RoomDefinitions.Ridley, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Screw Attack room to the Fast Ripper room.</summary>
    public static DoorDefinition ScrewAttackToFastRipper { get; } = new("ScrewAttackToFastRipper", RoomDefinitions.FastRipper, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Screw Attack room to the Golden Torizo Energy Refill room.</summary>
    public static DoorDefinition ScrewAttackToGoldenTorizoEnergyRefill { get; } = new("ScrewAttackToGoldenTorizoEnergyRefill", RoomDefinitions.GoldenTorizoEnergyRefill, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Screw Attack room to the Golden Torizo room.</summary>
    public static DoorDefinition ScrewAttackToGoldenTorizo { get; } = new("ScrewAttackToGoldenTorizo", RoomDefinitions.GoldenTorizo, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 22), new(1, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Lower Norfair Fireflea room to the Lower Norfair Spring Ball Maze room.</summary>
    public static DoorDefinition LowerNorfairFirefleaToLowerNorfairSpringBallMaze { get; } = new("LowerNorfairFirefleaToLowerNorfairSpringBallMaze", RoomDefinitions.LowerNorfairSpringBallMaze, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 22), new(1, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Lower Norfair Fireflea room to the Lower Norfair Escape Power Bombs room.</summary>
    public static DoorDefinition LowerNorfairFirefleaToLowerNorfairEscapePowerBombs { get; } = new("LowerNorfairFirefleaToLowerNorfairEscapePowerBombs", RoomDefinitions.LowerNorfairEscapePowerBombs, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Lower Norfair Fireflea room to the Red Kihunter Shaft room.</summary>
    public static DoorDefinition LowerNorfairFirefleaToRedKihunterShaft { get; } = new("LowerNorfairFirefleaToRedKihunterShaft", RoomDefinitions.RedKihunterShaft, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.RedKihunterShaftClosesScreen0x1OpensScreen0x0TopAligned);

    /// <summary>The door from the Lower Norfair Save room to the Red Kihunter Shaft room.</summary>
    public static DoorDefinition LowerNorfairSaveToRedKihunterShaft { get; } = new("LowerNorfairSaveToRedKihunterShaft", RoomDefinitions.RedKihunterShaft, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 54), new(0, 3), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);
}
