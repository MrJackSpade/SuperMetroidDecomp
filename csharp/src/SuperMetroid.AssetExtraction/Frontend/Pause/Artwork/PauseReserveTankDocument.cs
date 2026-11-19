using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>Editable JSON schema for the pause screen's reserve-energy strip artwork and placement.</summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: the reserve tank's packed sprite parts, which only the import reads to compose its pictures; MapPresentationExtractor.Extract writes it to the map presentation's import data (MapPresentationFiles.ImportDocumentFiles), which the installer deletes before it publishes.")]
public sealed record PauseReserveTankDocument
{
    /// <summary>Gets the reserve-tank presentation schema version.</summary>
    public required int Version { get; init; }

    /// <summary>Gets the OBJ palette index, from zero through seven, applied to every reserve-strip part.</summary>
    public required int Palette { get; init; }

    /// <summary>Gets the six screen-pixel anchors for five tank positions and the trailing cap.</summary>
    public required Offset16<ScreenPixels>[] Anchors { get; init; }

    /// <summary>Gets the ten named full, end-cap, empty, and partial-fill sprite compositions.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
