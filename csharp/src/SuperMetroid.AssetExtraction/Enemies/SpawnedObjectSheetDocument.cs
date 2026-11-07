namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// One owner's spawned-object pictures in the enemy sheet manifest (#1392, #1513): the installed sheet of the pictures
/// particular to its graphics set, and the common sheets holding its other pictures, each of which every set that draws it
/// reads.
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

    /// <summary>The palette row its frames were coloured in.</summary>
    public required int Palette { get; init; }

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
}
