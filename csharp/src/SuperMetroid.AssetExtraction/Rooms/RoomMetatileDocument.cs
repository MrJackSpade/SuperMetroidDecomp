using System.Diagnostics.CodeAnalysis;
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Editable JSON schema for one common-room-element or tileset-specific table of 16x16 visual blocks; collision and placement are not included.</summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: a tileset's packed block compositions, which only the import reads to compose the block sheets; RoomMetatileArtworkFiles.Extract writes it to its component's import data, which the installer deletes before it publishes.")]
public sealed record RoomMetatileDocument
{
    /// <summary>Schema revision, required to equal <see cref="RoomMetatileFormat.Version"/> when compiling the table.</summary>
    public required int Version { get; init; }
    /// <summary>Non-null, ordered block definitions whose count must match the selected native table length divided by eight.</summary>
    public required RoomMetatileDefinition[] Blocks { get; init; }
}
