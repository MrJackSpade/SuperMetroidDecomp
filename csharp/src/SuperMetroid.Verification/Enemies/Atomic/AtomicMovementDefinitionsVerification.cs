using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Atomic;
using SuperMetroid.Core.Enemies.Atomic.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Atomic;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Retail-word and guarded-execution checks for Atomic movement and animation selectors.</summary>
internal static class AtomicMovementDefinitionsVerification
{
    /// <summary>Matches four initial direction selectors against the retail table and rejects out-of-range values.</summary>
    private static void VerifyAtomicInitialProgramSelection(SuperMetroidAddressSpace rom)
    {
        for (ushort selector = 0; selector < 4; selector++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa80000 | ((ushort)(0xe380 + 2 * selector))),
                NativeEnemyProgramOracle.EntryOf(AtomicNativeListAddresses.Entries, AtomicSpins.InitialProgram(selector)),
                "Atomic native direction program");
        foreach (ushort selector in new ushort[] { 4, 5, 0x7fff, 0xffff })
            AssertThrows<InvalidDataException>(() => AtomicSpins.InitialProgram(selector),
                "Atomic invalid direction selector");
    }

    /// <summary>Runs the initializers and all four spinning loops with the selector table forbidden, checking native speeds.</summary>
    internal static void VerifyAtomicMovementDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyAtomicInitialProgramSelection), () => VerifyAtomicInitialProgramSelection(rom));

        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Atomic, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));

        for (ushort selector = 0; selector < 4; selector++)
        {
            slot.SetSpawnParameter1(EnemyDefinitionId.Atomic, selector);
            slot.SetSpawnParameter2(EnemyDefinitionId.Atomic, (ushort)(selector * 7));
            AtomicBehavior.InitializeAtomic(slot);
            AtomicEnemyState state = enemies.AtomicStates[0] ?? throw new InvalidOperationException("Verification requires enemies.AtomicStates[0].");
            AtomicProgram program = AtomicSpins.InitialProgram(selector);
            AssertEqual(AnimationProgramCursor<AtomicProgram>.Start(program), state.Program,
                $"Atomic initializer animation {selector}");

            ushort speedOffset = (ushort)(slot.SpawnSettings<AtomicSpawnSettings>().SpeedIndex * 8);
            SignedSixteenSixteen positive = EnemyLinearSpeeds.Read(speedOffset);
            SignedSixteenSixteen negative = EnemyLinearSpeeds.Read((ushort)(speedOffset + 4));
            AssertEqual(positive, state.Speed, $"Atomic positive speed {selector}");
            AssertEqual(negative, state.NegativeSpeed, $"Atomic negative speed {selector}");

            slot.InstructionTimer = WrappingCountdown.Start(1);
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(AtomicAnimations.Programs, program, AtomicFrameToolingDefinitions.Spritemap),
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
