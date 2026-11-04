namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Whose spawned-object sheet a projectile or sprite object draws from (#1392, #1513): the room's shared sheet, which
/// holds the objects any room spawns and the frames encounters draw themselves; the sheet of the enemy that spawned it,
/// which holds that enemy's objects in its colours; or the sheet of the shards and sparkles Mother Brain's tank glass
/// throws. The import composes each sheet; where its pictures come from is the import's knowledge alone.
/// </summary>
public readonly record struct SpawnedObjectOwner
{
    /// <summary>Which kind of sheet an owner names.</summary>
    private enum SheetKind
    {
        /// <summary>The room's shared sheet.</summary>
        Shared,
        /// <summary>The sheet of the objects one enemy spawns.</summary>
        Spawner,
        /// <summary>The sheet of Mother Brain's tank glass.</summary>
        MotherBrainTankGlass,
    }

    /// <summary>An owner naming <paramref name="spawner"/>'s sheet of kind <paramref name="kind"/>.</summary>
    private SpawnedObjectOwner(EnemyDefinitionId? spawner, SheetKind kind)
    {
        Spawner = spawner;
        Kind = kind;
    }

    /// <summary>The room's shared sheet.</summary>
    public static SpawnedObjectOwner Shared => new(null, SheetKind.Shared);

    /// <summary>The sheet of the shards and sparkles Mother Brain's tank glass throws as it breaks.</summary>
    public static SpawnedObjectOwner MotherBrainTankGlass => new(EnemyDefinitionId.MotherBrainBody, SheetKind.MotherBrainTankGlass);

    /// <summary>The sheet of the objects <paramref name="spawner"/> spawns.</summary>
    public static SpawnedObjectOwner Of(EnemyDefinitionId spawner) => new(spawner, SheetKind.Spawner);

    /// <summary>The enemy whose objects the sheet holds, or null for the shared sheet.</summary>
    public EnemyDefinitionId? Spawner { get; }

    /// <summary>Which kind of sheet the owner names.</summary>
    private SheetKind Kind { get; }

    /// <summary>Whether the sheet is the one Mother Brain's tank glass draws from, apart from her other objects.</summary>
    public bool IsMotherBrainTankGlass => Kind == SheetKind.MotherBrainTankGlass;

    /// <summary>Names the owner for diagnostics and installed sheet names.</summary>
    public override string ToString() => IsMotherBrainTankGlass ? "MotherBrainTankGlass"
        : Spawner is { } spawner ? spawner.ToString() : "Shared";
}
