using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Verification.Bosses.Kraid;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that a kill empties a Kraid part's own state.</summary>
internal static class KraidRecordStateVerification
{
    /// <summary>
    /// Confirms that a kill empties the record values of the killed part's own state (#1476, #1542): the function and
    /// function timer every part runs, and each part kind's own values.
    /// </summary>
    internal static void VerifyKraidRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), RoomEnemyCompatibilityFixtures.ForNewRoom());
        var body = new KraidBodyState();
        var lint = new KraidLintState();
        var nail = new KraidNailState();
        RoomEnemySlot bodySlot = enemies.SlotTable.Slots[0];
        RoomEnemySlot lintSlot = enemies.SlotTable.Slots[2];
        RoomEnemySlot nailSlot = enemies.SlotTable.Slots[6];
        bodySlot.SetSpeciesState<KraidPartState>(body, body.ClearWithRecord);
        lintSlot.SetSpeciesState<KraidPartState>(lint, lint.ClearWithRecord);
        nailSlot.SetSpeciesState<KraidPartState>(nail, nail.ClearWithRecord);

        body.Function = KraidAiFunction.RaiseRocksEvery8Frames;
        body.FunctionTimer = 18;
        body.HeadTimer = 64;
        body.CeilingRock = 3;
        body.RisingRockTimer = 96;
        lint.LintTravel = 32;
        lint.LintXOffset = -16;
        nail.Function = KraidAiFunction.FingernailFire;
        nail.NailVelocity = FixedVector<RoomPixels>.FromWords(0, 0, 6, 5);
        bodySlot.Clear();
        lintSlot.Clear();
        nailSlot.Clear();
        AssertEqual(KraidAiFunction.None, body.Function, "a kill empties the body's function");
        AssertEqual(0, body.FunctionTimer, "a kill empties the function timer");
        AssertEqual(0, body.HeadTimer, "a kill empties the head timer");
        AssertEqual(0, body.CeilingRock, "a kill empties the next ceiling rock");
        AssertEqual(0, body.RisingRockTimer, "a kill empties the rising-rock timer");
        AssertEqual(0, lint.LintTravel, "a kill empties the travel");
        AssertEqual(0, lint.LintXOffset, "a kill empties the lint offset");
        AssertEqual(KraidAiFunction.None, nail.Function, "a kill empties the nail's function");
        AssertEqual(default(FixedVector<RoomPixels>), nail.NailVelocity, "a kill empties the nail velocity");
        Console.WriteLine("Kraid record state: a kill empties the killed part's own state.");
    }

    /// <summary>Runs the focused Kraid record-state check.</summary>
    [VerificationCommand("--kraid-record-state")]
    internal static int KraidRecordStateCommand()
    {
        VerifyKraidRecordState();
        return 0;
    }
}
