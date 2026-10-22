using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// One tileset's installed pre-coloured block pictures (#1392): every block its rooms can show, over each artwork that
/// can be bound over its characters, coloured with the tileset's standard background palette and named by
/// <see cref="RoomBlockPictureNames"/>.
/// </summary>
public sealed class RoomBlockSheet
{
    /// <summary>Pixels along each side of a block picture.</summary>
    public const int BlockPixels = 16;

    /// <summary>Checks the sheet is coloured from the background palette and every picture is one block.</summary>
    /// <exception cref="InvalidDataException">The sheet names the sprite colours or holds a picture that is not one block.</exception>
    public RoomBlockSheet(TilesetDefinition tileset, SpriteSheet sheet)
    {
        Ensure.NotNull(sheet);
        if (sheet.Image.Colours != SpriteImageColours.Background)
            throw new InvalidDataException($"Room block sheet {sheet.Image.Name} is not coloured from the background palette.");
        Tileset = tileset;
        Sheet = sheet;
    }

    /// <summary>The tileset whose blocks the sheet pictures.</summary>
    public TilesetDefinition Tileset { get; }

    /// <summary>The sheet.</summary>
    public SpriteSheet Sheet { get; }

    /// <summary>The installed name of tileset <paramref name="tileset"/>'s block sheet.</summary>
    public static string NameOf(TilesetDefinition tileset) => $"{tileset.Blocks}.{tileset.Characters}.{tileset.Palette}";

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
