using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Checks the compiled Metroid escape and cry tables against cartridge data and exercises their production consumers.
    /// </summary>
    /// <param name="rom">Cartridge address space used as the reference for the original table values.</param>
    private static void VerifyMetroidBehaviorDefinitions(SuperMetroidAddressSpace rom)
    {
        for (ushort frame = 0; frame < 4; frame++)
        {
            MetroidEscapeDisplacement actual =
                MetroidBehaviorDefinitions.EscapeDisplacement(frame);
            AssertEqual(unchecked((short)ReadMetroidBehaviorWord(rom, 0xa3ea3f + frame * 2)), actual.X,
                $"Metroid escape X displacement {frame}");
            AssertEqual(unchecked((short)ReadMetroidBehaviorWord(rom, 0xa3ea47 + frame * 2)), actual.Y,
                $"Metroid escape Y displacement {frame}");
        }

        for (ushort index = 0; index < 8; index++)
        {
            AssertEqual(ReadMetroidBehaviorWord(rom, 0xa3ead6 + index * 2),
                MetroidBehaviorDefinitions.RandomCrySoundEffect(index),
                $"Metroid random cry {index}");
        }

        Suite(nameof(VerifyMetroidEscapeConsumer), () => VerifyMetroidEscapeConsumer(rom));
        Suite(nameof(VerifyMetroidCryConsumer), () => VerifyMetroidCryConsumer(rom));
        Console.WriteLine(
            "Metroid behavior definitions: eight displacement words, eight cry words, all 65,536 escape selectors and all eight production instruction selections pass with source tables forbidden.");
    }

    /// <summary>Confirms the production escape routine applies the compiled displacement and countdown behavior for every timer value.</summary>
    /// <param name="rom">Cartridge address space wrapped to reject reads from the migrated displacement table.</param>
    private static void VerifyMetroidEscapeConsumer(SuperMetroidAddressSpace rom)
    {
        MethodInfo runEscape = typeof(RoomEnemySystem).GetMethod(
            "RunMetroidPowerBombEscape",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        FieldInfo busField = typeof(RoomEnemySystem).GetField(
            "_bus", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var guarded = new MetroidBehaviorReadGuard(rom);
        // One system serves every timer: the escape step is static and keeps no per-call state.
        var enemies = new RoomEnemySystem();
        busField.SetValue(enemies, guarded);

        for (int timer = 0; timer <= ushort.MaxValue; timer++)
        {
            var slot = new RoomEnemySlot(0)
            {
                XPosition = 0x0001,
                YPosition = 0xffff,
                CurrentInstruction = 0x7777,
            };
            var state = new MetroidEnemyState(
                slot,
                new RoomSpriteObjectSlot(0),
                new RoomSpriteObjectSlot(1))
            {
                EscapeTimer = (ushort)timer,
                Function = MetroidAiFunction.PowerBombEscape,
            };

            MetroidEscapeDisplacement expected =
                MetroidBehaviorDefinitions.EscapeDisplacement((ushort)timer);
            runEscape.Invoke(enemies, [slot, state]);
            AssertEqual(unchecked((ushort)(1 + expected.X)), slot.XPosition,
                $"production Metroid escape X timer {timer}");
            AssertEqual(unchecked((ushort)(0xffff + expected.Y)), slot.YPosition,
                $"production Metroid escape Y timer {timer}");
            AssertEqual(unchecked((ushort)(timer - 1)), state.EscapeTimer,
                $"production Metroid escape countdown {timer}");
            AssertEqual(timer == 1 ? MetroidAiFunction.Homing : MetroidAiFunction.PowerBombEscape,
                state.Function,
                $"production Metroid escape function {timer}");
            AssertEqual(
                timer == 1
                    ? MetroidInstructionProgramDefinitions.ChasingSamus
                    : (ushort)0x7777,
                slot.CurrentInstruction,
                $"production Metroid escape instruction {timer}");
        }
    }

    /// <summary>Confirms the production instruction interpreter selects the compiled cry sound for each random-table index.</summary>
    /// <param name="rom">Cartridge address space used for reference instruction data and guarded against migrated-table reads.</param>
    private static void VerifyMetroidCryConsumer(SuperMetroidAddressSpace rom)
    {
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessInstructions",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        FieldInfo busField = typeof(RoomEnemySystem).GetField(
            "_bus", BindingFlags.Instance | BindingFlags.NonPublic)!;
        FieldInfo randomField = typeof(RoomEnemySystem).GetField(
            "_nextRandom", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var guarded = new MetroidBehaviorReadGuard(rom);

        for (ushort random = 0; random < 8; random++)
        {
            var enemies = new RoomEnemySystem();
            busField.SetValue(enemies, guarded);
            ushort selectedRandom = random;
            randomField.SetValue(enemies, (Func<ushort>)(() => selectedRandom));
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.MetroidDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
            slot.InstructionTimer = 1;
            slot.CurrentInstruction =
                MetroidInstructionProgramDefinitions.ChasingSoundCallback;

            process.Invoke(enemies, [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0]);

            AssertEqual(MetroidBehaviorDefinitions.RandomCrySoundEffect(random),
                enemies.LastMetroidSoundEffectLibrary2 ?? ushort.MaxValue,
                $"production Metroid random cry {random}");
            AssertEqual(16, slot.InstructionTimer,
                $"production Metroid script timer {random}");
            AssertEqual(ReadMetroidBehaviorWord(rom, 0xa3e9d1), slot.SpritemapPointer,
                $"production Metroid script spritemap {random}");
            AssertEqual(
                unchecked((ushort)(MetroidInstructionProgramDefinitions.ChasingSamus + 4)),
                slot.CurrentInstruction,
                $"production Metroid script continuation {random}");
        }
    }

    /// <summary>Reads a little-endian word from the cartridge table under verification.</summary>
    /// <param name="bus">Address space supplying the word's bytes.</param>
    /// <param name="address">Cartridge address of the low byte.</param>
    /// <returns>The unsigned 16-bit value stored at the address.</returns>
    private static ushort ReadMetroidBehaviorWord(SuperMetroidAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    /// <summary>
    /// Wraps cartridge access and fails if production Metroid behavior attempts to read the migrated escape or cry tables.
    /// </summary>
    /// <param name="source">Underlying address space used for permitted reads and forwarded writes.</param>
    private sealed class MetroidBehaviorReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes importer reads through the guarded address-space read path.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The byte at the address if it is outside the migrated tables.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads from migrated Metroid behavior tables and delegates all other reads.</summary>
        /// <param name="address">Cartridge address to read.</param>
        /// <returns>The byte returned by the underlying address space for an allowed address.</returns>
        /// <exception cref="InvalidOperationException">The address falls within an escape-displacement or random-cry table range.</exception>
        public byte ReadByte(int address)
        {
            if (address is >= 0xa3ea3f and < 0xa3ea4f ||
                address is >= 0xa3ead6 and < 0xa3eae6)
            {
                throw new InvalidOperationException(
                    $"Metroid behavior attempted migrated table read ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        /// <summary>Forwards a write to the wrapped cartridge address space.</summary>
        /// <param name="address">Cartridge address to update.</param>
        /// <param name="value">Byte to store at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
