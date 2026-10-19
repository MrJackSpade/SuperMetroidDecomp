using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.SpacePirates;
namespace SuperMetroid.Verification.Enemies.SpacePirates;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that a kill empties the WallSpacePirate values the cartridge keeps in the enemy's common record.</summary>
internal static class WallSpacePirateRecordStateVerification
{
    /// <summary>
    /// Installs a WallSpacePirate state on a slot with its record-clearing operation, gives every value the record holds a
    /// nonzero value, and confirms a kill empties each one.
    /// </summary>
    internal static void VerifyWallSpacePirateRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new WallSpacePirateEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.Function = Enum.GetValues<WallSpacePirateFunction>()[^1];
        state.WallJumpDestination = new Position16<RoomPixels>(1, 0);
        state.ClimbDirection = Enum.GetValues<WallSpacePirateClimbDirection>()[^1];
        state.WallJumpArcCenter = new Position16<RoomPixels>(1, 0);
        state.WallJumpArcAngle = SnesAngle.FromTableIndex(1);

        slot.Clear();
        AssertEqual<WallSpacePirateFunction?>(null, state.Function, "a kill empties Function");
        AssertEqual<Position16<RoomPixels>>(Position16<RoomPixels>.Zero, state.WallJumpDestination, "a kill empties WallJumpDestination");
        AssertEqual<WallSpacePirateClimbDirection>(default(WallSpacePirateClimbDirection), state.ClimbDirection, "a kill empties ClimbDirection");
        AssertEqual<Position16<RoomPixels>>(Position16<RoomPixels>.Zero, state.WallJumpArcCenter, "a kill empties WallJumpArcCenter");
        AssertEqual<SnesAngle>(SnesAngle.FromTableIndex(0), state.WallJumpArcAngle, "a kill empties WallJumpArcAngle");
        Console.WriteLine("WallSpacePirate record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused WallSpacePirate record-state check.</summary>
    [VerificationCommand("--wall-space-pirate-record-state")]
    internal static int WallSpacePirateRecordStateCommand()
    {
        VerifyWallSpacePirateRecordState();
        return 0;
    }
}
