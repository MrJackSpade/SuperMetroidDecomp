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
}
