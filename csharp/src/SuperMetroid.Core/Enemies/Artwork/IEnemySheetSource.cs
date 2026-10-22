using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>The pre-coloured sheet of each enemy, and of the objects enemies spawn, as a room graphics set loads them (#1392).</summary>
public interface IEnemySheetSource
{
    /// <summary>The sheet of <paramref name="enemy"/> as graphics set <paramref name="set"/> loads it.</summary>
    /// <exception cref="InvalidDataException">The set does not load that enemy.</exception>
    SpriteSheet For(RoomEnemyGraphicsSetDefinition set, EnemyDefinitionId enemy);

    /// <summary>
    /// Whether graphics set <paramref name="set"/>'s spawned objects draw from sheets. A set loading an enemy that draws
    /// from characters it uploads keeps the objects spawned there on characters too, as their characters change with it.
    /// </summary>
    bool SpawnsFromSheets(RoomEnemyGraphicsSetDefinition set);

    /// <summary>The sheet graphics set <paramref name="set"/> draws the objects spawned at <paramref name="tileBase"/> from.</summary>
    /// <exception cref="InvalidDataException">No spawner the set loads draws at that base.</exception>
    SpawnedObjectSheet Spawned(RoomEnemyGraphicsSetDefinition set, int tileBase);

    /// <summary>The uploads each encounter makes over the characters, whose pictures the sheets hold.</summary>
    RuntimeCharacterUploadCatalog Uploads { get; }

    /// <summary>
    /// The regions frame <paramref name="frame"/> of <paramref name="enemy"/>'s sheet under graphics set
    /// <paramref name="set"/> is drawn over whose characters the set's encounters upload after the room loads; none for a
    /// frame the set's own characters alone show.
    /// </summary>
    IReadOnlyList<CharacterRegion> Regions(RoomEnemyGraphicsSetDefinition set, EnemyDefinitionId enemy, string frame);

    /// <summary>The same for frame <paramref name="frame"/> of the set's spawned-object sheet for <paramref name="tileBase"/>.</summary>
    IReadOnlyList<CharacterRegion> SpawnedRegions(RoomEnemyGraphicsSetDefinition set, int tileBase, string frame);
}
