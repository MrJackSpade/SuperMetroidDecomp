using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Exact retail lifecycle proof for Wrecked Ship attic PLM $84:BB05.</summary>
internal static partial class RetailPlmPopulationAudit
{
    private const ushort WreckedShipAtticRoom = 0xca52;

    /// <summary>
    /// Production-loads both retail attic states and executes the installed cartridge
    /// callback. This catches both population omissions and false implementations of the
    /// deliberately inert $BAFA routine.
    /// </summary>
    public static int AuditWreckedShipAttic(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        if (ReadWord(bus, 0x84bb07) != RoomPlmInstructionLists.WreckedShipAttic ||
            ReadWord(bus, 0x840000 | RoomPlmInstructionLists.WreckedShipAttic) !=
                RoomPlmInstructionCodes.InstallPreInstruction ||
            ReadWord(bus, 0x84bb01) != WreckedShipAtticPlmRomData.NoOpCallback ||
            ReadWord(bus, 0x84bb03) != RoomPlmInstructionCodes.Sleep)
        {
            throw new InvalidDataException(
                "Retail BB05 header/list no longer matches the translated inert coroutine.");
        }

        AuditWreckedShipAtticState(bus, statePointer: 0xca64, expectedPopulation: 0xc231);
        AuditWreckedShipAtticState(bus, statePointer: 0xca7e, expectedPopulation: 0xc2ff);
        Console.WriteLine(
            "Wrecked Ship attic PLM audit passed: retail $CA64/$CA7E production-loaded; " +
            "both $BB05 actors install $BAFA, sleep at $BB03, and preserve terrain.");
        return 0;
    }

    private static void AuditWreckedShipAtticState(
        SuperMetroidAddressSpace bus,
        ushort statePointer,
        ushort expectedPopulation)
    {
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, WreckedShipAtticRoom) with
        {
            State = CartridgeRoomState.Load(bus, statePointer),
        };
        if (room.State.PlmPointer != expectedPopulation)
        {
            throw new InvalidDataException(
                $"Retail attic state $8F:{statePointer:X4} selected population " +
                $"$8F:{room.State.PlmPointer:X4}, expected $8F:{expectedPopulation:X4}.");
        }

        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        var plms = new RoomPlmSystem();
        plms.LoadRoomPopulation(
            bus,
            assets.LevelData,
            assets.LevelData.CreateBackgroundStreamer(),
            new SnesVram(),
            room.State.PlmPointer,
            new Bank80SystemState(),
            room.AreaIndex,
            () => new SamusState(),
            () => false);
        RoomPlmSlotSnapshot initial = plms.PopulationSlots.Single(
            slot => slot.HeaderPointer == RoomPlmHeaders.WreckedShipAttic);
        RoomCollisionBlock before = assets.LevelData.GetCollisionBlockByIndex(initial.BlockIndex);
        BackgroundTilemapStreamer streamer = assets.LevelData.CreateBackgroundStreamer();

        plms.Step(bus, assets.LevelData, streamer, 0, 0, 0, assets.Scrolls);
        RoomPlmSlotSnapshot sleeping = plms.PopulationSlots.Single(
            slot => slot.HeaderPointer == RoomPlmHeaders.WreckedShipAttic);
        if (sleeping.PreInstruction != WreckedShipAtticPlmRomData.NoOpCallback ||
            sleeping.InstructionPointer !=
                unchecked((ushort)(RoomPlmInstructionLists.WreckedShipAttic + 4)))
        {
            throw new InvalidDataException(
                $"Retail attic state $8F:{statePointer:X4} did not install $BAFA and " +
                "sleep at $BB03.");
        }

        plms.Step(bus, assets.LevelData, streamer, 0, 0, 0, assets.Scrolls);
        RoomCollisionBlock after = assets.LevelData.GetCollisionBlockByIndex(initial.BlockIndex);
        if (!plms.PopulationSlots.Any(
                slot => slot.HeaderPointer == RoomPlmHeaders.WreckedShipAttic) ||
            after.LevelWord != before.LevelWord || after.Behavior != before.Behavior)
        {
            throw new InvalidDataException(
                $"Retail attic state $8F:{statePointer:X4} callback was not inert/resident.");
        }
    }
}
