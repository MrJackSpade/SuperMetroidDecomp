using System.Collections.Immutable;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The import's classification of the cartridge's area-map characters (#1513): each character and mirroring the retail
/// area maps draw a cell with, and the cell it pictures, decided from the character's art and from where the maps use it.
/// Mirroring is folded into the cell: a mirrored character pictures the cell whose edges, slope or arrow it shows once
/// mirrored. A character lists its as-drawn entry first, so the first entry of a cell is the one the import pictures it
/// from; every other entry of the same cell paints the same pixels (<see cref="MapCellCharacters"/> requires it).
/// </summary>
/// <remarks>
/// The art (as the room-map and pause sheets picture it) and the maps' use: character 31 is the empty cell; 27 is a room's
/// filled interior; 32 to 39 are a room's interior with walls on the edges named; 40 to 43 are the four pieces of the
/// two-by-two step a sloped hallway is drawn with (the floor's diagonal in 40 and 42, the ceiling's in 41 and 43); 77 is
/// the save room's letter; 94, 110, 111, 118, 119, 142 and 143 are a room's interior with walls and the item dot; 16, 79
/// and 95 are elevator rooms with the elevator's platform, each with its corners rounded where the elevator's shaft (206,
/// a dotted column) joins it; 17 is the arrow at a shaft's end that leads out of the area, drawn pointing away from the
/// shaft; 109 is a dotted corridor without walls leading between rooms or off the map.
/// </remarks>
internal static class MapCellCharacterDefinitions
{
    /// <summary>
    /// The colour row each shade shows a cell in: the area maps store every mapped cell in the unexplored row, and a cell's
    /// row becomes the explored one once <c>$82:943D</c> clears its explored bit.
    /// </summary>
    internal static readonly ImmutableDictionary<MapCellShade, int> ShadeRows = ImmutableDictionary.CreateRange(
    [
        new KeyValuePair<MapCellShade, int>(MapCellShade.Unexplored, 3),
        new KeyValuePair<MapCellShade, int>(MapCellShade.Explored, 2),
    ]);

    /// <summary>An open edge.</summary>
    private const MapCellEdge O = MapCellEdge.Open;

    /// <summary>A walled edge.</summary>
    private const MapCellEdge W = MapCellEdge.Wall;

    /// <summary>The edge an elevator's shaft joins.</summary>
    private const MapCellEdge S = MapCellEdge.ElevatorShaft;

    /// <summary>A character drawn as it is.</summary>
    private const PictureFlip AsDrawn = PictureFlip.None;

    /// <summary>A character mirrored left to right.</summary>
    private const PictureFlip H = PictureFlip.Horizontal;

    /// <summary>A character mirrored top to bottom.</summary>
    private const PictureFlip V = PictureFlip.Vertical;

    /// <summary>A character mirrored both ways.</summary>
    private const PictureFlip HV = PictureFlip.Horizontal | PictureFlip.Vertical;

    /// <summary>
    /// Every character and mirroring the retail area maps use, with the cell it pictures; edges are listed top, bottom,
    /// left, right.
    /// </summary>
    internal static readonly ImmutableArray<MapCellCharacter> Characters =
    [
        new(new(31, AsDrawn), MapCell.Empty),
        new(new(27, AsDrawn), new(MapCellKind.Room, new(O, O, O, O))),
        new(new(27, H), new(MapCellKind.Room, new(O, O, O, O))),
        new(new(27, V), new(MapCellKind.Room, new(O, O, O, O))),
        new(new(27, HV), new(MapCellKind.Room, new(O, O, O, O))),
        new(new(32, AsDrawn), new(MapCellKind.Room, new(W, W, W, W))),
        new(new(32, V), new(MapCellKind.Room, new(W, W, W, W))),
        new(new(33, AsDrawn), new(MapCellKind.Room, new(W, W, W, O))),
        new(new(33, V), new(MapCellKind.Room, new(W, W, W, O))),
        new(new(33, H), new(MapCellKind.Room, new(W, W, O, W))),
        new(new(33, HV), new(MapCellKind.Room, new(W, W, O, W))),
        new(new(34, AsDrawn), new(MapCellKind.Room, new(W, W, O, O))),
        new(new(34, V), new(MapCellKind.Room, new(W, W, O, O))),
        new(new(34, H), new(MapCellKind.Room, new(W, W, O, O))),
        new(new(35, AsDrawn), new(MapCellKind.Room, new(O, O, W, W))),
        new(new(35, V), new(MapCellKind.Room, new(O, O, W, W))),
        new(new(36, AsDrawn), new(MapCellKind.Room, new(W, O, W, W))),
        new(new(36, V), new(MapCellKind.Room, new(O, W, W, W))),
        new(new(37, AsDrawn), new(MapCellKind.Room, new(W, O, W, O))),
        new(new(37, V), new(MapCellKind.Room, new(O, W, W, O))),
        new(new(37, H), new(MapCellKind.Room, new(W, O, O, W))),
        new(new(37, HV), new(MapCellKind.Room, new(O, W, O, W))),
        new(new(38, AsDrawn), new(MapCellKind.Room, new(W, O, O, O))),
        new(new(38, H), new(MapCellKind.Room, new(W, O, O, O))),
        new(new(38, V), new(MapCellKind.Room, new(O, W, O, O))),
        new(new(38, HV), new(MapCellKind.Room, new(O, W, O, O))),
        new(new(39, AsDrawn), new(MapCellKind.Room, new(O, O, O, W))),
        new(new(39, V), new(MapCellKind.Room, new(O, O, O, W))),
        new(new(39, H), new(MapCellKind.Room, new(O, O, W, O))),
        new(new(39, HV), new(MapCellKind.Room, new(O, O, W, O))),
        new(new(40, AsDrawn), new(MapCellKind.Room, new(O, O, O, O), MapCellSlope.RightwardFloorLow)),
        new(new(40, H), new(MapCellKind.Room, new(O, O, O, O), MapCellSlope.LeftwardFloorLow)),
        new(new(41, AsDrawn), new(MapCellKind.Room, new(O, O, O, O), MapCellSlope.RightwardCeilingLow)),
        new(new(41, H), new(MapCellKind.Room, new(O, O, O, O), MapCellSlope.LeftwardCeilingLow)),
        new(new(42, AsDrawn), new(MapCellKind.Room, new(O, O, O, O), MapCellSlope.RightwardFloorHigh)),
        new(new(42, H), new(MapCellKind.Room, new(O, O, O, O), MapCellSlope.LeftwardFloorHigh)),
        new(new(43, AsDrawn), new(MapCellKind.Room, new(O, O, O, O), MapCellSlope.RightwardCeilingHigh)),
        new(new(43, H), new(MapCellKind.Room, new(O, O, O, O), MapCellSlope.LeftwardCeilingHigh)),
        new(new(77, AsDrawn), new(MapCellKind.SaveStation, new(W, W, W, W))),
        new(new(94, AsDrawn), new(MapCellKind.Item, new(W, W, O, O))),
        new(new(110, AsDrawn), new(MapCellKind.Item, new(W, O, W, W))),
        new(new(110, V), new(MapCellKind.Item, new(O, W, W, W))),
        new(new(111, AsDrawn), new(MapCellKind.Item, new(W, W, W, W))),
        new(new(118, AsDrawn), new(MapCellKind.Item, new(W, O, O, O))),
        new(new(118, V), new(MapCellKind.Item, new(O, W, O, O))),
        new(new(119, AsDrawn), new(MapCellKind.Item, new(O, O, W, O))),
        new(new(119, H), new(MapCellKind.Item, new(O, O, O, W))),
        new(new(142, AsDrawn), new(MapCellKind.Item, new(W, O, W, O))),
        new(new(142, V), new(MapCellKind.Item, new(O, W, W, O))),
        new(new(142, H), new(MapCellKind.Item, new(W, O, O, W))),
        new(new(142, HV), new(MapCellKind.Item, new(O, W, O, W))),
        new(new(143, AsDrawn), new(MapCellKind.Item, new(W, W, W, O))),
        new(new(143, H), new(MapCellKind.Item, new(W, W, O, W))),
        new(new(16, V), new(MapCellKind.Elevator, new(S, O, W, W))),
        new(new(79, AsDrawn), new(MapCellKind.Elevator, new(W, S, W, W))),
        new(new(95, AsDrawn), new(MapCellKind.Elevator, new(S, W, O, O))),
        new(new(206, AsDrawn), new(MapCellKind.ElevatorShaft, new(O, O, O, O))),
        new(new(17, AsDrawn), new(MapCellKind.ElevatorExit, new(S, O, O, O))),
        new(new(17, V), new(MapCellKind.ElevatorExit, new(O, S, O, O))),
        new(new(109, AsDrawn), new(MapCellKind.Passage, new(O, O, O, O))),
    ];

    /// <summary>
    /// The cells a retail map stores in the explored row of <see cref="ShadeRows"/> although they are unexplored: Brinstar's up arrow to
    /// Crateria. The arrow paints only the ink both rows colour alike, so it shows the same in either shade; the import
    /// shows it by its exploration like every other cell, and requires that it shows alike
    /// (<see cref="MapCellCharacters.RequireStoredExploredCellsShowAlike"/>).
    /// </summary>
    internal static readonly ImmutableArray<MapCell> StoredExplored = [new(MapCellKind.ElevatorExit, new(O, S, O, O))];
}
