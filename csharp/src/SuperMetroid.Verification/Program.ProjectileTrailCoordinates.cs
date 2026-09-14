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
            AssertEqual(RomDataReader.ReadWordFixedBank(bus, address), ProjectileTrailCoordinateDefinitions.ReadWord(guard, address), "Compiled trail read retains odd/gap/boundary and wrapped words");
        }
        AssertEqual(3828, bytes, "174 pointer words and 870 four-coordinate records cover exactly the native region");
        VerifyCoordinateSpawn(bus);
        for (int index = 0; index <= ushort.MaxValue; index++)
        foreach (ushort operand in new ushort[] { 0, 1, 2 })
        {
            ushort expected;
            try { expected = SnesCpuOperandRead.ReadAbsoluteIndexedWord(bus, 0x9b, operand, (ushort)index); }
            catch (InvalidOperationException)
            {
                AssertThrows<InvalidOperationException>(() => ProjectileTrailCoordinateDefinitions.ReadCoordinateWord(guard, operand, (ushort)index), "Unknown hardware must still fail loudly");
                continue;
            }
            AssertEqual(expected, ProjectileTrailCoordinateDefinitions.ReadCoordinateWord(guard, operand, (ushort)index), "Compiled coordinate operand retains MDR/open bus, unaligned and carry behavior");
        }
        Console.WriteLine("Trail coordinates: 174 pointer words, 870 signed records, 32768 fixed-bank words and 196608 CPU operands match native reads with the compiled region forbidden.");
    }

    private static void VerifyCoordinateSpawn(ISnesAddressSpace bus)
    {
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
            ushort directions = RomDataReader.ReadWordFixedBank(bus, family + (type & 15) * 2);
            ushort offsets = RomDataReader.ReadWordFixedBank(bus, 0x9b0000 | unchecked((ushort)(directions + direction * 2)));
            ushort y = unchecked((ushort)(offsets + frame * 4));
            byte Read(ushort operand, ushort index) => (byte)(SnesCpuOperandRead.ReadAbsoluteIndexedWord(bus, 0x9b, operand, index) >> 8);
            ushort Position(byte offset) => unchecked((ushort)(origin + (sbyte)offset - 4));
            var expected = (Position(Read(0, unchecked((ushort)(y - 1)))), Position(Read(0, y)), Position(Read(1, y)), Position(Read(2, y)));
            system.Reset();
            var shot = new SamusProjectileSlot(0) { Type = type, Direction = direction, InstructionPointer = 0xf002, XPosition = origin, YPosition = origin };
            spawn(new TrailFrameFixtureBus(new TrailCoordinateGuard(bus), frame), shot);
            var actual = system.TrailSlots[SamusProjectileSystem.TrailSlotCount - 1];
            AssertEqual(expected, (actual.Left.XPosition, actual.Left.YPosition, actual.Right.XPosition, actual.Right.YPosition), "Real trail spawn matches four native signed positions at coordinate wrap boundaries");
            AssertEqual(1, actual.Left.InstructionTimer, "Compiled coordinates retain spawn timing");
            cases++;
        }
        Console.WriteLine($"Trail spawn: {cases} beam/charged/SBA/missile direction/frame/origin cases preserve all four native positions without coordinate ROM reads.");
    }

    private sealed class TrailFrameFixtureBus(ISnesAddressSpace source, ushort frame) : ISnesAddressSpace
    {
        public byte ReadByte(int address) => address switch
        {
            0x93f000 => (byte)frame,
            0x93f001 => (byte)(frame >> 8),
            _ => source.ReadByte(address),
        };
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }

    private sealed class TrailCoordinateGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (address is >= 0x9ba4b3 and <= 0x9bb3a6)
                throw new InvalidDataException("Trail coordinate owner still reads authored ROM data.");
            return source.ReadByte(address);
        }
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
