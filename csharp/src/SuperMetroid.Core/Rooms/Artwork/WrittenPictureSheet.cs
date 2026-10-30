using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The installed pre-coloured pictures written over one tileset's rooms after they load (#1392, #1466): each picture
/// something writes over BG1 or BG2 (typed text, the Ceres warning) or that BG3 shows (an effect's page, the cleared cell,
/// the HUD's images), coloured with the tileset's standard palette over what the rooms load, pictured again for each value
/// of the appearance inputs it depends on and named by <see cref="AppearancePictureNames"/>; with the inputs each picture
/// depends on, which choose among them.
/// </summary>
public sealed class WrittenPictureSheet
{
    /// <summary>The inputs each picture depends on, by the name its pictures begin with; empty for one that depends on none.</summary>
    private readonly Dictionary<string, ImmutableArray<AppearanceInput>> inputs;

    /// <summary>Indexes the pictures <paramref name="inputs"/> names in <paramref name="sheet"/>.</summary>
    /// <param name="layer">The backgrounds the pictures are written over.</param>
    /// <param name="tileset">The tileset whose rooms the pictures are written in.</param>
    /// <param name="sheet">The pictures.</param>
    /// <param name="inputs">The inputs each picture depends on, by the name its pictures begin with.</param>
    /// <exception cref="InvalidDataException">
    /// The sheet is not coloured as its layer's backgrounds are, or a picture has no frame for the room's load, or one that
    /// is not whole tiles.
    /// </exception>
    public WrittenPictureSheet(WrittenPictureLayer layer, TilesetDefinition tileset, SpriteSheet sheet,
        IReadOnlyDictionary<string, ImmutableArray<AppearanceInput>> inputs)
    {
        Layer = layer;
        Tileset = tileset;
        Sheet = Ensure.NotNull(sheet);
        this.inputs = new Dictionary<string, ImmutableArray<AppearanceInput>>(Ensure.NotNull(inputs), StringComparer.Ordinal);
        if (sheet.Image.Colours != ColoursOf(layer))
            throw new InvalidDataException($"{Name} is not coloured as {layer}'s backgrounds are.");
        foreach ((string picture, ImmutableArray<AppearanceInput> depends) in this.inputs)
        {
            string[] loaded = [.. Enumerable.Repeat(RoomAppearanceValues.Loaded, depends.Length)];
            SpriteSheetFrame frame = sheet.Frame(AppearancePictureNames.Of(picture, depends, loaded));
            if (frame.Size.X == 0 || frame.Size.X % ScreenGeometry.BackgroundTileSizePixels != 0
                || frame.Size.Y % ScreenGeometry.BackgroundTileSizePixels != 0)
                throw new InvalidDataException($"{Name} pictures {picture} as {frame.Size.X}x{frame.Size.Y} pixels, not whole tiles.");
        }
    }

    /// <summary>The backgrounds the pictures are written over.</summary>
    public WrittenPictureLayer Layer { get; }

    /// <summary>The tileset whose rooms the pictures are written in.</summary>
    public TilesetDefinition Tileset { get; }

    /// <summary>The installed name.</summary>
    public string Name => NameOf(Layer, Tileset);

    /// <summary>The pictures.</summary>
    public SpriteSheet Sheet { get; }

    /// <summary>The inputs each picture depends on, by the name its pictures begin with.</summary>
    public IReadOnlyDictionary<string, ImmutableArray<AppearanceInput>> Inputs => inputs;

    /// <summary>
    /// The picture of <paramref name="picture"/> while the room's appearance inputs take the values
    /// <paramref name="appearance"/> reads.
    /// </summary>
    /// <exception cref="InvalidDataException">The sheet pictures nothing by that name, or nothing for those values.</exception>
    public SpriteSheetFrame Picture(string picture, RoomAppearanceView appearance)
    {
        if (!inputs.TryGetValue(Ensure.NotNull(picture), out ImmutableArray<AppearanceInput> depends))
            throw new InvalidDataException($"{Name} pictures nothing named {picture}.");
        var values = new string[depends.Length];
        for (int input = 0; input < depends.Length; input++)
            values[input] = appearance.ValueOf(depends[input]);
        return Sheet.Frame(AppearancePictureNames.Of(picture, depends, values));
    }

    /// <summary>The installed name of the sheet of pictures written over <paramref name="layer"/> in <paramref name="tileset"/>'s rooms.</summary>
    public static string NameOf(WrittenPictureLayer layer, TilesetDefinition tileset) =>
        $"{(layer == WrittenPictureLayer.Bg3 ? "Bg3Pictures" : "Written")}.{RoomBlockSheet.NameOf(tileset)}";

    /// <summary>How the pictures of <paramref name="layer"/> are coloured: BG3's from four-colour rows.</summary>
    public static SpriteImageColours ColoursOf(WrittenPictureLayer layer) =>
        layer == WrittenPictureLayer.Bg3 ? SpriteImageColours.TwoBppBackground : SpriteImageColours.Background;
}
