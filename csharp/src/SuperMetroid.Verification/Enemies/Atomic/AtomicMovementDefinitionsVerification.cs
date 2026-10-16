using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Atomic;
using SuperMetroid.Core.Enemies.Atomic.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.Atomic;
using SuperMetroid.Verification.Harness;

/// <summary>Retail-word and guarded-execution checks for Atomic movement and animation selectors.</summary>
internal static class AtomicMovementDefinitionsVerification
{
    /// <summary>Matches four initial direction selectors against the retail table and rejects out-of-range values.</summary>
    private static void VerifyAtomicInitialProgramSelection(SuperMetroidAddressSpace rom)
    {
        for (ushort selector = 0; selector < 4; selector++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa80000 | ((ushort)(0xe380 + 2 * selector))),
                NativeEnemyProgramOracle.EntryOf(AtomicNativeListAddresses.Entries, AtomicMovementDefinitions.InitialProgram(selector)),
                "Atomic native direction program");
        foreach (ushort selector in new ushort[] { 4, 5, 0x7fff, 0xffff })
            AssertThrows<InvalidDataException>(() => AtomicMovementDefinitions.InitialProgram(selector),
                "Atomic invalid direction selector");
    }

    /// <summary>Runs the initializers and all four spinning loops with the selector table forbidden, checking native speeds.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for atomic movement definitions")]
    internal static void VerifyAtomicMovementDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyAtomicInitialProgramSelection), () => VerifyAtomicInitialProgramSelection(rom));

        var enemies = new RoomEnemySystem();
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Atomic, slot.Definition);
        slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = AtomicFrameToolingDefinitions.Bank });

        for (ushort selector = 0; selector < 4; selector++)
        {
            slot.Parameter1 = selector;
            slot.Parameter2 = (ushort)(selector * 7);
            enemies.AtomicBehavior.InitializeAtomic(slot);
            AtomicEnemyState state = enemies.AtomicStates[0] ?? throw new InvalidOperationException("Verification requires enemies.AtomicStates[0].");
            AtomicProgram program = AtomicMovementDefinitions.InitialProgram(selector);
            AssertEqual(AnimationProgramCursor<AtomicProgram>.Start(program), state.Program,
                $"Atomic initializer animation {selector}");

            ushort speedOffset = (ushort)(slot.Parameter2 * 8);
            (short positiveWhole, ushort positiveFraction) = EnemyLinearSpeedDefinitions.Read(speedOffset);
            (short negativeWhole, ushort negativeFraction) = EnemyLinearSpeedDefinitions.Read((ushort)(speedOffset + 4));
            AssertEqual(unchecked((ushort)positiveWhole), state.SpeedWhole, $"Atomic positive whole speed {selector}");
            AssertEqual(positiveFraction, state.SpeedFraction, $"Atomic positive fractional speed {selector}");
            AssertEqual(unchecked((ushort)negativeWhole), state.NegativeSpeedWhole, $"Atomic negative whole speed {selector}");
            AssertEqual(negativeFraction, state.NegativeSpeedFraction, $"Atomic negative fractional speed {selector}");

            slot.InstructionTimer = NativeCountdown.Start(1);
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(AtomicAnimations.Programs, program, AtomicFrameDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(AtomicAnimations.Programs, program)),
                $"Atomic spinning loop {selector}");
        }

        Console.WriteLine(
            "Atomic movement definitions: four native selectors, production initializers and speeds, and all four " +
            "spinning loops pass with the selector table forbidden.");
    }


    /// <summary>Runs the retail-backed Atomic movement checks and program oracle.</summary>
    [VerificationCommand("--atomic-instruction-mechanics")]
    internal static int AtomicInstructionMechanicsCommand()
    {
        SuperMetroidAddressSpace rom =
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyAtomicMovementDefinitions), () => VerifyAtomicMovementDefinitions(rom));
        Suite(nameof(AtomicAnimationsVerification.VerifyAtomicAnimations), () => AtomicAnimationsVerification.VerifyAtomicAnimations(rom));
        return 0;
    }
}
