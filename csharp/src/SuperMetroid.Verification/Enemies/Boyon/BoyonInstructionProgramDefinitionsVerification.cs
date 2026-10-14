using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Boyon;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Enemies.Boyon;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Boyon idle and bounce instruction programs, selectors, and live callback effects.</summary>
internal static class BoyonInstructionProgramDefinitionsVerification
{
    /// <summary>Lists the ten bank-A2 visual operand addresses independently of the compiled catalog.</summary>
    private static ushort[] BoyonPresentationOracle() =>
        [0x86ad,0x86b1,0x86b5,0x86b9,0x86c5,0x86c9,0x86cd,0x86d1,0x86d5,0x86d9];

    /// <summary>Compares control words with ROM and checks byte ownership, aliases, and invalid addresses.</summary>
    private static void VerifyBoyonMechanicsMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses = [0x86a7,0x86a9,0x86ab,0x86af,0x86b3,0x86b7,0x86bb,0x86bd,
            0x86bf,0x86c1,0x86c3,0x86c7,0x86cb,0x86cf,0x86d3,0x86d7,0x86db,0x86dd];
        AssertEqual(addresses.Length, BoyonInstructionProgramDefinitionsTooling.MechanicsWordCount, "Boyon word count");
        var bytes = new HashSet<int>();
        for (int index = 0; index < addresses.Length; index++)
        {
            ushort address = addresses[index];
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa20000 | address));
            var actual = BoyonInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(address, actual.Address, "Boyon native word position");
            AssertEqual(expected, actual.Value, "Boyon native enumerated word");
            AssertEqual(expected, BoyonInstructionProgramDefinitions.ReadMechanicsWord(address), "Boyon direct native word");
            bytes.Add(address);
            bytes.Add(address + 1);
        }
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool expected = bytes.Contains(address);
            AssertEqual(expected, BoyonInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0xa20000 | address), "Boyon full bank byte ownership");
            AssertEqual(expected, BoyonInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0x1a20000 | address), "Boyon high-bit alias preserved");
            AssertTrue(!BoyonInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0xa30000 | address), "Boyon wrong bank rejected");
        }
        var words = addresses.ToHashSet();
        for (int address = 0x86a5; address <= 0x86e1; address++)
            if (!words.Contains((ushort)address))
                AssertThrows<InvalidDataException>(() => BoyonInstructionProgramDefinitions.ReadMechanicsWord((ushort)address),
                    "Boyon presentation, misaligned and adjacent words rejected");
        foreach (ushort address in new ushort[] { 0, 0x7fff, 0xffff })
            AssertThrows<InvalidDataException>(() => BoyonInstructionProgramDefinitions.ReadMechanicsWord(address), "Boyon far invalid word");
        foreach (int index in new[] { int.MinValue, -1, 18, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => BoyonInstructionProgramDefinitionsTooling.MechanicsWord(index), "Boyon mechanics ordinal bounds");
    }

    /// <summary>Checks the complete visual-operand membership domain and ordinal bounds.</summary>
    private static void VerifyBoyonPresentationAddresses()
    {
        ushort[] addresses = BoyonPresentationOracle();
        AssertEqual(addresses.Length, BoyonInstructionProgramDefinitionsTooling.PresentationWordCount, "Boyon visual count");
        for (int index = 0; index < addresses.Length; index++)
            AssertEqual(addresses[index], BoyonInstructionProgramDefinitionsTooling.PresentationWordAddress(index), "Boyon original visual position");
        var valid = addresses.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
            AssertEqual(valid.Contains((ushort)address), BoyonInstructionProgramDefinitions.IsPresentationWord((ushort)address), "Boyon full visual membership");
        foreach (int index in new[] { int.MinValue, -1, 10, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => BoyonInstructionProgramDefinitionsTooling.PresentationWordAddress(index), "Boyon presentation ordinal bounds");
    }

    /// <summary>Matches compiled Boyon frame pointers with ROM while rejecting nearby nonvisual words.</summary>
    private static void VerifyBoyonVisualSelectors(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses = BoyonPresentationOracle();
        foreach (ushort address in addresses)
        {
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa20000 | address));
            AssertEqual(expected, EnemySpritemapDefinitions.BoyonFrameAt(address), "Boyon native visual value");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0xa2, address, out ushort shared), "Boyon shared visual selection");
            AssertEqual(expected, shared, "Boyon shared native visual value");
        }
        var valid = addresses.ToHashSet();
        for (int address = 0x86a5; address <= 0x86e1; address++)
            if (!valid.Contains((ushort)address))
            {
                AssertThrows<InvalidDataException>(() => EnemySpritemapDefinitions.BoyonFrameAt((ushort)address), "Boyon invalid visual operand");
                AssertTrue(!CompiledEnemyVisualSelectors.TryGet(0xa2, (ushort)address, out ushort missing), "Boyon shared hole rejected");
                AssertEqual((ushort)0, missing, "Boyon shared miss clears output");
            }
        foreach (ushort address in new ushort[] { 0, 0x7fff, 0xffff })
            AssertThrows<InvalidDataException>(() => EnemySpritemapDefinitions.BoyonFrameAt(address), "Boyon distant visual rejected");
    }
    /// <summary>Loads the retail ROM and runs the Boyon instruction suite.</summary>
    internal static void VerifyBoyonInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyBoyonInstructionProgramDefinitions), () => VerifyBoyonInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks ROM mappings and executes idle and bouncing paths with source reads forbidden.</summary>
    internal static void VerifyBoyonInstructionProgramDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        Suite(nameof(VerifyBoyonMechanicsMapping), () => VerifyBoyonMechanicsMapping(rom));
        Suite(nameof(VerifyBoyonPresentationAddresses), () => VerifyBoyonPresentationAddresses());
        Suite(nameof(VerifyBoyonVisualSelectors), () => VerifyBoyonVisualSelectors(rom));

        var guard = NewBoyonInstructionProgramReadGuard(rom);
        RoomEnemySystem idleSystem = CreateBoyonProgramSystem(guard, out RoomEnemySlot idle);
        AssertEqual(BoyonInstructionProgramDefinitions.Idle, idle.CurrentInstruction,
            "Boyon initializer selects idle program");
        RunBoyonProgram(idleSystem, idle, frames: 50);
        AssertTrue(!idle.Properties.HasAny(EnemyProperties.ProcessOffScreen),
            "Boyon idle program disables off-screen processing");

        RoomEnemySystem bouncingSystem =
            CreateBoyonProgramSystem(guard, out RoomEnemySlot bouncing);
        BoyonEnemyState bouncingState = (bouncingSystem.BoyonStates[0] ?? throw new InvalidOperationException("Verification requires bouncingSystem.BoyonStates[0]."));
        var samus = new SamusState
        {
            Position = FixedPosition<RoomPixels>.FromWhole(bouncing.Position.Whole),
        };
        Action<RoomEnemySlot, BoyonEnemyState, SamusState?> runMain = BoyonBehavior.RunBoyonMain;
        runMain(bouncing, (bouncingState ?? throw new InvalidOperationException("Verification requires bouncingState.")), samus);
        runMain(bouncing, bouncingState, samus);
        AssertEqual(BoyonInstructionProgramDefinitions.Bouncing, bouncing.CurrentInstruction,
            "Boyon proximity path selects bouncing program");
        RunBoyonProgram(bouncingSystem, bouncing, frames: 40);
        AssertTrue(bouncing.Properties.HasAny(EnemyProperties.ProcessOffScreen),
            "Boyon bouncing program enables off-screen processing");
        AssertEqual((ushort)0x000e, (bouncingSystem.BoyonBehavior.LastBoyonSoundEffect ?? throw new InvalidOperationException("Verification requires bouncingSystem.BoyonBehavior.LastBoyonSoundEffect.")),
            "Boyon bouncing callback publishes native sound");
        AssertTrue(!bouncingState.BounceDisabled,
            "Boyon bouncing callback permits the movement arc");

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled Boyon mechanics and visual selector bytes");

        _ = ProbeBoyonInstructionMechanicsAllocation();
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeBoyonInstructionMechanicsAllocation();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        AssertTrue(checksum != 0, "Boyon allocation probe consumes live data");
        AssertEqual(0L, allocated,
            "warmed Boyon mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Boyon instruction mechanics: eighteen compiled words, idle and bouncing " +
            "production loops, bounce callback/property changes, and ten compiled " +
            "visual selectors pass with source bytes forbidden.");

        static RoomEnemySystem CreateBoyonProgramSystem(
            CartridgeReadGuard guard,
            out RoomEnemySlot slot)
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guard);
            Action<RoomEnemySlot> initialize = enemies.BoyonBehavior.InitializeBoyon;
            slot = enemies.SlotTable.Slots[0];
            slot.EnemyDefinitionPointer = EnemyDefinitionId.Boyon;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
            slot.Parameter1 = 0;
            slot.Parameter2 = 0x0040;
            slot.Position = slot.Position.WithWhole(new(0x0100, 0x0200));
            initialize(slot);
            return enemies;
        }

        static void RunBoyonProgram(
            RoomEnemySystem enemies,
            RoomEnemySlot slot,
            int frames)
        {
            object?[] arguments =
                [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
            for (int frame = 0; frame < frames; frame++)
                EnemyInstructionRunner.Process(enemies, arguments);
        }
    }

    /// <summary>Consumes repeated idle-word lookups for the warmed allocation check.</summary>
    private static int ProbeBoyonInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += BoyonInstructionProgramDefinitions.ReadMechanicsWord(
                BoyonInstructionProgramDefinitions.Idle);
        }
        return checksum;
    }

    /// <summary>Rejects reads from the compiled Boyon mechanics and visual selector ranges.</summary>
    private static CartridgeReadGuard NewBoyonInstructionProgramReadGuard(ISnesAddressSpace source)
    {
        static string Message(int address) => $"Production read compiled Boyon instruction byte ${address:X6}.";
        return new CartridgeReadGuard(source)
            .Forbid(BoyonInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte, Message)
            .ForbidWords(0xa20000, BoyonInstructionProgramDefinitionsTooling.PresentationWordCount, BoyonInstructionProgramDefinitionsTooling.PresentationWordAddress, Message);
    }

    /// <summary>Runs the Boyon instruction suite through the verification command dispatcher.</summary>
    [VerificationCommand("--boyon-instruction-mechanics")]
    internal static int BoyonInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyBoyonInstructionProgramDefinitions), () => VerifyBoyonInstructionProgramDefinitions());
        return 0;
    }
}
