namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The Crateria rooms' doors, named by the room they lead from.</summary>
[NumberedIdentities("Each numbered door is one door of its room, named by its place in that room's door list; every door has its own destination and is used by name, never iterated or indexed.")]
public static class CrateriaDoorDefinitions
{

    /// <summary>The <c>Door_LandingSite_LandingCutscene</c> door.</summary>
    public static DoorDefinition LandingSiteLandingCutscene { get; } = DoorDefinitions.Find(nameof(LandingSiteLandingCutscene));

    /// <summary>The <c>Door_LandingSite_0</c> door.</summary>
    public static DoorDefinition LandingSite0 { get; } = DoorDefinitions.Find(nameof(LandingSite0));

    /// <summary>The <c>Door_LandingSite_1</c> door.</summary>
    public static DoorDefinition LandingSite1 { get; } = DoorDefinitions.Find(nameof(LandingSite1));

    /// <summary>The <c>Door_LandingSite_2</c> door.</summary>
    public static DoorDefinition LandingSite2 { get; } = DoorDefinitions.Find(nameof(LandingSite2));

    /// <summary>The <c>Door_LandingSite_3</c> door.</summary>
    public static DoorDefinition LandingSite3 { get; } = DoorDefinitions.Find(nameof(LandingSite3));

    /// <summary>The <c>Door_GauntletEntrance_0</c> door.</summary>
    public static DoorDefinition GauntletEntrance0 { get; } = DoorDefinitions.Find(nameof(GauntletEntrance0));

    /// <summary>The <c>Door_GauntletEntrance_1</c> door.</summary>
    public static DoorDefinition GauntletEntrance1 { get; } = DoorDefinitions.Find(nameof(GauntletEntrance1));

    /// <summary>The <c>Door_Parlor_0</c> door.</summary>
    public static DoorDefinition Parlor0 { get; } = DoorDefinitions.Find(nameof(Parlor0));

    /// <summary>The <c>Door_Parlor_1</c> door.</summary>
    public static DoorDefinition Parlor1 { get; } = DoorDefinitions.Find(nameof(Parlor1));

    /// <summary>The <c>Door_Parlor_2</c> door.</summary>
    public static DoorDefinition Parlor2 { get; } = DoorDefinitions.Find(nameof(Parlor2));

    /// <summary>The <c>Door_Parlor_3</c> door.</summary>
    public static DoorDefinition Parlor3 { get; } = DoorDefinitions.Find(nameof(Parlor3));

    /// <summary>The <c>Door_Parlor_4</c> door.</summary>
    public static DoorDefinition Parlor4 { get; } = DoorDefinitions.Find(nameof(Parlor4));

    /// <summary>The <c>Door_Parlor_5</c> door.</summary>
    public static DoorDefinition Parlor5 { get; } = DoorDefinitions.Find(nameof(Parlor5));

    /// <summary>The <c>Door_Parlor_6</c> door.</summary>
    public static DoorDefinition Parlor6 { get; } = DoorDefinitions.Find(nameof(Parlor6));

    /// <summary>The <c>Door_CrateriaPowerBombs_0</c> door.</summary>
    public static DoorDefinition CrateriaPowerBombs0 { get; } = DoorDefinitions.Find(nameof(CrateriaPowerBombs0));

    /// <summary>The <c>Door_CrateriaSave_0</c> door.</summary>
    public static DoorDefinition CrateriaSave0 { get; } = DoorDefinitions.Find(nameof(CrateriaSave0));

    /// <summary>The <c>Door_WestOcean_0</c> door.</summary>
    public static DoorDefinition WestOcean0 { get; } = DoorDefinitions.Find(nameof(WestOcean0));

    /// <summary>The <c>Door_WestOcean_1</c> door.</summary>
    public static DoorDefinition WestOcean1 { get; } = DoorDefinitions.Find(nameof(WestOcean1));

    /// <summary>The <c>Door_WestOcean_2</c> door.</summary>
    public static DoorDefinition WestOcean2 { get; } = DoorDefinitions.Find(nameof(WestOcean2));

    /// <summary>The <c>Door_WestOcean_3</c> door.</summary>
    public static DoorDefinition WestOcean3 { get; } = DoorDefinitions.Find(nameof(WestOcean3));

    /// <summary>The <c>Door_WestOcean_4</c> door.</summary>
    public static DoorDefinition WestOcean4 { get; } = DoorDefinitions.Find(nameof(WestOcean4));

    /// <summary>The <c>Door_WestOcean_5</c> door.</summary>
    public static DoorDefinition WestOcean5 { get; } = DoorDefinitions.Find(nameof(WestOcean5));

    /// <summary>The <c>Door_BowlingAlleyPath_0</c> door.</summary>
    public static DoorDefinition BowlingAlleyPath0 { get; } = DoorDefinitions.Find(nameof(BowlingAlleyPath0));

    /// <summary>The <c>Door_BowlingAlleyPath_1</c> door.</summary>
    public static DoorDefinition BowlingAlleyPath1 { get; } = DoorDefinitions.Find(nameof(BowlingAlleyPath1));

    /// <summary>The <c>Door_CrateriaKihunter_0</c> door.</summary>
    public static DoorDefinition CrateriaKihunter0 { get; } = DoorDefinitions.Find(nameof(CrateriaKihunter0));

    /// <summary>The <c>Door_CrateriaKihunter_1</c> door.</summary>
    public static DoorDefinition CrateriaKihunter1 { get; } = DoorDefinitions.Find(nameof(CrateriaKihunter1));

    /// <summary>The <c>Door_CrateriaKihunter_2</c> door.</summary>
    public static DoorDefinition CrateriaKihunter2 { get; } = DoorDefinitions.Find(nameof(CrateriaKihunter2));

    /// <summary>The <c>Door_ForgottenHighwayElev_0</c> door.</summary>
    public static DoorDefinition ForgottenHighwayElev0 { get; } = DoorDefinitions.Find(nameof(ForgottenHighwayElev0));

    /// <summary>The <c>Door_ForgottenHighwayElev_1</c> door.</summary>
    public static DoorDefinition ForgottenHighwayElev1 { get; } = DoorDefinitions.Find(nameof(ForgottenHighwayElev1));

    /// <summary>The <c>Door_EastOcean_0</c> door.</summary>
    public static DoorDefinition EastOcean0 { get; } = DoorDefinitions.Find(nameof(EastOcean0));

    /// <summary>The <c>Door_EastOcean_1</c> door.</summary>
    public static DoorDefinition EastOcean1 { get; } = DoorDefinitions.Find(nameof(EastOcean1));

    /// <summary>The <c>Door_ForgottenHighwayKagos_0</c> door.</summary>
    public static DoorDefinition ForgottenHighwayKagos0 { get; } = DoorDefinitions.Find(nameof(ForgottenHighwayKagos0));

    /// <summary>The <c>Door_ForgottenHighwayKagos_1</c> door.</summary>
    public static DoorDefinition ForgottenHighwayKagos1 { get; } = DoorDefinitions.Find(nameof(ForgottenHighwayKagos1));

    /// <summary>The <c>Door_CrabMaze_0</c> door.</summary>
    public static DoorDefinition CrabMaze0 { get; } = DoorDefinitions.Find(nameof(CrabMaze0));

    /// <summary>The <c>Door_ForgottenHighwayElbow_0</c> door.</summary>
    public static DoorDefinition ForgottenHighwayElbow0 { get; } = DoorDefinitions.Find(nameof(ForgottenHighwayElbow0));

    /// <summary>The <c>Door_CrabMaze_1</c> door.</summary>
    public static DoorDefinition CrabMaze1 { get; } = DoorDefinitions.Find(nameof(CrabMaze1));

    /// <summary>The <c>Door_ForgottenHighwayElbow_1</c> door.</summary>
    public static DoorDefinition ForgottenHighwayElbow1 { get; } = DoorDefinitions.Find(nameof(ForgottenHighwayElbow1));

    /// <summary>The <c>Door_CrateriaTube_0</c> door.</summary>
    public static DoorDefinition CrateriaTube0 { get; } = DoorDefinitions.Find(nameof(CrateriaTube0));

    /// <summary>The <c>Door_CrateriaTube_1</c> door.</summary>
    public static DoorDefinition CrateriaTube1 { get; } = DoorDefinitions.Find(nameof(CrateriaTube1));

    /// <summary>The <c>Door_Moat_0</c> door.</summary>
    public static DoorDefinition Moat0 { get; } = DoorDefinitions.Find(nameof(Moat0));

    /// <summary>The <c>Door_Moat_1</c> door.</summary>
    public static DoorDefinition Moat1 { get; } = DoorDefinitions.Find(nameof(Moat1));

    /// <summary>The <c>Door_RedBinstarElev_0</c> door.</summary>
    public static DoorDefinition RedBinstarElev0 { get; } = DoorDefinitions.Find(nameof(RedBinstarElev0));

    /// <summary>The <c>Door_RedBinstarElev_1</c> door.</summary>
    public static DoorDefinition RedBinstarElev1 { get; } = DoorDefinitions.Find(nameof(RedBinstarElev1));

    /// <summary>The <c>Door_GauntletETank_0</c> door.</summary>
    public static DoorDefinition GauntletETank0 { get; } = DoorDefinitions.Find(nameof(GauntletETank0));

    /// <summary>The <c>Door_GauntletETank_1</c> door.</summary>
    public static DoorDefinition GauntletETank1 { get; } = DoorDefinitions.Find(nameof(GauntletETank1));

    /// <summary>The <c>Door_PreBowling_0</c> door.</summary>
    public static DoorDefinition PreBowling0 { get; } = DoorDefinitions.Find(nameof(PreBowling0));

    /// <summary>The <c>Door_PreBowling_1</c> door.</summary>
    public static DoorDefinition PreBowling1 { get; } = DoorDefinitions.Find(nameof(PreBowling1));

    /// <summary>The <c>Door_Climb_0</c> door.</summary>
    public static DoorDefinition Climb0 { get; } = DoorDefinitions.Find(nameof(Climb0));

    /// <summary>The <c>Door_Climb_1</c> door.</summary>
    public static DoorDefinition Climb1 { get; } = DoorDefinitions.Find(nameof(Climb1));

    /// <summary>The <c>Door_Climb_2</c> door.</summary>
    public static DoorDefinition Climb2 { get; } = DoorDefinitions.Find(nameof(Climb2));

    /// <summary>The <c>Door_Climb_3</c> door.</summary>
    public static DoorDefinition Climb3 { get; } = DoorDefinitions.Find(nameof(Climb3));

    /// <summary>The <c>Door_Climb_4</c> door.</summary>
    public static DoorDefinition Climb4 { get; } = DoorDefinitions.Find(nameof(Climb4));

    /// <summary>The <c>Door_Pit_0</c> door.</summary>
    public static DoorDefinition Pit0 { get; } = DoorDefinitions.Find(nameof(Pit0));

    /// <summary>The <c>Door_Pit_1</c> door.</summary>
    public static DoorDefinition Pit1 { get; } = DoorDefinitions.Find(nameof(Pit1));

    /// <summary>The <c>Door_ElevToBlueBrinstar_0</c> door.</summary>
    public static DoorDefinition ElevToBlueBrinstar0 { get; } = DoorDefinitions.Find(nameof(ElevToBlueBrinstar0));

    /// <summary>The <c>Door_ElevToBlueBrinstar_1</c> door.</summary>
    public static DoorDefinition ElevToBlueBrinstar1 { get; } = DoorDefinitions.Find(nameof(ElevToBlueBrinstar1));

    /// <summary>The <c>Door_BombTorizo_0</c> door.</summary>
    public static DoorDefinition BombTorizo0 { get; } = DoorDefinitions.Find(nameof(BombTorizo0));

    /// <summary>The <c>Door_Flyway_0</c> door.</summary>
    public static DoorDefinition Flyway0 { get; } = DoorDefinitions.Find(nameof(Flyway0));

    /// <summary>The <c>Door_Flyway_1</c> door.</summary>
    public static DoorDefinition Flyway1 { get; } = DoorDefinitions.Find(nameof(Flyway1));

    /// <summary>The <c>Door_PreMapFlyway_0</c> door.</summary>
    public static DoorDefinition PreMapFlyway0 { get; } = DoorDefinitions.Find(nameof(PreMapFlyway0));

    /// <summary>The <c>Door_PreMapFlyway_1</c> door.</summary>
    public static DoorDefinition PreMapFlyway1 { get; } = DoorDefinitions.Find(nameof(PreMapFlyway1));

    /// <summary>The <c>Door_Terminator_0</c> door.</summary>
    public static DoorDefinition Terminator0 { get; } = DoorDefinitions.Find(nameof(Terminator0));

    /// <summary>The <c>Door_Terminator_1</c> door.</summary>
    public static DoorDefinition Terminator1 { get; } = DoorDefinitions.Find(nameof(Terminator1));

    /// <summary>The <c>Door_GreenBrinstarElev_0</c> door.</summary>
    public static DoorDefinition GreenBrinstarElev0 { get; } = DoorDefinitions.Find(nameof(GreenBrinstarElev0));

    /// <summary>The <c>Door_GreenBrinstarElev_1</c> door.</summary>
    public static DoorDefinition GreenBrinstarElev1 { get; } = DoorDefinitions.Find(nameof(GreenBrinstarElev1));

    /// <summary>The <c>Door_LowerMushrooms_0</c> door.</summary>
    public static DoorDefinition LowerMushrooms0 { get; } = DoorDefinitions.Find(nameof(LowerMushrooms0));

    /// <summary>The <c>Door_LowerMushrooms_1</c> door.</summary>
    public static DoorDefinition LowerMushrooms1 { get; } = DoorDefinitions.Find(nameof(LowerMushrooms1));

    /// <summary>The <c>Door_CrateriaMap_0</c> door.</summary>
    public static DoorDefinition CrateriaMap0 { get; } = DoorDefinitions.Find(nameof(CrateriaMap0));

    /// <summary>The <c>Door_GreenPiratesShaft_0</c> door.</summary>
    public static DoorDefinition GreenPiratesShaft0 { get; } = DoorDefinitions.Find(nameof(GreenPiratesShaft0));

    /// <summary>The <c>Door_GreenPiratesShaft_1</c> door.</summary>
    public static DoorDefinition GreenPiratesShaft1 { get; } = DoorDefinitions.Find(nameof(GreenPiratesShaft1));

    /// <summary>The <c>Door_GreenPiratesShaft_2</c> door.</summary>
    public static DoorDefinition GreenPiratesShaft2 { get; } = DoorDefinitions.Find(nameof(GreenPiratesShaft2));

    /// <summary>The <c>Door_GreenPiratesShaft_3</c> door.</summary>
    public static DoorDefinition GreenPiratesShaft3 { get; } = DoorDefinitions.Find(nameof(GreenPiratesShaft3));

    /// <summary>The <c>Door_CrateriaSuper_0</c> door.</summary>
    public static DoorDefinition CrateriaSuper0 { get; } = DoorDefinitions.Find(nameof(CrateriaSuper0));

    /// <summary>The <c>Door_CrateriaSuper_1</c> door.</summary>
    public static DoorDefinition CrateriaSuper1 { get; } = DoorDefinitions.Find(nameof(CrateriaSuper1));

    /// <summary>The <c>Door_FinalMissileBombway_0</c> door.</summary>
    public static DoorDefinition FinalMissileBombway0 { get; } = DoorDefinitions.Find(nameof(FinalMissileBombway0));

    /// <summary>The <c>Door_FinalMissileBombway_1</c> door.</summary>
    public static DoorDefinition FinalMissileBombway1 { get; } = DoorDefinitions.Find(nameof(FinalMissileBombway1));

    /// <summary>The <c>Door_FinalMissile_0</c> door.</summary>
    public static DoorDefinition FinalMissile0 { get; } = DoorDefinitions.Find(nameof(FinalMissile0));

    /// <summary>The <c>Door_StatuesHallway_0</c> door.</summary>
    public static DoorDefinition StatuesHallway0 { get; } = DoorDefinitions.Find(nameof(StatuesHallway0));

    /// <summary>The <c>Door_StatuesHallway_1</c> door.</summary>
    public static DoorDefinition StatuesHallway1 { get; } = DoorDefinitions.Find(nameof(StatuesHallway1));

    /// <summary>The <c>Door_Statues_0</c> door.</summary>
    public static DoorDefinition Statues0 { get; } = DoorDefinitions.Find(nameof(Statues0));

    /// <summary>The <c>Door_Statues_1</c> door.</summary>
    public static DoorDefinition Statues1 { get; } = DoorDefinitions.Find(nameof(Statues1));
}
