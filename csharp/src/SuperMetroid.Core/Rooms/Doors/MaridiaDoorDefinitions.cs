namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The Maridia rooms' doors, each named for the room it leads from and the room it leads to.</summary>
public static class MaridiaDoorDefinitions
{
    /// <summary>The door from the Glass Tunnel Save room to the Glass Tunnel room.</summary>
    public static DoorDefinition GlassTunnelSaveToGlassTunnel { get; } = DoorDefinitions.Find(nameof(GlassTunnelSaveToGlassTunnel));

    /// <summary>The door from the Glass Tunnel room to the Main Street room.</summary>
    public static DoorDefinition GlassTunnelToMainStreet { get; } = DoorDefinitions.Find(nameof(GlassTunnelToMainStreet));

    /// <summary>The door from the Glass Tunnel room to the West Tunnel room.</summary>
    public static DoorDefinition GlassTunnelToWestTunnel { get; } = DoorDefinitions.Find(nameof(GlassTunnelToWestTunnel));

    /// <summary>The door from the Glass Tunnel room to the East Tunnel room.</summary>
    public static DoorDefinition GlassTunnelToEastTunnel { get; } = DoorDefinitions.Find(nameof(GlassTunnelToEastTunnel));

    /// <summary>The door from the Glass Tunnel room to the Glass Tunnel Save room.</summary>
    public static DoorDefinition GlassTunnelToGlassTunnelSave { get; } = DoorDefinitions.Find(nameof(GlassTunnelToGlassTunnelSave));

    /// <summary>The door from the West Tunnel room to the Glass Tunnel room.</summary>
    public static DoorDefinition WestTunnelToGlassTunnel { get; } = DoorDefinitions.Find(nameof(WestTunnelToGlassTunnel));

    /// <summary>The door from the West Tunnel room to the Below Spazer room.</summary>
    public static DoorDefinition WestTunnelToBelowSpazer { get; } = DoorDefinitions.Find(nameof(WestTunnelToBelowSpazer));

    /// <summary>The door from the East Tunnel room to the Glass Tunnel room.</summary>
    public static DoorDefinition EastTunnelToGlassTunnel { get; } = DoorDefinitions.Find(nameof(EastTunnelToGlassTunnel));

    /// <summary>The door from the East Tunnel room to the Warehouse Entrance room.</summary>
    public static DoorDefinition EastTunnelToWarehouseEntrance { get; } = DoorDefinitions.Find(nameof(EastTunnelToWarehouseEntrance));

    /// <summary>The door from the East Tunnel room to the Crab Hole room.</summary>
    public static DoorDefinition EastTunnelToCrabHole { get; } = DoorDefinitions.Find(nameof(EastTunnelToCrabHole));

    /// <summary>The door from the Main Street room to the Glass Tunnel room.</summary>
    public static DoorDefinition MainStreetToGlassTunnel { get; } = DoorDefinitions.Find(nameof(MainStreetToGlassTunnel));

    /// <summary>The door from the Main Street room to the Crab Tunnel room.</summary>
    public static DoorDefinition MainStreetToCrabTunnel { get; } = DoorDefinitions.Find(nameof(MainStreetToCrabTunnel));

    /// <summary>The door from the Main Street room to the Fish Tank room.</summary>
    public static DoorDefinition MainStreetToFishTank { get; } = DoorDefinitions.Find(nameof(MainStreetToFishTank));

    /// <summary>The door from the Main Street room to the Mt Everest room, arriving at its upper doorway.</summary>
    public static DoorDefinition MainStreetToMtEverestUpper { get; } = DoorDefinitions.Find(nameof(MainStreetToMtEverestUpper));

    /// <summary>The door from the Main Street room to the Mt Everest room, arriving at its lower doorway.</summary>
    public static DoorDefinition MainStreetToMtEverestLower { get; } = DoorDefinitions.Find(nameof(MainStreetToMtEverestLower));

    /// <summary>The door from the Fish Tank room to the Main Street room.</summary>
    public static DoorDefinition FishTankToMainStreet { get; } = DoorDefinitions.Find(nameof(FishTankToMainStreet));

    /// <summary>The door from the Fish Tank room to the Mama Turtle room.</summary>
    public static DoorDefinition FishTankToMamaTurtle { get; } = DoorDefinitions.Find(nameof(FishTankToMamaTurtle));

    /// <summary>The door from the Fish Tank room to the Mt Everest room, arriving at its left shaft.</summary>
    public static DoorDefinition FishTankToMtEverestLeft { get; } = DoorDefinitions.Find(nameof(FishTankToMtEverestLeft));

    /// <summary>The door from the Fish Tank room to the Mt Everest room, arriving at its right shaft.</summary>
    public static DoorDefinition FishTankToMtEverestRight { get; } = DoorDefinitions.Find(nameof(FishTankToMtEverestRight));

    /// <summary>The door from the Mama Turtle room to the Fish Tank room.</summary>
    public static DoorDefinition MamaTurtleToFishTank { get; } = DoorDefinitions.Find(nameof(MamaTurtleToFishTank));

    /// <summary>The door from the Crab Tunnel room to the Main Street room.</summary>
    public static DoorDefinition CrabTunnelToMainStreet { get; } = DoorDefinitions.Find(nameof(CrabTunnelToMainStreet));

    /// <summary>The door from the Crab Tunnel room to the Crab Hole room.</summary>
    public static DoorDefinition CrabTunnelToCrabHole { get; } = DoorDefinitions.Find(nameof(CrabTunnelToCrabHole));

    /// <summary>The door from the Mt Everest room to the Red Fish room.</summary>
    public static DoorDefinition MtEverestToRedFish { get; } = DoorDefinitions.Find(nameof(MtEverestToRedFish));

    /// <summary>The door from the Mt Everest room to the Main Street room, arriving at its upper doorway.</summary>
    public static DoorDefinition MtEverestToMainStreetUpper { get; } = DoorDefinitions.Find(nameof(MtEverestToMainStreetUpper));

    /// <summary>The door from the Mt Everest room to the Fish Tank room, arriving at its left shaft.</summary>
    public static DoorDefinition MtEverestToFishTankLeft { get; } = DoorDefinitions.Find(nameof(MtEverestToFishTankLeft));

    /// <summary>The door from the Mt Everest room to the Fish Tank room, arriving at its right shaft.</summary>
    public static DoorDefinition MtEverestToFishTankRight { get; } = DoorDefinitions.Find(nameof(MtEverestToFishTankRight));

    /// <summary>The door from the Mt Everest room to the Main Street room, arriving at its lower doorway.</summary>
    public static DoorDefinition MtEverestToMainStreetLower { get; } = DoorDefinitions.Find(nameof(MtEverestToMainStreetLower));

    /// <summary>The door from the Mt Everest room to the Crab Shaft room.</summary>
    public static DoorDefinition MtEverestToCrabShaft { get; } = DoorDefinitions.Find(nameof(MtEverestToCrabShaft));

    /// <summary>The door from the Red Fish room to the Mt Everest room.</summary>
    public static DoorDefinition RedFishToMtEverest { get; } = DoorDefinitions.Find(nameof(RedFishToMtEverest));

    /// <summary>The door from the Red Fish room to the Caterpillar room.</summary>
    public static DoorDefinition RedFishToCaterpillar { get; } = DoorDefinitions.Find(nameof(RedFishToCaterpillar));

    /// <summary>The door from the Watering Hole room to the N West Maridia Bug room.</summary>
    public static DoorDefinition WateringHoleToNWestMaridiaBug { get; } = DoorDefinitions.Find(nameof(WateringHoleToNWestMaridiaBug));

    /// <summary>The door from the N West Maridia Bug room to the Watering Hole room.</summary>
    public static DoorDefinition NWestMaridiaBugToWateringHole { get; } = DoorDefinitions.Find(nameof(NWestMaridiaBugToWateringHole));

    /// <summary>The door from the N West Maridia Bug room to the Pseudo Plasma Spark room.</summary>
    public static DoorDefinition NWestMaridiaBugToPseudoPlasmaSpark { get; } = DoorDefinitions.Find(nameof(NWestMaridiaBugToPseudoPlasmaSpark));

    /// <summary>The door from the Crab Shaft room to the Mt Everest room.</summary>
    public static DoorDefinition CrabShaftToMtEverest { get; } = DoorDefinitions.Find(nameof(CrabShaftToMtEverest));

    /// <summary>The door from the Crab Shaft room to the Pseudo Plasma Spark room.</summary>
    public static DoorDefinition CrabShaftToPseudoPlasmaSpark { get; } = DoorDefinitions.Find(nameof(CrabShaftToPseudoPlasmaSpark));

    /// <summary>The door from the Crab Shaft room to the Aqueduct room.</summary>
    public static DoorDefinition CrabShaftToAqueduct { get; } = DoorDefinitions.Find(nameof(CrabShaftToAqueduct));

    /// <summary>The door from the Pseudo Plasma Spark room to the N West Maridia Bug room.</summary>
    public static DoorDefinition PseudoPlasmaSparkToNWestMaridiaBug { get; } = DoorDefinitions.Find(nameof(PseudoPlasmaSparkToNWestMaridiaBug));

    /// <summary>The door from the Pseudo Plasma Spark room to the Crab Shaft room.</summary>
    public static DoorDefinition PseudoPlasmaSparkToCrabShaft { get; } = DoorDefinitions.Find(nameof(PseudoPlasmaSparkToCrabShaft));

    /// <summary>The door from the Pseudo Plasma Spark room to the Plasma Spark room.</summary>
    public static DoorDefinition PseudoPlasmaSparkToPlasmaSpark { get; } = DoorDefinitions.Find(nameof(PseudoPlasmaSparkToPlasmaSpark));

    /// <summary>The door from the Crab Hole room to the Crab Tunnel room.</summary>
    public static DoorDefinition CrabHoleToCrabTunnel { get; } = DoorDefinitions.Find(nameof(CrabHoleToCrabTunnel));

    /// <summary>The door from the Crab Hole room to the West Sand Hall Tunnel room.</summary>
    public static DoorDefinition CrabHoleToWestSandHallTunnel { get; } = DoorDefinitions.Find(nameof(CrabHoleToWestSandHallTunnel));

    /// <summary>The door from the Crab Hole room to the East Tunnel room.</summary>
    public static DoorDefinition CrabHoleToEastTunnel { get; } = DoorDefinitions.Find(nameof(CrabHoleToEastTunnel));

    /// <summary>The door from the Crab Hole room to the Maridia Map room.</summary>
    public static DoorDefinition CrabHoleToMaridiaMap { get; } = DoorDefinitions.Find(nameof(CrabHoleToMaridiaMap));

    /// <summary>The door from the West Sand Hall Tunnel room to the Crab Hole room.</summary>
    public static DoorDefinition WestSandHallTunnelToCrabHole { get; } = DoorDefinitions.Find(nameof(WestSandHallTunnelToCrabHole));

    /// <summary>The door from the West Sand Hall Tunnel room to the West Sand Hall room.</summary>
    public static DoorDefinition WestSandHallTunnelToWestSandHall { get; } = DoorDefinitions.Find(nameof(WestSandHallTunnelToWestSandHall));

    /// <summary>The door from the Plasma Tutorial room to the Kassiuz room.</summary>
    public static DoorDefinition PlasmaTutorialToKassiuz { get; } = DoorDefinitions.Find(nameof(PlasmaTutorialToKassiuz));

    /// <summary>The door from the Plasma Tutorial room to the Plasma room.</summary>
    public static DoorDefinition PlasmaTutorialToPlasma { get; } = DoorDefinitions.Find(nameof(PlasmaTutorialToPlasma));

    /// <summary>The door from the Plasma room to the Plasma Tutorial room.</summary>
    public static DoorDefinition PlasmaToPlasmaTutorial { get; } = DoorDefinitions.Find(nameof(PlasmaToPlasmaTutorial));

    /// <summary>The door from the Thread The Needle room to the Bug Sand Hole room.</summary>
    public static DoorDefinition ThreadTheNeedleToBugSandHole { get; } = DoorDefinitions.Find(nameof(ThreadTheNeedleToBugSandHole));

    /// <summary>The door from the Thread The Needle room to the Maridia Elev room.</summary>
    public static DoorDefinition ThreadTheNeedleToMaridiaElev { get; } = DoorDefinitions.Find(nameof(ThreadTheNeedleToMaridiaElev));

    /// <summary>The door from the Maridia Elev room to the Thread The Needle room.</summary>
    public static DoorDefinition MaridiaElevToThreadTheNeedle { get; } = DoorDefinitions.Find(nameof(MaridiaElevToThreadTheNeedle));

    /// <summary>The door from the Maridia Elev room to the Forgotten Highway Save room.</summary>
    public static DoorDefinition MaridiaElevToForgottenHighwaySave { get; } = DoorDefinitions.Find(nameof(MaridiaElevToForgottenHighwaySave));

    /// <summary>The door from the Maridia Elev room to the Forgotten Highway Elev room.</summary>
    public static DoorDefinition MaridiaElevToForgottenHighwayElev { get; } = DoorDefinitions.Find(nameof(MaridiaElevToForgottenHighwayElev));

    /// <summary>The door from the Plasma Spark room to the Butterfly room.</summary>
    public static DoorDefinition PlasmaSparkToButterfly { get; } = DoorDefinitions.Find(nameof(PlasmaSparkToButterfly));

    /// <summary>The door from the Plasma Spark room to the Toilet room.</summary>
    public static DoorDefinition PlasmaSparkToToilet { get; } = DoorDefinitions.Find(nameof(PlasmaSparkToToilet));

    /// <summary>The door from the Plasma Spark room to the Kassiuz room.</summary>
    public static DoorDefinition PlasmaSparkToKassiuz { get; } = DoorDefinitions.Find(nameof(PlasmaSparkToKassiuz));

    /// <summary>The door from the Plasma Spark room to the Bug Sand Hole room.</summary>
    public static DoorDefinition PlasmaSparkToBugSandHole { get; } = DoorDefinitions.Find(nameof(PlasmaSparkToBugSandHole));

    /// <summary>The door from the Kassiuz room to the Plasma Spark room.</summary>
    public static DoorDefinition KassiuzToPlasmaSpark { get; } = DoorDefinitions.Find(nameof(KassiuzToPlasmaSpark));

    /// <summary>The door from the Kassiuz room to the Plasma Tutorial room.</summary>
    public static DoorDefinition KassiuzToPlasmaTutorial { get; } = DoorDefinitions.Find(nameof(KassiuzToPlasmaTutorial));

    /// <summary>The door from the Maridia Map room to the Crab Hole room.</summary>
    public static DoorDefinition MaridiaMapToCrabHole { get; } = DoorDefinitions.Find(nameof(MaridiaMapToCrabHole));

    /// <summary>The door from the Forgotten Highway Save room to the Maridia Elev room.</summary>
    public static DoorDefinition ForgottenHighwaySaveToMaridiaElev { get; } = DoorDefinitions.Find(nameof(ForgottenHighwaySaveToMaridiaElev));

    /// <summary>The door from the Toilet room to the Oasis room.</summary>
    public static DoorDefinition ToiletToOasis { get; } = DoorDefinitions.Find(nameof(ToiletToOasis));

    /// <summary>The door from the Toilet room to the Plasma Spark room.</summary>
    public static DoorDefinition ToiletToPlasmaSpark { get; } = DoorDefinitions.Find(nameof(ToiletToPlasmaSpark));

    /// <summary>The door from the Bug Sand Hole room to the Thread The Needle room.</summary>
    public static DoorDefinition BugSandHoleToThreadTheNeedle { get; } = DoorDefinitions.Find(nameof(BugSandHoleToThreadTheNeedle));

    /// <summary>The door from the Bug Sand Hole room to the Plasma Beach Quicksand room.</summary>
    public static DoorDefinition BugSandHoleToPlasmaBeachQuicksand { get; } = DoorDefinitions.Find(nameof(BugSandHoleToPlasmaBeachQuicksand));

    /// <summary>The door from the Bug Sand Hole room to the Plasma Spark room.</summary>
    public static DoorDefinition BugSandHoleToPlasmaSpark { get; } = DoorDefinitions.Find(nameof(BugSandHoleToPlasmaSpark));

    /// <summary>The door from the West Sand Hall room to the West Sand Hall Tunnel room.</summary>
    public static DoorDefinition WestSandHallToWestSandHallTunnel { get; } = DoorDefinitions.Find(nameof(WestSandHallToWestSandHallTunnel));

    /// <summary>The door from the West Sand Hall room to the Oasis room.</summary>
    public static DoorDefinition WestSandHallToOasis { get; } = DoorDefinitions.Find(nameof(WestSandHallToOasis));

    /// <summary>The door from the Oasis room to the West Sand Hall room.</summary>
    public static DoorDefinition OasisToWestSandHall { get; } = DoorDefinitions.Find(nameof(OasisToWestSandHall));

    /// <summary>The door from the Oasis room to the East Sand Hall room.</summary>
    public static DoorDefinition OasisToEastSandHall { get; } = DoorDefinitions.Find(nameof(OasisToEastSandHall));

    /// <summary>The door from the Oasis room to the Toilet room.</summary>
    public static DoorDefinition OasisToToilet { get; } = DoorDefinitions.Find(nameof(OasisToToilet));

    /// <summary>The door from the East Sand Hall room to the Oasis room.</summary>
    public static DoorDefinition EastSandHallToOasis { get; } = DoorDefinitions.Find(nameof(EastSandHallToOasis));

    /// <summary>The door from the East Sand Hall room to the Pants room.</summary>
    public static DoorDefinition EastSandHallToPants { get; } = DoorDefinitions.Find(nameof(EastSandHallToPants));

    /// <summary>The door from the West Sand Hole room to the West Aqueduct Quicksand room.</summary>
    public static DoorDefinition WestSandHoleToWestAqueductQuicksand { get; } = DoorDefinitions.Find(nameof(WestSandHoleToWestAqueductQuicksand));

    /// <summary>The door from the West Sand Hole room to the West Sand Hall room.</summary>
    public static DoorDefinition WestSandHoleToWestSandHall { get; } = DoorDefinitions.Find(nameof(WestSandHoleToWestSandHall));

    /// <summary>The door from the East Sand Hole room to the East Aqueduct Quicksand room.</summary>
    public static DoorDefinition EastSandHoleToEastAqueductQuicksand { get; } = DoorDefinitions.Find(nameof(EastSandHoleToEastAqueductQuicksand));

    /// <summary>The door from the East Sand Hole room to the East Sand Hall room.</summary>
    public static DoorDefinition EastSandHoleToEastSandHall { get; } = DoorDefinitions.Find(nameof(EastSandHoleToEastSandHall));

    /// <summary>The door from the West Aqueduct Quicksand room to the Aqueduct room.</summary>
    public static DoorDefinition WestAqueductQuicksandToAqueduct { get; } = DoorDefinitions.Find(nameof(WestAqueductQuicksandToAqueduct));

    /// <summary>The door from the West Aqueduct Quicksand room to the West Sand Hole room.</summary>
    public static DoorDefinition WestAqueductQuicksandToWestSandHole { get; } = DoorDefinitions.Find(nameof(WestAqueductQuicksandToWestSandHole));

    /// <summary>The door from the East Aqueduct Quicksand room to the Aqueduct room.</summary>
    public static DoorDefinition EastAqueductQuicksandToAqueduct { get; } = DoorDefinitions.Find(nameof(EastAqueductQuicksandToAqueduct));

    /// <summary>The door from the East Aqueduct Quicksand room to the East Sand Hole room.</summary>
    public static DoorDefinition EastAqueductQuicksandToEastSandHole { get; } = DoorDefinitions.Find(nameof(EastAqueductQuicksandToEastSandHole));

    /// <summary>The door from the Aqueduct room to the Crab Shaft room.</summary>
    public static DoorDefinition AqueductToCrabShaft { get; } = DoorDefinitions.Find(nameof(AqueductToCrabShaft));

    /// <summary>The door from the Aqueduct room to the West Aqueduct Quicksand room.</summary>
    public static DoorDefinition AqueductToWestAqueductQuicksand { get; } = DoorDefinitions.Find(nameof(AqueductToWestAqueductQuicksand));

    /// <summary>The door from the Aqueduct room to the East Aqueduct Quicksand room.</summary>
    public static DoorDefinition AqueductToEastAqueductQuicksand { get; } = DoorDefinitions.Find(nameof(AqueductToEastAqueductQuicksand));

    /// <summary>The door from the Aqueduct room to the Botwoon Hallway room.</summary>
    public static DoorDefinition AqueductToBotwoonHallway { get; } = DoorDefinitions.Find(nameof(AqueductToBotwoonHallway));

    /// <summary>The door from the Aqueduct room to the Below Botwoon E Tank room.</summary>
    public static DoorDefinition AqueductToBelowBotwoonETank { get; } = DoorDefinitions.Find(nameof(AqueductToBelowBotwoonETank));

    /// <summary>The door from the Aqueduct room to the Aqueduct Save room.</summary>
    public static DoorDefinition AqueductToAqueductSave { get; } = DoorDefinitions.Find(nameof(AqueductToAqueductSave));

    /// <summary>The door from the Butterfly room to the Plasma Spark room.</summary>
    public static DoorDefinition ButterflyToPlasmaSpark { get; } = DoorDefinitions.Find(nameof(ButterflyToPlasmaSpark));

    /// <summary>The door from the Butterfly room to the West Cactus Alley room.</summary>
    public static DoorDefinition ButterflyToWestCactusAlley { get; } = DoorDefinitions.Find(nameof(ButterflyToWestCactusAlley));

    /// <summary>The door from the Botwoon Hallway room to the Aqueduct room.</summary>
    public static DoorDefinition BotwoonHallwayToAqueduct { get; } = DoorDefinitions.Find(nameof(BotwoonHallwayToAqueduct));

    /// <summary>The door from the Botwoon Hallway room to the Botwoon room.</summary>
    public static DoorDefinition BotwoonHallwayToBotwoon { get; } = DoorDefinitions.Find(nameof(BotwoonHallwayToBotwoon));

    /// <summary>The door from the Pants room to the East Sand Hall room.</summary>
    public static DoorDefinition PantsToEastSandHall { get; } = DoorDefinitions.Find(nameof(PantsToEastSandHall));

    /// <summary>The door from the Pants room to the East Pants room.</summary>
    public static DoorDefinition PantsToEastPants { get; } = DoorDefinitions.Find(nameof(PantsToEastPants));

    /// <summary>The door from the Pants room to the Shaktool room.</summary>
    public static DoorDefinition PantsToShaktool { get; } = DoorDefinitions.Find(nameof(PantsToShaktool));

    /// <summary>The door from the Pants room to the Pants room.</summary>
    public static DoorDefinition PantsToPants { get; } = DoorDefinitions.Find(nameof(PantsToPants));

    /// <summary>The door from the East Pants room to the Pants room.</summary>
    public static DoorDefinition EastPantsToPants { get; } = DoorDefinitions.Find(nameof(EastPantsToPants));

    /// <summary>The door from the East Pants room to the Shaktool room.</summary>
    public static DoorDefinition EastPantsToShaktool { get; } = DoorDefinitions.Find(nameof(EastPantsToShaktool));

    /// <summary>The door from the Springball room to the Shaktool room.</summary>
    public static DoorDefinition SpringballToShaktool { get; } = DoorDefinitions.Find(nameof(SpringballToShaktool));

    /// <summary>The door from the Below Botwoon E Tank room to the Aqueduct room.</summary>
    public static DoorDefinition BelowBotwoonETankToAqueduct { get; } = DoorDefinitions.Find(nameof(BelowBotwoonETankToAqueduct));

    /// <summary>The door from the Colosseum room to the Halfie Climb room.</summary>
    public static DoorDefinition ColosseumToHalfieClimb { get; } = DoorDefinitions.Find(nameof(ColosseumToHalfieClimb));

    /// <summary>The door from the Colosseum room to the Draygon Save room.</summary>
    public static DoorDefinition ColosseumToDraygonSave { get; } = DoorDefinitions.Find(nameof(ColosseumToDraygonSave));

    /// <summary>The door from the Colosseum room to the The Precious room.</summary>
    public static DoorDefinition ColosseumToThePrecious { get; } = DoorDefinitions.Find(nameof(ColosseumToThePrecious));

    /// <summary>The door into the Precious room a Maridia load station enters through.</summary>
    public static DoorDefinition ThePreciousLoadStationEntry { get; } = DoorDefinitions.Find(nameof(ThePreciousLoadStationEntry));

    /// <summary>The door from the Aqueduct Save room to the Aqueduct room.</summary>
    public static DoorDefinition AqueductSaveToAqueduct { get; } = DoorDefinitions.Find(nameof(AqueductSaveToAqueduct));

    /// <summary>The door from the The Precious room to the Colosseum room.</summary>
    public static DoorDefinition ThePreciousToColosseum { get; } = DoorDefinitions.Find(nameof(ThePreciousToColosseum));

    /// <summary>The door from the The Precious room to the Draygon room.</summary>
    public static DoorDefinition ThePreciousToDraygon { get; } = DoorDefinitions.Find(nameof(ThePreciousToDraygon));

    /// <summary>The door from the Botwoon E Tank room to the Botwoon room.</summary>
    public static DoorDefinition BotwoonETankToBotwoon { get; } = DoorDefinitions.Find(nameof(BotwoonETankToBotwoon));

    /// <summary>The door from the Botwoon E Tank room to the Botwoon Quicksand room, arriving at its right half.</summary>
    public static DoorDefinition BotwoonETankToBotwoonQuicksandRight { get; } = DoorDefinitions.Find(nameof(BotwoonETankToBotwoonQuicksandRight));

    /// <summary>The door from the Botwoon E Tank room to the Botwoon Quicksand room, arriving at its left half.</summary>
    public static DoorDefinition BotwoonETankToBotwoonQuicksandLeft { get; } = DoorDefinitions.Find(nameof(BotwoonETankToBotwoonQuicksandLeft));

    /// <summary>The door from the Botwoon E Tank room to the Halfie Climb room.</summary>
    public static DoorDefinition BotwoonETankToHalfieClimb { get; } = DoorDefinitions.Find(nameof(BotwoonETankToHalfieClimb));

    /// <summary>The door from the Draygon Save room to the Maridia Energy Refill room.</summary>
    public static DoorDefinition DraygonSaveToMaridiaEnergyRefill { get; } = DoorDefinitions.Find(nameof(DraygonSaveToMaridiaEnergyRefill));

    /// <summary>The door from the Draygon Save room to the Colosseum room.</summary>
    public static DoorDefinition DraygonSaveToColosseum { get; } = DoorDefinitions.Find(nameof(DraygonSaveToColosseum));

    /// <summary>The door from the Maridia Missile Refill room to the Halfie Climb room.</summary>
    public static DoorDefinition MaridiaMissileRefillToHalfieClimb { get; } = DoorDefinitions.Find(nameof(MaridiaMissileRefillToHalfieClimb));

    /// <summary>The door from the Plasma Beach Quicksand room to the Butterfly room.</summary>
    public static DoorDefinition PlasmaBeachQuicksandToButterfly { get; } = DoorDefinitions.Find(nameof(PlasmaBeachQuicksandToButterfly));

    /// <summary>The door from the Botwoon Quicksand room to the Below Botwoon E Tank room, arriving at its left half.</summary>
    public static DoorDefinition BotwoonQuicksandToBelowBotwoonETankLeft { get; } = DoorDefinitions.Find(nameof(BotwoonQuicksandToBelowBotwoonETankLeft));

    /// <summary>The door from the Botwoon Quicksand room to the Below Botwoon E Tank room, arriving at its right half.</summary>
    public static DoorDefinition BotwoonQuicksandToBelowBotwoonETankRight { get; } = DoorDefinitions.Find(nameof(BotwoonQuicksandToBelowBotwoonETankRight));

    /// <summary>The door from the Shaktool room to the East Pants room.</summary>
    public static DoorDefinition ShaktoolToEastPants { get; } = DoorDefinitions.Find(nameof(ShaktoolToEastPants));

    /// <summary>The door from the Shaktool room to the Springball room.</summary>
    public static DoorDefinition ShaktoolToSpringball { get; } = DoorDefinitions.Find(nameof(ShaktoolToSpringball));

    /// <summary>The door from the Halfie Climb room to the Botwoon E Tank room.</summary>
    public static DoorDefinition HalfieClimbToBotwoonETank { get; } = DoorDefinitions.Find(nameof(HalfieClimbToBotwoonETank));

    /// <summary>The door from the Halfie Climb room to the Colosseum room.</summary>
    public static DoorDefinition HalfieClimbToColosseum { get; } = DoorDefinitions.Find(nameof(HalfieClimbToColosseum));

    /// <summary>The door from the Halfie Climb room to the Maridia Missile Refill room.</summary>
    public static DoorDefinition HalfieClimbToMaridiaMissileRefill { get; } = DoorDefinitions.Find(nameof(HalfieClimbToMaridiaMissileRefill));

    /// <summary>The door from the Halfie Climb room to the East Cactus Alley room.</summary>
    public static DoorDefinition HalfieClimbToEastCactusAlley { get; } = DoorDefinitions.Find(nameof(HalfieClimbToEastCactusAlley));

    /// <summary>The door from the Botwoon room to the Botwoon Hallway room.</summary>
    public static DoorDefinition BotwoonToBotwoonHallway { get; } = DoorDefinitions.Find(nameof(BotwoonToBotwoonHallway));

    /// <summary>The door from the Botwoon room to the Botwoon E Tank room.</summary>
    public static DoorDefinition BotwoonToBotwoonETank { get; } = DoorDefinitions.Find(nameof(BotwoonToBotwoonETank));

    /// <summary>The door from the Space Jump room to the Draygon room.</summary>
    public static DoorDefinition SpaceJumpToDraygon { get; } = DoorDefinitions.Find(nameof(SpaceJumpToDraygon));

    /// <summary>The door from the Maridia Energy Refill room to the Draygon Save room.</summary>
    public static DoorDefinition MaridiaEnergyRefillToDraygonSave { get; } = DoorDefinitions.Find(nameof(MaridiaEnergyRefillToDraygonSave));

    /// <summary>The door from the West Cactus Alley room to the Butterfly room.</summary>
    public static DoorDefinition WestCactusAlleyToButterfly { get; } = DoorDefinitions.Find(nameof(WestCactusAlleyToButterfly));

    /// <summary>The door from the West Cactus Alley room to the East Cactus Alley room.</summary>
    public static DoorDefinition WestCactusAlleyToEastCactusAlley { get; } = DoorDefinitions.Find(nameof(WestCactusAlleyToEastCactusAlley));

    /// <summary>The door from the East Cactus Alley room to the West Cactus Alley room.</summary>
    public static DoorDefinition EastCactusAlleyToWestCactusAlley { get; } = DoorDefinitions.Find(nameof(EastCactusAlleyToWestCactusAlley));

    /// <summary>The door from the East Cactus Alley room to the Halfie Climb room.</summary>
    public static DoorDefinition EastCactusAlleyToHalfieClimb { get; } = DoorDefinitions.Find(nameof(EastCactusAlleyToHalfieClimb));

    /// <summary>The door from the Draygon room to the The Precious room.</summary>
    public static DoorDefinition DraygonToThePrecious { get; } = DoorDefinitions.Find(nameof(DraygonToThePrecious));

    /// <summary>The door from the Draygon room to the Space Jump room.</summary>
    public static DoorDefinition DraygonToSpaceJump { get; } = DoorDefinitions.Find(nameof(DraygonToSpaceJump));
}
