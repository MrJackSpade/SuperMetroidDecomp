using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>The pre-coloured sheet of each enemy, and of the objects enemies spawn, as a room graphics set loads them (#1392).</summary>
public interface IEnemySheetSource
{
    /// <summary>The sheet of <paramref name="enemy"/> as graphics set <paramref name="set"/> loads it.</summary>
    /// <exception cref="InvalidDataException">The set does not load that enemy.</exception>
    SpriteSheet For(RoomEnemyGraphicsSetDefinition set, EnemyDefinitionId enemy);

    /// <summary>The sheet graphics set <paramref name="set"/> draws the objects spawned at <paramref name="tileBase"/> from.</summary>
    /// <exception cref="InvalidDataException">No spawner the set loads draws at that base.</exception>
    SpawnedObjectSheet Spawned(RoomEnemyGraphicsSetDefinition set, int tileBase);

    /// <summary>The looks each encounter shows, whose pictures the sheets hold.</summary>
    IEncounterLookCatalog Looks { get; }

    /// <summary>What covers each region of graphics set <paramref name="set"/>'s frames (#1406, #1463).</summary>
    SetLookCoverage Coverage(RoomEnemyGraphicsSetDefinition set);

    /// <summary>
    /// The layers frame <paramref name="frame"/> of <paramref name="enemy"/>'s sheet under graphics set
    /// <paramref name="set"/> draws as, front first, each with the regions it reads that the set's encounters show looks over
    /// after the room loads; none for a frame drawn whole, as the set's own look alone shows it.
    /// </summary>
    IReadOnlyList<LookRegion[]> Layers(RoomEnemyGraphicsSetDefinition set, EnemyDefinitionId enemy, string frame);

    /// <summary>The same for frame <paramref name="frame"/> of the set's spawned-object sheet for <paramref name="tileBase"/>.</summary>
    IReadOnlyList<LookRegion[]> SpawnedLayers(RoomEnemyGraphicsSetDefinition set, int tileBase, string frame);

    /// <summary>
    /// The name of the kind of rotting corpse whose rot frame <paramref name="frame"/> of <paramref name="enemy"/>'s sheet
    /// under graphics set <paramref name="set"/> shows, when the frame is drawn over the corpse's rows: the sheet holds the
    /// frame's picture of each rot the corpse can show (#1406). None for a frame that shows no rot.
    /// </summary>
    string? RotCorpse(RoomEnemyGraphicsSetDefinition set, EnemyDefinitionId enemy, string frame);

    /// <summary>The same for frame <paramref name="frame"/> of the set's spawned-object sheet for <paramref name="tileBase"/>.</summary>
    string? SpawnedRotCorpse(RoomEnemyGraphicsSetDefinition set, int tileBase, string frame);
}
