using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyCompiledDoorDefinitions()
    {
        SuperMetroidAddressSpace bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        ushort[] doorPointers = EnumerateRetailDoorPointers().ToArray();
        AssertEqual(DoorDefinitions.HeaderCount, doorPointers.Length,
            "compiled door catalog contains every physical retail header");
        foreach (ushort doorPointer in doorPointers)
        {
            AssertEqual(SuperMetroid.AssetExtraction.CartridgeDoorHeaderImporter.Load(bus, doorPointer),
                DoorDefinitions.Get(doorPointer),
                $"compiled door $83:{doorPointer:X4}");
        }
        AssertEqual(
            SuperMetroid.AssetExtraction.CartridgeDoorHeaderImporter.Load(bus, DoorHeaderRomData.ElevatorPseudoDoorPointer),
            DoorDefinitions.Get(DoorHeaderRomData.ElevatorPseudoDoorPointer),
            "compiled shared elevator pseudo-door");

        VerifyRetailDoorListMapping(bus);

        VerifyCompiledDoorListCollision();

        var guardedRuntime = new SuperMetroidRuntime(new DoorHeaderReadGuard(bus));
        guardedRuntime.InitializeStartingCeresRoom();
        AssertEqual(
            DoorDefinitions.Get(LoadStationDefinitions.Get(
                SuperMetroid.Core.Game.AreaId.Ceres, 0).DoorPointer),
            guardedRuntime.ActiveDoor!,
            "production Ceres entry uses its compiled physical door");

        Console.WriteLine(
            "Doors: 597 physical headers, the shared elevator pseudo-door, and 262 room " +
            "lists/603 BTS references match; " +
            "production entry and collision reject native door reads.");
    }

    private static void VerifyCompiledDoorListCollision()
    {
        DoorListDefinition landingDoors = DoorDefinitions.GetList(
            RoomHeaderDefinitions.Get(RoomHeaderPointers.LandingSite).DoorListPointer);
        var level = new RoomLevelData(
            1, 1, [0], [0], [0], [], doorListPointer: landingDoors.Pointer);
        CartridgeDoorHeader resolved = level.ResolveDoorCollision(
            new DoorNoReadAddressSpace(), behavior: 0, samusPose: 0);
        AssertEqual(DoorDefinitions.Get(landingDoors.DoorPointers.Span[0]), resolved,
            "production collision resolves compiled BTS index without a bus read");
        AssertEqual(resolved, level.PendingDoorTransition!,
            "compiled collision publishes the selected transition");

    }

    private static IEnumerable<ushort> EnumerateRetailDoorPointers()
    {
        for (int pointer = DoorHeaderRomData.PreFxBlockStart;
             pointer <= DoorHeaderRomData.PreFxBlockEnd;
             pointer += DoorHeaderRomData.RecordByteCount)
        {
            yield return checked((ushort)pointer);
        }

        for (int pointer = DoorHeaderRomData.PostFxBlockStart;
             pointer <= DoorHeaderRomData.PostFxBlockEnd;
             pointer += DoorHeaderRomData.RecordByteCount)
        {
            yield return checked((ushort)pointer);
        }
    }

    private static ushort ReadVerificationWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private sealed class DoorNoReadAddressSpace : ISnesAddressSpace, IImportCartridgeSource
    {
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public static byte ReadByte(int address) => throw new InvalidOperationException(
            $"Compiled door collision read address ${address:X6}.");

        public void WriteByte(int address, byte value) => throw new InvalidOperationException(
            $"Compiled door collision wrote address ${address:X6}.");
    }

    private sealed class DoorHeaderReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (IsDoorHeaderAddress(address))
            {
                throw new InvalidOperationException(
                    $"Production runtime read native door-header data at ${address:X6}.");
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);

        private static bool IsDoorHeaderAddress(int address)
        {
            int pointer = address - DoorHeaderRomData.BankAddress;
            return pointer >= DoorHeaderRomData.PreFxBlockStart &&
                    pointer < DoorHeaderRomData.PreFxBlockEnd + DoorHeaderRomData.RecordByteCount ||
                   pointer >= DoorHeaderRomData.PostFxBlockStart &&
                    pointer < DoorHeaderRomData.PostFxBlockEnd + DoorHeaderRomData.RecordByteCount;
        }
    }
}
