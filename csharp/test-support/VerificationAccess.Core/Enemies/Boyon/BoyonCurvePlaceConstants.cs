namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>Cartridge values and helpers of <see cref="BoyonCurvePlace"/> that only verification reads.</summary>
internal static class BoyonCurvePlaceConstants
{
    /// <summary>The place past the top a rise leaves after its update at the top entry, from which the fall starts.</summary>
    public static BoyonCurvePlace PastTheTop => BoyonCurvePlace.Top.Higher;
    /// <summary>The place <paramref name="entryFromTop"/> entries down the curve from its top, zero or more.</summary>
    /// <param name="entryFromTop">The entry, counted from the top.</param>
    public static BoyonCurvePlace AtEntry(int entryFromTop) => new global::SuperMetroid.Core.Enemies.Boyon.BoyonCurvePlace(entryFromTop);
}
