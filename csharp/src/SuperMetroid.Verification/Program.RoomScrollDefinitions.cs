using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyCompiledRoomScrollDefinitions()
    {
        string symbolPath = Path.GetFullPath(
            Path.Combine("upstream-sm", "assets", "names.txt"));
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        ushort[] roomPointers = File.ReadLines(symbolPath)
            .Select(TryParseRoomHeaderPointer)
            .Where(pointer => pointer.HasValue)
            .Select(pointer => pointer!.Value)
            .Distinct()
            .Order()
            .ToArray();

        var auditedStates = new HashSet<ushort>();
        var explicitPointers = new HashSet<ushort>();
        int explicitStateCount = 0;
        int implicitStateCount = 0;
        foreach (ushort roomPointer in roomPointers)
        foreach (RoomStateSelectionContext context in BuildRoomStateAuditContexts())
        {
            CartridgeRoomHeader header = CartridgeRoomHeader.LoadUsingCompiledSelection(
                bus, roomPointer, context);
            if (!auditedStates.Add(header.State.Pointer))
                continue;

            ushort scrollPointer = header.State.ScrollPointer;
            byte[] expected = new byte[RoomScrollGrid.StorageByteCount];
            if (unchecked((short)scrollPointer) < 0)
            {
                explicitStateCount++;
                explicitPointers.Add(scrollPointer);
                int source = RoomAssetRomData.Tilesets.DefinitionBank | scrollPointer;
                for (int index = 0; index < expected.Length; index++)
                    expected[index] = bus.ReadByte(source + index);
                AssertTrue(expected.AsSpan().SequenceEqual(
                        RoomScrollDefinitions.Get(scrollPointer).Storage.Span),
                    $"compiled explicit scroll allocation $8F:{scrollPointer:X4}");
            }
            else
            {
                implicitStateCount++;
                RoomScrollState lastRow = RoomScrollStates.FromCartridge(
                    unchecked((byte)(scrollPointer + 1)),
                    $"verification implicit scroll word ${scrollPointer:X4}");
                int logicalCount = header.WidthInScreens * header.HeightInScreens;
                for (int index = 0; index < expected.Length; index++)
                {
                    expected[index] = index < logicalCount
                        ? (byte)(index / header.WidthInScreens == header.HeightInScreens - 1
                            ? lastRow
                            : RoomScrollState.Green)
                        : (byte)RoomScrollState.RedBoundary;
                }
            }

            RoomScrollGrid compiled = RoomScrollDefinitions.CreateGrid(
                new RoomScrollNoReadAddressSpace(bus),
                scrollPointer,
                header.WidthInScreens,
                header.HeightInScreens);
            AssertTrue(expected.AsSpan().SequenceEqual(compiled.Storage),
                $"compiled scroll grid for state $8F:{header.State.Pointer:X4}");
        }

        AssertEqual(RoomStateDefinitions.RetailStateCount, auditedStates.Count,
            "compiled scroll audit reaches every room state");
        AssertEqual(200, explicitStateCount,
            "retail states selecting explicit scroll allocations");
        AssertEqual(123, implicitStateCount,
            "retail states selecting implicit scroll allocations");
        AssertEqual(RoomScrollDefinitions.ExplicitDefinitionCount, explicitPointers.Count,
            "compiled scroll catalog contains every distinct explicit allocation");
        AssertThrows<ArgumentOutOfRangeException>(
            () => RoomScrollDefinitions.Get(0xffff),
            "compiled scroll catalog rejects arbitrary pointers");
        AssertThrows<ArgumentException>(
            () => RoomScrollGrid.LoadCompiled(bus, [0], 1, 1),
            "compiled scroll installation rejects incomplete storage");

        ushort ceresPointer = LoadStationDefinitions.Get(AreaId.Ceres, 0).RoomPointer;
        CartridgeRoomHeader ceres = CartridgeRoomHeader.LoadUsingCompiledSelection(
            bus, ceresPointer);
        var guardedCeres = new SuperMetroidRuntime(new RoomScrollSourceReadGuard(
            bus, ceres.State.ScrollPointer));
        guardedCeres.InitializeStartingCeresRoom();
        var expectedCeres = new byte[RoomScrollGrid.StorageByteCount];
        int ceresLogicalCount = ceres.WidthInScreens * ceres.HeightInScreens;
        RoomScrollState ceresLastRow = RoomScrollStates.FromCartridge(
            unchecked((byte)(ceres.State.ScrollPointer + 1)),
            "Ceres implicit scroll word");
        for (int index = 0; index < expectedCeres.Length; index++)
        {
            expectedCeres[index] = index < ceresLogicalCount
                ? (byte)(index / ceres.WidthInScreens == ceres.HeightInScreens - 1
                    ? ceresLastRow
                    : RoomScrollState.Green)
                : (byte)RoomScrollState.RedBoundary;
        }
        AssertTrue(expectedCeres.AsSpan().SequenceEqual(guardedCeres.Camera!.Scrolls.Storage),
            "production Ceres entry installs compiled scroll storage");

        CartridgeRoomState landingState = RoomStateDefinitions.Get(
            RoomStateSelectionDefinitions.Select(RoomHeaderPointers.LandingSite, default));
        var guardedLanding = new SuperMetroidRuntime(new RoomScrollSourceReadGuard(
            bus, landingState.ScrollPointer));
        guardedLanding.InitializeLandingSiteCamera();
        AssertTrue(RoomScrollDefinitions.Get(landingState.ScrollPointer).Storage.Span
                .SequenceEqual(guardedLanding.Camera!.Scrolls.Storage),
            "special Landing Site entry installs compiled scroll storage");

        Console.WriteLine(
            "Room scrolls: all 323 states (200 explicit/123 implicit), 159 distinct " +
            "allocations, Ceres, and Landing Site match without native scroll reads.");
    }

    private sealed class RoomScrollNoReadAddressSpace(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        public byte ReadByte(int address) => throw new InvalidOperationException(
            $"Compiled room-scroll construction read address ${address:X6}.");

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }

    private sealed class RoomScrollSourceReadGuard(
        ISnesAddressSpace source,
        ushort scrollPointer) : ISnesAddressSpace
    {
        private readonly int _start = RoomAssetRomData.Tilesets.DefinitionBank | scrollPointer;

        public byte ReadByte(int address)
        {
            if (address >= _start && address < _start + RoomScrollGrid.StorageByteCount)
            {
                throw new InvalidOperationException(
                    $"Production runtime read native room-scroll data at ${address:X6}.");
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
