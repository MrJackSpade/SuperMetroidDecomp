using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Gunship;
namespace SuperMetroid.Verification.Enemies.Gunship;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the gunship's shared record words and that a kill empties them.</summary>
internal static class GunshipRecordStateVerification
{
    /// <summary>
    /// Confirms the views that share a record word see each other's writes, as the cartridge's words do, and that a
    /// kill empties every word.
    /// </summary>
    internal static void VerifyGunshipRecordState()
    {
        var enemies = new RoomEnemySystem();
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var part = new GunshipPartState();
        slot.SetSpeciesState(part, part.ClearWithRecord);

        part.LandingBrakeStep = 17;
        AssertEqual(17, part.BaseY, "the landing-brake step and the base Y are one record word");
        part.BobTimer = 70;
        AssertEqual(70, part.EngineSoundTimer, "the bob timer and the engine-sound timer are one record word");
        part.Function = GunshipFunction.NoOperation;
        AssertEqual((int)(ushort)GunshipFunction.NoOperation, part.EscapeVelocity, "the function and the escape velocity are one record word");

        part.FunctionTimer = 144;
        part.LiftoffTransferIndex = 3;
        part.BobPhase = 2;
        slot.Clear();
        AssertEqual(0, part.FunctionTimer, "a kill empties the function timer");
        AssertEqual(0, part.LiftoffTransferIndex, "a kill empties the transfer index");
        AssertEqual(0, part.BobPhase, "a kill empties the bob phase");
        AssertEqual(0, part.BobTimer, "a kill empties the timer word");
        AssertEqual(0, part.BaseY, "a kill empties the height word");
        AssertEqual(0, part.FunctionWord, "a kill empties the function word");
        Console.WriteLine("Gunship record state: shared words carry between views, and a kill empties the record.");
    }

    /// <summary>Runs the focused gunship record-state check.</summary>
    [VerificationCommand("--gunship-record-state")]
    internal static int GunshipRecordStateCommand()
    {
        VerifyGunshipRecordState();
        return 0;
    }
}
