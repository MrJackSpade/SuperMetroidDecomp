using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Selects the enemies the enemy pass processes this frame: the processing window, on-screen tests, and the active and
/// interactive enemies, each in <see cref="EnemyPlaceOrder"/>.
/// </summary>
[RoomLifetime]
internal sealed class EnemyProcessingWindow
{
    /// <summary>Frame policy that can force off-screen enemies into the active list.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>The places traversed, in place order, while choosing active enemies.</summary>
    private readonly EnemyPlaces _places;

    /// <summary>Binds processing-window decisions to one room's frame policy and enemy pool.</summary>
    /// <param name="frameContext">The frame policy that can force off-screen enemies into the active list.</param>
    /// <param name="places">The room's enemy places.</param>
    internal EnemyProcessingWindow(EnemyFrameContext frameContext, EnemyPlaces places)
    {
        _frameContext = frameContext;
        _places = places;
    }
    /// <summary>The enemies scheduled for AI this frame, including forced off-screen actors, in place order.</summary>
    private readonly List<RoomEnemyPlace> _activeEnemies = new();
    /// <summary>The active enemies still eligible for Samus interaction and grapple endpoint scans, in place order.</summary>
    private readonly List<RoomEnemyPlace> _interactiveEnemies = new();

    /// <summary>The enemies scheduled for AI this frame, in place order.</summary>
    internal IReadOnlyList<RoomEnemyPlace> ActiveEnemies => _activeEnemies;

    /// <summary>The active enemies still eligible for Samus interaction, in place order.</summary>
    internal IReadOnlyList<RoomEnemyPlace> InteractiveEnemies => _interactiveEnemies;

    /// <summary>Rebuilds active and interactive lists after clearing deleted places and applying off-screen policy.</summary>
    internal void DetermineWhichEnemiesToProcess(Position16<RoomPixels> camera)
    {
        _activeEnemies.Clear();
        _interactiveEnemies.Clear();
        foreach (RoomEnemyPlace place in _places.InUpdateOrder)
        {
            // A place with no enemy, one awaiting its enemy's respawn among them, is not processed.
            if (place.DefinitionId == EnemyDefinitionId.None)
                continue;
            if (place.Presence.HasAny(EnemyPresence.Deleted))
            {
                place.Unbind();
                continue;
            }

            bool active = _frameContext.ProcessAllEnemies ||
                place.Processing.HasAny(EnemyProcessing.OffScreen) ||
                (place.AiStates & EnemyAiStates.Frozen) != EnemyAiStates.None ||
                IsWithinProcessingWindow(place, camera);
            if (!active)
                continue;

            _activeEnemies.Add(place);
            if (!place.Contact.HasAny(EnemyContact.IgnoresSamus))
                _interactiveEnemies.Add(place);
        }
    }

    /// <summary>
    /// Whether the enemy lies within the processing window around the camera: its radius reaches the screen's width from
    /// the camera's left edge, and its centre lies from eight pixels above the camera to 248 pixels below it. The window
    /// selects the enemies an update processes, and also the enemies the draw pass reaches and the enemies Rios and
    /// pipe bugs treat as on screen.
    /// </summary>
    /// <remarks>
    /// The window measures across the room's axis, as <see cref="CameraPastRoomEdge.LiesBefore"/> reads it, because it tests
    /// every enemy and an enemy can lie above or left of the room's origin (#2537): a fly that has climbed through the
    /// ceiling, or a Bull braking past Samus, as <see cref="EnemyDeathsAndDrops"/> records. Its centre then
    /// lies at the end of the axis, and the window reads it the short way from the camera, across the axis. With the
    /// camera at or near the room's left edge, a body whose centre lies past that edge but whose radius reaches back past
    /// the camera is processed and drawn; with the camera at or near the room's top, so is a centre above that edge and no
    /// more than eight pixels above the camera. A plain difference would read either centre as far right of or below the
    /// view. The camera lies inside its room, at most 12575 pixels along either axis (#2441), so the wrap changes nothing
    /// for a centre inside the room.
    /// </remarks>
    internal static bool IsWithinProcessingWindow(
        RoomEnemyPlace place,
        Position16<RoomPixels> camera) =>
        !CameraPastRoomEdge.LiesBefore(place.Radius.X + place.Position.X.Whole, camera.X) &&
        !CameraPastRoomEdge.LiesBefore(place.Radius.X + camera.X + EnemyView.Extent, place.Position.X.Whole) &&
        !CameraPastRoomEdge.LiesBefore(place.Position.Y.Whole + ProcessingWindowAboveView, camera.Y) &&
        !CameraPastRoomEdge.LiesBefore(camera.Y + EnemyView.Extent - ProcessingWindowAboveView, place.Position.Y.Whole);

    /// <summary>How far above the view the processing window reaches, and so how far short of its bottom edge it stops.</summary>
    private const int ProcessingWindowAboveView = 8;

    /// <summary>Whether the enemy's centre lies in the view (<see cref="EnemyView.Contains"/>).</summary>
    internal static bool EnemyCenterIsOnScreen(RoomEnemyPlace place, Position16<RoomPixels> camera) =>
        EnemyView.Contains(place.Position.Whole, camera);

    /// <summary>
    /// A negative X, a right edge left of the camera, or a position at least the view's width right of the camera is
    /// off-screen.
    /// </summary>
    internal static bool EnemyIsHorizontallyOffScreen(RoomEnemyPlace place, int cameraX)
    {
        if (OffRoomPlaces.LiesBeforeRoom(place.Position.X.Whole))
            return true;
        int fromCamera = CameraPastRoomEdge.FromCamera(place.Position.X.Whole + place.Radius.X, cameraX);
        return fromCamera < 0 || !CameraPastRoomEdge.LiesBefore(fromCamera, EnemyView.Extent + place.Radius.X);
    }
}
