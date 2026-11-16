namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The Brinstar rooms' doors, each named for the room it leads from and the room it leads to.</summary>
public static class BrinstarDoorDefinitions
{
    /// <summary>The door from the Green Brinstar Main Shaft room to the Green Brinstar Elevator room.</summary>
    public static DoorDefinition GreenBrinstarMainShaftToGreenBrinstarElevator { get; } = DoorDefinitions.Find(nameof(GreenBrinstarMainShaftToGreenBrinstarElevator));

    /// <summary>The door from the Green Brinstar Main Shaft room to the Brinstar Pre Map room.</summary>
    public static DoorDefinition GreenBrinstarMainShaftToBrinstarPreMap { get; } = DoorDefinitions.Find(nameof(GreenBrinstarMainShaftToBrinstarPreMap));

    /// <summary>The door from the Green Brinstar Main Shaft room to the Green Brinstar Beetoms room.</summary>
    public static DoorDefinition GreenBrinstarMainShaftToGreenBrinstarBeetoms { get; } = DoorDefinitions.Find(nameof(GreenBrinstarMainShaftToGreenBrinstarBeetoms));

    /// <summary>The door from the Green Brinstar Main Shaft room to the Green Brinstar Firefleas room.</summary>
    public static DoorDefinition GreenBrinstarMainShaftToGreenBrinstarFirefleas { get; } = DoorDefinitions.Find(nameof(GreenBrinstarMainShaftToGreenBrinstarFirefleas));

    /// <summary>The door from the Green Brinstar Main Shaft room to the Early Supers room.</summary>
    public static DoorDefinition GreenBrinstarMainShaftToEarlySupers { get; } = DoorDefinitions.Find(nameof(GreenBrinstarMainShaftToEarlySupers));

    /// <summary>The door from the Green Brinstar Main Shaft room to the Dachora room.</summary>
    public static DoorDefinition GreenBrinstarMainShaftToDachora { get; } = DoorDefinitions.Find(nameof(GreenBrinstarMainShaftToDachora));

    /// <summary>The door at the foot of the Green Brinstar Main Shaft room that leads to its own right side.</summary>
    public static DoorDefinition GreenBrinstarMainShaftToItsRightSide { get; } = DoorDefinitions.Find(nameof(GreenBrinstarMainShaftToItsRightSide));

    /// <summary>The door from the Green Brinstar Main Shaft room to the Etecoon Energy Tank room.</summary>
    public static DoorDefinition GreenBrinstarMainShaftToEtecoonEnergyTank { get; } = DoorDefinitions.Find(nameof(GreenBrinstarMainShaftToEtecoonEnergyTank));

    /// <summary>The door at the foot of the Green Brinstar Main Shaft room that leads to its own left side.</summary>
    public static DoorDefinition GreenBrinstarMainShaftToItsLeftSide { get; } = DoorDefinitions.Find(nameof(GreenBrinstarMainShaftToItsLeftSide));

    /// <summary>The door from the Green Brinstar Main Shaft room to the Green Brinstar Save room.</summary>
    public static DoorDefinition GreenBrinstarMainShaftToGreenBrinstarSave { get; } = DoorDefinitions.Find(nameof(GreenBrinstarMainShaftToGreenBrinstarSave));

    /// <summary>The door from the Spore Spawn Super room to the Spore Spawn Farming room.</summary>
    public static DoorDefinition SporeSpawnSuperToSporeSpawnFarming { get; } = DoorDefinitions.Find(nameof(SporeSpawnSuperToSporeSpawnFarming));

    /// <summary>The door from the Spore Spawn Super room to the Spore Spawn room.</summary>
    public static DoorDefinition SporeSpawnSuperToSporeSpawn { get; } = DoorDefinitions.Find(nameof(SporeSpawnSuperToSporeSpawn));

    /// <summary>The door from the Brinstar Pre Map room to the Green Brinstar Map room.</summary>
    public static DoorDefinition BrinstarPreMapToGreenBrinstarMap { get; } = DoorDefinitions.Find(nameof(BrinstarPreMapToGreenBrinstarMap));

    /// <summary>The door from the Brinstar Pre Map room to the Green Brinstar Main Shaft room.</summary>
    public static DoorDefinition BrinstarPreMapToGreenBrinstarMainShaft { get; } = DoorDefinitions.Find(nameof(BrinstarPreMapToGreenBrinstarMainShaft));

    /// <summary>The door from the Early Supers room to the Green Brinstar Main Shaft room.</summary>
    public static DoorDefinition EarlySupersToGreenBrinstarMainShaft { get; } = DoorDefinitions.Find(nameof(EarlySupersToGreenBrinstarMainShaft));

    /// <summary>The door from the Early Supers room to the Brinstar Reserve Tank room.</summary>
    public static DoorDefinition EarlySupersToBrinstarReserveTank { get; } = DoorDefinitions.Find(nameof(EarlySupersToBrinstarReserveTank));

    /// <summary>The door from the Brinstar Reserve Tank room to the Early Supers room.</summary>
    public static DoorDefinition BrinstarReserveTankToEarlySupers { get; } = DoorDefinitions.Find(nameof(BrinstarReserveTankToEarlySupers));

    /// <summary>The door from the Green Brinstar Map room to the Brinstar Pre Map room.</summary>
    public static DoorDefinition GreenBrinstarMapToBrinstarPreMap { get; } = DoorDefinitions.Find(nameof(GreenBrinstarMapToBrinstarPreMap));

    /// <summary>The door from the Green Brinstar Firefleas room to the Green Brinstar Missile Refill room.</summary>
    public static DoorDefinition GreenBrinstarFirefleasToGreenBrinstarMissileRefill { get; } = DoorDefinitions.Find(nameof(GreenBrinstarFirefleasToGreenBrinstarMissileRefill));

    /// <summary>The door from the Green Brinstar Firefleas room to the Green Brinstar Main Shaft room.</summary>
    public static DoorDefinition GreenBrinstarFirefleasToGreenBrinstarMainShaft { get; } = DoorDefinitions.Find(nameof(GreenBrinstarFirefleasToGreenBrinstarMainShaft));

    /// <summary>The door from the Green Brinstar Missile Refill room to the Green Brinstar Firefleas room.</summary>
    public static DoorDefinition GreenBrinstarMissileRefillToGreenBrinstarFirefleas { get; } = DoorDefinitions.Find(nameof(GreenBrinstarMissileRefillToGreenBrinstarFirefleas));

    /// <summary>The door from the Dachora room to the Green Brinstar Main Shaft room.</summary>
    public static DoorDefinition DachoraToGreenBrinstarMainShaft { get; } = DoorDefinitions.Find(nameof(DachoraToGreenBrinstarMainShaft));

    /// <summary>The door from the Dachora room to the Big Pink room.</summary>
    public static DoorDefinition DachoraToBigPink { get; } = DoorDefinitions.Find(nameof(DachoraToBigPink));

    /// <summary>The door from the Dachora room to the Dachora Energy Refill room.</summary>
    public static DoorDefinition DachoraToDachoraEnergyRefill { get; } = DoorDefinitions.Find(nameof(DachoraToDachoraEnergyRefill));

    /// <summary>The door from the Big Pink room to the Spore Spawn Kihunters room.</summary>
    public static DoorDefinition BigPinkToSporeSpawnKihunters { get; } = DoorDefinitions.Find(nameof(BigPinkToSporeSpawnKihunters));

    /// <summary>The door from the Big Pink room to the Dachora room.</summary>
    public static DoorDefinition BigPinkToDachora { get; } = DoorDefinitions.Find(nameof(BigPinkToDachora));

    /// <summary>The door from the Big Pink room to the Pink Brinstar Power Bombs room, arriving at its lower doorway.</summary>
    public static DoorDefinition BigPinkToPinkBrinstarPowerBombsLower { get; } = DoorDefinitions.Find(nameof(BigPinkToPinkBrinstarPowerBombsLower));

    /// <summary>The door from the Big Pink room to the Green Hill Zone room.</summary>
    public static DoorDefinition BigPinkToGreenHillZone { get; } = DoorDefinitions.Find(nameof(BigPinkToGreenHillZone));

    /// <summary>The door from the Big Pink room to the Big Pink Save Room room.</summary>
    public static DoorDefinition BigPinkToBigPinkSaveRoom { get; } = DoorDefinitions.Find(nameof(BigPinkToBigPinkSaveRoom));

    /// <summary>The door from the Big Pink room to the Pink Brinstar Power Bombs room, arriving at its upper doorway.</summary>
    public static DoorDefinition BigPinkToPinkBrinstarPowerBombsUpper { get; } = DoorDefinitions.Find(nameof(BigPinkToPinkBrinstarPowerBombsUpper));

    /// <summary>The door from the Big Pink room to the Waterway Energy Tank room.</summary>
    public static DoorDefinition BigPinkToWaterwayEnergyTank { get; } = DoorDefinitions.Find(nameof(BigPinkToWaterwayEnergyTank));

    /// <summary>The door from the Big Pink room to the Pink Brinstar Hoppers room.</summary>
    public static DoorDefinition BigPinkToPinkBrinstarHoppers { get; } = DoorDefinitions.Find(nameof(BigPinkToPinkBrinstarHoppers));

    /// <summary>The door from the Big Pink room to the Spore Spawn Farming room.</summary>
    public static DoorDefinition BigPinkToSporeSpawnFarming { get; } = DoorDefinitions.Find(nameof(BigPinkToSporeSpawnFarming));

    /// <summary>The door from the Spore Spawn Kihunters room to the Big Pink room.</summary>
    public static DoorDefinition SporeSpawnKihuntersToBigPink { get; } = DoorDefinitions.Find(nameof(SporeSpawnKihuntersToBigPink));

    /// <summary>The door from the Spore Spawn Kihunters room to the Spore Spawn room.</summary>
    public static DoorDefinition SporeSpawnKihuntersToSporeSpawn { get; } = DoorDefinitions.Find(nameof(SporeSpawnKihuntersToSporeSpawn));

    /// <summary>The door from the Spore Spawn room to the Spore Spawn Super room.</summary>
    public static DoorDefinition SporeSpawnToSporeSpawnSuper { get; } = DoorDefinitions.Find(nameof(SporeSpawnToSporeSpawnSuper));

    /// <summary>The door from the Spore Spawn room to the Spore Spawn Kihunters room.</summary>
    public static DoorDefinition SporeSpawnToSporeSpawnKihunters { get; } = DoorDefinitions.Find(nameof(SporeSpawnToSporeSpawnKihunters));

    /// <summary>The door from the Pink Brinstar Power Bombs room to the Big Pink room, arriving at its upper doorway.</summary>
    public static DoorDefinition PinkBrinstarPowerBombsToBigPinkUpper { get; } = DoorDefinitions.Find(nameof(PinkBrinstarPowerBombsToBigPinkUpper));

    /// <summary>The door from the Pink Brinstar Power Bombs room to the Big Pink room, arriving at its lower doorway.</summary>
    public static DoorDefinition PinkBrinstarPowerBombsToBigPinkLower { get; } = DoorDefinitions.Find(nameof(PinkBrinstarPowerBombsToBigPinkLower));

    /// <summary>The door from the Green Hill Zone room to the Big Pink room.</summary>
    public static DoorDefinition GreenHillZoneToBigPink { get; } = DoorDefinitions.Find(nameof(GreenHillZoneToBigPink));

    /// <summary>The door from the Green Hill Zone room to the Morph Ball room.</summary>
    public static DoorDefinition GreenHillZoneToMorphBall { get; } = DoorDefinitions.Find(nameof(GreenHillZoneToMorphBall));

    /// <summary>The door from the Green Hill Zone room to the Noob Bridge room.</summary>
    public static DoorDefinition GreenHillZoneToNoobBridge { get; } = DoorDefinitions.Find(nameof(GreenHillZoneToNoobBridge));

    /// <summary>The door from the Morph Ball room to the Green Hill Zone room.</summary>
    public static DoorDefinition MorphBallToGreenHillZone { get; } = DoorDefinitions.Find(nameof(MorphBallToGreenHillZone));

    /// <summary>The door from the Morph Ball room to the Construction Zone room.</summary>
    public static DoorDefinition MorphBallToConstructionZone { get; } = DoorDefinitions.Find(nameof(MorphBallToConstructionZone));

    /// <summary>The door from the Morph Ball room to the Elevator To Blue Brinstar room.</summary>
    public static DoorDefinition MorphBallToElevatorToBlueBrinstar { get; } = DoorDefinitions.Find(nameof(MorphBallToElevatorToBlueBrinstar));

    /// <summary>The door from the Construction Zone room to the Morph Ball room.</summary>
    public static DoorDefinition ConstructionZoneToMorphBall { get; } = DoorDefinitions.Find(nameof(ConstructionZoneToMorphBall));

    /// <summary>The door from the Construction Zone room to the Blue Brinstar Energy Tank room.</summary>
    public static DoorDefinition ConstructionZoneToBlueBrinstarEnergyTank { get; } = DoorDefinitions.Find(nameof(ConstructionZoneToBlueBrinstarEnergyTank));

    /// <summary>The door from the Construction Zone room to the First Missile room.</summary>
    public static DoorDefinition ConstructionZoneToFirstMissile { get; } = DoorDefinitions.Find(nameof(ConstructionZoneToFirstMissile));

    /// <summary>The door from the Blue Brinstar Energy Tank room to the Construction Zone room.</summary>
    public static DoorDefinition BlueBrinstarEnergyTankToConstructionZone { get; } = DoorDefinitions.Find(nameof(BlueBrinstarEnergyTankToConstructionZone));

    /// <summary>The door from the Blue Brinstar Energy Tank room to the Blue Brinstar Boulders room.</summary>
    public static DoorDefinition BlueBrinstarEnergyTankToBlueBrinstarBoulders { get; } = DoorDefinitions.Find(nameof(BlueBrinstarEnergyTankToBlueBrinstarBoulders));

    /// <summary>The door from the Noob Bridge room to the Green Hill Zone room.</summary>
    public static DoorDefinition NoobBridgeToGreenHillZone { get; } = DoorDefinitions.Find(nameof(NoobBridgeToGreenHillZone));

    /// <summary>The door from the Noob Bridge room to the Red Tower room.</summary>
    public static DoorDefinition NoobBridgeToRedTower { get; } = DoorDefinitions.Find(nameof(NoobBridgeToRedTower));

    /// <summary>The door from the Green Brinstar Beetoms room to the Green Brinstar Main Shaft room.</summary>
    public static DoorDefinition GreenBrinstarBeetomsToGreenBrinstarMainShaft { get; } = DoorDefinitions.Find(nameof(GreenBrinstarBeetomsToGreenBrinstarMainShaft));

    /// <summary>The door from the Green Brinstar Beetoms room to the Etecoon Energy Tank room.</summary>
    public static DoorDefinition GreenBrinstarBeetomsToEtecoonEnergyTank { get; } = DoorDefinitions.Find(nameof(GreenBrinstarBeetomsToEtecoonEnergyTank));

    /// <summary>The door from the Etecoon Energy Tank room to the Green Brinstar Beetoms room.</summary>
    public static DoorDefinition EtecoonEnergyTankToGreenBrinstarBeetoms { get; } = DoorDefinitions.Find(nameof(EtecoonEnergyTankToGreenBrinstarBeetoms));

    /// <summary>The door from the Etecoon Energy Tank room to the Etecoon Super room.</summary>
    public static DoorDefinition EtecoonEnergyTankToEtecoonSuper { get; } = DoorDefinitions.Find(nameof(EtecoonEnergyTankToEtecoonSuper));

    /// <summary>The door from the Etecoon Energy Tank room to the Green Brinstar Main Shaft room.</summary>
    public static DoorDefinition EtecoonEnergyTankToGreenBrinstarMainShaft { get; } = DoorDefinitions.Find(nameof(EtecoonEnergyTankToGreenBrinstarMainShaft));

    /// <summary>The door from the Etecoon Energy Tank room to the Etecoon Save room.</summary>
    public static DoorDefinition EtecoonEnergyTankToEtecoonSave { get; } = DoorDefinitions.Find(nameof(EtecoonEnergyTankToEtecoonSave));

    /// <summary>The door from the Etecoon Super room to the Etecoon Energy Tank room.</summary>
    public static DoorDefinition EtecoonSuperToEtecoonEnergyTank { get; } = DoorDefinitions.Find(nameof(EtecoonSuperToEtecoonEnergyTank));

    /// <summary>The door from the Dachora Energy Refill room to the Dachora room.</summary>
    public static DoorDefinition DachoraEnergyRefillToDachora { get; } = DoorDefinitions.Find(nameof(DachoraEnergyRefillToDachora));

    /// <summary>The door from the Spore Spawn Farming room to the Spore Spawn Super room.</summary>
    public static DoorDefinition SporeSpawnFarmingToSporeSpawnSuper { get; } = DoorDefinitions.Find(nameof(SporeSpawnFarmingToSporeSpawnSuper));

    /// <summary>The door from the Spore Spawn Farming room to the Big Pink room.</summary>
    public static DoorDefinition SporeSpawnFarmingToBigPink { get; } = DoorDefinitions.Find(nameof(SporeSpawnFarmingToBigPink));

    /// <summary>The door from the Waterway Energy Tank room to the Big Pink room.</summary>
    public static DoorDefinition WaterwayEnergyTankToBigPink { get; } = DoorDefinitions.Find(nameof(WaterwayEnergyTankToBigPink));

    /// <summary>The door from the Waterway Energy Tank room to the Kraid Refill room.</summary>
    public static DoorDefinition WaterwayEnergyTankToKraidRefill { get; } = DoorDefinitions.Find(nameof(WaterwayEnergyTankToKraidRefill));

    /// <summary>The door from the First Missile room to the Construction Zone room.</summary>
    public static DoorDefinition FirstMissileToConstructionZone { get; } = DoorDefinitions.Find(nameof(FirstMissileToConstructionZone));

    /// <summary>The door from the Pink Brinstar Hoppers room to the Big Pink room.</summary>
    public static DoorDefinition PinkBrinstarHoppersToBigPink { get; } = DoorDefinitions.Find(nameof(PinkBrinstarHoppersToBigPink));

    /// <summary>The door from the Pink Brinstar Hoppers room to the Hopper Energy Tank room.</summary>
    public static DoorDefinition PinkBrinstarHoppersToHopperEnergyTank { get; } = DoorDefinitions.Find(nameof(PinkBrinstarHoppersToHopperEnergyTank));

    /// <summary>The door from the Hopper Energy Tank room to the Pink Brinstar Hoppers room.</summary>
    public static DoorDefinition HopperEnergyTankToPinkBrinstarHoppers { get; } = DoorDefinitions.Find(nameof(HopperEnergyTankToPinkBrinstarHoppers));

    /// <summary>The door from the Big Pink Save Room room to the Big Pink room.</summary>
    public static DoorDefinition BigPinkSaveRoomToBigPink { get; } = DoorDefinitions.Find(nameof(BigPinkSaveRoomToBigPink));

    /// <summary>The door from the Blue Brinstar Boulders room to the Blue Brinstar Energy Tank room.</summary>
    public static DoorDefinition BlueBrinstarBouldersToBlueBrinstarEnergyTank { get; } = DoorDefinitions.Find(nameof(BlueBrinstarBouldersToBlueBrinstarEnergyTank));

    /// <summary>The door from the Blue Brinstar Boulders room to the Billy Mays room.</summary>
    public static DoorDefinition BlueBrinstarBouldersToBillyMays { get; } = DoorDefinitions.Find(nameof(BlueBrinstarBouldersToBillyMays));

    /// <summary>The door from the Billy Mays room to the Blue Brinstar Boulders room.</summary>
    public static DoorDefinition BillyMaysToBlueBrinstarBoulders { get; } = DoorDefinitions.Find(nameof(BillyMaysToBlueBrinstarBoulders));

    /// <summary>The door from the Green Brinstar Save room to the Green Brinstar Main Shaft room.</summary>
    public static DoorDefinition GreenBrinstarSaveToGreenBrinstarMainShaft { get; } = DoorDefinitions.Find(nameof(GreenBrinstarSaveToGreenBrinstarMainShaft));

    /// <summary>The door from the Etecoon Save room to the Etecoon Energy Tank room.</summary>
    public static DoorDefinition EtecoonSaveToEtecoonEnergyTank { get; } = DoorDefinitions.Find(nameof(EtecoonSaveToEtecoonEnergyTank));

    /// <summary>The door from the Red Tower room to the Hellway room.</summary>
    public static DoorDefinition RedTowerToHellway { get; } = DoorDefinitions.Find(nameof(RedTowerToHellway));

    /// <summary>The door from the Red Tower room to the Noob Bridge room.</summary>
    public static DoorDefinition RedTowerToNoobBridge { get; } = DoorDefinitions.Find(nameof(RedTowerToNoobBridge));

    /// <summary>The door from the Red Tower room to the Red Brinstar Firefleas room.</summary>
    public static DoorDefinition RedTowerToRedBrinstarFirefleas { get; } = DoorDefinitions.Find(nameof(RedTowerToRedBrinstarFirefleas));

    /// <summary>The door from the Red Tower room to the Skree Boost room.</summary>
    public static DoorDefinition RedTowerToSkreeBoost { get; } = DoorDefinitions.Find(nameof(RedTowerToSkreeBoost));

    /// <summary>The door from the Red Tower room to the Red Tower Refill room.</summary>
    public static DoorDefinition RedTowerToRedTowerRefill { get; } = DoorDefinitions.Find(nameof(RedTowerToRedTowerRefill));

    /// <summary>The door from the Red Brinstar Firefleas room to the Xray Scope room.</summary>
    public static DoorDefinition RedBrinstarFirefleasToXrayScope { get; } = DoorDefinitions.Find(nameof(RedBrinstarFirefleasToXrayScope));

    /// <summary>The door from the Red Brinstar Firefleas room to the Red Tower room.</summary>
    public static DoorDefinition RedBrinstarFirefleasToRedTower { get; } = DoorDefinitions.Find(nameof(RedBrinstarFirefleasToRedTower));

    /// <summary>The door from the Xray Scope room to the Red Brinstar Firefleas room.</summary>
    public static DoorDefinition XrayScopeToRedBrinstarFirefleas { get; } = DoorDefinitions.Find(nameof(XrayScopeToRedBrinstarFirefleas));

    /// <summary>The door from the Hellway room to the Red Tower room.</summary>
    public static DoorDefinition HellwayToRedTower { get; } = DoorDefinitions.Find(nameof(HellwayToRedTower));

    /// <summary>The door from the Hellway room to the Caterpillar room.</summary>
    public static DoorDefinition HellwayToCaterpillar { get; } = DoorDefinitions.Find(nameof(HellwayToCaterpillar));

    /// <summary>The door from the Caterpillar room to the Alpha Power Bomb room.</summary>
    public static DoorDefinition CaterpillarToAlphaPowerBomb { get; } = DoorDefinitions.Find(nameof(CaterpillarToAlphaPowerBomb));

    /// <summary>The door from the Caterpillar room to the Beta Power Bomb room.</summary>
    public static DoorDefinition CaterpillarToBetaPowerBomb { get; } = DoorDefinitions.Find(nameof(CaterpillarToBetaPowerBomb));

    /// <summary>The door from the Caterpillar room to the Hellway room.</summary>
    public static DoorDefinition CaterpillarToHellway { get; } = DoorDefinitions.Find(nameof(CaterpillarToHellway));

    /// <summary>The door from the Caterpillar room to the Red Brinstar Elevator room.</summary>
    public static DoorDefinition CaterpillarToRedBrinstarElevator { get; } = DoorDefinitions.Find(nameof(CaterpillarToRedBrinstarElevator));

    /// <summary>The door from the Caterpillar room to the Red Fish room.</summary>
    public static DoorDefinition CaterpillarToRedFish { get; } = DoorDefinitions.Find(nameof(CaterpillarToRedFish));

    /// <summary>The door from the Caterpillar room to the Red Brinstar Save room.</summary>
    public static DoorDefinition CaterpillarToRedBrinstarSave { get; } = DoorDefinitions.Find(nameof(CaterpillarToRedBrinstarSave));

    /// <summary>The door from the Beta Power Bomb room to the Caterpillar room.</summary>
    public static DoorDefinition BetaPowerBombToCaterpillar { get; } = DoorDefinitions.Find(nameof(BetaPowerBombToCaterpillar));

    /// <summary>The door from the Alpha Power Bomb room to the Caterpillar room.</summary>
    public static DoorDefinition AlphaPowerBombToCaterpillar { get; } = DoorDefinitions.Find(nameof(AlphaPowerBombToCaterpillar));

    /// <summary>The door from the Skree Boost room to the Red Tower room.</summary>
    public static DoorDefinition SkreeBoostToRedTower { get; } = DoorDefinitions.Find(nameof(SkreeBoostToRedTower));

    /// <summary>The door from the Skree Boost room to the Below Spazer room.</summary>
    public static DoorDefinition SkreeBoostToBelowSpazer { get; } = DoorDefinitions.Find(nameof(SkreeBoostToBelowSpazer));

    /// <summary>The door from the Below Spazer room to the Skree Boost room.</summary>
    public static DoorDefinition BelowSpazerToSkreeBoost { get; } = DoorDefinitions.Find(nameof(BelowSpazerToSkreeBoost));

    /// <summary>The door from the Below Spazer room to the West Tunnel room.</summary>
    public static DoorDefinition BelowSpazerToWestTunnel { get; } = DoorDefinitions.Find(nameof(BelowSpazerToWestTunnel));

    /// <summary>The door from the Below Spazer room to the Spazer room.</summary>
    public static DoorDefinition BelowSpazerToSpazer { get; } = DoorDefinitions.Find(nameof(BelowSpazerToSpazer));

    /// <summary>The door from the Spazer room to the Below Spazer room.</summary>
    public static DoorDefinition SpazerToBelowSpazer { get; } = DoorDefinitions.Find(nameof(SpazerToBelowSpazer));

    /// <summary>The door from the Warehouse Zeela room to the Warehouse Entrance room.</summary>
    public static DoorDefinition WarehouseZeelaToWarehouseEntrance { get; } = DoorDefinitions.Find(nameof(WarehouseZeelaToWarehouseEntrance));

    /// <summary>The door from the Warehouse Zeela room to the Warehouse Energy Tank room.</summary>
    public static DoorDefinition WarehouseZeelaToWarehouseEnergyTank { get; } = DoorDefinitions.Find(nameof(WarehouseZeelaToWarehouseEnergyTank));

    /// <summary>The door from the Warehouse Zeela room to the Warehouse Kihunter room.</summary>
    public static DoorDefinition WarehouseZeelaToWarehouseKihunter { get; } = DoorDefinitions.Find(nameof(WarehouseZeelaToWarehouseKihunter));

    /// <summary>The door from the Warehouse Energy Tank room to the Warehouse Zeela room.</summary>
    public static DoorDefinition WarehouseEnergyTankToWarehouseZeela { get; } = DoorDefinitions.Find(nameof(WarehouseEnergyTankToWarehouseZeela));

    /// <summary>The door from the Warehouse Kihunter room to the Warehouse Zeela room.</summary>
    public static DoorDefinition WarehouseKihunterToWarehouseZeela { get; } = DoorDefinitions.Find(nameof(WarehouseKihunterToWarehouseZeela));

    /// <summary>The door from the Warehouse Kihunter room to the Mini Kraid room.</summary>
    public static DoorDefinition WarehouseKihunterToMiniKraid { get; } = DoorDefinitions.Find(nameof(WarehouseKihunterToMiniKraid));

    /// <summary>The door from the Warehouse Kihunter room to the Warehouse Save room.</summary>
    public static DoorDefinition WarehouseKihunterToWarehouseSave { get; } = DoorDefinitions.Find(nameof(WarehouseKihunterToWarehouseSave));

    /// <summary>The door from the Mini Kraid room to the Warehouse Kihunter room.</summary>
    public static DoorDefinition MiniKraidToWarehouseKihunter { get; } = DoorDefinitions.Find(nameof(MiniKraidToWarehouseKihunter));

    /// <summary>The door from the Mini Kraid room to the Kraid Eye Door room.</summary>
    public static DoorDefinition MiniKraidToKraidEyeDoor { get; } = DoorDefinitions.Find(nameof(MiniKraidToKraidEyeDoor));

    /// <summary>The door from the Kraid Eye Door room to the Mini Kraid room.</summary>
    public static DoorDefinition KraidEyeDoorToMiniKraid { get; } = DoorDefinitions.Find(nameof(KraidEyeDoorToMiniKraid));

    /// <summary>The door from the Kraid Eye Door room to the Kraid room.</summary>
    public static DoorDefinition KraidEyeDoorToKraid { get; } = DoorDefinitions.Find(nameof(KraidEyeDoorToKraid));

    /// <summary>The door from the Kraid Eye Door room to the Kraid Refill room.</summary>
    public static DoorDefinition KraidEyeDoorToKraidRefill { get; } = DoorDefinitions.Find(nameof(KraidEyeDoorToKraidRefill));

    /// <summary>The door from the Kraid room to the Kraid Eye Door room.</summary>
    public static DoorDefinition KraidToKraidEyeDoor { get; } = DoorDefinitions.Find(nameof(KraidToKraidEyeDoor));

    /// <summary>The door from the Kraid room to the Varia Suit room.</summary>
    public static DoorDefinition KraidToVariaSuit { get; } = DoorDefinitions.Find(nameof(KraidToVariaSuit));

    /// <summary>The door from the Red Tower Refill room to the Red Tower room.</summary>
    public static DoorDefinition RedTowerRefillToRedTower { get; } = DoorDefinitions.Find(nameof(RedTowerRefillToRedTower));

    /// <summary>The door from the Kraid Refill room to the Kraid Eye Door room.</summary>
    public static DoorDefinition KraidRefillToKraidEyeDoor { get; } = DoorDefinitions.Find(nameof(KraidRefillToKraidEyeDoor));

    /// <summary>The door from the Warehouse Entrance room to the East Tunnel room.</summary>
    public static DoorDefinition WarehouseEntranceToEastTunnel { get; } = DoorDefinitions.Find(nameof(WarehouseEntranceToEastTunnel));

    /// <summary>The door from the Warehouse Entrance room to the Warehouse Zeela room.</summary>
    public static DoorDefinition WarehouseEntranceToWarehouseZeela { get; } = DoorDefinitions.Find(nameof(WarehouseEntranceToWarehouseZeela));

    /// <summary>The door from the Warehouse Entrance room to the Business Center room.</summary>
    public static DoorDefinition WarehouseEntranceToBusinessCenter { get; } = DoorDefinitions.Find(nameof(WarehouseEntranceToBusinessCenter));

    /// <summary>The door from the Varia Suit room to the Kraid room.</summary>
    public static DoorDefinition VariaSuitToKraid { get; } = DoorDefinitions.Find(nameof(VariaSuitToKraid));

    /// <summary>The door from the Warehouse Save room to the Warehouse Kihunter room.</summary>
    public static DoorDefinition WarehouseSaveToWarehouseKihunter { get; } = DoorDefinitions.Find(nameof(WarehouseSaveToWarehouseKihunter));

    /// <summary>The door from the Red Brinstar Save room to the Caterpillar room.</summary>
    public static DoorDefinition RedBrinstarSaveToCaterpillar { get; } = DoorDefinitions.Find(nameof(RedBrinstarSaveToCaterpillar));
}
