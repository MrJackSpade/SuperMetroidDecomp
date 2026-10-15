using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>Editable JSON representation of a single ordered Kraid BG2 head frame; cells reference artwork rather than encoding animation or collision behavior.</summary>
[SpatialComponents("External format: the Kraid head tilemap JSON stores its declared grid dimensions as width/height integers.")]
public sealed record KraidHeadTilemapDocument
{
    /// <summary>Schema revision required to equal <see cref="KraidHeadTilemapFormat.Version"/> during loading.</summary>
    public required int Version { get; init; }
    /// <summary>Tilemap width in eight-pixel cells, required to be 32 rather than a pixel width or character-atlas stride.</summary>
    public required int Width { get; init; }
    /// <summary>Tilemap height in eight-pixel cells, required to be eleven.</summary>
    public required int Height { get; init; }
    /// <summary>Exactly 352 nonnull cells in 32-column row-major order, each retaining its character-atlas coordinates, BG palette, priority, and flip attributes.</summary>
    public required RoomBackgroundTilemapCell[] Cells { get; init; }
}
