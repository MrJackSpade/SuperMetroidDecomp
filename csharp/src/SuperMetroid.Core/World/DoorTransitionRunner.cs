using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Areas.Maridia;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
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
        _system = dependencies.System;
    }

    /// <summary>
    /// Samus's passage through the door, which the transition starts and finishes and the runtime hands to everything
    /// that holds still while she passes.
    /// </summary>
    internal DoorPassage Passage { get; } = new();

    /// <summary>
    /// Runs the active door's setup after room-object creation: its screen changes, then its room event.
    /// </summary>
    /// <remarks>
    /// The room load runs the door's setup between room-object
    /// construction and the first visible destination viewport. These setups are shared
    /// behaviour, not room-name patches: every door that names a setup receives the same
    /// screen changes and room event.
    /// </remarks>
    internal void RunDoorSetup(DoorDefinition door, DoorSetupRoom room)
    {
        door.Setup.ChangeScreens(room.Scrolls);

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
                // the door setup that owns the transition back to the ordinary display. Its
                // door leads out of the shaft, so the destination has no shaft's turn to end (#2287).
                _display.ActiveTurningFloor = null;
                _display.PublishDoorElevatorFloor(null);
                return;

            case DoorRoomEvent.StartsTreadmillRightwards:
                StartWreckedShipTreadmill(WreckedShipTreadmillDirection.Rightwards, room);
                return;

            case DoorRoomEvent.StartsTreadmillLeftwards:
                StartWreckedShipTreadmill(WreckedShipTreadmillDirection.Leftwards, room);
                return;

            case DoorRoomEvent.StartsElevatubeRideFromSouth:
                SetUpMaridiaElevatube(entry: ElevatubeEntry.FromSouth, room);
                return;

            case DoorRoomEvent.StartsElevatubeRideFromNorth:
                SetUpMaridiaElevatube(entry: ElevatubeEntry.FromNorth, room);
                return;

            case DoorRoomEvent.EndsElevatubeRide:
                // Both exit doors lead out of the tube, so the destination has no ride to end; the setup only restores
                // Samus's control (#2287).
                room.Samus.Control.Unlock();
                return;

            default:
                throw new InvalidOperationException(
                    $"Undefined {nameof(DoorRoomEvent)} {door.Setup.RoomEvent} in door {door.Name}.");
        }
    }

    /// <summary>Starts the room's entrance treadmill animation and attempts the matching entrance room-object spawn.</summary>
    private void StartWreckedShipTreadmill(WreckedShipTreadmillDirection direction, DoorSetupRoom room)
    {
        room.BlockAnimations.StartDoorTreadmill(direction);
        // The spawn reports whether the room had a free room-object place, and the door setup ignores it: with
        // every place held, the treadmill's blocks still animate and the entrance treadmill does not appear.
        _ = room.RoomObjects.WreckedShipTreadmill.TrySpawnWreckedShipEntranceTreadmill(
            room.Level,
            direction,
            _system.HasDefeatedAnyOf(room.Area, DefeatedBosses.AreaBoss));
    }

    /// <summary>Positions Samus for the selected elevatube entrance and attempts its room-object spawn.</summary>
    private static void SetUpMaridiaElevatube(ElevatubeEntry entry, DoorSetupRoom room)
    {
        MaridiaElevatubeRoomMainState maridiaElevatube = room.MaridiaElevatube ?? throw new InvalidDataException(
            $"Door setup event {entry} drives the elevatube in a room whose room behaviour is not the elevatube.");

        if (entry == ElevatubeEntry.FromSouth)
            maridiaElevatube.SetUpFromSouth(room.Samus);
        else
            maridiaElevatube.SetUpFromNorth(room.Samus);

        // The door setup ignores whether the spawn found a free room-object place: with all forty held, Samus is
        // still placed for the ride and the elevatube does not appear.
        _ = room.RoomObjects.MaridiaElevatube.TrySpawnMaridiaElevatube(room.Level);
    }
    /// <summary>Runs one update of the door's wait for its sounds to finish: enemies and drawing, without Samus's movement.</summary>
    public void RunDoorSoundWaitFrame(ControllerButton controllerInput, RoomScope room, SamusState samus)
    {
        RunDoorSoundWaitPrologue(controllerInput, room, samus);
        DrawDoorTransitionActors(enemyProjectiles: DoorTransitionEnemyProjectiles.Held, room, samus);
    }

    /// <summary>
    /// The opening of a door's sound-wait update: the frame boundary, then the liquid wave and statue descent and the random draw.
    /// The door's first update and each sound-wait update open this way while Samus is locked. On the first update it
    /// precedes the door's own step, so its room-FX sound requests are queued before the door cancels and disables
    /// sounds.
    /// </summary>
    public void RunDoorSoundWaitPrologue(ControllerButton controllerInput, RoomScope room, SamusState samus)
    {
        _frameBoundary.BeginAcceptedFrame(controllerInput);
        AdvanceEffectsAndRandom(room, samus);
    }

    /// <summary>The enemy and draw pass shared by the source door's waits and the fades.</summary>
    /// <param name="enemyProjectiles">
    /// Run for the destination fade-in; the source waits run enemies and drawing without
    /// the enemy projectiles.
    /// </param>
    /// <param name="room">The loaded room.</param>
    /// <param name="samus">Samus, whom the enemies touch and the pass draws.</param>
    internal void DrawDoorTransitionActors(DoorTransitionEnemyProjectiles enemyProjectiles, RoomScope room, SamusState samus)
    {
        _sprites.BeginFrame();
        _enemyPhase.RunEnemyUpdatePhase(processingList: EnemyProcessingList.Unprepared, room.Camera.PreviousSamusPoint, room, samus);
        // The Samus-collision pass belongs to gameplay only.
        if (enemyProjectiles == DoorTransitionEnemyProjectiles.Run)
            RunEnemyProjectiles(room, samus);
        _actorDrawing.DrawGameplayActors(room, samus, SamusDrawOwner.Gameplay, SamusColourEffects.Hold);
        // Drawing Samus every frame bypasses ordinary hurt flicker after the shared
        // draw. Elevator-owned drawing retains its separate alternating cadence.
        if (room.Enemies.Elevator.ElevatorStatus == ElevatorActorStatus.Inactive)
            samus.Presentation.DrawWithoutFlicker(samus, _sprites,
                room.Camera.PixelPosition, _display.ActiveTurningFloor);
        _sprites.FinalizeFrame();
    }
    /// <summary>
    /// Runs the enemy projectiles. Gameplay runs them every update, checking only the
    /// suspension X-ray applies, so their animations keep running while time is frozen for
    /// a reserve refill.
    /// </summary>
    internal void RunEnemyProjectiles(RoomScope room, SamusState samus)
    {
        if (samus.Xray.AreEnemyProjectilesSuspended)
            return;
        room.Enemies.AdvanceEnemyProjectileAnimations(
            room.LevelData, samus, room.Camera.PixelPosition, _clock.ShortCounter, room.BombProjectiles, _clock.Counter);
    }

    /// <summary>
    /// The opening of an update without gameplay: the liquid wave and statue descent, unless a door transition has stopped them
    /// (<see cref="LiquidWaveAndStatueDescentOrder"/>), then the update's random draw.
    /// </summary>
    internal void AdvanceEffectsAndRandom(RoomScope room, SamusState samus)
    {
        if (_liquidWaveAndStatueDescent.RunsThisUpdate)
        {
            _liquidWaveAndStatueDescent.Advance(room.RoomObjects, room);
            if (room.FxOverlay.MovesLiquidBeforeMainLoop)
                samus.LiquidPhysics.ConfigureFor(room.FxOverlay);
        }
        _system.NextRandom();
    }
}
