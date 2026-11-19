using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Bosses.Ridley.Artwork;

namespace SuperMetroid.Verification.Bosses.Ridley;

using SuperMetroid.Verification.Harness;

/// <summary>
/// #2329: Ridley's wing beat is a typed beat paced from his flight speed. Each update counts the pace of his faster axis's
/// speed off the beat's countdown, halved unless he climbs, and once the countdown runs below zero the wings take the
/// beat's next pose, the first after the last, and the countdown starts again from 32.
/// </summary>
internal static class RidleyWingBeatVerification
{
    /// <summary>The poses each Ridley starts in, a still Ridley's unmoving beat, and the beat's pace climbing, level and descending.</summary>
    internal static void VerifyRidleyWingBeat()
    {
        AssertEqual(RidleySupplementalVisualDefinitions.WingsRaised, new NorfairRidleyState().Flight.Wings.Pose.Entry, "Lower Norfair Ridley's wings start raised");
        AssertEqual(RidleySupplementalVisualDefinitions.WingsDown, new CeresRidleyState(Position16<RoomPixels>.Zero).Flight.Wings.Pose.Entry,
            "Ceres Ridley's wings start fully lowered");
        AssertEqual(10, RidleySupplementalVisualDefinitions.WingBeat.Count, "the wing beat holds ten poses");

        // Still, the beat counts nothing off and the wings keep their pose.
        RidleyFlightState still = TakenOff(SignedEightEight.Zero, SignedEightEight.Zero);
        for (int update = 0; update < 100; update++)
            RidleyFlight.TickWingAnimation(still);
        AssertEqual(0, still.Wings.Pose.Place, "a still Ridley's wings keep their pose");
        AssertEqual(RidleyWingDefinitions.TakeOffCountdown, still.Wings.Countdown.Left, "a still Ridley's beat counts nothing off");

        // Climbing at five pixels an update, the full pace of 48 shows a new pose every update, and the beat's tenth pose
        // is followed by its first.
        RidleyFlightState climbing = TakenOff(SignedEightEight.Zero, SignedEightEight.FromPixels(-5));
        for (int update = 1; update <= 12; update++)
        {
            RidleyFlight.TickWingAnimation(climbing);
            AssertEqual(update % 10, climbing.Wings.Pose.Place, $"climbing update {update}: a new pose every update, wrapping after the tenth");
            AssertEqual(RidleyWingDefinitions.BeatCountdown, climbing.Wings.Countdown.Left, $"climbing update {update}: the countdown starts again");
        }

        // Level at five pixels an update, half the pace, 24: 8 runs below zero, 32 does not, 8 does again.
        RidleyFlightState level = TakenOff(SignedEightEight.FromPixels(5), SignedEightEight.Zero);
        (int Place, int Countdown)[] levelBeat = [(1, 32), (1, 8), (2, 32), (2, 8), (3, 32)];
        foreach ((int place, int countdown) in levelBeat)
        {
            RidleyFlight.TickWingAnimation(level);
            AssertEqual((place, countdown), (level.Wings.Pose.Place, level.Wings.Countdown.Left), "level flight beats at half the pace");
        }

        // Descending at a quarter pixel an update, half the slowest-but-one pace of 14, 7: 8 counts down to 1, then below zero.
        RidleyFlightState descending = TakenOff(SignedEightEight.Zero, SignedEightEight.FromSixteenthsOfPixel(4));
        (int Place, int Countdown)[] descendingBeat = [(0, 1), (1, 32), (1, 25), (1, 18), (1, 11), (1, 4), (2, 32)];
        foreach ((int place, int countdown) in descendingBeat)
        {
            RidleyFlight.TickWingAnimation(descending);
            AssertEqual((place, countdown), (descending.Wings.Pose.Place, descending.Wings.Countdown.Left), "descending flight beats at half its speed's pace");
        }
        Console.WriteLine("  Ridley wing beat: the starting poses, a still beat, and the full climbing and halved level and descending paces hold.");
    }

    /// <summary>Lower Norfair Ridley's flight as he takes off, moving at <paramref name="x"/> and <paramref name="y"/>.</summary>
    private static RidleyFlightState TakenOff(SignedEightEight x, SignedEightEight y)
    {
        RidleyFlightState flight = new NorfairRidleyState().Flight;
        flight.Wings.Countdown.Restart(RidleyWingDefinitions.TakeOffCountdown);
        flight.Velocity = new EightEightVector<RoomPixels>(x, y);
        return flight;
    }

    /// <summary>Runs the wing beat check from the CLI.</summary>
    [VerificationCommand("--ridley-wing-beat")]
    internal static int RidleyWingBeatCommand()
    {
        Suite(nameof(VerifyRidleyWingBeat), () => VerifyRidleyWingBeat());
        return 0;
    }
}
