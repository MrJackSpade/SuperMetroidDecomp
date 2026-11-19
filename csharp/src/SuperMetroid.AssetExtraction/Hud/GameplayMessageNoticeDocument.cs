using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rooms;
using SuperMetroid.AssetExtraction.Artwork;


namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>Defines the editable collection of gameplay-message notices.</summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: the message notices' packed tile placements, which only the import reads to picture them; MapPresentationExtractor.Extract writes it to the map presentation's import data (MapPresentationFiles.ImportDocumentFiles), which the installer deletes before it publishes.")]
public sealed record GameplayMessageNoticeDocument
{
    /// <summary>Gets the presentation schema version.</summary>
    public required int Version { get; init; }
    /// <summary>Gets the shared 32-cell border row.</summary>
    public required BackgroundTile[] Border { get; init; }
    /// <summary>Gets the notices keyed by <see cref="GameplayMessageId"/> name.</summary>
    public required Dictionary<string, GameplayMessageNotice> Notices { get; init; }
}
