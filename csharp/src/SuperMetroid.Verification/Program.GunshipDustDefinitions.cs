using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Verifies native gunship-dust offsets and instruction selections through the production spawner without reading migrated tables.</summary>
    /// <param name="rom">Cartridge address space supplying native dust-table words and the shared vertical offset for comparison.</param>
    private static void VerifyCompiledGunshipDust(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var enemies = new RoomEnemySystem();
        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, new GunshipDustReadGuard(rom));
        var spawn = typeof(RoomEnemySystem).GetMethod("SpawnGunshipLiftoffDustCloud", flags)!
            .CreateDelegate<Action<ushort, SamusState>>(enemies);
        var samus = new SamusState();
        var slot = enemies.EnemyProjectiles[^1];
        for (ushort parameter = 0; parameter <= 10; parameter += 2)
        {
            short dx = unchecked((short)Word(EnemyRomTablePointersGunship.DustXOffsetWords + parameter));
            ushort instruction = Word(EnemyRomTablePointersGunship.DustInstructionPointers + parameter);
            var definition = GunshipDustDefinitions.ForParameter(parameter);
            AssertEqual(dx, definition.XOffset, "All native dust signed X offsets");
            AssertEqual(instruction, definition.Instruction, "All native dust instruction selections");
            for (int origin = 0; origin <= ushort.MaxValue; origin++)
            {
                slot.Clear();
                // Every world X and Y, including independent positive/negative wrap,
                // enters the actual allocator/header initializer with stale slot values.
                samus.XPosition = (ushort)origin;
                samus.YPosition = (ushort)(ushort.MaxValue - origin);
                slot.XSubposition = slot.YSubposition = 0xffff;
                slot.XVelocity = slot.YVelocity = 0x1234;
                spawn(parameter, samus);
                AssertEqual(RoomEnemyProjectileKind.GunshipLiftoffDustCloud, slot.Kind, "Native reverse allocation chooses last free slot");
                AssertEqual(unchecked((ushort)(samus.XPosition + dx)), slot.XPosition, "Actual dust signed X placement and wrap");
                AssertEqual(unchecked((ushort)(samus.YPosition + Word(0x86a2c5))), slot.YPosition, "Actual dust Y placement and wrap");
                AssertEqual(instruction, slot.InstructionPointer, "Actual dust program selection");
                AssertEqual(parameter, slot.Variable0, "Dust retains native even parameter");
                AssertEqual((ushort)1, slot.InstructionTimer, "Dust begins on first program tick");
                AssertEqual((ushort)0, slot.XSubposition, "Dust clears stale X fraction");
                AssertEqual((ushort)0, slot.YSubposition, "Dust clears stale Y fraction");
                AssertEqual((ushort)0, slot.XVelocity, "Dust clears stale X velocity");
                AssertEqual((ushort)0, slot.YVelocity, "Dust clears stale Y velocity");
            }
        }
        foreach (var occupied in enemies.EnemyProjectiles)
        {
            occupied.Kind = RoomEnemyProjectileKind.PhantoonStartingFlame;
            occupied.XPosition = 0x1234;
            occupied.InstructionPointer = 0x5678;
        }
        for (ushort parameter = 0; parameter <= 10; parameter += 2) spawn(parameter, samus);
        foreach (var occupied in enemies.EnemyProjectiles)
        {
            AssertEqual(RoomEnemyProjectileKind.PhantoonStartingFlame, occupied.Kind, "Full pool retains owner");
            AssertEqual((ushort)0x1234, occupied.XPosition, "Full pool retains X");
            AssertEqual((ushort)0x5678, occupied.InstructionPointer, "Full pool retains program");
        }
        foreach (ushort invalid in new ushort[] { 1, 3, 9, 11, 12, 0xffff })
            AssertThrows<ArgumentOutOfRangeException>(() => spawn(invalid, samus), "Invalid dust parameter still rejected before full-pool return");
        Console.WriteLine("Gunship dust: 12 native words, 393216 real wrapped spawns, full-pool retention and parameter validation pass with migrated reads forbidden.");
    }

    /// <summary>Address-space proxy that rejects reads from migrated gunship-dust tables while forwarding other reads and forbidding writes.</summary>
    /// <param name="source">Underlying address space for reads outside the dust-table range.</param>
    private sealed class GunshipDustReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes cartridge-byte requests through the dust-table read guard.</summary>
        /// <param name="address">Address requested by the caller.</param>
        /// <returns>The underlying byte unless the request targets a migrated dust table.</returns>
        /// <exception cref="InvalidOperationException">A cartridge read targets the migrated dust-table range.</exception>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads from the migrated dust-table range and forwards other byte requests.</summary>
        /// <param name="address">Address to read.</param>
        /// <returns>The byte returned by the underlying address space.</returns>
        /// <exception cref="InvalidOperationException">The address is within the migrated dust-table range.</exception>
        public byte ReadByte(int address) => address is >= 0x86a2d6 and < 0x86a2ee
            ? throw new InvalidOperationException("Unexpected migrated gunship dust table read.") : source.ReadByte(address);

        /// <summary>Rejects every write through this verification guard.</summary>
        /// <param name="address">Address the caller attempted to write.</param>
        /// <param name="value">Byte value the caller attempted to store.</param>
        /// <exception cref="InvalidOperationException">A write is attempted through the guard.</exception>
        public void WriteByte(int address, byte value) => throw new InvalidOperationException("Unexpected gunship dust bus write.");
    }
}
