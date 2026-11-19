using System.Diagnostics.CodeAnalysis;
namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>Editable JSON schema for the pause equipment page's static base tilemap.</summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: the equipment screen's packed tile placements, which only the import reads to picture it; MapPresentationExtractor.Extract writes it to the map presentation's import data (MapPresentationFiles.ImportDocumentFiles), which the installer deletes before it publishes.")]
public sealed record PauseEquipmentBaseDocument
{
    /// <summary>Must match the installed equipment-base format before any cell is accepted.</summary>
    public required int Version { get; init; }

    /// <summary>Gets all 1024 BG tile references in 32-column row-major order.</summary>
    public required PauseBackdropCell[] Cells { get; init; }
}
