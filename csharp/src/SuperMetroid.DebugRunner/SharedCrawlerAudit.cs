using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Runs four unchanged retail populations through the shared creepy-crawly engine. Zoomer
/// already has the awake-Parlor regression; these cases prove the species-specific wrappers
/// and instruction tables for Sciser, Viola, Zeela, and Sova.
/// </summary>
internal static class SharedCrawlerAudit
{
    private readonly record struct RetailCase(
        string Name,
        ushort RoomHeader,
        ushort RoomState,
        ushort Definition,
        int SlotIndex,
        int EnemyCount,
        ushort SpeciesTableOffset);

    private static readonly RetailCase[] Cases =
    [
        new("Crab Maze Sciser", 0x957d, 0x958a, 0xd77f, 0, 8, 8),
        new("Post-Croc Shaft Viola", 0xab07, 0xab14, 0xdabf, 0, 4, 6),
        new("Brinstar Pre-Map Zeela", 0x9b9d, 0x9baa, 0xdc7f, 1, 4, 2),
        new("Crumble Shaft Sova", 0xa8f8, 0xa905, 0xdcbf, 0, 6, 4),
    ];

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        int totalMaps = 0;
        int totalObjPieces = 0;
        foreach (RetailCase retailCase in Cases)
        {
            CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, retailCase.RoomHeader);
            CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
            var vram = new SnesVram();
            var cgram = new SnesCgram();
            assets.LoadGraphics(vram, cgram);
            var random = new Bank80SystemState();
            var enemies = new RoomEnemySystem();
            enemies.Load(
                bus,
                room.State.EnemyPopulationPointer,
                room.State.EnemyTilesetPointer,
                vram,
                cgram,
                random.NextRandom,
                random.SetRandomNumber);

            if (room.State.Pointer != retailCase.RoomState ||
                enemies.EnemyCount != retailCase.EnemyCount)
            {
                throw new InvalidDataException(
                    $"{retailCase.Name} selected state ${room.State.Pointer:X4} with " +
                    $"{enemies.EnemyCount} enemies.");
            }

            RoomEnemySlot slot = enemies.Slots[retailCase.SlotIndex];
            CrawlerEnemyState state = enemies.CrawlerStates[retailCase.SlotIndex]
                ?? throw new InvalidDataException(
                    $"{retailCase.Name} slot {retailCase.SlotIndex} has no crawler state.");
            if (slot.EnemyDefinitionPointer != retailCase.Definition ||
                slot.Parameter2 != retailCase.SpeciesTableOffset ||
                state.Function != CrawlerEnemyFunction.InstructionPending)
            {
                throw new InvalidDataException(
                    $"{retailCase.Name} init failed: definition=${slot.EnemyDefinitionPointer:X4}, " +
                    $"species offset={slot.Parameter2}, function=$A3:{(ushort)state.Function:X4}.");
            }

            ushort cameraX = slot.XPosition > 128
                ? unchecked((ushort)(slot.XPosition - 128))
                : (ushort)0;
            ushort cameraY = slot.YPosition > 112
                ? unchecked((ushort)(slot.YPosition - 112))
                : (ushort)0;
            var samus = new SamusState
            {
                Health = 999,
                MaxHealth = 999,
                Pose = SamusState.FacingRightNormalPose,
                XPosition = slot.XPosition,
                YPosition = slot.YPosition,
            };
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);

            ushort startX = slot.XPosition;
            ushort startY = slot.YPosition;
            var maps = new HashSet<ushort>();
            var functions = new HashSet<CrawlerEnemyFunction>();
            for (int frame = 0; frame < 180; frame++)
            {
                enemies.StepFrame(
                    cameraX,
                    cameraY,
                    timeIsFrozen: false,
                    samus,
                    level: assets.LevelData);
                maps.Add(slot.SpritemapPointer);
                functions.Add(state.Function);
            }

            if (state.Function == CrawlerEnemyFunction.InstructionPending || maps.Count < 2 ||
                slot.XPosition == startX && slot.YPosition == startY)
            {
                throw new InvalidDataException(
                    $"{retailCase.Name} did not animate/move: function=$A3:{(ushort)state.Function:X4}, " +
                    $"functions={string.Join(',', functions.Select(x => $"${(ushort)x:X4}"))}, " +
                    $"maps={maps.Count}, position=({startX},{startY})->({slot.XPosition},{slot.YPosition}).");
            }

            var oam = new OamBuffer();
            oam.BeginFrame();
            enemies.DrawLayers(oam, cameraX, cameraY, 0, 7);
            oam.FinalizeFrame();
            if (oam.LastFinalizedSpriteCount == 0)
                throw new InvalidDataException($"{retailCase.Name} emitted no live ROM OBJ.");
            totalMaps += maps.Count;
            totalObjPieces += oam.LastFinalizedSpriteCount;
        }

        Console.WriteLine(
            "Shared crawler audit passed: retail Sciser, Viola, Zeela, and Sova wrappers " +
            $"entered common surface/fall movement, animated {totalMaps} distinct maps, " +
            $"and rendered {totalObjPieces} OBJ pieces across four rooms.");
        return 0;
    }
}
