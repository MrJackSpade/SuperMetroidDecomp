using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyWaverInstructionProgramDefinitions()
    {
        VerifyWaverInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyWaverInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        for (int index = 0;
             index < WaverInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            WaverInstructionMechanicsWord definition =
                WaverInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadWaverInstructionWord(rom, 0xa30000 | definition.Address),
                $"Waver instruction mechanics word $A3:{definition.Address:X4}");
        }

        var guard = new WaverInstructionProgramReadGuard(rom);
        (WaverAnimationSelector Selector, ushort Terminal, int Frames)[] programs =
        [
            (WaverAnimationSelector.None, 0x86ab, 2),
            (WaverAnimationSelector.FacingRight, 0x86b1, 2),
            (WaverAnimationSelector.Spinning, 0x86c5, 34),
            (WaverAnimationSelector.Spinning | WaverAnimationSelector.FacingRight,
                0x86d9, 34),
        ];

        foreach ((WaverAnimationSelector selector, ushort terminal, int frames) in programs)
        {
            RoomEnemySystem enemies =
                CreateWaverProgramSystem(guard, selector, out RoomEnemySlot slot);
            WaverEnemyState state = enemies.WaverStates[0]!;
            AssertEqual(WaverAnimationDefinitions.InstructionList(selector),
                slot.CurrentInstruction, $"Waver installs {selector} program");
            RunWaverProgram(enemies, slot, frames);
            AssertEqual(terminal, slot.CurrentInstruction,
                $"Waver {selector} program sleeps at native terminal command");
            AssertEqual(selector.HasFlag(WaverAnimationSelector.Spinning),
                state.SpinFinished, $"Waver {selector} spin-completion callback");
        }

        AssertEqual(0, guard.ForbiddenPresentationReadAttempts,
            "Waver production programs never read installed visual selectors");
        for (int index = 0;
             index < WaverInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = WaverInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertEqual(ReadWaverInstructionWord(rom, 0xa30000 | address),
                EnemySpritemapDefinitions.WaverFrameAt(address),
                $"compiled Waver frame selection $A3:{address:X4}");
        }
        AssertThrows<InvalidDataException>(
            () => EnemySpritemapDefinitions.WaverFrameAt(0x86db),
            "unlisted Waver frame selector fails loudly");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Waver mechanics byte");

        AssertThrows<InvalidDataException>(
            () => WaverInstructionProgramDefinitions.ReadMechanicsWord(0x86a9),
            "interleaved Waver spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => WaverInstructionProgramDefinitions.ReadMechanicsWord(0x86db),
            "adjacent Waver selector table is rejected as mechanics");

        _ = ProbeWaverInstructionMechanicsAllocation();
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeWaverInstructionMechanicsAllocation();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        AssertTrue(checksum != 0, "Waver allocation probe consumes live data");
        AssertEqual(0L, allocated,
            "warmed Waver mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Waver instruction mechanics: sixteen compiled words, both steady and both " +
            "spinning programs, completion callbacks, and sleeps pass with all " +
            "ten visual-selector and mechanics source words forbidden.");

        static RoomEnemySystem CreateWaverProgramSystem(
            WaverInstructionProgramReadGuard guard,
            WaverAnimationSelector selector,
            out RoomEnemySlot slot)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            var initialize = typeof(RoomEnemySystem).GetMethod("InitializeWaver", flags)!
                .CreateDelegate<Action<RoomEnemySlot>>(enemies);
            var install = typeof(RoomEnemySystem).GetMethod("SetWaverInstructionList", flags)!
                .CreateDelegate<Action<RoomEnemySlot, WaverEnemyState>>();

            slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.WaverDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
            initialize(slot);
            WaverEnemyState state = enemies.WaverStates[0]!;
            state.CurrentInstructionListIndex = selector == WaverAnimationSelector.None
                ? WaverAnimationSelector.FacingRight
                : WaverAnimationSelector.None;
            state.RequestedInstructionListIndex = selector;
            state.SpinFinished = false;
            install(slot, state);
            return enemies;
        }

        static void RunWaverProgram(RoomEnemySystem enemies, RoomEnemySlot slot, int frames)
        {
            MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            for (int frame = 0; frame < frames; frame++)
                process.Invoke(enemies, arguments);
        }
    }

    private static int ProbeWaverInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += WaverInstructionProgramDefinitions.ReadMechanicsWord(
                WaverInstructionProgramDefinitions.SteadyFacingLeft);
        }
        return checksum;
    }

    private static ushort ReadWaverInstructionWord(SuperMetroidAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class WaverInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal int ForbiddenPresentationReadAttempts { get; private set; }
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (WaverInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Waver mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa30000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < WaverInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        WaverInstructionProgramDefinitions.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ForbiddenPresentationReadAttempts++;
                        throw new InvalidOperationException(
                            $"Production read installed Waver selector ${address:X6}.");
                    }
                }
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
