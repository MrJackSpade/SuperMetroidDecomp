using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Runtime.Attract;

/// <summary>
/// Drives the attract demo: sets up its room and Samus, then substitutes the scripted input each frame and
/// restores the player's controller afterwards.
/// </summary>
internal sealed class AttractDemoDriver : IAttractSamusInput
{
    /// <summary>Installs the selected Samus palette before demo room graphics are loaded.</summary>
    private readonly RuntimeArtwork _artwork;
    /// <summary>Owns the scripted input and the temporary controller substitution scope.</summary>
    private readonly AttractDemoSession _attractSession;
    /// <summary>Receives the demo Samus suit palette during scene setup.</summary>
    private readonly SnesCgram _cgram;
    /// <summary>Player controller replaced by demo input only during Samus updates.</summary>
    private readonly ControllerInputState _controller1;
    /// <summary>The session's Mode 7 transfer residue, which a demo's Samus reads.</summary>
    private readonly EnemySession _enemySession;
    /// <summary>Builds the initial HUD from the demo record's health and ammunition.</summary>
    private readonly GameplayHudSetup _hudSetup;
    /// <summary>Publishes initial HUD graphics before loading the demo room.</summary>
    private readonly AcceptedFrameBoundary _frameBoundary;
    /// <summary>Receives Samus state and the selected demo pose or shinespark launch.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Holds demo room viewport, camera scrolls, and load-station state.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Loads the recorded room and door independently using the demo camera position.</summary>
    private readonly RoomLoader _roomLoader;
    /// <summary>Seeds demo progression bits without reading or writing a player save.</summary>
    private readonly Bank80SystemState _system;

    /// <summary>Retains the runtime owners used to initialize and advance attract scenes.</summary>
    internal AttractDemoDriver(AttractDemoDriverDependencies dependencies)
    {
        _artwork = dependencies.Artwork;
        _attractSession = dependencies.AttractSession;
        _cgram = dependencies.Cgram;
        _controller1 = dependencies.Controller1;
        _enemySession = dependencies.EnemySession;
        _hudSetup = dependencies.HudSetup;
        _frameBoundary = dependencies.FrameBoundary;
        _player = dependencies.Player;
        _room = dependencies.Room;
        _roomLoader = dependencies.RoomLoader;
        _system = dependencies.System;
    }
    /// <summary>Reflects whether the current session is substituting an attract demo for gameplay.</summary>
    public bool IsAttractDemo => _attractSession.IsAttractDemo;

    /// <summary>Advances the recorded input and temporarily substitutes its held and new button edges.</summary>
    public void BeginAttractSamusInput()
    {
        if (_attractSession.Input is null) return;
        _player.IntroducedSamus.InputSnapshot.PreviousDrawNewInput = _attractSession.Input.Script.NewlyPressed;
        _attractSession.Input.StepStock(SuperMetroidGameState.PlayingDemo,
            SamusPoseMetadata.ReadMovementType(_player.IntroducedSamus));
        _attractSession.InputRestore = _controller1.UseDemoInput(
            _attractSession.Input.Script.Held, _attractSession.Input.Script.NewlyPressed);
    }

    /// <summary>Disposes the demo input scope so the player's controller is restored after Samus updates.</summary>
    internal void RestoreAttractPlayerInput()
    {
        _attractSession.InputRestore?.Dispose();
        _attractSession.InputRestore = null;
    }

    /// <summary>
    /// Loads the bank-$82/$91 demo scene without consulting or writing a player save.
    /// Uses the ordinary room, PLM, enemy, FX, and graphics loaders with demo progression.
    /// </summary>
    internal void InitializeAttractDemo(AttractDemoScene scene)
    {
        Ensure.NotNull(scene);
        byte[] itemBits = new byte[Bank80SystemState.ItemBitByteCount];
        Array.Fill(itemBits, byte.MaxValue);
        _system.LoadCollectedItemBytes(itemBits);
        _system.LoadRoomChozoBytes(itemBits);
        _system.LoadOpenedDoorBytes(new byte[Bank80SystemState.DoorBitByteCount]);
        _system.LoadEventBytes(new byte[Bank80SystemState.EventByteCount]);
        _system.LoadBossBytes(new byte[Bank80SystemState.AreaCount]);
        byte[] mapStations = new byte[Bank80SystemState.MapStationByteCount];
        mapStations.AsSpan(0, Bank80SystemState.AreaCount).Fill(byte.MaxValue);
        _system.LoadMapStationBytes(mapStations);
        byte[] usedStations = new byte[Bank80SystemState.UsedSaveStationByteCount];
        Array.Fill(usedStations, byte.MaxValue);
        _system.LoadUsedSaveStationBytes(usedStations);
        _system.LoadExploredMapBytes(new byte[Bank80SystemState.ExploredMapAreaCount * Bank80SystemState.ExploredMapBytesPerArea]);

        _player.Samus = new SamusState
        {
            Mode7QueueResidue = _enemySession.Mode7QueueResidue,
            Position = FixedPosition<RoomPixels>.FromWhole(scene.SamusPosition),
            EquippedItems = scene.Items, CollectedItems = scene.Items,
            EquippedBeams = scene.EquippedBeams, CollectedBeams = scene.CollectedBeams,
            Health = scene.Health, MaxHealth = scene.Health,
            Missiles = scene.Missiles, MaxMissiles = scene.Missiles,
            SuperMissiles = scene.SuperMissiles, MaxSuperMissiles = scene.SuperMissiles,
            PowerBombs = scene.PowerBombs, MaxPowerBombs = scene.PowerBombs,
        };
        _artwork.BindSamusPalettePresentation();
        ApplyAttractSamusSetup(scene.SamusSetupPointer);
        _player.Samus.PreviousHealthForHurtCheck = _player.Samus.Health;
        _hudSetup.InitializeHud(new HudSnapshot(_player.Samus.Health, _player.Samus.MaxHealth,
            _player.Samus.Missiles, _player.Samus.MaxMissiles, _player.Samus.SuperMissiles, _player.Samus.MaxSuperMissiles,
            _player.Samus.PowerBombs, _player.Samus.MaxPowerBombs, _player.Samus.EquippedItems, 0, 0, 0));
        _frameBoundary.BeginAcceptedFrame(0);
        CartridgeDoorHeader door = scene.Door;
        // The demo record independently supplies room and door. Do not replace room
        // selection with the door destination or derive placement from a load station.
        var room = _roomLoader.LoadCartridgeRoomHeader(scene.Room);
        _roomLoader.LoadCartridgeRoom(door, room, scene.CameraPosition,
            RoomViewportLoadMode.DisplayInitialViewport);
        // The scene's room setup acts on the demo room the load just built.
        ApplyAttractRoomSetup(scene.RoomSetupPointer, _room.LoadedScope.Enemies);
        _player.Samus.Presentation.LoadSuitPalette(_player.Samus, _cgram);
        SamusPoseMetadata.RefreshCollisionRadii(_player.Samus);
        SamusPresentation.InitializeAnimation(_player.Samus);
        _player.Samus.Presentation.PrimeGraphics(_player.Samus);
        _player.Samus.LiquidPhysics.RoomIdentity = room.Identity;
        _player.GroundedSamusMovementEnabled = true;
        _attractSession.Input = new AttractDemoInput(scene);
    }

    /// <summary>Applies the recorded entry pose, low-health adjustment, or demo shinespark launch.</summary>
    private void ApplyAttractSamusSetup(AttractDemoSamusSetup pointer)
    {
        var samus = _player.Samus ?? throw new InvalidOperationException("Demo setup requires Samus.");
        switch (pointer)
        {
            case AttractDemoSamusSetup.LandingSite:
                SamusPoseMetadata.ApplyForwardFacingPoseSetup(samus);
                break;
            case AttractDemoSamusSetup.StandingRight:
                samus.Pose = SamusPoseId.FacingRightNormalPose;
                break;
            case AttractDemoSamusSetup.LowHealthLeft:
                samus.Health = AttractDemoRomData.SetupValues.LowHealth;
                goto case AttractDemoSamusSetup.StandingLeft;
            case AttractDemoSamusSetup.StandingLeft:
                samus.Pose = SamusPoseId.FacingLeftNormalPose;
                break;
            case AttractDemoSamusSetup.MorphLeft:
                samus.Pose = SamusPoseId.MorphBallGroundLeftPose;
                break;
            case AttractDemoSamusSetup.FallingLeft:
                samus.Pose = SamusPoseId.FallingLeftPose;
                break;
            case AttractDemoSamusSetup.DiagonalShinespark:
                samus.Shinespark.BeginDemoLaunch(samus, SamusPoseId.ShinesparkDiagonalRightPose);
                break;
            case AttractDemoSamusSetup.HorizontalShinespark:
                samus.Shinespark.BeginDemoLaunch(samus, SamusPoseId.ShinesparkHorizontalLeftPose);
                break;
            default:
                throw new InvalidOperationException($"Undefined AttractDemoSamusSetup {pointer}.");
        }
    }

    /// <summary>Applies scene-specific scroll, sky-owner, Kraid timer, or defeated-boss setup.</summary>
    private void ApplyAttractRoomSetup(AttractDemoRoomSetup pointer, RoomEnemySystem enemies)
    {
        switch (pointer)
        {
            case AttractDemoRoomSetup.NoOp:
                break;
            case AttractDemoRoomSetup.ChargeBeamScroll:
                _room.LoadedCamera.Scrolls.SetStorage(AttractDemoRomData.SetupValues.ChargeBeamScrollIndex, RoomScrollState.RedBoundary);
                break;
            case AttractDemoRoomSetup.LandingSiteSky:
                // The shared land-sky loader has already selected the same vertical
                // BG2 page layout. Require that owner rather than writing a dummy mirror.
                if (_room.ScrollingSky is null)
                    throw new InvalidDataException("Landing Site demo requires the scrolling-sky BG2 owner.");
                break;
            case AttractDemoRoomSetup.KraidTimer:
                enemies.AttractDemoHooks.SetKraidFunctionTimer(AttractDemoRomData.SetupValues.KraidFunctionTimer);
                break;
            case AttractDemoRoomSetup.DefeatedKraid:
                _system.SetBossBits(AreaId.Brinstar, BossBits.AreaBoss);
                break;
            default:
                throw new InvalidOperationException($"Undefined AttractDemoRoomSetup {pointer}.");
        }
    }
}
