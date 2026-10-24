using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The installed pre-coloured Mode 7 tiles of one Ceres tileset (#1392): each of the 256 tiles whose characters the room's
/// load binds, sixteen to a row, coloured with the tileset's standard palette and the sprite half as a room load leaves
/// it, and what showed at each tile's characters when it was pictured; a tile nothing binds is not pictured.
/// </summary>
public sealed class Mode7TileSheet
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

    /// <summary>The room tile characters whose bytes one Mode 7 tile's pixels share.</summary>
    public const int TileCharacters = 4;

    /// <summary>Checks the picture holds every tile and is background-coloured, and that each tile records what it shows over.</summary>
    /// <exception cref="InvalidDataException">The picture is not every tile's size or colours, or a tile's artwork is missing.</exception>
    public Mode7TileSheet(TilesetDefinition tileset, SpriteSheet sheet, IReadOnlyList<BgArtworkShown?> shown)
    {
        Tileset = tileset;
        Sheet = Ensure.NotNull(sheet);
        Picture = sheet.Frame(FrameName);
        Shown = [.. Ensure.NotNull(shown)];
        if (Picture.Size.X != TilesPerRow * TilePixels || Picture.Size.Y != Tiles / TilesPerRow * TilePixels)
            throw new InvalidDataException($"{Name} is {Picture.Size.X}x{Picture.Size.Y}, not every tile's size.");
        if (Picture.Sheet.Colours != SpriteImageColours.FullPalette)
            throw new InvalidDataException($"{Name} is not coloured from the full palette.");
        if (Shown.Count != Tiles)
            throw new InvalidDataException($"{Name} records what {Shown.Count} tiles show over, not {Tiles}.");
    }

    /// <summary>The Ceres tileset.</summary>
    public TilesetDefinition Tileset { get; }

    /// <summary>The installed name.</summary>
    public string Name => NameOf(Tileset);

    /// <summary>The sheet.</summary>
    public SpriteSheet Sheet { get; }

    /// <summary>The picture of every tile.</summary>
    public SpriteSheetFrame Picture { get; }

    /// <summary>What showed at each tile's characters when it was pictured, by tile; null for a tile not pictured.</summary>
    public IReadOnlyList<BgArtworkShown?> Shown { get; }

    /// <summary>The top-left pixel of tile <paramref name="tile"/> within <see cref="Picture"/>.</summary>
    public static Position16<ImagePixels> Corner(int tile) =>
        new((ushort)(tile % TilesPerRow * TilePixels), (ushort)(tile / TilesPerRow * TilePixels));

    /// <summary>
    /// Checks every tile <paramref name="tiles"/> names is pictured, and that <paramref name="bindings"/> show its
    /// characters as they were pictured over.
    /// </summary>
    /// <exception cref="InvalidDataException">A named tile is not pictured, or its characters show other artwork.</exception>
    public void AssertShownBy(BgArtworkBindings bindings, IEnumerable<byte> tiles)
    {
        Ensure.NotNull(bindings);
        foreach (byte tile in Ensure.NotNull(tiles))
        {
            BgArtworkShown pictured = Shown[tile] ?? throw new InvalidDataException($"The Mode 7 map names tile {tile}, whose characters {Name} does not picture.");
            BgArtworkShown now = bindings.Showing(BgCharacterSet.Tiles, tile * TileCharacters, TileCharacters);
            if (now != pictured)
                throw new InvalidDataException($"Mode 7 tile {tile} shows {now.Artwork.Name} at {now.WordOffset}, not the {pictured.Artwork.Name} {Name} pictures.");
        }
    }

    /// <summary>The installed name of <paramref name="tileset"/>'s Mode 7 tiles.</summary>
    public static string NameOf(TilesetDefinition tileset) => $"Mode7.{RoomBlockSheet.NameOf(tileset)}";
}
