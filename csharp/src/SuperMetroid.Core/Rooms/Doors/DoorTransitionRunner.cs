using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Maridia;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// Door transitions: the door setup programs (Wrecked Ship treadmill, Maridia elevatube, scroll writes) and the
/// door sound-wait frames with their actor drawing.
/// </summary>
internal sealed class DoorTransitionRunner
{
    /// <summary>Draws Samus and room actors during door waits and fades.</summary>
    private readonly GameplayActorDrawing _actorDrawing;
    /// <summary>Provides scroll state to the enemy projectile pass.</summary>
    private readonly BackgroundScrollState _backgroundScroll;
    /// <summary>Holds active and NMI-published display transforms across transitions.</summary>
    private readonly GameplayDisplayState _display;
    /// <summary>Runs the enemy main phase while Samus gameplay is suspended.</summary>
    private readonly EnemyFramePhase _enemyPhase;
    /// <summary>Advances HDMA pre-instructions during the main-loop door prologue.</summary>
    private readonly HdmaObjectPhase _hdmaPhase;
    /// <summary>Provides native frame counters to enemy projectile processing.</summary>
    private readonly FrameClock _clock;
    /// <summary>Runs NMI on each sound-wait dispatch before the door state handler.</summary>
    private readonly AcceptedFrameBoundary _frameBoundary;
    /// <summary>Sprite frame begun and finalized around each drawn frame.</summary>
    private readonly SpriteFrameBuffer _sprites;
    /// <summary>Provides the locked Samus state and its presentation during transitions.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Holds destination room data, camera, and background streamer.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Provides boss bits and advances main-loop random state.</summary>
    private readonly Bank80SystemState _system;

    /// <summary>Captures the runtime services used by native-order door setup and wait dispatch.</summary>
    internal DoorTransitionRunner(DoorTransitionRunnerDependencies dependencies)
    {
        _actorDrawing = dependencies.ActorDrawing;
        _backgroundScroll = dependencies.BackgroundScroll;
        _display = dependencies.Display;
        _enemyPhase = dependencies.EnemyPhase;
        _hdmaPhase = dependencies.HdmaPhase;
        _clock = dependencies.Clock;
        _frameBoundary = dependencies.FrameBoundary;
        _sprites = dependencies.Sprites;
        _player = dependencies.Player;
        _room = dependencies.Room;
        _system = dependencies.System;
    }
    /// <summary>
    /// Runs the setup routine named by the active bank-$83 door header after PLM creation.
    /// </summary>
    /// <remarks>
    /// Native <c>LoadMoreThings</c> calls the door's bank-$8F routine between room-object
    /// construction and the first visible destination viewport. These routines are shared
    /// cartridge programs, not room-name patches: every door that points at a translated
    /// address receives the same writes. Unknown nonzero pointers throw so missing camera
    /// boundary behavior can never masquerade as a successful room load.
    /// </remarks>
    internal void RunDoorSetupCode(CartridgeDoorHeader door, RoomPlmSystem plms, WreckedShipTreadmillAnimatedTilesState wreckedShipTreadmill, CeresElevatorShaftRoomMainState ceresElevatorShaft, MaridiaElevatubeRoomMainState maridiaElevatube)
    {
        if (_room.Camera is null)
            throw new InvalidOperationException("Door setup requires the destination scroll grid.");

        DoorSetupCodeInterpreter.ApplyScrollWrites(
            door.SetupCodePointer,
            door.Name,
            _room.Camera.Scrolls);

        switch (door.SetupCodePointer)
        {
            case DoorSetupCode.None:
                return;

            case DoorSetupCode.DoorASM_ToCeresElevatorShaft:
                // `$8F:E4E0` owns the Ceres Mode-7 matrix and center registers. Those
                // values must exist before room graphics/actors initialize, so the shared
                // loader installs them at construction time and this native-order dispatch
                // point merely verifies that the recognized routine was not discarded.
                if (!door.UsesCeresElevatorMode7 || _display.ActiveSamusMode7Transform is null)
                {
                    throw new InvalidDataException(
                        "Ceres elevator door setup reached dispatch without its Mode-7 transform.");
                }
                return;

            case DoorSetupCode.DoorASM_FromCeresElevatorShaft:
                // `$8F:E513` is the paired exit routine for `$8F:E4E0`. Native writes
                // fake BGMODE `$09` (Mode 1 with BG3 priority) and clears `$0783`, the
                // Mode-7 IRQ/transfer flag. The software renderer represents those two
                // words by selecting its ordinary room compositor and removing both the
                // main-loop and NMI-published transforms. Do this explicitly here rather
                // than relying on the destination loader's defensive state reset: this is
                // the cartridge routine that owns the transition back to normal rendering.
                _display.ActiveSamusMode7Transform = null;
                _display.PublishDoorMode7(null);
                ceresElevatorShaft.Deactivate();
                return;

            case DoorSetupCode.DoorASM_StartWreckedShipTreadmillWestEntrance:
                StartWreckedShipTreadmill(WreckedShipTreadmillDirection.Rightwards, plms, wreckedShipTreadmill);
                return;

            case DoorSetupCode.DoorASM_StartWreckedShipTreadmillEastEntrance:
                StartWreckedShipTreadmill(WreckedShipTreadmillDirection.Leftwards, plms, wreckedShipTreadmill);
                return;

            case DoorSetupCode.DoorASM_SetupElevatubeFromSouth:
                SetUpMaridiaElevatube(fromSouth: true, plms, maridiaElevatube);
                return;

            case DoorSetupCode.DoorASM_SetupElevatubeFromNorth:
                SetUpMaridiaElevatube(fromSouth: false, plms, maridiaElevatube);
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
                if (DoorScrollPrograms.Contains(door.SetupCodePointer))
                    return;
                throw new InvalidOperationException(
                    $"Undefined {nameof(DoorSetupCode)} {(int)door.SetupCodePointer:X4} in door {door.Name}.");
        }
    }

    /// <summary>Starts animated treadmill tiles and attempts the matching entrance PLM spawn.</summary>
    private void StartWreckedShipTreadmill(WreckedShipTreadmillDirection direction, RoomPlmSystem plms, WreckedShipTreadmillAnimatedTilesState wreckedShipTreadmill)
    {
        if (_room.LevelData is null || _room.ActiveRoom is null)
        {
            throw new InvalidOperationException(
                "Wrecked Ship treadmill door setup requires an active destination room.");
        }

        wreckedShipTreadmill.Start(direction);
        // Both native spawn helpers return carry set when their fixed arrays are full, and
        // the door routine deliberately ignores that result. The dedicated animated owner
        // above cannot exhaust; preserve the PLM allocator's native false result here.
        _ = plms.WreckedShipTreadmill.TrySpawnWreckedShipEntranceTreadmill(
            _room.LevelData,
            direction,
            _system.HasAnyBossBits(_room.ActiveRoom.AreaIndex, BossBits.AreaBoss));
    }

    /// <summary>Positions Samus for the selected elevatube entrance and attempts its room PLM spawn.</summary>
    private void SetUpMaridiaElevatube(bool fromSouth, RoomPlmSystem plms, MaridiaElevatubeRoomMainState maridiaElevatube)
    {
        if (_room.LevelData is null)
            throw new InvalidOperationException("Maridia elevatube setup requires active level data.");
        SamusState samus = _player.Samus ?? throw new InvalidOperationException(
            "Maridia elevatube setup requires an active Samus state.");

        if (fromSouth)
            maridiaElevatube.SetUpFromSouth(samus);
        else
            maridiaElevatube.SetUpFromNorth(samus);

        // The door routine ignores SpawnHardcodedPLM's carry result when all forty IDs are
        // occupied. Retaining the bool-returning allocator preserves that cartridge edge.
        _ = plms.WreckedShipTreadmill.TrySpawnMaridiaElevatube(_room.LevelData);
    }
    /// <summary>Enemy and draw owners of the door sound-drain coroutine, without Samus movement.</summary>
    public void RunDoorSoundWaitFrame(SnesButton controllerInput, RoomScope room)
    {
        RunDoorSoundWaitPrologue(controllerInput, room.Plms, room.BombProjectiles, room);
        DrawDoorTransitionActors(runEnemyProjectiles: false, room);
    }

    /// <summary>
    /// The NMI and MainGameLoop HDMA/RNG prologue of a door dispatch. Both state-$09 entry
    /// and each $E29E sound-drain dispatch return to MainGameLoop, whose prologue still
    /// runs while Samus is locked. On the entry frame it precedes the state handler, so
    /// its room-FX sound requests are queued before the door's cancels and DisableSounds.
    /// </summary>
    public void RunDoorSoundWaitPrologue(SnesButton controllerInput, RoomPlmSystem plms, SamusBombProjectileSystem bombProjectiles, RoomScope room)
    {
        plms.Sounds.BindPowerBombAudio(bombProjectiles.PowerBombExplosion);
        _frameBoundary.BeginAcceptedFrame(controllerInput);
        AdvanceNonGameplayMainLoopRandom(hdmaObjectsEnabled: true, plms, room);
    }

    /// <summary>Native enemy/instruction and draw pass shared by source-door waits and fading.</summary>
    /// <param name="runEnemyProjectiles">
    /// True for the destination fade-in ($82:E743); the source waits ($E29E/$E2DB) run
    /// enemies and drawing without Enemy_Projectile_Handler.
    /// </param>
    /// <param name="room">The loaded room.</param>
    internal void DrawDoorTransitionActors(bool runEnemyProjectiles, RoomScope room)
    {
        _sprites.BeginFrame();
        // The draw handler can append spin-stop/charge sounds. Do not publish the
        // prior gameplay frame's liquid, movement, projectile or PLM requests again.
        _player.Samus?.LiquidPhysics.BeginFrameSoundRequests(room.BombProjectiles.PowerBombExplosion);
        _enemyPhase.RunEnemyMainPhase(processingListPrepared: false, _room.Camera?.PreviousSamusPoint, room);
        // The Samus-collision pass belongs to gameplay only.
        if (runEnemyProjectiles)
            RunEnemyProjectileHandler(room.BombProjectiles, room);
        _actorDrawing.DrawGameplayActors(room, deathOwnsSamus: false, advanceSamusPalette: false);
        // EnsureSamusDrawnEachFrame bypasses ordinary hurt flicker after the shared
        // draw. Elevator-owned drawing retains its separate alternating cadence.
        if (room.Enemies.Elevator.ElevatorStatus == ElevatorActorStatus.Inactive && _player.Samus is not null && _room.Camera is not null)
            _player.Samus.Presentation.Draw(_sprites,
                _room.Camera.PixelPosition, mode7Transform: _display.ActiveSamusMode7Transform);
        _sprites.FinalizeFrame();
    }
    /// <summary>
    /// <c>Enemy_Projectile_Handler</c> ($86:8104). State eight calls it unconditionally and it
    /// checks only the enable bit X-ray clears, so its instructions keep running while time
    /// is frozen for a reserve refill.
    /// </summary>
    internal void RunEnemyProjectileHandler(SamusBombProjectileSystem bombProjectiles, RoomScope room)
    {
        if (_player.Samus?.Xray.AreEnemyProjectilesSuspended == true || _room.LevelData is null || _room.Camera is null)
            return;
        room.Enemies.StepEnemyProjectileInstructions(
            _room.LevelData, _player.Samus, _room.Camera.PixelPosition,             _clock.Counter8, bombProjectiles, _backgroundScroll, _clock.Counter);
    }

    /// <summary>Shared HDMA/RNG prologue of an outer dispatch without a gameplay frame.</summary>
    internal void AdvanceNonGameplayMainLoopRandom(bool hdmaObjectsEnabled, RoomPlmSystem plms, RoomScope room)
    {
        if (hdmaObjectsEnabled)
        {
            _hdmaPhase.AdvanceHdmaObjectPreInstructions(plms, room);
            if (_player.Samus is not null && room.Layer3Fx.MovesLiquidInHdmaPass)
                room.Layer3Fx.ApplyToSamusLiquidPhysics(_player.Samus.LiquidPhysics);
        }
        _system.NextRandom();
    }
}
