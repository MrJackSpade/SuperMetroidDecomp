using SuperMetroid.Core.Compatibility;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.PipeBugs;

/// <summary>Checks the Brinstar Pipe Bug's emergence (#1845) against the original's arithmetic on the height's fraction.</summary>
internal static class PipeBugEmergenceRiseVerification
{
    /// <summary>
    /// Each emerging update rises two pixels when the height's fraction is zero and one otherwise, and counts the fraction
    /// down past zero to the top of its range, as the original's separate decrement does; a placed bug, with no fraction,
    /// opens with a two-pixel rise, and a respawned bug takes its spawn height's whole pixels as its fraction.
    /// </summary>
    internal static void VerifyPipeBugEmergenceRise()
    {
        foreach (int fraction in new[] { 0, 1, 2, 144, ushort.MaxValue })
        {
            FixedCoordinate risen = PipeBugEmergenceRise.Risen(new FixedCoordinate(100, fraction));
            AssertEqual(fraction == 0 ? 98 : 99, risen.Whole, $"the rise from a fraction of {fraction}");
            AssertEqual((fraction - 1) & ushort.MaxValue, risen.Fraction, $"the fraction after a rise from {fraction}");
        }

        FixedCoordinate placed = new(200, 0);
        int[] rises = [.. Enumerable.Range(0, 3).Select(_ =>
        {
            FixedCoordinate next = PipeBugEmergenceRise.Risen(placed);
            int rise = placed.Whole - next.Whole;
            placed = next;
            return rise;
        })];
        AssertEqual("2,1,1", string.Join(',', rises), "a placed bug opens its emergence with a two-pixel rise");

        AssertEqual(new FixedCoordinate(144, 144), PipeBugEmergenceRise.Respawned(144), "a respawned bug's fraction is its spawn height");
        Console.WriteLine("  Pipe Bug emergence: the two-pixel rise at a zero fraction, the fraction's count and the respawned height match.");
    }
}
