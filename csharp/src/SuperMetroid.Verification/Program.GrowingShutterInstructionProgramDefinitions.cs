using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Loads the retail cartridge and runs the growing-shutter mechanics and presentation verification suite.</summary>
    private static void VerifyGrowingShutterInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyGrowingShutterInstructionProgramDefinitions), () => VerifyGrowingShutterInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks real enemy execution against compiled growing-shutter instruction and presentation definitions.</summary>
    /// <param name="rom">Retail cartridge address space used as the native-data reference.</param>
    private static void VerifyGrowingShutterInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static |
            BindingFlags.NonPublic;
        Suite(nameof(VerifyGrowingShutterMechanicsMapping), () => VerifyGrowingShutterMechanicsMapping(rom));
        Suite(nameof(VerifyGrowingShutterPresentationMapping), () => VerifyGrowingShutterPresentationMapping());
        Suite(nameof(VerifyGrowingShutterProgramEntries), () => VerifyGrowingShutterProgramEntries());

        var guard = new GrowingShutterInstructionProgramReadGuard(rom);
        var enemies = new RoomEnemySystem();
        Type enemySystemType = typeof(RoomEnemySystem);
        enemySystemType.GetField("_bus", flags)!.SetValue(enemies, guard);
        var initialize = enemySystemType.GetMethod("InitializeGrowingShutter", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        var finishSection = enemySystemType.GetMethod("FinishGrowingShutterSection", flags)!
            .CreateDelegate<Action<RoomEnemySlot, GrowingShutterEnemyState, ushort, bool>>();
        MethodInfo process = enemySystemType.GetMethod("ProcessInstructions", flags)!;

        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = RoomEnemySystem.GrowingShutterDefinition;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        slot.CurrentInstruction = 0;
        slot.ExtraProperties = 0;
        slot.Parameter2 = 0;
        slot.YPosition = 0x0100;
        initialize(slot);
        GrowingShutterEnemyState state = enemies.GrowingShutterStates[0]!;

        object?[] processArguments =
            [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        for (int programIndex = 0;
             programIndex < GrowingShutterInstructionProgramDefinitions.ProgramCount;
             programIndex++)
        {
            ushort entry =
                GrowingShutterInstructionProgramDefinitions.ProgramEntryPoint(programIndex);
            AssertEqual(entry, slot.CurrentInstruction,
                $"growing-shutter stage {programIndex} installs compiled program identity");
            process.Invoke(enemies, processArguments);
            AssertEqual(unchecked((ushort)(entry + 4)), slot.CurrentInstruction,
                $"growing-shutter stage {programIndex} advances to terminal sleep");
            AssertEqual((ushort)1, slot.InstructionTimer,
                $"growing-shutter stage {programIndex} frame duration");
            process.Invoke(enemies, processArguments);
            AssertEqual(unchecked((ushort)(entry + 4)), slot.CurrentInstruction,
                $"growing-shutter stage {programIndex} terminal sleep remains installed");

            finishSection(
                slot,
                state,
                unchecked((ushort)(0x0110 + programIndex * 0x10)),
                false);
        }

        AssertEqual((ushort)4, state.GrowthLevel,
            "growing-shutter real section transitions complete all four stages");
        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "growing-shutter spritemap words use compiled visual selectors");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled growing-shutter mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => GrowingShutterInstructionProgramDefinitions.ReadMechanicsWord(0xe99a),
            "growing-shutter spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => GrowingShutterInstructionProgramDefinitions.ReadMechanicsWord(0xe9b0),
            "adjacent vertical-shutter program is rejected as growing-shutter mechanics");

        _ = ProbeGrowingShutterInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeGrowingShutterInstructionMechanicsAllocation();
        AssertTrue(checksum != 0,
            "growing-shutter allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed growing-shutter mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Growing-shutter instruction mechanics: eight compiled words, four real " +
            "stage programs, and four compiled visual selectors pass with mechanics " +
            "bytes forbidden.");
    }

    /// <summary>Checks the complete compiled mechanics-word domain and its values against native instruction data.</summary>
    /// <param name="rom">Cartridge address space containing the growing-shutter lists.</param>
    private static void VerifyGrowingShutterMechanicsMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses = [0xe998, 0xe99c, 0xe99e, 0xe9a2, 0xe9a4, 0xe9a8, 0xe9aa, 0xe9ae];
        AssertEqual(addresses.Length, GrowingShutterInstructionProgramDefinitionsTooling.MechanicsWordCount, "Growing shutter native mechanics count");
        var bytes = new HashSet<int>();
        for (int index = 0; index < addresses.Length; index++)
        {
            ushort address = addresses[index];
            ushort native = ReadGrowingShutterInstructionWord(rom, 0xa20000 | address);
            var word = GrowingShutterInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(address, word.Address, "Growing shutter native word address");
            AssertEqual(native, word.Value, "Growing shutter enumerated native word");
            AssertEqual(native, GrowingShutterInstructionProgramDefinitions.ReadMechanicsWord(address), "Growing shutter direct native word");
            bytes.Add(address);
            bytes.Add(address + 1);
        }
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            AssertEqual(bytes.Contains(address), GrowingShutterInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0xa20000 | address), "Growing shutter full byte ownership");
            AssertEqual(bytes.Contains(address), GrowingShutterInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0x1a20000 | address), "Growing shutter bank mask aliases");
            AssertTrue(!GrowingShutterInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0xa30000 | address), "Growing shutter rejects other bank");
        }
        var words = addresses.ToHashSet();
        for (int address = addresses[0] - 2; address <= addresses[^1] + 4; address++)
            if (!words.Contains((ushort)address))
                AssertThrows<InvalidDataException>(() => GrowingShutterInstructionProgramDefinitions.ReadMechanicsWord((ushort)address), "Growing shutter rejects visual words, odd addresses and adjacent programs");
        foreach (int index in new[] { int.MinValue, -1, addresses.Length, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => GrowingShutterInstructionProgramDefinitionsTooling.MechanicsWord(index), "Growing shutter mechanics bounds");
    }

    /// <summary>Checks the four visual operand positions and their membership bounds.</summary>
    private static void VerifyGrowingShutterPresentationMapping()
    {
        ushort[] addresses = [0xe99a, 0xe9a0, 0xe9a6, 0xe9ac];
        AssertEqual(addresses.Length, GrowingShutterInstructionProgramDefinitionsTooling.PresentationWordCount, "Growing shutter visual count");
        for (int index = 0; index < addresses.Length; index++)
            AssertEqual(addresses[index], GrowingShutterInstructionProgramDefinitionsTooling.PresentationWordAddress(index), "Growing shutter native visual position");
        var expected = addresses.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
            AssertEqual(expected.Contains((ushort)address), GrowingShutterInstructionProgramDefinitions.IsPresentationWord((ushort)address), "Growing shutter full visual membership domain");
        foreach (int index in new[] { int.MinValue, -1, addresses.Length, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => GrowingShutterInstructionProgramDefinitionsTooling.PresentationWordAddress(index), "Growing shutter visual bounds");
    }
    /// <summary>Checks each of the four native growth-stage entry pointers and rejects invalid stage indexes.</summary>
    private static void VerifyGrowingShutterProgramEntries()
    {
        ushort[] entries = [0xe998, 0xe99e, 0xe9a4, 0xe9aa];
        AssertEqual(entries.Length, GrowingShutterInstructionProgramDefinitions.ProgramCount, "growing shutter native stage count");
        for (int index = 0; index < entries.Length; index++)
            AssertEqual(entries[index], GrowingShutterInstructionProgramDefinitions.ProgramEntryPoint(index), "growing shutter native stage entry");
        foreach (int index in new[] { int.MinValue, -1, 4, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => GrowingShutterInstructionProgramDefinitions.ProgramEntryPoint(index), "growing shutter stage bounds");
    }
    /// <summary>Repeatedly reads compiled stage words to measure warmed lookup allocation behavior.</summary>
    /// <returns>A checksum that keeps the repeated mechanics reads observable.</returns>
    private static int ProbeGrowingShutterInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += GrowingShutterInstructionProgramDefinitions.ReadMechanicsWord(
                GrowingShutterInstructionProgramDefinitions.ProgramEntryPoint(index & 3));
        }
        return checksum;
    }

    /// <summary>Reads one little-endian instruction word from the supplied cartridge address space.</summary>
    /// <param name="bus">Cartridge address space containing the instruction bytes.</param>
    /// <param name="address">Address of the word's low byte.</param>
    /// <returns>The combined 16-bit instruction word.</returns>
    private static ushort ReadGrowingShutterInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    /// <summary>Guards production execution against mechanics rereads while recording accesses to presentation words.</summary>
    /// <param name="source">Underlying address space used after the guard checks each request.</param>
    private sealed class GrowingShutterInstructionProgramReadGuard(
        ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Presentation word addresses observed through cartridge reads.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>Number of production reads rejected for targeting compiled mechanics bytes.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes cartridge reads through the mechanics-byte guard and presentation observer.</summary>
        /// <param name="address">Cartridge address requested by production code.</param>
        /// <returns>The wrapped byte when the address is not forbidden.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects compiled mechanics reads, observes presentation operands, and forwards other reads.</summary>
        /// <param name="address">CPU bus address requested by production code.</param>
        /// <returns>The wrapped byte when the address is not forbidden.</returns>
        public byte ReadByte(int address)
        {
            if (GrowingShutterInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled growing-shutter mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa20000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < GrowingShutterInstructionProgramDefinitionsTooling.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        GrowingShutterInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ObservedPresentationWords.Add(presentation);
                    }
                }
            }

            return source.ReadByte(address);
        }

        /// <summary>Forwards a write to the underlying address space.</summary>
        /// <param name="address">CPU address receiving the byte.</param>
        /// <param name="value">Byte to store.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
