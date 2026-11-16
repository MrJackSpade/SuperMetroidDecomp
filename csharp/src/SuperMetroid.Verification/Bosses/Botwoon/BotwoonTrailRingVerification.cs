using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Compatibility;

namespace SuperMetroid.Verification.Bosses.Botwoon;

/// <summary>
/// Checks Botwoon's trail ring (#1627): its 256 places, and that counting on past the last place or back past the first
/// continues round the ring, so a place recurs every 256 moving updates.
/// </summary>
internal static class BotwoonTrailRingVerification
{
    /// <summary>Runs the ring's place and wrap checks.</summary>
    internal static void VerifyBotwoonTrailRing()
    {
        AssertEqual(256, BotwoonTrailRing.Places, "the trail ring holds the head's last 256 moving positions");
        AssertEqual(BotwoonTrailRing.Places, new BotwoonEnemyState().History.Length, "Botwoon's trail holds one position for each ring place");
        for (int place = -3 * BotwoonTrailRing.Places; place < 3 * BotwoonTrailRing.Places; place++)
        {
            int expected = ((place % 256) + 256) % 256;
            AssertEqual(expected, BotwoonTrailRing.PlaceAt(place), $"ring place counted to {place}");
            AssertEqual(BotwoonTrailRing.PlaceAt(place), BotwoonTrailRing.PlaceAt(place + BotwoonTrailRing.Places), $"ring place {place} recurs a whole ring later");
        }
        AssertEqual(0, BotwoonTrailRing.PlaceAt(255 + 1), "the head's next place after the ring's last is its first");
        AssertEqual(235, BotwoonTrailRing.PlaceAt(3 - 4 * 6), "a segment four spacings of six behind ring place three reaches back round the ring");
        Console.WriteLine("Botwoon trail ring: 256 places, wrapping both ways with the ring's period.");
    }
}
