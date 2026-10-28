using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Rooms.RoomMainCodeCatalogVerification;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Rooms.Scrolling;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.World.RoomStates;

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

        var storage = new RoomScrollStorage();
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
            byte[] expected = new byte[RoomScrollGrid.StorageByteCount];
            if (unchecked((short)scrollPointer) < 0)
            {
                explicitStateCount++;
                explicitPointers.Add(scrollPointer);
                int source = RoomTilesetTableConstants.DefinitionBank | scrollPointer;
                for (int index = 0; index < expected.Length; index++)
                    expected[index] = bus.ReadCartridgeByte(source + index);
                AssertTrue(expected.SequenceEqual(
                        RoomScrollDefinitions.Get(scrollPointer).Storage.Span),
                    $"compiled explicit scroll allocation $8F:{scrollPointer:X4}");
            }
            else
            {
                implicitStateCount++;
                RoomScrollState lastRow = RoomScrollStates.FromAuthoredByte(
                    unchecked((byte)(scrollPointer + 1)),
                    $"verification implicit scroll word ${scrollPointer:X4}");
                int logicalCount = header.Size.X * header.Size.Y;
                for (int index = 0; index < expected.Length; index++)
                {
                    // $82:E88D writes only the room's cells; the rest keeps the bytes the
                    // previous grid (here, the previous audited state) left behind.
                    expected[index] = index < logicalCount
                        ? (byte)(index / header.Size.X == header.Size.Y - 1
                            ? lastRow
                            : RoomScrollState.Green)
                        : storage[index];
                }
            }

            RoomScrollGrid compiled = RoomScrollGrid.FromDefinition(
                storage,
                header.State.Scroll,
                header.Size);
            AssertTrue(expected.SequenceEqual(compiled.StorageBytes.ToArray()),
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
            () => RoomScrollGrid.LoadCompiled(new RoomScrollStorage(), [0], new Extent16<RoomScreens>(1, 1)),
            "compiled scroll installation rejects incomplete storage");

        SelectedRoom ceres = SelectedRoom.LoadUsingCompiledSelection(
            LoadStationDefinitions.Get(AreaId.Ceres, 0).Room ?? throw new InvalidDataException("Ceres station has no room."));
        var guardedCeres = CreateRetailRuntimeFixture();
        // A new session's scroll allocation starts zeroed, so Ceres's tail past its cells reads zero.
        var ceresResidue = new byte[RoomScrollGrid.StorageByteCount];
        guardedCeres.RoomLoader.InitializeStartingCeresRoom();
        var expectedCeres = new byte[RoomScrollGrid.StorageByteCount];
        int ceresLogicalCount = ceres.Size.X * ceres.Size.Y;
        RoomScrollState ceresLastRow = RoomScrollStates.FromAuthoredByte(
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
        AssertTrue(expectedCeres.SequenceEqual(guardedCeres.Room.LoadedCamera.Scrolls.StorageBytes.ToArray()),
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
