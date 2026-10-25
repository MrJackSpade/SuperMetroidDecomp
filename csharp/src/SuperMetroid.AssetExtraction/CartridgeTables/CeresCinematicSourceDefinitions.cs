using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// Where the cartridge keeps the opening flight's and the Ceres destruction's artwork and actor placements (#1416): the
/// import pipeline's private knowledge of the cartridge, decoded once into the installed Ceres cinematic documents.
/// </summary>
[CpuBusBoundary("catalog: Ceres cinematic cartridge sources")]
internal static class CeresCinematicSourceDefinitions
{
    /// <summary>$8C:E5E9, the flight's 256-colour palette.</summary>
    internal const int FlightPalette = 0x8ce5e9;

    /// <summary>$95:A82F, the flight's compressed floor characters.</summary>
    internal const int FlightFloorCharacters = 0x95a82f;

    /// <summary>$96:FE69, the compressed Ceres maps: the flight's front and rear views, then the destruction's.</summary>
    internal const int CeresTilemaps = 0x96fe69;

    /// <summary>$96:D10A, the flight's compressed ship, asteroid and colony sprite characters.</summary>
    internal const int FlightObjectCharacters = 0x96d10a;

    /// <summary>$97:8ADB, the Zebes reveal's compressed tilemap.</summary>
    internal const int ZebesTilemap = 0x978adb;

    /// <summary>$96:EC76, the Zebes reveal's compressed characters.</summary>
    internal const int ZebesCharacters = 0x96ec76;

    /// <summary>The most decompressed bytes a Ceres scene tilemap may hold.</summary>
    internal const int CompressedTilemapLimit = 0x1000;

    /// <summary>The bank that holds the cinematic actors' initializers and their placement immediates.</summary>
    internal const int PlacementBank = 0x8b0000;

    /// <summary>
    /// The X/Y immediates of the flight's five rear-view actors' initializers ($8B:BE3B..BE5C), in spawn order; the
    /// last X word is a signed -32.
    /// </summary>
    internal static IReadOnlyList<CinematicPlacementSourceDefinition> RearViewPlacements { get; } =
    [
        new(CeresFlightActorDefinitions.RearViewPlacementId(0), 0xbf23),
        new(CeresFlightActorDefinitions.RearViewPlacementId(1), 0xbf4d),
        new(CeresFlightActorDefinitions.RearViewPlacementId(2), 0xbf77),
        new(CeresFlightActorDefinitions.RearViewPlacementId(3), 0xbfb4),
        new(CeresFlightActorDefinitions.RearViewPlacementId(4), 0xbea3),
    ];

    /// <summary>The X/Y immediates of the Zebes reveal's six actors' initializers ($8B:C810..C831), in spawn order.</summary>
    internal static IReadOnlyList<CinematicPlacementSourceDefinition> ZebesPlacements { get; } =
    [
        new(CeresDestructionActorDefinitions.ZebesPlacementId(0), 0xc83c),
        new(CeresDestructionActorDefinitions.ZebesPlacementId(1), 0xc944),
        new(CeresDestructionActorDefinitions.ZebesPlacementId(2), 0xc958),
        new(CeresDestructionActorDefinitions.ZebesPlacementId(3), 0xc96c),
        new(CeresDestructionActorDefinitions.ZebesPlacementId(4), 0xc980),
        new(CeresDestructionActorDefinitions.ZebesPlacementId(5), 0xc993),
    ];
}
