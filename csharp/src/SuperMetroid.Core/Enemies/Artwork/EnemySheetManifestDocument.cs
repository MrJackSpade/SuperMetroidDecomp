namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>The installed enemy sheets' manifest (#1392): each room graphics set's sheet for each of its enemies.</summary>
public sealed record EnemySheetManifestDocument
{
    /// <summary>The manifest revision.</summary>
    public required int Version { get; init; }

    /// <summary>For each graphics set by name, the installed sheet name of each of its enemies by definition name.</summary>
    public required Dictionary<string, Dictionary<string, string>> Sets { get; init; }
}
