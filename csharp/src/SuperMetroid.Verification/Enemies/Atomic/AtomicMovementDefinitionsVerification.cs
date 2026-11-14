using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Atomic;
using SuperMetroid.Core.Enemies.Atomic.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Atomic;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Retail-word and guarded-execution checks for Atomic movement and animation selectors.</summary>
internal static class AtomicMovementDefinitionsVerification
{
    /// <summary>Matches four initial direction selectors against the retail table and rejects out-of-range values.</summary>
    private static void VerifyAtomicInitialProgramSelection(CartridgeImportAddressSpace rom)
    {
        for (ushort selector = 0; selector < 4; selector++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa80000 | ((ushort)(0xe380 + 2 * selector))),
                NativeEnemyAnimationOracle.EntryOf(AtomicNativeListAddresses.Entries, DecodedSpin(selector)),
                "Atomic native direction program");
        foreach (ushort selector in new ushort[] { 4, 5, 0x7fff, 0xffff })
            AssertThrows<InvalidDataException>(() => DecodedSpin(selector),
                "Atomic invalid direction selector");
    }

    /// <summary>The spinning animation the import decodes from a first parameter word.</summary>
    private static AtomicSegment DecodedSpin(int parameter1) =>
        ((AtomicSpawnSettings)EnemySpawnSettingsCodec.Decode(EnemyDefinitionId.Atomic, PopulationWords.Parameters(parameter1, 0))).Spin;

    /// <summary>Runs the initializers and all four spinning loops with the selector table forbidden, checking native speeds.</summary>
    internal static void VerifyAtomicMovementDefinitions(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyAtomicInitialProgramSelection), () => VerifyAtomicInitialProgramSelection(rom));

        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Atomic, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));

        for (ushort selector = 0; selector < 4; selector++)
        {
            slot.SetSpawnParameter1(EnemyDefinitionId.Atomic, selector);
            slot.SetSpawnParameter2(EnemyDefinitionId.Atomic, (ushort)(selector * 7));
            AtomicBehavior.InitializeAtomic(slot);
            AtomicEnemyState state = enemies.AtomicStates[0] ?? throw new InvalidOperationException("Verification requires enemies.AtomicStates[0].");
            AtomicSegment program = DecodedSpin(selector);
            AssertEqual(new AnimationPlayback<AtomicSegment>(AtomicAnimations.StartingWith(program), AnimationPlace.StartOfAnimation), state.Playback,
                $"Atomic initializer animation {selector}");

            AssertEqual(EnemyLinearSpeedTable.Speed(selector * 7), state.Speed, $"Atomic speed {selector}");

            slot.AnimationClock.StepOnNextUpdate();
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(AtomicAnimations.Segments, program, AtomicFrameToolingDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(AtomicAnimations.Segments, program)),
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
        CartridgeImportAddressSpace rom =
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyAtomicMovementDefinitions), () => VerifyAtomicMovementDefinitions(rom));
        Suite(nameof(AtomicAnimationsVerification.VerifyAtomicAnimations), () => AtomicAnimationsVerification.VerifyAtomicAnimations(rom));
        return 0;
    }
}
