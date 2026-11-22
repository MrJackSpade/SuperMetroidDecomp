namespace SuperMetroid.Core.Earthquakes;

/// <summary>The displacement an earthquake's side shows (#2569).</summary>
public static partial class EarthquakeSides
{
    /// <summary>
    /// The displacement <paramref name="side"/> shows of <paramref name="displacement"/>: as it is from the near side, and
    /// negated from the far side.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="side"/> is undefined.</exception>
    public static T Showing<T>(this EarthquakeSide side, T displacement) where T : System.Numerics.IUnaryNegationOperators<T, T> => side switch
    {
        EarthquakeSide.Near => displacement,
        EarthquakeSide.Far => -displacement,
        _ => throw new ArgumentOutOfRangeException(nameof(side), side, $"Undefined {nameof(EarthquakeSide)}."),
    };
}
