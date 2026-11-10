using System.Text.Json.Serialization;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// One variant of an enemy in the enemy sheet manifest (#1599): the enemy it shows, the installed sheet showing it in the
/// variant's colours, how encounters' looks change its frames and where a rotting corpse lies in its pictures.
/// </summary>
internal sealed record EnemyVariantDocument
{
    /// <summary>The enemy the variant shows, by definition name.</summary>
    public required string Enemy { get; init; }

    /// <summary>The installed sheet showing the enemy's frames in the variant's colours.</summary>
    public required string Sheet { get; init; }

    /// <summary>
    /// For each of the sheet's frames encounters' looks change, by frame name, the encounters whose looks change it (#1538),
    /// by encounter name in encounter order, each with the names of its looks the frame is drawn in. The sheet holds the
    /// frame's finished picture of each combination of those looks, one look of each encounter. Absent when no frame changes.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, Dictionary<string, string[]>>? LookChanges { get; init; }

    /// <summary>
    /// For each of the sheet's pictures that shows a rotting corpse, by picture name, where the corpse lies in it (#1539),
    /// which the game draws whole through the corpse's own rows as its rot leaves them. Absent when no picture shows one.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, CorpsePictureDocument>? CorpsePictures { get; init; }
}
