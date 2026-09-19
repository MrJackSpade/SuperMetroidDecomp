using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifySkulteraInstructionProgramDefinitions()
    {
        VerifySkulteraInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifySkulteraInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        for (int index = 0;
             index < SkulteraInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            SkulteraInstructionMechanicsWord definition =
                SkulteraInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadSkulteraInstructionWord(rom, 0xa30000 | definition.Address),
                $"Skultera instruction mechanics word $A3:{definition.Address:X4}");
        }

        var guard = new SkulteraInstructionProgramReadGuard(rom);
        RoomEnemySystem leftSystem =
            CreateSkulteraProgramSystem(guard, startsLeft: true, out RoomEnemySlot left);
        RoomEnemySystem rightSystem =
            CreateSkulteraProgramSystem(guard, startsLeft: false, out RoomEnemySlot right);

        AssertEqual(SkulteraInstructionProgramDefinitions.SwimmingLeft,
            left.CurrentInstruction, "Skultera initializer selects left-swimming program");
        AssertEqual(SkulteraInstructionProgramDefinitions.SwimmingRight,
            right.CurrentInstruction, "Skultera initializer selects right-swimming program");

        RunSkulteraProgram(leftSystem, left, frames: 45);
        RunSkulteraProgram(rightSystem, right, frames: 45);
        AssertEqual((ushort)2, left.Layer,
            "left-swimming Skultera program selects native layer two");
        AssertEqual((ushort)6, right.Layer,
            "right-swimming Skultera program selects native layer six");

        var beginTurn = typeof(RoomEnemySystem).GetMethod("BeginSkulteraTurn", flags)!
            .CreateDelegate<Action<RoomEnemySlot, SkulteraEnemyState, bool>>();
        SkulteraEnemyState leftState = leftSystem.SkulteraStates[0]!;
        SkulteraEnemyState rightState = rightSystem.SkulteraStates[0]!;
        beginTurn(left, leftState, true);
        beginTurn(right, rightState, false);
        AssertEqual(SkulteraInstructionProgramDefinitions.TurningRight,
            left.CurrentInstruction, "left swimmer enters turning-right program");
        AssertEqual(SkulteraInstructionProgramDefinitions.TurningLeft,
            right.CurrentInstruction, "right swimmer enters turning-left program");

        RunSkulteraProgram(leftSystem, left, frames: 80);
        RunSkulteraProgram(rightSystem, right, frames: 80);
        AssertTrue(leftState.TurnFinished,
            "turning-right program publishes Skultera completion flag");
        AssertTrue(rightState.TurnFinished,
            "turning-left program publishes Skultera completion flag");
        AssertEqual((ushort)0x905e, left.CurrentInstruction,
            "turning-right program sleeps at its native terminal command");
        AssertEqual((ushort)0x9094, right.CurrentInstruction,
            "turning-left program sleeps at its native terminal command");

        AssertEqual(
            SkulteraInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all live Skultera spritemap words remain cartridge reads");
        for (int index = 0;
             index < SkulteraInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                SkulteraInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Skultera presentation word $A3:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Skultera mechanics byte");

        AssertThrows<InvalidDataException>(
            () => SkulteraInstructionProgramDefinitions.ReadMechanicsWord(0x902e),
            "interleaved Skultera spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => SkulteraInstructionProgramDefinitions.ReadMechanicsWord(0x9096),
            "adjacent Skultera callback implementation is rejected as mechanics");

        _ = ProbeSkulteraInstructionMechanicsAllocation();
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeSkulteraInstructionMechanicsAllocation();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        AssertTrue(checksum != 0, "Skultera allocation probe consumes live data");
        AssertEqual(0L, allocated,
            "warmed Skultera mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Skultera instruction mechanics: thirty-two compiled words, both swimming " +
            "loops, both turning programs, layer/completion callbacks, sleeps, and " +
            "twenty-two live spritemap reads pass with mechanics bytes forbidden.");

        static RoomEnemySystem CreateSkulteraProgramSystem(
            SkulteraInstructionProgramReadGuard guard,
            bool startsLeft,
            out RoomEnemySlot slot)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            var initialize = typeof(RoomEnemySystem).GetMethod("InitializeSkultera", flags)!
                .CreateDelegate<Action<RoomEnemySlot>>(enemies);
            slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.SkulteraDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
            slot.Parameter1 = startsLeft ? (ushort)0x0100 : (ushort)0x0000;
            slot.Parameter2 = 0x0120;
            slot.XPosition = 0x0100;
            slot.YPosition = 0x0200;
            initialize(slot);
            slot.InstructionTimer = 1;
            return enemies;
        }

        static void RunSkulteraProgram(
            RoomEnemySystem enemies,
            RoomEnemySlot slot,
            int frames)
        {
            MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            for (int frame = 0; frame < frames; frame++)
                process.Invoke(enemies, arguments);
        }
    }

    private static int ProbeSkulteraInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += SkulteraInstructionProgramDefinitions.ReadMechanicsWord(
                SkulteraInstructionProgramDefinitions.SwimmingLeft);
        }
        return checksum;
    }

    private static ushort ReadSkulteraInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class SkulteraInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (SkulteraInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Skultera mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa30000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < SkulteraInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        SkulteraInstructionProgramDefinitions.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ObservedPresentationWords.Add(presentation);
                        break;
                    }
                }
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
