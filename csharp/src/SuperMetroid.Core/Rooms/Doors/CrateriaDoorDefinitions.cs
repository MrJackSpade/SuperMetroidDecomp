namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The Crateria rooms' doors, each named for the room it leads from and the room it leads to.</summary>
public static class CrateriaDoorDefinitions
{

    /// <summary>The door into the Landing Site room that the landing cutscene uses.</summary>
    public static DoorDefinition LandingSiteLandingCutscene { get; } = new("LandingSiteLandingCutscene", RoomDefinitions.LandingSite, new(DoorDirection.Up, DoorClosingBehavior.None), null, new(4, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Landing Site room to the Parlor room.</summary>
    public static DoorDefinition LandingSiteToParlor { get; } = new("LandingSiteToParlor", RoomDefinitions.Parlor, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(78, 6), new(4, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Landing Site room to the Crateria Tube room.</summary>
    public static DoorDefinition LandingSiteToCrateriaTube { get; } = new("LandingSiteToCrateriaTube", RoomDefinitions.CrateriaTube, new(DoorDirection.Right, DoorClosingBehavior.None), null, new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Landing Site room to the Gauntlet Entrance room.</summary>
    public static DoorDefinition LandingSiteToGauntletEntrance { get; } = new("LandingSiteToGauntletEntrance", RoomDefinitions.GauntletEntrance, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(78, 6), new(4, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Landing Site room to the Crateria Power Bombs room.</summary>
    public static DoorDefinition LandingSiteToCrateriaPowerBombs { get; } = new("LandingSiteToCrateriaPowerBombs", RoomDefinitions.CrateriaPowerBombs, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Gauntlet Entrance room to the Landing Site room.</summary>
    public static DoorDefinition GauntletEntranceToLandingSite { get; } = new("GauntletEntranceToLandingSite", RoomDefinitions.LandingSite, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 38), new(0, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.LandingSiteOpensScreen1x2TopAligned);

    /// <summary>The door from the Gauntlet Entrance room to the Gauntlet Energy Tank room.</summary>
    public static DoorDefinition GauntletEntranceToGauntletEnergyTank { get; } = new("GauntletEntranceToGauntletEnergyTank", RoomDefinitions.GauntletEnergyTank, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(94, 6), new(5, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Parlor room to the Terminator room.</summary>
    public static DoorDefinition ParlorToTerminator { get; } = new("ParlorToTerminator", RoomDefinitions.Terminator, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(94, 6), new(5, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Parlor room to the Landing Site room.</summary>
    public static DoorDefinition ParlorToLandingSite { get; } = new("ParlorToLandingSite", RoomDefinitions.LandingSite, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 70), new(0, 4), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Parlor room to the Pre Map Flyway room.</summary>
    public static DoorDefinition ParlorToPreMapFlyway { get; } = new("ParlorToPreMapFlyway", RoomDefinitions.PreMapFlyway, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Parlor room to the Flyway room.</summary>
    public static DoorDefinition ParlorToFlyway { get; } = new("ParlorToFlyway", RoomDefinitions.Flyway, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Parlor room to the Climb room.</summary>
    public static DoorDefinition ParlorToClimb { get; } = new("ParlorToClimb", RoomDefinitions.Climb, new(DoorDirection.Down, DoorClosingBehavior.BlueDoorCloses), new(22, 2), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Parlor room to the Crateria Save room.</summary>
    public static DoorDefinition ParlorToCrateriaSave { get; } = new("ParlorToCrateriaSave", RoomDefinitions.CrateriaSave, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Parlor room to the Final Missile Bombway room.</summary>
    public static DoorDefinition ParlorToFinalMissileBombway { get; } = new("ParlorToFinalMissileBombway", RoomDefinitions.FinalMissileBombway, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 6), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Crateria Power Bombs room to the Landing Site room.</summary>
    public static DoorDefinition CrateriaPowerBombsToLandingSite { get; } = new("CrateriaPowerBombsToLandingSite", RoomDefinitions.LandingSite, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(142, 22), new(8, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Crateria Save room to the Parlor room.</summary>
    public static DoorDefinition CrateriaSaveToParlor { get; } = new("CrateriaSaveToParlor", RoomDefinitions.Parlor, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(17, 38), new(1, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.ParlorOpensScreen1x1);

    /// <summary>The door from the West Ocean room to the Moat room.</summary>
    public static DoorDefinition WestOceanToMoat { get; } = new("WestOceanToMoat", RoomDefinitions.Moat, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 6), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the West Ocean room to the Wrecked Ship Entrance room.</summary>
    public static DoorDefinition WestOceanToWreckedShipEntrance { get; } = new("WestOceanToWreckedShipEntrance", RoomDefinitions.WreckedShipEntrance, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.WreckedShipEntranceStartsTreadmillRightwards);

    /// <summary>The door from the West Ocean room to the Bowling Alley Path room.</summary>
    public static DoorDefinition WestOceanToBowlingAlleyPath { get; } = new("WestOceanToBowlingAlleyPath", RoomDefinitions.BowlingAlleyPath, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the West Ocean room to the Attic room.</summary>
    public static DoorDefinition WestOceanToAttic { get; } = new("WestOceanToAttic", RoomDefinitions.Attic, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the West Ocean room to the Bowling Alley room.</summary>
    public static DoorDefinition WestOceanToBowlingAlley { get; } = new("WestOceanToBowlingAlley", RoomDefinitions.BowlingAlley, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(33, 6), new(2, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.BowlingAlleyClosesScreens2x1And3x1And4x1And5x1);

    /// <summary>The door from the West Ocean room to the Gravity Suit room.</summary>
    public static DoorDefinition WestOceanToGravitySuit { get; } = new("WestOceanToGravitySuit", RoomDefinitions.GravitySuit, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Bowling Alley Path room to the West Ocean room.</summary>
    public static DoorDefinition BowlingAlleyPathToWestOcean { get; } = new("BowlingAlleyPathToWestOcean", RoomDefinitions.WestOcean, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(46, 38), new(2, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Bowling Alley Path room to the Pre Bowling room.</summary>
    public static DoorDefinition BowlingAlleyPathToPreBowling { get; } = new("BowlingAlleyPathToPreBowling", RoomDefinitions.PreBowling, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Crateria Kihunter room to the Crateria Tube room.</summary>
    public static DoorDefinition CrateriaKihunterToCrateriaTube { get; } = new("CrateriaKihunterToCrateriaTube", RoomDefinitions.CrateriaTube, new(DoorDirection.Left, DoorClosingBehavior.None), null, new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Crateria Kihunter room to the Moat room.</summary>
    public static DoorDefinition CrateriaKihunterToMoat { get; } = new("CrateriaKihunterToMoat", RoomDefinitions.Moat, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Crateria Kihunter room to the Red Brinstar Elevator room.</summary>
    public static DoorDefinition CrateriaKihunterToRedBrinstarElevator { get; } = new("CrateriaKihunterToRedBrinstarElevator", RoomDefinitions.RedBrinstarElevator, new(DoorDirection.Down, DoorClosingBehavior.BlueDoorCloses), new(6, 2), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Forgotten Highway Elevator room to the Forgotten Highway Elbow room.</summary>
    public static DoorDefinition ForgottenHighwayElevatorToForgottenHighwayElbow { get; } = new("ForgottenHighwayElevatorToForgottenHighwayElbow", RoomDefinitions.ForgottenHighwayElbow, new(DoorDirection.Up, DoorClosingBehavior.BlueDoorCloses), new(6, 13), new(0, 0), DoorSamusSpeed.PixelsPerStep(1, 3, 4), DoorSetupDefinitions.None);

    /// <summary>The door from the Forgotten Highway Elevator room to the Maridia Elevator room.</summary>
    public static DoorDefinition ForgottenHighwayElevatorToMaridiaElevator { get; } = new("ForgottenHighwayElevatorToMaridiaElevator", RoomDefinitions.MaridiaElevator, new(DoorDirection.Down, DoorClosingBehavior.None), null, new(0, 0), DoorSamusSpeed.None, DoorSetupDefinitions.None);

    /// <summary>The door from the East Ocean room to the Electric Death room.</summary>
    public static DoorDefinition EastOceanToElectricDeath { get; } = new("EastOceanToElectricDeath", RoomDefinitions.ElectricDeath, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the East Ocean room to the Forgotten Highway Kagos room.</summary>
    public static DoorDefinition EastOceanToForgottenHighwayKagos { get; } = new("EastOceanToForgottenHighwayKagos", RoomDefinitions.ForgottenHighwayKagos, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Forgotten Highway Kagos room to the East Ocean room.</summary>
    public static DoorDefinition ForgottenHighwayKagosToEastOcean { get; } = new("ForgottenHighwayKagosToEastOcean", RoomDefinitions.EastOcean, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(110, 70), new(6, 4), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Forgotten Highway Kagos room to the Crab Maze room.</summary>
    public static DoorDefinition ForgottenHighwayKagosToCrabMaze { get; } = new("ForgottenHighwayKagosToCrabMaze", RoomDefinitions.CrabMaze, new(DoorDirection.Down, DoorClosingBehavior.BlueDoorCloses), new(54, 2), new(3, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Crab Maze room to the Forgotten Highway Kagos room.</summary>
    public static DoorDefinition CrabMazeToForgottenHighwayKagos { get; } = new("CrabMazeToForgottenHighwayKagos", RoomDefinitions.ForgottenHighwayKagos, new(DoorDirection.Up, DoorClosingBehavior.BlueDoorCloses), new(6, 61), new(0, 3), DoorSamusSpeed.PixelsPerStep(1, 3, 4), DoorSetupDefinitions.None);

    /// <summary>The door from the Forgotten Highway Elbow room to the Crab Maze room.</summary>
    public static DoorDefinition ForgottenHighwayElbowToCrabMaze { get; } = new("ForgottenHighwayElbowToCrabMaze", RoomDefinitions.CrabMaze, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 22), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Crab Maze room to the Forgotten Highway Elbow room.</summary>
    public static DoorDefinition CrabMazeToForgottenHighwayElbow { get; } = new("CrabMazeToForgottenHighwayElbow", RoomDefinitions.ForgottenHighwayElbow, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Forgotten Highway Elbow room to the Forgotten Highway Elevator room.</summary>
    public static DoorDefinition ForgottenHighwayElbowToForgottenHighwayElevator { get; } = new("ForgottenHighwayElbowToForgottenHighwayElevator", RoomDefinitions.ForgottenHighwayElevator, new(DoorDirection.Down, DoorClosingBehavior.BlueDoorCloses), new(6, 2), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Crateria Tube room to the Landing Site room.</summary>
    public static DoorDefinition CrateriaTubeToLandingSite { get; } = new("CrateriaTubeToLandingSite", RoomDefinitions.LandingSite, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(142, 70), new(8, 4), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Crateria Tube room to the Crateria Kihunter room.</summary>
    public static DoorDefinition CrateriaTubeToCrateriaKihunter { get; } = new("CrateriaTubeToCrateriaKihunter", RoomDefinitions.CrateriaKihunter, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Moat room to the Crateria Kihunter room.</summary>
    public static DoorDefinition MoatToCrateriaKihunter { get; } = new("MoatToCrateriaKihunter", RoomDefinitions.CrateriaKihunter, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(46, 6), new(2, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Moat room to the West Ocean room.</summary>
    public static DoorDefinition MoatToWestOcean { get; } = new("MoatToWestOcean", RoomDefinitions.WestOcean, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 70), new(0, 4), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Red Brinstar Elevator room to the Crateria Kihunter room.</summary>
    public static DoorDefinition RedBrinstarElevatorToCrateriaKihunter { get; } = new("RedBrinstarElevatorToCrateriaKihunter", RoomDefinitions.CrateriaKihunter, new(DoorDirection.Up, DoorClosingBehavior.BlueDoorCloses), new(22, 45), new(1, 2), DoorSamusSpeed.PixelsPerStep(1, 3, 4), DoorSetupDefinitions.CrateriaKihunterOpensScreens1x0And1x1);

    /// <summary>The door from the Red Brinstar Elevator room to the Caterpillar room.</summary>
    public static DoorDefinition RedBrinstarElevatorToCaterpillar { get; } = new("RedBrinstarElevatorToCaterpillar", RoomDefinitions.Caterpillar, new(DoorDirection.Down, DoorClosingBehavior.None), null, new(0, 0), DoorSamusSpeed.None, DoorSetupDefinitions.CaterpillarOpensScreen0x2);

    /// <summary>The door from the Gauntlet Energy Tank room to the Gauntlet Entrance room.</summary>
    public static DoorDefinition GauntletEnergyTankToGauntletEntrance { get; } = new("GauntletEnergyTankToGauntletEntrance", RoomDefinitions.GauntletEntrance, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Gauntlet Energy Tank room to the Green Pirates Shaft room.</summary>
    public static DoorDefinition GauntletEnergyTankToGreenPiratesShaft { get; } = new("GauntletEnergyTankToGreenPiratesShaft", RoomDefinitions.GreenPiratesShaft, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.GreenPiratesShaftOpensScreen0x3);

    /// <summary>The door from the Pre Bowling room to the Bowling Alley Path room.</summary>
    public static DoorDefinition PreBowlingToBowlingAlleyPath { get; } = new("PreBowlingToBowlingAlleyPath", RoomDefinitions.BowlingAlleyPath, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 6), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Pre Bowling room to the Bowling Alley room.</summary>
    public static DoorDefinition PreBowlingToBowlingAlley { get; } = new("PreBowlingToBowlingAlley", RoomDefinitions.BowlingAlley, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 22), new(0, 1), DoorSamusSpeed.PixelsPerStep(0, 5, 8), DoorSetupDefinitions.BowlingAlleyClosesScreens2x0And3x0And4x0And5x0And5x1And0x2And1x2And5x2);

    /// <summary>The door from the Climb room to the Parlor room.</summary>
    public static DoorDefinition ClimbToParlor { get; } = new("ClimbToParlor", RoomDefinitions.Parlor, new(DoorDirection.Up, DoorClosingBehavior.BlueDoorCloses), new(22, 77), new(1, 4), DoorSamusSpeed.PixelsPerStep(1, 3, 4), DoorSetupDefinitions.ParlorOpensScreen1x1);

    /// <summary>The door from the Climb room to the Crateria Super room, arriving at its upper doorway.</summary>
    public static DoorDefinition ClimbToCrateriaSuperUpper { get; } = new("ClimbToCrateriaSuperUpper", RoomDefinitions.CrateriaSuper, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Climb room to the Crateria Super room, arriving at its lower doorway.</summary>
    public static DoorDefinition ClimbToCrateriaSuperLower { get; } = new("ClimbToCrateriaSuperLower", RoomDefinitions.CrateriaSuper, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 118), new(0, 7), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Climb room to the Pit room.</summary>
    public static DoorDefinition ClimbToPit { get; } = new("ClimbToPit", RoomDefinitions.Pit, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Climb room to the Tourian Escape 4 room.</summary>
    public static DoorDefinition ClimbToTourianEscape4 { get; } = new("ClimbToTourianEscape4", RoomDefinitions.TourianEscape4, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(46, 22), new(2, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Pit room to the Climb room.</summary>
    public static DoorDefinition PitToClimb { get; } = new("PitToClimb", RoomDefinitions.Climb, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 134), new(1, 8), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Pit room to the Elevator To Blue Brinstar room.</summary>
    public static DoorDefinition PitToElevatorToBlueBrinstar { get; } = new("PitToElevatorToBlueBrinstar", RoomDefinitions.ElevatorToBlueBrinstar, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Elevator To Blue Brinstar room to the Pit room.</summary>
    public static DoorDefinition ElevatorToBlueBrinstarToPit { get; } = new("ElevatorToBlueBrinstarToPit", RoomDefinitions.Pit, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(46, 6), new(2, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Elevator To Blue Brinstar room to the Morph Ball room.</summary>
    public static DoorDefinition ElevatorToBlueBrinstarToMorphBall { get; } = new("ElevatorToBlueBrinstarToMorphBall", RoomDefinitions.MorphBall, new(DoorDirection.Down, DoorClosingBehavior.None), null, new(5, 0), DoorSamusSpeed.None, DoorSetupDefinitions.None);

    /// <summary>The door from the Bomb Torizo room to the Flyway room.</summary>
    public static DoorDefinition BombTorizoToFlyway { get; } = new("BombTorizoToFlyway", RoomDefinitions.Flyway, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(46, 6), new(2, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Flyway room to the Parlor room.</summary>
    public static DoorDefinition FlywayToParlor { get; } = new("FlywayToParlor", RoomDefinitions.Parlor, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(62, 38), new(3, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.ParlorClosesScreen4x0OpensScreen3x1);

    /// <summary>The door from the Flyway room to the Bomb Torizo room.</summary>
    public static DoorDefinition FlywayToBombTorizo { get; } = new("FlywayToBombTorizo", RoomDefinitions.BombTorizo, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Pre Map Flyway room to the Parlor room.</summary>
    public static DoorDefinition PreMapFlywayToParlor { get; } = new("PreMapFlywayToParlor", RoomDefinitions.Parlor, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(30, 54), new(1, 3), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.ParlorOpensScreen1x1);

    /// <summary>The door from the Pre Map Flyway room to the Crateria Map room.</summary>
    public static DoorDefinition PreMapFlywayToCrateriaMap { get; } = new("PreMapFlywayToCrateriaMap", RoomDefinitions.CrateriaMap, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Terminator room to the Green Pirates Shaft room.</summary>
    public static DoorDefinition TerminatorToGreenPiratesShaft { get; } = new("TerminatorToGreenPiratesShaft", RoomDefinitions.GreenPiratesShaft, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 70), new(0, 4), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Terminator room to the Parlor room.</summary>
    public static DoorDefinition TerminatorToParlor { get; } = new("TerminatorToParlor", RoomDefinitions.Parlor, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.ParlorOpensScreen0x0TopAligned);

    /// <summary>The door from the Green Brinstar Elevator room to the Lower Mushrooms room.</summary>
    public static DoorDefinition GreenBrinstarElevatorToLowerMushrooms { get; } = new("GreenBrinstarElevatorToLowerMushrooms", RoomDefinitions.LowerMushrooms, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Green Brinstar Elevator room to the Green Brinstar Main Shaft room.</summary>
    public static DoorDefinition GreenBrinstarElevatorToGreenBrinstarMainShaft { get; } = new("GreenBrinstarElevatorToGreenBrinstarMainShaft", RoomDefinitions.GreenBrinstarMainShaft, new(DoorDirection.Down, DoorClosingBehavior.None), null, new(0, 0), DoorSamusSpeed.None, DoorSetupDefinitions.GreenBrinstarMainShaftOpensScreen1x7TopAligned);

    /// <summary>The door from the Lower Mushrooms room to the Green Pirates Shaft room.</summary>
    public static DoorDefinition LowerMushroomsToGreenPiratesShaft { get; } = new("LowerMushroomsToGreenPiratesShaft", RoomDefinitions.GreenPiratesShaft, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 102), new(0, 6), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Lower Mushrooms room to the Green Brinstar Elevator room.</summary>
    public static DoorDefinition LowerMushroomsToGreenBrinstarElevator { get; } = new("LowerMushroomsToGreenBrinstarElevator", RoomDefinitions.GreenBrinstarElevator, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Crateria Map room to the Pre Map Flyway room.</summary>
    public static DoorDefinition CrateriaMapToPreMapFlyway { get; } = new("CrateriaMapToPreMapFlyway", RoomDefinitions.PreMapFlyway, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(46, 6), new(2, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Green Pirates Shaft room to the Terminator room.</summary>
    public static DoorDefinition GreenPiratesShaftToTerminator { get; } = new("GreenPiratesShaftToTerminator", RoomDefinitions.Terminator, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 38), new(0, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Green Pirates Shaft room to the Lower Mushrooms room.</summary>
    public static DoorDefinition GreenPiratesShaftToLowerMushrooms { get; } = new("GreenPiratesShaftToLowerMushrooms", RoomDefinitions.LowerMushrooms, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(62, 6), new(3, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Green Pirates Shaft room to the Statues Hallway room.</summary>
    public static DoorDefinition GreenPiratesShaftToStatuesHallway { get; } = new("GreenPiratesShaftToStatuesHallway", RoomDefinitions.StatuesHallway, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Green Pirates Shaft room to the Gauntlet Energy Tank room.</summary>
    public static DoorDefinition GreenPiratesShaftToGauntletEnergyTank { get; } = new("GreenPiratesShaftToGauntletEnergyTank", RoomDefinitions.GauntletEnergyTank, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.GauntletEnergyTankOpensScreen4x0TopAligned);

    /// <summary>The door from the Crateria Super room to the Climb room, arriving at its upper doorway.</summary>
    public static DoorDefinition CrateriaSuperToClimbUpper { get; } = new("CrateriaSuperToClimbUpper", RoomDefinitions.Climb, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(46, 6), new(2, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.ClimbOpensScreen2x0TopAligned);

    /// <summary>The door from the Crateria Super room to the Climb room, arriving at its lower doorway.</summary>
    public static DoorDefinition CrateriaSuperToClimbLower { get; } = new("CrateriaSuperToClimbLower", RoomDefinitions.Climb, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(46, 118), new(2, 7), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.ClimbOpensScreen2x7TopAligned);

    /// <summary>The door from the Final Missile Bombway room to the Final Missile room.</summary>
    public static DoorDefinition FinalMissileBombwayToFinalMissile { get; } = new("FinalMissileBombwayToFinalMissile", RoomDefinitions.FinalMissile, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Final Missile Bombway room to the Parlor room.</summary>
    public static DoorDefinition FinalMissileBombwayToParlor { get; } = new("FinalMissileBombwayToParlor", RoomDefinitions.Parlor, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(17, 54), new(1, 3), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.ParlorOpensScreen1x1);

    /// <summary>The door from the Final Missile room to the Final Missile Bombway room.</summary>
    public static DoorDefinition FinalMissileToFinalMissileBombway { get; } = new("FinalMissileToFinalMissileBombway", RoomDefinitions.FinalMissileBombway, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Statues Hallway room to the Green Pirates Shaft room.</summary>
    public static DoorDefinition StatuesHallwayToGreenPiratesShaft { get; } = new("StatuesHallwayToGreenPiratesShaft", RoomDefinitions.GreenPiratesShaft, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(14, 102), new(0, 6), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Statues Hallway room to the Statues room.</summary>
    public static DoorDefinition StatuesHallwayToStatues { get; } = new("StatuesHallwayToStatues", RoomDefinitions.Statues, new(DoorDirection.Right, DoorClosingBehavior.BlueDoorCloses), new(1, 6), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Statues room to the Statues Hallway room.</summary>
    public static DoorDefinition StatuesToStatuesHallway { get; } = new("StatuesToStatuesHallway", RoomDefinitions.StatuesHallway, new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses), new(78, 6), new(4, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Statues room to the Tourian First room.</summary>
    public static DoorDefinition StatuesToTourianFirst { get; } = new("StatuesToTourianFirst", RoomDefinitions.TourianFirst, new(DoorDirection.Down, DoorClosingBehavior.None), null, new(0, 0), DoorSamusSpeed.None, DoorSetupDefinitions.None);
}
