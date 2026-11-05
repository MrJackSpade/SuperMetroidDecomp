namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>One spawned-object sheet in the enemy sheet manifest (#1392).</summary>
internal sealed record SpawnedObjectSheetDocument
{
    /// <summary>The installed sheet name.</summary>
    public required string Sheet { get; init; }

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
}
