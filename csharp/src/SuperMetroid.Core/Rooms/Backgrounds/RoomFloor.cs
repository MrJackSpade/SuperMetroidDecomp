using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The turning floor a Ceres room shows (#1349, #1467): its tileset's installed floor, and the pictures gameplay has drawn
/// over parts of it since the room loaded, by name. A picture drawn where another lies replaces it, so each part of the
/// floor shows the state last drawn there. Room state; each frame publishes it as an <see cref="AffineFloor"/>.
/// </summary>
public sealed class RoomFloor
{
    /// <summary>The names of the pictures drawn over the floor, in drawing order, each over a part no other covers.</summary>
    internal readonly List<string> drawn = [];

    /// <summary>The floor as drawn, until the next drawing.</summary>
    [NonSerialized] private AffineFloor? published;

    /// <summary>Starts from the tileset's floor with nothing drawn over it.</summary>
    /// <param name="sheet">The tileset's installed floor and the pictures drawn over it.</param>
    public RoomFloor(RoomFloorSheet sheet) => Sheet = Ensure.NotNull(sheet);

    /// <summary>The tileset's installed floor and the pictures drawn over it.</summary>
    public RoomFloorSheet Sheet { get; }

    /// <summary>Draws the picture named <paramref name="picture"/> over the part of the floor it covers, replacing what was drawn there.</summary>
    /// <exception cref="InvalidDataException">The floor's sheet holds no such picture.</exception>
    public void Draw(string picture)
    {
        SpriteSheetFrame drawing = Sheet.Picture(picture);
        drawn.RemoveAll(name => Sheet.Picture(name).Origin == drawing.Origin);
        drawn.Add(picture);
        published = null;
    }

    /// <summary>The floor the next frame shows; shared until the next drawing.</summary>
    public AffineFloor Publish() => published ??= new AffineFloor(Sheet.Floor.Image, [.. drawn.Select(Sheet.Picture)]);
}
