using SuperMetroid.Core.Enemies.Artwork;
namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The room's enemy graphics set: the ordered members whose installed sheets its enemies draw from (#1392, #1513). The
/// colours each member draws in are its installed sheets' (#1512), which the import pre-colours as the set loads them,
/// and where the set's artwork lands is the import's knowledge alone.
/// </summary>
internal sealed class EnemyGraphicsSet
{
    /// <summary>Supplies the selected enemy artwork and guards room-bound operations.</summary>
    private readonly EnemyRoomBinding _room;

    /// <summary>Binds the room whose artwork the graphics set selects.</summary>
    internal EnemyGraphicsSet(EnemyRoomBinding room) => _room = room;

    /// <summary>
    /// The graphics set whose installed sheets the room draws from: the one it loaded, or, when its population is empty
    /// and it loads none, the standard sprites alone (<see cref="RoomEnemyGraphicsSetDefinitions.SessionStart"/>).
    /// </summary>
    internal RoomEnemyGraphicsSetDefinition LoadedSet { get; private set; } = RoomEnemyGraphicsSetDefinitions.SessionStart;

    /// <summary>Selects the set whose members' installed sheets the room draws from.</summary>
    internal void LoadGraphicsSet(RoomEnemyGraphicsSetDefinition graphicsSet) => LoadedSet = Ensure.NotNull(graphicsSet);
}
