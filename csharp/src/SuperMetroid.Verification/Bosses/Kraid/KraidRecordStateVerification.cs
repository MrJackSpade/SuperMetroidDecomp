using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Verification.Bosses.Kraid;
using SuperMetroid.Verification.Harness;

/// <summary>Checks a Kraid part's shared record words and that a kill empties them.</summary>
internal static class KraidRecordStateVerification
{
    /// <summary>
    /// Confirms the views that share a record word see each other's writes, as the cartridge's words do, and that a
    /// kill empties every word of the registered part.
    /// </summary>
    internal static void VerifyKraidRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        RoomEnemySlot slot = enemies.SlotTable.Slots[6];
        var part = new KraidPartState();
        slot.SetSpeciesState(part, part.ClearWithRecord);

        part.FunctionTimer = 18;
        AssertEqual(18, part.CeilingRockOffset, "the function timer and the ceiling-rock offset are one record word");
        part.HeadTimer = 64;
        AssertEqual((ushort)64, part.NailVelocity.X.WholeWord, "the head timer and a nail's horizontal whole word are one record word");
        part.LintTravel = 32;
        AssertEqual((ushort)32, part.NailVelocity.X.Fraction, "a lint's travel and a nail's horizontal fraction are one record word");

        part.Function = KraidAiFunction.FingernailFire;
        part.NailVelocity = FixedVector<RoomPixels>.FromWords(0, 0, 6, 5);
        slot.Clear();
        AssertEqual((KraidAiFunction)0, part.Function, "a kill empties the function");
        AssertEqual(0, part.LintTravel, "a kill empties the travel word");
        AssertEqual(0, part.HeadTimer, "a kill empties the head-timer word");
        AssertEqual(default(FixedVector<RoomPixels>), part.NailVelocity, "a kill empties the nail velocity");
        AssertEqual(0, part.FunctionTimer, "a kill empties the timer word");
        Console.WriteLine("Kraid record state: shared words carry between views, and a kill empties the record.");
    }

    /// <summary>Runs the focused Kraid record-state check.</summary>
    [VerificationCommand("--kraid-record-state")]
    internal static int KraidRecordStateCommand()
    {
        VerifyKraidRecordState();
        return 0;
    }
}
