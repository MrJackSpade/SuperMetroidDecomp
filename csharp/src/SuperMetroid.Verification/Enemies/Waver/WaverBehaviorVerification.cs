using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Waver;
using SuperMetroid.Core.Enemies.Waver.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Waver;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Waver's four animations and the spin's completion cue.</summary>
internal static class WaverBehaviorVerification
{
    /// <summary>
    /// Each selected animation shows its frames and holds the last; only the two spins report their completion to main AI.
    /// </summary>
    internal static void VerifyWaverBehavior()
    {
        foreach (WaverAnimationSelector selector in new[]
                 {
                     WaverAnimationSelector.None, WaverAnimationSelector.FacingRight,
                     WaverAnimationSelector.Spinning, WaverAnimationSelector.Spinning | WaverAnimationSelector.FacingRight,
                 })
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.Waver, slot.Definition);
            slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = WaverFrameToolingDefinitions.Bank });
            WaverBehavior.InitializeWaver(slot);
            WaverEnemyState state = enemies.WaverStates[0] ?? throw new InvalidOperationException("Verification requires enemies.WaverStates[0].");
            state.CurrentInstructionListIndex = selector == WaverAnimationSelector.None ? WaverAnimationSelector.FacingRight : WaverAnimationSelector.None;
            state.RequestedInstructionListIndex = selector;
            state.SpinFinished = false;
            WaverBehavior.SetWaverProgram(slot, state);

            WaverProgram program = WaverAnimationDefinitions.Program(selector);
            ushort[] frames = WaverAnimationsConstants.Spritemaps(program).ToArray();
            // A one-update frame is still flagged new when the sleep runs on the next update; an eight-update frame's flag
            // has cleared by then.
            int duration = frames.Length == 1 ? 1 : 8;
            ushort[] shown = duration == 1 ? [.. frames, frames[^1]] : frames;
            string expected = EnemyInstructionRunner.ExpectedFrames(shown, shown.Select(_ => duration));
            AssertEqual(expected, EnemyInstructionRunner.ShownFrames(slot, frames.Length * duration + 2), $"Waver {selector} animation");
            AssertEqual(selector.HasFlag(WaverAnimationSelector.Spinning), state.SpinFinished, $"Waver {selector} spin-completion cue");
        }
        Console.WriteLine("  Waver behavior: both steady poses and both spins run, and only the spins report completion.");
    }

    /// <summary>Runs the Waver behavior check and program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--waver-instruction-mechanics")]
    internal static int WaverBehaviorCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyWaverBehavior), () => VerifyWaverBehavior());
        Suite(nameof(WaverAnimationsVerification.VerifyWaverAnimations), () => WaverAnimationsVerification.VerifyWaverAnimations(rom));
        return 0;
    }
}
