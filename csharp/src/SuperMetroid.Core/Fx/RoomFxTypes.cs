namespace SuperMetroid.Core.Fx;

/// <summary>Rules shared by room effects of several types (#1415: the effect's kind is decoded at import).</summary>
public static class RoomFxTypes
{
    /// <summary>
    /// Water and the statue effect show the same water; the statue effect installs ordinary effect overlay
    /// water motion for the statue effect. Samus dispatches both through the low
    /// selection, while statue background motion remains independently owned.
    /// </summary>
    public static bool UsesWater(RoomFxType type) =>
        type is RoomFxType.Water or RoomFxType.TourianEntranceStatue;

    /// <summary>
    /// Whether a room of effect <paramref name="type"/> fills with a liquid the effect overlay shows below its surface:
    /// lava, acid, water and the Tourian entrance statue's water (#1706). The other effects that show artwork are atmospheres.
    /// </summary>
    public static bool ShowsLiquid(RoomFxType type) =>
        type is RoomFxType.Lava or RoomFxType.Acid || UsesWater(type);
}
