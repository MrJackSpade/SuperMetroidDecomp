using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyBlueBrinstarFaceBlockInstructionProgramDefinitions()
    {
        VerifyBlueBrinstarFaceBlockInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyBlueBrinstarFaceBlockInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        for (int index = 0;
             index < BlueBrinstarFaceBlockInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            BlueBrinstarFaceBlockInstructionMechanicsWord definition =
                BlueBrinstarFaceBlockInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadBlueBrinstarFaceBlockWord(rom, 0xa80000 | definition.Address),
                $"Blue Brinstar face-block mechanics word $A8:{definition.Address:X4}");
        }

        var guard = new BlueBrinstarFaceBlockProgramReadGuard(rom);
        VerifyBlueBrinstarFaceBlockProgram(
            guard,
            parameter2: 0,
            samusX: null,
            BlueBrinstarFaceBlockInstructionProgramDefinitions.Initial,
            terminal: 0xe82c,
            frames: 4,
            "initial");
        VerifyBlueBrinstarFaceBlockProgram(
            guard,
            parameter2: 0,
            samusX: 0x00e0,
            BlueBrinstarFaceBlockInstructionProgramDefinitions.SamusLeft,
            terminal: 0xe818,
            frames: 96,
            "Samus-left");
        VerifyBlueBrinstarFaceBlockProgram(
            guard,
            parameter2: 1,
            samusX: 0x0120,
            BlueBrinstarFaceBlockInstructionProgramDefinitions.SamusRight,
            terminal: 0xe826,
            frames: 96,
            "Samus-right");

        AssertEqual(
            BlueBrinstarFaceBlockInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all live Blue Brinstar face-block spritemap words remain cartridge reads");
        for (int index = 0;
             index < BlueBrinstarFaceBlockInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                BlueBrinstarFaceBlockInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads face-block presentation word $A8:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled face-block mechanics byte");

        AssertThrows<InvalidDataException>(
            () => BlueBrinstarFaceBlockInstructionProgramDefinitions.ReadMechanicsWord(0xe80e),
            "interleaved face-block spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => BlueBrinstarFaceBlockInstructionProgramDefinitions.ReadMechanicsWord(0xe82e),
            "adjacent face-block initialization code is rejected as mechanics");

        _ = ProbeBlueBrinstarFaceBlockInstructionMechanicsAllocation();
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeBlueBrinstarFaceBlockInstructionMechanicsAllocation();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        AssertTrue(checksum != 0, "face-block allocation probe consumes live data");
        AssertEqual(0L, allocated,
            "warmed face-block mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Blue Brinstar face-block instruction mechanics: ten compiled words, all " +
            "three production programs, both activation sides, and seven live " +
            "spritemap reads pass with mechanics bytes forbidden.");

        static void VerifyBlueBrinstarFaceBlockProgram(
            BlueBrinstarFaceBlockProgramReadGuard guard,
            ushort parameter2,
            ushort? samusX,
            ushort expectedProgram,
            ushort terminal,
            int frames,
            string description)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            var initialize = typeof(RoomEnemySystem)
                .GetMethod("InitializeBlueBrinstarFaceBlock", flags)!
                .CreateDelegate<Action<RoomEnemySlot, SamusState?>>(enemies);
            var runMain = typeof(RoomEnemySystem)
                .GetMethod("RunBlueBrinstarFaceBlockMain", flags)!
                .CreateDelegate<Action<RoomEnemySlot, BlueBrinstarFaceBlockEnemyState, SamusState?>>(enemies);
            MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;

            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.BlueBrinstarFaceBlockDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
            slot.Parameter1 = 0x0040;
            slot.Parameter2 = parameter2;
            slot.XPosition = 0x0100;
            slot.YPosition = 0x0100;
            initialize(slot, null);

            if (samusX is ushort x)
            {
                var samus = new SamusState
                {
                    CollectedItems = 0x0004,
                    XPosition = x,
                    YPosition = slot.YPosition,
                };
                runMain(slot, enemies.BlueBrinstarFaceBlockStates[0]!, samus);
                AssertTrue(enemies.BlueBrinstarFaceBlockStates[0]!.Activated,
                    $"face-block {description} path activates");
            }

            AssertEqual(expectedProgram, slot.CurrentInstruction,
                $"face-block {description} program selection");
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            for (int frame = 0; frame < frames; frame++)
                process.Invoke(enemies, arguments);
            AssertEqual(terminal, slot.CurrentInstruction,
                $"face-block {description} terminal sleep");
        }
    }

    private static int ProbeBlueBrinstarFaceBlockInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += BlueBrinstarFaceBlockInstructionProgramDefinitions.ReadMechanicsWord(
                BlueBrinstarFaceBlockInstructionProgramDefinitions.Initial);
        }
        return checksum;
    }

    private static ushort ReadBlueBrinstarFaceBlockWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class BlueBrinstarFaceBlockProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (BlueBrinstarFaceBlockInstructionProgramDefinitions.IsCompiledMechanicsByte(
                    address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled face-block mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa80000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < BlueBrinstarFaceBlockInstructionProgramDefinitions
                         .PresentationWordCount;
                     index++)
                {
                    ushort presentation = BlueBrinstarFaceBlockInstructionProgramDefinitions
                        .PresentationWordAddress(index);
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
