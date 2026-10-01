using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyZoaInstructionProgramDefinitions()
    {
        VerifyZoaInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyZoaInstructionProgramDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        VerifyZoaMechanicsDispatch(rom);
        VerifyZoaPresentationPositions(rom);

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

        AssertEqual(0, guard.ForbiddenPresentationReadAttempts,
            "Zoa production programs never read installed visual selectors");
        for (int index = 0;
             index < ZoaInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = ZoaInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertEqual(ReadZoaInstructionWord(rom, 0xa30000 | address),
                EnemySpritemapDefinitions.ZoaFrameAt(address),
                $"compiled Zoa frame selection $A3:{address:X4}");
        }
        AssertThrows<InvalidDataException>(
            () => EnemySpritemapDefinitions.ZoaFrameAt(0xb40d),
            "unlisted Zoa frame selector fails loudly");
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
            "three speed callbacks per shooting direction pass with all twelve " +
            "visual-selector and mechanics source words forbidden.");

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

    private static void VerifyZoaMechanicsDispatch(SuperMetroidAddressSpace rom)
    {
        // Independent native word positions from bank_A3.asm, not the enumerator.
        ushort[] addresses = [0xb3c1, 0xb3c3, 0xb3c7, 0xb3c9, 0xb3cd, 0xb3cf, 0xb3d3,
            0xb3d5, 0xb3d7, 0xb3db, 0xb3df, 0xb3e3, 0xb3e5, 0xb3e7, 0xb3e9, 0xb3ed,
            0xb3ef, 0xb3f3, 0xb3f5, 0xb3f9, 0xb3fb, 0xb3fd, 0xb401, 0xb405, 0xb409, 0xb40b];
        AssertEqual(addresses.Length, ZoaInstructionProgramDefinitions.MechanicsWordCount, "Zoa mechanics count");
        for (int index = 0; index < addresses.Length; index++)
        {
            ushort address = addresses[index];
            ushort original = ReadZoaInstructionWord(rom, 0xa30000 | address);
            AssertEqual(original, ZoaInstructionProgramDefinitions.ReadMechanicsWord(address), "Zoa original control word");
            AssertEqual(new ZoaInstructionMechanicsWord(address, original),
                ZoaInstructionProgramDefinitions.MechanicsWord(index), "Zoa ordered control enumeration");
        }
        for (int address = 0xb3c0; address <= 0xb40e; address++)
        {
            bool isWord = addresses.Contains((ushort)address);
            bool isByte = isWord || addresses.Contains((ushort)(address - 1));
            AssertEqual(isByte, ZoaInstructionProgramDefinitions.IsCompiledMechanicsByte(0xa30000 | address),
                "Zoa mechanics byte ownership");
            AssertEqual(false, ZoaInstructionProgramDefinitions.IsCompiledMechanicsByte(0xa40000 | address),
                "Zoa mechanics bank ownership");
            if (!isWord) AssertThrows<InvalidDataException>(() =>
                ZoaInstructionProgramDefinitions.ReadMechanicsWord((ushort)address), "Zoa excludes noncontrol word");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 26, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => ZoaInstructionProgramDefinitions.MechanicsWord(invalid),
                "Zoa mechanics enumeration bounds");
    }

    private static void VerifyZoaPresentationPositions(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses = [0xb3c5, 0xb3cb, 0xb3d1, 0xb3d9, 0xb3dd, 0xb3e1,
            0xb3eb, 0xb3f1, 0xb3f7, 0xb3ff, 0xb403, 0xb407];
        AssertEqual(addresses.Length, ZoaInstructionProgramDefinitions.PresentationWordCount, "Zoa presentation count");
        for (int index = 0; index < addresses.Length; index++)
        {
            ushort address = ZoaInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertEqual(addresses[index], address, "Zoa native presentation position");
            AssertEqual(ReadZoaInstructionWord(rom, 0xa30000 | addresses[index]),
                ReadZoaInstructionWord(rom, 0xa30000 | address), "Zoa original presentation word at position");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 12, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => ZoaInstructionProgramDefinitions.PresentationWordAddress(invalid),
                "Zoa presentation enumeration bounds");
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
        ISnesAddressSpace, IImportCartridgeSource
    {
        internal int ForbiddenPresentationReadAttempts { get; private set; }
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

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
                        ForbiddenPresentationReadAttempts++;
                        throw new InvalidOperationException(
                            $"Production read installed Zoa selector ${address:X6}.");
                    }
                }
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
