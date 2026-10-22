namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The installed enemy sheets' manifest (#1392): each room graphics set's sheet for each of its enemies, and for each tile
/// base its spawned objects draw at.
/// </summary>
public sealed record EnemySheetManifestDocument
{
    /// <summary>The manifest revision.</summary>
    public required int Version { get; init; }

    /// <summary>For each graphics set by name, the installed sheet name of each of its enemies by definition name.</summary>
    public required Dictionary<string, Dictionary<string, string>> Sets { get; init; }

    /// <summary>
    /// For each graphics set whose spawned objects draw from sheets, by name, the spawned-object sheet of each tile base
    /// they draw at, by the base in decimal.
    /// </summary>
    public required Dictionary<string, Dictionary<string, SpawnedObjectSheetDocument>> Spawned { get; init; }

    /// <summary>
    /// For each graphics set by name, then each enemy by definition name and each of its frames by name, the regions the
    /// frame is drawn over whose characters the set's encounters upload after the room loads; the enemy's sheet holds a
    /// picture of that frame for each upload each region can show. A frame drawn over none is absent.
    /// </summary>
    public required Dictionary<string, Dictionary<string, Dictionary<string, CharacterRegion[]>>> Pictures { get; init; }

    /// <summary>The same for each set's spawned-object sheets, by the tile base in decimal in place of the enemy.</summary>
    public required Dictionary<string, Dictionary<string, Dictionary<string, CharacterRegion[]>>> SpawnedPictures { get; init; }
}
