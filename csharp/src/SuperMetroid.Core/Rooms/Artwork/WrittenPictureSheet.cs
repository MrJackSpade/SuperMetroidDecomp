using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The installed pre-coloured pictures shown on one display layer of a tileset's rooms (#1392, #1466, #1601): for the
/// room's layers, each picture an encounter writes over the foreground or the background (typed text, the Ceres warning);
/// for the effect overlay, each room effect's page and the HUD's images. Each is coloured with its own standard colours,
/// pictured again for each value of the appearance inputs it depends on and named by <see cref="AppearancePictureNames"/>;
/// with the inputs each picture depends on, which choose among them. A picture is placed in pixels, so it may be any size.
/// Each picture is installed as its two parts (#1634, <see cref="PictureParts"/>): what of it lies behind the actors passing
/// in front of the room's back tiles and what lies in front of them. The HUD's images are installed whole.
/// </summary>
public sealed class WrittenPictureSheet
{
    /// <summary>The inputs each picture depends on, by the name its pictures begin with; empty for one that depends on none.</summary>
    internal readonly Dictionary<string, ImmutableArray<AppearanceInput>> inputs;

    /// <summary>Indexes the pictures <paramref name="inputs"/> names in <paramref name="sheet"/>.</summary>
    /// <param name="layer">The display layer the pictures show on.</param>
    /// <param name="tileset">The tileset whose rooms the pictures are written in.</param>
    /// <param name="sheet">The pictures.</param>
    /// <param name="inputs">The inputs each picture depends on, by the name its pictures begin with.</param>
    /// <exception cref="InvalidDataException">A picture installs neither part of a frame for the room's load.</exception>
    public WrittenPictureSheet(WrittenPictureLayer layer, TilesetDefinition tileset, SpriteSheet sheet,
        IReadOnlyDictionary<string, ImmutableArray<AppearanceInput>> inputs)
    {
        Layer = layer;
        Tileset = tileset;
        Sheet = Ensure.NotNull(sheet);
        this.inputs = new Dictionary<string, ImmutableArray<AppearanceInput>>(Ensure.NotNull(inputs), StringComparer.Ordinal);
        foreach ((string picture, ImmutableArray<AppearanceInput> depends) in this.inputs)
        {
            string[] loaded = [.. Enumerable.Repeat(RoomAppearanceValues.Loaded, depends.Length)];
            _ = PictureParts.Of(sheet, AppearancePictureNames.Of(picture, depends, loaded));
        }
    }

    /// <summary>The display layer the pictures show on.</summary>
    public WrittenPictureLayer Layer { get; }

    /// <summary>The tileset whose rooms the pictures are written in.</summary>
    public TilesetDefinition Tileset { get; }

    /// <summary>The installed name.</summary>
    public string Name => NameOf(Layer, Tileset);

    /// <summary>The pictures.</summary>
    public SpriteSheet Sheet { get; }

    /// <summary>
    /// The picture of <paramref name="picture"/> while the room's appearance inputs take the values
    /// <paramref name="appearance"/> reads, as its part behind and its part in front (#1634).
    /// </summary>
    /// <exception cref="InvalidDataException">The sheet pictures nothing by that name, or nothing for those values.</exception>
    public LayeredPicture Picture(string picture, RoomAppearanceView appearance)
    {
        if (!inputs.TryGetValue(Ensure.NotNull(picture), out ImmutableArray<AppearanceInput> depends))
            throw new InvalidDataException($"{Name} pictures nothing named {picture}.");
        var values = new string[depends.Length];
        for (int input = 0; input < depends.Length; input++)
            values[input] = appearance.ValueOf(depends[input]);
        return PictureParts.Of(Sheet, AppearancePictureNames.Of(picture, depends, values));
    }

    /// <summary>
    /// The installed name of the sheet of pictures shown on <paramref name="layer"/> in <paramref name="tileset"/>'s rooms:
    /// the layer's role, then the tileset's name.
    /// </summary>
    public static string NameOf(WrittenPictureLayer layer, TilesetDefinition tileset) =>
        $"{(layer == WrittenPictureLayer.FxOverlayAndHud ? "OverlayPictures" : "Written")}.{RoomBlockSheet.NameOf(tileset)}";
}
