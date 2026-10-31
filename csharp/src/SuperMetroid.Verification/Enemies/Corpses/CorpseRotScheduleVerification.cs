using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies.Corpses;
namespace SuperMetroid.Verification.Enemies.Corpses;
using SuperMetroid.Verification.Harness;

/// <summary>Checks a corpse's own rot schedule (#1472): its staggered row moves and the rows it lets fall.</summary>
internal static class CorpseRotScheduleVerification
{
    /// <summary>
    /// Runs the first rot call of a three-row corpse: the bottom row moves and falls at once, and the two rows above it,
    /// already in their lead-in, move themselves down since both are at or past the row before the last.
    /// </summary>
    internal static void VerifyCorpseRotScheduleFirstCall()
    {
        var schedule = new CorpseRotSchedule(3);
        CorpseRotScheduleChecks.AssertFreshSchedule(3, schedule, "three-row corpse");

        var moves = new List<CorpseRowMove>();
        var fallen = new List<int>();
        bool rotting = schedule.Step((row, operation) => moves.Add(new CorpseRowMove(row, operation)), fallen.Add);

        AssertTrue(rotting, "a row other than the last falling does not end the rot");
        AssertTrue(moves.SequenceEqual(new[]
            {
                new CorpseRowMove(2, CorpseRowOperation.Move),
                new CorpseRowMove(1, CorpseRowOperation.Move),
                new CorpseRowMove(0, CorpseRowOperation.Move),
            }), "row move order and lead-in moves near the corpse's foot");
        AssertTrue(fallen.SequenceEqual(new[] { 0 }), "only the bottom row, whose delay was over, falls");
        AssertTrue(schedule.Rows[0].HasFallen, "the fallen row is skipped from now on");
        AssertEqual(1, schedule.Rows[1].PixelRow, "the second row has not started falling");
        AssertEqual(1, schedule.Rows[1].DelayCalls, "the second row counts its delay down");
        Console.WriteLine("  Corpse rot schedule: first call's move order, lead-in moves and fallen rows pass.");
    }

    /// <summary>Runs the corpse rot schedule checks; no cartridge is needed.</summary>
    [VerificationCommand("--corpse-rot-schedule")]
    internal static int CorpseRotScheduleCommand()
    {
        Suite(nameof(VerifyCorpseRotScheduleFirstCall), () => VerifyCorpseRotScheduleFirstCall());
        return 0;
    }
}
