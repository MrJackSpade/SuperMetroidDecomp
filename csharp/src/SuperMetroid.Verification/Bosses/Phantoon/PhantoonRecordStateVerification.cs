using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Verification.Bosses.Phantoon;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks a Phantoon part's record values and that a kill empties them.</summary>
internal static class PhantoonRecordStateVerification
{
    /// <summary>
    /// Confirms the values the cartridge keeps in one record word are held apart (#1476), and that a kill empties every
    /// value of the registered part.
    /// </summary>
    internal static void VerifyPhantoonRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()));
        RoomEnemySlot slot = enemies.SlotTable.Slots[1];
        var part = new PhantoonPartState();
        slot.SetSpeciesState(part, part.ClearWithRecord);

        part.PathCursor = 533;
        AssertEqual(0, part.ClosedTimer, "the path cursor leaves the eye's closed timer alone");
        part.SpeedWhole = 1;
        AssertTrue(!part.FigureEightReversed, "the speed leaves the eye's figure-eight direction alone");
        part.Function = PhantoonAiFunction.SpawnStartingFlames;
        AssertTrue(!part.FadeComplete, "a part's function leaves the eye's fade-complete flag alone");

        part.SpeedFraction = 0x8000;
        part.SpeedPhase = 3;
        part.FunctionTimer = 120;
        part.FadeComplete = true;
        part.WavyPhaseDelta = 2;
        slot.Clear();
        AssertEqual(0, part.PathCursor, "a kill empties the path cursor");
        AssertEqual(0, part.SpeedFraction, "a kill empties the speed fraction");
        AssertEqual(0, part.SpeedWhole, "a kill empties the speed");
        AssertEqual(0, part.SpeedPhase, "a kill empties the speed phase");
        AssertEqual(0, part.FunctionTimer, "a kill empties the function timer");
        AssertTrue(!part.FadeComplete, "a kill empties the fade-complete flag");
        AssertEqual(0, part.WavyPhaseDelta, "a kill empties the wavy phase step");
        Console.WriteLine("Phantoon record state: each part's values are held apart, and a kill empties the record.");
    }

    /// <summary>Runs the focused Phantoon record-state check.</summary>
    [VerificationCommand("--phantoon-record-state")]
    internal static int PhantoonRecordStateCommand()
    {
        VerifyPhantoonRecordState();
        return 0;
    }
}
