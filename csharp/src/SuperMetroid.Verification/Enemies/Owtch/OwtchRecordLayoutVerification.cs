using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Owtch;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Owtch;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that a recorded Owtch record imports into the Owtch's state by the cartridge layout.</summary>
internal static class OwtchRecordLayoutVerification
{
    /// <summary>
    /// #1387: a recorded Owtch record imports by the cartridge layout of <c>$A2:A423-$A473</c>: right fraction and
    /// whole, left fraction and whole, the state, then the sink depth. The Owtch walks at its placement's speed (#2439), which
    /// the recorded speed must be.
    /// </summary>
    internal static void VerifyOwtchRecordLayout()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        // Slot zero's word C carries the Zebetites' shared pulse (#2259), so the Owtch records from slot one.
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[1];
        var state = new OwtchEnemyState();
        place.BindPlaced(EnemyDefinitionId.Owtch, new OwtchSpawnSettings(OwtchBehaviorState.Underground, UndergroundWait: 1, Speed: SignedSixteenSixteen.FromParts(2, 0x1234), TravelDistance: 1));
        place.SetSpeciesState(state);
        NativeEnemyFamilyStateImport.Import(enemies, place, new NativeEnemyRecordWords(
            A: 0x1234, B: 0x0002, C: 0xedcc, D: 0xfffd, E: OwtchNativeWords.Behaviors.Word(OwtchBehaviorState.Underground), F: 0x0010));
        AssertThrows<InvalidDataException>(() => NativeEnemyFamilyStateImport.Import(enemies, place, new NativeEnemyRecordWords(
                A: 0x1235, B: 0x0002, C: 0xedcb, D: 0xfffd, E: OwtchNativeWords.Behaviors.Word(OwtchBehaviorState.Underground), F: 0x0010)),
            "variables 0/1 are the speed's fraction and whole, variables 2/3 its negation, and the speed is the placement's");
        AssertEqual(OwtchBehaviorState.Underground, state.Behavior, "variable 4 is the state");
        AssertTrue(state.Sink.IsOver && state.Rise.IsOver, "variable 5, the sink depth, is a whole burial while underground");
        Console.WriteLine("Owtch record layout: speed, state and sink import from variables 0-5.");
    }

    /// <summary>Runs the focused Owtch record-layout check.</summary>
    [VerificationCommand("--owtch-record-layout")]
    internal static int OwtchRecordLayoutCommand()
    {
        VerifyOwtchRecordLayout();
        return 0;
    }
}
