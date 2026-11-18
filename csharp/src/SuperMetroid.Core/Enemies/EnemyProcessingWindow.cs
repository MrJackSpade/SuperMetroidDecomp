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
    /// <summary>The species' work stepped at the start of the update, before enemy selection.</summary>
    private readonly IStepBeforeEnemySelection _stepBeforeSelection;
    /// <summary>The places traversed, in place order, while choosing active enemies.</summary>
    private readonly EnemyPlaces _places;

    /// <summary>Binds processing-window decisions to one room's frame policy and enemy pool.</summary>
    /// <param name="frameContext">The frame policy that can force off-screen enemies into the active list.</param>
    /// <param name="stepBeforeSelection">The species' work that steps at the start of the update, before selection.</param>
    /// <param name="places">The room's enemy places.</param>
    internal EnemyProcessingWindow(EnemyFrameContext frameContext, IStepBeforeEnemySelection stepBeforeSelection, EnemyPlaces places)
    {
        _frameContext = frameContext;
        _stepBeforeSelection = stepBeforeSelection;
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

    /// <summary>
    /// Steps the species' work that runs before selection (the Morph Ball eye's beam), then decides which enemies to process.
    /// </summary>
    /// <remarks>
    /// Main gameplay builds the window first, before Samus's input and movement, so the grapple's endpoint scan sees this
    /// update's interactive enemies, including one a death explosion respawned at the end of the previous update. The
    /// Morph Ball eye's beam steps earlier still, at the start of the update, which shows as it starts, a beam set up on
    /// one update beginning on the next, and as it ends, the full beam seeing the eye shut down on the update after.
    /// </remarks>
    /// <param name="camera">Camera position (viewport top-left) in whole room pixels.</param>
    public void PrepareEnemyProcessingList(Position16<RoomPixels> camera)
    {
        _stepBeforeSelection.StepBeforeEnemySelection();
        DetermineWhichEnemiesToProcess(camera);
    }

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
    internal static bool IsWithinProcessingWindow(
        RoomEnemyPlace place,
        Position16<RoomPixels> camera) =>
        !CoordinateAxis.Precedes(place.Radius.X + place.Position.X.Whole, camera.X) &&
        !CoordinateAxis.Precedes(place.Radius.X + camera.X + EnemyView.Extent, place.Position.X.Whole) &&
        !CoordinateAxis.Precedes(place.Position.Y.Whole + ProcessingWindowAboveView, camera.Y) &&
        !CoordinateAxis.Precedes(camera.Y + EnemyView.Extent - ProcessingWindowAboveView, place.Position.Y.Whole);

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
        if (CoordinateAxis.AsSigned(place.Position.X.Whole) < 0)
            return true;
        int fromCamera = CoordinateAxis.Wrap(place.Position.X.Whole + place.Radius.X - cameraX);
        return CoordinateAxis.AsSigned(fromCamera) < 0 || !CoordinateAxis.Precedes(fromCamera, EnemyView.Extent + place.Radius.X);
    }
}
