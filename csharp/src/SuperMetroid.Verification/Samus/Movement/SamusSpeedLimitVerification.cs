using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Verification.Samus.Movement;

/// <summary>Checks how Samus's speeds are tested against their maximum and their dash caps (#1621).</summary>
internal static class SamusSpeedLimitVerification
{
    private static readonly int[] BoundaryParts = [0, 1, 2, 3, 7, 8, 0x4000, 0x7fff, 0x8000, 0x8001, 0xc000, 0xc001, 0xffff];

    /// <summary>
    /// Confirms <see cref="SamusSpeedMagnitude.ReadsPastLimit"/> (the strict tie in whole pixels, a fraction or whole
    /// speed more than half its range above the limit's reading as under it) and <see cref="SamusSpeedMagnitude.ReadsAtCap"/>
    /// (a fraction of half a pixel or more failing the cap whatever the whole pixels), each against the part-wise sign
    /// tests they replaced, and both at their sites in Samus's horizontal speed.
    /// </summary>
    internal static void VerifySamusSpeedLimits()
    {
        var maximum = new UnsignedSixteenSixteen(2, 0xc000);
        AssertTrue(!SamusSpeedMagnitude.ReadsPastLimit(maximum, maximum), "a speed exactly at its maximum is not past it");
        AssertTrue(SamusSpeedMagnitude.ReadsPastLimit(new(2, 0xc001), maximum), "a fraction one step above the maximum's is past it");
        AssertTrue(SamusSpeedMagnitude.ReadsPastLimit(new(3, 0), maximum), "whole pixels above the maximum's are past it whatever the fraction");
        AssertTrue(!SamusSpeedMagnitude.ReadsPastLimit(new(2, 0x4000), maximum), "a slower speed is not past its maximum");
        AssertTrue(!SamusSpeedMagnitude.ReadsPastLimit(new(0x8003, 0), maximum), "whole pixels more than half the range above the maximum's read as under it");
        AssertTrue(!SamusSpeedMagnitude.ReadsPastLimit(new(2, 0xc001), new(2, 0x4000)), "a fraction more than half a pixel above the maximum's reads as under it");

        var dashCap = new UnsignedSixteenSixteen(2, 0);
        AssertTrue(SamusSpeedMagnitude.ReadsAtCap(dashCap, dashCap), "a speed exactly at its cap is held");
        AssertTrue(!SamusSpeedMagnitude.ReadsAtCap(new(1, 0xf000), dashCap), "a slower speed is not held");
        AssertTrue(SamusSpeedMagnitude.ReadsAtCap(new(3, 0x7fff), dashCap), "a faster speed under half a pixel's fraction is held");
        AssertTrue(!SamusSpeedMagnitude.ReadsAtCap(new(3, 0x8000), dashCap), "a fraction of half a pixel fails the cap although the speed is above it");
        AssertTrue(!SamusSpeedMagnitude.ReadsAtCap(new(0x8007, 0), new(7, 0)), "whole pixels half the range above the cap's read as under it");

        int compared = 0;
        int unlikeOneNumber = 0;
        foreach (int whole in BoundaryParts)
        foreach (int fraction in BoundaryParts)
        foreach (int limitWhole in new[] { 0, 2, 7 })
        foreach (int limitFraction in new[] { 0, 0x4000, 0xc000 })
        {
            var speed = new UnsignedSixteenSixteen(whole, fraction);
            var limit = new UnsignedSixteenSixteen(limitWhole, limitFraction);
            bool replacedPast = ReplacedIsGreater(whole, fraction, limitWhole, limitFraction);
            bool replacedCap = ReplacedPartsAtLeast(whole, fraction, limitWhole, limitFraction);
            AssertEqual(replacedPast, SamusSpeedMagnitude.ReadsPastLimit(speed, limit), $"{whole:X4}.{fraction:X4} past {limitWhole}.{limitFraction:X4}");
            AssertEqual(replacedCap, SamusSpeedMagnitude.ReadsAtCap(speed, limit), $"{whole:X4}.{fraction:X4} at cap {limitWhole}.{limitFraction:X4}");
            if (replacedCap != (speed.RawValue >= limit.RawValue))
                unlikeOneNumber++;
            compared++;
        }
        AssertTrue(unlikeOneNumber > 0, "some samples separate the part-wise cap test from a comparison of the speeds as one number");

        // The dash's extra speed with a fraction of half a pixel or more is not held at the Speed Booster's seven pixels:
        // it keeps accelerating.
        var boosted = new SamusHorizontalSpeedState(SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus().WaterSpeedsInAir) { ExtraRunSpeed = new UnsignedSixteenSixteen(7, 0x8000) };
        boosted.HandleExtraRunSpeed(SamusMovementType.Running, ControllerButton.B, speedBoosterEquipped: true);
        AssertEqual(new UnsignedSixteenSixteen(7, 0x8000) + SamusMovementDefinitions.HorizontalMotion.ExtraRunAcceleration, boosted.ExtraRunSpeed,
            "an extra speed above seven pixels with half a pixel's fraction is not held at the cap");
        var held = new SamusHorizontalSpeedState(SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus().WaterSpeedsInAir) { ExtraRunSpeed = new UnsignedSixteenSixteen(7, 0x7fff) };
        held.HandleExtraRunSpeed(SamusMovementType.Running, ControllerButton.B, speedBoosterEquipped: true);
        AssertEqual(new UnsignedSixteenSixteen(7, 0), held.ExtraRunSpeed, "an extra speed above seven pixels under half a pixel's fraction is held at the cap");

        // A base speed accelerated exactly onto its maximum keeps it; one past it in the fraction is replaced by it.
        var entry = new SamusHorizontalSpeeds(SignedSixteenSixteen.FromParts(0, 0x3000), SignedSixteenSixteen.FromParts(2, 0xc000), SignedSixteenSixteen.FromParts(0, 0x8000));
        var exact = new SamusHorizontalSpeedState(SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus().WaterSpeedsInAir) { BaseSpeed = new UnsignedSixteenSixteen(2, 0x9000) };
        exact.CalculateBaseSpeed(entry);
        AssertEqual(new UnsignedSixteenSixteen(2, 0xc000), exact.BaseSpeed, "a speed accelerated onto its maximum keeps it");
        var past = new SamusHorizontalSpeedState(SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus().WaterSpeedsInAir) { BaseSpeed = new UnsignedSixteenSixteen(2, 0x9001) };
        past.CalculateBaseSpeed(entry);
        AssertEqual(new UnsignedSixteenSixteen(2, 0xc000), past.BaseSpeed, "a speed accelerated past its maximum is replaced by it");
        var underFraction = new SamusHorizontalSpeedState(SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus().WaterSpeedsInAir) { BaseSpeed = new UnsignedSixteenSixteen(2, 0x4001) };
        underFraction.CalculateBaseSpeed(entry with { MaximumSpeed = SignedSixteenSixteen.FromParts(2, 0x4000) });
        AssertEqual(new UnsignedSixteenSixteen(2, 0x4000), underFraction.BaseSpeed,
            "a speed whose fraction lies less than half a pixel above its maximum's is replaced by it");
        var halfRangeFraction = new SamusHorizontalSpeedState(SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus().WaterSpeedsInAir) { BaseSpeed = new UnsignedSixteenSixteen(2, 0x9001) };
        halfRangeFraction.CalculateBaseSpeed(entry with { MaximumSpeed = SignedSixteenSixteen.FromParts(2, 0x4000) });
        AssertEqual(new UnsignedSixteenSixteen(2, 0xc001), halfRangeFraction.BaseSpeed,
            "a speed whose fraction lies more than half a pixel above its maximum's reads as under it and keeps accelerating");
        Console.WriteLine($"Samus speed limits: the strict maximum, the part-wise cap and their half-range readings hold; {compared} speed pairs agree with the replaced sign tests.");
    }

    /// <summary>Whether a part reaches a threshold as the replaced test read it: their difference taken as a signed sixteen-bit value is not negative.</summary>
    private static bool ReplacedAtLeast(int part, int threshold) => unchecked((short)(part - threshold)) >= 0;

    /// <summary>The replaced maximum test: the whole pixels reach the limit's, and differ from them or the fraction reaches and differs from the limit's.</summary>
    private static bool ReplacedIsGreater(int whole, int fraction, int limitWhole, int limitFraction) =>
        ReplacedAtLeast(whole, limitWhole) &&
        (whole != limitWhole || (fraction != limitFraction && ReplacedAtLeast(fraction, limitFraction)));

    /// <summary>The replaced cap test: both parts reach the cap's, each on its own.</summary>
    private static bool ReplacedPartsAtLeast(int whole, int fraction, int capWhole, int capFraction) =>
        ReplacedAtLeast(whole, capWhole) && ReplacedAtLeast(fraction, capFraction);
}
