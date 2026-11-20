namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>The immutable Puyo hop definitions, independent of animation artwork.</summary>
internal static class PuyoHopDefinitions
{
    /// <summary>
    /// Each hop's height, whole pixels moved across each update, launch search step, speed-curve step, airborne behaviour and the speed its drop falls at when a
    /// wall or ceiling cuts it short (#1820): one speed after a small, big or giant hop, faster after the long hop and
    /// slower after a dropped bounce.
    /// </summary>
    internal static readonly Dictionary<PuyoHopType, PuyoHopDefinition> Hops = new()
    {
        [PuyoHopType.NormalSmall] = new(16, 1, QuadraticSpeedCurvePlace.FromMarks(1), QuadraticSpeedCurvePlace.FromMarks(2), PuyoAirborneMovement.NormalShortHop, SignedSixteenSixteen.FromWholePixels(2)),
        [PuyoHopType.NormalBig] = new(32, 1, QuadraticSpeedCurvePlace.FromMarks(1), QuadraticSpeedCurvePlace.FromMarks(2), PuyoAirborneMovement.NormalBigHop, SignedSixteenSixteen.FromWholePixels(2)),
        [PuyoHopType.NormalLong] = new(32, 2, QuadraticSpeedCurvePlace.FromMarks(2), QuadraticSpeedCurvePlace.FromMarks(3), PuyoAirborneMovement.NormalLongHop, SignedSixteenSixteen.FromWholePixels(3)),
        [PuyoHopType.Giant] = new(128, 1, QuadraticSpeedCurvePlace.FromMarkFraction(5, 4), QuadraticSpeedCurvePlace.FromMarks(2), PuyoAirborneMovement.GiantHop, SignedSixteenSixteen.FromWholePixels(2)),
        [PuyoHopType.Dropping] = new(0, 0, QuadraticSpeedCurvePlace.Start, QuadraticSpeedCurvePlace.FromMarks(0), PuyoAirborneMovement.Dropping, SignedSixteenSixteen.FromWholePixels(1)),
        [PuyoHopType.DroppedSmall] = new(16, 1, QuadraticSpeedCurvePlace.FromMarks(1), QuadraticSpeedCurvePlace.FromMarkFraction(7, 4), PuyoAirborneMovement.Dropped, SignedSixteenSixteen.FromPixelFraction(7, 4)),
        [PuyoHopType.DroppedBig] = new(21, 1, QuadraticSpeedCurvePlace.FromMarks(1), QuadraticSpeedCurvePlace.FromMarkFraction(7, 4), PuyoAirborneMovement.Dropped, SignedSixteenSixteen.FromPixelFraction(7, 4)),
    };

    /// <summary>
    /// The random field a Puyo's hop roll starts from. The roll adds the enemy's frame counter and
    /// continues past the field's last value from zero.
    /// </summary>
    internal static readonly RandomSampleField HopRoll = new(0, 3);

    /// <summary>
    /// The lowest bit of <see cref="HopRoll"/>: the part of the roll a choice between two outcomes reads, the same bit
    /// whether the frame counter advances the whole roll or this part alone.
    /// </summary>
    private static readonly RandomSampleField HopRollParity = new(0, 1);

    /// <summary>
    /// The ordinary hop a Puyo that last made the small one takes next (#1619): its hop roll advanced by the enemy's frame
    /// counter chooses the small hop again or the big one, one time in two each.
    /// </summary>
    internal static readonly WeightedRandomChoice<PuyoHopType> HopAfterSmallHop = new(
        HopRollParity, new(1, PuyoHopType.NormalSmall), new(1, PuyoHopType.NormalBig));

    /// <summary>
    /// The ordinary hop a Puyo that last made the big or the long one takes next (#1619): its <see cref="HopRoll"/> advanced
    /// by the enemy's frame counter chooses the small hop one time in eight, the big one one time in eight and the long
    /// one six times in eight.
    /// </summary>
    internal static readonly WeightedRandomChoice<PuyoHopType> HopAfterLargerHop = new(
        HopRoll, new(1, PuyoHopType.NormalSmall), new(1, PuyoHopType.NormalBig), new(6, PuyoHopType.NormalLong));

    /// <summary>
    /// The chance a dropped Puyo's bounce is <see cref="PuyoHopType.DroppedBig"/> rather than
    /// <see cref="PuyoHopType.DroppedSmall"/> (#1619): its hop roll, advanced by the enemy's frame counter, chooses it one
    /// time in two.
    /// </summary>
    internal static readonly RandomChance BigDroppedBounce = RandomChance.Highest(1, HopRollParity);

    /// <summary>The farthest place along the quadratic speed curve an airborne Puyo's speed is read from.</summary>
    internal static QuadraticSpeedCurvePlace FastestSpeedPlace { get; } = QuadraticSpeedCurvePlace.FromMarks(64);
}
