using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The installed pre-coloured tile words of one tileset's rooms (#1392, #1466): an image of each part something writes over
/// the rooms' BG1, BG2 or BG3 after they load (typed text, the HUD, an effect's page), coloured with the tileset's
/// standard palette over what the rooms load, where each tile word is pictured for each value of the appearance inputs it
/// depends on, and which inputs those are.
/// </summary>
public sealed class TileWordSheet
{
    /// <summary>The frame picturing tile words for inputs that no longer show the room's load, which no part shows.</summary>
    public const string ReboundFrameName = "rebound";

    /// <summary>Where each tile word is pictured for each value of its inputs.</summary>
    private readonly Dictionary<TileWordAppearance, BgTilePicture> pictures = [];

    /// <summary>The inputs each tile word that depends on any depends on.</summary>
    private readonly Dictionary<BackgroundTile, ImmutableArray<AppearanceInput>> inputs;

    /// <summary>Indexes <paramref name="tiles"/> into <paramref name="sheet"/>'s frames.</summary>
    /// <param name="layer">The backgrounds the words are written over.</param>
    /// <param name="tileset">The tileset whose rooms the words are written in.</param>
    /// <param name="sheet">The part images.</param>
    /// <param name="tiles">Where each tile word is pictured for each value of its inputs.</param>
    /// <param name="inputs">The inputs each tile word depends on; a word absent here depends on none.</param>
    /// <exception cref="InvalidDataException">
    /// The sheet is not coloured as its layer's characters are, a tile lies outside its frame, or a word is pictured twice
    /// for the same values.
    /// </exception>
    public TileWordSheet(TileWordLayer layer, TilesetDefinition tileset, SpriteSheet sheet, IEnumerable<TileWordSheetTile> tiles,
        IReadOnlyDictionary<BackgroundTile, ImmutableArray<AppearanceInput>> inputs)
    {
        Layer = layer;
        Tileset = tileset;
        Sheet = Ensure.NotNull(sheet);
        this.inputs = new Dictionary<BackgroundTile, ImmutableArray<AppearanceInput>>(Ensure.NotNull(inputs));
        Tiles = [.. Ensure.NotNull(tiles)];
        foreach (TileWordSheetTile tile in Tiles)
        {
            SpriteSheetFrame frame = sheet.Frame(tile.Frame);
            if (frame.Sheet.Colours != ColoursOf(layer))
                throw new InvalidDataException($"{Name} is not coloured as {layer}'s characters are.");
            if (tile.Corner.X + 8 > frame.Size.X || tile.Corner.Y + 8 > frame.Size.Y)
                throw new InvalidDataException($"{Name} pictures tile {tile.Tile.Tile} outside its frame {tile.Frame}.");
            if (!pictures.TryAdd(tile.Tile, new BgTilePicture(frame, tile.Corner)))
                throw new InvalidDataException($"{Name} pictures tile {tile.Tile.Tile} for {tile.Tile.Appearance} twice.");
        }
    }

    /// <summary>The backgrounds the words are written over.</summary>
    public TileWordLayer Layer { get; }

    /// <summary>The tileset whose rooms the words are written in.</summary>
    public TilesetDefinition Tileset { get; }

    /// <summary>The installed name.</summary>
    public string Name => NameOf(Layer, Tileset);

    /// <summary>The part images.</summary>
    public SpriteSheet Sheet { get; }

    /// <summary>Every tile pictured.</summary>
    public IReadOnlyList<TileWordSheetTile> Tiles { get; }

    /// <summary>The inputs each tile word that depends on any depends on.</summary>
    public IReadOnlyDictionary<BackgroundTile, ImmutableArray<AppearanceInput>> Inputs => inputs;

    /// <summary>Where <paramref name="tile"/> is pictured, if it is.</summary>
    public bool TryPicture(TileWordAppearance tile, out BgTilePicture picture) => pictures.TryGetValue(tile, out picture);

    /// <summary>
    /// <paramref name="tile"/> as it shows while the room's appearance inputs take the values <paramref name="appearance"/>
    /// reads: the values of the inputs it depends on.
    /// </summary>
    public TileWordAppearance AppearanceOf(BackgroundTile tile, RoomAppearanceView appearance)
    {
        if (!inputs.TryGetValue(tile, out ImmutableArray<AppearanceInput> depends))
            return new TileWordAppearance(tile, "");
        var values = new string[depends.Length];
        for (int input = 0; input < depends.Length; input++)
            values[input] = appearance.ValueOf(depends[input]);
        return new TileWordAppearance(tile, AppearancePictureNames.Spelling(depends, values));
    }

    /// <summary>The installed name of the sheet of words written over <paramref name="layer"/> in <paramref name="tileset"/>'s rooms.</summary>
    public static string NameOf(TileWordLayer layer, TilesetDefinition tileset) =>
        $"{(layer == TileWordLayer.Bg3 ? "Bg3Words" : "Words")}.{RoomBlockSheet.NameOf(tileset)}";

    /// <summary>How the characters of <paramref name="layer"/> are coloured: BG3's are two-bit.</summary>
    public static SpriteImageColours ColoursOf(TileWordLayer layer) =>
        layer == TileWordLayer.Bg3 ? SpriteImageColours.TwoBppBackground : SpriteImageColours.Background;
}
