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

    /// <summary>
    /// How frame <paramref name="frame"/> of <paramref name="enemy"/>'s sheet under graphics set <paramref name="set"/>
    /// follows the artwork sequences the set's encounters play (#1513); none for a frame no sequence changes.
    /// </summary>
    IReadOnlyList<FrameArtworkVariation> Variations(RoomEnemyGraphicsSetDefinition set, EnemyDefinitionId enemy, string frame);

    /// <summary>The same for frame <paramref name="frame"/> of the set's spawned-object sheet for <paramref name="tileBase"/>.</summary>
    IReadOnlyList<FrameArtworkVariation> SpawnedVariations(RoomEnemyGraphicsSetDefinition set, int tileBase, string frame);

    /// <summary>
    /// The name of the kind of rotting corpse whose rot frame <paramref name="frame"/> of <paramref name="enemy"/>'s sheet
    /// under graphics set <paramref name="set"/> shows, when the frame is drawn over the corpse's rows: the sheet holds the
    /// frame's picture of each rot the corpse can show (#1406). None for a frame that shows no rot.
    /// </summary>
    string? RotCorpse(RoomEnemyGraphicsSetDefinition set, EnemyDefinitionId enemy, string frame);

    /// <summary>The same for frame <paramref name="frame"/> of the set's spawned-object sheet for <paramref name="tileBase"/>.</summary>
    string? SpawnedRotCorpse(RoomEnemyGraphicsSetDefinition set, int tileBase, string frame);
}
