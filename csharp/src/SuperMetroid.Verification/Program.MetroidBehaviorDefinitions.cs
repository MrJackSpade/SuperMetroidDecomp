using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
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

        VerifyMetroidEscapeConsumer(rom);
        VerifyMetroidCryConsumer(rom);
        Console.WriteLine(
            "Metroid behavior definitions: eight displacement words, eight cry words, all 65,536 escape selectors and all eight production instruction selections pass with source tables forbidden.");
    }

    private static void VerifyMetroidEscapeConsumer(SuperMetroidAddressSpace rom)
    {
        MethodInfo runEscape = typeof(RoomEnemySystem).GetMethod(
            "RunMetroidPowerBombEscape",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        FieldInfo busField = typeof(RoomEnemySystem).GetField(
            "_bus", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var guarded = new MetroidBehaviorReadGuard(rom);

        for (int timer = 0; timer <= ushort.MaxValue; timer++)
        {
            var enemies = new RoomEnemySystem();
            busField.SetValue(enemies, guarded);
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
            AssertEqual(timer == 1 ? 0xe9cf : 0x7777, slot.CurrentInstruction,
                $"production Metroid escape instruction {timer}");
        }
    }

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
            slot.CurrentInstruction = MetroidBehaviorReadGuard.ScriptPointer;

            process.Invoke(enemies, [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0]);

            AssertEqual(MetroidBehaviorDefinitions.RandomCrySoundEffect(random),
                enemies.LastMetroidSoundEffectLibrary2 ?? ushort.MaxValue,
                $"production Metroid random cry {random}");
            AssertEqual(1, slot.InstructionTimer,
                $"production Metroid script timer {random}");
            AssertEqual(0x1234, slot.SpritemapPointer,
                $"production Metroid script spritemap {random}");
            AssertEqual(MetroidBehaviorReadGuard.ScriptPointer + 6, slot.CurrentInstruction,
                $"production Metroid script continuation {random}");
        }
    }

    private static ushort ReadMetroidBehaviorWord(SuperMetroidAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class MetroidBehaviorReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal const ushort ScriptPointer = 0x8000;

        public byte ReadByte(int address)
        {
            if (address is >= 0xa3ea3f and < 0xa3ea4f ||
                address is >= 0xa3ead6 and < 0xa3eae6)
            {
                throw new InvalidOperationException(
                    $"Metroid behavior attempted migrated table read ${address:X6}.");
            }

            return address switch
            {
                0xa38000 => unchecked((byte)EnemyInstructionCodePointers.Instruction_Metroid_PlayRandomMetroidSFX),
                0xa38001 => (byte)(EnemyInstructionCodePointers.Instruction_Metroid_PlayRandomMetroidSFX >> 8),
                0xa38002 => 0x01,
                0xa38003 => 0x00,
                0xa38004 => 0x34,
                0xa38005 => 0x12,
                _ => source.ReadByte(address),
            };
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
