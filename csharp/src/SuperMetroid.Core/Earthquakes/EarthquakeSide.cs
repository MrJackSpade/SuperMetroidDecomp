namespace SuperMetroid.Core.Earthquakes;

/// <summary>
/// The side of its resting place a shaking room shows on a frame (#2421): its shake's displacement, or the far side opposite
/// it. Its values are arbitrary.
/// </summary>
public enum EarthquakeSide
{
    /// <summary>The room shows its shake's displacement.</summary>
    Near,

    /// <summary>The room shows the side opposite its shake's displacement: the displacement negated.</summary>
    Far,
}

/// <summary>The displacement an earthquake's side shows (#2569).</summary>
public static class EarthquakeSides
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
