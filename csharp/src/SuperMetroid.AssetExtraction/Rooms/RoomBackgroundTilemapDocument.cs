using System.Diagnostics.CodeAnalysis;
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Editable schema for one or two ordered native BG tilemap pages; contains visual references rather than room collision or metatile data.</summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: a room background's packed tile placements, which only the import reads to compose the background's pictures; RoomBackgroundTilemapArtworkFiles.Extract writes it to its component's import data, which the installer deletes before it publishes.")]
public sealed record RoomBackgroundTilemapDocument
{
    /// <summary>Schema revision; compilation requires <see cref="RoomBackgroundTilemapFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Caller-owned mutable array of one or two pages in native transfer order; page arrangement is determined by the consumer.</summary>
    public required RoomBackgroundTilemapPage[] Pages { get; init; }
}
