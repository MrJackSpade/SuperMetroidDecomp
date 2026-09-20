using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyNorfairLavaJumperInstructionProgramDefinitions()
    {
        VerifyNorfairLavaJumperInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyNorfairLavaJumperInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < NorfairLavaJumperInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            NorfairLavaJumperInstructionMechanicsWord definition =
                NorfairLavaJumperInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadNorfairLavaJumperInstructionWord(rom, 0xa20000 | definition.Address),
                $"Norfair lava-jumper mechanics word $A2:{definition.Address:X4}");
        }

        var guard = new NorfairLavaJumperInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        Type type = typeof(RoomEnemySystem);
        type.GetField("_bus", flags)!.SetValue(enemies, guard);
        type.GetField("_nextRandom", flags)!.SetValue(enemies, (Func<ushort>)(() => 0));
        var initialize = type.GetMethod("InitializeNorfairLavaJumpingEnemy", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        var runMain = type.GetMethod("RunNorfairLavaJumpingEnemyMain", flags)!
            .CreateDelegate<Action<RoomEnemySlot, NorfairLavaJumpingEnemyState>>(enemies);
        MethodInfo process = type.GetMethod("ProcessInstructions", flags)!;

        RoomEnemySlot parent = enemies.Slots[0];
        parent.EnemyDefinitionPointer = RoomEnemySystem.NorfairLavaJumpingEnemyDefinition;
        parent.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        parent.XPosition = 0x0100;
        parent.YPosition = 0x01e0;
        initialize(parent);
        NorfairLavaJumpingEnemyState parentState =
            enemies.NorfairLavaJumpingEnemyStates[0]!;
        object?[] processArguments =
            [parent, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];

        RunNorfairLavaJumperProgram(
            enemies, process, processArguments, parent,
            NorfairLavaJumperInstructionProgramDefinitions.Hidden,
            NorfairLavaJumperInstructionProgramDefinitions.HiddenSleep,
            timedFrames: 1);

        runMain(parent, parentState);
        parentState.YVelocity = 0xfbc8;
        runMain(parent, parentState);
        AssertEqual(NorfairLavaJumperInstructionProgramDefinitions.Jump,
            parent.CurrentInstruction,
            "real lava-jumper rise handoff installs jump program");
        RunNorfairLavaJumperProgram(
            enemies, process, processArguments, parent,
            NorfairLavaJumperInstructionProgramDefinitions.Jump,
            NorfairLavaJumperInstructionProgramDefinitions.JumpSleep,
            timedFrames: 7);
        AssertTrue(parentState.AnimationFinished,
            "jump program publishes its real animation handshake");

        RoomEnemySlot follower = enemies.Slots[1];
        follower.EnemyDefinitionPointer = RoomEnemySystem.NorfairLavaJumpingEnemyDefinition;
        follower.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        follower.Parameter1 = 0x8000;
        initialize(follower);
        AssertEqual(NorfairLavaJumperInstructionProgramDefinitions.Follower,
            follower.CurrentInstruction,
            "real follower initializer installs compiled loop");
        processArguments[0] = follower;
        for (int frame = 0; frame < 7; frame++)
        {
            follower.InstructionTimer = 1;
            process.Invoke(enemies, processArguments);
        }
        AssertEqual(unchecked((ushort)(
                NorfairLavaJumperInstructionProgramDefinitions.Follower + 4)),
            follower.CurrentInstruction,
            "follower program loops to its first timed frame");

        AssertEqual(NorfairLavaJumperInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "lava-jumper spritemap operands remain cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled lava-jumper mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => NorfairLavaJumperInstructionProgramDefinitions.ReadMechanicsWord(0xbe44),
            "lava-jumper presentation pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => NorfairLavaJumperInstructionProgramDefinitions.ReadMechanicsWord(0xbe86),
            "adjacent lava-jumper velocity table is rejected as mechanics");

        _ = ProbeNorfairLavaJumperInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeNorfairLavaJumperInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "lava-jumper allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed lava-jumper mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Norfair lava-jumper instruction mechanics: 23 compiled words, all three " +
            "production programs, the real handshake, and 14 live spritemap reads pass.");
    }

    private static void RunNorfairLavaJumperProgram(
        RoomEnemySystem enemies,
        MethodInfo process,
        object?[] arguments,
        RoomEnemySlot slot,
        ushort entry,
        ushort terminalSleep,
        int timedFrames)
    {
        AssertEqual(entry, slot.CurrentInstruction, "lava-jumper compiled program entry");
        for (int frame = 0; frame < timedFrames; frame++)
        {
            slot.InstructionTimer = 1;
            process.Invoke(enemies, arguments);
        }
        slot.InstructionTimer = 1;
        process.Invoke(enemies, arguments);
        AssertEqual(terminalSleep, slot.CurrentInstruction,
            "lava-jumper program reaches terminal sleep");
    }

    private static int ProbeNorfairLavaJumperInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += NorfairLavaJumperInstructionProgramDefinitions.ReadMechanicsWord(
                (index % 3) switch
                {
                    0 => NorfairLavaJumperInstructionProgramDefinitions.Hidden,
                    1 => NorfairLavaJumperInstructionProgramDefinitions.Jump,
                    _ => NorfairLavaJumperInstructionProgramDefinitions.Follower,
                });
        }
        return checksum;
    }

    private static ushort ReadNorfairLavaJumperInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class NorfairLavaJumperInstructionReadGuard(
        ISnesAddressSpace source) : ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (NorfairLavaJumperInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled lava-jumper mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa20000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < NorfairLavaJumperInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        NorfairLavaJumperInstructionProgramDefinitions.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                        ObservedPresentationWords.Add(presentation);
                }
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
