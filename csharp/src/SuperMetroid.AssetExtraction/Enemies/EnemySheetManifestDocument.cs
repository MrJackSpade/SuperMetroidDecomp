namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The installed enemy sheets' manifest (#1392): each room graphics set's sheet for each of its enemies, and for each tile
/// base its spawned objects draw at.
/// </summary>
internal sealed record EnemySheetManifestDocument
{
    /// <summary>The manifest revision.</summary>
    public required int Version { get; init; }

    /// <summary>For each graphics set by name, the installed sheet name of each of its enemies by definition name.</summary>
    public required Dictionary<string, Dictionary<string, string>> Sets { get; init; }

    /// <summary>
    /// For each graphics set whose spawned objects draw from sheets, by name, where each owner of its spawned objects, by
    /// owner name, draws its pictures from: the set's own sheet and the common sheets holding common art once (#1513).
    /// </summary>
    public required Dictionary<string, Dictionary<string, SpawnedObjectSheetDocument>> Spawned { get; init; }

    /// <summary>
    /// For each graphics set by name, then each enemy by definition name and each of its frames by name, the encounters whose
    /// looks change the frame (#1538), by encounter name in encounter order, each with the names of its looks the frame is
    /// drawn in. The enemy's sheet holds the frame's finished picture of each combination of those looks, one look of each
    /// encounter. A frame no encounter's looks change is absent and has one picture.
    /// </summary>
    public required Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, string[]>>>> LookChanges { get; init; }

    /// <summary>The same for each set's spawned-object sheets, by owner in place of the enemy.</summary>
    public required Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, string[]>>>> SpawnedLookChanges { get; init; }

    /// <summary>
    /// For each graphics set by name, then each enemy by definition name and each of its frames that shows a rotting corpse
    /// by name, where the corpse lies in the frame's pictures (#1531), which the game draws through the corpse's rows as its
    /// rot leaves them. The enemy's sheet holds each such frame once. A frame that shows no rotting corpse is absent.
    /// </summary>
    public required Dictionary<string, Dictionary<string, Dictionary<string, CorpsePictureDocument>>> CorpsePictures { get; init; }

    /// <summary>The same for each set's spawned-object sheets, by owner in place of the enemy.</summary>
    public required Dictionary<string, Dictionary<string, Dictionary<string, CorpsePictureDocument>>> SpawnedCorpsePictures { get; init; }

}
