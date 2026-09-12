using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifySamusIndexedSpeeds(SuperMetroidAddressSpace rom)
    {
        SpeedTableEntry Read(int a)
        {
            ushort Word(int offset) => (ushort)(rom.ReadByte(a + offset) | rom.ReadByte(a + offset + 1) << 8);
            return new(Word(0), Word(2), Word(4), Word(6), Word(8), Word(10));
        }
        var speed = new SamusHorizontalSpeedState();
        var guard = new IndexedSpeedReadGuard(rom);
        int compiledCount = 0;
        for (ushort medium = 0; medium < 3; medium++)
        {
            speed.SelectEnvironmentSpeedTable(medium);
            for (int movement = 0; movement <= byte.MaxValue; movement++)
            {
                int address = speed.ResolveEntryAddress((SamusMovementType)movement);
                SpeedTableEntry expected = Read(address);
                bool authored = address >= 0x909f55 && address < 0x90a32d;
                AssertEqual(authored, SamusHorizontalMotionDefinitions.TryResolveIndexed(address, out var entry), "Native authored/overread boundary");
                if (authored)
                {
                    AssertEqual(expected, entry, "All six indexed words match native data");
                    compiledCount++;
                }
                guard.ForbidReads = authored;
                AssertEqual(expected, speed.ReadEntry(guard, (SamusMovementType)movement), "Actual byte-indexed read preserves adjacent tables and overreads");
            }
        }
        AssertEqual(166, compiledCount, "Air 82 + water 56 + lava 28 authored address outcomes");
        // Every possible restored base word must preserve address wrapping and alignment.
        var baseProperty = typeof(SamusHorizontalSpeedState).GetProperty(nameof(SamusHorizontalSpeedState.ActiveSpeedTableBaseAddress))!;
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            baseProperty.SetValue(speed, (ushort)raw);
            int address = speed.ResolveEntryAddress(SamusMovementType.Running);
            AssertEqual(0x900000 | unchecked((ushort)(raw + 12)), address, "Restored base wraps as a native word");
            if (address < 0x908000) continue; // Mutable aliases are separately exercised below.
            guard.ForbidReads = address >= 0x909f25 && address < 0x90a32d && (address - 0x909f25) % 12 == 0;
            AssertEqual(Read(address), speed.ReadEntry(guard, SamusMovementType.Running), "Restored ROM base respects exact record alignment");
        }
        var mutable = new TestAddressSpace();
        baseProperty.SetValue(speed, (ushort)0x0100);
        foreach (ushort changed in new ushort[] { 0x1234, 0x4321 })
        {
            WriteTestWord(mutable, 0x90010c, changed);
            AssertEqual(changed, speed.ReadEntry(mutable, SamusMovementType.Running).Acceleration, "Restored WRAM base remains live");
        }
        Console.WriteLine("Indexed Samus speeds: all 492 authored words, 768 medium/movement reads and every restored base address preserve native data/alignment; compiled reads forbidden.");
    }

    private sealed class IndexedSpeedReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public bool ForbidReads { get; set; }
        public byte ReadByte(int address)
        {
            // Unaligned/non-catalog spans can overlap known words, so classify each
            // caller's entry in the test rather than forbidding every byte in a range.
            if (ForbidReads) throw new InvalidOperationException("Authored indexed speed unexpectedly reads the ROM.");
            return source.ReadByte(address);
        }
        public void WriteByte(int address, byte value) => throw new InvalidOperationException("Unexpected indexed-speed write.");
    }
}
