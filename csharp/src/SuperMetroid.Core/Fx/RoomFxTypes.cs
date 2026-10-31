namespace SuperMetroid.Core.Fx;

/// <summary>Rules shared by room effects of several types (#1415: the record's type byte is decoded at import).</summary>
public static class RoomFxTypes
{
    /// <summary>
    /// Water and the statue effect select the same water page; the statue effect installs ordinary BG3
    /// water motion for the statue effect. Samus dispatches both through the low
    /// nibble, while statue BG2 motion remains independently owned.
    /// </summary>
    public static bool UsesWater(RoomFxType type) =>
        type is RoomFxType.Water or RoomFxType.TourianEntranceStatue;
}
