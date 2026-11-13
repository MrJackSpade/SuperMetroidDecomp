using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The installed pre-coloured pictures shown on one display layer of a tileset's rooms (#1392, #1466, #1601): for the
/// room's layers, each picture an encounter writes over the foreground or the background (typed text, the Ceres warning);
/// for the effect overlay, each room effect's liquid or atmosphere (#1706). The HUD's images are not written pictures: they
/// are a sheet of their own (<see cref="HudPictureSheet"/>, #1731). Each is coloured with its own standard colours,
/// pictured again for each value of the appearance inputs it depends on and named by <see cref="AppearancePictureNames"/>;
/// with the inputs each picture depends on, which choose among them. A picture is placed in pixels, so it may be any size.
/// Each picture is installed as its two parts (#1634, <see cref="PictureParts"/>): what of it lies behind the actors passing
/// in front of the room's back tiles and what lies in front of them.
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
    /// what the layer shows, then the tileset's name.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The layer is undefined.</exception>
    public static string NameOf(WrittenPictureLayer layer, TilesetDefinition tileset) =>
        $"{ShownOn(layer)}.{RoomBlockSheet.NameOf(tileset)}";

    /// <summary>What <paramref name="layer"/>'s sheets are named for: the pictures written over the room, or the room effects.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The layer is undefined.</exception>
    private static string ShownOn(WrittenPictureLayer layer) => layer switch
    {
        WrittenPictureLayer.WrittenOverRoom => "Written",
        WrittenPictureLayer.FxOverlay => "RoomEffects",
        _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, "Undefined written picture layer."),
    };
}
