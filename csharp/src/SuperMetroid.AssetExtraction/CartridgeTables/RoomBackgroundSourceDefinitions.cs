using System.Collections.Frozen;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// Where the cartridge keeps the library backgrounds' sources and where their transfers write (#1414): the import
/// pipeline's private knowledge of the cartridge, decoded once into the installed background tilemaps and images.
/// </summary>
[CpuBusBoundary("catalog: library-background cartridge sources and destinations")]
internal static class RoomBackgroundSourceDefinitions
{
    /// <summary>Where each compressed background tilemap is stored, as the disassembly labels it.</summary>
    internal static readonly FrozenDictionary<RoomBackgroundTilemap, int> Tilemaps =
        new Dictionary<RoomBackgroundTilemap, int>
        {
            [RoomBackgroundTilemap.NorfairSmallPatternBrownPurpleBright] = 0xb9a634,
            [RoomBackgroundTilemap.NorfairSmallPatternBrownPurple0] = 0xb9a714,
            [RoomBackgroundTilemap.NorfairHorizontalPatternBrick] = 0xb9a7a8,
            [RoomBackgroundTilemap.NorfairVerticalPatternBrick] = 0xb9a83a,
            [RoomBackgroundTilemap.NorfairCavernStalactites] = 0xb9ac83,
            [RoomBackgroundTilemap.NorfairCavernVertical] = 0xb9aeff,
            [RoomBackgroundTilemap.NorfairCavernHorizontalRuins] = 0xb9b2f0,
            [RoomBackgroundTilemap.NorfairCavernVerticalRuins] = 0xb9b6bb,
            [RoomBackgroundTilemap.NorfairCavernRoomPillars] = 0xb9bba5,
            [RoomBackgroundTilemap.NorfairRoomStatues] = 0xb9bf3b,
            [RoomBackgroundTilemap.NorfairPurplePatches] = 0xb9c26f,
            [RoomBackgroundTilemap.NorfairBubbles] = 0xb9c5c8,
            [RoomBackgroundTilemap.BrinstarVerticalGlowPatches] = 0xb9c972,
            [RoomBackgroundTilemap.BrinstarHorizontalGlowPatches] = 0xb9cd01,
            [RoomBackgroundTilemap.BrinstarLargeHorizontalPattern] = 0xb9ce9f,
            [RoomBackgroundTilemap.BrinstarMediumHorizontalPattern] = 0xb9cff8,
            [RoomBackgroundTilemap.BrinstarThinHorizontalPattern] = 0xb9d1fb,
            [RoomBackgroundTilemap.BrinstarSmallPattern] = 0xb9d38f,
            [RoomBackgroundTilemap.BrinstarSpheresPattern] = 0xb9d3c5,
            [RoomBackgroundTilemap.BrinstarSmallPatternVariety0] = 0xb9d3fb,
            [RoomBackgroundTilemap.BrinstarDarkPattern] = 0xb9d5d8,
            [RoomBackgroundTilemap.BrinstarWideVerticalTowerBrick0] = 0xb9d715,
            [RoomBackgroundTilemap.BrinstarVerticalTower] = 0xb9e1b3,
            [RoomBackgroundTilemap.BrinstarNarrowVerticalTowerBrick] = 0xb9e61c,
            [RoomBackgroundTilemap.BrinstarVerticalBrick0] = 0xb9e885,
            [RoomBackgroundTilemap.BrinstarMechanicalRoomSpikeFloor] = 0xb9ea80,
            [RoomBackgroundTilemap.BrinstarMechanicalRoom] = 0xb9ebc7,
            [RoomBackgroundTilemap.BrinstarNarrowVerticalTowerBrickVines0] = 0xb9ee52,
            [RoomBackgroundTilemap.BrinstarNarrowVerticalTowerBrickGrey0] = 0xb9f1c8,
            [RoomBackgroundTilemap.BrinstarBlueGridBlocks] = 0xb9f94f,
            [RoomBackgroundTilemap.BrinstarKraidUpper] = 0xb9fa38,
            [RoomBackgroundTilemap.BrinstarKraidLower0] = 0xb9fe3e,
            [RoomBackgroundTilemap.CrateriaVerticalPatternRocks] = 0xba807e,
            [RoomBackgroundTilemap.CrateriaHorizontalPatternRocks] = 0xba82c4,
            [RoomBackgroundTilemap.CrateriaRocks] = 0xba8437,
            [RoomBackgroundTilemap.CrateriaPurpleRocks] = 0xba85ba,
            [RoomBackgroundTilemap.CrateriaBrickRoomWallArtDark] = 0xba86fc,
            [RoomBackgroundTilemap.CrateriaMechanical] = 0xba8780,
            [RoomBackgroundTilemap.CrateriaBrickRoomWallArt] = 0xba8a49,
            [RoomBackgroundTilemap.CrateriaElevator] = 0xba8acd,
            [RoomBackgroundTilemap.CrateriaBrickRoom] = 0xba8dbd,
            [RoomBackgroundTilemap.WreckedShipEntranceHall0] = 0xba8de7,
            [RoomBackgroundTilemap.WreckedShipColumnsTubes] = 0xba9386,
            [RoomBackgroundTilemap.WreckedShipExperimentRoom] = 0xba988d,
            [RoomBackgroundTilemap.WreckedShipPanelsVents] = 0xba9c35,
            [RoomBackgroundTilemap.WreckedShipPanelsVentsHorizontal] = 0xba9f12,
            [RoomBackgroundTilemap.WreckedShipPanelsVentsVertical] = 0xbaa119,
            [RoomBackgroundTilemap.MaridiaBrownWall] = 0xbaa475,
            [RoomBackgroundTilemap.MaridiaGreenWall] = 0xbaa69f,
            [RoomBackgroundTilemap.TourianMechanicalRoom] = 0xbaaa78,
            [RoomBackgroundTilemap.TourianMechanicalRoomLetterbox] = 0xbaadf0,
            [RoomBackgroundTilemap.TourianVerticalStructures] = 0xbaafe6,
            [RoomBackgroundTilemap.TourianPipeRoomLetterbox] = 0xbab36b,
            [RoomBackgroundTilemap.TourianPipeRoomVerticalSeparation] = 0xbab5d8,
            [RoomBackgroundTilemap.TourianPipeRoom] = 0xbab9a3,
            [RoomBackgroundTilemap.TourianPipeRoomDark] = 0xbabdd9,
            [RoomBackgroundTilemap.TourianStatueHall] = 0xbac22a,
            [RoomBackgroundTilemap.TourianStatues] = 0xbac4bc,
        }.ToFrozenDictionary();

    /// <summary><c>$87:AD64</c>, the Tourian entrance statue ghost's 48 uncompressed 4-bpp characters (<c>kTileData_TourianEntranceStatueGhost</c>).</summary>
    internal const int TourianStatueGhostCharacters = 0x87ad64;

    /// <summary><c>$8A:B180</c>, the first of the seven contiguous 32x32 scrolling-sky tilemap pages.</summary>
    internal const int FirstSkyPage = 0x8ab180;

    /// <summary>The bytes of one sky page: the stride between consecutive pages.</summary>
    internal const int SkyPageBytes = 0x0800;

    /// <summary>The VRAM word each transfer destination starts at, as the transfers' operands name it.</summary>
    internal static readonly FrozenDictionary<LibraryBackgroundDestination, ushort> DestinationWords =
        new Dictionary<LibraryBackgroundDestination, ushort>
        {
            [LibraryBackgroundDestination.BackgroundFirstPage] = 0x4800,
            [LibraryBackgroundDestination.BackgroundSecondPage] = 0x4c00,
            [LibraryBackgroundDestination.KraidBackgroundPages] = 0x4000,
            [LibraryBackgroundDestination.StatueGhostSpriteCharacters] = 0x6d00,
            [LibraryBackgroundDestination.KraidLayer3Characters] = 0x2000,
        }.ToFrozenDictionary();
}
