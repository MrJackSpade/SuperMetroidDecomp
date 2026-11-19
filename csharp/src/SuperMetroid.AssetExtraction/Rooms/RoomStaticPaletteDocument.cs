using System.Diagnostics.CodeAnalysis;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Editable JSON representation of one room graphics-set palette source.</summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: a tileset's base colours, which only the import reads to pre-colour the room sheets; RoomStaticPaletteArtworkFiles.Extract writes it to its component's import data, which the installer deletes before it publishes.")]
public sealed record RoomStaticPaletteDocument
{
    /// <summary>Gets the room-palette schema version.</summary>
    public required int Version { get; init; }

    /// <summary>Gets the exactly 128 RGB5 colors in native CGRAM order.</summary>
    public required PaletteRgb5[] Colors { get; init; }
}
