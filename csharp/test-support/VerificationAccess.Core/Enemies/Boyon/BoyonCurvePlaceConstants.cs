namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>Cartridge values and helpers of <see cref="BoyonCurvePlace"/> that only verification reads.</summary>
internal static class BoyonCurvePlaceConstants
{
    /// <summary>The place past the top a rise leaves after its update at the top entry, from which the fall starts.</summary>
    public static BoyonCurvePlace PastTheTop => global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Enemies.Boyon.BoyonCurvePlace>([typeof(int)], new object?[] { -1 });
    /// <summary>The place <paramref name="entryFromTop"/> entries down the curve from its top, zero or more.</summary>
    /// <param name="entryFromTop">The entry, counted from the top.</param>
    public static BoyonCurvePlace AtEntry(int entryFromTop) => global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Enemies.Boyon.BoyonCurvePlace>([typeof(int)], new object?[] { Ensure.AtLeastZero(entryFromTop) });
}
