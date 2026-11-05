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
    /// How frame <paramref name="frame"/> of <paramref name="enemy"/>'s sheet under graphics set <paramref name="set"/>
    /// follows the artwork sequences the set's encounters play (#1513); none for a frame no sequence changes.
    /// </summary>
    IReadOnlyList<FrameArtworkVariation> Variations(RoomEnemyGraphicsSetDefinition set, EnemyDefinitionId enemy, string frame);

    /// <summary>The same for frame <paramref name="frame"/> of the set's spawned-object sheet for <paramref name="owner"/>.</summary>
    IReadOnlyList<FrameArtworkVariation> SpawnedVariations(RoomEnemyGraphicsSetDefinition set, SpawnedObjectOwner owner, string frame);

    /// <summary>
    /// Where a rotting corpse lies in picture <paramref name="picture"/> of a frame of <paramref name="enemy"/>'s sheet under
    /// graphics set <paramref name="set"/> (#1531), which the picture is drawn through as the corpse's rot leaves it; none for
    /// a picture that shows no rotting corpse.
    /// </summary>
    CorpsePictureLayout? CorpseLayout(RoomEnemyGraphicsSetDefinition set, EnemyDefinitionId enemy, string picture);

    /// <summary>The same for picture <paramref name="picture"/> of the set's spawned-object sheet for <paramref name="owner"/>.</summary>
    CorpsePictureLayout? SpawnedCorpseLayout(RoomEnemyGraphicsSetDefinition set, SpawnedObjectOwner owner, string picture);

    /// <summary>
    /// Whether <paramref name="picture"/>, a picture of a frame of <paramref name="enemy"/>'s sheet under graphics set
    /// <paramref name="set"/>, shows nothing: every character the frame reads is blank in it, as the room's load, an
    /// encounter's artwork sequence leaves them, so the sheet holds no such picture and the frame draws
    /// nothing then (#1513).
    /// </summary>
    bool ShowsNothing(RoomEnemyGraphicsSetDefinition set, EnemyDefinitionId enemy, string picture);

    /// <summary>The same for a picture of a frame of the set's spawned-object sheet for <paramref name="owner"/>.</summary>
    bool SpawnedShowsNothing(RoomEnemyGraphicsSetDefinition set, SpawnedObjectOwner owner, string picture);
}
