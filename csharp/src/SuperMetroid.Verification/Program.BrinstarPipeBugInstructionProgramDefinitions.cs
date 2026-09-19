using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyBrinstarPipeBugInstructionProgramDefinitions()
    {
        VerifyBrinstarPipeBugInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyBrinstarPipeBugInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        for (int index = 0;
             index < BrinstarPipeBugInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            BrinstarPipeBugInstructionMechanicsWord definition =
                BrinstarPipeBugInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadBrinstarPipeBugInstructionWord(rom, 0xb30000 | definition.Address),
                $"Brinstar Pipe Bug instruction mechanics word $B3:{definition.Address:X4}");
        }

        var guard = new BrinstarPipeBugInstructionProgramReadGuard(rom);
        (PipeBugAnimationSelector Selector, ushort NormalCursor, int NormalFrames,
            ushort StrongCursor, int StrongFrames)[] programs =
        [
            (PipeBugAnimationSelector.None, 0x87af, 17, 0x8a21, 7),
            (PipeBugAnimationSelector.Shooting, 0x87d3, 7, 0x8a35, 13),
            (PipeBugAnimationSelector.FacingRight, 0x87ef, 17, 0x8a49, 7),
            (PipeBugAnimationSelector.FacingRight | PipeBugAnimationSelector.Shooting,
                0x8813, 7, 0x8a5d, 13),
        ];

        foreach (var program in programs)
        {
            VerifyProgram(strong: false, program.Selector, program.NormalCursor,
                program.NormalFrames);
            VerifyProgram(strong: true, program.Selector, program.StrongCursor,
                program.StrongFrames);
        }

        AssertEqual(BrinstarPipeBugInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all live Brinstar Pipe Bug spritemap words remain cartridge reads");
        for (int index = 0;
             index < BrinstarPipeBugInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                BrinstarPipeBugInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Pipe Bug presentation word $B3:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Brinstar Pipe Bug mechanics byte");

        AssertThrows<InvalidDataException>(
            () => BrinstarPipeBugInstructionProgramDefinitions.ReadMechanicsWord(0x87ad),
            "interleaved Pipe Bug spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => BrinstarPipeBugInstructionProgramDefinitions.ReadMechanicsWord(0x882b),
            "adjacent Pipe Bug selector table is rejected as mechanics");

        _ = ProbeBrinstarPipeBugInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeBrinstarPipeBugInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Pipe Bug allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Pipe Bug mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Brinstar Pipe Bug instruction mechanics: sixty compiled words, all eight " +
            "normal/strong programs, complete loops, and forty-four live spritemap reads " +
            "pass with mechanics bytes forbidden.");

        void VerifyProgram(
            bool strong,
            PipeBugAnimationSelector selector,
            ushort expectedCursor,
            int frames)
        {
            RoomEnemySystem enemies = CreateProgramSystem(strong, selector, out RoomEnemySlot slot);
            AssertEqual(PipeBugDefinitions.BrinstarInstructionList(strong, selector),
                slot.CurrentInstruction,
                $"{(strong ? "strong" : "normal")} Pipe Bug installs {selector}");
            RunProgram(enemies, slot, frames);
            AssertEqual(expectedCursor, slot.CurrentInstruction,
                $"{(strong ? "strong" : "normal")} Pipe Bug {selector} loops");
        }

        RoomEnemySystem CreateProgramSystem(
            bool strong,
            PipeBugAnimationSelector selector,
            out RoomEnemySlot slot)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            var initialize = typeof(RoomEnemySystem).GetMethod(
                "InitializeBrinstarPipeBug", flags)!
                .CreateDelegate<Action<RoomEnemySlot>>(enemies);
            MethodInfo install = typeof(RoomEnemySystem).GetMethod(
                "SelectBrinstarPipeBugAnimation", flags)!;
            slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = strong
                ? PipeBugDefinitions.StrongBrinstarEnemyDefinition
                : PipeBugDefinitions.BrinstarEnemyDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xb3 };
            slot.Parameter1 = strong ? (ushort)1 : (ushort)0;
            initialize(slot);
            PipeBugEnemyState state = enemies.PipeBugStates[0]!;
            state.InstalledAnimationState = selector == PipeBugAnimationSelector.None
                ? PipeBugAnimationSelector.Shooting
                : PipeBugAnimationSelector.None;
            state.AnimationState = selector;
            install.Invoke(null, [slot, state]);
            return enemies;
        }

        static void RunProgram(RoomEnemySystem enemies, RoomEnemySlot slot, int frames)
        {
            MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            for (int frame = 0; frame < frames; frame++)
                process.Invoke(enemies, arguments);
        }
    }

    private static int ProbeBrinstarPipeBugInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += BrinstarPipeBugInstructionProgramDefinitions.ReadMechanicsWord(
                BrinstarPipeBugInstructionProgramDefinitions.NormalRisingLeft);
        }
        return checksum;
    }

    private static ushort ReadBrinstarPipeBugInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class BrinstarPipeBugInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (BrinstarPipeBugInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Pipe Bug mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xb30000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < BrinstarPipeBugInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        BrinstarPipeBugInstructionProgramDefinitions.PresentationWordAddress(index);
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
