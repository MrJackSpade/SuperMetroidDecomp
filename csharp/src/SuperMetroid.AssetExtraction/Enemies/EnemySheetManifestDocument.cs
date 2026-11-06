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
    /// For each graphics set by name, then each enemy by definition name and each of its frames by name, how the frame
    /// follows each encounter artwork sequence that changes it (#1513), by the sequence's name: for each step, the step whose
    /// picture it shows then, or null for its loaded picture, the steps it is drawn at, and whether it is drawn before the
    /// first. The enemy's sheet holds the frame's picture of each combination of those steps it is drawn at. A frame no
    /// sequence changes is absent and drawn as its room's load leaves it.
    /// </summary>
    public required Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, FrameArtworkVariationDocument>>>> Variations { get; init; }

    /// <summary>The same for each set's spawned-object sheets, by the tile base in decimal in place of the enemy.</summary>
    public required Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, FrameArtworkVariationDocument>>>> SpawnedVariations { get; init; }

    /// <summary>
    /// For each graphics set by name, then each enemy by definition name and each of its frames that shows a rotting corpse
    /// by name, where the corpse lies in the frame's pictures (#1531), which the game draws through the corpse's rows as its
    /// rot leaves them. The enemy's sheet holds each such frame once. A frame that shows no rotting corpse is absent.
    /// </summary>
    public required Dictionary<string, Dictionary<string, Dictionary<string, CorpsePictureDocument>>> CorpsePictures { get; init; }

    /// <summary>The same for each set's spawned-object sheets, by owner in place of the enemy.</summary>
    public required Dictionary<string, Dictionary<string, Dictionary<string, CorpsePictureDocument>>> SpawnedCorpsePictures { get; init; }

    /// <summary>
    /// For each graphics set by name, then each enemy by definition name, the names of its frames' pictures in which every
    /// character they read is blank (#1513), as the room's load or an encounter's artwork sequence leaves
    /// them: the frame shows nothing while it shows such a picture, so its sheet holds none and the frame draws nothing then.
    /// An enemy none of whose pictures shows nothing is absent.
    /// </summary>
    public required Dictionary<string, Dictionary<string, string[]>> BlankPictures { get; init; }

    /// <summary>The same for each set's spawned-object sheets, by owner in place of the enemy.</summary>
    public required Dictionary<string, Dictionary<string, string[]>> SpawnedBlankPictures { get; init; }
}
