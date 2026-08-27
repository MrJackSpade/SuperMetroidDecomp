using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

/// <summary>Cartridge-driven room loading kept separate from the gameplay-frame monolith.</summary>
public sealed partial class SuperMetroidRuntime
{
    /// <summary>The load-station record that most recently established this runtime.</summary>
    public LoadStationEntry? ActiveLoadStation { get; private set; }

    /// <summary>The selected room/state header for a generic cartridge-backed room.</summary>
    public CartridgeRoomHeader? ActiveRoom { get; private set; }

    /// <summary>The load-station door whose setup code established the current room.</summary>
    public CartridgeDoorHeader? ActiveDoor { get; private set; }

    /// <summary>Decompressed assets owned by <see cref="ActiveRoom"/>.</summary>
    public CartridgeRoomAssets? ActiveRoomAssets { get; private set; }

    /// <summary>The two native bank-$86 objects that carry Samus into fresh Ceres.</summary>
    public CeresElevatorArrivalState? CeresElevatorArrival { get; private set; }

    /// <summary>
    /// Executes the data-loading portion of native loading-state $1F: area six, station
    /// zero, the station-selected room, and its fresh-save state-selection branch.
    /// </summary>
    public InitialViewportResult InitializeStartingCeresRoom()
    {
        // `InitAndLoadGameData_Async` writes these exact indexes before calling
        // LoadFromLoadStation. Reading them through the general table proves which room the
        // retail cartridge selected; no Ceres room pointer is duplicated in host code.
        LoadStationEntry station = LoadStationEntry.Load(_addressSpace, areaIndex: 6, stationIndex: 0);
        CartridgeDoorHeader door = CartridgeDoorHeader.Load(_addressSpace, station.DoorPointer);
        if (door.DestinationRoomPointer != station.RoomPointer)
        {
            throw new InvalidDataException(
                $"Ceres station room $8F:{station.RoomPointer:X4} disagrees with door " +
                $"$83:{door.Pointer:X4}'s destination $8F:{door.DestinationRoomPointer:X4}.");
        }

        CartridgeRoomHeader room = CartridgeRoomHeader.Load(_addressSpace, door.DestinationRoomPointer);
        if (room.AreaIndex != 6)
        {
            throw new InvalidDataException(
                $"Fresh Ceres station targets area ${room.AreaIndex:X2}, expected area $06.");
        }

        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(_addressSpace, room);
        ActiveLoadStation = station;
        ActiveDoor = door;
        ActiveRoom = room;
        ActiveRoomAssets = assets;
        LevelData = assets.LevelData;
        Camera = new ScrollBoundaryCamera(assets.Scrolls);
        Camera.SetPosition(station.CameraX, station.CameraY);
        BackgroundScroll.Layer2ScrollX = room.State.Layer2ScrollX;
        BackgroundScroll.Layer2ScrollY = room.State.Layer2ScrollY;
        BackgroundScroll.PrimePreviousBlocks();

        // Gameplay initializes BG1SC=$51 and BG2SC=$49. DisplayViewablePartOfRoom stores
        // their $0800-word separation so the same streamer can address both circular maps.
        BackgroundStreamer = LevelData.CreateBackgroundStreamer(sizeOfBg2: 0x0800);
        ScrollingSky = null;
        LandingSiteEntry = null;
        assets.LoadGraphics(Vram, Cgram);

        Enemies.Load(
            _addressSpace,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            Vram,
            Cgram,
            System.NextRandom);
        Enemies.QueueGraphicsUploads(VramWrites);

        BackgroundScroll.Layer1XPosition = Camera.XPosition;
        BackgroundScroll.Layer1YPosition = Camera.YPosition;
        IReadOnlyList<BackgroundUpdateRequest> requests = BackgroundScroll.BuildInitialViewportRequests();
        LastBackgroundUpdateCount = requests.Count;
        int segmentCount = 0;
        foreach (BackgroundUpdateRequest request in requests)
        {
            TilemapStreamUpdate update = BackgroundStreamer.Build(request)
                ?? throw new InvalidOperationException("Starting Ceres room unexpectedly requested Mode 7 streaming.");
            update.ExecuteTo(Vram);
            segmentCount += update.Segments.Count;
        }
        BackgroundScroll.PrimePreviousBlocks();
        return new InitialViewportResult(requests.Count, segmentCount);
    }

    /// <summary>
    /// Applies $90:F1E9's Ceres-start Samus pose/state after room loading.
    /// </summary>
    /// <remarks>
    /// The native routine intentionally replaces the load-station Y coordinate with zero;
    /// elevator platform/projectile motion later carries front-facing Samus into the room.
    /// That elevator actor is a separate pending subsystem, so this method does not invent a
    /// floor placement or prematurely unlock controls.
    /// </remarks>
    public void InitializeCeresStartSamus()
    {
        if (ActiveRoom is null || ActiveLoadStation is null)
            throw new InvalidOperationException("The starting Ceres room must be loaded before Samus setup.");

        Samus = new SamusState
        {
            Pose = 0x00,
            AnimationFrame = 0,
            XPosition = ActiveLoadStation.SamusX,
            YPosition = 0,
        };
        Samus.LoadPowerSuitPalette(_addressSpace, Cgram);
        Samus.RefreshCollisionRadii(_addressSpace);
        Samus.InitializeAnimation(_addressSpace);
        Samus.LiquidPhysics.AreaIndex = ActiveRoom.AreaIndex;
        Samus.LiquidPhysics.RoomIndex = ActiveRoom.RoomIndex;
        Samus.PrimeGraphics(_addressSpace);
        PreviousMovementTypeForXray = Samus.ReadMovementType(_addressSpace);

        // SpawnEprojWithGfx receives enemy slot zero as its graphics owner. Preserve that
        // otherwise-surprising dependency: the Ceres elevator art uses exactly the base
        // tile and palette words produced while initializing the first room enemy.
        RoomEnemySlot graphicsOwner = Enemies.Slots[0];
        ushort graphicsIndex = unchecked((ushort)(
            graphicsOwner.VramTilesIndex | graphicsOwner.PaletteIndex));
        CeresElevatorArrival = new CeresElevatorArrivalState(
            _addressSpace,
            Samus,
            graphicsIndex);

        // `SamusCode_08_SetupForCeresStart` installs a locked frame handler. Reuse the
        // existing runtime switch to ensure ordinary movement cannot begin before the
        // elevator arrival sequence has been translated.
        GroundedSamusMovementEnabled = false;
    }

    /// <summary>Returns room-header geometry for either supported cartridge room path.</summary>
    private ActiveRoomGeometry GetActiveRoomGeometry()
    {
        if (ActiveRoom is not null)
        {
            return new ActiveRoomGeometry(
                ActiveRoom.AreaIndex,
                ActiveRoom.MapX,
                ActiveRoom.MapY,
                ActiveRoom.UpScroller,
                ActiveRoom.DownScroller);
        }

        if (LandingSiteEntry is not null)
        {
            return new ActiveRoomGeometry(
                LandingSiteEntry.AreaIndex,
                LandingSiteEntry.RoomMapX,
                LandingSiteEntry.RoomMapY,
                LandingSiteEntry.UpScroller,
                LandingSiteEntry.DownScroller);
        }

        throw new InvalidOperationException("No cartridge room header is active.");
    }
}

/// <summary>Room-header fields shared by camera tracking and minimap publication.</summary>
internal readonly record struct ActiveRoomGeometry(
    byte AreaIndex,
    byte MapX,
    byte MapY,
    byte UpScroller,
    byte DownScroller);
