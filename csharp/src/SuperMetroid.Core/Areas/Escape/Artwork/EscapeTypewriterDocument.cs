namespace SuperMetroid.Core.Areas.Escape.Artwork;

/// <summary>Editable warning schema containing the exact two named escape programs.</summary>
public sealed record EscapeTypewriterDocument
{
    /// <summary>Schema revision; loading requires <see cref="EscapeTypewriterDefinitions.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Caller-owned mutable dictionary with exactly the case-sensitive keys Ceres and Zebes; None is not a document key.</summary>
    public required Dictionary<string, EscapeTypewriterProgramDocument> Programs { get; init; }
}
