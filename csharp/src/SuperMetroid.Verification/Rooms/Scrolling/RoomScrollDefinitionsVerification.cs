using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Rooms.RoomMainCodeCatalogVerification;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Rooms.Scrolling;
using SuperMetroid.Verification.Harness;

/// <summary>Checks compiled explicit and implicit room scroll grids across every retail room state.</summary>
internal static class RoomScrollDefinitionsVerification
{
    /// <summary>Compares all state grids and Ceres entry with cartridge behavior while blocking scroll-source reads.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for room scroll definitions")]
    internal static void VerifyCompiledRoomScrollDefinitions()
    {
        string symbolPath = Path.GetFullPath(
            Path.Combine("upstream-sm", "assets", "names.txt"));
        SuperMetroidAddressSpace bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        ushort[] roomPointers = File.ReadLines(symbolPath)
            .Select(TryParseRoomHeaderPointer)
            .Where(pointer => pointer.HasValue)
            .Select(pointer => (pointer ?? throw new InvalidOperationException("Verification requires pointer.")))
            .Distinct()
            .Order()
            .ToArray();

        var scrolls = new RoomScrollMemory(bus);
        var auditedStates = new HashSet<ushort>();
        var explicitPointers = new HashSet<ushort>();
        int explicitStateCount = 0;
        int implicitStateCount = 0;
        foreach (ushort roomPointer in roomPointers)
        foreach (RoomStateSelectionContext context in BuildRoomStateAuditContexts())
        {
            CartridgeRoomHeader header = CartridgeRoomHeader.LoadUsingCompiledSelection(
                roomPointer, context);
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
                    expected[index] = bus.ReadCartridgeByte(source + index);
                AssertTrue(expected.SequenceEqual(
                        RoomScrollDefinitions.Get(scrollPointer).Storage.Span),
                    $"compiled explicit scroll allocation $8F:{scrollPointer:X4}");
            }
            else
            {
                implicitStateCount++;
                RoomScrollState lastRow = RoomScrollStates.FromCartridge(
                    unchecked((byte)(scrollPointer + 1)),
                    $"verification implicit scroll word ${scrollPointer:X4}");
                int logicalCount = header.Size.X * header.Size.Y;
                for (int index = 0; index < expected.Length; index++)
                {
                    // $82:E88D writes only the room's cells; the rest keeps the WRAM the
                    // previous grid (here, the previous audited state) left behind.
                    expected[index] = index < logicalCount
                        ? (byte)(index / header.Size.X == header.Size.Y - 1
                            ? lastRow
                            : RoomScrollState.Green)
                        : scrolls.Zones[index];
                }
            }

            RoomScrollGrid compiled = RoomScrollDefinitions.CreateGrid(
                NewRoomScrollNoReadAddressSpace(bus),
                scrollPointer,
                header.Size);
            AssertTrue(expected.SequenceEqual(compiled.Storage),
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
            () => RoomScrollGrid.LoadCompiled(bus, [0], new Extent16<RoomScreens>(1, 1)),
            "compiled scroll installation rejects incomplete storage");

        ushort ceresPointer = LoadStationDefinitions.Get(AreaId.Ceres, 0).RoomPointer;
        CartridgeRoomHeader ceres = CartridgeRoomHeader.LoadUsingCompiledSelection(
            ceresPointer);
        var guardedCeres = CreateRetailRuntimeFixture(NewRoomScrollSourceReadGuard(
            bus, ceres.State.ScrollPointer));
        var ceresResidue = new byte[RoomScrollGrid.StorageByteCount];
        for (int index = 0; index < ceresResidue.Length; index++)
            ceresResidue[index] = scrolls.Zones[index];
        guardedCeres.RoomLoader.InitializeStartingCeresRoom();
        var expectedCeres = new byte[RoomScrollGrid.StorageByteCount];
        int ceresLogicalCount = ceres.Size.X * ceres.Size.Y;
        RoomScrollState ceresLastRow = RoomScrollStates.FromCartridge(
            unchecked((byte)(ceres.State.ScrollPointer + 1)),
            "Ceres implicit scroll word");
        for (int index = 0; index < expectedCeres.Length; index++)
        {
            expectedCeres[index] = index < ceresLogicalCount
                ? (byte)(index / ceres.Size.X == ceres.Size.Y - 1
                    ? ceresLastRow
                    : RoomScrollState.Green)
                : ceresResidue[index];
        }
        AssertTrue(expectedCeres.SequenceEqual(guardedCeres.Room.LoadedCamera.Scrolls.Storage),
            "production Ceres entry installs compiled scroll storage");

        Console.WriteLine(
            "Room scrolls: all 323 states (200 explicit/123 implicit), 159 distinct " +
            "allocations and Ceres match without native scroll reads.");
    }

    /// <summary>Allows only prior WRAM scroll storage so implicit grids retain the previous room's residue.</summary>
    private static MutableCartridgeReadGuard NewRoomScrollNoReadAddressSpace(ISnesAddressSpace source)
    {
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(address => address is not (>= RoomScrollGridConstants.WorkRamAddress and
                < RoomScrollGridConstants.WorkRamAddress + RoomScrollGrid.StorageByteCount),
            address => $"Compiled room-scroll construction read address ${address:X6}.");
        return guard;
    }

    /// <summary>Rejects runtime access to the selected cartridge scroll allocation during Ceres entry.</summary>
    private static MutableCartridgeReadGuard NewRoomScrollSourceReadGuard(ISnesAddressSpace source, ushort scrollPointer)
    {
        int start = RoomAssetRomData.Tilesets.DefinitionBank | scrollPointer;
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(address => address >= start && address < start + RoomScrollGrid.StorageByteCount,
            address => $"Production runtime read native room-scroll data at ${address:X6}.");
        return guard;
    }

    /// <summary>Runs the complete retail room scroll audit from the command dispatcher.</summary>
    [VerificationCommand("--room-scroll-definitions")]
    internal static int RoomScrollDefinitionsCommand()
    {
        Suite(nameof(VerifyCompiledRoomScrollDefinitions), () => VerifyCompiledRoomScrollDefinitions());
        return 0;
    }
}
