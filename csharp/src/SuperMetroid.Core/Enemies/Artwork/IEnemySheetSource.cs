using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>The pre-coloured sheet of each enemy, and of the objects enemies spawn, as a room graphics set loads them (#1392).</summary>
public interface IEnemySheetSource
{
    /// <summary>The sheet of <paramref name="enemy"/> as graphics set <paramref name="set"/> loads it.</summary>
    /// <exception cref="InvalidDataException">The set does not load that enemy.</exception>
    SpriteSheet For(RoomEnemyGraphicsSetDefinition set, EnemyDefinitionId enemy);

    /// <summary>The sheet graphics set <paramref name="set"/> draws the objects <paramref name="owner"/> owns from.</summary>
    /// <exception cref="InvalidDataException">The set installs no sheet for that owner.</exception>
    SpawnedObjectSheet Spawned(RoomEnemyGraphicsSetDefinition set, SpawnedObjectOwner owner);

    /// <summary>
    /// The encounters whose looks change frame <paramref name="frame"/> of <paramref name="enemy"/>'s sheet under graphics set
    /// <paramref name="set"/> (#1538), in encounter order: the sheet holds the frame's finished picture of each look they
    /// show while it is drawn. None for a frame no encounter's look changes, which has one picture.
    /// </summary>
    IReadOnlyList<EncounterArtwork> ChangingEncounters(RoomEnemyGraphicsSetDefinition set, EnemyDefinitionId enemy, string frame);

    /// <summary>The same for frame <paramref name="frame"/> of the set's spawned-object sheet for <paramref name="owner"/>.</summary>
    IReadOnlyList<EncounterArtwork> SpawnedChangingEncounters(RoomEnemyGraphicsSetDefinition set, SpawnedObjectOwner owner, string frame);

    /// <summary>
    /// Where a rotting corpse lies in picture <paramref name="picture"/> of a frame of <paramref name="enemy"/>'s sheet under
    /// graphics set <paramref name="set"/> (#1539), which the picture is drawn whole through as the corpse's own rot leaves it; none for
    /// a picture that shows no rotting corpse.
    /// </summary>
    CorpsePicturePlacement? CorpsePlacement(RoomEnemyGraphicsSetDefinition set, EnemyDefinitionId enemy, string picture);

    /// <summary>The same for picture <paramref name="picture"/> of the set's spawned-object sheet for <paramref name="owner"/>.</summary>
    CorpsePicturePlacement? SpawnedCorpsePlacement(RoomEnemyGraphicsSetDefinition set, SpawnedObjectOwner owner, string picture);
}
