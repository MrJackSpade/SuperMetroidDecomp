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

    /// <summary>
    /// Shows the set's members' looks, after the standard sprites room setup selected (#1406, #1463): each member's look as
    /// the room loaded it, in set order.
    /// </summary>
    /// <remarks>
    /// The members' looks are shown after the standard sprites, as LoadEnemyTileData's copies follow the standard room
    /// graphics; the game queues them before any enemy AI's own transfer, which these showings also precede.
    /// </remarks>
    public void ShowGraphics()
    {
        _room.EnsureLoaded();
        for (int member = 0; member < LoadedSet.Records.Length; member++)
            _room.Session.SpriteLooks.Show(new GraphicsSetTileCharacters(LoadedSet, member));
    }

    /// <summary>Selects the set whose members' installed sheets the room draws from.</summary>
    internal void LoadGraphicsSet(RoomEnemyGraphicsSetDefinition graphicsSet) => LoadedSet = Ensure.NotNull(graphicsSet);
}
