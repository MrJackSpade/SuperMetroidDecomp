using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Enemies.Metroids.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Metroids;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Mochtroid's flight and attached loops and its change-detecting install.</summary>
internal static class MochtroidBehaviorVerification
{
    /// <summary>
    /// The real initializer installs the free-flight loop, which pulses on time; switching to the attached loop starts
    /// it, while re-requesting the installed loop leaves the running animation alone, as the native change detector does.
    /// </summary>
    internal static void VerifyMochtroidBehavior()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Mochtroid, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        MochtroidBehavior.InitializeMochtroid(slot);
        MochtroidEnemyState state = enemies.MochtroidStates[0] ?? throw new InvalidOperationException("Mochtroid has no state.");
        AssertEqual((MochtroidSegment?)MochtroidSegment.FreeFlight, state.InstalledAnimation, "initializer installs free flight");
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(MochtroidAnimations.Segments, MochtroidSegment.FreeFlight, MochtroidFrameToolingDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(MochtroidAnimations.Segments, MochtroidSegment.FreeFlight)),
            "Mochtroid free-flight loop");

        AnimationPlayback<MochtroidSegment> running = state.Playback;
        MochtroidBehavior.SetMochtroidAnimation(slot, state, MochtroidSegment.FreeFlight);
        AssertEqual(running, state.Playback, "re-requesting the installed loop keeps it running");

        MochtroidBehavior.SetMochtroidAnimation(slot, state, MochtroidSegment.Attached);
        AssertEqual(new AnimationPlayback<MochtroidSegment>(MochtroidAnimations.StartingWith(MochtroidSegment.Attached), AnimationPlace.StartOfAnimation),
            state.Playback, "switching starts the attached loop");
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(MochtroidAnimations.Segments, MochtroidSegment.Attached, MochtroidFrameToolingDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(MochtroidAnimations.Segments, MochtroidSegment.Attached)),
            "Mochtroid attached loop");
        Console.WriteLine("  Mochtroid behavior: both pulse loops and the change-detecting install pass.");
    }

    /// <summary>Runs the Mochtroid behavior check and its program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--mochtroid-instruction-mechanics")]
    internal static int MochtroidBehaviorCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyMochtroidBehavior), () => VerifyMochtroidBehavior());
        Suite(nameof(MochtroidAnimationsVerification.VerifyMochtroidAnimations), () => MochtroidAnimationsVerification.VerifyMochtroidAnimations(rom));
        return 0;
    }
}
