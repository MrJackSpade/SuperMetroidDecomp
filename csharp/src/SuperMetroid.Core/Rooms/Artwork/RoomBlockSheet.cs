using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// One tileset's installed pre-coloured block pictures (#1392, #1466): every block its rooms can show, for each value each
/// appearance input it depends on can take, coloured with the tileset's standard background palette and named by
/// <see cref="RoomBlockPictureNames"/>; with the inputs each of its blocks depends on, which choose among them.
/// </summary>
public sealed class RoomBlockSheet
{
    /// <summary>Pixels along each side of a block picture.</summary>
    public const int BlockPixels = 16;

    /// <summary>Checks the sheet is coloured from the background palette.</summary>
    /// <exception cref="InvalidDataException">The sheet names the sprite colours.</exception>
    public RoomBlockSheet(TilesetDefinition tileset, SpriteSheet sheet, RoomBlockAppearances inputs)
    {
        Ensure.NotNull(sheet);
        Inputs = Ensure.NotNull(inputs);
        if (sheet.Image.Colours != SpriteImageColours.Background)
            throw new InvalidDataException($"Room block sheet {sheet.Image.Name} is not coloured from the background palette.");
        Tileset = tileset;
        Sheet = sheet;
    }

    /// <summary>The tileset whose blocks the sheet pictures.</summary>
    public TilesetDefinition Tileset { get; }

    /// <summary>The sheet.</summary>
    public SpriteSheet Sheet { get; }

    /// <summary>The appearance inputs each block of the tileset depends on (#1466).</summary>
    public RoomBlockAppearances Inputs { get; }

    /// <summary>The installed name of tileset <paramref name="tileset"/>'s block sheet.</summary>
    public static string NameOf(TilesetDefinition tileset) => tileset.GraphicsSet.ToString();

    /// <summary>The picture named <paramref name="name"/>.</summary>
    /// <exception cref="InvalidDataException">The sheet has no such picture, or it is not one block.</exception>
    public SpriteSheetFrame Picture(string name)
    {
        SpriteSheetFrame picture = Sheet.Frame(name);
        if (picture.Size.X != BlockPixels || picture.Size.Y != BlockPixels)
            throw new InvalidDataException($"Room block sheet {Sheet.Image.Name} picture {name} is not one block.");
        return picture;
    }
}
