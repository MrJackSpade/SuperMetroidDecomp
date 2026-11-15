namespace SuperMetroid.Core.Enemies;

/// <summary>
/// How a hop that accelerates along the shared quadratic speed curve climbs (#2026): the height it climbs and the marks it
/// steps along the curve each pass, which fix the place it starts its climb from (<see cref="StartPlace"/>).
/// </summary>
/// <param name="Height">How high the hop climbs, no higher than <see cref="QuadraticSpeedCurveDefinitions.LargestClimbHeight"/> pixels.</param>
/// <param name="MarksPerPass">How many whole marks along the curve each pass of the search steps.</param>
internal readonly record struct QuadraticCurveClimb(SignedSixteenSixteen Height, int MarksPerPass)
{
    /// <summary>
    /// The place the hop starts its climb from, as the Beetom and the hoppers choose it: stepping from the curve's start
    /// <see cref="MarksPerPass"/> marks a pass, the first place at which the coarse speeds (<see cref="QuadraticCurveSpeed"/>)
    /// of the places passed sum to <see cref="Height"/>; summing full speeds would change every hop. The height is reached
    /// within the curve's marks or never: past them reading the speed rejects the climb before the place could grow
    /// further, and a climb of no marks a pass sums the start's speed of zero only (#1627).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The height is negative or above the largest climb height.</exception>
    /// <exception cref="InvalidDataException">The climb does not reach its height within the curve's marks.</exception>
    internal QuadraticSpeedCurvePlace StartPlace
    {
        get
        {
            if (Height.IsNegative || Height.InTwoFiftySixths > SignedSixteenSixteen.FromWholePixels(QuadraticSpeedCurveDefinitions.LargestClimbHeight).InTwoFiftySixths)
                throw new ArgumentOutOfRangeException(nameof(Height), Height,
                    $"A hop climbs no lower than nothing and no higher than {QuadraticSpeedCurveDefinitions.LargestClimbHeight} pixels.");
            QuadraticSpeedCurvePlace step = QuadraticSpeedCurvePlace.FromMarks(MarksPerPass);
            QuadraticSpeedCurvePlace place = QuadraticSpeedCurvePlace.Start;
            SignedSixteenSixteen climbed = SignedSixteenSixteen.Zero;
            for (int pass = 0; pass < QuadraticSpeedCurveDefinitions.MarkCount; pass++)
            {
                place += step;
                climbed += SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(place.Speed.CoarseToward(EnemySpeedSign.Positive));
                if (climbed.InTwoFiftySixths >= Height.InTwoFiftySixths)
                    return place;
            }
            throw new InvalidDataException($"A hop climbing {Height.InTwoFiftySixths} 256ths of a pixel at {MarksPerPass} marks a pass never reaches its height.");
        }
    }
}
