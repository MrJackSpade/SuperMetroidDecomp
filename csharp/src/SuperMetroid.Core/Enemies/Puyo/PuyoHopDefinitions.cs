namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>Seven immutable Puyo hop definitions, independent of animation artwork.</summary>
public static class PuyoHopDefinitions
{
    /// <summary>
    /// Each hop's height, horizontal speed, speed-curve step, airborne behaviour and the speed its drop falls at when a
    /// wall or ceiling cuts it short (#1820): two pixels per frame after a small, big or giant hop, three after the long
    /// hop and a pixel and three quarters after a dropped bounce, the same number as the hop's curve step.
    /// </summary>
    internal static readonly Dictionary<PuyoHopType, PuyoHopDefinition> Hops = new()
    {
        [PuyoHopType.NormalSmall] = new(16, 256, QuadraticSpeedCurvePlace.FromMarks(2), PuyoAirborneMovement.NormalShortHop, SignedSixteenSixteen.FromWholePixels(2)),
        [PuyoHopType.NormalBig] = new(32, 256, QuadraticSpeedCurvePlace.FromMarks(2), PuyoAirborneMovement.NormalBigHop, SignedSixteenSixteen.FromWholePixels(2)),
        [PuyoHopType.NormalHigh] = new(32, 512, QuadraticSpeedCurvePlace.FromMarks(3), PuyoAirborneMovement.NormalLongHop, SignedSixteenSixteen.FromWholePixels(3)),
        [PuyoHopType.Giant] = new(128, 320, QuadraticSpeedCurvePlace.FromMarks(2), PuyoAirborneMovement.GiantHop, SignedSixteenSixteen.FromWholePixels(2)),
        [PuyoHopType.Dropping] = new(0, 0, QuadraticSpeedCurvePlace.FromMarks(0), PuyoAirborneMovement.Dropping, SignedSixteenSixteen.FromWholePixels(1)),
        [PuyoHopType.DroppedSmall] = new(16, 256, QuadraticSpeedCurvePlace.FromTwoHundredFiftySixthsOfMark(448), PuyoAirborneMovement.Dropped, SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(448)),
        [PuyoHopType.DroppedBig] = new(21, 256, QuadraticSpeedCurvePlace.FromTwoHundredFiftySixthsOfMark(448), PuyoAirborneMovement.Dropped, SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(448)),
    };

    /// <summary>
    /// The random field a Puyo's hop roll starts from: the sample modulo 8. The roll adds the enemy's frame counter and
    /// continues past the field's last value from zero.
    /// </summary>
    internal static readonly RandomSampleField HopRoll = new(0, 3);

    /// <summary>
    /// The lowest bit of <see cref="HopRoll"/>: the part of the roll a choice between two outcomes reads, the same bit
    /// whether the frame counter advances the whole roll or this bit alone.
    /// </summary>
    private static readonly RandomSampleField HopRollParity = new(0, 1);

    /// <summary>
    /// The ordinary hop a Puyo that last made the small one takes next (#1619): the parity of its hop roll advanced by the
    /// enemy's frame counter chooses the small hop again or the big one, one time in two each.
    /// </summary>
    internal static readonly WeightedRandomChoice<PuyoHopType> HopAfterSmallHop = new(
        HopRollParity, new(1, PuyoHopType.NormalSmall), new(1, PuyoHopType.NormalBig));

    /// <summary>
    /// The ordinary hop a Puyo that last made the big or the high one takes next (#1619): its <see cref="HopRoll"/> advanced
    /// by the enemy's frame counter chooses the small hop one time in eight, the big one one time in eight and the high
    /// one six times in eight.
    /// </summary>
    internal static readonly WeightedRandomChoice<PuyoHopType> HopAfterLargerHop = new(
        HopRoll, new(1, PuyoHopType.NormalSmall), new(1, PuyoHopType.NormalBig), new(6, PuyoHopType.NormalHigh));

    /// <summary>
    /// The chance a dropped Puyo's bounce is <see cref="PuyoHopType.DroppedBig"/> rather than
    /// <see cref="PuyoHopType.DroppedSmall"/> (#1619): its hop roll, advanced by the enemy's frame counter, odd, one time
    /// in two.
    /// </summary>
    internal static readonly RandomChance BigDroppedBounce = RandomChance.Highest(1, HopRollParity);
}
