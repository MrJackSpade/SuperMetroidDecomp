using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// Door transitions: the door setup programs (Wrecked Ship treadmill, Maridia elevatube, scroll writes) and the
/// door sound-wait frames with their actor drawing.
/// </summary>
internal sealed class DoorTransitionRunner
{
    /// <summary>Stores actor drawing.</summary>
    private readonly GameplayActorDrawing _actorDrawing;
    /// <summary>Stores background scroll.</summary>
    private readonly BackgroundScrollState _backgroundScroll;
    /// <summary>Stores bomb projectiles.</summary>
    private readonly SamusBombProjectileSystem _bombProjectiles;
    /// <summary>Stores ceres elevator shaft.</summary>
    private readonly CeresElevatorShaftRoomMainState _ceresElevatorShaft;
    /// <summary>Stores display.</summary>
    private readonly GameplayDisplayState _display;
    /// <summary>Stores enemies.</summary>
    private readonly RoomEnemySystem _enemies;
    /// <summary>Stores enemy phase.</summary>
    private readonly EnemyFramePhase _enemyPhase;
    /// <summary>Stores hdma phase.</summary>
    private readonly HdmaObjectPhase _hdmaPhase;
    /// <summary>Stores maridia elevatube.</summary>
    private readonly MaridiaElevatubeRoomMainState _maridiaElevatube;
    /// <summary>Stores nmi.</summary>
    private readonly NmiClock _nmi;
    /// <summary>Stores nmi handler.</summary>
    private readonly NmiHandler _nmiHandler;
    /// <summary>Stores oam.</summary>
    private readonly OamBuffer _oam;
    /// <summary>Stores player.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Stores plms.</summary>
    private readonly RoomPlmSystem _plms;
    /// <summary>Stores room.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Stores room layer3 fx.</summary>
    private readonly RoomLayer3FxState _roomLayer3Fx;
    /// <summary>Stores system.</summary>
    private readonly Bank80SystemState _system;
    /// <summary>Stores wrecked ship treadmill.</summary>
    private readonly WreckedShipTreadmillAnimatedTilesState _wreckedShipTreadmill;
    /// <summary>Stores address space.</summary>
    private readonly ISnesAddressSpace _addressSpace;

    /// <summary>Initializes a new door transition runner instance.</summary>
    internal DoorTransitionRunner(DoorTransitionRunnerDependencies dependencies)
    {
        _actorDrawing = dependencies.ActorDrawing;
        _backgroundScroll = dependencies.BackgroundScroll;
        _bombProjectiles = dependencies.BombProjectiles;
        _ceresElevatorShaft = dependencies.CeresElevatorShaft;
        _display = dependencies.Display;
        _enemies = dependencies.Enemies;
        _enemyPhase = dependencies.EnemyPhase;
        _hdmaPhase = dependencies.HdmaPhase;
        _maridiaElevatube = dependencies.MaridiaElevatube;
        _nmi = dependencies.Nmi;
        _nmiHandler = dependencies.NmiHandler;
        _oam = dependencies.Oam;
        _player = dependencies.Player;
        _plms = dependencies.Plms;
        _room = dependencies.Room;
        _roomLayer3Fx = dependencies.RoomLayer3Fx;
        _system = dependencies.System;
        _wreckedShipTreadmill = dependencies.WreckedShipTreadmill;
        _addressSpace = dependencies.AddressSpace;
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
    internal void RunDoorSetupCode(CartridgeDoorHeader door)
    {
        if (_room.Camera is null)
            throw new InvalidOperationException("Door setup requires the destination scroll grid.");

        DoorSetupCodeInterpreter.ApplyScrollWrites(
            door.SetupCodePointer,
            door.Pointer,
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
                _display.DisplayedSamusMode7Transform = null;
                _ceresElevatorShaft.Reset(active: false, _room.RoomMainScratch);
                return;

            case DoorSetupCode.DoorASM_StartWreckedShipTreadmillWestEntrance:
                StartWreckedShipTreadmill(WreckedShipTreadmillDirection.Rightwards);
                return;

            case DoorSetupCode.DoorASM_StartWreckedShipTreadmillEastEntrance:
                StartWreckedShipTreadmill(WreckedShipTreadmillDirection.Leftwards);
                return;

            case DoorSetupCode.DoorASM_SetupElevatubeFromSouth:
                SetUpMaridiaElevatube(fromSouth: true);
                return;

            case DoorSetupCode.DoorASM_SetupElevatubeFromNorth:
                SetUpMaridiaElevatube(fromSouth: false);
                return;

            case DoorSetupCode.DoorASM_ResetElevatubeOnNorthExit:
            case DoorSetupCode.DoorASM_ResetElevatubeOnSouthExit:
                _maridiaElevatube.ResetOnExit(_player.Samus ?? throw new InvalidOperationException(
                    "Maridia elevatube exit setup requires an active Samus state."));
                return;

            default:
                if (DoorScrollPrograms.Contains(door.SetupCodePointer))
                    return;
                throw new InvalidOperationException(
                    $"Undefined {nameof(DoorSetupCode)} {(int)door.SetupCodePointer:X4} in door $83:{door.Pointer:X4}.");
        }
    }

    /// <summary>Starts wrecked ship treadmill.</summary>
    private void StartWreckedShipTreadmill(WreckedShipTreadmillDirection direction)
    {
        if (_room.LevelData is null || _room.BackgroundStreamer is null || _room.ActiveRoom is null)
        {
            throw new InvalidOperationException(
                "Wrecked Ship treadmill door setup requires an active destination room.");
        }

        _wreckedShipTreadmill.Start(_addressSpace, direction);
        // Both native spawn helpers return carry set when their fixed arrays are full, and
        // the door routine deliberately ignores that result. The dedicated animated owner
        // above cannot exhaust; preserve the PLM allocator's native false result here.
        _ = _plms.WreckedShipTreadmill.TrySpawnWreckedShipEntranceTreadmill(
            _room.LevelData,
            _room.BackgroundStreamer,
            direction,
            _system.HasAnyBossBits(_room.ActiveRoom.AreaIndex, BossBits.AreaBoss));
    }

    /// <summary>Sets up maridia elevatube.</summary>
    private void SetUpMaridiaElevatube(bool fromSouth)
    {
        if (_room.LevelData is null)
            throw new InvalidOperationException("Maridia elevatube setup requires active level data.");
        SamusState samus = _player.Samus ?? throw new InvalidOperationException(
            "Maridia elevatube setup requires an active Samus state.");

        if (fromSouth)
            _maridiaElevatube.SetUpFromSouth(samus);
        else
            _maridiaElevatube.SetUpFromNorth(samus);

        // The door routine ignores SpawnHardcodedPLM's carry result when all forty IDs are
        // occupied. Retaining the bool-returning allocator preserves that cartridge edge.
        _ = _plms.WreckedShipTreadmill.TrySpawnMaridiaElevatube(_room.LevelData);
    }
    /// <summary>Enemy and draw owners of the door sound-drain coroutine, without Samus movement.</summary>
    public void RunDoorSoundWaitFrame(ushort controllerInput)
    {
        RunDoorSoundWaitPrologue(controllerInput);
        DrawDoorTransitionActors(runEnemyProjectiles: false);
    }

    /// <summary>
    /// The NMI and MainGameLoop HDMA/RNG prologue of a door dispatch. Both state-$09 entry
    /// and each $E29E sound-drain dispatch return to MainGameLoop, whose prologue still
    /// runs while Samus is locked. On the entry frame it precedes the state handler, so
    /// its room-FX sound requests are queued before the door's cancels and DisableSounds.
    /// </summary>
    public void RunDoorSoundWaitPrologue(ushort controllerInput)
    {
        _plms.Sounds.BindPowerBombAudio(_bombProjectiles.PowerBombExplosion);
        _nmiHandler.RunNmi(controllerInput, mainLoopRequestedNmi: true);
        AdvanceNonGameplayMainLoopRandom(hdmaObjectsEnabled: true);
    }

    /// <summary>Native enemy/instruction and draw pass shared by source-door waits and fading.</summary>
    /// <param name="runEnemyProjectiles">
    /// True for the destination fade-in ($82:E743); the source waits ($E29E/$E2DB) run
    /// enemies and drawing without Enemy_Projectile_Handler.
    /// </param>
    internal void DrawDoorTransitionActors(bool runEnemyProjectiles)
    {
        _oam.BeginFrame();
        _actorDrawing.LastSamusBodyDrawn = false;
        _actorDrawing.LastShinesparkCrashDrawingHandlerActive = false;
        _actorDrawing.LastGrappleDrawingHandlerActive = false;
        _actorDrawing.LastGrappleBeamSpecificDrawingPath = false;
        _actorDrawing.LastGrappleFlareDrawn = false;
        // The draw handler can append spin-stop/charge sounds. Do not publish the
        // prior gameplay frame's liquid, movement, projectile or PLM requests again.
        _player.Samus?.LiquidPhysics.BeginFrameSoundRequests(_bombProjectiles.PowerBombExplosion);
        _enemyPhase.RunEnemyMainPhase(processingListPrepared: false, _room.Camera?.PreviousSamusPoint);
        // The Samus-collision pass belongs to gameplay only.
        if (runEnemyProjectiles)
            RunEnemyProjectileHandler();
        _actorDrawing.DrawGameplayActors(deathOwnsSamus: false, advanceSamusPalette: false);
        // EnsureSamusDrawnEachFrame bypasses ordinary hurt flicker after the shared
        // draw. Elevator-owned drawing retains its separate alternating cadence.
        if (_enemies.ElevatorBehavior.ElevatorStatus == ElevatorActorStatus.Inactive && _player.Samus is not null && _room.Camera is not null)
            _actorDrawing.LastSamusBodyDrawn = _player.Samus.Presentation.Draw(_addressSpace, _oam,
                _room.Camera.PixelPosition, mode7Transform: _display.ActiveSamusMode7Transform);
        _oam.FinalizeFrame();
    }
    /// <summary>
    /// <c>Enemy_Projectile_Handler</c> ($86:8104): runs every projectile unless
    /// <c>EnemyProjectile_Enable</c> is clear (time freeze or X-ray suspension).
    /// </summary>
    /// <summary>
    /// <c>Enemy_Projectile_Handler</c> ($86:8104). State eight calls it unconditionally and it
    /// checks only the enable bit X-ray clears, so its instructions keep running while time
    /// is frozen for a reserve refill.
    /// </summary>
    internal void RunEnemyProjectileHandler()
    {
        if (_player.Samus?.Xray.AreEnemyProjectilesSuspended == true || _room.LevelData is null || _room.Camera is null)
            return;
        _enemies.StepEnemyProjectileInstructions(
            _room.LevelData, _player.Samus, _room.Camera.PixelPosition,             _nmi.NmiFrameCounter8, _bombProjectiles, _backgroundScroll, _nmi.NmiFrameCounter);
    }

    /// <summary>Shared HDMA/RNG prologue of an outer dispatch without a gameplay frame.</summary>
    internal void AdvanceNonGameplayMainLoopRandom(bool hdmaObjectsEnabled)
    {
        if (hdmaObjectsEnabled)
        {
            _hdmaPhase.AdvanceHdmaObjectPreInstructions();
            if (_player.Samus is not null && _roomLayer3Fx.MovesLiquidInHdmaPass)
                _roomLayer3Fx.ApplyToSamusLiquidPhysics(_player.Samus.LiquidPhysics);
        }
        _system.NextRandom();
    }
}
