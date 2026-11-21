namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The Maridia rooms' doors, each named for the room it leads from and the room it leads to.</summary>
public static partial class MaridiaDoorDefinitions
{
    /// <summary>The door from the Glass Tunnel Save room to the Glass Tunnel room.</summary>
    public static DoorDefinition GlassTunnelSaveToGlassTunnel { get; } = new("GlassTunnelSaveToGlassTunnel", RoomDefinitions.GlassTunnel, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 38), new(0, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Glass Tunnel room to the Main Street room.</summary>
    public static DoorDefinition GlassTunnelToMainStreet { get; } = new("GlassTunnelToMainStreet", RoomDefinitions.MainStreet, new(DoorDirection.Up, DoorClosingBehavior.BlueDoorCloses), new(22, 125), new(1, 7), DoorSamusSpeed.PixelsPerStep(2), DoorSetupDefinitions.None);

    /// <summary>The door from the Glass Tunnel room to the West Tunnel room.</summary>
    public static DoorDefinition GlassTunnelToWestTunnel { get; } = new("GlassTunnelToWestTunnel", RoomDefinitions.WestTunnel, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Glass Tunnel room to the East Tunnel room.</summary>
    public static DoorDefinition GlassTunnelToEastTunnel { get; } = new("GlassTunnelToEastTunnel", RoomDefinitions.EastTunnel, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.EastTunnelClosesScreen0x0OpensScreen0x1TopAligned);

    /// <summary>The door from the Glass Tunnel room to the Glass Tunnel Save room.</summary>
    public static DoorDefinition GlassTunnelToGlassTunnelSave { get; } = new("GlassTunnelToGlassTunnelSave", RoomDefinitions.GlassTunnelSave, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the West Tunnel room to the Glass Tunnel room.</summary>
    public static DoorDefinition WestTunnelToGlassTunnel { get; } = new("WestTunnelToGlassTunnel", RoomDefinitions.GlassTunnel, new(DoorDirection.Right, DoorClosingBehavior.None), null, new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.GlassTunnelClosesScreens0x0And0x2OpensScreen0x1TopAligned);

    /// <summary>The door from the West Tunnel room to the Below Spazer room.</summary>
    public static DoorDefinition WestTunnelToBelowSpazer { get; } = new("WestTunnelToBelowSpazer", RoomDefinitions.BelowSpazer, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 22), new(1, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None) { ExchangesExploredMaps = true };

    /// <summary>The door from the East Tunnel room to the Glass Tunnel room.</summary>
    public static DoorDefinition EastTunnelToGlassTunnel { get; } = new("EastTunnelToGlassTunnel", RoomDefinitions.GlassTunnel, new(DoorDirection.Left, DoorClosingBehavior.None), null, new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.GlassTunnelClosesScreens0x0And0x2OpensScreen0x1TopAligned);

    /// <summary>The door from the East Tunnel room to the Warehouse Entrance room.</summary>
    public static DoorDefinition EastTunnelToWarehouseEntrance { get; } = new("EastTunnelToWarehouseEntrance", RoomDefinitions.WarehouseEntrance, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None) { ExchangesExploredMaps = true };

    /// <summary>The door from the East Tunnel room to the Crab Hole room.</summary>
    public static DoorDefinition EastTunnelToCrabHole { get; } = new("EastTunnelToCrabHole", RoomDefinitions.CrabHole, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.CrabHoleClosesScreen0x0OpensScreen0x1TopAligned);

    /// <summary>The door from the Main Street room to the Glass Tunnel room.</summary>
    public static DoorDefinition MainStreetToGlassTunnel { get; } = new("MainStreetToGlassTunnel", RoomDefinitions.GlassTunnel, new(DoorDirection.Down, DoorClosingBehavior.BlueDoorCloses), new(6, 2), new(0, 0), DoorSamusSpeed.PixelsPerStep(1, 7, 16), DoorSetupDefinitions.None);

    /// <summary>The door from the Main Street room to the Crab Tunnel room.</summary>
    public static DoorDefinition MainStreetToCrabTunnel { get; } = new("MainStreetToCrabTunnel", RoomDefinitions.CrabTunnel, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Main Street room to the Fish Tank room.</summary>
    public static DoorDefinition MainStreetToFishTank { get; } = new("MainStreetToFishTank", RoomDefinitions.FishTank, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 38), new(0, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Main Street room to the Mt Everest room, arriving at its upper doorway.</summary>
    public static DoorDefinition MainStreetToMtEverestUpper { get; } = new("MainStreetToMtEverestUpper", RoomDefinitions.MtEverest, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Main Street room to the Mt Everest room, arriving at its lower doorway.</summary>
    public static DoorDefinition MainStreetToMtEverestLower { get; } = new("MainStreetToMtEverestLower", RoomDefinitions.MtEverest, new(DoorDirection.Right, DoorClosingBehavior.None), null, new(1, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Fish Tank room to the Main Street room.</summary>
    public static DoorDefinition FishTankToMainStreet { get; } = new("FishTankToMainStreet", RoomDefinitions.MainStreet, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(46, 102), new(2, 6), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Fish Tank room to the Mama Turtle room.</summary>
    public static DoorDefinition FishTankToMamaTurtle { get; } = new("FishTankToMamaTurtle", RoomDefinitions.MamaTurtle, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 54), new(0, 3), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Fish Tank room to the Mt Everest room, arriving at its left shaft.</summary>
    public static DoorDefinition FishTankToMtEverestLeft { get; } = new("FishTankToMtEverestLeft", RoomDefinitions.MtEverest, new(DoorDirection.Up, DoorClosingBehavior.BlueDoorCloses), new(22, 61), new(1, 3), DoorSamusSpeed.PixelsPerStep(1, 3, 4), DoorSetupDefinitions.None);

    /// <summary>The door from the Fish Tank room to the Mt Everest room, arriving at its right shaft.</summary>
    public static DoorDefinition FishTankToMtEverestRight { get; } = new("FishTankToMtEverestRight", RoomDefinitions.MtEverest, new(DoorDirection.Up, DoorClosingBehavior.BlueDoorCloses), new(70, 61), new(4, 3), DoorSamusSpeed.PixelsPerStep(1, 3, 4), DoorSetupDefinitions.None);

    /// <summary>The door from the Mama Turtle room to the Fish Tank room.</summary>
    public static DoorDefinition MamaTurtleToFishTank { get; } = new("MamaTurtleToFishTank", RoomDefinitions.FishTank, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(62, 38), new(3, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Crab Tunnel room to the Main Street room.</summary>
    public static DoorDefinition CrabTunnelToMainStreet { get; } = new("CrabTunnelToMainStreet", RoomDefinitions.MainStreet, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 118), new(1, 7), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Crab Tunnel room to the Crab Hole room.</summary>
    public static DoorDefinition CrabTunnelToCrabHole { get; } = new("CrabTunnelToCrabHole", RoomDefinitions.CrabHole, new(DoorDirection.Right, DoorClosingBehavior.None), null, new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Mt Everest room to the Red Fish room.</summary>
    public static DoorDefinition MtEverestToRedFish { get; } = new("MtEverestToRedFish", RoomDefinitions.RedFish, new(DoorDirection.Up, DoorClosingBehavior.BlueDoorCloses), new(38, 29), new(2, 1), DoorSamusSpeed.PixelsPerStep(1, 3, 4), DoorSetupDefinitions.None);

    /// <summary>The door from the Mt Everest room to the Main Street room, arriving at its upper doorway.</summary>
    public static DoorDefinition MtEverestToMainStreetUpper { get; } = new("MtEverestToMainStreetUpper", RoomDefinitions.MainStreet, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 6), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Mt Everest room to the Fish Tank room, arriving at its left shaft.</summary>
    public static DoorDefinition MtEverestToFishTankLeft { get; } = new("MtEverestToFishTankLeft", RoomDefinitions.FishTank, new(DoorDirection.Down, DoorClosingBehavior.BlueDoorCloses), new(6, 2), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Mt Everest room to the Fish Tank room, arriving at its right shaft.</summary>
    public static DoorDefinition MtEverestToFishTankRight { get; } = new("MtEverestToFishTankRight", RoomDefinitions.FishTank, new(DoorDirection.Down, DoorClosingBehavior.BlueDoorCloses), new(54, 2), new(3, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Mt Everest room to the Main Street room, arriving at its lower doorway.</summary>
    public static DoorDefinition MtEverestToMainStreetLower { get; } = new("MtEverestToMainStreetLower", RoomDefinitions.MainStreet, new(DoorDirection.Left, DoorClosingBehavior.None), null, new(2, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Mt Everest room to the Crab Shaft room.</summary>
    public static DoorDefinition MtEverestToCrabShaft { get; } = new("MtEverestToCrabShaft", RoomDefinitions.CrabShaft, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 38), new(0, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Red Fish room to the Mt Everest room.</summary>
    public static DoorDefinition RedFishToMtEverest { get; } = new("RedFishToMtEverest", RoomDefinitions.MtEverest, new(DoorDirection.Down, DoorClosingBehavior.BlueDoorCloses), new(38, 2), new(2, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Red Fish room to the Caterpillar room.</summary>
    public static DoorDefinition RedFishToCaterpillar { get; } = new("RedFishToCaterpillar", RoomDefinitions.Caterpillar, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(46, 54), new(2, 3), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.CaterpillarClosesScreen0x3OpensScreen1x3TopAligned) { ExchangesExploredMaps = true };

    /// <summary>The door from the Watering Hole room to the Northwest Maridia Bug room.</summary>
    public static DoorDefinition WateringHoleToNorthwestMaridiaBug { get; } = new("WateringHoleToNorthwestMaridiaBug", RoomDefinitions.NorthwestMaridiaBug, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.PixelsPerStep(1, 1, 4), DoorSetupDefinitions.None);

    /// <summary>The door from the Northwest Maridia Bug room to the Watering Hole room.</summary>
    public static DoorDefinition NorthwestMaridiaBugToWateringHole { get; } = new("NorthwestMaridiaBugToWateringHole", RoomDefinitions.WateringHole, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 6), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Northwest Maridia Bug room to the Pseudo Plasma Spark room.</summary>
    public static DoorDefinition NorthwestMaridiaBugToPseudoPlasmaSpark { get; } = new("NorthwestMaridiaBugToPseudoPlasmaSpark", RoomDefinitions.PseudoPlasmaSpark, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Crab Shaft room to the Mt Everest room.</summary>
    public static DoorDefinition CrabShaftToMtEverest { get; } = new("CrabShaftToMtEverest", RoomDefinitions.MtEverest, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(94, 6), new(5, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Crab Shaft room to the Pseudo Plasma Spark room.</summary>
    public static DoorDefinition CrabShaftToPseudoPlasmaSpark { get; } = new("CrabShaftToPseudoPlasmaSpark", RoomDefinitions.PseudoPlasmaSpark, new(DoorDirection.Up, DoorClosingBehavior.BlueDoorCloses), new(6, 45), new(0, 2), DoorSamusSpeed.PixelsPerStep(1, 3, 4), DoorSetupDefinitions.None);

    /// <summary>The door from the Crab Shaft room to the Aqueduct room.</summary>
    public static DoorDefinition CrabShaftToAqueduct { get; } = new("CrabShaftToAqueduct", RoomDefinitions.Aqueduct, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Pseudo Plasma Spark room to the Northwest Maridia Bug room.</summary>
    public static DoorDefinition PseudoPlasmaSparkToNorthwestMaridiaBug { get; } = new("PseudoPlasmaSparkToNorthwestMaridiaBug", RoomDefinitions.NorthwestMaridiaBug, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(62, 22), new(3, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Pseudo Plasma Spark room to the Crab Shaft room.</summary>
    public static DoorDefinition PseudoPlasmaSparkToCrabShaft { get; } = new("PseudoPlasmaSparkToCrabShaft", RoomDefinitions.CrabShaft, new(DoorDirection.Down, DoorClosingBehavior.BlueDoorCloses), new(6, 2), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Pseudo Plasma Spark room to the Plasma Spark room.</summary>
    public static DoorDefinition PseudoPlasmaSparkToPlasmaSpark { get; } = new("PseudoPlasmaSparkToPlasmaSpark", RoomDefinitions.PlasmaSpark, new(DoorDirection.Right, DoorClosingBehavior.None), null, new(0, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Crab Hole room to the Crab Tunnel room.</summary>
    public static DoorDefinition CrabHoleToCrabTunnel { get; } = new("CrabHoleToCrabTunnel", RoomDefinitions.CrabTunnel, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(62, 6), new(3, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Crab Hole room to the West Sand Hall Tunnel room.</summary>
    public static DoorDefinition CrabHoleToWestSandHallTunnel { get; } = new("CrabHoleToWestSandHallTunnel", RoomDefinitions.WestSandHallTunnel, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Crab Hole room to the East Tunnel room.</summary>
    public static DoorDefinition CrabHoleToEastTunnel { get; } = new("CrabHoleToEastTunnel", RoomDefinitions.EastTunnel, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(62, 6), new(3, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Crab Hole room to the Maridia Map room.</summary>
    public static DoorDefinition CrabHoleToMaridiaMap { get; } = new("CrabHoleToMaridiaMap", RoomDefinitions.MaridiaMap, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the West Sand Hall Tunnel room to the Crab Hole room.</summary>
    public static DoorDefinition WestSandHallTunnelToCrabHole { get; } = new("WestSandHallTunnelToCrabHole", RoomDefinitions.CrabHole, new(DoorDirection.Left, DoorClosingBehavior.None), null, new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the West Sand Hall Tunnel room to the West Sand Hall room.</summary>
    public static DoorDefinition WestSandHallTunnelToWestSandHall { get; } = new("WestSandHallTunnelToWestSandHall", RoomDefinitions.WestSandHall, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Plasma Tutorial room to the Kassiuz room.</summary>
    public static DoorDefinition PlasmaTutorialToKassiuz { get; } = new("PlasmaTutorialToKassiuz", RoomDefinitions.Kassiuz, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Plasma Tutorial room to the Plasma room.</summary>
    public static DoorDefinition PlasmaTutorialToPlasma { get; } = new("PlasmaTutorialToPlasma", RoomDefinitions.Plasma, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Plasma room to the Plasma Tutorial room.</summary>
    public static DoorDefinition PlasmaToPlasmaTutorial { get; } = new("PlasmaToPlasmaTutorial", RoomDefinitions.PlasmaTutorial, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Thread The Needle room to the Bug Sand Hole room.</summary>
    public static DoorDefinition ThreadTheNeedleToBugSandHole { get; } = new("ThreadTheNeedleToBugSandHole", RoomDefinitions.BugSandHole, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Thread The Needle room to the Maridia Elevator room.</summary>
    public static DoorDefinition ThreadTheNeedleToMaridiaElevator { get; } = new("ThreadTheNeedleToMaridiaElevator", RoomDefinitions.MaridiaElevator, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 86), new(0, 5), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Maridia Elevator room to the Thread The Needle room.</summary>
    public static DoorDefinition MaridiaElevatorToThreadTheNeedle { get; } = new("MaridiaElevatorToThreadTheNeedle", RoomDefinitions.ThreadTheNeedle, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(110, 6), new(6, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Maridia Elevator room to the Forgotten Highway Save room.</summary>
    public static DoorDefinition MaridiaElevatorToForgottenHighwaySave { get; } = new("MaridiaElevatorToForgottenHighwaySave", RoomDefinitions.ForgottenHighwaySave, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Maridia Elevator room to the Forgotten Highway Elevator room.</summary>
    public static DoorDefinition MaridiaElevatorToForgottenHighwayElevator { get; } = new("MaridiaElevatorToForgottenHighwayElevator", RoomDefinitions.ForgottenHighwayElevator, new(DoorDirection.Up, DoorClosingBehavior.None), null, new(0, 0), DoorSamusSpeed.None, DoorSetupDefinitions.None) { ExchangesExploredMaps = true };

    /// <summary>The door from the Plasma Spark room to the Butterfly room.</summary>
    public static DoorDefinition PlasmaSparkToButterfly { get; } = new("PlasmaSparkToButterfly", RoomDefinitions.Butterfly, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Plasma Spark room to the Toilet room.</summary>
    public static DoorDefinition PlasmaSparkToToilet { get; } = new("PlasmaSparkToToilet", RoomDefinitions.Toilet, new(DoorDirection.Down, DoorClosingBehavior.None), null, new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.ToiletStartsElevatubeRideFromNorth);

    /// <summary>The door from the Plasma Spark room to the Kassiuz room.</summary>
    public static DoorDefinition PlasmaSparkToKassiuz { get; } = new("PlasmaSparkToKassiuz", RoomDefinitions.Kassiuz, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 54), new(0, 3), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Plasma Spark room to the Bug Sand Hole room.</summary>
    public static DoorDefinition PlasmaSparkToBugSandHole { get; } = new("PlasmaSparkToBugSandHole", RoomDefinitions.BugSandHole, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Kassiuz room to the Plasma Spark room.</summary>
    public static DoorDefinition KassiuzToPlasmaSpark { get; } = new("KassiuzToPlasmaSpark", RoomDefinitions.PlasmaSpark, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(46, 22), new(2, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Kassiuz room to the Plasma Tutorial room.</summary>
    public static DoorDefinition KassiuzToPlasmaTutorial { get; } = new("KassiuzToPlasmaTutorial", RoomDefinitions.PlasmaTutorial, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Maridia Map room to the Crab Hole room.</summary>
    public static DoorDefinition MaridiaMapToCrabHole { get; } = new("MaridiaMapToCrabHole", RoomDefinitions.CrabHole, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.CrabHoleClosesScreen0x0OpensScreen0x1TopAligned);

    /// <summary>The door from the Forgotten Highway Save room to the Maridia Elevator room.</summary>
    public static DoorDefinition ForgottenHighwaySaveToMaridiaElevator { get; } = new("ForgottenHighwaySaveToMaridiaElevator", RoomDefinitions.MaridiaElevator, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 70), new(0, 4), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Toilet room to the Oasis room.</summary>
    public static DoorDefinition ToiletToOasis { get; } = new("ToiletToOasis", RoomDefinitions.Oasis, new(DoorDirection.Down, DoorClosingBehavior.BlueDoorCloses), new(6, 2), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.OasisEndsElevatubeRideOpensScreens0x0And0x1);

    /// <summary>The door from the Toilet room to the Plasma Spark room.</summary>
    public static DoorDefinition ToiletToPlasmaSpark { get; } = new("ToiletToPlasmaSpark", RoomDefinitions.PlasmaSpark, new(DoorDirection.Up, DoorClosingBehavior.BlueDoorCloses), new(6, 45), new(0, 2), DoorSamusSpeed.PixelsPerStep(2), DoorSetupDefinitions.PlasmaSparkEndsElevatubeRide);

    /// <summary>The door from the Bug Sand Hole room to the Thread The Needle room.</summary>
    public static DoorDefinition BugSandHoleToThreadTheNeedle { get; } = new("BugSandHoleToThreadTheNeedle", RoomDefinitions.ThreadTheNeedle, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Bug Sand Hole room to the Plasma Beach Quicksand room.</summary>
    public static DoorDefinition BugSandHoleToPlasmaBeachQuicksand { get; } = new("BugSandHoleToPlasmaBeachQuicksand", RoomDefinitions.PlasmaBeachQuicksand, new(DoorDirection.Down, DoorClosingBehavior.None), null, new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Bug Sand Hole room to the Plasma Spark room.</summary>
    public static DoorDefinition BugSandHoleToPlasmaSpark { get; } = new("BugSandHoleToPlasmaSpark", RoomDefinitions.PlasmaSpark, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(62, 54), new(3, 3), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the West Sand Hall room to the West Sand Hall Tunnel room.</summary>
    public static DoorDefinition WestSandHallToWestSandHallTunnel { get; } = new("WestSandHallToWestSandHallTunnel", RoomDefinitions.WestSandHallTunnel, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the West Sand Hall room to the Oasis room.</summary>
    public static DoorDefinition WestSandHallToOasis { get; } = new("WestSandHallToOasis", RoomDefinitions.Oasis, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the West Sand Hall room up into the West Sand Hole room.</summary>
    public static DoorDefinition WestSandHallToWestSandHole { get; } = new("WestSandHallToWestSandHole", RoomDefinitions.WestSandHole, new(DoorDirection.Up, DoorClosingBehavior.None), null, new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Oasis room to the West Sand Hall room.</summary>
    public static DoorDefinition OasisToWestSandHall { get; } = new("OasisToWestSandHall", RoomDefinitions.WestSandHall, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(62, 6), new(3, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Oasis room to the East Sand Hall room.</summary>
    public static DoorDefinition OasisToEastSandHall { get; } = new("OasisToEastSandHall", RoomDefinitions.EastSandHall, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Oasis room to the Toilet room.</summary>
    public static DoorDefinition OasisToToilet { get; } = new("OasisToToilet", RoomDefinitions.Toilet, new(DoorDirection.Up, DoorClosingBehavior.None), null, new(0, 9), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.ToiletStartsElevatubeRideFromSouth);

    /// <summary>The door from the East Sand Hall room to the Oasis room.</summary>
    public static DoorDefinition EastSandHallToOasis { get; } = new("EastSandHallToOasis", RoomDefinitions.Oasis, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the East Sand Hall room to the Pants room.</summary>
    public static DoorDefinition EastSandHallToPants { get; } = new("EastSandHallToPants", RoomDefinitions.Pants, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 54), new(0, 3), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.PantsClosesScreen0x2);

    /// <summary>The door from the East Sand Hall room up into the East Sand Hole room.</summary>
    public static DoorDefinition EastSandHallToEastSandHole { get; } = new("EastSandHallToEastSandHole", RoomDefinitions.EastSandHole, new(DoorDirection.Up, DoorClosingBehavior.None), null, new(1, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the West Sand Hole room to the West Aqueduct Quicksand room.</summary>
    public static DoorDefinition WestSandHoleToWestAqueductQuicksand { get; } = new("WestSandHoleToWestAqueductQuicksand", RoomDefinitions.WestAqueductQuicksand, new(DoorDirection.Up, DoorClosingBehavior.None), null, new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the West Sand Hole room to the West Sand Hall room.</summary>
    public static DoorDefinition WestSandHoleToWestSandHall { get; } = new("WestSandHoleToWestSandHall", RoomDefinitions.WestSandHall, new(DoorDirection.Down, DoorClosingBehavior.None), null, new(2, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the East Sand Hole room to the East Aqueduct Quicksand room.</summary>
    public static DoorDefinition EastSandHoleToEastAqueductQuicksand { get; } = new("EastSandHoleToEastAqueductQuicksand", RoomDefinitions.EastAqueductQuicksand, new(DoorDirection.Up, DoorClosingBehavior.None), null, new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the East Sand Hole room to the East Sand Hall room.</summary>
    public static DoorDefinition EastSandHoleToEastSandHall { get; } = new("EastSandHoleToEastSandHall", RoomDefinitions.EastSandHall, new(DoorDirection.Down, DoorClosingBehavior.None), null, new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the West Aqueduct Quicksand room to the Aqueduct room.</summary>
    public static DoorDefinition WestAqueductQuicksandToAqueduct { get; } = new("WestAqueductQuicksandToAqueduct", RoomDefinitions.Aqueduct, new(DoorDirection.Up, DoorClosingBehavior.None), null, new(1, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the West Aqueduct Quicksand room to the West Sand Hole room.</summary>
    public static DoorDefinition WestAqueductQuicksandToWestSandHole { get; } = new("WestAqueductQuicksandToWestSandHole", RoomDefinitions.WestSandHole, new(DoorDirection.Down, DoorClosingBehavior.None), null, new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the East Aqueduct Quicksand room to the Aqueduct room.</summary>
    public static DoorDefinition EastAqueductQuicksandToAqueduct { get; } = new("EastAqueductQuicksandToAqueduct", RoomDefinitions.Aqueduct, new(DoorDirection.Up, DoorClosingBehavior.None), null, new(3, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the East Aqueduct Quicksand room to the East Sand Hole room.</summary>
    public static DoorDefinition EastAqueductQuicksandToEastSandHole { get; } = new("EastAqueductQuicksandToEastSandHole", RoomDefinitions.EastSandHole, new(DoorDirection.Down, DoorClosingBehavior.None), null, new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Aqueduct room to the Crab Shaft room.</summary>
    public static DoorDefinition AqueductToCrabShaft { get; } = new("AqueductToCrabShaft", RoomDefinitions.CrabShaft, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 54), new(1, 3), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.CrabShaftOpensScreen0x3TopAligned);

    /// <summary>The door from the Aqueduct room to the West Aqueduct Quicksand room.</summary>
    public static DoorDefinition AqueductToWestAqueductQuicksand { get; } = new("AqueductToWestAqueductQuicksand", RoomDefinitions.WestAqueductQuicksand, new(DoorDirection.Down, DoorClosingBehavior.None), null, new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Aqueduct room to the East Aqueduct Quicksand room.</summary>
    public static DoorDefinition AqueductToEastAqueductQuicksand { get; } = new("AqueductToEastAqueductQuicksand", RoomDefinitions.EastAqueductQuicksand, new(DoorDirection.Down, DoorClosingBehavior.None), null, new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Aqueduct room to the Botwoon Hallway room.</summary>
    public static DoorDefinition AqueductToBotwoonHallway { get; } = new("AqueductToBotwoonHallway", RoomDefinitions.BotwoonHallway, new(DoorDirection.Up, DoorClosingBehavior.BlueDoorCloses), new(6, 13), new(0, 0), DoorSamusSpeed.PixelsPerStep(1, 3, 4), DoorSetupDefinitions.None);

    /// <summary>The door from the Aqueduct room to the Below Botwoon Energy Tank room.</summary>
    public static DoorDefinition AqueductToBelowBotwoonEnergyTank { get; } = new("AqueductToBelowBotwoonEnergyTank", RoomDefinitions.BelowBotwoonEnergyTank, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Aqueduct room to the Aqueduct Save room.</summary>
    public static DoorDefinition AqueductToAqueductSave { get; } = new("AqueductToAqueductSave", RoomDefinitions.AqueductSave, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Butterfly room to the Plasma Spark room.</summary>
    public static DoorDefinition ButterflyToPlasmaSpark { get; } = new("ButterflyToPlasmaSpark", RoomDefinitions.PlasmaSpark, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(62, 86), new(3, 5), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Butterfly room to the West Cactus Alley room.</summary>
    public static DoorDefinition ButterflyToWestCactusAlley { get; } = new("ButterflyToWestCactusAlley", RoomDefinitions.WestCactusAlley, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Botwoon Hallway room to the Aqueduct room.</summary>
    public static DoorDefinition BotwoonHallwayToAqueduct { get; } = new("BotwoonHallwayToAqueduct", RoomDefinitions.Aqueduct, new(DoorDirection.Down, DoorClosingBehavior.BlueDoorCloses), new(6, 2), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Botwoon Hallway room to the Botwoon room.</summary>
    public static DoorDefinition BotwoonHallwayToBotwoon { get; } = new("BotwoonHallwayToBotwoon", RoomDefinitions.Botwoon, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Pants room to the East Sand Hall room.</summary>
    public static DoorDefinition PantsToEastSandHall { get; } = new("PantsToEastSandHall", RoomDefinitions.EastSandHall, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(46, 6), new(2, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Pants room to the East Pants room.</summary>
    public static DoorDefinition PantsToEastPants { get; } = new("PantsToEastPants", RoomDefinitions.EastPants, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 38), new(0, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Pants room to the Shaktool room.</summary>
    public static DoorDefinition PantsToShaktool { get; } = new("PantsToShaktool", RoomDefinitions.Shaktool, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Pants room to the Pants room.</summary>
    public static DoorDefinition PantsToPants { get; } = new("PantsToPants", RoomDefinitions.Pants, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 54), new(0, 3), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.PantsClosesScreens0x2And1x3);

    /// <summary>The door from the East Pants room to the Pants room.</summary>
    public static DoorDefinition EastPantsToPants { get; } = new("EastPantsToPants", RoomDefinitions.Pants, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 54), new(0, 3), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.PantsClosesScreen0x2);

    /// <summary>The door from the East Pants room to the Shaktool room.</summary>
    public static DoorDefinition EastPantsToShaktool { get; } = new("EastPantsToShaktool", RoomDefinitions.Shaktool, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Springball room to the Shaktool room.</summary>
    public static DoorDefinition SpringballToShaktool { get; } = new("SpringballToShaktool", RoomDefinitions.Shaktool, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(62, 6), new(3, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Below Botwoon Energy Tank room to the Aqueduct room.</summary>
    public static DoorDefinition BelowBotwoonEnergyTankToAqueduct { get; } = new("BelowBotwoonEnergyTankToAqueduct", RoomDefinitions.Aqueduct, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(94, 22), new(5, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Colosseum room to the Halfie Climb room.</summary>
    public static DoorDefinition ColosseumToHalfieClimb { get; } = new("ColosseumToHalfieClimb", RoomDefinitions.HalfieClimb, new(DoorDirection.Left, DoorClosingBehavior.None), null, new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Colosseum room to the Draygon Save room.</summary>
    public static DoorDefinition ColosseumToDraygonSave { get; } = new("ColosseumToDraygonSave", RoomDefinitions.DraygonSave, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Colosseum room to the Precious room.</summary>
    public static DoorDefinition ColosseumToThePrecious { get; } = new("ColosseumToThePrecious", RoomDefinitions.ThePrecious, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door into the Precious room a Maridia load station enters through.</summary>
    public static DoorDefinition ThePreciousLoadStationEntry { get; } = new("ThePreciousLoadStationEntry", RoomDefinitions.ThePrecious, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Aqueduct Save room to the Aqueduct room.</summary>
    public static DoorDefinition AqueductSaveToAqueduct { get; } = new("AqueductSaveToAqueduct", RoomDefinitions.Aqueduct, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 38), new(0, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Precious room to the Colosseum room.</summary>
    public static DoorDefinition ThePreciousToColosseum { get; } = new("ThePreciousToColosseum", RoomDefinitions.Colosseum, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(110, 22), new(6, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Precious room to the Draygon room.</summary>
    public static DoorDefinition ThePreciousToDraygon { get; } = new("ThePreciousToDraygon", RoomDefinitions.Draygon, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 6), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Botwoon Energy Tank room to the Botwoon room.</summary>
    public static DoorDefinition BotwoonEnergyTankToBotwoon { get; } = new("BotwoonEnergyTankToBotwoon", RoomDefinitions.Botwoon, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 6), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.BotwoonOpensScreen1x0TopAligned);

    /// <summary>The door from the Botwoon Energy Tank room to the Botwoon Quicksand room, arriving at its right half.</summary>
    public static DoorDefinition BotwoonEnergyTankToBotwoonQuicksandRight { get; } = new("BotwoonEnergyTankToBotwoonQuicksandRight", RoomDefinitions.BotwoonQuicksand, new(DoorDirection.Down, DoorClosingBehavior.None), null, new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Botwoon Energy Tank room to the Botwoon Quicksand room, arriving at its left half.</summary>
    public static DoorDefinition BotwoonEnergyTankToBotwoonQuicksandLeft { get; } = new("BotwoonEnergyTankToBotwoonQuicksandLeft", RoomDefinitions.BotwoonQuicksand, new(DoorDirection.Down, DoorClosingBehavior.None), null, new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Botwoon Energy Tank room to the Halfie Climb room.</summary>
    public static DoorDefinition BotwoonEnergyTankToHalfieClimb { get; } = new("BotwoonEnergyTankToHalfieClimb", RoomDefinitions.HalfieClimb, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 38), new(0, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Draygon Save room to the Maridia Energy Refill room.</summary>
    public static DoorDefinition DraygonSaveToMaridiaEnergyRefill { get; } = new("DraygonSaveToMaridiaEnergyRefill", RoomDefinitions.MaridiaEnergyRefill, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Draygon Save room to the Colosseum room.</summary>
    public static DoorDefinition DraygonSaveToColosseum { get; } = new("DraygonSaveToColosseum", RoomDefinitions.Colosseum, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(110, 6), new(6, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Maridia Missile Refill room to the Halfie Climb room.</summary>
    public static DoorDefinition MaridiaMissileRefillToHalfieClimb { get; } = new("MaridiaMissileRefillToHalfieClimb", RoomDefinitions.HalfieClimb, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(78, 38), new(4, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.HalfieClimbClosesScreen0x2OpensScreen1x2TopAligned);

    /// <summary>The door from the Plasma Beach Quicksand room to the Butterfly room.</summary>
    public static DoorDefinition PlasmaBeachQuicksandToButterfly { get; } = new("PlasmaBeachQuicksandToButterfly", RoomDefinitions.Butterfly, new(DoorDirection.Down, DoorClosingBehavior.None), null, new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Botwoon Quicksand room to the Below Botwoon Energy Tank room, arriving at its left half.</summary>
    public static DoorDefinition BotwoonQuicksandToBelowBotwoonEnergyTankLeft { get; } = new("BotwoonQuicksandToBelowBotwoonEnergyTankLeft", RoomDefinitions.BelowBotwoonEnergyTank, new(DoorDirection.Down, DoorClosingBehavior.None), null, new(2, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Botwoon Quicksand room to the Below Botwoon Energy Tank room, arriving at its right half.</summary>
    public static DoorDefinition BotwoonQuicksandToBelowBotwoonEnergyTankRight { get; } = new("BotwoonQuicksandToBelowBotwoonEnergyTankRight", RoomDefinitions.BelowBotwoonEnergyTank, new(DoorDirection.Down, DoorClosingBehavior.None), null, new(3, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Shaktool room to the East Pants room.</summary>
    public static DoorDefinition ShaktoolToEastPants { get; } = new("ShaktoolToEastPants", RoomDefinitions.EastPants, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.EastPantsClosesScreen0x2OpensScreen0x1TopAligned);

    /// <summary>The door from the Shaktool room to the Springball room.</summary>
    public static DoorDefinition ShaktoolToSpringball { get; } = new("ShaktoolToSpringball", RoomDefinitions.Springball, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Halfie Climb room to the Botwoon Energy Tank room.</summary>
    public static DoorDefinition HalfieClimbToBotwoonEnergyTank { get; } = new("HalfieClimbToBotwoonEnergyTank", RoomDefinitions.BotwoonEnergyTank, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(110, 6), new(6, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Halfie Climb room to the Colosseum room.</summary>
    public static DoorDefinition HalfieClimbToColosseum { get; } = new("HalfieClimbToColosseum", RoomDefinitions.Colosseum, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Halfie Climb room to the Maridia Missile Refill room.</summary>
    public static DoorDefinition HalfieClimbToMaridiaMissileRefill { get; } = new("HalfieClimbToMaridiaMissileRefill", RoomDefinitions.MaridiaMissileRefill, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Halfie Climb room to the East Cactus Alley room.</summary>
    public static DoorDefinition HalfieClimbToEastCactusAlley { get; } = new("HalfieClimbToEastCactusAlley", RoomDefinitions.EastCactusAlley, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(78, 22), new(4, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Botwoon room to the Botwoon Hallway room.</summary>
    public static DoorDefinition BotwoonToBotwoonHallway { get; } = new("BotwoonToBotwoonHallway", RoomDefinitions.BotwoonHallway, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(62, 6), new(3, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Botwoon room to the Botwoon Energy Tank room.</summary>
    public static DoorDefinition BotwoonToBotwoonEnergyTank { get; } = new("BotwoonToBotwoonEnergyTank", RoomDefinitions.BotwoonEnergyTank, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Space Jump room to the Draygon room.</summary>
    public static DoorDefinition SpaceJumpToDraygon { get; } = new("SpaceJumpToDraygon", RoomDefinitions.Draygon, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Maridia Energy Refill room to the Draygon Save room.</summary>
    public static DoorDefinition MaridiaEnergyRefillToDraygonSave { get; } = new("MaridiaEnergyRefillToDraygonSave", RoomDefinitions.DraygonSave, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the West Cactus Alley room to the Butterfly room.</summary>
    public static DoorDefinition WestCactusAlleyToButterfly { get; } = new("WestCactusAlleyToButterfly", RoomDefinitions.Butterfly, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the West Cactus Alley room to the East Cactus Alley room.</summary>
    public static DoorDefinition WestCactusAlleyToEastCactusAlley { get; } = new("WestCactusAlleyToEastCactusAlley", RoomDefinitions.EastCactusAlley, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the East Cactus Alley room to the West Cactus Alley room.</summary>
    public static DoorDefinition EastCactusAlleyToWestCactusAlley { get; } = new("EastCactusAlleyToWestCactusAlley", RoomDefinitions.WestCactusAlley, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the East Cactus Alley room to the Halfie Climb room.</summary>
    public static DoorDefinition EastCactusAlleyToHalfieClimb { get; } = new("EastCactusAlleyToHalfieClimb", RoomDefinitions.HalfieClimb, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Draygon room to the Precious room.</summary>
    public static DoorDefinition DraygonToThePrecious { get; } = new("DraygonToThePrecious", RoomDefinitions.ThePrecious, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 38), new(0, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.ThePreciousOpensScreens0x0And0x1);

    /// <summary>The door from the Draygon room to the Space Jump room.</summary>
    public static DoorDefinition DraygonToSpaceJump { get; } = new("DraygonToSpaceJump", RoomDefinitions.SpaceJump, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);
}
