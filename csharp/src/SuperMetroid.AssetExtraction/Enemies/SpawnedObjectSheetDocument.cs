namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// One variant of an owner's spawned-object pictures in the enemy sheet manifest (#1392, #1513, #1599): the installed sheet
/// of the pictures particular to it, and the common sheets holding its other pictures, each of which every variant that
/// draws it reads.
/// </summary>
internal sealed record SpawnedObjectSheetDocument
{
    /// <summary>The installed sheet of the pictures no other spawned-object sheet holds; null when every picture is common.</summary>
    public required string? Sheet { get; init; }

    /// <summary>
    /// The installed common sheets the owner's other pictures are held on, by sheet name, each with the names of the
    /// pictures drawn from it (#1513). A picture is held on one sheet only.
    /// </summary>
    public required Dictionary<string, List<string>> CommonPictures { get; init; }

    /// <summary>The enemies whose spawned objects it holds, by definition name.</summary>
    public required List<string> Spawners { get; init; }

    /// <summary>The owner whose sheet it is, which the sheet is composed for, by owner name.</summary>
    public required string Owner { get; init; }

    /// <summary>On the shared sheet, the enemies whose room-graphics spawns it holds, by definition name.</summary>
    public required List<string> SharedSpawners { get; init; }

    /// <summary>On the shared sheet, the common spawners whose objects it holds, by spawner name (#1513).</summary>
    public required List<string> Common { get; init; }

    /// <summary>The owner's finished pictures that show nothing, which no sheet holds (#1538).</summary>
    public required List<string> Empty { get; init; }

    /// <summary>
    /// For each frame encounters' looks change, by frame name, the encounters whose looks change it (#1538), by encounter
    /// name in encounter order, each with the names of its looks the frame is drawn in; absent when no frame changes.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, Dictionary<string, string[]>>? LookChanges { get; init; }

    /// <summary>
    /// For each picture that shows a rotting corpse, by picture name, where the corpse lies in it (#1539); absent when none
    /// shows one.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, CorpsePictureDocument>? CorpsePictures { get; init; }
}
