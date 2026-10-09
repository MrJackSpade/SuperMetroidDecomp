using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Checks all sixteen statue fragments against native tables and production projectile allocation with those tables guarded.</summary>
    /// <param name="rom">Retail address space supplying the native fragment tables used as the reference.</param>
    private static void VerifyBombTorizoStatueFragmentDefinitions(
        SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyStatueFragmentProgramField), () => VerifyStatueFragmentProgramField(rom));
        Suite(nameof(VerifyStatueFragmentXField), () => VerifyStatueFragmentXField(rom));
        Suite(nameof(VerifyStatueFragmentYField), () => VerifyStatueFragmentYField(rom));
        Suite(nameof(VerifyStatueFragmentVelocityField), () => VerifyStatueFragmentVelocityField(rom));
        Suite(nameof(VerifyStatueFragmentAccelerationField), () => VerifyStatueFragmentAccelerationField(rom));
        BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (ushort parameter = 1; parameter < 32; parameter += 2)
            AssertThrows<ArgumentOutOfRangeException>(() => BombTorizoStatueFragmentDefinitions.ForParameter(parameter), "statue rejects every odd parameter in the native range");
        foreach (ushort parameter in new ushort[] {32,33,0x100,0x101,0x7ffe,0x8000,0xfffe,0xffff})
            AssertThrows<ArgumentOutOfRangeException>(() => BombTorizoStatueFragmentDefinitions.ForParameter(parameter), "statue rejects parameters beyond the sixteen fragments");
        var guarded = new BombTorizoStatueFragmentReadGuard(rom);
        for (ushort index = 0; index < 16; index++)
        {
            ushort parameter = unchecked((ushort)(index * 2));
            BombTorizoStatueFragmentDefinition expected =
                BombTorizoStatueFragmentDefinitions.ForParameter(parameter);
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!
                .SetValue(enemies, guarded);
            RoomEnemyProjectileSlot projectile =
                enemies.SpawnBombTorizoStatueBreakingProjectile(
                    new BombTorizoStatueProjectileRequest(
                        BombTorizoStatueFragmentDefinitions.ProjectileDefinition,
                        parameter,
                        PlmBlockX: 10,
                        PlmBlockY: 20)) ??
                throw new InvalidDataException(
                    $"Bomb Torizo statue fragment {index} failed to allocate.");

            AssertEqual(RoomEnemyProjectileKind.BombTorizoStatueBreaking,
                projectile.Kind,
                $"Bomb Torizo statue fragment {index} projectile kind");
            AssertEqual(expected.InstructionList, projectile.InstructionPointer,
                $"Bomb Torizo statue fragment {index} production instruction");
            AssertEqual(unchecked((ushort)(10 * 16 + expected.XOffset)),
                projectile.XPosition,
                $"Bomb Torizo statue fragment {index} production X");
            AssertEqual(unchecked((ushort)(20 * 16 + expected.YOffset)),
                projectile.YPosition,
                $"Bomb Torizo statue fragment {index} production Y");
            AssertEqual(expected.YVelocity, projectile.YVelocity,
                $"Bomb Torizo statue fragment {index} production velocity");
            AssertEqual(expected.Acceleration, projectile.Variable1,
                $"Bomb Torizo statue fragment {index} production acceleration");
        }

        Console.WriteLine(
            "Bomb Torizo statue fragments: 56 native source words and all sixteen real room-graphics projectile allocations pass with the five physical tables forbidden.");
    }

    /// <summary>Reads one little-endian word from the native statue-fragment table.</summary>
    /// <param name="rom">Address space containing the reference table bytes.</param>
    /// <param name="address">Absolute address of the word's low byte.</param>
    /// <returns>The two adjacent bytes combined into a 16-bit value.</returns>
    private static ushort ReadStatueFragmentOracleWord(ISnesAddressSpace rom, int address) =>
        (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);

    /// <summary>Compares each valid even selector's instruction pointer with the native program table.</summary>
    /// <param name="rom">Address space supplying the native instruction words.</param>
    private static void VerifyStatueFragmentProgramField(ISnesAddressSpace rom)
    {
        for (ushort parameter = 0; parameter < 32; parameter += 2)
            AssertEqual(ReadStatueFragmentOracleWord(rom, 0x86a7ab + parameter),
                BombTorizoStatueFragmentDefinitions.ForParameter(parameter).InstructionList, "statue native program field");
    }
    /// <summary>Checks each fragment's signed horizontal offset, including the mirrored half of the table.</summary>
    /// <param name="rom">Address space supplying the native signed X offsets.</param>
    private static void VerifyStatueFragmentXField(ISnesAddressSpace rom)
    {
        for (ushort parameter = 0; parameter < 32; parameter += 2)
            AssertEqual(unchecked((short)ReadStatueFragmentOracleWord(rom, 0x86a7cb + parameter)),
                BombTorizoStatueFragmentDefinitions.ForParameter(parameter).XOffset, "statue native signed X field including mirrored set");
    }
    /// <summary>Checks signed vertical offsets using the native eight-row table wrap.</summary>
    /// <param name="rom">Address space supplying the native signed Y offsets.</param>
    private static void VerifyStatueFragmentYField(ISnesAddressSpace rom)
    {
        for (ushort parameter = 0; parameter < 32; parameter += 2)
            AssertEqual(unchecked((short)ReadStatueFragmentOracleWord(rom, 0x86a7eb + (parameter & 15))),
                BombTorizoStatueFragmentDefinitions.ForParameter(parameter).YOffset, "statue native signed Y field with eight-row wrap");
    }
    /// <summary>Checks the constant vertical launch velocity assigned to each fragment.</summary>
    /// <param name="rom">Address space supplying the native velocity words.</param>
    private static void VerifyStatueFragmentVelocityField(ISnesAddressSpace rom)
    {
        for (ushort parameter = 0; parameter < 32; parameter += 2)
            AssertEqual(ReadStatueFragmentOracleWord(rom, 0x86a7fb + (parameter & 15)),
                BombTorizoStatueFragmentDefinitions.ForParameter(parameter).YVelocity, "statue native constant launch velocity field");
    }
    /// <summary>Checks the constant vertical acceleration assigned to each fragment.</summary>
    /// <param name="rom">Address space supplying the native acceleration words.</param>
    private static void VerifyStatueFragmentAccelerationField(ISnesAddressSpace rom)
    {
        for (ushort parameter = 0; parameter < 32; parameter += 2)
            AssertEqual(ReadStatueFragmentOracleWord(rom, 0x86a80b + (parameter & 15)),
                BombTorizoStatueFragmentDefinitions.ForParameter(parameter).Acceleration, "statue native constant acceleration field");
    }
    /// <summary>Prevents production fragment spawning from rereading the compiled statue-fragment table range.</summary>
    /// <param name="source">Underlying address space used for reads outside the migrated tables and for writes.</param>
    private sealed class BombTorizoStatueFragmentReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes importer reads through the same migrated-table check.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The underlying byte when the guarded read is allowed.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads from the migrated fragment tables and forwards other addresses.</summary>
        /// <param name="address">Absolute cartridge address to read.</param>
        /// <returns>The underlying source byte for an allowed address.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to the migrated statue-fragment tables.</exception>
        public byte ReadByte(int address) =>
            address is >= 0x86a7ab and < 0x86a81b
                ? throw new InvalidOperationException(
                    $"Bomb Torizo statue fragment attempted migrated table read ${address:X6}.")
                : source.ReadByte(address);

        /// <summary>Forwards a cartridge write to the wrapped address space.</summary>
        /// <param name="address">Absolute cartridge address to update.</param>
        /// <param name="value">Byte written to that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
