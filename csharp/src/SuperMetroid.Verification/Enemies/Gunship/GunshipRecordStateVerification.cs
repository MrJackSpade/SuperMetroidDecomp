using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Gunship;
namespace SuperMetroid.Verification.Enemies.Gunship;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the gunship part's values are separate and that a kill empties them.</summary>
internal static class GunshipRecordStateVerification
{
    /// <summary>
    /// Confirms each of the part's values keeps its own value when another is written, and that a kill empties every
    /// value.
    /// </summary>
    internal static void VerifyGunshipRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()));
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var part = new GunshipPartState();
        slot.SetSpeciesState(part, part.ClearWithRecord);

        part.BaseY = 160;
        part.LandingBrakeStep = 17;
        part.RumbleFrame = 64;
        AssertEqual(160, part.BaseY, "the base Y keeps its value apart from the landing-brake step and the rumble frame");
        AssertEqual(17, part.LandingBrakeStep, "the landing-brake step keeps its value");
        part.BobTimer = WrappingCountdown.Start(70);
        part.EngineSoundTimer = WrappingCountdown.Start(71);
        AssertEqual(70, part.BobTimer.RawValue, "the bob timer keeps its value apart from the engine-sound timer");
        part.Function = GunshipFunction.NoOperation;
        part.EscapeVelocity = 256;
        AssertEqual(GunshipFunction.NoOperation, part.Function, "the function keeps its value apart from the escape velocity");

        part.FunctionTimer = WrappingCountdown.Start(144);
        part.NextLiftoffPage = 3;
        part.BobPhase = 2;
        slot.Clear();
        AssertEqual(0, part.FunctionTimer.RawValue, "a kill empties the function timer");
        AssertEqual(0, part.NextLiftoffPage, "a kill empties the next liftoff page");
        AssertEqual(0, part.BobPhase, "a kill empties the bob phase");
        AssertEqual(0, part.BobTimer.RawValue, "a kill empties the bob timer");
        AssertEqual(0, part.EngineSoundTimer.RawValue, "a kill empties the engine-sound timer");
        AssertEqual(0, part.BaseY, "a kill empties the base Y");
        AssertEqual(0, part.LandingBrakeStep, "a kill empties the landing-brake step");
        AssertEqual(0, part.RumbleFrame, "a kill empties the rumble frame");
        AssertEqual(0, part.EscapeVelocity, "a kill empties the escape velocity");
        AssertEqual(GunshipFunction.None, part.Function, "a kill empties the function");
        Console.WriteLine("Gunship record state: each value is the part's own, and a kill empties the record.");
    }

    /// <summary>Runs the focused gunship record-state check.</summary>
    [VerificationCommand("--gunship-record-state")]
    internal static int GunshipRecordStateCommand()
    {
        VerifyGunshipRecordState();
        return 0;
    }
}
