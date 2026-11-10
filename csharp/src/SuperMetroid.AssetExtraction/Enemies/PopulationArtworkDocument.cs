namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The variants one population's enemies draw in, in the enemy sheet manifest (#1599): the import decodes the graphics set
/// the population's rooms load once, and names the variant each enemy drawn there shows and each owner's spawned objects show.
/// </summary>
internal sealed record PopulationArtworkDocument
{
    /// <summary>The variant each enemy the population's rooms draw shows, by enemy definition name.</summary>
    public required Dictionary<string, string> Enemies { get; init; }

    /// <summary>The spawned-object variant each owner's objects show, by owner name.</summary>
    public required Dictionary<string, string> Spawned { get; init; }
}
