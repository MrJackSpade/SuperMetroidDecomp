using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Maridia;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.World;

/// <summary>
/// Door transitions: the door's setup (Wrecked Ship treadmill, Maridia elevatube, screen changes) and the
/// door sound-wait frames with their actor drawing.
/// </summary>
internal sealed class DoorTransitionRunner
{
    /// <summary>Draws Samus and room actors during door waits and fades.</summary>
    private readonly GameplayActorDrawing _actorDrawing;
    /// <summary>Holds the active and the shown display transforms across transitions.</summary>
    private readonly GameplayDisplayState _display;
    /// <summary>Runs the enemy main phase while Samus gameplay is suspended.</summary>
    private readonly EnemyFramePhase _enemyPhase;
    /// <summary>Advances the liquid wave and the statues at the start of a door update.</summary>
    private readonly LiquidWaveAndStatueDescent _liquidWaveAndStatueDescent;
    /// <summary>Provides the frame counters to enemy projectile processing.</summary>
    private readonly FrameClock _clock;
    /// <summary>Shows the previous frame and latches input at the start of each sound-wait update, before the door's step.</summary>
    private readonly AcceptedFrameBoundary _frameBoundary;
    /// <summary>Sprite frame begun and finalized around each drawn frame.</summary>
    private readonly SpriteFrameBuffer _sprites;
    /// <summary>Provides the locked Samus state and its presentation during transitions.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Holds destination room data, camera, and background streamer.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Provides defeated bosses and advances main-loop random state.</summary>
    private readonly SharedGameState _system;

    /// <summary>Captures the runtime services used by the door's setup and its wait updates.</summary>
    internal DoorTransitionRunner(DoorTransitionRunnerDependencies dependencies)
    {
        _actorDrawing = dependencies.ActorDrawing;
        _display = dependencies.Display;
        _enemyPhase = dependencies.EnemyPhase;
        _liquidWaveAndStatueDescent = dependencies.LiquidWaveAndStatueDescent;
        _clock = dependencies.Clock;
        _frameBoundary = dependencies.FrameBoundary;
        _sprites = dependencies.Sprites;
        _player = dependencies.Player;
        _room = dependencies.Room;
        _system = dependencies.System;
    }
    /// <summary>
    /// Runs the active door's setup after room-object creation: its screen changes, then its room event.
    /// </summary>
    /// <remarks>
    /// The room load runs the door's setup between room-object
    /// construction and the first visible destination viewport. These setups are shared
    /// behaviour, not room-name patches: every door that names a setup receives the same
    /// screen changes and room event.
    /// </remarks>
    internal void RunDoorSetup(DoorDefinition door, RoomObjectSystem roomObjects, WreckedShipTreadmillBlockAnimationsState wreckedShipTreadmill, CeresElevatorShaftRoomMainState ceresElevatorShaft, MaridiaElevatubeRoomMainState maridiaElevatube)
    {
        if (_room.Camera is null)
            throw new InvalidOperationException("Door setup requires the destination scroll grid.");

        door.Setup.ChangeScreens(_room.Camera.Scrolls);

        switch (door.Setup.RoomEvent)
        {
            case DoorRoomEvent.None:
                return;

            case DoorRoomEvent.ShowsTurningFloor:
                // Ceres's door setup owns the turning floor's rotation and centre. Those
                // values must exist before room graphics/actors initialize, so the shared
                // loader installs them at construction time and this point merely verifies
                // that the recognized setup was not discarded.
                if (_display.ActiveTurningFloor is null)
                {
                    throw new InvalidDataException(
                        "Ceres elevator door setup reached its room event without its turning-floor transform.");
                }
                return;

            case DoorRoomEvent.EndsTurningFloor:
                // The paired exit setup of the Ceres elevator shaft's entry: it returns
                // the screen from the turning floor to the ordinary room display, removing
                // both the active and the shown transforms. Do this explicitly here rather
                // than relying on the destination loader's defensive state reset: this is
                // the door setup that owns the transition back to the ordinary display.
                _display.ActiveTurningFloor = null;
                _display.PublishDoorElevatorFloor(null);
                ceresElevatorShaft.Deactivate();
                return;

            case DoorRoomEvent.StartsTreadmillRightwards:
                StartWreckedShipTreadmill(WreckedShipTreadmillDirection.Rightwards, roomObjects, wreckedShipTreadmill);
                return;

            case DoorRoomEvent.StartsTreadmillLeftwards:
                StartWreckedShipTreadmill(WreckedShipTreadmillDirection.Leftwards, roomObjects, wreckedShipTreadmill);
                return;

            case DoorRoomEvent.StartsElevatubeRideFromSouth:
                SetUpMaridiaElevatube(entry: ElevatubeEntry.FromSouth, roomObjects, maridiaElevatube);
                return;

            case DoorRoomEvent.StartsElevatubeRideFromNorth:
                SetUpMaridiaElevatube(entry: ElevatubeEntry.FromNorth, roomObjects, maridiaElevatube);
                return;

            case DoorRoomEvent.EndsElevatubeRide:
                maridiaElevatube.ResetOnExit(_player.Samus ?? throw new InvalidOperationException(
                    "Maridia elevatube exit setup requires an active Samus state."));
                return;

            default:
                throw new InvalidOperationException(
                    $"Undefined {nameof(DoorRoomEvent)} {door.Setup.RoomEvent} in door {door.Name}.");
        }
    }

    /// <summary>Starts the treadmills' block animations and attempts the matching entrance room-object spawn.</summary>
    private void StartWreckedShipTreadmill(WreckedShipTreadmillDirection direction, RoomObjectSystem roomObjects, WreckedShipTreadmillBlockAnimationsState wreckedShipTreadmill)
    {
        if (_room.LevelData is null || _room.ActiveRoom is null)
        {
            throw new InvalidOperationException(
                "Wrecked Ship treadmill door setup requires an active destination room.");
        }

        wreckedShipTreadmill.Start(direction);
        // Both spawn helpers report when they have no free slot left, and
        // the door setup deliberately ignores that result. The dedicated animated owner
        // above cannot exhaust; preserve the room-object allocator's false result here.
        _ = roomObjects.WreckedShipTreadmill.TrySpawnWreckedShipEntranceTreadmill(
            _room.LevelData,
            direction,
            _system.HasDefeatedAnyOf(_room.ActiveRoom.AreaIndex, DefeatedBosses.AreaBoss));
    }

    /// <summary>Positions Samus for the selected elevatube entrance and attempts its room-object spawn.</summary>
    private void SetUpMaridiaElevatube(ElevatubeEntry entry, RoomObjectSystem roomObjects, MaridiaElevatubeRoomMainState maridiaElevatube)
    {
        if (_room.LevelData is null)
            throw new InvalidOperationException("Maridia elevatube setup requires active level data.");
        SamusState samus = _player.Samus ?? throw new InvalidOperationException(
            "Maridia elevatube setup requires an active Samus state.");

        if (entry == ElevatubeEntry.FromSouth)
            maridiaElevatube.SetUpFromSouth(samus);
        else
            maridiaElevatube.SetUpFromNorth(samus);

        // The door setup ignores whether the spawn found a free slot when all forty are
        // occupied. Retaining the bool-returning allocator preserves that edge.
        _ = roomObjects.WreckedShipTreadmill.TrySpawnMaridiaElevatube(_room.LevelData);
    }
    /// <summary>Enemy and draw owners of the door sound-drain coroutine, without Samus movement.</summary>
    public void RunDoorSoundWaitFrame(ControllerButton controllerInput, RoomScope room)
    {
        RunDoorSoundWaitPrologue(controllerInput, room.RoomObjects, room);
        DrawDoorTransitionActors(enemyProjectiles: DoorTransitionEnemyProjectiles.Held, room);
    }

    /// <summary>
    /// The opening of a door's sound-wait update: the frame boundary, then the liquid wave and statue descent and the random draw.
    /// The door's first update and each sound-wait update open this way while Samus is locked. On the first update it
    /// precedes the door's own step, so its room-FX sound requests are queued before the door cancels and disables
    /// sounds.
    /// </summary>
    public void RunDoorSoundWaitPrologue(ControllerButton controllerInput, RoomObjectSystem roomObjects, RoomScope room)
    {
        _frameBoundary.BeginAcceptedFrame(controllerInput);
        AdvanceEffectsAndRandom(roomObjects, room);
    }

    /// <summary>The enemy and draw pass shared by the source door's waits and the fades.</summary>
    /// <param name="enemyProjectiles">
    /// Run for the destination fade-in; the source waits run enemies and drawing without
    /// the enemy projectiles.
    /// </param>
    /// <param name="room">The loaded room.</param>
    internal void DrawDoorTransitionActors(DoorTransitionEnemyProjectiles enemyProjectiles, RoomScope room)
    {
        _sprites.BeginFrame();
        _enemyPhase.RunEnemyMainPhase(processingList: EnemyProcessingList.Unprepared, _room.Camera?.PreviousSamusPoint, room);
        // The Samus-collision pass belongs to gameplay only.
        if (enemyProjectiles == DoorTransitionEnemyProjectiles.Run)
            RunEnemyProjectileHandler(room.BombProjectiles, room);
        _actorDrawing.DrawGameplayActors(room, SamusDrawOwner.Gameplay, SamusPaletteEffects.Hold);
        // Drawing Samus every frame bypasses ordinary hurt flicker after the shared
        // draw. Elevator-owned drawing retains its separate alternating cadence.
        if (room.Enemies.Elevator.ElevatorStatus == ElevatorActorStatus.Inactive && _player.Samus is not null && _room.Camera is not null)
            _player.Samus.Presentation.Draw(_player.Samus, _sprites,
                _room.Camera.PixelPosition, turningFloor: _display.ActiveTurningFloor);
        _sprites.FinalizeFrame();
    }
    /// <summary>
    /// Runs the enemy projectiles. Gameplay runs them every update, checking only the
    /// suspension X-ray applies, so their animations keep running while time is frozen for
    /// a reserve refill.
    /// </summary>
    internal void RunEnemyProjectileHandler(SamusBombProjectileSystem bombProjectiles, RoomScope room)
    {
        if (_player.Samus?.Xray.AreEnemyProjectilesSuspended == true || _room.LevelData is null || _room.Camera is null)
            return;
        room.Enemies.AdvanceEnemyProjectileAnimations(
            _room.LevelData, _player.Samus, _room.Camera.PixelPosition, _clock.ShortCounter, bombProjectiles, _clock.Counter);
    }

    /// <summary>
    /// The opening of an update without gameplay: the liquid wave and statue descent, unless a door transition has stopped them
    /// (<see cref="EffectPhaseRandomOrder"/>), then the update's random draw.
    /// </summary>
    internal void AdvanceEffectsAndRandom(RoomObjectSystem roomObjects, RoomScope room)
    {
        if (_liquidWaveAndStatueDescent.RunsThisUpdate)
        {
            _liquidWaveAndStatueDescent.Advance(roomObjects, room);
            if (_player.Samus is not null && room.FxOverlay.MovesLiquidBeforeMainLoop)
                _player.Samus.LiquidPhysics.ConfigureFor(room.FxOverlay);
        }
        _system.NextRandom();
    }
}
