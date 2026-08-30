using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// ROM-backed checkpoint for Kraid's translated room lockout and rise. The audit deliberately
/// ends at the first combat thinker: reaching that pointer proves the current slice, while the
/// explicit unsupported exception on the following frame keeps unfinished combat visible.
/// </summary>
internal static class KraidAudit
{
    private const ushort RoomPointer = 0xa59f;
    private const ushort PopulationPointer = 0x9eb5;
    private const ushort CameraX = 0;
    private const ushort CameraY = 256;

    private static readonly ushort[] ExpectedDefinitions =
        [0xe2bf, 0xe2ff, 0xe33f, 0xe37f, 0xe3bf, 0xe3ff, 0xe43f, 0xe47f];

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, RoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        VerifyRetailRoom(room);

        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState(0x1234);
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            XPosition = 300,
            YPosition = 456,
            Pose = SamusState.FacingRightNormalPose,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);

        var enemies = new RoomEnemySystem();
        enemies.Load(
            bus,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber,
            readRandomNumber: () => random.RandomNumber,
            level: assets.LevelData,
            samus: samus,
            isAreaBossDefeated: () => false,
            cameraX: CameraX,
            cameraY: CameraY);

        KraidEnemyState state = enemies.Kraid ??
            throw new InvalidDataException("Live Kraid room did not allocate typed encounter state.");
        RoomEnemySlot body = enemies.Slots[0];
        if (enemies.EnemyCount != ExpectedDefinitions.Length ||
            body.XPosition != 176 || body.YPosition != 592 || body.Health != 1000 ||
            body.VariableA != (ushort)KraidAiFunction.RestrictSamusToFirstScreen ||
            body.VariableF != 300 || !state.BackgroundTilemapsPrepared)
        {
            throw new InvalidDataException(
                $"Kraid initialization mismatch: count={enemies.EnemyCount}, body=" +
                $"({body.XPosition},{body.YPosition}) hp={body.Health}, " +
                $"function=$A7:{body.VariableA:X4}, timer={body.VariableF}, " +
                $"BG prepared={state.BackgroundTilemapsPrepared}.");
        }
        for (int slot = 0; slot < ExpectedDefinitions.Length; slot++)
        {
            if (enemies.Slots[slot].EnemyDefinitionPointer != ExpectedDefinitions[slot])
            {
                throw new InvalidDataException(
                    $"Kraid slot {slot} loaded ${enemies.Slots[slot].EnemyDefinitionPointer:X4}, " +
                    $"expected ${ExpectedDefinitions[slot]:X4}.");
            }
        }
        ushort[] expectedEighths = [125, 250, 375, 500, 625, 750, 875, 1000];
        ushort[] expectedQuarters = [250, 500, 750, 1000];
        if (!state.HealthEighthThresholds.SequenceEqual(expectedEighths) ||
            !state.HealthQuarterThresholds.SequenceEqual(expectedQuarters))
        {
            throw new InvalidDataException("Kraid health phase thresholds do not match retail arithmetic.");
        }

        var functions = new HashSet<KraidAiFunction>();
        ushort nailStartX = enemies.Slots[6].XPosition;
        bool sawNailMovement = false;
        int frame;
        for (frame = 0; frame < 1200; frame++)
        {
            functions.Add((KraidAiFunction)body.VariableA);
            if (body.VariableA == (ushort)KraidAiFunction.MainloopThinking)
                break;
            enemies.StepFrame(
                CameraX,
                CameraY,
                timeIsFrozen: false,
                samus,
                level: assets.LevelData);
            sawNailMovement |= enemies.Slots[6].XPosition != nailStartX;
        }

        if (body.VariableA != (ushort)KraidAiFunction.MainloopThinking ||
            samus.XPosition != 256 || body.XPosition != 176 || body.YPosition >= 457 ||
            state.TopTilemapUploadCount != 1 || state.BottomTilemapUploadCount != 1 ||
            state.MusicRequest != 5 || state.RiseRockSpawnRequestCount == 0 ||
            !sawNailMovement || functions.Count < 6)
        {
            throw new InvalidDataException(
                $"Kraid rise mismatch after {frame} frames: function=$A7:{body.VariableA:X4}, " +
                $"Samus X={samus.XPosition}, body=({body.XPosition},{body.YPosition}), " +
                $"uploads={state.TopTilemapUploadCount}/{state.BottomTilemapUploadCount}, " +
                $"music={state.MusicRequest}, rocks={state.RiseRockSpawnRequestCount}, " +
                $"nail moved={sawNailMovement}, functions={functions.Count}.");
        }

        VerifyDefeatedRoom(bus, room);
        Console.WriteLine(
            $"Kraid audit passed to first combat thinker in {frame} frames: retail 2x2 room, " +
            "eight-part population, phase thresholds, Samus lockout, BG2 upload cadence, " +
            "rise rocks/music, half-pixel body motion, fingernail motion, and defeated-room deletion.");
        return 0;
    }

    private static void VerifyRetailRoom(CartridgeRoomHeader room)
    {
        if (room.AreaIndex != 1 || room.WidthInScreens != 2 || room.HeightInScreens != 2 ||
            room.State.Pointer != 0xa5b1 || room.State.EnemyPopulationPointer != PopulationPointer)
        {
            throw new InvalidDataException(
                $"Kraid room mismatch: area={room.AreaIndex}, " +
                $"size={room.WidthInScreens}x{room.HeightInScreens}, " +
                $"state=$8F:{room.State.Pointer:X4}, population=$A1:" +
                $"{room.State.EnemyPopulationPointer:X4}.");
        }
    }

    private static void VerifyDefeatedRoom(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room)
    {
        var defeated = new RoomEnemySystem();
        defeated.Load(
            bus,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            new SnesVram(),
            new SnesCgram(),
            () => 0,
            isAreaBossDefeated: () => true);
        if (defeated.EnemyCount != ExpectedDefinitions.Length || defeated.Kraid is null ||
            defeated.Slots.Take(ExpectedDefinitions.Length).Any(
                slot => !slot.Properties.HasAny(
                    EnemyProperties.Deleted | EnemyProperties.Invisible)))
        {
            throw new InvalidDataException(
                "Defeated Kraid room did not retain and delete all eight native part records.");
        }
    }
}
