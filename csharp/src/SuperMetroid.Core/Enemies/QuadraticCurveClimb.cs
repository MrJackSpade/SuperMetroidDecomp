namespace SuperMetroid.Core.Enemies;

/// <summary>
/// How a climb that accelerates along the shared quadratic speed curve is planned (#2026, #2065): the height it climbs and
/// how far it steps along the curve each pass, which fix the place it starts its climb from (<see cref="StartPlace"/>).
/// The Beetom's leaps, the hoppers' hops and the Ki-Hunter's detached wings plan their climbs this way.
/// </summary>
/// <param name="Height">How high the climb rises, none or more: 16 to 64 pixels for the climbs the game plans.</param>
/// <param name="StepPerPass">How far along the curve each pass of the search steps.</param>
internal readonly partial record struct QuadraticCurveClimb(SignedSixteenSixteen Height, QuadraticSpeedCurvePlace StepPerPass)
{
    /// <summary>
    /// The most passes a search can make before its place leaves the curve: the finest step leaves it after this many.
    /// </summary>
    private static int MostPasses => QuadraticSpeedCurvePlace.PastLastMark.StepsOf(QuadraticSpeedCurvePlace.FinestStep);

    /// <summary>
    /// The place the climb starts from: stepping from the curve's start <see cref="StepPerPass"/> a pass, the first place
    /// at which the coarse speeds (<see cref="QuadraticCurveSpeed"/>) of the places passed sum to <see cref="Height"/>;
    /// at which the coarse speeds (<see cref="QuadraticCurveSpeed"/>) of the places passed sum to <see cref="Height"/>.
    /// The height is reached within the curve's marks or never: past them the climb is rejected, and a climb that steps
    /// nowhere sums the start's speed of zero only (#1627).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The height is negative.</exception>
    /// <exception cref="InvalidDataException">The climb does not reach its height within the curve's marks.</exception>
    internal QuadraticSpeedCurvePlace StartPlace
    {
        get
        {
            if (Height.IsNegative)
                throw new ArgumentOutOfRangeException(nameof(Height), Height, "A climb rises no lower than nothing.");
            QuadraticSpeedCurvePlace place = QuadraticSpeedCurvePlace.Start;
            SignedSixteenSixteen climbed = SignedSixteenSixteen.Zero;
            for (int pass = 0; pass < MostPasses; pass++)
            {
                place += StepPerPass;
                climbed += place.Speed.CoarseRise;
                if (climbed >= Height)
                    return place;
            }
            throw new InvalidDataException(
                $"A climb of {Height.InTwoFiftySixths} 256ths of a pixel at {StepPerPass.InTwoHundredFiftySixthsOfMark} 256ths of a mark a pass never reaches its height.");
        }
    }
}
