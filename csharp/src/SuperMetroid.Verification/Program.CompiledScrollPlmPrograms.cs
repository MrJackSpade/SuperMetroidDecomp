using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Verifies compiled retail scroll programs execute without fetching their bytes from the cartridge bus.</summary>
    /// <param name="rom">Address space that supplies room data and WRAM for the verification scenario.</param>
    private static void VerifyCompiledRoomScrollPrograms(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyScrollProgramIdentities), () => VerifyScrollProgramIdentities(rom));
        Suite(nameof(VerifyScrollProgramIndices), () => VerifyScrollProgramIndices(rom));
        Suite(nameof(VerifyScrollProgramStates), () => VerifyScrollProgramStates(rom));
        Suite(nameof(VerifyScrollProgramTermination), () => VerifyScrollProgramTermination(rom));

        // Retail population $8F:8230 contains one resident $B703 trigger at
        // (8,13). Its $8F:94FA program changes storage cell zero to green.
        // Guard every compiled retail program range during the actual room PLM
        // load/touch/handler path. Constructed-room programs are exercised by
        // VerifyRoomScrollPlms and supply decoded pairs before the handler runs.
        var guarded = new RetailScrollProgramReadGuard(rom);
        const int width = 64;
        var level = new RoomLevelData(width, 64,
            new ushort[width * 64], new byte[width * 64],
            new ushort[width * 64], new byte[0x400 * 8]);
        var plms = new RoomPlmSystem();
        AssertEqual(1, plms.LoadRoomPopulation(guarded, level,
                level.CreateBackgroundStreamer(), new SnesVram(), RoomPlmPopulationDefinition.FromCompiled(0x8230),
                new Bank80SystemState(), AreaId.Crateria,
                () => new SamusState(), () => false),
            "retail scroll-only population loads from compiled placement");
        RoomScrollGrid scrolls = RoomScrollGrid.CreateImplicit(guarded,
            widthInScreens: 4, heightInScreens: 4,
            lastRowState: RoomScrollState.RedBoundary);
        scrolls.SetStorage(0, RoomScrollState.RedBoundary);
        AssertTrue(plms.TryNotifyScrollTouch(level.GetBlockIndex(8, 13)),
            "retail scroll trigger wakes at its authored coordinate");
        plms.Step(guarded, level, level.CreateBackgroundStreamer(),
            0, 0, 0, scrolls);
        AssertEqual((byte)RoomScrollState.Green, scrolls.ReadStorage(0),
            "compiled retail scroll program mutates the intended cell");
        AssertTrue(!plms.ScrollPlmTriggered(level.GetBlockIndex(8, 13)),
            "retail scroll trigger returns to resident sleep");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "retail scroll PLM did not read any compiled program from the ROM bus");
    }

    /// <summary>Forwards memory access while rejecting cartridge reads from compiled retail scroll-program ranges.</summary>
    /// <param name="source">Underlying address space for all reads and writes outside guarded program bytes.</param>
    private sealed class RetailScrollProgramReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace, IImportCartridgeSource, ISnesMutableMemory
    {
        /// <summary>Number of attempted reads within a compiled scroll-program byte range.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes cartridge reads through the same compiled-program guard as ordinary bus reads.</summary>
        /// <param name="address">CPU address requested by the importer.</param>
        /// <returns>The byte returned by the guarded address-space read.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads of compiled scroll instructions and forwards other addresses to the source.</summary>
        /// <param name="address">CPU address to inspect and read.</param>
        /// <returns>The source byte when the address is outside every compiled program range.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to a compiled retail scroll program.</exception>
        public byte ReadByte(int address)
        {
            foreach (ushort pointer in RoomPlmScrollProgramDefinitions.Pointers)
            {
                int first = 0x8f0000 | pointer;
                if (address >= first &&
                    address < first + RoomPlmScrollProgramDefinitions.Get(pointer).Length)
                {
                    ForbiddenReadAttempts++;
                    throw new InvalidOperationException(
                        $"Retail scroll PLM reread compiled program byte ${address:X6}.");
                }
            }
            return source.ReadByte(address);
        }

        /// <summary>Forwards writes unchanged because the guard restricts reads from compiled program ranges.</summary>
        /// <param name="address">CPU address to write.</param>
        /// <param name="value">Byte stored at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);

        // Room scroll storage keeps WRAM bytes across rooms; WRAM is not guarded cartridge data.
        /// <summary>Forwards a work-RAM read; scroll storage is outside the cartridge-program guard.</summary>
        /// <param name="cpuAddress">Work-RAM address requested by the caller.</param>
        /// <returns>The byte stored at that work-RAM address.</returns>
        public byte ReadWorkRamByte(int cpuAddress) => ((ISnesMutableMemory)source).ReadWorkRamByte(cpuAddress);

        /// <summary>Forwards a save-RAM read to the underlying mutable memory.</summary>
        /// <param name="cpuAddress">Save-RAM address requested by the caller.</param>
        /// <returns>The byte stored at that save-RAM address.</returns>
        public byte ReadSaveRamByte(int cpuAddress) => ((ISnesMutableMemory)source).ReadSaveRamByte(cpuAddress);
    }
}
