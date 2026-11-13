using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Waver;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Waver;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Waver's four animations and the spin's completion cue.</summary>
internal static class WaverBehaviorVerification
{
    /// <summary>
    /// Each selected animation shows its frames and holds the last; only the two spins report their completion to main AI.
    /// </summary>
    internal static void VerifyWaverBehavior()
    {
        foreach (WaverSegment selector in new[]
                 {
                     WaverSegment.SteadyFacingLeft, WaverSegment.SteadyFacingRight,
                     WaverSegment.SpinningFacingLeft, WaverSegment.SpinningFacingRight,
                 })
        {
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.Waver, slot.Definition);
            slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
            slot.SetSpawnWords(EnemyDefinitionId.Waver, words => words);
            WaverBehavior.InitializeWaver(slot);
            WaverEnemyState state = enemies.WaverStates[0] ?? throw new InvalidOperationException("Verification requires enemies.WaverStates[0].");
            state.InstalledAnimation = selector == WaverSegment.SteadyFacingLeft ? WaverSegment.SteadyFacingRight : WaverSegment.SteadyFacingLeft;
            state.RequestedAnimation = selector;
            state.SpinFinished = false;
            WaverBehavior.SetWaverAnimation(slot, state);

            WaverSegment program = selector;
            ushort[] frames = WaverAnimationsConstants.Spritemaps(program).ToArray();
            // A one-update frame is still flagged new when the sleep runs on the next update; an eight-update frame's flag
            // has cleared by then.
            int duration = frames.Length == 1 ? 1 : 8;
            ushort[] shown = duration == 1 ? [.. frames, frames[^1]] : frames;
            string expected = EnemyInstructionRunner.ExpectedFrames(shown, shown.Select(_ => duration));
            AssertEqual(expected, EnemyInstructionRunner.ShownFrames(slot, frames.Length * duration + 2), $"Waver {selector} animation");
            AssertEqual(selector is WaverSegment.SpinningFacingLeft or WaverSegment.SpinningFacingRight, state.SpinFinished, $"Waver {selector} spin-completion cue");
        }
        Console.WriteLine("  Waver behavior: both steady poses and both spins run, and only the spins report completion.");
    }

    /// <summary>Runs the Waver behavior check and program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--waver-instruction-mechanics")]
    internal static int WaverBehaviorCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyWaverBehavior), () => VerifyWaverBehavior());
        Suite(nameof(WaverAnimationsVerification.VerifyWaverAnimations), () => WaverAnimationsVerification.VerifyWaverAnimations(rom));
        return 0;
    }
}
