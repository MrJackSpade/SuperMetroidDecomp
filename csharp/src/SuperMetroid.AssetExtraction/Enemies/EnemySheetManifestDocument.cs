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
    /// For each graphics set whose spawned objects draw from sheets, by name, the spawned-object sheet of each tile base
    /// they draw at, by the base in decimal.
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
    /// For each graphics set by name, then each enemy by definition name and each of its frames drawn over a rotting
    /// corpse's rows by name, the name of the kind of corpse whose rot the frame shows (#1406). The enemy's sheet holds the
    /// frame's picture of each rot that corpse can show at which it shows anything. A frame drawn over no corpse's rows is
    /// absent.
    /// </summary>
    public required Dictionary<string, Dictionary<string, Dictionary<string, string>>> RotPictures { get; init; }

    /// <summary>The same for each set's spawned-object sheets, by the tile base in decimal in place of the enemy.</summary>
    public required Dictionary<string, Dictionary<string, Dictionary<string, string>>> SpawnedRotPictures { get; init; }

    /// <summary>
    /// For each graphics set by name, then each enemy by definition name, the names of its frames' pictures of the rots at
    /// which every character they read is blank (#1513): the frame shows nothing at that rot, so its sheet holds no such
    /// picture and the frame draws nothing then. An enemy none of whose frames shows nothing at a rot is absent.
    /// </summary>
    public required Dictionary<string, Dictionary<string, string[]>> BlankRotPictures { get; init; }

    /// <summary>The same for each set's spawned-object sheets, by owner in place of the enemy.</summary>
    public required Dictionary<string, Dictionary<string, string[]>> SpawnedBlankRotPictures { get; init; }
}
