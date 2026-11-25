using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.World;

/// <summary>Door transitions: the door sound-wait frames with their actor drawing.</summary>
internal sealed partial class DoorTransitionRunner
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
    /// <summary>Draws each update's random number.</summary>
    private readonly SharedGameState _system;
    /// <summary>Whether time is frozen, which holds the room effect's overlay still.</summary>
    private readonly GameplayTimeFreeze _freeze;

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
        _freeze = dependencies.Freeze;
    }

    /// <summary>
    /// Samus's passage through the door, which the transition starts and finishes and the runtime hands to everything
    /// that holds still while she passes.
    /// </summary>
    internal DoorPassage Passage { get; } = new();

    /// <summary>Runs one update of the door's wait for its sounds to finish: enemies and drawing, without Samus's movement.</summary>
    public void RunDoorSoundWaitFrame(ControllerButton controllerInput, RoomScope room, SamusState samus)
    {
        RunDoorWaitOpening(controllerInput, room, samus);
        DrawDoorTransitionActors(enemyProjectiles: DoorTransitionEnemyProjectiles.Held, room, samus);
    }

    /// <summary>
    /// The opening of a door's sound-wait update: the frame boundary, then the liquid wave and statue descent and the random draw.
    /// The door's first update and each sound-wait update open this way while Samus is locked. On the first update it
    /// precedes the door's own step, so its room-FX sound requests are queued before the door cancels and disables
    /// sounds.
    /// </summary>
    public void RunDoorWaitOpening(ControllerButton controllerInput, RoomScope room, SamusState samus)
    {
        _frameBoundary.BeginAcceptedFrame(controllerInput);
        AdvanceEffectsAndRandom(room, samus);
        StepEffectDisplay(room);
    }

    /// <summary>
    /// Runs the room effect's overlay display for one update of the door (#3047), as the cartridge's HDMA handler runs it at
    /// the top of each update while its HDMA objects are enabled: through the room left's waits and fade out, and through
    /// the room entered's updates from its animated tiles' step on.
    /// </summary>
    internal void StepEffectDisplay(RoomScope room) => Ensure.NotNull(room).StepEffectDisplayDuringDoor(_freeze.TimeIsFrozen);

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
        _ = _enemyPhase.RunEnemyUpdatePhase(processingList: EnemyProcessingList.Unprepared, room.Camera.PreviousSamusPoint, room, samus);
        // The Samus-collision pass belongs to gameplay only.
        if (enemyProjectiles == DoorTransitionEnemyProjectiles.Run)
            RunEnemyProjectiles(room, samus);
        _actorDrawing.DrawGameplayActors(room, samus, SamusDrawOwner.Gameplay, SamusColourEffects.Hold, deathStep: null);
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
            if (room.FxOverlay.HasMovingLiquid)
                samus.LiquidPhysics.ConfigureFor(room.FxOverlay);
        }
        _system.NextRandom();
    }
}
