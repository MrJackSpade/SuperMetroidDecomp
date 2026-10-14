using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.FaceBlocks;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.FaceBlocks;
using SuperMetroid.Verification.Harness;

/// <summary>Checks face-block instruction mechanics and installed visual bindings against retail behavior.</summary>
internal static class BlueBrinstarFaceBlockInstructionProgramDefinitionsVerification
{
    /// <summary>Runs the retail-backed face-block mechanics and visual ownership checks as a named suite.</summary>
    internal static void VerifyBlueBrinstarFaceBlockInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyBlueBrinstarFaceBlockInstructionProgramDefinitions), () => VerifyBlueBrinstarFaceBlockInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Compares compiled control words and sprite operands with bank $A8, then executes initial and both Samus-side activation programs under a cartridge-read guard.</summary>
    internal static void VerifyBlueBrinstarFaceBlockInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        Suite(nameof(VerifyFaceBlockMechanicsMapping), () => VerifyFaceBlockMechanicsMapping(rom));
        Suite(nameof(VerifyFaceBlockPresentationMapping), () => VerifyFaceBlockPresentationMapping());
        Suite(nameof(VerifyFaceBlockVisualMapping), () => VerifyFaceBlockVisualMapping(rom));

        var guard = NewBlueBrinstarFaceBlockProgramReadGuard(rom);
        Suite(nameof(VerifyBlueBrinstarFaceBlockProgram), () => VerifyBlueBrinstarFaceBlockProgram(
            guard,
            parameter2: 0,
            samusX: null,
            BlueBrinstarFaceBlockInstructionProgramDefinitions.Initial,
            terminal: 0xe82c,
            frames: 4,
            "initial"));
        Suite(nameof(VerifyBlueBrinstarFaceBlockProgram), () => VerifyBlueBrinstarFaceBlockProgram(
            guard,
            parameter2: 0,
            samusX: 0x00e0,
            BlueBrinstarFaceBlockInstructionProgramDefinitions.SamusLeft,
            terminal: 0xe818,
            frames: 96,
            "Samus-left"));
        Suite(nameof(VerifyBlueBrinstarFaceBlockProgram), () => VerifyBlueBrinstarFaceBlockProgram(
            guard,
            parameter2: 1,
            samusX: 0x0120,
            BlueBrinstarFaceBlockInstructionProgramDefinitions.SamusRight,
            terminal: 0xe826,
            frames: 96,
            "Samus-right"));

        AssertEqual(0, guard.ObservedWords.Count,
            "face-block programs use compiled spritemap selectors");
        for (int index = 0;
             index < BlueBrinstarFaceBlockInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort address =
                BlueBrinstarFaceBlockInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertTrue(!guard.ObservedWords.Contains(address),
                $"production execution avoids face-block presentation word $A8:{address:X4}");
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
            "three production programs, both activation sides, and seven compiled " +
            "spritemap selectors pass without instruction or presentation ROM reads.");

        // Initialize one face block, optionally activate it from a Samus side, and advance to sleep.
        static void VerifyBlueBrinstarFaceBlockProgram(
            CartridgeReadGuard guard,
            ushort parameter2,
            ushort? samusX,
            ushort expectedProgram,
            ushort terminal,
            int frames,
            string description)
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guard);
            Action<RoomEnemySlot, SamusState?> initialize = enemies.BlueBrinstarFaceBlockBehavior.InitializeBlueBrinstarFaceBlock;
            Action<RoomEnemySlot, BlueBrinstarFaceBlockEnemyState, SamusState?> runMain = enemies.BlueBrinstarFaceBlockBehavior.RunBlueBrinstarFaceBlockMain;

            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.EnemyDefinitionPointer = EnemyDefinitionId.FaceBlock;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
            slot.Parameter1 = 0x0040;
            slot.Parameter2 = parameter2;
            slot.Position = slot.Position.WithWhole(new(0x0100, 0x0100));
            initialize(slot, null);

            if (samusX is ushort x)
            {
                var samus = new SamusState
                {
                    CollectedItems = 0x0004,
                    Position = FixedPosition<RoomPixels>.FromWhole(new(x, slot.Position.Y.Whole)),
                };
                runMain(slot, (enemies.BlueBrinstarFaceBlockStates[0] ?? throw new InvalidOperationException("Verification requires enemies.BlueBrinstarFaceBlockStates[0].")), samus);
                AssertTrue((enemies.BlueBrinstarFaceBlockStates[0] ?? throw new InvalidOperationException("Verification requires enemies.BlueBrinstarFaceBlockStates[0].")).Activated,
                    $"face-block {description} path activates");
            }

            AssertEqual(expectedProgram, slot.CurrentInstruction,
                $"face-block {description} program selection");
            object?[] arguments =
                [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
            for (int frame = 0; frame < frames; frame++)
                EnemyInstructionRunner.Process(enemies, arguments);
            AssertEqual(terminal, slot.CurrentInstruction,
                $"face-block {description} terminal sleep");
        }
    }

    /// <summary>Checks all ten control words against retail bytes and proves the mechanics-byte predicate owns only those bank-$A8 bytes.</summary>
    private static void VerifyFaceBlockMechanicsMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] expected = [0xe80c,0xe810,0xe814,0xe818,0xe81a,0xe81e,0xe822,0xe826,0xe828,0xe82c];
        AssertEqual(expected.Length, BlueBrinstarFaceBlockInstructionProgramDefinitionsTooling.MechanicsWordCount, "face-block native control count");
        var bytes = new HashSet<int>();
        for (int i = 0; i < expected.Length; i++)
        {
            var actual = BlueBrinstarFaceBlockInstructionProgramDefinitionsTooling.MechanicsWord(i);
            AssertEqual(expected[i], actual.Address, "face-block native control position");
            ushort native = ReadBlueBrinstarFaceBlockWord(rom, 0xa80000 | expected[i]);
            AssertEqual(native, actual.Value, "face-block enumerated native control");
            AssertEqual(native, BlueBrinstarFaceBlockInstructionProgramDefinitions.ReadMechanicsWord(expected[i]), "face-block direct native control");
            bytes.Add(expected[i]); bytes.Add(expected[i] + 1);
        }
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            AssertEqual(bytes.Contains(address), BlueBrinstarFaceBlockInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0xa80000 | address), "face-block full byte ownership");
            AssertEqual(bytes.Contains(address), BlueBrinstarFaceBlockInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0x1a80000 | address), "face-block high-bit alias");
            AssertTrue(!BlueBrinstarFaceBlockInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0xa90000 | address), "face-block other bank rejected");
        }
        var words = expected.ToHashSet();
        for (int address = 0xe80a; address <= 0xe830; address++)
            if (!words.Contains((ushort)address))
                AssertThrows<InvalidDataException>(() => BlueBrinstarFaceBlockInstructionProgramDefinitions.ReadMechanicsWord((ushort)address), "face-block rejects odd, visual and adjacent words");
        foreach (ushort address in new ushort[] {0,0x7fff,0xffff})
            AssertThrows<InvalidDataException>(() => BlueBrinstarFaceBlockInstructionProgramDefinitions.ReadMechanicsWord(address), "face-block distant invalid controls");
        foreach (int index in new[] {int.MinValue,-1,10,int.MaxValue})
            AssertThrows<IndexOutOfRangeException>(() => BlueBrinstarFaceBlockInstructionProgramDefinitionsTooling.MechanicsWord(index), "face-block control ordinal bounds");
    }
    /// <summary>Checks the seven interleaved spritemap operand locations and rejects every other word address as a presentation operand.</summary>
    private static void VerifyFaceBlockPresentationMapping()
    {
        ushort[] expected = [0xe80e,0xe812,0xe816,0xe81c,0xe820,0xe824,0xe82a];
        AssertEqual(expected.Length, BlueBrinstarFaceBlockInstructionProgramDefinitionsTooling.PresentationWordCount, "face-block native operand count");
        for (int i = 0; i < expected.Length; i++)
            AssertEqual(expected[i], BlueBrinstarFaceBlockInstructionProgramDefinitionsTooling.PresentationWordAddress(i), "face-block native operand position");
        var words = expected.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
            AssertEqual(words.Contains((ushort)address), BlueBrinstarFaceBlockInstructionProgramDefinitions.IsPresentationWord((ushort)address), "face-block full operand membership");
        foreach (int index in new[] {int.MinValue,-1,7,int.MaxValue})
            AssertThrows<IndexOutOfRangeException>(() => BlueBrinstarFaceBlockInstructionProgramDefinitionsTooling.PresentationWordAddress(index), "face-block operand ordinal bounds");
    }
    /// <summary>Checks compiled sprite selectors and exported four-piece frame identities against the native face-block program and frame table.</summary>
    private static void VerifyFaceBlockVisualMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] operands = [0xe80e,0xe812,0xe816,0xe81c,0xe820,0xe824,0xe82a];
        foreach (ushort operand in operands)
        {
            ushort native = ReadBlueBrinstarFaceBlockWord(rom, 0xa80000 | operand);
            AssertEqual(native, BlueBrinstarFaceBlockVisualDefinitions.FrameAt(operand), "face-block native sprite pointer");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0xa8, operand, out ushort shared), "face-block shared selector found");
            AssertEqual(native, shared, "face-block shared pointer");
            AssertTrue(CompiledEnemyVisualSelectors.IsCalculatedSelector(0xa80000 | operand), "face-block excluded from literal regeneration");
        }
        ushort[] pointers = [0xe92c,0xe942,0xe958,0xe96e,0xe984];
        string[] names = ["face_block_neutral","face_block_samus_left_1","face_block_samus_left_2","face_block_samus_right_1","face_block_samus_right_2"];
        var exported = BlueBrinstarFaceBlockVisualDefinitions.Frames();
        AssertEqual(pointers.Length, exported.Length, "face-block export count");
        for (int i = 0; i < pointers.Length; i++)
        {
            AssertEqual((ushort)4, ReadBlueBrinstarFaceBlockWord(rom, 0xa80000 | pointers[i]), "face-block native four-piece map");
            AssertEqual(pointers[i], exported[i].Pointer, "face-block export pointer order");
            AssertEqual(names[i], exported[i].Name, "face-block stable export name");
            AssertEqual((byte)0xa8, exported[i].Bank, "face-block export bank");
        }
        var known = operands.ToHashSet();
        for (int address = 0xe80a; address <= 0xe830; address++)
            if (!known.Contains((ushort)address))
            {
                AssertThrows<InvalidDataException>(() => BlueBrinstarFaceBlockVisualDefinitions.FrameAt((ushort)address), "face-block invalid visual operand");
                AssertTrue(!CompiledEnemyVisualSelectors.TryGet(0xa8,(ushort)address,out ushort missing), "face-block shared holes rejected");
                AssertEqual((ushort)0, missing, "face-block missing output cleared");
            }
        foreach (ushort address in new ushort[] {0,0x7fff,0xffff})
            AssertThrows<InvalidDataException>(() => BlueBrinstarFaceBlockVisualDefinitions.FrameAt(address), "face-block distant invalid operand");
    }
    /// <summary>Repeatedly consumes the initial compiled control word so the caller can measure warmed lookup allocation.</summary>
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

    /// <summary>Reads a retail bank-$A8 word solely as the oracle for compiled face-block definitions.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Blue Brinstar face block words")]
    internal static ushort ReadBlueBrinstarFaceBlockWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        CartridgeImportSource.Require(bus).ReadCartridgeWord(address);

    /// <summary>Forbids production reads of compiled control bytes while recording any presentation-word reads during instruction execution.</summary>
    private static CartridgeReadGuard NewBlueBrinstarFaceBlockProgramReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(BlueBrinstarFaceBlockInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled face-block mechanics byte ${address:X6}.")
            .ObserveWords(0xa80000,
                BlueBrinstarFaceBlockInstructionProgramDefinitionsTooling.PresentationWordCount,
                BlueBrinstarFaceBlockInstructionProgramDefinitionsTooling.PresentationWordAddress);

    /// <summary>Exposes the face-block compiled-mechanics verification through its dedicated command-line switch.</summary>
    [VerificationCommand("--blue-brinstar-face-block-instruction-mechanics")]
    internal static int BlueBrinstarFaceBlockInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyBlueBrinstarFaceBlockInstructionProgramDefinitions), () => VerifyBlueBrinstarFaceBlockInstructionProgramDefinitions());
        return 0;
    }
}
