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
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.Mochtroid, place.KeptDefinition);
        place.Bind(place.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
        // The Mochtroid's setup builds the state its steps and its animation, which the check plays, run on.
        MochtroidEnemyState state = MochtroidBehavior.InitializeMochtroid(place);
        void Animate() => MochtroidBehavior.AdvanceMochtroidTypedAnimation(place, state);
        AssertTrue(ReferenceEquals(MochtroidAnimations.StartingWith(MochtroidSegment.FreeFlight), state.Playback.Animation), "initializer starts free flight");
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(MochtroidAnimations.Segments, MochtroidSegment.FreeFlight, MochtroidFrameToolingDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(place, EnemyInstructionRunner.LoopUpdates(MochtroidAnimations.Segments, MochtroidSegment.FreeFlight), Animate),
            "Mochtroid free-flight loop");

        AnimationPlayback<MochtroidSegment> running = state.Playback;
        MochtroidBehavior.SetMochtroidAnimation(place, state, MochtroidSegment.FreeFlight);
        AssertEqual(running, state.Playback, "re-requesting the installed loop keeps it running");

        MochtroidBehavior.SetMochtroidAnimation(place, state, MochtroidSegment.Attached);
        AssertEqual(new AnimationPlayback<MochtroidSegment>(MochtroidAnimations.StartingWith(MochtroidSegment.Attached), AnimationPlace.StartOfAnimation),
            state.Playback, "switching starts the attached loop");
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(MochtroidAnimations.Segments, MochtroidSegment.Attached, MochtroidFrameToolingDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(place, EnemyInstructionRunner.LoopUpdates(MochtroidAnimations.Segments, MochtroidSegment.Attached), Animate),
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
