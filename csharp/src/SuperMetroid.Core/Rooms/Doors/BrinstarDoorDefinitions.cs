namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The Brinstar rooms' doors, each named for the room it leads from and the room it leads to.</summary>
public static class BrinstarDoorDefinitions
{
    /// <summary>The door from the Green Brinstar Main Shaft room to the Green Brinstar Elevator room.</summary>
    public static DoorDefinition GreenBrinstarMainShaftToGreenBrinstarElevator { get; } = new("GreenBrinstarMainShaftToGreenBrinstarElevator", RoomDefinitions.GreenBrinstarElevator, new(DoorDirection.Up, DoorClosingBehavior.None), new(0, 0), new(0, 0), DoorSamusSpeed.None, DoorSetupDefinitions.GreenBrinstarElevatorOpensScreen0x2BelowTheRoom);

    /// <summary>The door from the Green Brinstar Main Shaft room to the Brinstar Pre Map room.</summary>
    public static DoorDefinition GreenBrinstarMainShaftToBrinstarPreMap { get; } = new("GreenBrinstarMainShaftToBrinstarPreMap", RoomDefinitions.BrinstarPreMap, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(46, 6), new(2, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Green Brinstar Main Shaft room to the Green Brinstar Beetoms room.</summary>
    public static DoorDefinition GreenBrinstarMainShaftToGreenBrinstarBeetoms { get; } = new("GreenBrinstarMainShaftToGreenBrinstarBeetoms", RoomDefinitions.GreenBrinstarBeetoms, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Green Brinstar Main Shaft room to the Green Brinstar Firefleas room.</summary>
    public static DoorDefinition GreenBrinstarMainShaftToGreenBrinstarFirefleas { get; } = new("GreenBrinstarMainShaftToGreenBrinstarFirefleas", RoomDefinitions.GreenBrinstarFirefleas, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(46, 6), new(2, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Green Brinstar Main Shaft room to the Early Supers room.</summary>
    public static DoorDefinition GreenBrinstarMainShaftToEarlySupers { get; } = new("GreenBrinstarMainShaftToEarlySupers", RoomDefinitions.EarlySupers, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Green Brinstar Main Shaft room to the Dachora room.</summary>
    public static DoorDefinition GreenBrinstarMainShaftToDachora { get; } = new("GreenBrinstarMainShaftToDachora", RoomDefinitions.Dachora, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.DachoraClosesScreen4x0);

    /// <summary>The door at the foot of the Green Brinstar Main Shaft room that leads to its own right side.</summary>
    public static DoorDefinition GreenBrinstarMainShaftToItsRightSide { get; } = new("GreenBrinstarMainShaftToItsRightSide", RoomDefinitions.GreenBrinstarMainShaft, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 118), new(1, 7), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.GreenBrinstarMainShaftOpensScreen1x7TopAligned);

    /// <summary>The door from the Green Brinstar Main Shaft room to the Etecoon Energy Tank room.</summary>
    public static DoorDefinition GreenBrinstarMainShaftToEtecoonEnergyTank { get; } = new("GreenBrinstarMainShaftToEtecoonEnergyTank", RoomDefinitions.EtecoonEnergyTank, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(78, 22), new(4, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.EtecoonEnergyTankOpensScreens0x1And1x1TopAligned);

    /// <summary>The door at the foot of the Green Brinstar Main Shaft room that leads to its own left side.</summary>
    public static DoorDefinition GreenBrinstarMainShaftToItsLeftSide { get; } = new("GreenBrinstarMainShaftToItsLeftSide", RoomDefinitions.GreenBrinstarMainShaft, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 118), new(0, 7), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.GreenBrinstarMainShaftOpensScreens0x6And0x7);

    /// <summary>The door from the Green Brinstar Main Shaft room to the Green Brinstar Save room.</summary>
    public static DoorDefinition GreenBrinstarMainShaftToGreenBrinstarSave { get; } = new("GreenBrinstarMainShaftToGreenBrinstarSave", RoomDefinitions.GreenBrinstarSave, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Spore Spawn Super room to the Spore Spawn Farming room.</summary>
    public static DoorDefinition SporeSpawnSuperToSporeSpawnFarming { get; } = new("SporeSpawnSuperToSporeSpawnFarming", RoomDefinitions.SporeSpawnFarming, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(46, 6), new(2, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Spore Spawn Super room to the Spore Spawn room.</summary>
    public static DoorDefinition SporeSpawnSuperToSporeSpawn { get; } = new("SporeSpawnSuperToSporeSpawn", RoomDefinitions.SporeSpawn, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Brinstar Pre Map room to the Green Brinstar Map room.</summary>
    public static DoorDefinition BrinstarPreMapToGreenBrinstarMap { get; } = new("BrinstarPreMapToGreenBrinstarMap", RoomDefinitions.GreenBrinstarMap, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Brinstar Pre Map room to the Green Brinstar Main Shaft room.</summary>
    public static DoorDefinition BrinstarPreMapToGreenBrinstarMainShaft { get; } = new("BrinstarPreMapToGreenBrinstarMainShaft", RoomDefinitions.GreenBrinstarMainShaft, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 70), new(0, 4), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Early Supers room to the Green Brinstar Main Shaft room.</summary>
    public static DoorDefinition EarlySupersToGreenBrinstarMainShaft { get; } = new("EarlySupersToGreenBrinstarMainShaft", RoomDefinitions.GreenBrinstarMainShaft, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 70), new(0, 4), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Early Supers room to the Brinstar Reserve Tank room.</summary>
    public static DoorDefinition EarlySupersToBrinstarReserveTank { get; } = new("EarlySupersToBrinstarReserveTank", RoomDefinitions.BrinstarReserveTank, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Brinstar Reserve Tank room to the Early Supers room.</summary>
    public static DoorDefinition BrinstarReserveTankToEarlySupers { get; } = new("BrinstarReserveTankToEarlySupers", RoomDefinitions.EarlySupers, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(46, 22), new(2, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Green Brinstar Map room to the Brinstar Pre Map room.</summary>
    public static DoorDefinition GreenBrinstarMapToBrinstarPreMap { get; } = new("GreenBrinstarMapToBrinstarPreMap", RoomDefinitions.BrinstarPreMap, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Green Brinstar Firefleas room to the Green Brinstar Missile Refill room.</summary>
    public static DoorDefinition GreenBrinstarFirefleasToGreenBrinstarMissileRefill { get; } = new("GreenBrinstarFirefleasToGreenBrinstarMissileRefill", RoomDefinitions.GreenBrinstarMissileRefill, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Green Brinstar Firefleas room to the Green Brinstar Main Shaft room.</summary>
    public static DoorDefinition GreenBrinstarFirefleasToGreenBrinstarMainShaft { get; } = new("GreenBrinstarFirefleasToGreenBrinstarMainShaft", RoomDefinitions.GreenBrinstarMainShaft, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 102), new(0, 6), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Green Brinstar Missile Refill room to the Green Brinstar Firefleas room.</summary>
    public static DoorDefinition GreenBrinstarMissileRefillToGreenBrinstarFirefleas { get; } = new("GreenBrinstarMissileRefillToGreenBrinstarFirefleas", RoomDefinitions.GreenBrinstarFirefleas, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Dachora room to the Green Brinstar Main Shaft room.</summary>
    public static DoorDefinition DachoraToGreenBrinstarMainShaft { get; } = new("DachoraToGreenBrinstarMainShaft", RoomDefinitions.GreenBrinstarMainShaft, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 102), new(0, 6), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Dachora room to the Big Pink room.</summary>
    public static DoorDefinition DachoraToBigPink { get; } = new("DachoraToBigPink", RoomDefinitions.BigPink, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(33, 38), new(2, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Dachora room to the Dachora Energy Refill room.</summary>
    public static DoorDefinition DachoraToDachoraEnergyRefill { get; } = new("DachoraToDachoraEnergyRefill", RoomDefinitions.DachoraEnergyRefill, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Big Pink room to the Spore Spawn Kihunters room.</summary>
    public static DoorDefinition BigPinkToSporeSpawnKihunters { get; } = new("BigPinkToSporeSpawnKihunters", RoomDefinitions.SporeSpawnKihunters, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Big Pink room to the Dachora room.</summary>
    public static DoorDefinition BigPinkToDachora { get; } = new("BigPinkToDachora", RoomDefinitions.Dachora, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(110, 6), new(6, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Big Pink room to the Pink Brinstar Power Bombs room, arriving at its lower doorway.</summary>
    public static DoorDefinition BigPinkToPinkBrinstarPowerBombsLower { get; } = new("BigPinkToPinkBrinstarPowerBombsLower", RoomDefinitions.PinkBrinstarPowerBombs, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 22), new(1, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.PinkBrinstarPowerBombsOpensScreens0x1And1x1);

    /// <summary>The door from the Big Pink room to the Green Hill Zone room.</summary>
    public static DoorDefinition BigPinkToGreenHillZone { get; } = new("BigPinkToGreenHillZone", RoomDefinitions.GreenHillZone, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Big Pink room to the Big Pink Save Room room.</summary>
    public static DoorDefinition BigPinkToBigPinkSaveRoom { get; } = new("BigPinkToBigPinkSaveRoom", RoomDefinitions.BigPinkSaveRoom, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Big Pink room to the Pink Brinstar Power Bombs room, arriving at its upper doorway.</summary>
    public static DoorDefinition BigPinkToPinkBrinstarPowerBombsUpper { get; } = new("BigPinkToPinkBrinstarPowerBombsUpper", RoomDefinitions.PinkBrinstarPowerBombs, new(DoorDirection.Left, DoorClosingBehavior.None), new(30, 6), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Big Pink room to the Waterway Energy Tank room.</summary>
    public static DoorDefinition BigPinkToWaterwayEnergyTank { get; } = new("BigPinkToWaterwayEnergyTank", RoomDefinitions.WaterwayEnergyTank, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(110, 6), new(6, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Big Pink room to the Pink Brinstar Hoppers room.</summary>
    public static DoorDefinition BigPinkToPinkBrinstarHoppers { get; } = new("BigPinkToPinkBrinstarHoppers", RoomDefinitions.PinkBrinstarHoppers, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Big Pink room to the Spore Spawn Farming room.</summary>
    public static DoorDefinition BigPinkToSporeSpawnFarming { get; } = new("BigPinkToSporeSpawnFarming", RoomDefinitions.SporeSpawnFarming, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Spore Spawn Kihunters room to the Big Pink room.</summary>
    public static DoorDefinition SporeSpawnKihuntersToBigPink { get; } = new("SporeSpawnKihuntersToBigPink", RoomDefinitions.BigPink, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(62, 6), new(3, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Spore Spawn Kihunters room to the Spore Spawn room.</summary>
    public static DoorDefinition SporeSpawnKihuntersToSporeSpawn { get; } = new("SporeSpawnKihuntersToSporeSpawn", RoomDefinitions.SporeSpawn, new(DoorDirection.Up, DoorClosingBehavior.BlueDoorCloses), new(6, 46), new(0, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Spore Spawn room to the Spore Spawn Super room.</summary>
    public static DoorDefinition SporeSpawnToSporeSpawnSuper { get; } = new("SporeSpawnToSporeSpawnSuper", RoomDefinitions.SporeSpawnSuper, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.SporeSpawnSuperClosesScreen1x1OpensScreen1x0TopAligned);

    /// <summary>The door from the Spore Spawn room to the Spore Spawn Kihunters room.</summary>
    public static DoorDefinition SporeSpawnToSporeSpawnKihunters { get; } = new("SporeSpawnToSporeSpawnKihunters", RoomDefinitions.SporeSpawnKihunters, new(DoorDirection.Down, DoorClosingBehavior.BlueDoorCloses), new(54, 3), new(3, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Pink Brinstar Power Bombs room to the Big Pink room, arriving at its upper doorway.</summary>
    public static DoorDefinition PinkBrinstarPowerBombsToBigPinkUpper { get; } = new("PinkBrinstarPowerBombsToBigPinkUpper", RoomDefinitions.BigPink, new(DoorDirection.Right, DoorClosingBehavior.None), new(0, 0), new(2, 3), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Pink Brinstar Power Bombs room to the Big Pink room, arriving at its lower doorway.</summary>
    public static DoorDefinition PinkBrinstarPowerBombsToBigPinkLower { get; } = new("PinkBrinstarPowerBombsToBigPinkLower", RoomDefinitions.BigPink, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(33, 70), new(2, 4), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Green Hill Zone room to the Big Pink room.</summary>
    public static DoorDefinition GreenHillZoneToBigPink { get; } = new("GreenHillZoneToBigPink", RoomDefinitions.BigPink, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(62, 102), new(3, 6), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Green Hill Zone room to the Morph Ball room.</summary>
    public static DoorDefinition GreenHillZoneToMorphBall { get; } = new("GreenHillZoneToMorphBall", RoomDefinitions.MorphBall, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 38), new(0, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Green Hill Zone room to the Noob Bridge room.</summary>
    public static DoorDefinition GreenHillZoneToNoobBridge { get; } = new("GreenHillZoneToNoobBridge", RoomDefinitions.NoobBridge, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Morph Ball room to the Green Hill Zone room.</summary>
    public static DoorDefinition MorphBallToGreenHillZone { get; } = new("MorphBallToGreenHillZone", RoomDefinitions.GreenHillZone, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 6), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Morph Ball room to the Construction Zone room.</summary>
    public static DoorDefinition MorphBallToConstructionZone { get; } = new("MorphBallToConstructionZone", RoomDefinitions.ConstructionZone, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Morph Ball room to the Elevator To Blue Brinstar room.</summary>
    public static DoorDefinition MorphBallToElevatorToBlueBrinstar { get; } = new("MorphBallToElevatorToBlueBrinstar", RoomDefinitions.ElevatorToBlueBrinstar, new(DoorDirection.Up, DoorClosingBehavior.None), new(0, 0), new(0, 0), DoorSamusSpeed.None, DoorSetupDefinitions.None);

    /// <summary>The door from the Construction Zone room to the Morph Ball room.</summary>
    public static DoorDefinition ConstructionZoneToMorphBall { get; } = new("ConstructionZoneToMorphBall", RoomDefinitions.MorphBall, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(126, 38), new(7, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Construction Zone room to the Blue Brinstar Energy Tank room.</summary>
    public static DoorDefinition ConstructionZoneToBlueBrinstarEnergyTank { get; } = new("ConstructionZoneToBlueBrinstarEnergyTank", RoomDefinitions.BlueBrinstarEnergyTank, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 38), new(0, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Construction Zone room to the First Missile room.</summary>
    public static DoorDefinition ConstructionZoneToFirstMissile { get; } = new("ConstructionZoneToFirstMissile", RoomDefinitions.FirstMissile, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Blue Brinstar Energy Tank room to the Construction Zone room.</summary>
    public static DoorDefinition BlueBrinstarEnergyTankToConstructionZone { get; } = new("BlueBrinstarEnergyTankToConstructionZone", RoomDefinitions.ConstructionZone, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Blue Brinstar Energy Tank room to the Blue Brinstar Boulders room.</summary>
    public static DoorDefinition BlueBrinstarEnergyTankToBlueBrinstarBoulders { get; } = new("BlueBrinstarEnergyTankToBlueBrinstarBoulders", RoomDefinitions.BlueBrinstarBoulders, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 6), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Noob Bridge room to the Green Hill Zone room.</summary>
    public static DoorDefinition NoobBridgeToGreenHillZone { get; } = new("NoobBridgeToGreenHillZone", RoomDefinitions.GreenHillZone, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(126, 54), new(7, 3), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Noob Bridge room to the Red Tower room.</summary>
    public static DoorDefinition NoobBridgeToRedTower { get; } = new("NoobBridgeToRedTower", RoomDefinitions.RedTower, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 70), new(0, 4), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Green Brinstar Beetoms room to the Green Brinstar Main Shaft room.</summary>
    public static DoorDefinition GreenBrinstarBeetomsToGreenBrinstarMainShaft { get; } = new("GreenBrinstarBeetomsToGreenBrinstarMainShaft", RoomDefinitions.GreenBrinstarMainShaft, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 166), new(0, 10), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.GreenBrinstarMainShaftOpensScreens0x6And0x7);

    /// <summary>The door from the Green Brinstar Beetoms room to the Etecoon Energy Tank room.</summary>
    public static DoorDefinition GreenBrinstarBeetomsToEtecoonEnergyTank { get; } = new("GreenBrinstarBeetomsToEtecoonEnergyTank", RoomDefinitions.EtecoonEnergyTank, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 6), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Etecoon Energy Tank room to the Green Brinstar Beetoms room.</summary>
    public static DoorDefinition EtecoonEnergyTankToGreenBrinstarBeetoms { get; } = new("EtecoonEnergyTankToGreenBrinstarBeetoms", RoomDefinitions.GreenBrinstarBeetoms, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Etecoon Energy Tank room to the Etecoon Super room.</summary>
    public static DoorDefinition EtecoonEnergyTankToEtecoonSuper { get; } = new("EtecoonEnergyTankToEtecoonSuper", RoomDefinitions.EtecoonSuper, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Etecoon Energy Tank room to the Green Brinstar Main Shaft room.</summary>
    public static DoorDefinition EtecoonEnergyTankToGreenBrinstarMainShaft { get; } = new("EtecoonEnergyTankToGreenBrinstarMainShaft", RoomDefinitions.GreenBrinstarMainShaft, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(33, 182), new(2, 11), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.GreenBrinstarMainShaftOpensScreen1x7TopAligned);

    /// <summary>The door from the Etecoon Energy Tank room to the Etecoon Save room.</summary>
    public static DoorDefinition EtecoonEnergyTankToEtecoonSave { get; } = new("EtecoonEnergyTankToEtecoonSave", RoomDefinitions.EtecoonSave, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Etecoon Super room to the Etecoon Energy Tank room.</summary>
    public static DoorDefinition EtecoonSuperToEtecoonEnergyTank { get; } = new("EtecoonSuperToEtecoonEnergyTank", RoomDefinitions.EtecoonEnergyTank, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Dachora Energy Refill room to the Dachora room.</summary>
    public static DoorDefinition DachoraEnergyRefillToDachora { get; } = new("DachoraEnergyRefillToDachora", RoomDefinitions.Dachora, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 102), new(0, 6), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.DachoraOpensScreen4x1);

    /// <summary>The door from the Spore Spawn Farming room to the Spore Spawn Super room.</summary>
    public static DoorDefinition SporeSpawnFarmingToSporeSpawnSuper { get; } = new("SporeSpawnFarmingToSporeSpawnSuper", RoomDefinitions.SporeSpawnSuper, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 134), new(0, 8), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Spore Spawn Farming room to the Big Pink room.</summary>
    public static DoorDefinition SporeSpawnFarmingToBigPink { get; } = new("SporeSpawnFarmingToBigPink", RoomDefinitions.BigPink, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(78, 86), new(4, 5), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.BigPinkClosesScreen3x5OpensScreen4x5TopAligned);

    /// <summary>The door from the Waterway Energy Tank room to the Big Pink room.</summary>
    public static DoorDefinition WaterwayEnergyTankToBigPink { get; } = new("WaterwayEnergyTankToBigPink", RoomDefinitions.BigPink, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 150), new(0, 9), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.BigPinkOpensScreens2x6And1x7And2x7);

    /// <summary>The door from the Waterway Energy Tank room to the Kraid Refill room.</summary>
    public static DoorDefinition WaterwayEnergyTankToKraidRefill { get; } = new("WaterwayEnergyTankToKraidRefill", RoomDefinitions.KraidRefill, new(DoorDirection.Down, DoorClosingBehavior.BlueDoorCloses), new(22, 3), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the First Missile room to the Construction Zone room.</summary>
    public static DoorDefinition FirstMissileToConstructionZone { get; } = new("FirstMissileToConstructionZone", RoomDefinitions.ConstructionZone, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.ConstructionZoneOpensScreen0x0OpensScreen0x1TopAligned);

    /// <summary>The door from the Pink Brinstar Hoppers room to the Big Pink room.</summary>
    public static DoorDefinition PinkBrinstarHoppersToBigPink { get; } = new("PinkBrinstarHoppersToBigPink", RoomDefinitions.BigPink, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(62, 70), new(3, 4), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Pink Brinstar Hoppers room to the Hopper Energy Tank room.</summary>
    public static DoorDefinition PinkBrinstarHoppersToHopperEnergyTank { get; } = new("PinkBrinstarHoppersToHopperEnergyTank", RoomDefinitions.HopperEnergyTank, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Hopper Energy Tank room to the Pink Brinstar Hoppers room.</summary>
    public static DoorDefinition HopperEnergyTankToPinkBrinstarHoppers { get; } = new("HopperEnergyTankToPinkBrinstarHoppers", RoomDefinitions.PinkBrinstarHoppers, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 22), new(1, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Big Pink Save Room room to the Big Pink room.</summary>
    public static DoorDefinition BigPinkSaveRoomToBigPink { get; } = new("BigPinkSaveRoomToBigPink", RoomDefinitions.BigPink, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(17, 6), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.BigPinkOpensScreen1x0);

    /// <summary>The door from the Blue Brinstar Boulders room to the Blue Brinstar Energy Tank room.</summary>
    public static DoorDefinition BlueBrinstarBouldersToBlueBrinstarEnergyTank { get; } = new("BlueBrinstarBouldersToBlueBrinstarEnergyTank", RoomDefinitions.BlueBrinstarEnergyTank, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(33, 6), new(2, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.BlueBrinstarEnergyTankOpensScreen2x0TopAligned);

    /// <summary>The door from the Blue Brinstar Boulders room to the Billy Mays room.</summary>
    public static DoorDefinition BlueBrinstarBouldersToBillyMays { get; } = new("BlueBrinstarBouldersToBillyMays", RoomDefinitions.BillyMays, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Billy Mays room to the Blue Brinstar Boulders room.</summary>
    public static DoorDefinition BillyMaysToBlueBrinstarBoulders { get; } = new("BillyMaysToBlueBrinstarBoulders", RoomDefinitions.BlueBrinstarBoulders, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Green Brinstar Save room to the Green Brinstar Main Shaft room.</summary>
    public static DoorDefinition GreenBrinstarSaveToGreenBrinstarMainShaft { get; } = new("GreenBrinstarSaveToGreenBrinstarMainShaft", RoomDefinitions.GreenBrinstarMainShaft, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 86), new(0, 5), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Etecoon Save room to the Etecoon Energy Tank room.</summary>
    public static DoorDefinition EtecoonSaveToEtecoonEnergyTank { get; } = new("EtecoonSaveToEtecoonEnergyTank", RoomDefinitions.EtecoonEnergyTank, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.EtecoonEnergyTankOpensScreens0x1And1x1TopAligned);

    /// <summary>The door from the Red Tower room to the Hellway room.</summary>
    public static DoorDefinition RedTowerToHellway { get; } = new("RedTowerToHellway", RoomDefinitions.Hellway, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Red Tower room to the Noob Bridge room.</summary>
    public static DoorDefinition RedTowerToNoobBridge { get; } = new("RedTowerToNoobBridge", RoomDefinitions.NoobBridge, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(94, 6), new(5, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Red Tower room to the Red Brinstar Firefleas room.</summary>
    public static DoorDefinition RedTowerToRedBrinstarFirefleas { get; } = new("RedTowerToRedBrinstarFirefleas", RoomDefinitions.RedBrinstarFirefleas, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(126, 6), new(7, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Red Tower room to the Skree Boost room.</summary>
    public static DoorDefinition RedTowerToSkreeBoost { get; } = new("RedTowerToSkreeBoost", RoomDefinitions.SkreeBoost, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Red Tower room to the Red Tower Refill room.</summary>
    public static DoorDefinition RedTowerToRedTowerRefill { get; } = new("RedTowerToRedTowerRefill", RoomDefinitions.RedTowerRefill, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Red Brinstar Firefleas room to the Xray Scope room.</summary>
    public static DoorDefinition RedBrinstarFirefleasToXrayScope { get; } = new("RedBrinstarFirefleasToXrayScope", RoomDefinitions.XrayScope, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 6), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Red Brinstar Firefleas room to the Red Tower room.</summary>
    public static DoorDefinition RedBrinstarFirefleasToRedTower { get; } = new("RedBrinstarFirefleasToRedTower", RoomDefinitions.RedTower, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 102), new(0, 6), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Xray Scope room to the Red Brinstar Firefleas room.</summary>
    public static DoorDefinition XrayScopeToRedBrinstarFirefleas { get; } = new("XrayScopeToRedBrinstarFirefleas", RoomDefinitions.RedBrinstarFirefleas, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Hellway room to the Red Tower room.</summary>
    public static DoorDefinition HellwayToRedTower { get; } = new("HellwayToRedTower", RoomDefinitions.RedTower, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Hellway room to the Caterpillar room.</summary>
    public static DoorDefinition HellwayToCaterpillar { get; } = new("HellwayToCaterpillar", RoomDefinitions.Caterpillar, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 86), new(0, 5), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.CaterpillarOpensScreen0x2);

    /// <summary>The door from the Caterpillar room to the Alpha Power Bomb room.</summary>
    public static DoorDefinition CaterpillarToAlphaPowerBomb { get; } = new("CaterpillarToAlphaPowerBomb", RoomDefinitions.AlphaPowerBomb, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(46, 6), new(2, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Caterpillar room to the Beta Power Bomb room.</summary>
    public static DoorDefinition CaterpillarToBetaPowerBomb { get; } = new("CaterpillarToBetaPowerBomb", RoomDefinitions.BetaPowerBomb, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 6), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Caterpillar room to the Hellway room.</summary>
    public static DoorDefinition CaterpillarToHellway { get; } = new("CaterpillarToHellway", RoomDefinitions.Hellway, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(46, 6), new(2, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Caterpillar room to the Red Brinstar Elevator room.</summary>
    public static DoorDefinition CaterpillarToRedBrinstarElevator { get; } = new("CaterpillarToRedBrinstarElevator", RoomDefinitions.RedBrinstarElevator, new(DoorDirection.Up, DoorClosingBehavior.None), new(0, 0), new(0, 0), DoorSamusSpeed.None, DoorSetupDefinitions.None);

    /// <summary>The door from the Caterpillar room to the Red Fish room.</summary>
    public static DoorDefinition CaterpillarToRedFish { get; } = new("CaterpillarToRedFish", RoomDefinitions.RedFish, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.RedFishClosesScreen2x0OpensScreen1x0TopAligned);

    /// <summary>The door from the Caterpillar room to the Red Brinstar Save room.</summary>
    public static DoorDefinition CaterpillarToRedBrinstarSave { get; } = new("CaterpillarToRedBrinstarSave", RoomDefinitions.RedBrinstarSave, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Beta Power Bomb room to the Caterpillar room.</summary>
    public static DoorDefinition BetaPowerBombToCaterpillar { get; } = new("BetaPowerBombToCaterpillar", RoomDefinitions.Caterpillar, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 54), new(0, 3), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Alpha Power Bomb room to the Caterpillar room.</summary>
    public static DoorDefinition AlphaPowerBombToCaterpillar { get; } = new("AlphaPowerBombToCaterpillar", RoomDefinitions.Caterpillar, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 118), new(0, 7), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.CaterpillarOpensScreens0x5And0x6);

    /// <summary>The door from the Skree Boost room to the Red Tower room.</summary>
    public static DoorDefinition SkreeBoostToRedTower { get; } = new("SkreeBoostToRedTower", RoomDefinitions.RedTower, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 150), new(0, 9), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.RedTowerOpensScreens0x6And0x7);

    /// <summary>The door from the Skree Boost room to the Below Spazer room.</summary>
    public static DoorDefinition SkreeBoostToBelowSpazer { get; } = new("SkreeBoostToBelowSpazer", RoomDefinitions.BelowSpazer, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Below Spazer room to the Skree Boost room.</summary>
    public static DoorDefinition BelowSpazerToSkreeBoost { get; } = new("BelowSpazerToSkreeBoost", RoomDefinitions.SkreeBoost, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 6), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Below Spazer room to the West Tunnel room.</summary>
    public static DoorDefinition BelowSpazerToWestTunnel { get; } = new("BelowSpazerToWestTunnel", RoomDefinitions.WestTunnel, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Below Spazer room to the Spazer room.</summary>
    public static DoorDefinition BelowSpazerToSpazer { get; } = new("BelowSpazerToSpazer", RoomDefinitions.Spazer, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Spazer room to the Below Spazer room.</summary>
    public static DoorDefinition SpazerToBelowSpazer { get; } = new("SpazerToBelowSpazer", RoomDefinitions.BelowSpazer, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 6), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.BelowSpazerOpensScreens0x0And1x0);

    /// <summary>The door from the Warehouse Zeela room to the Warehouse Entrance room.</summary>
    public static DoorDefinition WarehouseZeelaToWarehouseEntrance { get; } = new("WarehouseZeelaToWarehouseEntrance", RoomDefinitions.WarehouseEntrance, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(46, 6), new(2, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.WarehouseEntranceClosesScreen0x0OpensScreen1x0);

    /// <summary>The door from the Warehouse Zeela room to the Warehouse Energy Tank room.</summary>
    public static DoorDefinition WarehouseZeelaToWarehouseEnergyTank { get; } = new("WarehouseZeelaToWarehouseEnergyTank", RoomDefinitions.WarehouseEnergyTank, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Warehouse Zeela room to the Warehouse Kihunter room.</summary>
    public static DoorDefinition WarehouseZeelaToWarehouseKihunter { get; } = new("WarehouseZeelaToWarehouseKihunter", RoomDefinitions.WarehouseKihunter, new(DoorDirection.Up, DoorClosingBehavior.BlueDoorCloses), new(6, 12), new(0, 0), DoorSamusSpeed.PixelsPerStep(2, 1, 4), DoorSetupDefinitions.None);

    /// <summary>The door from the Warehouse Energy Tank room to the Warehouse Zeela room.</summary>
    public static DoorDefinition WarehouseEnergyTankToWarehouseZeela { get; } = new("WarehouseEnergyTankToWarehouseZeela", RoomDefinitions.WarehouseZeela, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Warehouse Kihunter room to the Warehouse Zeela room.</summary>
    public static DoorDefinition WarehouseKihunterToWarehouseZeela { get; } = new("WarehouseKihunterToWarehouseZeela", RoomDefinitions.WarehouseZeela, new(DoorDirection.Down, DoorClosingBehavior.BlueDoorCloses), new(22, 19), new(1, 1), DoorSamusSpeed.PixelsPerStep(1, 3, 4), DoorSetupDefinitions.None);

    /// <summary>The door from the Warehouse Kihunter room to the Mini Kraid room.</summary>
    public static DoorDefinition WarehouseKihunterToMiniKraid { get; } = new("WarehouseKihunterToMiniKraid", RoomDefinitions.MiniKraid, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Warehouse Kihunter room to the Warehouse Save room.</summary>
    public static DoorDefinition WarehouseKihunterToWarehouseSave { get; } = new("WarehouseKihunterToWarehouseSave", RoomDefinitions.WarehouseSave, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Mini Kraid room to the Warehouse Kihunter room.</summary>
    public static DoorDefinition MiniKraidToWarehouseKihunter { get; } = new("MiniKraidToWarehouseKihunter", RoomDefinitions.WarehouseKihunter, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 22), new(1, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Mini Kraid room to the Kraid Eye Door room.</summary>
    public static DoorDefinition MiniKraidToKraidEyeDoor { get; } = new("MiniKraidToKraidEyeDoor", RoomDefinitions.KraidEyeDoor, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Kraid Eye Door room to the Mini Kraid room.</summary>
    public static DoorDefinition KraidEyeDoorToMiniKraid { get; } = new("KraidEyeDoorToMiniKraid", RoomDefinitions.MiniKraid, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(94, 6), new(5, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Kraid Eye Door room to the Kraid room.</summary>
    public static DoorDefinition KraidEyeDoorToKraid { get; } = new("KraidEyeDoorToKraid", RoomDefinitions.Kraid, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Kraid Eye Door room to the Kraid Refill room.</summary>
    public static DoorDefinition KraidEyeDoorToKraidRefill { get; } = new("KraidEyeDoorToKraidRefill", RoomDefinitions.KraidRefill, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Kraid room to the Kraid Eye Door room.</summary>
    public static DoorDefinition KraidToKraidEyeDoor { get; } = new("KraidToKraidEyeDoor", RoomDefinitions.KraidEyeDoor, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 22), new(1, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Kraid room to the Varia Suit room.</summary>
    public static DoorDefinition KraidToVariaSuit { get; } = new("KraidToVariaSuit", RoomDefinitions.VariaSuit, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Red Tower Refill room to the Red Tower room.</summary>
    public static DoorDefinition RedTowerRefillToRedTower { get; } = new("RedTowerRefillToRedTower", RoomDefinitions.RedTower, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 150), new(0, 9), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.RedTowerOpensScreens0x6And0x7);

    /// <summary>The door from the Kraid Refill room to the Kraid Eye Door room.</summary>
    public static DoorDefinition KraidRefillToKraidEyeDoor { get; } = new("KraidRefillToKraidEyeDoor", RoomDefinitions.KraidEyeDoor, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.KraidEyeDoorOpensScreen0x0);

    /// <summary>The door from the Warehouse Entrance room to the East Tunnel room.</summary>
    public static DoorDefinition WarehouseEntranceToEastTunnel { get; } = new("WarehouseEntranceToEastTunnel", RoomDefinitions.EastTunnel, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.EastTunnelClosesScreen0x0OpensScreen0x1TopAligned);

    /// <summary>The door from the Warehouse Entrance room to the Warehouse Zeela room.</summary>
    public static DoorDefinition WarehouseEntranceToWarehouseZeela { get; } = new("WarehouseEntranceToWarehouseZeela", RoomDefinitions.WarehouseZeela, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Warehouse Entrance room to the Business Center room.</summary>
    public static DoorDefinition WarehouseEntranceToBusinessCenter { get; } = new("WarehouseEntranceToBusinessCenter", RoomDefinitions.BusinessCenter, new(DoorDirection.Down, DoorClosingBehavior.None), new(0, 0), new(0, 0), DoorSamusSpeed.None, DoorSetupDefinitions.None);

    /// <summary>The door from the Varia Suit room to the Kraid room.</summary>
    public static DoorDefinition VariaSuitToKraid { get; } = new("VariaSuitToKraid", RoomDefinitions.Kraid, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 22), new(1, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Warehouse Save room to the Warehouse Kihunter room.</summary>
    public static DoorDefinition WarehouseSaveToWarehouseKihunter { get; } = new("WarehouseSaveToWarehouseKihunter", RoomDefinitions.WarehouseKihunter, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(62, 6), new(3, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.WarehouseKihunterOpensScreens2x0And3x0TopAligned);

    /// <summary>The door from the Red Brinstar Save room to the Caterpillar room.</summary>
    public static DoorDefinition RedBrinstarSaveToCaterpillar { get; } = new("RedBrinstarSaveToCaterpillar", RoomDefinitions.Caterpillar, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 70), new(0, 4), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);
}
