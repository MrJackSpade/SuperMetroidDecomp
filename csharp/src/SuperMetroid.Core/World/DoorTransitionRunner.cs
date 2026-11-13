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
/// Door transitions: the door setup programs (Wrecked Ship treadmill, Maridia elevatube, scroll writes) and the
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
    /// <summary>Provides boss bits and advances main-loop random state.</summary>
    private readonly SharedGameState _system;

    /// <summary>Captures the runtime services used by the door's setup programs and its wait updates.</summary>
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
    /// Runs the setup routine named by the active door header after room-object creation.
    /// </summary>
    /// <remarks>
    /// The room load runs the door's routine between room-object
    /// construction and the first visible destination viewport. These routines are shared
    /// programs, not room-name patches: every door that names a translated
    /// routine receives the same writes. An unknown routine throws so missing camera
    /// boundary behavior can never masquerade as a successful room load.
    /// </remarks>
    internal void RunDoorSetupCode(DoorDefinition door, RoomObjectSystem roomObjects, WreckedShipTreadmillAnimatedTilesState wreckedShipTreadmill, CeresElevatorShaftRoomMainState ceresElevatorShaft, MaridiaElevatubeRoomMainState maridiaElevatube)
    {
        if (_room.Camera is null)
            throw new InvalidOperationException("Door setup requires the destination scroll grid.");

        DoorSetupCodeInterpreter.ApplyScrollWrites(
            door.SetupCode,
            door.Name,
            _room.Camera.Scrolls);

        switch (door.SetupCode)
        {
            case DoorSetupCode.None:
                return;

            case DoorSetupCode.DoorASM_ToCeresElevatorShaft:
                // Ceres's door setup owns the rotating-room matrix and centre. Those
                // values must exist before room graphics/actors initialize, so the shared
                // loader installs them at construction time and this dispatch
                // point merely verifies that the recognized routine was not discarded.
                if (!door.EntersCeresElevatorShaft || _display.ActiveTurningFloor is null)
                {
                    throw new InvalidDataException(
                        "Ceres elevator door setup reached dispatch without its turning-floor transform.");
                }
                return;

            case DoorSetupCode.DoorASM_FromCeresElevatorShaft:
                // The paired exit program of the Ceres elevator shaft's entry: it returns
                // the screen from the turning floor to the ordinary room display, removing
                // both the active and the shown transforms. Do this explicitly here rather
                // than relying on the destination loader's defensive state reset: this is
                // the door program that owns the transition back to the ordinary display.
                _display.ActiveTurningFloor = null;
                _display.PublishDoorElevatorFloor(null);
                ceresElevatorShaft.Deactivate();
                return;

            case DoorSetupCode.DoorASM_StartWreckedShipTreadmillWestEntrance:
                StartWreckedShipTreadmill(WreckedShipTreadmillDirection.Rightwards, roomObjects, wreckedShipTreadmill);
                return;

            case DoorSetupCode.DoorASM_StartWreckedShipTreadmillEastEntrance:
                StartWreckedShipTreadmill(WreckedShipTreadmillDirection.Leftwards, roomObjects, wreckedShipTreadmill);
                return;

            case DoorSetupCode.DoorASM_SetupElevatubeFromSouth:
                SetUpMaridiaElevatube(entry: ElevatubeEntry.FromSouth, roomObjects, maridiaElevatube);
                return;

            case DoorSetupCode.DoorASM_SetupElevatubeFromNorth:
                SetUpMaridiaElevatube(entry: ElevatubeEntry.FromNorth, roomObjects, maridiaElevatube);
                return;

            case DoorSetupCode.DoorASM_ResetElevatubeOnNorthExit:
            case DoorSetupCode.DoorASM_ResetElevatubeOnSouthExit:
                maridiaElevatube.ResetOnExit(_player.Samus ?? throw new InvalidOperationException(
                    "Maridia elevatube exit setup requires an active Samus state."));
                return;

            case DoorSetupCode.DoorCode_Scroll6_Green:
            case DoorSetupCode.DoorASM_Scroll_0_Blue:
            case DoorSetupCode.DoorASM_Scroll_13_Blue:
            case DoorSetupCode.DoorASM_Scroll_4_Red_8_Green:
            case DoorSetupCode.DoorASM_Scroll_8_9_A_B_Red:
            case DoorSetupCode.DoorASM_Scroll_2_3_4_5_B_C_D_11_Red:
            case DoorSetupCode.DoorASM_Scroll_1_4_Green:
            case DoorSetupCode.DoorASM_Scroll_2_Blue:
            case DoorSetupCode.DoorASM_Scroll_17_Blue:
            case DoorSetupCode.DoorASM_Scroll_4_Blue:
            case DoorSetupCode.DoorASM_Scroll_6_Green_duplicate:
            case DoorSetupCode.DoorASM_Scroll_3_Green:
            case DoorSetupCode.DoorASM_Scroll_18_1C_Green:
            case DoorSetupCode.DoorASM_Scroll_5_6_Blue:
            case DoorSetupCode.DoorASM_Scroll_1D_Blue:
            case DoorSetupCode.DoorASM_Scroll_2_3_Green:
            case DoorSetupCode.DoorASM_Scroll_0_Red_1_Green:
            case DoorSetupCode.DoorASM_Scroll_B_Green:
            case DoorSetupCode.DoorASM_Scroll_Scroll_1C_Red_1D_Blue:
            case DoorSetupCode.DoorASM_Scroll_4_Red:
            case DoorSetupCode.DoorASM_Scroll_20_24_25_Green:
            case DoorSetupCode.DoorASM_Scroll_2_Blue_duplicate:
            case DoorSetupCode.DoorASM_Scroll_0_Green:
            case DoorSetupCode.DoorASM_Scroll_6_7_Green:
            case DoorSetupCode.DoorASM_Scroll_1_Blue_2_Red:
            case DoorSetupCode.DoorASM_Scroll_1_Blue_3_Red:
            case DoorSetupCode.DoorASM_Scroll_0_Red_4_Blue:
            case DoorSetupCode.DoorASM_Scroll_2_3_Blue:
            case DoorSetupCode.DoorASM_Scroll_0_1_Green:
            case DoorSetupCode.DoorASM_Scroll_1_Green:
            case DoorSetupCode.DoorASM_Scroll_F_12_Green:
            case DoorSetupCode.DoorASM_Scroll_6_Green_duplicate_again:
            case DoorSetupCode.DoorASM_Scroll_0_Green_1_Blue:
            case DoorSetupCode.DoorASM_Scroll_2_Green:
            case DoorSetupCode.DoorASM_Scroll_3_4_Red_6_7_8_Blue:
            case DoorSetupCode.DoorASM_Scroll_1_2_3_Blue_4_Green_6_Red:
            case DoorSetupCode.DoorASM_Scroll_0_1_Blue:
            case DoorSetupCode.DoorASM_Scroll_0_Blue_1_Red:
            case DoorSetupCode.DoorASM_Scroll_A_Green:
            case DoorSetupCode.DoorASM_Scroll_0_2_Green:
            case DoorSetupCode.DoorASM_Scroll_6_7_Blue_8_Red:
            case DoorSetupCode.DoorASM_Scroll_2_Red_3_Blue:
            case DoorSetupCode.DoorASM_Scroll_7_Green:
            case DoorSetupCode.DoorASM_Scroll_1_Red_2_Blue:
            case DoorSetupCode.DoorASM_Scroll_0_Blue_3_Red:
            case DoorSetupCode.DoorASM_Scroll_1_Blue_4_Red:
            case DoorSetupCode.DoorASM_Scroll_0_Blue_1_2_3_Red:
            case DoorSetupCode.DoorASM_Scroll_0_Green_duplicate:
            case DoorSetupCode.DoorASM_Scroll_0_1_Blue_4_Red:
            case DoorSetupCode.DoorASM_Scroll_0_Blue_3_Red_duplicate:
            case DoorSetupCode.DoorASM_Scroll_0_Blue_duplicate:
            case DoorSetupCode.DoorASM_Scroll_0_Blue_1_Red_duplicate:
            case DoorSetupCode.DoorASM_Scroll_18_Blue:
            case DoorSetupCode.DoorASM_Scroll_2_Blue_3_Red:
            case DoorSetupCode.DoorASM_Scroll_E_Red:
            case DoorSetupCode.DoorASM_Scroll_1_Blue:
            case DoorSetupCode.DoorASM_Scroll_0_Green_duplicate_again:
            case DoorSetupCode.DoorASM_Scroll_3_Red_4_Blue:
            case DoorSetupCode.DoorASM_Scroll_29_Blue:
            case DoorSetupCode.DoorASM_Scroll_28_2E_Green:
            case DoorSetupCode.DoorASM_Scroll_6_7_8_9_A_B_Red:
            case DoorSetupCode.DoorASM_Scroll_A_Red_B_Blue:
            case DoorSetupCode.DoorASM_Scroll_0_Red_4_Blue_duplicate:
            case DoorSetupCode.DoorASM_Scroll_0_Red_1_Blue:
            case DoorSetupCode.DoorASM_Scroll_9_Red_A_Blue:
            case DoorSetupCode.DoorASM_Scroll_0_2_Red_1_Blue:
            case DoorSetupCode.DoorASM_Scroll_1_Blue_duplicate:
            case DoorSetupCode.DoorASM_Scroll_6_Blue:
            case DoorSetupCode.DoorASM_Scroll_4_Red_duplicate:
            case DoorSetupCode.DoorASM_Scroll_4_7_Red:
            case DoorSetupCode.DoorASM_Scroll_1_Blue_2_Red_duplicate:
            case DoorSetupCode.DoorASM_Scroll_0_2_Green_duplicate:
            case DoorSetupCode.DoorASM_Scroll_0_1_Green_duplicate:
            case DoorSetupCode.DoorASM_Scroll_18_Blue_19_Red:
            default:
                if (DoorScrollPrograms.Contains(door.SetupCode))
                    return;
                throw new InvalidOperationException(
                    $"Undefined {nameof(DoorSetupCode)} {door.SetupCode} in door {door.Name}.");
        }
    }

    /// <summary>Starts animated treadmill tiles and attempts the matching entrance room-object spawn.</summary>
    private void StartWreckedShipTreadmill(WreckedShipTreadmillDirection direction, RoomObjectSystem roomObjects, WreckedShipTreadmillAnimatedTilesState wreckedShipTreadmill)
    {
        if (_room.LevelData is null || _room.ActiveRoom is null)
        {
            throw new InvalidOperationException(
                "Wrecked Ship treadmill door setup requires an active destination room.");
        }

        wreckedShipTreadmill.Start(direction);
        // Both spawn helpers report when they have no free slot left, and
        // the door routine deliberately ignores that result. The dedicated animated owner
        // above cannot exhaust; preserve the room-object allocator's false result here.
        _ = roomObjects.WreckedShipTreadmill.TrySpawnWreckedShipEntranceTreadmill(
            _room.LevelData,
            direction,
            _system.HasAnyBossBits(_room.ActiveRoom.AreaIndex, BossBits.AreaBoss));
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

        // The door routine ignores whether the spawn found a free slot when all forty are
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
    /// suspension X-ray applies, so their instructions keep running while time is frozen for
    /// a reserve refill.
    /// </summary>
    internal void RunEnemyProjectileHandler(SamusBombProjectileSystem bombProjectiles, RoomScope room)
    {
        if (_player.Samus?.Xray.AreEnemyProjectilesSuspended == true || _room.LevelData is null || _room.Camera is null)
            return;
        room.Enemies.StepEnemyProjectileInstructions(
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
