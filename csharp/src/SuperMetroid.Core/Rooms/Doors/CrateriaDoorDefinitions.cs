namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The Crateria rooms' doors, each named for the room it leads from and the room it leads to.</summary>
public static class CrateriaDoorDefinitions
{

    /// <summary>The door into the Landing Site room that the landing cutscene uses.</summary>
    public static DoorDefinition LandingSiteLandingCutscene { get; } = DoorDefinitions.Find(nameof(LandingSiteLandingCutscene));

    /// <summary>The door from the Landing Site room to the Parlor room.</summary>
    public static DoorDefinition LandingSiteToParlor { get; } = DoorDefinitions.Find(nameof(LandingSiteToParlor));

    /// <summary>The door from the Landing Site room to the Crateria Tube room.</summary>
    public static DoorDefinition LandingSiteToCrateriaTube { get; } = DoorDefinitions.Find(nameof(LandingSiteToCrateriaTube));

    /// <summary>The door from the Landing Site room to the Gauntlet Entrance room.</summary>
    public static DoorDefinition LandingSiteToGauntletEntrance { get; } = DoorDefinitions.Find(nameof(LandingSiteToGauntletEntrance));

    /// <summary>The door from the Landing Site room to the Crateria Power Bombs room.</summary>
    public static DoorDefinition LandingSiteToCrateriaPowerBombs { get; } = DoorDefinitions.Find(nameof(LandingSiteToCrateriaPowerBombs));

    /// <summary>The door from the Gauntlet Entrance room to the Landing Site room.</summary>
    public static DoorDefinition GauntletEntranceToLandingSite { get; } = DoorDefinitions.Find(nameof(GauntletEntranceToLandingSite));

    /// <summary>The door from the Gauntlet Entrance room to the Gauntlet Energy Tank room.</summary>
    public static DoorDefinition GauntletEntranceToGauntletEnergyTank { get; } = DoorDefinitions.Find(nameof(GauntletEntranceToGauntletEnergyTank));

    /// <summary>The door from the Parlor room to the Terminator room.</summary>
    public static DoorDefinition ParlorToTerminator { get; } = DoorDefinitions.Find(nameof(ParlorToTerminator));

    /// <summary>The door from the Parlor room to the Landing Site room.</summary>
    public static DoorDefinition ParlorToLandingSite { get; } = DoorDefinitions.Find(nameof(ParlorToLandingSite));

    /// <summary>The door from the Parlor room to the Pre Map Flyway room.</summary>
    public static DoorDefinition ParlorToPreMapFlyway { get; } = DoorDefinitions.Find(nameof(ParlorToPreMapFlyway));

    /// <summary>The door from the Parlor room to the Flyway room.</summary>
    public static DoorDefinition ParlorToFlyway { get; } = DoorDefinitions.Find(nameof(ParlorToFlyway));

    /// <summary>The door from the Parlor room to the Climb room.</summary>
    public static DoorDefinition ParlorToClimb { get; } = DoorDefinitions.Find(nameof(ParlorToClimb));

    /// <summary>The door from the Parlor room to the Crateria Save room.</summary>
    public static DoorDefinition ParlorToCrateriaSave { get; } = DoorDefinitions.Find(nameof(ParlorToCrateriaSave));

    /// <summary>The door from the Parlor room to the Final Missile Bombway room.</summary>
    public static DoorDefinition ParlorToFinalMissileBombway { get; } = DoorDefinitions.Find(nameof(ParlorToFinalMissileBombway));

    /// <summary>The door from the Crateria Power Bombs room to the Landing Site room.</summary>
    public static DoorDefinition CrateriaPowerBombsToLandingSite { get; } = DoorDefinitions.Find(nameof(CrateriaPowerBombsToLandingSite));

    /// <summary>The door from the Crateria Save room to the Parlor room.</summary>
    public static DoorDefinition CrateriaSaveToParlor { get; } = DoorDefinitions.Find(nameof(CrateriaSaveToParlor));

    /// <summary>The door from the West Ocean room to the Moat room.</summary>
    public static DoorDefinition WestOceanToMoat { get; } = DoorDefinitions.Find(nameof(WestOceanToMoat));

    /// <summary>The door from the West Ocean room to the Wrecked Ship Entrance room.</summary>
    public static DoorDefinition WestOceanToWreckedShipEntrance { get; } = DoorDefinitions.Find(nameof(WestOceanToWreckedShipEntrance));

    /// <summary>The door from the West Ocean room to the Bowling Alley Path room.</summary>
    public static DoorDefinition WestOceanToBowlingAlleyPath { get; } = DoorDefinitions.Find(nameof(WestOceanToBowlingAlleyPath));

    /// <summary>The door from the West Ocean room to the Attic room.</summary>
    public static DoorDefinition WestOceanToAttic { get; } = DoorDefinitions.Find(nameof(WestOceanToAttic));

    /// <summary>The door from the West Ocean room to the Bowling Alley room.</summary>
    public static DoorDefinition WestOceanToBowlingAlley { get; } = DoorDefinitions.Find(nameof(WestOceanToBowlingAlley));

    /// <summary>The door from the West Ocean room to the Gravity Suit room.</summary>
    public static DoorDefinition WestOceanToGravitySuit { get; } = DoorDefinitions.Find(nameof(WestOceanToGravitySuit));

    /// <summary>The door from the Bowling Alley Path room to the West Ocean room.</summary>
    public static DoorDefinition BowlingAlleyPathToWestOcean { get; } = DoorDefinitions.Find(nameof(BowlingAlleyPathToWestOcean));

    /// <summary>The door from the Bowling Alley Path room to the Pre Bowling room.</summary>
    public static DoorDefinition BowlingAlleyPathToPreBowling { get; } = DoorDefinitions.Find(nameof(BowlingAlleyPathToPreBowling));

    /// <summary>The door from the Crateria Kihunter room to the Crateria Tube room.</summary>
    public static DoorDefinition CrateriaKihunterToCrateriaTube { get; } = DoorDefinitions.Find(nameof(CrateriaKihunterToCrateriaTube));

    /// <summary>The door from the Crateria Kihunter room to the Moat room.</summary>
    public static DoorDefinition CrateriaKihunterToMoat { get; } = DoorDefinitions.Find(nameof(CrateriaKihunterToMoat));

    /// <summary>The door from the Crateria Kihunter room to the Red Brinstar Elevator room.</summary>
    public static DoorDefinition CrateriaKihunterToRedBrinstarElevator { get; } = DoorDefinitions.Find(nameof(CrateriaKihunterToRedBrinstarElevator));

    /// <summary>The door from the Forgotten Highway Elevator room to the Forgotten Highway Elbow room.</summary>
    public static DoorDefinition ForgottenHighwayElevatorToForgottenHighwayElbow { get; } = DoorDefinitions.Find(nameof(ForgottenHighwayElevatorToForgottenHighwayElbow));

    /// <summary>The door from the Forgotten Highway Elevator room to the Maridia Elevator room.</summary>
    public static DoorDefinition ForgottenHighwayElevatorToMaridiaElevator { get; } = DoorDefinitions.Find(nameof(ForgottenHighwayElevatorToMaridiaElevator));

    /// <summary>The door from the East Ocean room to the Electric Death room.</summary>
    public static DoorDefinition EastOceanToElectricDeath { get; } = DoorDefinitions.Find(nameof(EastOceanToElectricDeath));

    /// <summary>The door from the East Ocean room to the Forgotten Highway Kagos room.</summary>
    public static DoorDefinition EastOceanToForgottenHighwayKagos { get; } = DoorDefinitions.Find(nameof(EastOceanToForgottenHighwayKagos));

    /// <summary>The door from the Forgotten Highway Kagos room to the East Ocean room.</summary>
    public static DoorDefinition ForgottenHighwayKagosToEastOcean { get; } = DoorDefinitions.Find(nameof(ForgottenHighwayKagosToEastOcean));

    /// <summary>The door from the Forgotten Highway Kagos room to the Crab Maze room.</summary>
    public static DoorDefinition ForgottenHighwayKagosToCrabMaze { get; } = DoorDefinitions.Find(nameof(ForgottenHighwayKagosToCrabMaze));

    /// <summary>The door from the Crab Maze room to the Forgotten Highway Kagos room.</summary>
    public static DoorDefinition CrabMazeToForgottenHighwayKagos { get; } = DoorDefinitions.Find(nameof(CrabMazeToForgottenHighwayKagos));

    /// <summary>The door from the Forgotten Highway Elbow room to the Crab Maze room.</summary>
    public static DoorDefinition ForgottenHighwayElbowToCrabMaze { get; } = DoorDefinitions.Find(nameof(ForgottenHighwayElbowToCrabMaze));

    /// <summary>The door from the Crab Maze room to the Forgotten Highway Elbow room.</summary>
    public static DoorDefinition CrabMazeToForgottenHighwayElbow { get; } = DoorDefinitions.Find(nameof(CrabMazeToForgottenHighwayElbow));

    /// <summary>The door from the Forgotten Highway Elbow room to the Forgotten Highway Elevator room.</summary>
    public static DoorDefinition ForgottenHighwayElbowToForgottenHighwayElevator { get; } = DoorDefinitions.Find(nameof(ForgottenHighwayElbowToForgottenHighwayElevator));

    /// <summary>The door from the Crateria Tube room to the Landing Site room.</summary>
    public static DoorDefinition CrateriaTubeToLandingSite { get; } = DoorDefinitions.Find(nameof(CrateriaTubeToLandingSite));

    /// <summary>The door from the Crateria Tube room to the Crateria Kihunter room.</summary>
    public static DoorDefinition CrateriaTubeToCrateriaKihunter { get; } = DoorDefinitions.Find(nameof(CrateriaTubeToCrateriaKihunter));

    /// <summary>The door from the Moat room to the Crateria Kihunter room.</summary>
    public static DoorDefinition MoatToCrateriaKihunter { get; } = DoorDefinitions.Find(nameof(MoatToCrateriaKihunter));

    /// <summary>The door from the Moat room to the West Ocean room.</summary>
    public static DoorDefinition MoatToWestOcean { get; } = DoorDefinitions.Find(nameof(MoatToWestOcean));

    /// <summary>The door from the Red Brinstar Elevator room to the Crateria Kihunter room.</summary>
    public static DoorDefinition RedBrinstarElevatorToCrateriaKihunter { get; } = DoorDefinitions.Find(nameof(RedBrinstarElevatorToCrateriaKihunter));

    /// <summary>The door from the Red Brinstar Elevator room to the Caterpillar room.</summary>
    public static DoorDefinition RedBrinstarElevatorToCaterpillar { get; } = DoorDefinitions.Find(nameof(RedBrinstarElevatorToCaterpillar));

    /// <summary>The door from the Gauntlet Energy Tank room to the Gauntlet Entrance room.</summary>
    public static DoorDefinition GauntletEnergyTankToGauntletEntrance { get; } = DoorDefinitions.Find(nameof(GauntletEnergyTankToGauntletEntrance));

    /// <summary>The door from the Gauntlet Energy Tank room to the Green Pirates Shaft room.</summary>
    public static DoorDefinition GauntletEnergyTankToGreenPiratesShaft { get; } = DoorDefinitions.Find(nameof(GauntletEnergyTankToGreenPiratesShaft));

    /// <summary>The door from the Pre Bowling room to the Bowling Alley Path room.</summary>
    public static DoorDefinition PreBowlingToBowlingAlleyPath { get; } = DoorDefinitions.Find(nameof(PreBowlingToBowlingAlleyPath));

    /// <summary>The door from the Pre Bowling room to the Bowling Alley room.</summary>
    public static DoorDefinition PreBowlingToBowlingAlley { get; } = DoorDefinitions.Find(nameof(PreBowlingToBowlingAlley));

    /// <summary>The door from the Climb room to the Parlor room.</summary>
    public static DoorDefinition ClimbToParlor { get; } = DoorDefinitions.Find(nameof(ClimbToParlor));

    /// <summary>The door from the Climb room to the Crateria Super room, arriving at its upper doorway.</summary>
    public static DoorDefinition ClimbToCrateriaSuperUpper { get; } = DoorDefinitions.Find(nameof(ClimbToCrateriaSuperUpper));

    /// <summary>The door from the Climb room to the Crateria Super room, arriving at its lower doorway.</summary>
    public static DoorDefinition ClimbToCrateriaSuperLower { get; } = DoorDefinitions.Find(nameof(ClimbToCrateriaSuperLower));

    /// <summary>The door from the Climb room to the Pit room.</summary>
    public static DoorDefinition ClimbToPit { get; } = DoorDefinitions.Find(nameof(ClimbToPit));

    /// <summary>The door from the Climb room to the Tourian Escape 4 room.</summary>
    public static DoorDefinition ClimbToTourianEscape4 { get; } = DoorDefinitions.Find(nameof(ClimbToTourianEscape4));

    /// <summary>The door from the Pit room to the Climb room.</summary>
    public static DoorDefinition PitToClimb { get; } = DoorDefinitions.Find(nameof(PitToClimb));

    /// <summary>The door from the Pit room to the Elevator To Blue Brinstar room.</summary>
    public static DoorDefinition PitToElevatorToBlueBrinstar { get; } = DoorDefinitions.Find(nameof(PitToElevatorToBlueBrinstar));

    /// <summary>The door from the Elevator To Blue Brinstar room to the Pit room.</summary>
    public static DoorDefinition ElevatorToBlueBrinstarToPit { get; } = DoorDefinitions.Find(nameof(ElevatorToBlueBrinstarToPit));

    /// <summary>The door from the Elevator To Blue Brinstar room to the Morph Ball room.</summary>
    public static DoorDefinition ElevatorToBlueBrinstarToMorphBall { get; } = DoorDefinitions.Find(nameof(ElevatorToBlueBrinstarToMorphBall));

    /// <summary>The door from the Bomb Torizo room to the Flyway room.</summary>
    public static DoorDefinition BombTorizoToFlyway { get; } = DoorDefinitions.Find(nameof(BombTorizoToFlyway));

    /// <summary>The door from the Flyway room to the Parlor room.</summary>
    public static DoorDefinition FlywayToParlor { get; } = DoorDefinitions.Find(nameof(FlywayToParlor));

    /// <summary>The door from the Flyway room to the Bomb Torizo room.</summary>
    public static DoorDefinition FlywayToBombTorizo { get; } = DoorDefinitions.Find(nameof(FlywayToBombTorizo));

    /// <summary>The door from the Pre Map Flyway room to the Parlor room.</summary>
    public static DoorDefinition PreMapFlywayToParlor { get; } = DoorDefinitions.Find(nameof(PreMapFlywayToParlor));

    /// <summary>The door from the Pre Map Flyway room to the Crateria Map room.</summary>
    public static DoorDefinition PreMapFlywayToCrateriaMap { get; } = DoorDefinitions.Find(nameof(PreMapFlywayToCrateriaMap));

    /// <summary>The door from the Terminator room to the Green Pirates Shaft room.</summary>
    public static DoorDefinition TerminatorToGreenPiratesShaft { get; } = DoorDefinitions.Find(nameof(TerminatorToGreenPiratesShaft));

    /// <summary>The door from the Terminator room to the Parlor room.</summary>
    public static DoorDefinition TerminatorToParlor { get; } = DoorDefinitions.Find(nameof(TerminatorToParlor));

    /// <summary>The door from the Green Brinstar Elevator room to the Lower Mushrooms room.</summary>
    public static DoorDefinition GreenBrinstarElevatorToLowerMushrooms { get; } = DoorDefinitions.Find(nameof(GreenBrinstarElevatorToLowerMushrooms));

    /// <summary>The door from the Green Brinstar Elevator room to the Green Brinstar Main Shaft room.</summary>
    public static DoorDefinition GreenBrinstarElevatorToGreenBrinstarMainShaft { get; } = DoorDefinitions.Find(nameof(GreenBrinstarElevatorToGreenBrinstarMainShaft));

    /// <summary>The door from the Lower Mushrooms room to the Green Pirates Shaft room.</summary>
    public static DoorDefinition LowerMushroomsToGreenPiratesShaft { get; } = DoorDefinitions.Find(nameof(LowerMushroomsToGreenPiratesShaft));

    /// <summary>The door from the Lower Mushrooms room to the Green Brinstar Elevator room.</summary>
    public static DoorDefinition LowerMushroomsToGreenBrinstarElevator { get; } = DoorDefinitions.Find(nameof(LowerMushroomsToGreenBrinstarElevator));

    /// <summary>The door from the Crateria Map room to the Pre Map Flyway room.</summary>
    public static DoorDefinition CrateriaMapToPreMapFlyway { get; } = DoorDefinitions.Find(nameof(CrateriaMapToPreMapFlyway));

    /// <summary>The door from the Green Pirates Shaft room to the Terminator room.</summary>
    public static DoorDefinition GreenPiratesShaftToTerminator { get; } = DoorDefinitions.Find(nameof(GreenPiratesShaftToTerminator));

    /// <summary>The door from the Green Pirates Shaft room to the Lower Mushrooms room.</summary>
    public static DoorDefinition GreenPiratesShaftToLowerMushrooms { get; } = DoorDefinitions.Find(nameof(GreenPiratesShaftToLowerMushrooms));

    /// <summary>The door from the Green Pirates Shaft room to the Statues Hallway room.</summary>
    public static DoorDefinition GreenPiratesShaftToStatuesHallway { get; } = DoorDefinitions.Find(nameof(GreenPiratesShaftToStatuesHallway));

    /// <summary>The door from the Green Pirates Shaft room to the Gauntlet Energy Tank room.</summary>
    public static DoorDefinition GreenPiratesShaftToGauntletEnergyTank { get; } = DoorDefinitions.Find(nameof(GreenPiratesShaftToGauntletEnergyTank));

    /// <summary>The door from the Crateria Super room to the Climb room, arriving at its upper doorway.</summary>
    public static DoorDefinition CrateriaSuperToClimbUpper { get; } = DoorDefinitions.Find(nameof(CrateriaSuperToClimbUpper));

    /// <summary>The door from the Crateria Super room to the Climb room, arriving at its lower doorway.</summary>
    public static DoorDefinition CrateriaSuperToClimbLower { get; } = DoorDefinitions.Find(nameof(CrateriaSuperToClimbLower));

    /// <summary>The door from the Final Missile Bombway room to the Final Missile room.</summary>
    public static DoorDefinition FinalMissileBombwayToFinalMissile { get; } = DoorDefinitions.Find(nameof(FinalMissileBombwayToFinalMissile));

    /// <summary>The door from the Final Missile Bombway room to the Parlor room.</summary>
    public static DoorDefinition FinalMissileBombwayToParlor { get; } = DoorDefinitions.Find(nameof(FinalMissileBombwayToParlor));

    /// <summary>The door from the Final Missile room to the Final Missile Bombway room.</summary>
    public static DoorDefinition FinalMissileToFinalMissileBombway { get; } = DoorDefinitions.Find(nameof(FinalMissileToFinalMissileBombway));

    /// <summary>The door from the Statues Hallway room to the Green Pirates Shaft room.</summary>
    public static DoorDefinition StatuesHallwayToGreenPiratesShaft { get; } = DoorDefinitions.Find(nameof(StatuesHallwayToGreenPiratesShaft));

    /// <summary>The door from the Statues Hallway room to the Statues room.</summary>
    public static DoorDefinition StatuesHallwayToStatues { get; } = DoorDefinitions.Find(nameof(StatuesHallwayToStatues));

    /// <summary>The door from the Statues room to the Statues Hallway room.</summary>
    public static DoorDefinition StatuesToStatuesHallway { get; } = DoorDefinitions.Find(nameof(StatuesToStatuesHallway));

    /// <summary>The door from the Statues room to the Tourian First room.</summary>
    public static DoorDefinition StatuesToTourianFirst { get; } = DoorDefinitions.Find(nameof(StatuesToTourianFirst));
}
