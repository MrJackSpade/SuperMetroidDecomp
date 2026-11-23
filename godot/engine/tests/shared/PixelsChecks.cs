using Engine.Shared;

namespace Engine.Tests.Shared;

/// <summary>Checks of exact distances: their whole and fractional parts and their arithmetic.</summary>
public sealed class PixelsChecks
{
    [Check]
    public void WholePixelsHaveNoFraction()
    {
        Pixels distance = Pixels.FromWhole(12);
        Expect.Equal(12, distance.Whole);
        Expect.Equal(0, distance.Fraction);
    }

    [Check]
    public void ANegativeDistanceKeepsANonNegativeFraction()
    {
        Pixels quarter = Pixels.FromParts(0, Pixels.FractionsPerPixel / 4);
        Pixels minusQuarter = -quarter;
        Expect.Equal(-1, minusQuarter.Whole);
        Expect.Equal(Pixels.FractionsPerPixel * 3 / 4, minusQuarter.Fraction);
        Expect.True(minusQuarter.IsNegative);
    }

    [Check]
    public void FractionsCarryIntoTheWholePixel()
    {
        Pixels half = Pixels.FromParts(0, Pixels.FractionsPerPixel / 2);
        Pixels sum = Pixels.FromWhole(3) + half + half;
        Expect.Equal(Pixels.FromWhole(4), sum);
    }

    [Check]
    public void AddingAndSubtractingTheSameDistanceReturnsToTheStart()
    {
        Pixels start = Pixels.FromParts(-7, 1234);
        Pixels step = Pixels.FromParts(2, 60000);
        Expect.Equal(start, start + step - step);
    }

    [Check]
    public void TheWholeAndTheFractionCanEachBeReplacedAlone()
    {
        Pixels distance = Pixels.FromParts(5, 100);
        Expect.Equal(Pixels.FromParts(9, 100), distance.WithWhole(9));
        Expect.Equal(Pixels.FromParts(5, Pixels.FractionsPerPixel - 1), distance.WithFraction(Pixels.FractionsPerPixel - 1));
    }

    [Check]
    public void DistancesCompareByTheirTrueSize()
    {
        Expect.True(Pixels.FromParts(-1, 1) < Pixels.Zero);
        Expect.True(Pixels.FromParts(2, 1) > Pixels.FromWhole(2));
        Expect.Equal(Pixels.FromParts(1, 500), (-Pixels.FromParts(1, 500)).Abs());
    }

    [Check]
    public void AFractionOutsideOnePixelIsRefused()
    {
        Expect.Throws<ArgumentOutOfRangeException>(() => Pixels.FromParts(0, Pixels.FractionsPerPixel));
        Expect.Throws<ArgumentOutOfRangeException>(() => Pixels.FromParts(0, -1));
    }
}
