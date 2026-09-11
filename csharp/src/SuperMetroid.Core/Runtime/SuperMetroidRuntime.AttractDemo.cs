using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    private AttractDemoInput? _attractDemoInput;
    private IDisposable? _attractDemoInputRestore;
    internal bool IsAttractDemo => _attractDemoInput is not null;

    private void BeginAttractSamusInput()
    {
        if (_attractDemoInput is null) return;
        Samus!.PreviousDrawNewInput = _attractDemoInput.Script.NewlyPressed;
        _attractDemoInput.StepStock(SuperMetroidGameState.PlayingDemo,
            Samus!.ReadMovementType(_addressSpace));
        _attractDemoInputRestore = Controller1.UseDemoInput(
            _attractDemoInput.Script.Held, _attractDemoInput.Script.NewlyPressed);
    }

    private void RestoreAttractPlayerInput()
    {
        _attractDemoInputRestore?.Dispose();
        _attractDemoInputRestore = null;
    }

    /// <summary>
    /// Loads the bank-$82/$91 demo scene without consulting or writing a player save.
    /// Uses the ordinary room, PLM, enemy, FX, and graphics loaders with demo progression.
    /// </summary>
    internal void InitializeAttractDemo(AttractDemoScene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        byte[] itemBits = new byte[Bank80SystemState.ItemBitByteCount];
        Array.Fill(itemBits, byte.MaxValue);
        System.LoadCollectedItemBytes(itemBits);
        System.LoadRoomChozoBytes(itemBits);
        System.LoadOpenedDoorBytes(new byte[Bank80SystemState.DoorBitByteCount]);
        System.LoadEventBytes(new byte[Bank80SystemState.EventByteCount]);
        System.LoadBossBytes(new byte[Bank80SystemState.AreaCount]);
        byte[] mapStations = new byte[Bank80SystemState.MapStationByteCount];
        mapStations.AsSpan(0, Bank80SystemState.AreaCount).Fill(byte.MaxValue);
        System.LoadMapStationBytes(mapStations);
        byte[] usedStations = new byte[Bank80SystemState.UsedSaveStationByteCount];
        Array.Fill(usedStations, byte.MaxValue);
        System.LoadUsedSaveStationBytes(usedStations);
        System.LoadExploredMapBytes(new byte[Bank80SystemState.ExploredMapAreaCount * Bank80SystemState.ExploredMapBytesPerArea]);

        Samus = new SamusState
        {
            XPosition = scene.SamusX, YPosition = scene.SamusY,
            EquippedItems = scene.Items, CollectedItems = scene.Items,
            EquippedBeams = scene.EquippedBeams, CollectedBeams = scene.CollectedBeams,
            Health = scene.Health, MaxHealth = scene.Health,
            Missiles = scene.Missiles, MaxMissiles = scene.Missiles,
            SuperMissiles = scene.SuperMissiles, MaxSuperMissiles = scene.SuperMissiles,
            PowerBombs = scene.PowerBombs, MaxPowerBombs = scene.PowerBombs,
        };
        ApplyAttractSamusSetup(scene.SamusSetupPointer);
        InitializeHud(new HudSnapshot(Samus.Health, Samus.MaxHealth,
            Samus.Missiles, Samus.MaxMissiles, Samus.SuperMissiles, Samus.MaxSuperMissiles,
            Samus.PowerBombs, Samus.MaxPowerBombs, Samus.EquippedItems, 0, 0, 0));
        RunNmi(0, true);
        var door = CartridgeDoorHeader.Load(_addressSpace, scene.DoorPointer);
        // The demo record independently supplies room and door. Do not replace room
        // selection with the door destination or derive placement from a load station.
        var room = LoadCartridgeRoomHeader(scene.RoomPointer);
        ActiveLoadStation = null;
        CeresElevatorArrival = null;
        LoadCartridgeRoom(door, room, scene.CameraX, scene.CameraY,
            RoomViewportLoadMode.DisplayInitialViewport);
        ApplyAttractRoomSetup(scene.RoomSetupPointer);
        Samus.LoadSuitPalette(_addressSpace, Cgram);
        Samus.RefreshCollisionRadii(_addressSpace);
        Samus.InitializeAnimation(_addressSpace);
        Samus.PrimeGraphics(_addressSpace);
        Samus.LiquidPhysics.RoomIdentity = room.Identity;
        GroundedSamusMovementEnabled = true;
        _attractDemoInput = new AttractDemoInput(scene);
    }

    private void ApplyAttractSamusSetup(ushort pointer)
    {
        var samus = Samus ?? throw new InvalidOperationException("Demo setup requires Samus.");
        switch (pointer)
        {
            case AttractDemoRomData.SamusSetup.LandingSite:
                samus.ApplyForwardFacingPoseSetup(_addressSpace);
                break;
            case AttractDemoRomData.SamusSetup.StandingRight:
                samus.Pose = SamusPoseIds.FacingRightNormalPose;
                break;
            case AttractDemoRomData.SamusSetup.LowHealthLeft:
                samus.Health = AttractDemoRomData.SetupValues.LowHealth;
                goto case AttractDemoRomData.SamusSetup.StandingLeft;
            case AttractDemoRomData.SamusSetup.StandingLeft:
                samus.Pose = SamusPoseIds.FacingLeftNormalPose;
                break;
            case AttractDemoRomData.SamusSetup.MorphLeft:
                samus.Pose = SamusPoseIds.MorphBallGroundLeftPose;
                break;
            case AttractDemoRomData.SamusSetup.FallingLeft:
                samus.Pose = SamusPoseIds.FallingLeftPose;
                break;
            case AttractDemoRomData.SamusSetup.DiagonalShinespark:
                samus.Shinespark.BeginDemoLaunch(_addressSpace, samus, SamusPoseIds.ShinesparkDiagonalRightPose);
                break;
            case AttractDemoRomData.SamusSetup.HorizontalShinespark:
                samus.Shinespark.BeginDemoLaunch(_addressSpace, samus, SamusPoseIds.ShinesparkHorizontalLeftPose);
                break;
            default:
                throw new InvalidDataException($"Unknown demo Samus setup $91:{pointer:X4}.");
        }
    }

    private void ApplyAttractRoomSetup(ushort pointer)
    {
        switch (pointer)
        {
            case AttractDemoRomData.RoomSetup.NoOp:
                break;
            case AttractDemoRomData.RoomSetup.ChargeBeamScroll:
                Camera!.Scrolls.SetStorage(AttractDemoRomData.SetupValues.ChargeBeamScrollIndex, RoomScrollState.RedBoundary);
                break;
            case AttractDemoRomData.RoomSetup.LandingSiteSky:
                // The shared land-sky loader has already selected the same vertical
                // BG2 page layout. Require that owner rather than writing a dummy mirror.
                if (ScrollingSky is null)
                    throw new InvalidDataException("Landing Site demo requires the scrolling-sky BG2 owner.");
                break;
            case AttractDemoRomData.RoomSetup.KraidTimer:
                Enemies.Slots[0].VariableF = AttractDemoRomData.SetupValues.KraidFunctionTimer;
                break;
            case AttractDemoRomData.RoomSetup.DefeatedKraid:
                System.SetBossBits(AreaId.Brinstar, BossBits.AreaBoss);
                break;
            default:
                throw new InvalidDataException($"Unknown demo room setup $82:{pointer:X4}.");
        }
    }
}
