using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Waver;
using SuperMetroid.Core.Programs;

/// <summary>Checks Waver steady and spinning programs, completion callbacks, and compiled-only playback.</summary>
internal static class WaverInstructionProgramDefinitionsVerification
{
    /// <summary>Loads retail data and runs the Waver instruction program suite.</summary>
    internal static void VerifyWaverInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyWaverInstructionProgramDefinitions), () => VerifyWaverInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks control words, steady and spinning programs, completion callbacks, and forbidden ROM reads.</summary>
    internal static void VerifyWaverInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        for (int index = 0;
             index < WaverInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                WaverInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa30000 | definition.Address)),
                $"Waver instruction mechanics word $A3:{definition.Address:X4}");
        }

        var guard = NewWaverInstructionProgramReadGuard(rom);
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
            WaverEnemyState state = (enemies.WaverStates[0] ?? throw new InvalidOperationException("Verification requires enemies.WaverStates[0]."));
            AssertEqual(WaverAnimationDefinitions.InstructionList(selector),
                slot.CurrentInstruction, $"Waver installs {selector} program");
            RunWaverProgram(enemies, slot, frames);
            AssertEqual(terminal, slot.CurrentInstruction,
                $"Waver {selector} program sleeps at native terminal command");
            AssertEqual(selector.HasFlag(WaverAnimationSelector.Spinning),
                (state ?? throw new InvalidOperationException("Verification requires state.")).SpinFinished, $"Waver {selector} spin-completion callback");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Waver production programs never read installed visual selectors");
        for (int index = 0;
             index < WaverInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort address = WaverInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa30000 | address)),
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
            CartridgeReadGuard guard,
            WaverAnimationSelector selector,
            out RoomEnemySlot slot)
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guard);
            Action<RoomEnemySlot> initialize = enemies.WaverBehavior.InitializeWaver;
            Action<RoomEnemySlot, WaverEnemyState> install = WaverBehavior.SetWaverInstructionList;

            slot = enemies.SlotTable.Slots[0];
            slot.EnemyDefinitionPointer = EnemyDefinitionId.Waver;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
            initialize(slot);
            WaverEnemyState state = (enemies.WaverStates[0] ?? throw new InvalidOperationException("Verification requires enemies.WaverStates[0]."));
            (state ?? throw new InvalidOperationException("Verification requires state.")).CurrentInstructionListIndex = selector == WaverAnimationSelector.None
                ? WaverAnimationSelector.FacingRight
                : WaverAnimationSelector.None;
            state.RequestedInstructionListIndex = selector;
            state.SpinFinished = false;
            install(slot, state);
            return enemies;
        }

        static void RunWaverProgram(RoomEnemySystem enemies, RoomEnemySlot slot, int frames)
        {
            object?[] arguments =
                [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
            for (int frame = 0; frame < frames; frame++)
                EnemyInstructionRunner.Process(enemies, arguments);
        }
    }

    /// <summary>Repeatedly resolves a steady-list word to measure warmed lookup allocation.</summary>
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

    /// <summary>Forbids Waver mechanics bytes and installed visual-selector words during production playback.</summary>
    private static CartridgeReadGuard NewWaverInstructionProgramReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(WaverInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Waver mechanics byte ${address:X6}.")
            .ForbidWords(0xa30000, WaverInstructionProgramDefinitionsTooling.PresentationWordCount, WaverInstructionProgramDefinitionsTooling.PresentationWordAddress,
                address => $"Production read installed Waver selector ${address:X6}.");

    /// <summary>Runs the Waver instruction mechanics verification command.</summary>
    [VerificationCommand("--waver-instruction-mechanics")]
    internal static int WaverInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyWaverInstructionProgramDefinitions), () => VerifyWaverInstructionProgramDefinitions());
        return 0;
    }
}
