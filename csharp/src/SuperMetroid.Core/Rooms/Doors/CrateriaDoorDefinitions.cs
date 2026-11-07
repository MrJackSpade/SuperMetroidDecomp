namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The Crateria rooms' doors, named by the room they lead from.</summary>
[NumberedIdentities("Each numbered door is one door of its room, named by its place in that room's door list; every door has its own destination and is used by name, never iterated or indexed.")]
public static class CrateriaDoorDefinitions
{

    /// <summary>The door into the Landing Site room that the landing cutscene uses.</summary>
    public static DoorDefinition LandingSiteLandingCutscene { get; } = DoorDefinitions.Find(nameof(LandingSiteLandingCutscene));

    /// <summary>The Landing Site room's door 0.</summary>
    public static DoorDefinition LandingSite0 { get; } = DoorDefinitions.Find(nameof(LandingSite0));

    /// <summary>The Landing Site room's door 1.</summary>
    public static DoorDefinition LandingSite1 { get; } = DoorDefinitions.Find(nameof(LandingSite1));

    /// <summary>The Landing Site room's door 2.</summary>
    public static DoorDefinition LandingSite2 { get; } = DoorDefinitions.Find(nameof(LandingSite2));

    /// <summary>The Landing Site room's door 3.</summary>
    public static DoorDefinition LandingSite3 { get; } = DoorDefinitions.Find(nameof(LandingSite3));

    /// <summary>The Gauntlet Entrance room's door 0.</summary>
    public static DoorDefinition GauntletEntrance0 { get; } = DoorDefinitions.Find(nameof(GauntletEntrance0));

    /// <summary>The Gauntlet Entrance room's door 1.</summary>
    public static DoorDefinition GauntletEntrance1 { get; } = DoorDefinitions.Find(nameof(GauntletEntrance1));

    /// <summary>The Parlor room's door 0.</summary>
    public static DoorDefinition Parlor0 { get; } = DoorDefinitions.Find(nameof(Parlor0));

    /// <summary>The Parlor room's door 1.</summary>
    public static DoorDefinition Parlor1 { get; } = DoorDefinitions.Find(nameof(Parlor1));

    /// <summary>The Parlor room's door 2.</summary>
    public static DoorDefinition Parlor2 { get; } = DoorDefinitions.Find(nameof(Parlor2));

    /// <summary>The Parlor room's door 3.</summary>
    public static DoorDefinition Parlor3 { get; } = DoorDefinitions.Find(nameof(Parlor3));

    /// <summary>The Parlor room's door 4.</summary>
    public static DoorDefinition Parlor4 { get; } = DoorDefinitions.Find(nameof(Parlor4));

    /// <summary>The Parlor room's door 5.</summary>
    public static DoorDefinition Parlor5 { get; } = DoorDefinitions.Find(nameof(Parlor5));

    /// <summary>The Parlor room's door 6.</summary>
    public static DoorDefinition Parlor6 { get; } = DoorDefinitions.Find(nameof(Parlor6));

    /// <summary>The Crateria Power Bombs room's door 0.</summary>
    public static DoorDefinition CrateriaPowerBombs0 { get; } = DoorDefinitions.Find(nameof(CrateriaPowerBombs0));

    /// <summary>The Crateria Save room's door 0.</summary>
    public static DoorDefinition CrateriaSave0 { get; } = DoorDefinitions.Find(nameof(CrateriaSave0));

    /// <summary>The West Ocean room's door 0.</summary>
    public static DoorDefinition WestOcean0 { get; } = DoorDefinitions.Find(nameof(WestOcean0));

    /// <summary>The West Ocean room's door 1.</summary>
    public static DoorDefinition WestOcean1 { get; } = DoorDefinitions.Find(nameof(WestOcean1));

    /// <summary>The West Ocean room's door 2.</summary>
    public static DoorDefinition WestOcean2 { get; } = DoorDefinitions.Find(nameof(WestOcean2));

    /// <summary>The West Ocean room's door 3.</summary>
    public static DoorDefinition WestOcean3 { get; } = DoorDefinitions.Find(nameof(WestOcean3));

    /// <summary>The West Ocean room's door 4.</summary>
    public static DoorDefinition WestOcean4 { get; } = DoorDefinitions.Find(nameof(WestOcean4));

    /// <summary>The West Ocean room's door 5.</summary>
    public static DoorDefinition WestOcean5 { get; } = DoorDefinitions.Find(nameof(WestOcean5));

    /// <summary>The Bowling Alley Path room's door 0.</summary>
    public static DoorDefinition BowlingAlleyPath0 { get; } = DoorDefinitions.Find(nameof(BowlingAlleyPath0));

    /// <summary>The Bowling Alley Path room's door 1.</summary>
    public static DoorDefinition BowlingAlleyPath1 { get; } = DoorDefinitions.Find(nameof(BowlingAlleyPath1));

    /// <summary>The Crateria Kihunter room's door 0.</summary>
    public static DoorDefinition CrateriaKihunter0 { get; } = DoorDefinitions.Find(nameof(CrateriaKihunter0));

    /// <summary>The Crateria Kihunter room's door 1.</summary>
    public static DoorDefinition CrateriaKihunter1 { get; } = DoorDefinitions.Find(nameof(CrateriaKihunter1));

    /// <summary>The Crateria Kihunter room's door 2.</summary>
    public static DoorDefinition CrateriaKihunter2 { get; } = DoorDefinitions.Find(nameof(CrateriaKihunter2));

    /// <summary>The Forgotten Highway Elevator room's door 0.</summary>
    public static DoorDefinition ForgottenHighwayElev0 { get; } = DoorDefinitions.Find(nameof(ForgottenHighwayElev0));

    /// <summary>The Forgotten Highway Elevator room's door 1.</summary>
    public static DoorDefinition ForgottenHighwayElev1 { get; } = DoorDefinitions.Find(nameof(ForgottenHighwayElev1));

    /// <summary>The East Ocean room's door 0.</summary>
    public static DoorDefinition EastOcean0 { get; } = DoorDefinitions.Find(nameof(EastOcean0));

    /// <summary>The East Ocean room's door 1.</summary>
    public static DoorDefinition EastOcean1 { get; } = DoorDefinitions.Find(nameof(EastOcean1));

    /// <summary>The Forgotten Highway Kagos room's door 0.</summary>
    public static DoorDefinition ForgottenHighwayKagos0 { get; } = DoorDefinitions.Find(nameof(ForgottenHighwayKagos0));

    /// <summary>The Forgotten Highway Kagos room's door 1.</summary>
    public static DoorDefinition ForgottenHighwayKagos1 { get; } = DoorDefinitions.Find(nameof(ForgottenHighwayKagos1));

    /// <summary>The Crab Maze room's door 0.</summary>
    public static DoorDefinition CrabMaze0 { get; } = DoorDefinitions.Find(nameof(CrabMaze0));

    /// <summary>The Forgotten Highway Elbow room's door 0.</summary>
    public static DoorDefinition ForgottenHighwayElbow0 { get; } = DoorDefinitions.Find(nameof(ForgottenHighwayElbow0));

    /// <summary>The Crab Maze room's door 1.</summary>
    public static DoorDefinition CrabMaze1 { get; } = DoorDefinitions.Find(nameof(CrabMaze1));

    /// <summary>The Forgotten Highway Elbow room's door 1.</summary>
    public static DoorDefinition ForgottenHighwayElbow1 { get; } = DoorDefinitions.Find(nameof(ForgottenHighwayElbow1));

    /// <summary>The Crateria Tube room's door 0.</summary>
    public static DoorDefinition CrateriaTube0 { get; } = DoorDefinitions.Find(nameof(CrateriaTube0));

    /// <summary>The Crateria Tube room's door 1.</summary>
    public static DoorDefinition CrateriaTube1 { get; } = DoorDefinitions.Find(nameof(CrateriaTube1));

    /// <summary>The Moat room's door 0.</summary>
    public static DoorDefinition Moat0 { get; } = DoorDefinitions.Find(nameof(Moat0));

    /// <summary>The Moat room's door 1.</summary>
    public static DoorDefinition Moat1 { get; } = DoorDefinitions.Find(nameof(Moat1));

    /// <summary>The Red Brinstar Elevator room's door 0.</summary>
    public static DoorDefinition RedBinstarElev0 { get; } = DoorDefinitions.Find(nameof(RedBinstarElev0));

    /// <summary>The Red Brinstar Elevator room's door 1.</summary>
    public static DoorDefinition RedBinstarElev1 { get; } = DoorDefinitions.Find(nameof(RedBinstarElev1));

    /// <summary>The Gauntlet Energy Tank room's door 0.</summary>
    public static DoorDefinition GauntletETank0 { get; } = DoorDefinitions.Find(nameof(GauntletETank0));

    /// <summary>The Gauntlet Energy Tank room's door 1.</summary>
    public static DoorDefinition GauntletETank1 { get; } = DoorDefinitions.Find(nameof(GauntletETank1));

    /// <summary>The Pre Bowling room's door 0.</summary>
    public static DoorDefinition PreBowling0 { get; } = DoorDefinitions.Find(nameof(PreBowling0));

    /// <summary>The Pre Bowling room's door 1.</summary>
    public static DoorDefinition PreBowling1 { get; } = DoorDefinitions.Find(nameof(PreBowling1));

    /// <summary>The Climb room's door 0.</summary>
    public static DoorDefinition Climb0 { get; } = DoorDefinitions.Find(nameof(Climb0));

    /// <summary>The Climb room's door 1.</summary>
    public static DoorDefinition Climb1 { get; } = DoorDefinitions.Find(nameof(Climb1));

    /// <summary>The Climb room's door 2.</summary>
    public static DoorDefinition Climb2 { get; } = DoorDefinitions.Find(nameof(Climb2));

    /// <summary>The Climb room's door 3.</summary>
    public static DoorDefinition Climb3 { get; } = DoorDefinitions.Find(nameof(Climb3));

    /// <summary>The Climb room's door 4.</summary>
    public static DoorDefinition Climb4 { get; } = DoorDefinitions.Find(nameof(Climb4));

    /// <summary>The Pit room's door 0.</summary>
    public static DoorDefinition Pit0 { get; } = DoorDefinitions.Find(nameof(Pit0));

    /// <summary>The Pit room's door 1.</summary>
    public static DoorDefinition Pit1 { get; } = DoorDefinitions.Find(nameof(Pit1));

    /// <summary>The Elevator to Blue Brinstar room's door 0.</summary>
    public static DoorDefinition ElevToBlueBrinstar0 { get; } = DoorDefinitions.Find(nameof(ElevToBlueBrinstar0));

    /// <summary>The Elevator to Blue Brinstar room's door 1.</summary>
    public static DoorDefinition ElevToBlueBrinstar1 { get; } = DoorDefinitions.Find(nameof(ElevToBlueBrinstar1));

    /// <summary>The Bomb Torizo room's door 0.</summary>
    public static DoorDefinition BombTorizo0 { get; } = DoorDefinitions.Find(nameof(BombTorizo0));

    /// <summary>The Flyway room's door 0.</summary>
    public static DoorDefinition Flyway0 { get; } = DoorDefinitions.Find(nameof(Flyway0));

    /// <summary>The Flyway room's door 1.</summary>
    public static DoorDefinition Flyway1 { get; } = DoorDefinitions.Find(nameof(Flyway1));

    /// <summary>The Pre Map Flyway room's door 0.</summary>
    public static DoorDefinition PreMapFlyway0 { get; } = DoorDefinitions.Find(nameof(PreMapFlyway0));

    /// <summary>The Pre Map Flyway room's door 1.</summary>
    public static DoorDefinition PreMapFlyway1 { get; } = DoorDefinitions.Find(nameof(PreMapFlyway1));

    /// <summary>The Terminator room's door 0.</summary>
    public static DoorDefinition Terminator0 { get; } = DoorDefinitions.Find(nameof(Terminator0));

    /// <summary>The Terminator room's door 1.</summary>
    public static DoorDefinition Terminator1 { get; } = DoorDefinitions.Find(nameof(Terminator1));

    /// <summary>The Green Brinstar Elevator room's door 0.</summary>
    public static DoorDefinition GreenBrinstarElev0 { get; } = DoorDefinitions.Find(nameof(GreenBrinstarElev0));

    /// <summary>The Green Brinstar Elevator room's door 1.</summary>
    public static DoorDefinition GreenBrinstarElev1 { get; } = DoorDefinitions.Find(nameof(GreenBrinstarElev1));

    /// <summary>The Lower Mushrooms room's door 0.</summary>
    public static DoorDefinition LowerMushrooms0 { get; } = DoorDefinitions.Find(nameof(LowerMushrooms0));

    /// <summary>The Lower Mushrooms room's door 1.</summary>
    public static DoorDefinition LowerMushrooms1 { get; } = DoorDefinitions.Find(nameof(LowerMushrooms1));

    /// <summary>The Crateria Map room's door 0.</summary>
    public static DoorDefinition CrateriaMap0 { get; } = DoorDefinitions.Find(nameof(CrateriaMap0));

    /// <summary>The Green Pirates Shaft room's door 0.</summary>
    public static DoorDefinition GreenPiratesShaft0 { get; } = DoorDefinitions.Find(nameof(GreenPiratesShaft0));

    /// <summary>The Green Pirates Shaft room's door 1.</summary>
    public static DoorDefinition GreenPiratesShaft1 { get; } = DoorDefinitions.Find(nameof(GreenPiratesShaft1));

    /// <summary>The Green Pirates Shaft room's door 2.</summary>
    public static DoorDefinition GreenPiratesShaft2 { get; } = DoorDefinitions.Find(nameof(GreenPiratesShaft2));

    /// <summary>The Green Pirates Shaft room's door 3.</summary>
    public static DoorDefinition GreenPiratesShaft3 { get; } = DoorDefinitions.Find(nameof(GreenPiratesShaft3));

    /// <summary>The Crateria Super room's door 0.</summary>
    public static DoorDefinition CrateriaSuper0 { get; } = DoorDefinitions.Find(nameof(CrateriaSuper0));

    /// <summary>The Crateria Super room's door 1.</summary>
    public static DoorDefinition CrateriaSuper1 { get; } = DoorDefinitions.Find(nameof(CrateriaSuper1));

    /// <summary>The Final Missile Bombway room's door 0.</summary>
    public static DoorDefinition FinalMissileBombway0 { get; } = DoorDefinitions.Find(nameof(FinalMissileBombway0));

    /// <summary>The Final Missile Bombway room's door 1.</summary>
    public static DoorDefinition FinalMissileBombway1 { get; } = DoorDefinitions.Find(nameof(FinalMissileBombway1));

    /// <summary>The Final Missile room's door 0.</summary>
    public static DoorDefinition FinalMissile0 { get; } = DoorDefinitions.Find(nameof(FinalMissile0));

    /// <summary>The Statues Hallway room's door 0.</summary>
    public static DoorDefinition StatuesHallway0 { get; } = DoorDefinitions.Find(nameof(StatuesHallway0));

    /// <summary>The Statues Hallway room's door 1.</summary>
    public static DoorDefinition StatuesHallway1 { get; } = DoorDefinitions.Find(nameof(StatuesHallway1));

    /// <summary>The Statues room's door 0.</summary>
    public static DoorDefinition Statues0 { get; } = DoorDefinitions.Find(nameof(Statues0));

    /// <summary>The Statues room's door 1.</summary>
    public static DoorDefinition Statues1 { get; } = DoorDefinitions.Find(nameof(Statues1));
}
