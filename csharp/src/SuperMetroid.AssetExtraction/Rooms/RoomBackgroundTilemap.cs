namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The tilemaps a room's library background decompresses as its room loads, each named for the background it
/// pictures, as the disassembly labels it.
/// </summary>
public enum RoomBackgroundTilemap
{
    /// <summary>Norfair's small pattern brown purple bright background tilemap: <c>Background_Norfair_9_A_SmallPatternBrownPurple_Bright</c>.</summary>
    NorfairSmallPatternBrownPurpleBright = 1,

    /// <summary>Norfair's small pattern brown purple (variant 0) background tilemap: <c>Background_Norfair_9_A_SmallPatternBrownPurple_0</c>.</summary>
    NorfairSmallPatternBrownPurple0,

    /// <summary>Norfair's horizontal pattern brick background tilemap: <c>Background_Norfair_9_HorizontalPatternBrick</c>.</summary>
    NorfairHorizontalPatternBrick,

    /// <summary>Norfair's vertical pattern brick background tilemap: <c>Background_Norfair_9_VerticalPatternBrick</c>.</summary>
    NorfairVerticalPatternBrick,

    /// <summary>Norfair's cavern stalactites background tilemap: <c>Background_Norfair_9_A_CavernStalactites</c>.</summary>
    NorfairCavernStalactites,

    /// <summary>Norfair's cavern vertical background tilemap: <c>Background_Norfair_9_A_CavernVertical</c>.</summary>
    NorfairCavernVertical,

    /// <summary>Norfair's cavern horizontal ruins background tilemap: <c>Background_Norfair_9_CavernHorizontalRuins</c>.</summary>
    NorfairCavernHorizontalRuins,

    /// <summary>Norfair's cavern vertical ruins background tilemap: <c>Background_Norfair_9_CavernVerticalRuins</c>.</summary>
    NorfairCavernVerticalRuins,

    /// <summary>Norfair's cavern room pillars background tilemap: <c>Background_Norfair_9_CavernRoomPillars</c>.</summary>
    NorfairCavernRoomPillars,

    /// <summary>Norfair's room statues background tilemap: <c>Background_Norfair_9_RoomStatues</c>.</summary>
    NorfairRoomStatues,

    /// <summary>Norfair's purple patches background tilemap: <c>Background_Norfair_9_PurplePatches</c>.</summary>
    NorfairPurplePatches,

    /// <summary>Norfair's bubbles background tilemap: <c>Background_Norfair_9_Bubbles</c>.</summary>
    NorfairBubbles,

    /// <summary>Brinstar's vertical glow patches background tilemap: <c>Background_Brinstar_6_Vertical_GlowPatches</c>.</summary>
    BrinstarVerticalGlowPatches,

    /// <summary>Brinstar's horizontal glow patches background tilemap: <c>Background_Brinstar_6_Horizontal_GlowPatches</c>.</summary>
    BrinstarHorizontalGlowPatches,

    /// <summary>Brinstar's large horizontal pattern background tilemap: <c>Background_Brinstar_6_LargeHorizontalPattern</c>.</summary>
    BrinstarLargeHorizontalPattern,

    /// <summary>Brinstar's medium horizontal pattern background tilemap: <c>Background_Brinstar_6_MediumHorizontalPattern</c>.</summary>
    BrinstarMediumHorizontalPattern,

    /// <summary>Brinstar's thin horizontal pattern background tilemap: <c>Background_Brinstar_6_ThinHorizontalPattern</c>.</summary>
    BrinstarThinHorizontalPattern,

    /// <summary>Brinstar's small pattern background tilemap: <c>Background_Brinstar_6_SmallPattern</c>.</summary>
    BrinstarSmallPattern,

    /// <summary>Brinstar's spheres pattern background tilemap: <c>Background_Brinstar_6_SpheresPattern</c>.</summary>
    BrinstarSpheresPattern,

    /// <summary>Brinstar's small pattern variety (variant 0) background tilemap: <c>Background_Brinstar_6_SmallPattern_Variety_0</c>.</summary>
    BrinstarSmallPatternVariety0,

    /// <summary>Brinstar's dark pattern background tilemap: <c>Background_Brinstar_6_DarkPattern</c>.</summary>
    BrinstarDarkPattern,

    /// <summary>Brinstar's wide vertical tower brick (variant 0) background tilemap: <c>Background_Brinstar_7_WideVerticalTower_Brick_0</c>.</summary>
    BrinstarWideVerticalTowerBrick0,

    /// <summary>Brinstar's vertical tower background tilemap: <c>Background_Brinstar_7_VerticalTower</c>.</summary>
    BrinstarVerticalTower,

    /// <summary>Brinstar's narrow vertical tower brick background tilemap: <c>Background_Brinstar_7_NarrowVerticalTower_Brick</c>.</summary>
    BrinstarNarrowVerticalTowerBrick,

    /// <summary>Brinstar's vertical brick (variant 0) background tilemap: <c>Background_Brinstar_7_VerticalBrick_0</c>.</summary>
    BrinstarVerticalBrick0,

    /// <summary>Brinstar's mechanical room spike floor background tilemap: <c>Background_Brinstar_7_MechanicalRoom_SpikeFloor</c>.</summary>
    BrinstarMechanicalRoomSpikeFloor,

    /// <summary>Brinstar's mechanical room background tilemap: <c>Background_Brinstar_7_MechanicalRoom</c>.</summary>
    BrinstarMechanicalRoom,

    /// <summary>Brinstar's narrow vertical tower brick vines (variant 0) background tilemap: <c>Background_Brinstar_7_NarrowVerticalTower_Brick_Vines_0</c>.</summary>
    BrinstarNarrowVerticalTowerBrickVines0,

    /// <summary>Brinstar's narrow vertical tower brick grey (variant 0) background tilemap: <c>Background_Brinstar_8_NarrowVerticalTower_Brick_Grey_0</c>.</summary>
    BrinstarNarrowVerticalTowerBrickGrey0,

    /// <summary>Brinstar's blue grid blocks background tilemap: <c>Background_Brinstar_7_BlueGridBlocks</c>.</summary>
    BrinstarBlueGridBlocks,

    /// <summary>Brinstar's kraid upper background tilemap: <c>Background_Brinstar_1A_Kraid_Upper</c>.</summary>
    BrinstarKraidUpper,

    /// <summary>Brinstar's kraid lower (variant 0) background tilemap: <c>Background_Brinstar_1A_Kraid_Lower_0</c>.</summary>
    BrinstarKraidLower0,

    /// <summary>Crateria's vertical pattern rocks background tilemap: <c>Background_Crateria_0_VerticalPatternRocks</c>.</summary>
    CrateriaVerticalPatternRocks,

    /// <summary>Crateria's horizontal pattern rocks background tilemap: <c>Background_Crateria_0_HorizontalPatternRocks</c>.</summary>
    CrateriaHorizontalPatternRocks,

    /// <summary>Crateria's rocks background tilemap: <c>Background_Crateria_0_Rocks</c>.</summary>
    CrateriaRocks,

    /// <summary>Crateria's purple rocks background tilemap: <c>Background_Crateria_2_PurpleRocks</c>.</summary>
    CrateriaPurpleRocks,

    /// <summary>Crateria's brick room wall art dark background tilemap: <c>Background_Crateria_2_BrickRoom_WallArt_Dark</c>.</summary>
    CrateriaBrickRoomWallArtDark,

    /// <summary>Crateria's mechanical background tilemap: <c>Background_Crateria_2_3_Mechanical</c>.</summary>
    CrateriaMechanical,

    /// <summary>Crateria's brick room wall art background tilemap: <c>Background_Crateria_2_BrickRoom_WallArt</c>.</summary>
    CrateriaBrickRoomWallArt,

    /// <summary>Crateria's elevator background tilemap: <c>Background_Crateria_2_Elevator</c>.</summary>
    CrateriaElevator,

    /// <summary>Crateria's brick room background tilemap: <c>Background_Crateria_2_BrickRoom</c>.</summary>
    CrateriaBrickRoom,

    /// <summary>The Wrecked Ship's entrance hall (variant 0) background tilemap: <c>Background_WreckedShip_4_5_EntranceHall_0</c>.</summary>
    WreckedShipEntranceHall0,

    /// <summary>The Wrecked Ship's columns tubes background tilemap: <c>Background_WreckedShip_4_5_Columns_Tubes</c>.</summary>
    WreckedShipColumnsTubes,

    /// <summary>The Wrecked Ship's experiment room background tilemap: <c>Background_WreckedShip_4_5_ExperimentRoom</c>.</summary>
    WreckedShipExperimentRoom,

    /// <summary>The Wrecked Ship's panels vents background tilemap: <c>Background_WreckedShip_4_5_Panels_Vents</c>.</summary>
    WreckedShipPanelsVents,

    /// <summary>The Wrecked Ship's panels vents horizontal background tilemap: <c>Background_WreckedShip_4_5_Panels_Vents_Horizontal</c>.</summary>
    WreckedShipPanelsVentsHorizontal,

    /// <summary>The Wrecked Ship's panels vents vertical background tilemap: <c>Background_WreckedShip_4_5_Panels_Vents_Vertical</c>.</summary>
    WreckedShipPanelsVentsVertical,

    /// <summary>Maridia's brown wall background tilemap: <c>Background_Maridia_B_BrownWall</c>.</summary>
    MaridiaBrownWall,

    /// <summary>Maridia's green wall background tilemap: <c>Background_Maridia_C_GreenWall</c>.</summary>
    MaridiaGreenWall,

    /// <summary>Tourian's mechanical room background tilemap: <c>Background_Tourian_D_MechanicalRoom</c>.</summary>
    TourianMechanicalRoom,

    /// <summary>Tourian's mechanical room letterbox background tilemap: <c>Background_Tourian_D_MechanicalRoom_Letterbox</c>.</summary>
    TourianMechanicalRoomLetterbox,

    /// <summary>Tourian's vertical structures background tilemap: <c>Background_Tourian_D_VerticalStructures</c>.</summary>
    TourianVerticalStructures,

    /// <summary>Tourian's pipe room letterbox background tilemap: <c>Background_Tourian_D_PipeRoom_Letterbox</c>.</summary>
    TourianPipeRoomLetterbox,

    /// <summary>Tourian's pipe room vertical separation background tilemap: <c>Background_Tourian_D_PipeRoom_VerticalSeparation</c>.</summary>
    TourianPipeRoomVerticalSeparation,

    /// <summary>Tourian's pipe room background tilemap: <c>Background_Tourian_D_PipeRoom</c>.</summary>
    TourianPipeRoom,

    /// <summary>Tourian's pipe room dark background tilemap: <c>Background_Tourian_E_PipeRoom_Dark</c>.</summary>
    TourianPipeRoomDark,

    /// <summary>Tourian's statue hall background tilemap: <c>Background_Tourian_D_E_StatueHall</c>.</summary>
    TourianStatueHall,

    /// <summary>Tourian's statues background tilemap: <c>Background_Tourian_15_Statues</c>.</summary>
    TourianStatues,
}
