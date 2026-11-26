using Engine.Actors;
using Engine.Shared;

namespace Game.Compatibility;

/// <summary>
/// The cartridge's reading of Samus's speed against its limits, and of how fast she moves up or down. It keeps a
/// speed as a size with its direction apart: one worn past rest, as gravity wears the speed of a rise at its top or
/// of a moonfall, goes on from the top of its range, where it reads as below whatever a fall is asked about. So a
/// moonfall's ball lands with no rebound, its landing is no hard one, and it shows no fast fall. It compares the whole pixels and the fraction of a
/// pixel each on its own, by the sign of their difference taken within one pixel's range, so a fraction more than
/// half a pixel above a limit's reads as below it: running from rest reads as past 2.75 at 2.0625 and is 2.75 a few
/// updates early, a speed of three quarters of a pixel does not read as past a top speed of none, and a dash whose
/// fraction is half a pixel or more over its cap is not held by it.
/// </summary>
public sealed class CartridgeSpeedReadings : ISpeedReadings
{
    /// <summary>The half of a part's range a difference must stay within to keep its sign.</summary>
    private const int HalfRange = Pixels.FractionsPerPixel / 2;

    /// <summary>Past the top: the whole pixels read as above the top's, or equal them while the fraction reads as above the top's.</summary>
    public bool IsPastTop(Pixels speed, Pixels top) =>
        ReadsAbove(Part(speed.Whole), Part(top.Whole)) || (speed.Whole == top.Whole && ReadsAbove(speed.Fraction, top.Fraction));

    /// <summary>At the cap: the whole pixels and the fraction each read as reaching the cap's, on their own.</summary>
    public bool IsAtCap(Pixels dash, Pixels cap) =>
        ReadsAtLeast(Part(dash.Whole), Part(cap.Whole)) && ReadsAtLeast(dash.Fraction, cap.Fraction);

    /// <summary>At least so many whole pixels: the size's whole pixels, as the cartridge holds them, read as reaching the count.</summary>
    public bool ReachesWhole(Pixels size, int whole) => ReadsAtLeast(Part(size.Whole), Part(whole));

    /// <summary>
    /// The whole pixels of a speed's size as the cartridge holds them, as a plain count from nought to the top of
    /// the range: what the cartridge compares where it does not go by the sign of a difference.
    /// </summary>
    public static int WholeAsCount(Pixels size) => Part(size.Whole);

    /// <summary>A count of whole pixels as the cartridge holds it: within the same range as a fraction.</summary>
    private static int Part(int whole) => ((whole % Pixels.FractionsPerPixel) + Pixels.FractionsPerPixel) % Pixels.FractionsPerPixel;

    /// <summary>Whether a part reads as above another: it reads as reaching it and is not equal to it.</summary>
    private static bool ReadsAbove(int part, int other) => part != other && ReadsAtLeast(part, other);

    /// <summary>Whether a part reads as reaching another: their difference keeps a sign that is not negative within the range.</summary>
    private static bool ReadsAtLeast(int part, int other)
    {
        int difference = part - other;
        return difference is (>= 0 and < HalfRange) or < -HalfRange;
    }
}
