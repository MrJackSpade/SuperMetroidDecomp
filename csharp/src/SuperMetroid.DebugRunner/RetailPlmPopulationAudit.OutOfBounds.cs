using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Retail proof for the two shipped off-room colored-door population records.</summary>
internal static partial class RetailPlmPopulationAudit
{
    private static readonly OutOfBoundsPlmScenario[] OutOfBoundsPlmScenarios =
    [
        new(
            RoomPointer: 0x9c5e,
            StatePointer: 0x9c6b,
            PopulationPointer: 0x8548,
            RecordIndex: 0,
            HeaderPointer: RoomPlmHeaders.GreenDoorFacingRight,
            BlockX: 1,
            BlockY: 38),
        new(
            RoomPointer: 0xd72a,
            StatePointer: 0xd737,
            PopulationPointer: 0xc6ef,
            RecordIndex: 1,
            HeaderPointer: RoomPlmHeaders.GreenDoorFacingLeft,
            BlockX: 78,
            BlockY: 38),
    ];

    /// <summary>
    /// Production-loads and advances both exact retail states, proving their native block
    /// indices and descending slots survive setup without touching logical host bounds.
    /// </summary>
    public static int AuditOutOfBoundsSetup(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        foreach (OutOfBoundsPlmScenario scenario in OutOfBoundsPlmScenarios)
        {
            CartridgeRoomHeader defaultRoom = CartridgeRoomHeader.Load(bus, scenario.RoomPointer);
            CartridgeRoomState exactState = CartridgeRoomState.Load(bus, scenario.StatePointer);
            if (exactState.PlmPointer != scenario.PopulationPointer)
            {
                throw new InvalidDataException(
                    $"Room/state $8F:{scenario.RoomPointer:X4}/${scenario.StatePointer:X4} " +
                    $"selected population $8F:{exactState.PlmPointer:X4}, expected " +
                    $"$8F:{scenario.PopulationPointer:X4}.");
            }

            CartridgeRoomHeader room = defaultRoom with { State = exactState };
            CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
            ScrollAuditPopulationRecord[] records = ReadScrollAuditPopulation(
                bus,
                exactState.PlmPointer);
            ScrollAuditPopulationRecord source = records[scenario.RecordIndex];
            if (source.Header != scenario.HeaderPointer ||
                source.BlockX != scenario.BlockX ||
                source.BlockY != scenario.BlockY)
            {
                throw new InvalidDataException(
                    $"Retail off-room PLM record {scenario.RecordIndex} no longer matches " +
                    $"header/coordinate $84:{scenario.HeaderPointer:X4} " +
                    $"({scenario.BlockX},{scenario.BlockY}).");
            }

            int nativeBlockIndex = scenario.BlockY * assets.LevelData.WidthInBlocks +
                scenario.BlockX;
            if (nativeBlockIndex < assets.LevelData.ForegroundEntries.Length)
            {
                throw new InvalidDataException(
                    $"Retail scenario $8F:{scenario.StatePointer:X4} unexpectedly resolves " +
                    $"inside logical terrain at block {nativeBlockIndex}.");
            }

            var system = new Bank80SystemState();
            var plms = new RoomPlmSystem();
            BackgroundTilemapStreamer streamer = assets.LevelData.CreateBackgroundStreamer();
            int loaded = plms.LoadRoomPopulation(
                bus,
                assets.LevelData,
                streamer,
                new SnesVram(),
                exactState.PlmPointer,
                system,
                room.AreaIndex,
                getSamus: () => new SamusState(),
                isAreaTorizoDefeated: () => false,
                isTourianStatueFinished: () => false,
                hasAreaBossBit: _ => false,
                hasEvent: system.HasEvent,
                setEvent: system.SetEvent);
            if (loaded != records.Length)
            {
                throw new InvalidDataException(
                    $"Retail state $8F:{scenario.StatePointer:X4} loaded {loaded} records, " +
                    $"expected {records.Length}.");
            }

            RoomPlmSlotSnapshot slot = plms.PopulationSlots.Single(candidate =>
                candidate.HeaderPointer == scenario.HeaderPointer &&
                candidate.BlockIndex == nativeBlockIndex);
            int expectedSlot = 39 - scenario.RecordIndex;
            if (slot.NativeSlotIndex != expectedSlot)
            {
                throw new InvalidDataException(
                    $"Retail state $8F:{scenario.StatePointer:X4} placed record " +
                    $"{scenario.RecordIndex} in slot {slot.NativeSlotIndex}, expected " +
                    $"native slot {expectedSlot}.");
            }

            // The first gameplay PLM pass performs the door's authored four-block draw.
            // It must remain a harmless write in the bounded native tail, not a deferred
            // host bounds exception.
            plms.Step(bus, assets.LevelData, streamer, 0, 0, 0, assets.Scrolls);
        }

        Console.WriteLine(
            "Out-of-bounds PLM audit passed: Fireflea and Colosseum retail states " +
            "production-loaded and advanced with native slot/index ordering.");
        return 0;
    }

    private readonly record struct OutOfBoundsPlmScenario(
        ushort RoomPointer,
        ushort StatePointer,
        ushort PopulationPointer,
        int RecordIndex,
        ushort HeaderPointer,
        byte BlockX,
        byte BlockY);
}
