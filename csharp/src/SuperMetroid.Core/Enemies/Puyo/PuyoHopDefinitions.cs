namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>Seven immutable NTSC Puyo hop definitions, independent of animation artwork.</summary>
public static class PuyoHopDefinitions
{
    /// <summary>Each hop's height, horizontal speed, vertical-curve step and airborne behaviour.</summary>
    internal static readonly Dictionary<PuyoHopType, PuyoHopDefinition> Hops = new()
    {
        [PuyoHopType.NormalSmall] = new(16, 256, 512, PuyoAirborneFunction.NormalShortHop),
        [PuyoHopType.NormalBig] = new(32, 256, 512, PuyoAirborneFunction.NormalBigHop),
        [PuyoHopType.NormalHigh] = new(32, 512, 768, PuyoAirborneFunction.NormalLongHop),
        [PuyoHopType.Giant] = new(128, 320, 512, PuyoAirborneFunction.GiantHop),
        [PuyoHopType.Dropping] = new(0, 0, 256, PuyoAirborneFunction.Dropping),
        [PuyoHopType.DroppedSmall] = new(16, 256, 448, PuyoAirborneFunction.Dropped),
        [PuyoHopType.DroppedBig] = new(21, 256, 448, PuyoAirborneFunction.Dropped),
    };

    /// <summary>
    /// The random field a Puyo's hop roll starts from: bits zero to two. The roll adds the enemy's frame counter and
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
