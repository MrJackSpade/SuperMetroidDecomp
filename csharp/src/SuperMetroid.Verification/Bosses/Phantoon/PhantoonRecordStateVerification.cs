using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Verification.Bosses.Phantoon;
using SuperMetroid.Verification.Harness;

/// <summary>Checks a Phantoon part's shared record words and that a kill empties them.</summary>
internal static class PhantoonRecordStateVerification
{
    /// <summary>
    /// Confirms the views that share a record word see each other's writes, as the cartridge's words do, and that a
    /// kill empties every word of the registered part.
    /// </summary>
    internal static void VerifyPhantoonRecordState()
    {
        var enemies = new RoomEnemySystem();
        RoomEnemySlot slot = enemies.SlotTable.Slots[1];
        var part = new PhantoonPartState();
        slot.SetSpeciesState(part, part.ClearWithRecord);

        part.PathCursor = 533;
        AssertEqual(533, part.ClosedTimer, "the path cursor and the eye's closed timer are one record word");
        part.SpeedWhole = 1;
        AssertTrue(part.FigureEightReversed, "the speed's whole word and the eye's figure-eight direction are one record word");
        part.Function = PhantoonAiFunction.SpawnStartingFlames;
        AssertTrue(part.FadeComplete, "a part's function word reads as a completed fade, as the cartridge's word does");

        part.SpeedFraction = 0x8000;
        part.SpeedPhase = 3;
        part.FunctionTimer = 120;
        slot.Clear();
        AssertEqual(0, part.PathCursor, "a kill empties word A");
        AssertEqual(0, part.SpeedFraction, "a kill empties word B");
        AssertEqual(0, part.SpeedWhole, "a kill empties word C");
        AssertEqual(0, part.SpeedPhase, "a kill empties word D");
        AssertEqual(0, part.FunctionTimer, "a kill empties word E");
        AssertEqual(0, part.WavyPhaseDelta, "a kill empties word F");
        Console.WriteLine("Phantoon record state: shared words carry between views, and a kill empties the record.");
    }

    /// <summary>Runs the focused Phantoon record-state check.</summary>
    [VerificationCommand("--phantoon-record-state")]
    internal static int PhantoonRecordStateCommand()
    {
        VerifyPhantoonRecordState();
        return 0;
    }
}
