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
    /// For each graphics set by name, then each enemy by definition name and each of its frames by name, the frame's layers
    /// when it is drawn over characters the set's encounters upload after the room loads: its parts in draw order, in runs
    /// reading the same uploaded regions, each run with the regions it reads. The enemy's sheet holds a picture of each
    /// layer for each upload each of its regions can show. A frame drawn over none is absent and drawn whole.
    /// </summary>
    public required Dictionary<string, Dictionary<string, Dictionary<string, CharacterRegion[][]>>> Pictures { get; init; }

    /// <summary>The same for each set's spawned-object sheets, by the tile base in decimal in place of the enemy.</summary>
    public required Dictionary<string, Dictionary<string, Dictionary<string, CharacterRegion[][]>>> SpawnedPictures { get; init; }
}
