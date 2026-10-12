using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using static AssertionsVerification;
using static SuiteTimingVerification;

internal static class AtomicMovementDefinitionsVerification
{
    private static void VerifyAtomicInitialProgramSelection(SuperMetroidAddressSpace rom)
    {
        for (ushort selector = 0; selector < 4; selector++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa80000 | ((ushort)(0xe380 + 2 * selector))),
                AtomicMovementDefinitions.InitialInstructionList(selector), "Atomic native direction program");
        foreach (ushort selector in new ushort[] { 4, 5, 0x7fff, 0xffff })
            AssertThrows<InvalidDataException>(() => AtomicMovementDefinitions.InitialInstructionList(selector),
                "Atomic invalid direction selector");
    }

    private static void VerifyAtomicMechanicsMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses =
        [
            0xe310, 0xe314, 0xe318, 0xe31c, 0xe320, 0xe324, 0xe328, 0xe32a,
            0xe32c, 0xe330, 0xe334, 0xe338, 0xe33c, 0xe340, 0xe344, 0xe346,
            0xe348, 0xe34c, 0xe350, 0xe354, 0xe358, 0xe35c, 0xe360, 0xe362,
            0xe364, 0xe368, 0xe36c, 0xe370, 0xe374, 0xe378, 0xe37c, 0xe37e,
        ];
        AssertEqual(addresses.Length, AtomicInstructionProgramDefinitionsTooling.MechanicsWordCount,
            "Atomic mechanics count");
        var ownedBytes = new HashSet<int>();
        for (int index = 0; index < addresses.Length; index++)
        {
            ushort address = addresses[index];
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa80000 | address);
            var actual = AtomicInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(address, actual.Address, "Atomic native word enumeration");
            AssertEqual(expected, actual.Value, "Atomic enumerated native value");
            AssertEqual(expected, AtomicInstructionProgramDefinitions.ReadMechanicsWord(address),
                "Atomic direct native word");
            ownedBytes.Add(address);
            ownedBytes.Add(address + 1);
        }
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool expected = ownedBytes.Contains(address);
            AssertEqual(expected, AtomicInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0xa80000 | address),
                "Atomic full bank ownership");
            AssertEqual(expected, AtomicInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0x1a80000 | address),
                "Atomic high address bits remain masked");
            AssertTrue(!AtomicInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0xa70000 | address),
                "Atomic wrong bank rejected");
        }
        var words = addresses.ToHashSet();
        for (int address = 0xe30e; address <= 0xe382; address++)
            if (!words.Contains((ushort)address))
                AssertThrows<InvalidDataException>(
                    () => AtomicInstructionProgramDefinitions.ReadMechanicsWord((ushort)address),
                    "Atomic visual operands, odd bytes and adjacent words rejected");
        foreach (ushort address in new ushort[] { 0, 0x7fff, 0xffff })
            AssertThrows<InvalidDataException>(() => AtomicInstructionProgramDefinitions.ReadMechanicsWord(address),
                "Atomic distant invalid word");
        foreach (int index in new[] { int.MinValue, -1, 32, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => AtomicInstructionProgramDefinitionsTooling.MechanicsWord(index),
                "Atomic mechanics ordinal bounds");
    }

    private static ushort[] AtomicPresentationAddressOracle() =>
        [
            0xe312, 0xe316, 0xe31a, 0xe31e, 0xe322, 0xe326,
            0xe32e, 0xe332, 0xe336, 0xe33a, 0xe33e, 0xe342,
            0xe34a, 0xe34e, 0xe352, 0xe356, 0xe35a, 0xe35e,
            0xe366, 0xe36a, 0xe36e, 0xe372, 0xe376, 0xe37a,
        ];

    private static void VerifyAtomicPresentationAddresses()
    {
        ushort[] expected = AtomicPresentationAddressOracle();
        AssertEqual(expected.Length, AtomicInstructionProgramDefinitionsTooling.PresentationWordCount,
            "Atomic presentation operand count");
        for (int index = 0; index < expected.Length; index++)
            AssertEqual(expected[index], AtomicInstructionProgramDefinitionsTooling.PresentationWordAddress(index),
                "Atomic native presentation operand position");
        var words = expected.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
            AssertEqual(words.Contains((ushort)address), AtomicInstructionProgramDefinitions.IsPresentationWord((ushort)address),
                "Atomic full presentation membership domain");
        foreach (int index in new[] { int.MinValue, -1, 24, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => AtomicInstructionProgramDefinitionsTooling.PresentationWordAddress(index),
                "Atomic presentation ordinal bounds");
    }

    private static void VerifyAtomicVisualSelectors(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses = AtomicPresentationAddressOracle();
        foreach (ushort address in addresses)
        {
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa80000 | address),
                EnemySpritemapDefinitions.AtomicFrameAt(address),
                "Atomic native visual operand value");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0xa8, address, out ushort shared),
                "Atomic shared calculated visual selection");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa80000 | address), shared, "Atomic shared native visual value");
        }
        var valid = addresses.ToHashSet();
        for (int address = 0xe30e; address <= 0xe382; address++)
            if (!valid.Contains((ushort)address))
            {
                AssertThrows<InvalidDataException>(() => EnemySpritemapDefinitions.AtomicFrameAt((ushort)address),
                    "Atomic mechanics words, odd bytes and boundaries reject visual selection");
                AssertTrue(!CompiledEnemyVisualSelectors.TryGet(0xa8, (ushort)address, out ushort missing),
                    "Atomic shared selection rejects holes");
                AssertEqual((ushort)0, missing, "Atomic missing shared result clears output");
            }
        foreach (ushort address in new ushort[] { 0, 0x7fff, 0xffff })
            AssertThrows<InvalidDataException>(() => EnemySpritemapDefinitions.AtomicFrameAt(address),
                "Atomic distant invalid visual operand");
    }

    [CpuBusBoundary("exact bus test: retail ROM oracle for atomic movement definitions")]
    internal static void VerifyAtomicMovementDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyAtomicInitialProgramSelection), () => VerifyAtomicInitialProgramSelection(rom));
        Suite(nameof(VerifyAtomicMechanicsMapping), () => VerifyAtomicMechanicsMapping(rom));
        Suite(nameof(VerifyAtomicPresentationAddresses), () => VerifyAtomicPresentationAddresses());
        Suite(nameof(VerifyAtomicVisualSelectors), () => VerifyAtomicVisualSelectors(rom));
        const int instructionTable = 0xa8e380;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(NewAtomicInstructionReadGuard(new TestAddressSpace()));
        Action<RoomEnemySlot> initialize = enemies.AtomicBehavior.InitializeAtomic;
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];

        for (ushort selector = 0; selector < 4; selector++)
        {
            int address = instructionTable + selector * 2;
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);

            slot.Parameter1 = selector;
            slot.Parameter2 = (ushort)(selector * 7);
            initialize(slot);
            AssertEqual(native, slot.CurrentInstruction,
                $"Atomic initializer instruction selector {selector}");

            AtomicEnemyState state = enemies.AtomicStates[0]!;
            ushort speedOffset = (ushort)(slot.Parameter2 * 8);
            (short positiveWhole, ushort positiveFraction) =
                EnemyLinearSpeedDefinitions.Read(speedOffset);
            (short negativeWhole, ushort negativeFraction) =
                EnemyLinearSpeedDefinitions.Read((ushort)(speedOffset + 4));
            AssertEqual(unchecked((ushort)positiveWhole), state.SpeedWhole,
                $"Atomic positive whole speed {selector}");
            AssertEqual(positiveFraction, state.SpeedFraction,
                $"Atomic positive fractional speed {selector}");
            AssertEqual(unchecked((ushort)negativeWhole), state.NegativeSpeedWhole,
                $"Atomic negative whole speed {selector}");
            AssertEqual(negativeFraction, state.NegativeSpeedFraction,
                $"Atomic negative fractional speed {selector}");
        }

        var programGuard = NewAtomicProgramReadGuard(rom);
        ushort[] entries =
        [
            AtomicInstructionProgramDefinitions.UpRight,
            AtomicInstructionProgramDefinitions.UpLeft,
            AtomicInstructionProgramDefinitions.DownLeft,
            AtomicInstructionProgramDefinitions.DownRight,
        ];
        foreach (ushort entry in entries)
        {
            var programSystem = new RoomEnemySystem();
            programSystem.BindFixtureBus(programGuard);
            RoomEnemySlot programSlot = programSystem.SlotTable.Slots[0];
            programSlot.EnemyDefinitionPointer = EnemyDefinitionId.Atomic;
            programSlot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
            programSlot.CurrentInstruction = entry;
            programSlot.InstructionTimer = 1;
            object?[] arguments =
                [programSlot, null, null, Position16<RoomPixels>.Zero, (ushort)0];

            // The six eight-frame entries total 48 frames; the margin proves the terminal
            // goto restarts the production stream rather than merely reaching its target.
            for (int frame = 0; frame < 60; frame++)
                EnemyInstructionRunner.Process(programSystem, arguments);
        }

        AssertEqual(0, programGuard.ForbiddenReadAttempts,
            "production execution avoids compiled Atomic mechanics and visual bytes");

        _ = AtomicInstructionProgramDefinitions.ReadMechanicsWord(
            AtomicInstructionProgramDefinitions.UpRight);
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += AtomicInstructionProgramDefinitions.ReadMechanicsWord(
                AtomicInstructionProgramDefinitions.UpRight);
        }
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        AssertTrue(checksum != 0, "Atomic allocation probe consumes live data");
        AssertEqual(0L, allocated,
            "warmed Atomic mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Atomic movement definitions: four native selectors and production initializers, " +
            "32 compiled instruction words, and four complete loops pass with mechanics " +
            "and 24 visual-selector source reads forbidden.");
    }

    private static CartridgeReadGuard NewAtomicInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address is >= 0xa8e380 and < 0xa8e388,
            address => $"Atomic initializer attempted migrated instruction read ${address:X6}.");

    private static CartridgeReadGuard NewAtomicProgramReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(AtomicInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Atomic instruction byte ${address:X6}.")
            .ForbidWords(0xa80000, AtomicInstructionProgramDefinitionsTooling.PresentationWordCount,
                AtomicInstructionProgramDefinitionsTooling.PresentationWordAddress,
                address => $"Production read compiled Atomic instruction byte ${address:X6}.");

    [VerificationCommand("--atomic-instruction-mechanics")]
    internal static int AtomicInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyAtomicMovementDefinitions), () => VerifyAtomicMovementDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
