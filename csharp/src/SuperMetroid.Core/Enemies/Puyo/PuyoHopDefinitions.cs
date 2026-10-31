namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>Seven immutable NTSC Puyo hop definitions, independent of animation artwork.</summary>
public static class PuyoHopDefinitions
{
    /// <summary>Native table stride for the serialized byte-index selector.</summary>
    public const int RecordSize = 8;

    /// <summary>The seven hops, by native byte index.</summary>
    internal static readonly Dictionary<int, PuyoHopDefinition> Hops = new()
    {
        [0] = new(16, 256, 512, PuyoAirborneFunction.NormalShortHop),
        [8] = new(32, 256, 512, PuyoAirborneFunction.NormalBigHop),
        [16] = new(32, 512, 768, PuyoAirborneFunction.NormalLongHop),
        [24] = new(128, 320, 512, PuyoAirborneFunction.GiantHop),
        [32] = new(0, 0, 256, PuyoAirborneFunction.Dropping),
        [40] = new(16, 256, 448, PuyoAirborneFunction.Dropped),
        [48] = new(21, 256, 448, PuyoAirborneFunction.Dropped),
    };
}
