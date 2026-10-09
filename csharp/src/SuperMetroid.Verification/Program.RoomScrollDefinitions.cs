using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>Audits compiled explicit and implicit room-scroll grids against retail room states,
    /// and verifies production Ceres initialization installs the expected grid without native scroll reads.</summary>
    private static void VerifyCompiledRoomScrollDefinitions()
    {
        string symbolPath = Path.GetFullPath(
            Path.Combine("upstream-sm", "assets", "names.txt"));
        SuperMetroidAddressSpace bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
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
                    // $82:E88D writes only the room's cells; the rest keeps the WRAM the
                    // previous grid (here, the previous audited state) left behind.
                    expected[index] = index < logicalCount
                        ? (byte)(index / header.WidthInScreens == header.HeightInScreens - 1
                            ? lastRow
                            : RoomScrollState.Green)
                        : bus.ReadWorkRamByte(RoomScrollGrid.WorkRamAddress + index);
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
            ceresPointer);
        var guardedCeres = CreateRetailRuntimeFixture(new RoomScrollSourceReadGuard(
            bus, ceres.State.ScrollPointer));
        var ceresResidue = new byte[RoomScrollGrid.StorageByteCount];
        for (int index = 0; index < ceresResidue.Length; index++)
            ceresResidue[index] = bus.ReadWorkRamByte(RoomScrollGrid.WorkRamAddress + index);
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
                : ceresResidue[index];
        }
        AssertTrue(expectedCeres.AsSpan().SequenceEqual(guardedCeres.Camera!.Scrolls.Storage),
            "production Ceres entry installs compiled scroll storage");

        Console.WriteLine(
            "Room scrolls: all 323 states (200 explicit/123 implicit), 159 distinct " +
            "allocations and Ceres match without native scroll reads.");
    }

    /// <summary>Address-space adapter that forbids cartridge and save reads during compiled grid creation,
    /// while preserving reads of the prior room's scroll buffer in WRAM.</summary>
    /// <param name="source">Underlying memory used only for the preserved scroll-buffer reads and writes.</param>
    private sealed class RoomScrollNoReadAddressSpace(ISnesAddressSpace source)
        : ISnesAddressSpace, IImportCartridgeSource, ISnesMutableMemory
    {
        /// <summary>Routes cartridge access to the rejecting read path.</summary>
        /// <param name="address">Cartridge address requested by the caller.</param>
        /// <exception cref="InvalidOperationException">Cartridge reads are forbidden during compiled grid creation.</exception>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        // The scroll buffer itself is WRAM the previous room left; implicit grids keep it.
        /// <summary>Reads retained scroll-buffer WRAM and rejects other work-RAM reads.</summary>
        /// <param name="address">Work-RAM address requested by the caller.</param>
        /// <returns>The prior buffer byte when the address belongs to the scroll grid.</returns>
        public byte ReadWorkRamByte(int address) =>
            address >= RoomScrollGrid.WorkRamAddress &&
            address < RoomScrollGrid.WorkRamAddress + RoomScrollGrid.StorageByteCount
                ? ((ISnesMutableMemory)source).ReadWorkRamByte(address)
                : ReadByte(address);
        /// <summary>Routes save-RAM access to the rejecting read path.</summary>
        /// <param name="address">Save-RAM address requested by the caller.</param>
        /// <exception cref="InvalidOperationException">Save-RAM reads are forbidden during compiled grid creation.</exception>
        public byte ReadSaveRamByte(int address) => ReadByte(address);

        /// <summary>Rejects any read not handled by the explicit prior-buffer WRAM exception.</summary>
        /// <param name="address">Address whose access would require a native or unrelated memory read.</param>
        /// <exception cref="InvalidOperationException">A forbidden address-space read was attempted.</exception>
        public static byte ReadByte(int address) => throw new InvalidOperationException(
            $"Compiled room-scroll construction read address ${address:X6}.");

        /// <summary>Forwards memory writes to the wrapped mutable address space.</summary>
        /// <param name="address">Address receiving the write.</param>
        /// <param name="value">Byte stored at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }

    /// <summary>Address-space guard that rejects reads from the selected native scroll allocation.</summary>
    /// <param name="source">Underlying memory for reads outside the guarded range and for all writes.</param>
    /// <param name="scrollPointer">Native room-scroll pointer whose ROM allocation must not be read.</param>
    private sealed class RoomScrollSourceReadGuard(
        ISnesAddressSpace source,
        ushort scrollPointer) : ISnesAddressSpace, IImportCartridgeSource, ISnesMutableMemory
    {
        /// <summary>Routes importer cartridge access through the selected-scroll read guard.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The wrapped byte when the address is outside the selected scroll allocation.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Forwards work-RAM reads without applying the cartridge allocation guard.</summary>
        /// <param name="address">Work-RAM address requested by the caller.</param>
        /// <returns>The byte stored at that work-RAM address.</returns>
        public byte ReadWorkRamByte(int address) =>
            ((ISnesMutableMemory)source).ReadWorkRamByte(address);

        /// <summary>Forwards save-RAM reads without applying the cartridge allocation guard.</summary>
        /// <param name="address">Save-RAM address requested by the caller.</param>
        /// <returns>The byte stored at that save-RAM address.</returns>
        public byte ReadSaveRamByte(int address) =>
            ((ISnesMutableMemory)source).ReadSaveRamByte(address);

        /// <summary>First cartridge address of the allocation selected by the room-scroll pointer.</summary>
        private readonly int _start = RoomAssetRomData.Tilesets.DefinitionBank | scrollPointer;

        /// <summary>Rejects reads within the selected native scroll allocation and forwards other cartridge reads.</summary>
        /// <param name="address">Cartridge address requested by the runtime.</param>
        /// <returns>The wrapped byte when the address is outside the guarded scroll allocation.</returns>
        /// <exception cref="InvalidOperationException">The address lies within the guarded scroll allocation.</exception>
        public byte ReadByte(int address)
        {
            if (address >= _start && address < _start + RoomScrollGrid.StorageByteCount)
            {
                throw new InvalidOperationException(
                    $"Production runtime read native room-scroll data at ${address:X6}.");
            }

            return source.ReadByte(address);
        }

        /// <summary>Forwards writes to the wrapped mutable address space without applying the read guard.</summary>
        /// <param name="address">Address receiving the write.</param>
        /// <param name="value">Byte stored at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
