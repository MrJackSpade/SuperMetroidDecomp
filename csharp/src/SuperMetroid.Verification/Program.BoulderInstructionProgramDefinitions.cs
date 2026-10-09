using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;

internal static partial class Program
{
    private static ushort[] BoulderPresentationOracle() =>
        [0x86a9,0x86ad,0x86b1,0x86b5,0x86b9,0x86bd,0x86c1,0x86c5,
         0x86cd,0x86d1,0x86d5,0x86d9,0x86dd,0x86e1,0x86e5,0x86e9];

    private static void VerifyBoulderMechanicsMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses =
            [0x86a7,0x86ab,0x86af,0x86b3,0x86b7,0x86bb,0x86bf,0x86c3,0x86c7,0x86c9,
             0x86cb,0x86cf,0x86d3,0x86d7,0x86db,0x86df,0x86e3,0x86e7,0x86eb,0x86ed];
        AssertEqual(addresses.Length, BoulderInstructionProgramDefinitionsTooling.MechanicsWordCount, "Boulder word count");
        var bytes = new HashSet<int>();
        for (int index = 0; index < addresses.Length; index++)
        {
            ushort address = addresses[index];
            ushort expected = ReadBoulderInstructionWord(rom, 0xa60000 | address);
            var actual = BoulderInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(address, actual.Address, "Boulder native word position");
            AssertEqual(expected, actual.Value, "Boulder native enumerated word");
            AssertEqual(expected, BoulderInstructionProgramDefinitions.ReadMechanicsWord(address), "Boulder native direct word");
            bytes.Add(address);
            bytes.Add(address + 1);
        }
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool expected = bytes.Contains(address);
            AssertEqual(expected, BoulderInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0xa60000 | address),
                "Boulder full bank byte ownership with odd word starts");
            AssertEqual(expected, BoulderInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0x1a60000 | address),
                "Boulder high-bit alias preserved");
            AssertTrue(!BoulderInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0xa70000 | address),
                "Boulder wrong bank rejected");
        }
        var words = addresses.ToHashSet();
        for (int address = 0x86a5; address <= 0x86f1; address++)
            if (!words.Contains((ushort)address))
                AssertThrows<InvalidDataException>(() => BoulderInstructionProgramDefinitions.ReadMechanicsWord((ushort)address),
                    "Boulder visual, misaligned and adjacent words rejected");
        foreach (ushort address in new ushort[] { 0, 0x7fff, 0xffff })
            AssertThrows<InvalidDataException>(() => BoulderInstructionProgramDefinitions.ReadMechanicsWord(address),
                "Boulder distant invalid word");
        foreach (int index in new[] { int.MinValue, -1, 20, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => BoulderInstructionProgramDefinitionsTooling.MechanicsWord(index),
                "Boulder mechanics ordinal bounds");
    }

    private static void VerifyBoulderPresentationAddresses()
    {
        ushort[] addresses = BoulderPresentationOracle();
        AssertEqual(addresses.Length, BoulderInstructionProgramDefinitionsTooling.PresentationWordCount, "Boulder visual count");
        for (int index = 0; index < addresses.Length; index++)
            AssertEqual(addresses[index], BoulderInstructionProgramDefinitionsTooling.PresentationWordAddress(index),
                "Boulder original visual position");
        var valid = addresses.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
            AssertEqual(valid.Contains((ushort)address), BoulderInstructionProgramDefinitions.IsPresentationWord((ushort)address),
                "Boulder full visual membership");
        foreach (int index in new[] { int.MinValue, -1, 16, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => BoulderInstructionProgramDefinitionsTooling.PresentationWordAddress(index),
                "Boulder presentation ordinal bounds");
    }

    private static void VerifyBoulderVisualSelectors(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses = BoulderPresentationOracle();
        foreach (ushort address in addresses)
        {
            ushort expected = ReadBoulderInstructionWord(rom, 0xa60000 | address);
            AssertEqual(expected, EnemySpritemapDefinitions.BoulderFrameAt(address), "Boulder native visual value");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0xa6, address, out ushort shared), "Boulder shared visual selection");
            AssertEqual(expected, shared, "Boulder shared native visual value");
        }
        var valid = addresses.ToHashSet();
        for (int address = 0x86a5; address <= 0x86f1; address++)
            if (!valid.Contains((ushort)address))
            {
                AssertThrows<InvalidDataException>(() => EnemySpritemapDefinitions.BoulderFrameAt((ushort)address),
                    "Boulder control, misaligned and adjacent visual operands rejected");
                AssertTrue(!CompiledEnemyVisualSelectors.TryGet(0xa6, (ushort)address, out ushort missing), "Boulder shared holes rejected");
                AssertEqual((ushort)0, missing, "Boulder shared miss clears output");
            }
        foreach (ushort address in new ushort[] { 0, 0x7fff, 0xffff })
            AssertThrows<InvalidDataException>(() => EnemySpritemapDefinitions.BoulderFrameAt(address), "Boulder far invalid visual");
    }

    private static void VerifyBoulderInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyBoulderInstructionProgramDefinitions), () => VerifyBoulderInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    private static void VerifyBoulderInstructionProgramDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        Suite(nameof(VerifyBoulderMechanicsMapping), () => VerifyBoulderMechanicsMapping(rom));
        Suite(nameof(VerifyBoulderPresentationAddresses), () => VerifyBoulderPresentationAddresses());
        Suite(nameof(VerifyBoulderVisualSelectors), () => VerifyBoulderVisualSelectors(rom));

        var guard = new BoulderInstructionProgramReadGuard(rom);
        Suite(nameof(VerifyBoulderInstructionProgram), () => VerifyBoulderInstructionProgram(
            guard,
            parameter1: 0x0100,
            BoulderInstructionProgramDefinitions.Left,
            "left"));
        Suite(nameof(VerifyBoulderInstructionProgram), () => VerifyBoulderInstructionProgram(
            guard,
            parameter1: 0x0000,
            BoulderInstructionProgramDefinitions.Right,
            "right"));

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled Boulder mechanics and visual bytes");

        _ = ProbeBoulderInstructionMechanicsAllocation();
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeBoulderInstructionMechanicsAllocation();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        AssertTrue(checksum != 0, "Boulder allocation probe consumes live data");
        AssertEqual(0L, allocated,
            "warmed Boulder mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Boulder instruction mechanics: twenty compiled words, both mirrored " +
            "production loops, and sixteen compiled visual selectors pass with source " +
            "bytes forbidden.");

        static void VerifyBoulderInstructionProgram(
            BoulderInstructionProgramReadGuard guard,
            ushort parameter1,
            ushort expectedProgram,
            string description)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            var initialize = typeof(RoomEnemySystem).GetMethod("InitializeBoulder", flags)!
                .CreateDelegate<Action<RoomEnemySlot>>(enemies);
            MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;

            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.BoulderDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa6 };
            slot.CurrentInstruction = 0x0008;
            slot.InstructionTimer = 1;
            slot.Parameter1 = parameter1;
            slot.Parameter2 = 0x0108;
            slot.XPosition = 0x0100;
            slot.YPosition = 0x0200;
            initialize(slot);

            AssertEqual(expectedProgram, slot.CurrentInstruction,
                $"Boulder {description} initializer program selection");
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0];

            // Eight eight-frame entries consume 64 actor frames. The margin crosses the
            // terminal goto and proves the production stream restarted its first frame.
            for (int frame = 0; frame < 70; frame++)
                process.Invoke(enemies, arguments);
            AssertEqual(unchecked((ushort)(expectedProgram + 4)), slot.CurrentInstruction,
                $"Boulder {description} program loops to its first frame");
        }
    }

    private static int ProbeBoulderInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += BoulderInstructionProgramDefinitions.ReadMechanicsWord(
                BoulderInstructionProgramDefinitions.Left);
        }
        return checksum;
    }

    private static ushort ReadBoulderInstructionWord(SuperMetroidAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class BoulderInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (BoulderInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                IsCompiledPresentationByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Boulder instruction byte ${address:X6}.");
            }

            return source.ReadByte(address);
        }

        private static bool IsCompiledPresentationByte(int address)
        {
            if ((address & 0xff0000) == 0xa60000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < BoulderInstructionProgramDefinitionsTooling.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        BoulderInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
