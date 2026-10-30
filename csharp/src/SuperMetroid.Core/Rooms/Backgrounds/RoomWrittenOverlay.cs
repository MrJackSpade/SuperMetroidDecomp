using System.Collections.Immutable;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The pictures a room's actors wrote over a background (#1346, #1348, #1466): the Ceres escape warning over BG1, and the
/// escape typewriter's text over BG1 (Ceres) or BG2 (Zebes), in the order they were written, each at its tilemap cell. A
/// later picture covers an earlier one where they overlap. They stay, over what the background otherwise shows, until the
/// next room. Immutable; each write publishes a copy.
/// </summary>
public sealed class RoomWrittenOverlay
{
    /// <summary>Tilemap cells in a two-screen tilemap.</summary>
    public const int CellCount = 2 * ScreenGeometry.TilemapScreenCellCount;

    /// <summary>The overlay with nothing written.</summary>
    public static RoomWrittenOverlay Empty { get; } = new([]);

    /// <summary>Takes the pictures, in the order they were written.</summary>
    private RoomWrittenOverlay(ImmutableArray<WrittenPicture> pictures) => Pictures = pictures;

    /// <summary>The pictures, in the order they were written.</summary>
    public ImmutableArray<WrittenPicture> Pictures { get; }

    /// <summary>A copy with <paramref name="written"/> written over it.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The picture's cell lies outside the tilemap.</exception>
    public RoomWrittenOverlay With(WrittenPicture written)
    {
        ArgumentException.ThrowIfNullOrEmpty(written.Picture);
        if (written.Cell < 0 || written.Cell >= CellCount)
            throw new ArgumentOutOfRangeException(nameof(written), written.Cell, "A picture is written outside the two-screen tilemap.");
        return new(Pictures.Add(written));
    }
}
