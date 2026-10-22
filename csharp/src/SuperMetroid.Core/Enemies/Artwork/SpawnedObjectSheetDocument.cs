namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>One spawned-object sheet in the enemy sheet manifest (#1392).</summary>
public sealed record SpawnedObjectSheetDocument
{
    /// <summary>The installed sheet name.</summary>
    public required string Sheet { get; init; }

    /// <summary>The palette row its frames were coloured in.</summary>
    public required int Palette { get; init; }

    /// <summary>The enemies whose spawned objects it holds, by definition name.</summary>
    public required List<string> Spawners { get; init; }

    /// <summary>Whether it also holds the objects any room spawns.</summary>
    public required bool Anywhere { get; init; }
}
