namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The installed enemy sheets' manifest (#1392, #1599): every variant of each enemy and of each owner's spawned objects, by
/// the variant's name, the variants a room that places no enemies draws, and the variants each retail population draws.
/// </summary>
internal sealed record EnemySheetManifestDocument
{
    /// <summary>The manifest revision.</summary>
    public required int Version { get; init; }

    /// <summary>Every enemy variant, by name.</summary>
    public required Dictionary<string, EnemyVariantDocument> Variants { get; init; }

    /// <summary>
    /// Every variant of an owner's spawned objects, by name: where its pictures are drawn from, its own sheet and the common
    /// sheets holding common art once (#1513).
    /// </summary>
    public required Dictionary<string, SpawnedObjectSheetDocument> SpawnedVariants { get; init; }

    /// <summary>The variants a room that places no enemies draws its spawned objects in: each owner's standard variant.</summary>
    public required PopulationArtworkDocument Standard { get; init; }

    /// <summary>The variants each retail population that places enemies draws, by the population's name.</summary>
    public required Dictionary<string, PopulationArtworkDocument> Populations { get; init; }
}
