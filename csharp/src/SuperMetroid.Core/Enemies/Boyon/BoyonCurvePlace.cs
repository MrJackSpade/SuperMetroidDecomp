namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>
/// Where a Boyon stands along its bounce curve (<see cref="BoyonBounceCurve"/>, #1684): an entry of the curve, counted
/// from the top of the arc, where the Boyon moves slowest, down toward the baseline its fall ends at; or past the top,
/// where a rise leaves it after its update at the top entry. A fall starts from past the top by moving one entry lower,
/// back to the top entry, so a bounce spends two updates at the top of its arc. Places only move one entry at a time and
/// compare; a rise never starts past the top, and a fall never reads the curve there.
/// </summary>
public readonly record struct BoyonCurvePlace : IComparable<BoyonCurvePlace>
{
    /// <summary>The entry from the top; none past the top.</summary>
    private readonly int entry;

    /// <summary>Keeps the place's entry, zero or more.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="entry"/> is negative.</exception>
    internal BoyonCurvePlace(int entry) => this.entry = Ensure.AtLeastZero(entry);

    /// <summary>The place past the top a rise leaves.</summary>
    private BoyonCurvePlace(bool pastTheTop) => IsPastTheTop = pastTheTop;

    /// <summary>The top entry of the curve, where a Boyon moves slowest: the place a Boyon starts from.</summary>
    public static BoyonCurvePlace Top => default;

    /// <summary>Whether this is the place past the top a rise leaves.</summary>
    public bool IsPastTheTop { get; }

    /// <summary>The entry from the top whose sample the Boyon's multiplier scales here.</summary>
    /// <exception cref="InvalidOperationException">The place is past the top, where no entry is read.</exception>
    [IntegerQuantity("entry of a Boyon's bounce curve, counted from the top of the arc")]
    public int EntryFromTop => IsPastTheTop
        ? throw new InvalidOperationException("A Boyon past the top of its arc stands at no entry of its bounce curve.")
        : entry;

    /// <summary>The place one entry further down the curve, as a fall moves: from past the top, the top entry.</summary>
    public BoyonCurvePlace Lower => IsPastTheTop ? Top : new(checked(entry + 1));

    /// <summary>The place one entry further up the curve, as a rise moves: from the top entry, past the top.</summary>
    /// <exception cref="InvalidOperationException">The place is already past the top.</exception>
    public BoyonCurvePlace Higher => IsPastTheTop
        ? throw new InvalidOperationException("A Boyon past the top of its arc rises no further.")
        : entry == 0 ? new BoyonCurvePlace(pastTheTop: true) : new(entry - 1);

    /// <summary>Orders places from the top down: past the top first, then each entry by its distance from the top.</summary>
    public int CompareTo(BoyonCurvePlace other) => IsPastTheTop || other.IsPastTheTop
        ? other.IsPastTheTop.CompareTo(IsPastTheTop)
        : entry.CompareTo(other.entry);

    /// <summary>Whether <paramref name="left"/> lies above <paramref name="right"/> on the curve.</summary>
    public static bool operator <(BoyonCurvePlace left, BoyonCurvePlace right) => left.CompareTo(right) < 0;

    /// <summary>Whether <paramref name="left"/> lies below <paramref name="right"/> on the curve.</summary>
    public static bool operator >(BoyonCurvePlace left, BoyonCurvePlace right) => left.CompareTo(right) > 0;
}
