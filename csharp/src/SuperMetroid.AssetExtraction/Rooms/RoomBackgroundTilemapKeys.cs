using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The installed key of each <see cref="RoomBackgroundTilemap"/> (#2520), written out so renaming a member never
/// changes a name the installation, an installed document or a save spells it by.
/// </summary>
internal static class RoomBackgroundTilemapKeys
{
    /// <summary>Each RoomBackgroundTilemap member's installed key.</summary>
    internal static InstalledKeys<RoomBackgroundTilemap> Installed { get; } = new(new Dictionary<RoomBackgroundTilemap, string>
    {
        [RoomBackgroundTilemap.NorfairSmallPatternBrownPurpleBright] = "NorfairSmallPatternBrownPurpleBright",
        [RoomBackgroundTilemap.NorfairSmallPatternBrownPurple] = "NorfairSmallPatternBrownPurple",
        [RoomBackgroundTilemap.NorfairVerticalPatternBrick] = "NorfairVerticalPatternBrick",
        [RoomBackgroundTilemap.NorfairCavernStalactites] = "NorfairCavernStalactites",
        [RoomBackgroundTilemap.NorfairCavernVertical] = "NorfairCavernVertical",
        [RoomBackgroundTilemap.NorfairCavernHorizontalRuins] = "NorfairCavernHorizontalRuins",
        [RoomBackgroundTilemap.NorfairCavernVerticalRuins] = "NorfairCavernVerticalRuins",
        [RoomBackgroundTilemap.NorfairCavernRoomPillars] = "NorfairCavernRoomPillars",
        [RoomBackgroundTilemap.NorfairRoomStatues] = "NorfairRoomStatues",
        [RoomBackgroundTilemap.NorfairPurplePatches] = "NorfairPurplePatches",
        [RoomBackgroundTilemap.NorfairBubbles] = "NorfairBubbles",
        [RoomBackgroundTilemap.BrinstarVerticalGlowPatches] = "BrinstarVerticalGlowPatches",
        [RoomBackgroundTilemap.BrinstarHorizontalGlowPatches] = "BrinstarHorizontalGlowPatches",
        [RoomBackgroundTilemap.BrinstarLargeHorizontalPattern] = "BrinstarLargeHorizontalPattern",
        [RoomBackgroundTilemap.BrinstarMediumHorizontalPattern] = "BrinstarMediumHorizontalPattern",
        [RoomBackgroundTilemap.BrinstarThinHorizontalPattern] = "BrinstarThinHorizontalPattern",
        [RoomBackgroundTilemap.BrinstarSmallPattern] = "BrinstarSmallPattern",
        [RoomBackgroundTilemap.BrinstarSpheresPattern] = "BrinstarSpheresPattern",
        [RoomBackgroundTilemap.BrinstarSmallPatternVariety] = "BrinstarSmallPatternVariety",
        [RoomBackgroundTilemap.BrinstarDarkPattern] = "BrinstarDarkPattern",
        [RoomBackgroundTilemap.BrinstarWideVerticalTowerBrick] = "BrinstarWideVerticalTowerBrick",
        [RoomBackgroundTilemap.BrinstarVerticalTower] = "BrinstarVerticalTower",
        [RoomBackgroundTilemap.BrinstarNarrowVerticalTowerBrick] = "BrinstarNarrowVerticalTowerBrick",
        [RoomBackgroundTilemap.BrinstarVerticalBrick] = "BrinstarVerticalBrick",
        [RoomBackgroundTilemap.BrinstarMechanicalRoomSpikeFloor] = "BrinstarMechanicalRoomSpikeFloor",
        [RoomBackgroundTilemap.BrinstarMechanicalRoom] = "BrinstarMechanicalRoom",
        [RoomBackgroundTilemap.BrinstarNarrowVerticalTowerBrickVines] = "BrinstarNarrowVerticalTowerBrickVines",
        [RoomBackgroundTilemap.BrinstarNarrowVerticalTowerBrickGrey] = "BrinstarNarrowVerticalTowerBrickGrey",
        [RoomBackgroundTilemap.BrinstarBlueGridBlocks] = "BrinstarBlueGridBlocks",
        [RoomBackgroundTilemap.BrinstarKraidUpper] = "BrinstarKraidUpper",
        [RoomBackgroundTilemap.BrinstarKraidLower] = "BrinstarKraidLower",
        [RoomBackgroundTilemap.CrateriaVerticalPatternRocks] = "CrateriaVerticalPatternRocks",
        [RoomBackgroundTilemap.CrateriaHorizontalPatternRocks] = "CrateriaHorizontalPatternRocks",
        [RoomBackgroundTilemap.CrateriaRocks] = "CrateriaRocks",
        [RoomBackgroundTilemap.CrateriaPurpleRocks] = "CrateriaPurpleRocks",
        [RoomBackgroundTilemap.CrateriaBrickRoomWallArtDark] = "CrateriaBrickRoomWallArtDark",
        [RoomBackgroundTilemap.CrateriaMechanical] = "CrateriaMechanical",
        [RoomBackgroundTilemap.CrateriaBrickRoomWallArt] = "CrateriaBrickRoomWallArt",
        [RoomBackgroundTilemap.CrateriaElevator] = "CrateriaElevator",
        [RoomBackgroundTilemap.CrateriaBrickRoom] = "CrateriaBrickRoom",
        [RoomBackgroundTilemap.WreckedShipEntranceHall] = "WreckedShipEntranceHall",
        [RoomBackgroundTilemap.WreckedShipColumnsTubes] = "WreckedShipColumnsTubes",
        [RoomBackgroundTilemap.WreckedShipExperimentRoom] = "WreckedShipExperimentRoom",
        [RoomBackgroundTilemap.WreckedShipPanelsVents] = "WreckedShipPanelsVents",
        [RoomBackgroundTilemap.WreckedShipPanelsVentsHorizontal] = "WreckedShipPanelsVentsHorizontal",
        [RoomBackgroundTilemap.WreckedShipPanelsVentsVertical] = "WreckedShipPanelsVentsVertical",
        [RoomBackgroundTilemap.MaridiaBrownWall] = "MaridiaBrownWall",
        [RoomBackgroundTilemap.MaridiaGreenWall] = "MaridiaGreenWall",
        [RoomBackgroundTilemap.TourianMechanicalRoom] = "TourianMechanicalRoom",
        [RoomBackgroundTilemap.TourianMechanicalRoomLetterbox] = "TourianMechanicalRoomLetterbox",
        [RoomBackgroundTilemap.TourianVerticalStructures] = "TourianVerticalStructures",
        [RoomBackgroundTilemap.TourianPipeRoomLetterbox] = "TourianPipeRoomLetterbox",
        [RoomBackgroundTilemap.TourianPipeRoomVerticalSeparation] = "TourianPipeRoomVerticalSeparation",
        [RoomBackgroundTilemap.TourianPipeRoom] = "TourianPipeRoom",
        [RoomBackgroundTilemap.TourianPipeRoomDark] = "TourianPipeRoomDark",
        [RoomBackgroundTilemap.TourianStatueHall] = "TourianStatueHall",
        [RoomBackgroundTilemap.TourianStatues] = "TourianStatues",
    });
}
