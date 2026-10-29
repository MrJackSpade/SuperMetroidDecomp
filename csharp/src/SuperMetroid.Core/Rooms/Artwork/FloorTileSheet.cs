using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The installed pre-coloured floor tiles of one Ceres tileset (#1392): each of the 256 tiles whose characters the room's
/// load binds, sixteen to a row, coloured with the tileset's standard palette and the sprite half as a room load leaves
/// it, which tiles it pictures (a tile whose characters the room's load leaves empty is not), and the floor map a room
/// of the tileset starts from, which extraction imports (#1422).
/// </summary>
public sealed class FloorTileSheet
{
    /// <summary>The frame picturing every tile.</summary>
    public const string FrameName = "tiles";

    /// <summary>Tiles the map can name.</summary>
    public const int Tiles = 256;

    /// <summary>Tiles across a row of the picture.</summary>
    public const int TilesPerRow = 16;

    /// <summary>Pixels along a tile's side.</summary>
    public const int TilePixels = 8;

    /// <summary>VRAM words one tile's characters occupy, whose high bytes are its pixels.</summary>
    public const int TileWords = TilePixels * TilePixels;

    /// <summary>
    /// Checks the picture holds every tile and is background-coloured, that each tile records whether it is pictured, and
    /// that the map names a tile for every cell.
    /// </summary>
    /// <exception cref="InvalidDataException">The picture is not every tile's size or colours, a tile's record is missing, or the map is not every cell.</exception>
    public FloorTileSheet(TilesetDefinition tileset, SpriteSheet sheet, IReadOnlyList<bool> pictured, ReadOnlySpan<byte> map)
    {
        Tileset = tileset;
        Sheet = Ensure.NotNull(sheet);
        Picture = sheet.Frame(FrameName);
        Pictured = [.. Ensure.NotNull(pictured)];
        if (map.Length != FloorMap.Cells)
            throw new InvalidDataException($"{Name}'s floor map holds {map.Length} cells, not {FloorMap.Cells}.");
        this.map = map.ToArray();
        if (Picture.Size.X != TilesPerRow * TilePixels || Picture.Size.Y != Tiles / TilesPerRow * TilePixels)
            throw new InvalidDataException($"{Name} is {Picture.Size.X}x{Picture.Size.Y}, not every tile's size.");
        if (Picture.Sheet.Colours != SpriteImageColours.FullPalette)
            throw new InvalidDataException($"{Name} is not coloured from the full palette.");
        if (Pictured.Count != Tiles)
            throw new InvalidDataException($"{Name} records whether {Pictured.Count} tiles are pictured, not {Tiles}.");
    }

    /// <summary>The floor map a room of the tileset starts from: its tile numbers, row-major.</summary>
    private readonly byte[] map;

    /// <summary>The Ceres tileset.</summary>
    public TilesetDefinition Tileset { get; }

    /// <summary>The floor map a room of the tileset starts from: <see cref="FloorMap.Cells"/> tile numbers, row-major.</summary>
    public ReadOnlySpan<byte> Map => map;

    /// <summary>The installed name.</summary>
    public string Name => NameOf(Tileset);

    /// <summary>The sheet.</summary>
    public SpriteSheet Sheet { get; }

    /// <summary>The picture of every tile.</summary>
    public SpriteSheetFrame Picture { get; }

    /// <summary>Whether each tile is pictured, by tile.</summary>
    public IReadOnlyList<bool> Pictured { get; }

    /// <summary>The top-left pixel of tile <paramref name="tile"/> within <see cref="Picture"/>.</summary>
    public static Position16<ImagePixels> Corner(int tile) =>
        new((ushort)(tile % TilesPerRow * TilePixels), (ushort)(tile / TilesPerRow * TilePixels));

    /// <summary>
    /// Checks every tile <paramref name="tiles"/> names is pictured: nothing a Ceres room shows over its load reaches the
    /// floor's tiles (#1466), so a pictured tile shows as it was pictured.
    /// </summary>
    /// <exception cref="InvalidDataException">A named tile is not pictured.</exception>
    public void AssertPictured(IEnumerable<byte> tiles)
    {
        foreach (int tile in Ensure.NotNull(tiles))
        {
            if (!Pictured[tile])
                throw new InvalidDataException($"The floor map names tile {tile}, whose characters {Name} does not picture.");
        }
    }

    /// <summary>The installed name of <paramref name="tileset"/>'s floor tiles.</summary>
    public static string NameOf(TilesetDefinition tileset) => $"Floor.{RoomBlockSheet.NameOf(tileset)}";
}
