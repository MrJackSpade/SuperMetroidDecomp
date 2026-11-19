namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// How fast Phantoon runs his figure-eight path (#2555): places of the path an update, with a fraction of a place carried
/// between updates, zero or more forward and zero or less backward. Each update he takes the speed's whole places along
/// the path; the fraction only decides on which update a speed change reaches the next whole place.
/// </summary>
public readonly record struct PhantoonPathSpeed
{
    /// <summary>Keeps the speed in places an update, fraction included, at the precision the game steps it in.</summary>
    private PhantoonPathSpeed(SignedSixteenSixteen places) => _places = places;

    /// <summary>The speed in places an update, fraction included.</summary>
    private readonly SignedSixteenSixteen _places;

    /// <summary>No movement along the path.</summary>
    public static PhantoonPathSpeed Zero => default;

    /// <summary>A speed of <paramref name="places"/> whole places an update, negative backward.</summary>
    public static PhantoonPathSpeed FromPlaces(int places) => new(SignedSixteenSixteen.FromWholePixels(places));

    /// <summary>
    /// A speed of <paramref name="numerator"/> <paramref name="denominator"/>ths of a place an update, the denominator a
    /// power of two from 1 through 65536: three 128ths of a place, or a sixteenth.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The denominator is no power of two from 1 through 65536.</exception>
    public static PhantoonPathSpeed FromPlaceFraction(int numerator, int denominator) =>
        new(SignedSixteenSixteen.FromPixelFraction(numerator, denominator));

    /// <summary>The whole places the speed takes each update, its fraction dropped toward fewer places.</summary>
    [IntegerQuantity("whole places of Phantoon's figure-eight path taken each update")]
    public int WholePlaces => _places.WholePixels;

    /// <summary>The speed raised by <paramref name="change"/>, its fraction carrying into the whole places.</summary>
    public static PhantoonPathSpeed operator +(PhantoonPathSpeed speed, PhantoonPathSpeed change) => new(speed._places + change._places);

    /// <summary>The speed lowered by <paramref name="change"/>, a fraction below none taken from the whole places.</summary>
    public static PhantoonPathSpeed operator -(PhantoonPathSpeed speed, PhantoonPathSpeed change) => new(speed._places - change._places);
}
