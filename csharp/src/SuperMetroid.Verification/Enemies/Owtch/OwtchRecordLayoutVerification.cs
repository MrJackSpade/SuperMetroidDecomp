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
    /// whole, left fraction and whole, the state, then the sink depth.
    /// </summary>
    internal static void VerifyOwtchRecordLayout()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new OwtchEnemyState();
        slot.SetSpeciesState(state);
        NativeEnemyFamilyStateImport.Import(enemies, slot, new NativeEnemyRecordWords(
            A: 0x1234, B: 0x0002, C: 0xedcc, D: 0xfffd, E: OwtchNativeWords.Behaviors.Word(OwtchBehaviorState.Underground), F: 0x0010));
        AssertEqual(SignedSixteenSixteen.FromParts(2, 0x1234), state.Speed, "variables 0/1 are the speed's fraction and whole, variables 2/3 its negation");
        AssertEqual(OwtchBehaviorState.Underground, state.Behavior, "variable 4 is the state");
        AssertEqual(16, state.BurialDepth, "variable 5 is the sink depth");
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
