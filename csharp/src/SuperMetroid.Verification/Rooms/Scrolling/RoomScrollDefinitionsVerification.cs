using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Rooms.RoomMainCodeCatalogVerification;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Rooms.Scrolling;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.AssetExtraction.World.RoomStates;

/// <summary>Checks compiled explicit and implicit room scroll grids across every retail room state.</summary>
internal static class RoomScrollDefinitionsVerification
{
    /// <summary>Compares all state grids and Ceres entry with cartridge behavior while blocking scroll-source reads.</summary>
    internal static void VerifyCompiledRoomScrollDefinitions()
    {
        string symbolPath = Path.GetFullPath(
            Path.Combine("upstream-sm", "assets", "names.txt"));
        SuperMetroidAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        ushort[] roomPointers = File.ReadLines(symbolPath)
            .Select(TryParseRoomHeaderPointer)
            .Where(pointer => pointer.HasValue)
            .Select(pointer => (pointer ?? throw new InvalidOperationException("Verification requires pointer.")))
            .Distinct()
            .Order()
            .ToArray();

        var storage = RoomScrollGrid.PowerOnCells();
        var auditedStates = new HashSet<ushort>();
        var explicitPointers = new HashSet<ushort>();
        int explicitStateCount = 0;
        int implicitStateCount = 0;
        foreach (ushort roomPointer in roomPointers)
        foreach (RoomStateSelectionContext context in BuildRoomStateAuditContexts())
        {
            SelectedRoom header = SelectedRoom.LoadUsingCompiledSelection(
                RoomHeaderDefinitions.Get(roomPointer), context);
            if (!auditedStates.Add(header.State.Pointer))
                continue;

            ushort scrollPointer = header.State.ScrollPointer;
            var expected = new RoomScrollState[RoomScrollGrid.KeptCellCount];
            if (unchecked((short)scrollPointer) < 0)
            {
                explicitStateCount++;
                explicitPointers.Add(scrollPointer);
                int source = RoomTilesetTableConstants.DefinitionBank | scrollPointer;
                var native = new byte[RoomScrollGrid.KeptCellCount];
                for (int index = 0; index < native.Length; index++)
                    native[index] = bus.ReadCartridgeByte(source + index);
                // The table's fifty bytes are compared as the cells the camera reads them as.
                expected = RoomScrollStates.FromStorageBytes(native);
                AssertTrue(expected.SequenceEqual(RoomScrollDefinitions.Get(scrollPointer).Cells),
                    $"compiled explicit scroll table $8F:{scrollPointer:X4}");
            }
            else
            {
                implicitStateCount++;
                RoomScrollState lastRow = RoomScrollAuthoredBytes.FromAuthored(
                    unchecked((byte)(scrollPointer + 1)),
                    $"verification implicit scroll word ${scrollPointer:X4}");
                int logicalCount = header.Size.X * header.Size.Y;
                for (int index = 0; index < expected.Length; index++)
                {
                    // $82:E88D writes only the room's cells; the rest keeps the cells the
                    // previous grid (here, the previous audited state) left behind.
                    expected[index] = index < logicalCount
                        ? index / header.Size.X == header.Size.Y - 1
                            ? lastRow
                            : RoomScrollState.Green
                        : storage.Read(index);
                }
            }

            RoomScrollGrid compiled = RoomScrollGrid.FromDefinition(
                storage,
                header.State.Scroll,
                header.Size);
            AssertTrue(expected.SequenceEqual(compiled.KeptStates),
                $"compiled scroll grid for state $8F:{header.State.Pointer:X4}");
        }

        AssertEqual(RoomStateDefinitions.RetailStateCount, auditedStates.Count,
            "compiled scroll audit reaches every room state");
        AssertEqual(200, explicitStateCount,
            "retail states selecting explicit scroll allocations");
        AssertEqual(123, implicitStateCount,
            "retail states selecting implicit scroll allocations");
        AssertEqual(RoomScrollDefinitionsConstants.ExplicitDefinitionCount, explicitPointers.Count,
            "compiled scroll catalog contains every distinct explicit allocation");
        AssertThrows<ArgumentException>(
            () => RoomScrollGrid.LoadNativeTable([0], new Extent16<RoomScreens>(1, 1)),
            "compiled scroll installation rejects incomplete storage");

        SelectedRoom ceres = SelectedRoom.LoadUsingCompiledSelection(
            SuperMetroid.Core.Progression.LoadStationDefinitions.CeresElevator.Placement.Room);
        var guardedCeres = CreateRetailRuntimeFixture();
        // A new session's scroll cells start cleared, so Ceres's cells past its own read red.
        RoomScrollState[] ceresResidue = [.. Enumerable.Repeat(RoomScrollState.RedBoundary, RoomScrollGrid.KeptCellCount)];
        guardedCeres.RoomLoader.InitializeStartingCeresRoom();
        var expectedCeres = new RoomScrollState[RoomScrollGrid.KeptCellCount];
        int ceresLogicalCount = ceres.Size.X * ceres.Size.Y;
        RoomScrollState ceresLastRow = RoomScrollAuthoredBytes.FromAuthored(
            unchecked((byte)(ceres.State.ScrollPointer + 1)),
            "Ceres implicit scroll word");
        for (int index = 0; index < expectedCeres.Length; index++)
        {
            expectedCeres[index] = index < ceresLogicalCount
                ? index / ceres.Size.X == ceres.Size.Y - 1
                    ? ceresLastRow
                    : RoomScrollState.Green
                : ceresResidue[index];
        }
        AssertTrue(expectedCeres.SequenceEqual(guardedCeres.Room.LoadedCamera.Scrolls.KeptStates),
            "production Ceres entry installs compiled scroll storage");

        Console.WriteLine(
            "Room scrolls: all 323 states (200 explicit/123 implicit), 159 distinct " +
            "allocations and Ceres match without native scroll reads.");
    }

    /// <summary>Runs the complete retail room scroll audit from the command dispatcher.</summary>
    [VerificationCommand("--room-scroll-definitions")]
    internal static int RoomScrollDefinitionsCommand()
    {
        Suite(nameof(VerifyCompiledRoomScrollDefinitions), () => VerifyCompiledRoomScrollDefinitions());
        return 0;
    }
}
