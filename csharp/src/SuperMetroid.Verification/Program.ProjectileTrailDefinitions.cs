using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyProjectileTrailDefinitions(ISnesAddressSpace bus)
    {
        VerifyProjectileTrailCoordinates(bus);
        int start = SamusProjectileRomData.Trails.LeftInstructionPointers;
        int end = SamusProjectileRomData.Trails.RightInstructionPointers + 64 * sizeof(ushort);
        for (int address = start; address < end; address += sizeof(ushort))
            AssertEqual(RomDataReader.ReadWordFixedBank(bus, address), ProjectileTrailDefinitions.ReadSelector(address), "Trail selectors retain every reachable aligned native word");
        AssertThrows<InvalidDataException>(() => ProjectileTrailDefinitions.ReadSelector(start - 2), "Trail selector rejects the preceding word");
        AssertThrows<InvalidDataException>(() => ProjectileTrailDefinitions.ReadSelector(start + 1), "Trail selector rejects unaligned reads");
        AssertThrows<InvalidDataException>(() => ProjectileTrailDefinitions.ReadSelector(end), "Trail selector rejects the following word");
        var spawn = typeof(SamusProjectileSystem).GetMethod("SpawnTrail", BindingFlags.NonPublic | BindingFlags.Instance)!;
        for (int selection = 0; selection < 64; selection++)
        {
            var projectiles = new SamusProjectileSystem();
            var projectile = new SamusProjectileSlot(0)
            {
                Type = (ushort)selection,
                InstructionPointer = 0x86e3,
                XPosition = 100,
                YPosition = 200,
            };
            spawn.Invoke(projectiles, [new TrailSelectorGuard(start, end), projectile]);
            var trail = projectiles.TrailSlots[SamusProjectileSystem.TrailSlotCount - 1];
            AssertEqual(RomDataReader.ReadWordFixedBank(bus, start + selection * 2), trail.Left.InstructionPointer, "Real spawn selects left trail including adjacent right-table entries");
            AssertEqual(RomDataReader.ReadWordFixedBank(bus, SamusProjectileRomData.Trails.RightInstructionPointers + selection * 2), trail.Right.InstructionPointer, "Real spawn retains right-table overrun behavior");
            AssertEqual(1, trail.Left.InstructionTimer, "Selector extraction leaves allocation timer unchanged");
            AssertEqual(96, trail.Left.XPosition, "Selector extraction leaves origin offset unchanged");
        }
        Console.WriteLine("Trail selectors: 103 reachable native words and 64 real spawn selections pass with the complete selector window forbidden.");
    }

    private sealed class TrailSelectorGuard(int start, int end) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (address >= start && address < end) throw new InvalidDataException("Trail spawn still reads authored selector ROM.");
            // Unrelated visual offsets are zero-filled so all low-six-bit combinations
            // have deterministic origins without weakening the selector guard.
            return (byte)0;
        }
        public void WriteByte(int address, byte value) => throw new InvalidOperationException("Trail spawn wrote ROM.");
    }
}
