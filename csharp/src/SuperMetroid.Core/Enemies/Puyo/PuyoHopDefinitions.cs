namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>The Puyo's hops (<see cref="PuyoHopType"/>).</summary>
internal static class PuyoHopDefinitions
{
    /// <summary>The ordinary hops, whose next hop proximity and chance choose: the small, big and long ones.</summary>
    internal static readonly PuyoHopType[] NormalHops = [PuyoHopType.NormalSmall, PuyoHopType.NormalBig, PuyoHopType.NormalLong];
}
