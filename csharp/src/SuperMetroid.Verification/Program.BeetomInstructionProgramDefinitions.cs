using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyBeetomInstructionProgramDefinitions()
    {
        VerifyBeetomInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyBeetomInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static |
            BindingFlags.NonPublic;
        for (int index = 0;
             index < BeetomInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            BeetomInstructionMechanicsWord definition =
                BeetomInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadBeetomInstructionWord(rom, definition.Address),
                $"Beetom mechanics word $A8:{definition.Address:X4}");
        }

        var guard = new BeetomInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        Type type = typeof(RoomEnemySystem);
        type.GetField("_bus", flags)!.SetValue(enemies, guard);
        type.GetField("_setRandomNumber", flags)!.SetValue(
            enemies,
            (Action<ushort>)(_ => { }));
        var initialize = type.GetMethod("InitializeBeetom", flags)!
            .CreateDelegate<Action<RoomEnemySlot, SamusState, ushort>>(enemies);
        var startCrawling = type.GetMethod("StartBeetomCrawling", flags)!
            .CreateDelegate<Action<RoomEnemySlot, BeetomEnemyState, bool>>();
        var startDraining = type.GetMethod("StartBeetomDrain", flags)!
            .CreateDelegate<Action<RoomEnemySlot, BeetomEnemyState, bool>>();
        MethodInfo startHop = type.GetMethod("StartBeetomHop", flags)!;
        MethodInfo process = type.GetMethod("ProcessInstructions", flags)!;

        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = RoomEnemySystem.BeetomDefinition;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
        slot.XPosition = 0x0200;
        var samus = new SamusState { XPosition = 0x0100 };
        initialize(slot, samus, 0);
        BeetomEnemyState state = enemies.BeetomStates[0]!;
        object?[] processArguments =
            [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];

        AssertEqual(BeetomInstructionProgramDefinitions.CrawlingLeft,
            slot.CurrentInstruction,
            "real Beetom initializer installs left-crawl program");
        RunBeetomLoop(
            enemies,
            process,
            processArguments,
            slot,
            BeetomInstructionProgramDefinitions.CrawlingLeft,
            BeetomInstructionProgramDefinitions.CrawlingLeftLoop,
            callsThroughGoto: 5);
        AssertTrue(!slot.Properties.HasAny(EnemyProperties.ProcessOffScreen),
            "left-crawl program disables off-screen processing");

        startCrawling(slot, state, false);
        RunBeetomLoop(
            enemies,
            process,
            processArguments,
            slot,
            BeetomInstructionProgramDefinitions.CrawlingRight,
            BeetomInstructionProgramDefinitions.CrawlingRightLoop,
            callsThroughGoto: 5);
        AssertTrue(!slot.Properties.HasAny(EnemyProperties.ProcessOffScreen),
            "right-crawl program disables off-screen processing");

        StartBeetomHop(startHop, slot, state, left: true);
        RunBeetomSleepProgram(
            enemies,
            process,
            processArguments,
            slot,
            BeetomInstructionProgramDefinitions.HopLeft,
            BeetomInstructionProgramDefinitions.HopLeftSleep,
            timedFrames: 4);
        AssertTrue(slot.Properties.HasAny(EnemyProperties.ProcessOffScreen),
            "left-hop program enables off-screen processing");

        StartBeetomHop(startHop, slot, state, left: false);
        RunBeetomSleepProgram(
            enemies,
            process,
            processArguments,
            slot,
            BeetomInstructionProgramDefinitions.HopRight,
            BeetomInstructionProgramDefinitions.HopRightSleep,
            timedFrames: 4);
        AssertTrue(slot.Properties.HasAny(EnemyProperties.ProcessOffScreen),
            "right-hop program enables off-screen processing");

        startDraining(slot, state, true);
        RunBeetomLoop(
            enemies,
            process,
            processArguments,
            slot,
            BeetomInstructionProgramDefinitions.DrainingLeft,
            BeetomInstructionProgramDefinitions.DrainingLeftLoop,
            callsThroughGoto: 9);

        startDraining(slot, state, false);
        RunBeetomLoop(
            enemies,
            process,
            processArguments,
            slot,
            BeetomInstructionProgramDefinitions.DrainingRight,
            BeetomInstructionProgramDefinitions.DrainingRightLoop,
            callsThroughGoto: 9);

        AssertEqual(BeetomInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Beetom spritemap operands remain cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Beetom mechanics byte");
        AssertThrows<InvalidDataException>(
            () => BeetomInstructionProgramDefinitions.ReadMechanicsWord(0xb69a),
            "Beetom spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => BeetomInstructionProgramDefinitions.ReadMechanicsWord(0xb6c0),
            "unused Beetom small-hop program is rejected as production mechanics");

        _ = ProbeBeetomInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeBeetomInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Beetom allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Beetom mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Beetom instruction mechanics: 48 compiled words, all six production " +
            "programs, and 32 live spritemap reads pass with mechanics bytes forbidden.");
    }

    private static void StartBeetomHop(
        MethodInfo startHop,
        RoomEnemySlot slot,
        BeetomEnemyState state,
        bool left) =>
        startHop.Invoke(null, [slot, state, (ushort)0, left, false]);

    private static void RunBeetomLoop(
        RoomEnemySystem enemies,
        MethodInfo process,
        object?[] arguments,
        RoomEnemySlot slot,
        ushort entry,
        ushort loop,
        int callsThroughGoto)
    {
        AssertEqual(entry, slot.CurrentInstruction, "Beetom compiled loop entry");
        for (int call = 0; call < callsThroughGoto; call++)
        {
            slot.InstructionTimer = 1;
            process.Invoke(enemies, arguments);
        }
        AssertEqual(unchecked((ushort)(loop + 4)), slot.CurrentInstruction,
            "Beetom program completes its native goto and first repeated frame");
    }

    private static void RunBeetomSleepProgram(
        RoomEnemySystem enemies,
        MethodInfo process,
        object?[] arguments,
        RoomEnemySlot slot,
        ushort entry,
        ushort sleep,
        int timedFrames)
    {
        AssertEqual(entry, slot.CurrentInstruction, "Beetom compiled sleep-program entry");
        for (int frame = 0; frame < timedFrames; frame++)
        {
            slot.InstructionTimer = 1;
            process.Invoke(enemies, arguments);
        }
        slot.InstructionTimer = 1;
        process.Invoke(enemies, arguments);
        AssertEqual(sleep, slot.CurrentInstruction,
            "Beetom hop program reaches terminal sleep");
    }

    private static int ProbeBeetomInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += BeetomInstructionProgramDefinitions.ReadMechanicsWord(
                (index % 6) switch
                {
                    0 => BeetomInstructionProgramDefinitions.CrawlingLeft,
                    1 => BeetomInstructionProgramDefinitions.HopLeft,
                    2 => BeetomInstructionProgramDefinitions.DrainingLeft,
                    3 => BeetomInstructionProgramDefinitions.CrawlingRight,
                    4 => BeetomInstructionProgramDefinitions.HopRight,
                    _ => BeetomInstructionProgramDefinitions.DrainingRight,
                });
        }
        return checksum;
    }

    private static ushort ReadBeetomInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa80000 | address) |
            source.ReadByte(0xa80000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class BeetomInstructionReadGuard(
        ISnesAddressSpace source) : ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (BeetomInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Beetom mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa80000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < BeetomInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        BeetomInstructionProgramDefinitions.PresentationWordAddress(index);
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
