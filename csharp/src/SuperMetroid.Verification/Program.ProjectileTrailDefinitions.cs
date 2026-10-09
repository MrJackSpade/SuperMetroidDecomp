using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    /// <summary>Checks compiled trail selectors against native words and verifies every real spawn preserves indexed selection behavior.</summary>
    /// <param name="bus">Cartridge address space providing native selector words for comparison.</param>
    private static void VerifyProjectileTrailDefinitions(ISnesAddressSpace bus)
    {
        Suite(nameof(VerifyProjectileTrailCoordinates), () => VerifyProjectileTrailCoordinates(bus));
        int start = SamusProjectileRomData.Trails.LeftInstructionPointers;
        int end = SamusProjectileRomData.Trails.RightInstructionPointers + 64 * sizeof(ushort);
        for (int address = start; address < end; address += sizeof(ushort))
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address), ProjectileTrailDefinitions.ReadSelector(address), "Trail selectors retain every reachable aligned native word");
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
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), start + selection * 2), trail.Left.InstructionPointer, "Real spawn selects left trail including adjacent right-table entries");
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), SamusProjectileRomData.Trails.RightInstructionPointers + selection * 2), trail.Right.InstructionPointer, "Real spawn retains right-table overrun behavior");
            AssertEqual(1, trail.Left.InstructionTimer, "Selector extraction leaves allocation timer unchanged");
            AssertEqual(96, trail.Left.XPosition, "Selector extraction leaves origin offset unchanged");
        }
        Console.WriteLine("Trail selectors: 103 reachable native words and 64 real spawn selections pass with the complete selector window forbidden.");
    }

    /// <summary>Memory fixture that forbids trail-selector ROM reads while supplying deterministic values for unrelated memory.</summary>
    /// <param name="start">Inclusive beginning of the native trail-selector window that must not be read.</param>
    /// <param name="end">Exclusive end of the guarded selector window.</param>
    private sealed class TrailSelectorGuard(int start, int end) :
        ISnesAddressSpace, IImportCartridgeSource, ISnesMutableMemory
    {
        /// <summary>Routes cartridge-import reads through the guarded byte-address implementation.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>Zero for unrelated visual data; selector-window reads are rejected.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        // A wrapped trail-coordinate pointer may address the low WRAM mirror.
        // The constructed fixture leaves that memory zero-filled, as the old
        // generic read guard did for all unrelated visual offsets.
        /// <summary>Returns the fixture's zero-filled low-WRAM mirror used by wrapped trail-coordinate pointers.</summary>
        /// <param name="address">Work-RAM address requested by the projectile code.</param>
        /// <returns>The fixture's default zero byte.</returns>
        public byte ReadWorkRamByte(int address) => 0;

        /// <summary>Rejects SRAM access because the selector fixture does not model save memory.</summary>
        /// <param name="address">Save-RAM address requested by the projectile code.</param>
        /// <returns>This method never returns.</returns>
        /// <exception cref="InvalidOperationException">Any save-memory read is attempted.</exception>
        public byte ReadSaveRamByte(int address) =>
            throw new InvalidOperationException("Trail selector fixture cannot read SRAM.");

        /// <summary>Rejects reads of the native selector window and zero-fills other cartridge addresses.</summary>
        /// <param name="address">Cartridge address requested by the simulated projectile code.</param>
        /// <returns>Zero for addresses outside the guarded selector window.</returns>
        /// <exception cref="InvalidDataException">A trail-selector ROM byte is read.</exception>
        public byte ReadByte(int address)
        {
            if (address >= start && address < end) throw new InvalidDataException("Trail spawn still reads authored selector ROM.");
            // Unrelated visual offsets are zero-filled so all low-six-bit combinations
            // have deterministic origins without weakening the selector guard.
            return (byte)0;
        }
        /// <summary>Rejects writes because the selector fixture treats cartridge memory as immutable.</summary>
        /// <param name="address">Cartridge address the code attempted to modify.</param>
        /// <param name="value">Byte the code attempted to store.</param>
        /// <exception cref="InvalidOperationException">Any cartridge write is attempted.</exception>
        public void WriteByte(int address, byte value) => throw new InvalidOperationException("Trail spawn wrote ROM.");
    }
}
