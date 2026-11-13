namespace SuperMetroid.Core.Fx;

/// <summary>Rules shared by room effects of several types (#1415: the record's type is decoded at import).</summary>
public static class RoomFxTypes
{
    /// <summary>
    /// Water and the statue effect select the same water page; the statue effect installs ordinary effect overlay
    /// water motion for the statue effect. Samus dispatches both through the low
    /// selection, while statue background motion remains independently owned.
    /// </summary>
    public static bool UsesWater(RoomFxType type) =>
        type is RoomFxType.Water or RoomFxType.TourianEntranceStatue;
}
