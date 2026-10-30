namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>JSON schema for the complete ordered staff-credit script.</summary>
public sealed record CreditsPresentationDocument
{
    /// <summary>Gets the schema version required by <see cref="CreditsPresentationDefinitions.Version"/>.</summary>
    public required int Version { get; init; }

    /// <summary>Gets the editable lines in their fixed native display order.</summary>
    public required CreditsLineDocument[] Lines { get; init; }
}
