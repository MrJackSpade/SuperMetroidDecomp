using SuperMetroid.Core.Animation;
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
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            place.Bind(EnemyDefinitionId.Waver, place.Definition);
            place.Bind(place.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
            place.SetSpawnWords(EnemyDefinitionId.Waver, words => words);
            WaverBehavior.InitializeWaver(place.HeldOccupant(), place);
            WaverEnemyState state = enemies.WaverStates[0] ?? throw new InvalidOperationException("Verification requires enemies.WaverStates[0].");
            state.Playback = new(WaverAnimations.StartingWith(selector == WaverSegment.SteadyFacingLeft ? WaverSegment.SteadyFacingRight : WaverSegment.SteadyFacingLeft),
                AnimationPlace.StartOfAnimation);
            WaverBehavior.SetWaverAnimation(place, state, selector);

            WaverSegment program = selector;
            ushort[] frames = WaverAnimationsConstants.Spritemaps(program).ToArray();
            // A steady pose is held (#2276): it is new only on the update it shows; a spin shows each eight-update frame.
            int duration = frames.Length == 1 ? 1 : 8;
            ushort[] shown = frames;
            string expected = EnemyInstructionRunner.ExpectedFrames(shown, shown.Select(_ => duration));
            AssertEqual(expected, EnemyInstructionRunner.ShownFrames(place, frames.Length * duration + 2), $"Waver {selector} animation");
            AssertEqual(selector is WaverSegment.SpinningFacingLeft or WaverSegment.SpinningFacingRight ? WaverAction.SpinFinished : WaverAction.Gliding,
                state.Action, $"Waver {selector} spin-completion cue");
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
