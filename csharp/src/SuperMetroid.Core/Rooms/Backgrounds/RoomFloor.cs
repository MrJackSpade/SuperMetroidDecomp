using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The turning floor a Ceres room shows (#1349, #1467): its tileset's installed floor, and the pictures gameplay has drawn
/// over parts of it since the room loaded, by floor animation frame. A picture drawn where another lies replaces it, so each part of the
/// floor shows the state last drawn there. Room state; each frame publishes it as an <see cref="AffineFloor"/>.
/// </summary>
public sealed class RoomFloor
{
    /// <summary>The floor animation frames drawn over the floor, in drawing order, each over a part no other covers.</summary>
    internal readonly List<CeresFloorAnimationFrame> drawn = [];

    /// <summary>The floor as drawn, until the next drawing.</summary>
    private AffineFloor? published;

    /// <summary>Starts from the tileset's floor with nothing drawn over it.</summary>
    /// <param name="sheet">The tileset's installed floor and the pictures drawn over it.</param>
    public RoomFloor(RoomFloorSheet sheet) => Sheet = Ensure.NotNull(sheet);

    /// <summary>The tileset's installed floor and the pictures drawn over it.</summary>
    public RoomFloorSheet Sheet { get; }

    /// <summary>Draws <paramref name="frame"/>'s picture over the part of the floor it covers, replacing what was drawn there.</summary>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> is not a floor animation frame.</exception>
    public void Draw(CeresFloorAnimationFrame frame)
    {
        SpriteSheetFrame drawing = Sheet.Picture(frame);
        drawn.RemoveAll(other => Sheet.Picture(other).Origin == drawing.Origin);
        drawn.Add(frame);
        published = null;
    }

    /// <summary>The floor the next frame shows; shared until the next drawing.</summary>
    public AffineFloor Publish() => published ??= new AffineFloor(Sheet.Floor.Image, [.. drawn.Select(Sheet.Picture)]);
}
