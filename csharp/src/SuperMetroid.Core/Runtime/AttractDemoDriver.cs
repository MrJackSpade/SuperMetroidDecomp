using SuperMetroid.Core.Input;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// Drives the attract demo: sets up its room and Samus, then substitutes the scripted input each frame and
/// restores the player's controller afterwards.
/// </summary>
internal sealed class AttractDemoDriver
{
    /// <summary>Stores artwork.</summary>
    private readonly RuntimeArtwork _artwork;
    /// <summary>Stores attract session.</summary>
    private readonly AttractDemoSession _attractSession;
    /// <summary>Stores cgram.</summary>
    private readonly SnesCgram _cgram;
    /// <summary>Stores controller1.</summary>
    private readonly ControllerInputState _controller1;
    /// <summary>Stores enemies.</summary>
    private readonly RoomEnemySystem _enemies;
    /// <summary>Stores hud setup.</summary>
    private readonly GameplayHudSetup _hudSetup;
    /// <summary>Stores nmi handler.</summary>
    private readonly NmiHandler _nmiHandler;
    /// <summary>Stores player.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Stores room.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Stores room loader.</summary>
    private readonly RoomLoader _roomLoader;
    /// <summary>Stores system.</summary>
    private readonly Bank80SystemState _system;
    /// <summary>Stores address space.</summary>
    private readonly ISnesAddressSpace _addressSpace;

    /// <summary>Initializes a new attract demo driver instance.</summary>
    internal AttractDemoDriver(AttractDemoDriverDependencies dependencies)
    {
        _artwork = dependencies.Artwork;
        _attractSession = dependencies.AttractSession;
        _cgram = dependencies.Cgram;
        _controller1 = dependencies.Controller1;
        _enemies = dependencies.Enemies;
        _hudSetup = dependencies.HudSetup;
        _nmiHandler = dependencies.NmiHandler;
        _player = dependencies.Player;
        _room = dependencies.Room;
        _roomLoader = dependencies.RoomLoader;
        _system = dependencies.System;
        _addressSpace = dependencies.AddressSpace;
    }
    /// <summary>Gets is attract demo.</summary>
    internal bool IsAttractDemo => _attractSession.IsAttractDemo;

    /// <summary>begin attract samus input.</summary>
    internal void BeginAttractSamusInput()
    {
        if (_attractSession.Input is null) return;
        _player.Samus!.InputSnapshot.PreviousDrawNewInput = _attractSession.Input.Script.NewlyPressed;
        _attractSession.Input.StepStock(SuperMetroidGameState.PlayingDemo,
            _player.Samus!.PoseMetadata.ReadMovementType(_addressSpace));
        _attractSession.InputRestore = _controller1.UseDemoInput(
            _attractSession.Input.Script.Held, _attractSession.Input.Script.NewlyPressed);
    }

    /// <summary>restore attract player input.</summary>
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
        _nmiHandler.RunNmi(0, true);
        CartridgeDoorHeader door = DoorDefinitions.Get(scene.DoorPointer);
        // The demo record independently supplies room and door. Do not replace room
        // selection with the door destination or derive placement from a load station.
        var room = _roomLoader.LoadCartridgeRoomHeader(scene.RoomPointer);
        _room.ActiveLoadStation = null;
        _room.CeresElevatorArrival = null;
        _roomLoader.LoadCartridgeRoom(door, room, scene.CameraPosition,
            RoomViewportLoadMode.DisplayInitialViewport);
        ApplyAttractRoomSetup(scene.RoomSetupPointer);
        _player.Samus.Presentation.LoadSuitPalette(_addressSpace, _cgram);
        _player.Samus.PoseMetadata.RefreshCollisionRadii(_addressSpace);
        _player.Samus.Presentation.InitializeAnimation(_addressSpace);
        _player.Samus.Presentation.PrimeGraphics(_addressSpace);
        _player.Samus.LiquidPhysics.RoomIdentity = room.Identity;
        _player.GroundedSamusMovementEnabled = true;
        _attractSession.Input = new AttractDemoInput(scene);
    }

    /// <summary>apply attract samus setup.</summary>
    private void ApplyAttractSamusSetup(AttractDemoSamusSetup pointer)
    {
        var samus = _player.Samus ?? throw new InvalidOperationException("Demo setup requires Samus.");
        switch (pointer)
        {
            case AttractDemoSamusSetup.LandingSite:
                samus.PoseMetadata.ApplyForwardFacingPoseSetup(_addressSpace);
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
                samus.Shinespark.BeginDemoLaunch(_addressSpace, samus, SamusPoseId.ShinesparkDiagonalRightPose);
                break;
            case AttractDemoSamusSetup.HorizontalShinespark:
                samus.Shinespark.BeginDemoLaunch(_addressSpace, samus, SamusPoseId.ShinesparkHorizontalLeftPose);
                break;
            default:
                throw new InvalidOperationException($"Undefined AttractDemoSamusSetup {pointer}.");
        }
    }

    /// <summary>apply attract room setup.</summary>
    private void ApplyAttractRoomSetup(AttractDemoRoomSetup pointer)
    {
        switch (pointer)
        {
            case AttractDemoRoomSetup.NoOp:
                break;
            case AttractDemoRoomSetup.ChargeBeamScroll:
                _room.Camera!.Scrolls.SetStorage(AttractDemoRomData.SetupValues.ChargeBeamScrollIndex, RoomScrollState.RedBoundary);
                break;
            case AttractDemoRoomSetup.LandingSiteSky:
                // The shared land-sky loader has already selected the same vertical
                // BG2 page layout. Require that owner rather than writing a dummy mirror.
                if (_room.ScrollingSky is null)
                    throw new InvalidDataException("Landing Site demo requires the scrolling-sky BG2 owner.");
                break;
            case AttractDemoRoomSetup.KraidTimer:
                _enemies.SlotTable.Slots[0].VariableF = AttractDemoRomData.SetupValues.KraidFunctionTimer;
                break;
            case AttractDemoRoomSetup.DefeatedKraid:
                _system.SetBossBits(AreaId.Brinstar, BossBits.AreaBoss);
                break;
            default:
                throw new InvalidOperationException($"Undefined AttractDemoRoomSetup {pointer}.");
        }
    }
}
