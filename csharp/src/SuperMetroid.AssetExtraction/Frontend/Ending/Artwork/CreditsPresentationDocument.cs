using System.Diagnostics.CodeAnalysis;
namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>JSON schema for the complete ordered staff-credit script.</summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: the staff credits' lines with their packed tile palettes, which only the import reads to picture the credits rows; MapPresentationExtractor.Extract writes it to the map presentation's import data (MapPresentationFiles.ImportDocumentFiles), which the installer deletes before it publishes.")]
public sealed record CreditsPresentationDocument
{
    /// <summary>Gets the schema version required by <see cref="CreditsPresentationDefinitions.Version"/>.</summary>
    public required int Version { get; init; }

    /// <summary>Gets the editable lines in their fixed native display order.</summary>
    public required CreditsLineDocument[] Lines { get; init; }
}
