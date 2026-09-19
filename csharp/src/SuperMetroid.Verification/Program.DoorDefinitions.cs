using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyCompiledDoorDefinitions()
    {
        ISnesAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        ushort[] doorPointers = EnumerateRetailDoorPointers().ToArray();
        AssertEqual(DoorDefinitions.HeaderCount, doorPointers.Length,
            "compiled door catalog contains every physical retail header");
        foreach (ushort doorPointer in doorPointers)
        {
            AssertEqual(CartridgeDoorHeader.Load(bus, doorPointer),
                DoorDefinitions.Get(doorPointer),
                $"compiled door $83:{doorPointer:X4}");
        }
        AssertEqual(
            CartridgeDoorHeader.Load(bus, DoorHeaderRomData.ElevatorPseudoDoorPointer),
            DoorDefinitions.Get(DoorHeaderRomData.ElevatorPseudoDoorPointer),
            "compiled shared elevator pseudo-door");

        string symbolPath = Path.GetFullPath(
            Path.Combine("upstream-sm", "assets", "names.txt"));
        ushort[] roomPointers = File.ReadLines(symbolPath)
            .Select(TryParseRoomHeaderPointer)
            .Where(pointer => pointer.HasValue)
            .Select(pointer => pointer!.Value)
            .Distinct()
            .Order()
            .ToArray();
        var catalogPointers = doorPointers.ToHashSet();
        catalogPointers.Add(DoorHeaderRomData.ElevatorPseudoDoorPointer);
        var referencedPointers = new HashSet<ushort>();
        int pseudoDoorReferenceCount = 0;
        int referenceCount = 0;
        foreach (ushort roomPointer in roomPointers)
        {
            ushort listPointer = RoomHeaderDefinitions.Get(roomPointer).DoorListPointer;
            DoorListDefinition compiled = DoorDefinitions.GetList(listPointer);
            int address = RoomHeaderRomData.BankAddress | listPointer;
            for (int index = 0; index < compiled.DoorPointers.Length; index++)
            {
                ushort nativePointer = ReadVerificationWord(bus, address + index * 2);
                AssertEqual(nativePointer, compiled.DoorPointers.Span[index],
                    $"room $8F:{roomPointer:X4} door-list entry {index}");
                AssertEqual(DoorDefinitions.Get(nativePointer),
                    DoorDefinitions.Resolve(listPointer, checked((byte)index)),
                    $"room $8F:{roomPointer:X4} BTS {index}");
                AssertEqual(DoorDefinitions.Get(nativePointer),
                    DoorDefinitions.Resolve(listPointer, checked((byte)(index | 0x80))),
                    $"room $8F:{roomPointer:X4} high-bit BTS {index}");
                referencedPointers.Add(nativePointer);
                if (nativePointer == DoorHeaderRomData.ElevatorPseudoDoorPointer)
                    pseudoDoorReferenceCount++;
                referenceCount++;
            }

            ushort followingWord = ReadVerificationWord(
                bus, address + compiled.DoorPointers.Length * 2);
            AssertTrue(!catalogPointers.Contains(followingWord),
                $"room $8F:{roomPointer:X4} compiled door-list length reaches native terminator");
            AssertThrows<InvalidDataException>(
                () => DoorDefinitions.Resolve(
                    listPointer, checked((byte)compiled.DoorPointers.Length)),
                $"room $8F:{roomPointer:X4} rejects the first out-of-range BTS");
        }

        AssertEqual(DoorDefinitions.DoorListCount, roomPointers.Length,
            "compiled catalog contains every retail room door list");
        AssertEqual(DoorDefinitions.RoomDoorReferenceCount, referenceCount,
            "compiled room door-reference count");
        AssertEqual(12, pseudoDoorReferenceCount,
            "shared elevator pseudo-door appears in every native elevator room list");
        AssertEqual(
            DoorDefinitions.RoomDoorReferenceCount - pseudoDoorReferenceCount + 1,
            referencedPointers.Count,
            "every physical room door is unique while elevator rooms share one pseudo-door");
        AssertThrows<ArgumentOutOfRangeException>(
            () => DoorDefinitions.Get(0xffff),
            "compiled doors reject arbitrary pointers");
        AssertThrows<ArgumentOutOfRangeException>(
            () => DoorDefinitions.GetList(0xffff),
            "compiled door lists reject arbitrary pointers");

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

    private sealed class DoorNoReadAddressSpace : ISnesAddressSpace
    {
        public byte ReadByte(int address) => throw new InvalidOperationException(
            $"Compiled door collision read address ${address:X6}.");

        public void WriteByte(int address, byte value) => throw new InvalidOperationException(
            $"Compiled door collision wrote address ${address:X6}.");
    }

    private sealed class DoorHeaderReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
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
