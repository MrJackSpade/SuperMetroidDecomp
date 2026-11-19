using SuperMetroid.Core.Rooms;
using SuperMetroid.AssetExtraction.Artwork;


namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>Defines the editable collection of gameplay-message notices.</summary>
public sealed record GameplayMessageNoticeDocument
{
    /// <summary>Gets the presentation schema version.</summary>
    public required int Version { get; init; }
    /// <summary>Gets the shared 32-cell border row.</summary>
    public required BackgroundTile[] Border { get; init; }
    /// <summary>Gets the notices keyed by <see cref="GameplayMessageId"/> name.</summary>
    public required Dictionary<string, GameplayMessageNotice> Notices { get; init; }
}
