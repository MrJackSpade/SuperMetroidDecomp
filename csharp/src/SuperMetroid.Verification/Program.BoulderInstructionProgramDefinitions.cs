using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using static SuiteTimingVerification;

/// <summary>Represents boulder instruction program definitions verification.</summary>
internal static class BoulderInstructionProgramDefinitionsVerification
{
    /// <summary>Boulder presentation oracle.</summary>
    private static ushort[] BoulderPresentationOracle() =>
        [0x86a9,0x86ad,0x86b1,0x86b5,0x86b9,0x86bd,0x86c1,0x86c5,
         0x86cd,0x86d1,0x86d5,0x86d9,0x86dd,0x86e1,0x86e5,0x86e9];

    /// <summary>Verifies boulder mechanics mapping.</summary>
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
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa60000 | address));
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

    /// <summary>Verifies boulder presentation addresses.</summary>
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

    /// <summary>Verifies boulder visual selectors.</summary>
    private static void VerifyBoulderVisualSelectors(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses = BoulderPresentationOracle();
        foreach (ushort address in addresses)
        {
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa60000 | address));
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

    /// <summary>Verifies boulder instruction program definitions.</summary>
    internal static void VerifyBoulderInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyBoulderInstructionProgramDefinitions), () => VerifyBoulderInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Verifies boulder instruction program definitions.</summary>
    internal static void VerifyBoulderInstructionProgramDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        Suite(nameof(VerifyBoulderMechanicsMapping), () => VerifyBoulderMechanicsMapping(rom));
        Suite(nameof(VerifyBoulderPresentationAddresses), () => VerifyBoulderPresentationAddresses());
        Suite(nameof(VerifyBoulderVisualSelectors), () => VerifyBoulderVisualSelectors(rom));

        var guard = NewBoulderInstructionProgramReadGuard(rom);
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
            CartridgeReadGuard guard,
            ushort parameter1,
            ushort expectedProgram,
            string description)
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guard);
            Action<RoomEnemySlot> initialize = enemies.BoulderBehavior.InitializeBoulder;

            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.EnemyDefinitionPointer = EnemyDefinitionId.Boulder;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa6 };
            slot.CurrentInstruction = 0x0008;
            slot.InstructionTimer = 1;
            slot.Parameter1 = parameter1;
            slot.Parameter2 = 0x0108;
            slot.Position = slot.Position.WithWhole(new(0x0100, 0x0200));
            initialize(slot);

            AssertEqual(expectedProgram, slot.CurrentInstruction,
                $"Boulder {description} initializer program selection");
            object?[] arguments =
                [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];

            // Eight eight-frame entries consume 64 actor frames. The margin crosses the
            // terminal goto and proves the production stream restarted its first frame.
            for (int frame = 0; frame < 70; frame++)
                EnemyInstructionRunner.Process(enemies, arguments);
            AssertEqual(unchecked((ushort)(expectedProgram + 4)), slot.CurrentInstruction,
                $"Boulder {description} program loops to its first frame");
        }
    }

    /// <summary>Probes boulder instruction mechanics allocation.</summary>
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

    /// <summary>Creates boulder instruction program read guard.</summary>
    private static CartridgeReadGuard NewBoulderInstructionProgramReadGuard(ISnesAddressSpace source)
    {
        static string Message(int address) => $"Production read compiled Boulder instruction byte ${address:X6}.";
        return new CartridgeReadGuard(source)
            .Forbid(BoulderInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte, Message)
            .ForbidWords(0xa60000, BoulderInstructionProgramDefinitionsTooling.PresentationWordCount, BoulderInstructionProgramDefinitionsTooling.PresentationWordAddress, Message);
    }

    /// <summary>Boulder instruction mechanics command.</summary>
    [VerificationCommand("--boulder-instruction-mechanics")]
    internal static int BoulderInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyBoulderInstructionProgramDefinitions), () => VerifyBoulderInstructionProgramDefinitions());
        return 0;
    }
}
