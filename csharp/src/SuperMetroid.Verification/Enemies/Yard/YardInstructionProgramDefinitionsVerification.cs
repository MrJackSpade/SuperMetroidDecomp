using SuperMetroid.Core.Animation;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Yard;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Yard;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Yard's crawling, turning, hiding, and airborne instruction entries against bank A3.</summary>
internal static class YardInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge for the Yard animation checks.</summary>
    internal static void VerifyYardInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyYardInstructionProgramDefinitions), () => VerifyYardInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>
    /// Every program runs for 80 forced updates; a hiding Yard keeps its shell closed update after update, while a visible
    /// Yard with an even random draw runs on into the hidden animation; the program oracle matches bank $A3.
    /// </summary>
    internal static void VerifyYardInstructionProgramDefinitions(CartridgeImportAddressSpace rom)
    {
        RoomEnemySystem enemies = CreateYardInstructionSystem(out RoomEnemyPlace yard);
        YardEnemyState state = enemies.Places.InUpdateOrder[0].EntityState<YardEntity, YardEnemyState>() ?? throw new InvalidOperationException("The Yard has no state.");
        foreach (YardSegment program in YardAnimations.StartingAt.Keys)
        {
            state.Playback = new(YardAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
            RunYard(yard, 80);
        }

        // Hiding: the closed shell shows again however often its update ends (#2180).
        ushort repeatedPose = YardAnimationsConstants.Spritemaps(YardSegment.ShellClosedOnFloorMovingLeft).First();
        state.Behavior = YardBehavior.Hiding;
        state.Playback = new(YardAnimations.StartingWith(YardSegment.HidingOnFloorMovingLeft), AnimationPlace.StartOfAnimation);
        RunYard(yard, 20);
        AssertEqual(YardSegment.ShellClosedOnFloorMovingLeft, StandsIn(state.Playback), "a hiding Yard stays in its closed shell");
        AssertEqual(repeatedPose, yard.SpritemapPointer, "a hiding Yard keeps showing its closed shell");

        // Visible, with the fixture's even random draw: the hide runs on into the hidden animation.
        state.Behavior = YardBehavior.NormalCrawling;
        state.Playback = new(YardAnimations.StartingWith(YardSegment.HidingOnFloorMovingLeft), AnimationPlace.StartOfAnimation);
        RunYard(yard, 3);
        AssertEqual(YardSegment.HiddenOnFloorMovingLeft, StandsIn(state.Playback), "a visible Yard's even draw runs on into the hidden animation");

        Suite(nameof(YardAnimationsVerification.VerifyYardAnimations), () => YardAnimationsVerification.VerifyYardAnimations(rom));
        Console.WriteLine("Yard: all 38 programs run, and the closed shell holds a hiding Yard and runs a visible one on.");
    }

    /// <summary>The segment a playback stands in: the segment of the phase it plays.</summary>
    private static YardSegment StandsIn(AnimationPlayback<YardSegment> playback) => playback.Animation.Phases[playback.Place.Phase].Segment;

    /// <summary>Forces the requested number of animation updates.</summary>
    private static void RunYard(RoomEnemyPlace yard, int updates)
    {
        for (int update = 0; update < updates; update++)
        {
            yard.AnimationClock.StepOnNextUpdate();
            EnemyAnimationPlayer.AdvanceAnimation(yard, RoomEnemyFrameFixtures.StandaloneSamus(), RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), Position16<RoomPixels>.Zero, 0);
        }
    }

    /// <summary>Creates a bank A3 Yard slot with deterministic random input and its behavior state installed.</summary>
    internal static RoomEnemySystem CreateYardInstructionSystem(
        out RoomEnemyPlace yard)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().NextRandom = () => 0;

        yard = enemies.Places.InUpdateOrder[0];
        yard.Bind(EnemyDefinitionId.Yard, yard.KeptDefinition);
        yard.Bind(yard.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
        var state = new YardEnemyState { Playback = new(YardAnimations.StartingWith(YardSegment.CrawlingOnFloorMovingLeft), AnimationPlace.StartOfAnimation) };
        yard.SetSpeciesState(state);
        return enemies;
    }

    /// <summary>Runs the Yard instruction comparison from the verification command dispatcher.</summary>
    [VerificationCommand("--yard-instruction-program-definitions")]
    internal static int YardInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyYardInstructionProgramDefinitions), () => VerifyYardInstructionProgramDefinitions());
        return 0;
    }
}
