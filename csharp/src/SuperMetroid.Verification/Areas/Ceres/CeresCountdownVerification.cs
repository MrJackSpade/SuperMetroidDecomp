using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Samus;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>
/// Checks the Ceres countdowns that are true durations (#2426): the pre-elevator hall's falling tiles, every ninth update
/// once the first has fallen after the wait the hall inherits, and the elevator pad's wait, which moves Samus on its 60th
/// update.
/// </summary>
internal static class CeresCountdownVerification
{
    /// <summary>
    /// The hall's first tile falls on the update after the inherited count runs out, or at once when the shaft's backward
    /// sweep left it; later tiles fall nine updates apart, and the hall leaves the updates before its next tile for the
    /// following room's main, which a hall entered again reads back on the same schedule.
    /// </summary>
    internal static void VerifyCeresFallingDebrisInterval()
    {
        // An inherited count of three drops the first tile on the fourth update.
        var carryover = new RoomMainHandOvers().ShaftTurnToDebris;
        carryover.LeaveDebrisWait(WrappingCountdown.Start(3));
        var hall = new CeresFallingDebrisRoomMainState(carryover);
        AssertEqual(4, UpdatesToTile(hall), "the hall's first tile falls on the update after the inherited count of three runs out");
        AssertEqual(8, carryover.FirstDebrisWait.RemainingFrames, "the hall leaves the eight updates before its next tile as the tile falls");
        for (int tile = 0; tile < 3; tile++)
            AssertEqual(9, UpdatesToTile(hall), $"later tile {tile} falls nine updates after the one before it");

        // A hall entered again mid-interval drops its next tile on the update the interval left would have.
        for (int update = 0; update < 4; update++)
            AssertTrue(!hall.Step(), "four updates into an interval no tile falls");
        AssertEqual(4, carryover.FirstDebrisWait.RemainingFrames, "four updates into an interval the hall leaves the four updates before its next tile");
        var reentered = new CeresFallingDebrisRoomMainState(carryover);
        AssertEqual(5, UpdatesToTile(reentered), "a hall entered again drops the tile the interval had left after five updates");

        // The shaft's backward sweep leaves its steps reading as run past zero, so the first tile falls at once.
        var backward = new RoomMainHandOvers().ShaftTurnToDebris;
        backward.LeaveTurnStep(68, backward: true);
        AssertEqual(1, UpdatesToTile(new CeresFallingDebrisRoomMainState(backward)),
            "a count the shaft's backward sweep left drops the first tile on the first update");

        // A count of zero wraps on its first step and drops the first tile at once; a count at the turn of the sign waits
        // its whole length.
        var zero = new RoomMainHandOvers().ShaftTurnToDebris;
        zero.LeaveDebrisWait(WrappingCountdown.Start(0));
        AssertEqual(1, UpdatesToTile(new CeresFallingDebrisRoomMainState(zero)), "an inherited count of zero drops the first tile at once");
        var longest = new RoomMainHandOvers().ShaftTurnToDebris;
        longest.LeaveDebrisWait(WrappingCountdown.Start(32768));
        AssertEqual(32769, UpdatesToTile(new CeresFallingDebrisRoomMainState(longest)),
            "an inherited count whose first step reads as not yet run past zero waits for its whole length");
    }

    /// <summary>The updates the hall steps until a tile falls, the one it falls on included.</summary>
    private static int UpdatesToTile(CeresFallingDebrisRoomMainState hall)
    {
        for (int update = 1; update <= 65536; update++)
        {
            if (hall.Step())
                return update;
        }
        throw new InvalidOperationException("The hall dropped no tile within a whole round of its count.");
    }

    /// <summary>The elevator pad holds Samus still for 59 updates and lowers her a pixel on the 60th and every update after.</summary>
    internal static void VerifyCeresElevatorPadWait()
    {
        SamusState samus = InstalledFixtures.CreateSamus();
        samus.Position = FixedPosition<RoomPixels>.FromWhole(new(128, 0));
        var arrival = new CeresElevatorArrivalState(samus);
        for (int update = 1; update < 60; update++)
        {
            arrival.Step(samus);
            AssertEqual(0, samus.Position.Y.Whole, $"the pad holds Samus still on update {update} of its wait");
        }
        arrival.Step(samus);
        AssertEqual(1, samus.Position.Y.Whole, "the pad lowers Samus on the update its wait runs out");
        arrival.Step(samus);
        AssertEqual(2, samus.Position.Y.Whole, "the pad lowers Samus on every update after its wait");
    }
}
