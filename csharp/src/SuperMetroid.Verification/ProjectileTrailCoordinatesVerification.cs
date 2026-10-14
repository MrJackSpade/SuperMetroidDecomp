using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using static SuiteTimingVerification;

/// <summary>Checks compiled projectile-trail coordinates and CPU operand routing against native data and live-memory aliases.</summary>
internal static class ProjectileTrailCoordinatesVerification
{
    /// <summary>Compares the bounded bank-$9B coordinate catalog and every indexed operand with the native CPU read behavior.</summary>
    internal static void VerifyProjectileTrailCoordinates(ISnesAddressSpace bus)
    {
        var guard = NewTrailCoordinateGuard(bus);
        int bytes = 0;
        for (int address = 0x9b8000; address <= 0x9bffff; address++)
        {
            if (ProjectileTrailCoordinateDefinitions.TryReadByte(address, out byte value))
            { AssertEqual(ReadRetailTrailOperandByte(bus, address), value, "Compiled trail pointer/coordinate byte matches pinned ROM"); bytes++; }
            int next = SnesAddressMath.AddWithinBank(address, 1);
            if (ProjectileTrailCoordinateDefinitions.TryReadByte(address, out _) &&
                ProjectileTrailCoordinateDefinitions.TryReadByte(next, out _))
                AssertEqual(ReadRetailTrailWord(bus, address), ProjectileTrailCoordinateDefinitions.ReadCompiledWord(address), "Compiled trail pointer word preserves authored odd and wrapped words");
            else
            {
                int rejectedAddress = address;
                AssertThrows<InvalidDataException>(() => ProjectileTrailCoordinateDefinitions.ReadCompiledWord(rejectedAddress), "Trail pointer reader rejects gaps and unrelated upper-ROM words");
            }
        }
        AssertEqual(4342, bytes, "174 pointer words, 870 four-coordinate records, 28 adjacent observations, 485 further reflected-list-start bytes and the empty-family wrapped byte cover the reachable native region");
        Suite(nameof(VerifyCoordinateSpawn), () => VerifyCoordinateSpawn(bus));
        VerifyTrailBankEndWordWrapsIntoWorkRamMirror(bus, guard);
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

    /// <summary>Checks actual trail spawn positions for beam, missile, and reflected list-start cases at coordinate wraps.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for trail family, direction and reflected list-start pointer tables")]
    private static void VerifyCoordinateSpawn(ISnesAddressSpace bus)
    {
        ReadOnlySpan<ushort> compiledFrameWordAddresses =
        [
            0x86e1, 0x8743, 0x8759, 0x8761, 0x873b, 0x8771, 0x8779, 0x8781,
            0x8789, 0x8791, 0x8799, 0x87a1, 0x87a9, 0x87b1, 0x87b9, 0x86db,
            0x9541, 0x9549, 0x9551, 0x9559, 0x9561, 0x9569,
        ];
        var system = new SamusProjectileSystem();
        var spawn = new Action<ISnesAddressSpace, SamusProjectileSlot>(system.Trails.SpawnTrail);
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
            byte Read(ushort operand, ushort index) => SnesLittleEndian.HighByte(ReadNativeTrailOperandWord(bus, operand, index));
            ushort Position(byte offset) => unchecked((ushort)(origin + (sbyte)offset - 4));
            var expected = (Position(Read(0, unchecked((ushort)(y - 1)))), Position(Read(0, y)), Position(Read(1, y)), Position(Read(2, y)));
            system.Reset();
            var shot = new SamusProjectileSlot(0)
            {
                Type = type,
                Direction = direction,
                InstructionPointer = unchecked((ushort)(compiledFrameWordAddresses[frame] + 2)),
                Position = new(new(origin, 0), new(origin, 0)),
            };
            spawn(NewTrailCoordinateGuard(bus), shot);
            var actual = system.TrailSlots[SamusProjectileSystem.TrailSlotCount - 1];
            AssertEqual(expected, (actual.Left.Position.X, actual.Left.Position.Y, actual.Right.Position.X, actual.Right.Position.Y), "Real trail spawn matches four native signed positions at coordinate wrap boundaries");
            AssertEqual(1, actual.Left.InstructionTimer.RawValue, "Compiled coordinates retain spawn timing");
            cases++;
        }
        // $90:BE17 reflects a projectile onto the start of its new direction's list. A trail
        // spawned before that list's first record reads the preceding word as its frame; the
        // 13% movie's reflected charged beam reads Grapple swing data at $9B:C1FB this way.
        foreach ((ushort type, int dataTable, int index) in ReflectableTrailTypes())
        for (ushort direction = 0; direction < 10; direction++)
        {
            ushort data = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), dataTable + index * 2);
            ushort listStart = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x930000 | unchecked((ushort)(data + 2 + direction * 2)));
            ushort frame = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x930000 | unchecked((ushort)(listStart - 2)));
            int family = (type & 0x20) != 0 ? 0x9ba4e3 : (type & 0x10) != 0 ? 0x9ba4cb : 0x9ba4b3;
            ushort directions = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), family + (type & 15) * 2);
            // An empty SBA family indexes bank-$9B WRAM; that live read is covered above.
            if (directions < 0x8000) continue;
            ushort offsets = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x9b0000 | unchecked((ushort)(directions + direction * 2)));
            ushort y = unchecked((ushort)(offsets + frame * 4));
            byte Read(ushort operand, ushort index) => SnesLittleEndian.HighByte(ReadNativeTrailOperandWord(bus, operand, index));
            ushort Position(byte offset) => unchecked((ushort)(0x0100 + (sbyte)offset - 4));
            system.Reset();
            var shot = new SamusProjectileSlot(0)
            {
                Type = type, Direction = direction, InstructionPointer = listStart, Position = new(new(0x0100, 0), new(0x0100, 0)),
            };
            (ushort, ushort, ushort, ushort) expected;
            try { expected = (Position(Read(0, unchecked((ushort)(y - 1)))), Position(Read(0, y)), Position(Read(1, y)), Position(Read(2, y))); }
            catch (InvalidOperationException)
            {
                // Hardware the native reference does not model must fail in production too.
                AssertThrows<InvalidOperationException>(() => spawn(NewTrailCoordinateGuard(bus), shot),
                    $"reflected type ${type:X4} direction {direction} trail fails on unmodeled hardware");
                cases++;
                continue;
            }
            spawn(NewTrailCoordinateGuard(bus), shot);
            var actual = system.TrailSlots[SamusProjectileSystem.TrailSlotCount - 1];
            AssertEqual(expected, (actual.Left.Position.X, actual.Left.Position.Y, actual.Right.Position.X, actual.Right.Position.Y),
                $"reflected type ${type:X4} direction {direction} trail reads the word before its list start");
            cases++;
        }
        Console.WriteLine($"Trail spawn: {cases} beam/charged/SBA/missile direction/frame/origin and reflected list-start cases preserve all four native positions without coordinate ROM reads.");
    }

    /// <summary>Every projectile type $90:BE17 can reflect, with its bank-$93 data table and index.</summary>
    private static IEnumerable<(ushort Type, int DataTable, int Index)> ReflectableTrailTypes()
    {
        for (ushort combination = 0; combination < 12; combination++)
        {
            yield return (combination, 0x9383c1, combination);
            yield return ((ushort)(combination | 0x10), 0x9383d9, combination);
            yield return ((ushort)(combination | 0x30), 0x9383d9, combination);
        }
        yield return (0x0100, 0x9383f1, 1);
        yield return (0x0200, 0x9383f1, 2);
    }

    /// <summary>Reads a native trail pointer or coordinate word for the cartridge-side oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for compiled trail pointer and coordinate words")]
    private static ushort ReadRetailTrailWord(ISnesAddressSpace bus, int address) =>
        RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);

    /// <summary>Reads a cartridge byte for a native bank-$9B trail operand.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for a $9B trail operand byte the native CPU reads from the cartridge")]
    private static byte ReadRetailTrailOperandByte(ISnesAddressSpace bus, int address) =>
        CartridgeImportSource.Require(bus).ReadCartridgeByte(address);

    /// <summary>Reads a modeled peripheral response or rejects an unmodeled hardware operand.</summary>
    [CpuBusBoundary("exact bus test: device-response oracle for a $9B trail operand outside memory and the cartridge")]
    private static byte ReadNativeTrailOperandDeviceByte(ISnesAddressSpace bus, int address) =>
        bus is ISnesCpuPeripheralSource peripheral
            ? peripheral.ReadPeripheralByte(address)
            : throw new InvalidOperationException(
                $"Native trail operand ${address:X6} addresses unmodeled hardware.");

    /// <summary>
    /// The bank-end trail operand's high byte is the following bank's first byte, $9C:0000, a
    /// low-window mirror of WRAM: the word combines the compiled ROM byte with live WRAM.
    /// </summary>
    [CpuBusBoundary("exact bus test: bank-end trail operand word wraps into the next bank's low-window WRAM mirror")]
    private static void VerifyTrailBankEndWordWrapsIntoWorkRamMirror(ISnesAddressSpace bus, ISnesAddressSpace guard)
    {
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
    }

    /// <summary>Resolves the native $9B absolute-indexed word through ROM, WRAM, SRAM, open bus, or devices in CPU read order.</summary>
    [CpuBusBoundary("exact bus test: reference CPU read of the $9B absolute-indexed trail operand across ROM, WRAM, SRAM, open bus and devices")]
    private static ushort ReadNativeTrailOperandWord(
        ISnesAddressSpace bus, ushort operand, ushort index)
    {
        int address = (0x9b0000 + operand + index) & SnesCpuAddressLayout.AddressMask;
        byte ReadData(int source, byte memoryDataRegister)
        {
            int bank = SnesAddressMath.BankNumber(source);
            int offset = SnesAddressMath.OffsetInBank(source);
            if ((bank & 0x40) == 0 &&
                (offset >= SnesCpuOpenBusWindows.ReservedBBusStart &&
                 offset <= SnesCpuOpenBusWindows.ReservedBBusEnd ||
                 offset >= SnesCpuOpenBusWindows.UnpopulatedExpansionStart &&
                 offset <= SnesCpuOpenBusWindows.UnpopulatedExpansionEnd))
                return memoryDataRegister;

            return SnesDmaSourceMap.Classify(SnesAddress.FromBusAddress(source)) switch
            {
                SnesDmaSourceKind.Cartridge => ReadRetailTrailOperandByte(bus, source),
                SnesDmaSourceKind.WorkRam =>
                    ((ISnesMutableMemory)bus).ReadWorkRamByte(source),
                SnesDmaSourceKind.SaveRam =>
                    ((ISnesMutableMemory)bus).ReadSaveRamByte(source),
                _ => ReadNativeTrailOperandDeviceByte(bus, source),
            };
        }

        byte low = ReadData(address, SnesLittleEndian.HighByte(operand));
        byte high = ReadData((address + 1) & SnesCpuAddressLayout.AddressMask, low);
        return (ushort)(SnesLittleEndian.Word(low, high));
    }

    /// <summary>Forbids runtime reads of the migrated bank-$9B trail coordinate region.</summary>
    private static MutableCartridgeReadGuard NewTrailCoordinateGuard(ISnesAddressSpace source)
    {
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(address => address is >= 0x9ba4b3 and <= 0x9bb3a6,
            _ => new InvalidDataException("Trail coordinate owner still reads authored ROM data."));
        return guard;
    }

    /// <summary>Runs the trail-coordinate CPU operand comparison against the local retail cartridge.</summary>
    [VerificationCommand("--projectile-trail-coordinates")]
    internal static int ProjectileTrailCoordinatesCommand()
    {
        Suite(nameof(VerifyProjectileTrailCoordinates), () => VerifyProjectileTrailCoordinates(SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
