using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Verification.Bosses.Kraid;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks a Kraid part's record values and that a kill empties them.</summary>
internal static class KraidRecordStateVerification
{
    /// <summary>
    /// Confirms the values the cartridge keeps in one record word are held apart (#1476), and that a kill empties every
    /// value of the registered part.
    /// </summary>
    internal static void VerifyKraidRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()));
        RoomEnemySlot slot = enemies.SlotTable.Slots[6];
        var part = new KraidPartState();
        slot.SetSpeciesState(part, part.ClearWithRecord);

        part.FunctionTimer = 18;
        AssertEqual(0, part.CeilingRock, "the function timer leaves the next ceiling rock alone");
        part.HeadTimer = 64;
        AssertEqual(default(FixedVector<RoomPixels>), part.NailVelocity, "the head timer and lint travel leave a nail's velocity alone");
        part.LintTravel = 32;
        part.CeilingRock = 3;
        part.RisingRockTimer = 96;
        part.LintXOffset = 65520;

        part.Function = KraidAiFunction.FingernailFire;
        part.NailVelocity = FixedVector<RoomPixels>.FromWords(0, 0, 6, 5);
        slot.Clear();
        AssertEqual((KraidAiFunction)0, part.Function, "a kill empties the function");
        AssertEqual(0, part.LintTravel, "a kill empties the travel");
        AssertEqual(0, part.LintXOffset, "a kill empties the lint offset");
        AssertEqual(0, part.HeadTimer, "a kill empties the head timer");
        AssertEqual(default(FixedVector<RoomPixels>), part.NailVelocity, "a kill empties the nail velocity");
        AssertEqual(0, part.FunctionTimer, "a kill empties the function timer");
        AssertEqual(0, part.CeilingRock, "a kill empties the next ceiling rock");
        AssertEqual(0, part.RisingRockTimer, "a kill empties the rising-rock timer");
        Console.WriteLine("Kraid record state: each part's values are held apart, and a kill empties the record.");
    }

    /// <summary>Runs the focused Kraid record-state check.</summary>
    [VerificationCommand("--kraid-record-state")]
    internal static int KraidRecordStateCommand()
    {
        VerifyKraidRecordState();
        return 0;
    }
}
