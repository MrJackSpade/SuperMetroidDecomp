using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyProjectileTrailCoordinates(ISnesAddressSpace bus)
    {
        var guard = new TrailCoordinateGuard(bus);
        int bytes = 0;
        for (int address = 0x9b8000; address <= 0x9bffff; address++)
        {
            if (ProjectileTrailCoordinateDefinitions.TryReadByte(address, out byte value))
            { AssertEqual(bus.ReadByte(address), value, "Compiled trail pointer/coordinate byte matches pinned ROM"); bytes++; }
            int next = (address & 0xff0000) | ((address + 1) & 0xffff);
            if (ProjectileTrailCoordinateDefinitions.TryReadByte(address, out _) &&
                ProjectileTrailCoordinateDefinitions.TryReadByte(next, out _))
                AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address), ProjectileTrailCoordinateDefinitions.ReadCompiledWord(address), "Compiled trail pointer word preserves authored odd and wrapped words");
            else
            {
                int rejectedAddress = address;
                AssertThrows<InvalidDataException>(() => ProjectileTrailCoordinateDefinitions.ReadCompiledWord(rejectedAddress), "Trail pointer reader rejects gaps and unrelated upper-ROM words");
            }
        }
        AssertEqual(3857, bytes, "174 pointer words, 870 four-coordinate records, 28 adjacent observations and the empty-family wrapped byte cover the reachable native region");
        Suite(nameof(VerifyCoordinateSpawn), () => VerifyCoordinateSpawn(bus));
        AssertTrue(ProjectileTrailCoordinateDefinitions.TryReadByte(0x9bffff, out byte boundaryLow),
            "bank-end trail operand has a compiled low byte");
        byte originalBoundaryHigh = ((ISnesMutableMemory)bus).ReadWorkRamByte(0x9c0000);
        try
        {
            bus.WriteByte(0x9c0000, 0x34);
            AssertEqual((ushort)(boundaryLow | 0x3400),
                ProjectileTrailCoordinateDefinitions.ReadCoordinateWord(guard, 0, 0xffff),
                "bank-end trail word combines compiled ROM byte with live next-bank WRAM");
        }
        finally
        {
            bus.WriteByte(0x9c0000, originalBoundaryHigh);
        }
        for (int index = 0; index <= ushort.MaxValue; index++)
        foreach (ushort operand in new ushort[] { 0, 1, 2 })
        {
            int address = (0x9b0000 + operand + index) & SnesCpuAddressLayout.AddressMask;
            int next = (address + 1) & SnesCpuAddressLayout.AddressMask;
            bool lowCompiled = ProjectileTrailCoordinateDefinitions.TryReadByte(address, out _);
            bool highCompiled = ProjectileTrailCoordinateDefinitions.TryReadByte(next, out _);
            bool reachesUncompiledRom =
                (!lowCompiled && SnesAddress.FromBusAddress(address).IsUpperLoRomWindow) ||
                (!highCompiled && SnesAddress.FromBusAddress(next).IsUpperLoRomWindow);
            if (reachesUncompiledRom)
            {
                // An earlier low-half access can hit strict, unimplemented hardware before
                // the CPU reaches the following ROM byte. Preserve that ordering.
                try { _ = ReadNativeTrailOperandWord(bus, operand, (ushort)index); }
                catch (InvalidOperationException)
                {
                    AssertThrows<InvalidOperationException>(() => ProjectileTrailCoordinateDefinitions.ReadCoordinateWord(guard, operand, (ushort)index), "Unknown hardware still fails before a following unrelated ROM byte");
                    continue;
                }
                AssertThrows<InvalidOperationException>(() => ProjectileTrailCoordinateDefinitions.ReadCoordinateWord(guard, operand, (ushort)index),
                    "runtime CPU boundary rejects unrelated upper-ROM data without reading the cartridge");
                continue;
            }
            ushort expected;
            try { expected = ReadNativeTrailOperandWord(bus, operand, (ushort)index); }
            catch (InvalidOperationException)
            {
                AssertThrows<InvalidOperationException>(() => ProjectileTrailCoordinateDefinitions.ReadCoordinateWord(guard, operand, (ushort)index), "Unknown hardware must still fail loudly");
                continue;
            }
            AssertEqual(expected, ProjectileTrailCoordinateDefinitions.ReadCoordinateWord(guard, operand, (ushort)index), "Compiled coordinate operand retains MDR/open bus, unaligned and carry behavior");
        }
        Console.WriteLine("Trail coordinates: 174 pointer words, 870 signed records, 29 bounded observations and 196608 CPU operands preserve compiled/live-memory behavior while rejecting unrelated upper ROM.");
    }

    private static void VerifyCoordinateSpawn(ISnesAddressSpace bus)
    {
        ReadOnlySpan<ushort> compiledFrameWordAddresses =
        [
            0x86e1, 0x8743, 0x8759, 0x8761, 0x873b, 0x8771, 0x8779, 0x8781,
            0x8789, 0x8791, 0x8799, 0x87a1, 0x87a9, 0x87b1, 0x87b9, 0x86db,
            0x9541, 0x9549, 0x9551, 0x9559, 0x9561, 0x9569,
        ];
        var system = new SamusProjectileSystem();
        var spawn = typeof(SamusProjectileSystem).GetMethod("SpawnTrail", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .CreateDelegate<Action<ISnesAddressSpace, SamusProjectileSlot>>(system);
        int cases = 0;
        foreach (ushort type in new ushort[] { 1, 2, 6, 7, 10, 11, 17, 18, 22, 23, 26, 27, 37, 0x100, 0x200 })
        for (ushort direction = 0; direction < 10; direction++)
        for (ushort frame = 0; frame < 22; frame++)
        foreach (ushort origin in new ushort[] { 0, ushort.MaxValue })
        {
            int family = (type & 0x20) != 0 ? 0x9ba4e3 : (type & 0x10) != 0 ? 0x9ba4cb : 0x9ba4b3;
            ushort directions = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), family + (type & 15) * 2);
            ushort offsets = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x9b0000 | unchecked((ushort)(directions + direction * 2)));
            ushort y = unchecked((ushort)(offsets + frame * 4));
            byte Read(ushort operand, ushort index) => (byte)(ReadNativeTrailOperandWord(bus, operand, index) >> 8);
            ushort Position(byte offset) => unchecked((ushort)(origin + (sbyte)offset - 4));
            var expected = (Position(Read(0, unchecked((ushort)(y - 1)))), Position(Read(0, y)), Position(Read(1, y)), Position(Read(2, y)));
            system.Reset();
            var shot = new SamusProjectileSlot(0)
            {
                Type = type,
                Direction = direction,
                InstructionPointer = unchecked((ushort)(compiledFrameWordAddresses[frame] + 2)),
                XPosition = origin,
                YPosition = origin,
            };
            spawn(new TrailCoordinateGuard(bus), shot);
            var actual = system.TrailSlots[SamusProjectileSystem.TrailSlotCount - 1];
            AssertEqual(expected, (actual.Left.XPosition, actual.Left.YPosition, actual.Right.XPosition, actual.Right.YPosition), "Real trail spawn matches four native signed positions at coordinate wrap boundaries");
            AssertEqual(1, actual.Left.InstructionTimer, "Compiled coordinates retain spawn timing");
            cases++;
        }
        Console.WriteLine($"Trail spawn: {cases} beam/charged/SBA/missile direction/frame/origin cases preserve all four native positions without coordinate ROM reads.");
    }

    // Import-only reference for the actual $9B absolute-indexed operand. The
    // production CPU reader intentionally rejects ROM, so native expectations
    // must resolve cartridge bytes through the asset-import boundary instead.
    private static ushort ReadNativeTrailOperandWord(
        ISnesAddressSpace bus, ushort operand, ushort index)
    {
        int address = (0x9b0000 + operand + index) & SnesCpuAddressLayout.AddressMask;
        byte ReadData(int source, byte memoryDataRegister)
        {
            int bank = source >> 16;
            int offset = source & 0xffff;
            if ((bank & 0x40) == 0 &&
                (offset >= SnesCpuOpenBusWindows.ReservedBBusStart &&
                 offset <= SnesCpuOpenBusWindows.ReservedBBusEnd ||
                 offset >= SnesCpuOpenBusWindows.UnpopulatedExpansionStart &&
                 offset <= SnesCpuOpenBusWindows.UnpopulatedExpansionEnd))
                return memoryDataRegister;

            return SnesDmaSourceMap.Classify(SnesAddress.FromBusAddress(source)) switch
            {
                SnesDmaSourceKind.Cartridge =>
                    CartridgeImportSource.Require(bus).ReadCartridgeByte(source),
                SnesDmaSourceKind.WorkRam =>
                    ((ISnesMutableMemory)bus).ReadWorkRamByte(source),
                SnesDmaSourceKind.SaveRam =>
                    ((ISnesMutableMemory)bus).ReadSaveRamByte(source),
                _ => bus is ISnesCpuPeripheralSource peripheral
                    ? peripheral.ReadPeripheralByte(source)
                    : throw new InvalidOperationException(
                        $"Native trail operand ${source:X6} addresses unmodeled hardware."),
            };
        }

        byte low = ReadData(address, (byte)(operand >> 8));
        byte high = ReadData((address + 1) & SnesCpuAddressLayout.AddressMask, low);
        return (ushort)(low | high << 8);
    }

    private sealed class TrailCoordinateGuard(ISnesAddressSpace source) : ISnesAddressSpace,
        ISnesMutableMemory, IImportCartridgeSource
    {
        public byte ReadWorkRamByte(int address) =>
            ((ISnesMutableMemory)source).ReadWorkRamByte(address);
        public byte ReadSaveRamByte(int address) =>
            ((ISnesMutableMemory)source).ReadSaveRamByte(address);
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (address is >= 0x9ba4b3 and <= 0x9bb3a6)
                throw new InvalidDataException("Trail coordinate owner still reads authored ROM data.");
            return source.ReadByte(address);
        }
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
