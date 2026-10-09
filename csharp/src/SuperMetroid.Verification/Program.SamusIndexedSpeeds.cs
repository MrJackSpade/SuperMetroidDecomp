using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks compiled Samus speed records, restored table bases, mutable aliases, and rejection of ROM overreads.</summary>
    /// <param name="rom">The retail address space used to compare authored indexed speed words.</param>
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
                    guard.ForbidReads = true;
                    AssertEqual(expected, speed.ReadEntry(guard, (SamusMovementType)movement), "Actual byte-indexed read preserves adjacent authored tables");
                }
                else
                {
                    AssertThrows<InvalidDataException>(
                        () => speed.ReadEntry(guard, (SamusMovementType)movement),
                        "Byte-indexed high-bank overread rejected");
                }
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
            bool compiled = address >= 0x909f25 && address < 0x90a32d && (address - 0x909f25) % 12 == 0;
            if (compiled)
            {
                guard.ForbidReads = true;
                AssertEqual(Read(address), speed.ReadEntry(guard, SamusMovementType.Running), "Restored authored ROM base respects exact record alignment");
            }
            else
            {
                AssertThrows<InvalidDataException>(
                    () => speed.ReadEntry(guard, SamusMovementType.Running),
                    "Restored non-catalog ROM base rejected");
            }
        }
        var mutable = new TestAddressSpace();
        baseProperty.SetValue(speed, (ushort)0x0100);
        foreach (ushort changed in new ushort[] { 0x1234, 0x4321 })
        {
            WriteTestWord(mutable, 0x90010c, changed);
            AssertEqual(changed, speed.ReadEntry(mutable, SamusMovementType.Running).Acceleration, "Restored WRAM base remains live");
        }
        Console.WriteLine("Indexed Samus speeds: all 492 authored words and every restored base address preserve authored data/alignment; compiled reads are forbidden, mutable aliases stay live, and ROM overreads fail loudly.");
    }

    /// <summary>Can be armed to reject any cartridge read while verifying that an authored speed record comes from compiled data.</summary>
    /// <param name="source">The underlying address space used while reads are permitted.</param>
    private sealed class IndexedSpeedReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>When enabled, causes each attempted cartridge read to fail immediately.</summary>
        public bool ForbidReads { get; set; }

        /// <summary>Routes a cartridge-import read through the guard's current read policy.</summary>
        /// <param name="address">The bus address requested by the caller.</param>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads while armed and otherwise delegates to the wrapped address space.</summary>
        /// <param name="address">The bus address to read.</param>
        /// <returns>The wrapped byte when reads are permitted.</returns>
        public byte ReadByte(int address)
        {
            // Unaligned/non-catalog spans can overlap known words, so classify each
            // caller's entry in the test rather than forbidding every byte in a range.
            if (ForbidReads) throw new InvalidOperationException("Authored indexed speed unexpectedly reads the ROM.");
            return source.ReadByte(address);
        }

        /// <summary>Rejects writes because this guard is used only to observe read behavior.</summary>
        /// <param name="address">The bus address a caller attempted to modify.</param>
        /// <param name="value">The byte a caller attempted to store.</param>
        /// <exception cref="InvalidOperationException">This verification guard does not permit writes.</exception>
        public void WriteByte(int address, byte value) => throw new InvalidOperationException("Unexpected indexed-speed write.");
    }
}
