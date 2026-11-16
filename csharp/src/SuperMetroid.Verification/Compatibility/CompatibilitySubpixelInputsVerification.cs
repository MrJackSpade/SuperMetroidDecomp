using SuperMetroid.Core.Compatibility;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Compatibility;

using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks the compatibility services that keep a sub-pixel position for later (#1627): the rainbow beam's hidden push,
/// the shots' fractions a room hands over and the elevatube's fraction a room main leaves each take every fraction up to
/// the coordinate's last (<see cref="FixedCoordinate.LastFraction"/>) and refuse one past it, and give back what they took.
/// </summary>
internal static class CompatibilitySubpixelInputsVerification
{
    /// <summary>Checks each service's sub-pixel input at its last fraction, one past it and below zero.</summary>
    internal static void VerifyCompatibilitySubpixelInputs()
    {
        const int last = FixedCoordinate.LastFraction;
        var checkpoint = FixedPosition<RoomPixels>.FromStoredParts(10, 0x1234, 20, 0x5678);

        var complete = new RainbowBeamCameraCheckpoint();
        complete.HideHorizontalMovement(last);
        complete.HideVerticalMovement(last);
        AssertEqual(FixedPosition<RoomPixels>.FromStoredParts(10, last, 20, last), complete.Consume(checkpoint),
            "a complete hide takes the last fraction whole");
        var coarse = new RainbowBeamCameraCheckpoint();
        coarse.HideCoarseHorizontalMovement(last);
        coarse.HideCoarseVerticalMovement(last);
        AssertEqual(FixedPosition<RoomPixels>.FromStoredParts(10, 0xff34, 20, 0xff78), coarse.Consume(checkpoint),
            "a coarse hide of the last fraction replaces only its coarse part");
        var refused = new RainbowBeamCameraCheckpoint();
        AssertThrows<ArgumentOutOfRangeException>(() => refused.HideHorizontalMovement(last + 1), "a complete hide past the last fraction");
        AssertThrows<ArgumentOutOfRangeException>(() => refused.HideCoarseVerticalMovement(last + 1), "a coarse hide past the last fraction");
        AssertThrows<ArgumentOutOfRangeException>(() => refused.HideVerticalMovement(-1), "a complete hide below zero");
        AssertEqual(checkpoint, refused.Consume(checkpoint), "refused hides leave the record as it was");

        var handOver = new RoomHandOver();
        handOver.LeaveShots([0, last]);
        AssertEqual(0, handOver.ShotHorizontalFraction(0), "a shot place hands over a zero fraction");
        AssertEqual(last, handOver.ShotHorizontalFraction(1), "a shot place hands over the last fraction");
        AssertThrows<ArgumentOutOfRangeException>(() => handOver.LeaveShots([last + 1]), "a shot fraction past the last");
        AssertThrows<ArgumentOutOfRangeException>(() => handOver.LeaveShots([-1]), "a negative shot fraction");
        AssertEqual(last, handOver.ShotHorizontalFraction(1), "a refused hand-over keeps what the room before left");

        var carryover = new RoomMainCarryover();
        carryover.LeaveFraction(last);
        AssertEqual(last, carryover.Fraction, "a room main leaves the last fraction");
        AssertThrows<ArgumentOutOfRangeException>(() => carryover.LeaveFraction(last + 1), "a room main's fraction past the last");
        AssertThrows<ArgumentOutOfRangeException>(() => carryover.LeaveFraction(-1), "a negative room main fraction");
        AssertEqual(last, carryover.Fraction, "a refused fraction keeps what the room main left");
        Console.WriteLine("Compatibility sub-pixel inputs: the rainbow beam's hide, the shots' hand-over and the room main's fraction take every fraction to the last and refuse one past it.");
    }

    /// <summary>Dispatches the compatibility services' sub-pixel input checks.</summary>
    [VerificationCommand("--compatibility-subpixel-inputs")]
    internal static int CompatibilitySubpixelInputsCommand()
    {
        Suite(nameof(VerifyCompatibilitySubpixelInputs), () => VerifyCompatibilitySubpixelInputs());
        return 0;
    }
}
