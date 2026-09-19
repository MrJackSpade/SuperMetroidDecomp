using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyZoaInstructionProgramDefinitions()
    {
        VerifyZoaInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyZoaInstructionProgramDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        for (int index = 0;
             index < ZoaInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            ZoaInstructionMechanicsWord definition =
                ZoaInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadZoaInstructionWord(rom, 0xa30000 | definition.Address),
                $"Zoa instruction mechanics word $A3:{definition.Address:X4}");
        }

        var guard = new ZoaInstructionProgramReadGuard(rom);
        ZoaAnimationSelector[] selectors =
        [
            ZoaAnimationSelector.None,
            ZoaAnimationSelector.Rising,
            ZoaAnimationSelector.FacingRight,
            ZoaAnimationSelector.FacingRight | ZoaAnimationSelector.Rising,
        ];
        foreach (ZoaAnimationSelector selector in selectors)
        {
            RoomEnemySystem enemies = CreateZoaProgramSystem(guard, selector, out RoomEnemySlot slot);
            ZoaEnemyState state = enemies.ZoaStates[0]!;
            AssertEqual(ZoaAnimationDefinitions.InstructionList(selector),
                slot.CurrentInstruction, $"Zoa installs {selector} program");

            if ((selector & ZoaAnimationSelector.Rising) != 0)
            {
                RunZoaProgram(enemies, slot, 16);
                continue;
            }

            RunZoaProgram(enemies, slot, 1);
            AssertEqual((ushort)4, state.XSpeedTableIndex,
                $"Zoa {selector} first callback selects speed row four");
            RunZoaProgram(enemies, slot, 64);
            AssertEqual((ushort)8, state.XSpeedTableIndex,
                $"Zoa {selector} second callback selects speed row eight");
            RunZoaProgram(enemies, slot, 8);
            AssertEqual((ushort)12, state.XSpeedTableIndex,
                $"Zoa {selector} third callback selects speed row twelve");
            RunZoaProgram(enemies, slot, 48);
            AssertEqual((ushort)4, state.XSpeedTableIndex,
                $"Zoa {selector} loop restarts at speed row four");
        }

        AssertEqual(ZoaInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all live Zoa spritemap words remain cartridge reads");
        for (int index = 0;
             index < ZoaInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = ZoaInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Zoa presentation word $A3:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Zoa mechanics byte");

        AssertThrows<InvalidDataException>(
            () => ZoaInstructionProgramDefinitions.ReadMechanicsWord(0xb3c5),
            "interleaved Zoa spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => ZoaInstructionProgramDefinitions.ReadMechanicsWord(0xb40d),
            "adjacent Zoa selector table is rejected as mechanics");

        _ = ProbeZoaInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeZoaInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Zoa allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Zoa mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Zoa instruction mechanics: twenty-six compiled words, all four programs, " +
            "three speed callbacks per shooting direction, and twelve live spritemap " +
            "reads pass with mechanics bytes forbidden.");

        static RoomEnemySystem CreateZoaProgramSystem(
            ZoaInstructionProgramReadGuard guard,
            ZoaAnimationSelector selector,
            out RoomEnemySlot slot)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            typeof(RoomEnemySystem).GetMethod("InitializeZoa", flags)!
                .Invoke(enemies, [slot = enemies.Slots[0]]);
            slot.EnemyDefinitionPointer = RoomEnemySystem.ZoaDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
            ZoaEnemyState state = enemies.ZoaStates[0]!;
            state.PreviousInstructionListTableIndex = selector == ZoaAnimationSelector.None
                ? ZoaAnimationSelector.Rising
                : ZoaAnimationSelector.None;
            state.InstructionListTableIndex = selector;
            state.XSpeedTableIndex = 0;
            typeof(RoomEnemySystem).GetMethod("SetZoaInstructionList", flags)!
                .Invoke(null, [slot, state]);
            return enemies;
        }

        static void RunZoaProgram(RoomEnemySystem enemies, RoomEnemySlot slot, int frames)
        {
            MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            for (int frame = 0; frame < frames; frame++)
                process.Invoke(enemies, arguments);
        }
    }

    private static int ProbeZoaInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += ZoaInstructionProgramDefinitions.ReadMechanicsWord(
                ZoaInstructionProgramDefinitions.FacingLeftShooting);
        }
        return checksum;
    }

    private static ushort ReadZoaInstructionWord(SuperMetroidAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class ZoaInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (ZoaInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Zoa mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa30000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < ZoaInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        ZoaInstructionProgramDefinitions.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ObservedPresentationWords.Add(presentation);
                    }
                }
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
