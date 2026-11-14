using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// One tileset's installed pre-coloured block pictures (#1392, #1466): every block some room of the tileset shows and no
/// other (#1636), for each value each appearance input it depends on can take, coloured with the tileset's standard
/// colours and named by <see cref="RoomBlockPictureNames"/>; with the inputs each of its blocks depends on, which choose
/// among them. A room that shows a block the sheet has no picture of fails, naming the sheet and the block.
/// </summary>
public sealed class RoomBlockSheet
{
    /// <summary>Pixels along each side of a block picture.</summary>
    public const int BlockPixels = 16;

    /// <summary>Checks the sheet names the backdrop its tileset's rooms show (#1544).</summary>
    /// <exception cref="InvalidDataException">The sheet names no backdrop colour.</exception>
    public RoomBlockSheet(TilesetDefinition tileset, SpriteSheet sheet, RoomBlockAppearances inputs)
    {
        Ensure.NotNull(sheet);
        Inputs = Ensure.NotNull(inputs);
        if (sheet.Image.Backdrop is null)
            throw new InvalidDataException($"Room block sheet {sheet.Image.Name} names no backdrop colour for its rooms.");
        Tileset = tileset;
        Sheet = sheet;
    }

    /// <summary>The tileset whose blocks the sheet pictures.</summary>
    public TilesetDefinition Tileset { get; }

    /// <summary>The sheet.</summary>
    public SpriteSheet Sheet { get; }

    /// <summary>The appearance inputs each block of the tileset depends on (#1466), and which blocks its rooms show (#1636).</summary>
    public RoomBlockAppearances Inputs { get; }

    /// <summary>The installed name of tileset <paramref name="tileset"/>'s block sheet.</summary>
    public static string NameOf(TilesetDefinition tileset) => tileset.GraphicsSet.ToString();

    /// <summary>
    /// The picture named <paramref name="name"/>, as its part drawn behind the actors passing in front of the room's back
    /// tiles and its part drawn in front of them (#1634); either part, or both, may show nothing.
    /// </summary>
    /// <exception cref="InvalidDataException">The sheet holds no such picture, or a part is not one block.</exception>
    public LayeredPicture Picture(string name)
    {
        LayeredPicture picture = PictureParts.Of(Sheet, name);
        foreach (SpriteSheetFrame part in picture.Parts)
        {
            if (part.Size.X != BlockPixels || part.Size.Y != BlockPixels)
                throw new InvalidDataException($"Room block sheet {Sheet.Image.Name} picture {name} is not one block.");
        }
        return picture;
    }

    /// <summary>The inputs block <paramref name="block"/>'s picture depends on (#1466).</summary>
    /// <exception cref="InvalidDataException">
    /// The sheet has no picture of the block, as the import found no room of the tileset that shows it (#1636).
    /// </exception>
    public ImmutableArray<AppearanceInput> InputsOf(int block)
    {
        if (!Inputs.Shows(block))
            throw new InvalidDataException(
                $"Room block sheet {Sheet.Image.Name} has no picture of block {block}: the import found no room of the tileset that shows it.");
        return Inputs.Of(block);
    }

    /// <summary>
    /// The picture block <paramref name="block"/> shows while the inputs it depends on, <paramref name="inputs"/>, take
    /// <paramref name="values"/>.
    /// </summary>
    /// <exception cref="InvalidDataException">The sheet has no picture of the block for those values (#1636), or its picture is not one block.</exception>
    public LayeredPicture BlockPicture(int block, ImmutableArray<AppearanceInput> inputs, ReadOnlySpan<string> values)
    {
        string name = RoomBlockPictureNames.Of(block, inputs, values);
        if (!PictureParts.Holds(Sheet, name))
            throw new InvalidDataException(
                $"Room block sheet {Sheet.Image.Name} has no picture of block {block} for its inputs' values: it holds no picture {name}.");
        return Picture(name);
    }

    /// <summary>The picture of <paramref name="collectible"/>'s frame, which its kind shows where it lies in the tileset's rooms (#1538).</summary>
    /// <exception cref="InvalidDataException">The sheet has no such picture: no room of the tileset places the kind.</exception>
    public LayeredPicture PictureOf(CollectiblePicture collectible) => Picture(RoomBlockPictureNames.OfCollectible(collectible));
}
