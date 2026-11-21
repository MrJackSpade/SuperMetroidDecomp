namespace SuperMetroid.Core.Fx;

/// <summary>Rules shared by room effects of several types (#1415: the effect's kind is decoded at import).</summary>
public static class RoomFxTypes
{
    /// <summary>
    /// Water and the statue effect show the same water; the statue effect starts the ordinary effect overlay
    /// water motion. Samus treats both as water, while the statue's background motion remains independently owned.
    /// </summary>
    public static bool UsesWater(RoomFxType type) => Of(type).Overlay == RoomFxOverlayMotion.Water;

    /// <summary>
    /// Whether a room of effect <paramref name="type"/> fills with a liquid the effect overlay shows below its surface:
    /// lava, acid, water and the Tourian entrance statue's water (#1706). The other effects that show artwork are atmospheres.
    /// </summary>
    public static bool ShowsLiquid(RoomFxType type) => IsLavaOrAcid(type) || UsesWater(type);

    /// <summary>Whether a room of effect <paramref name="type"/> fills with lava or acid.</summary>
    internal static bool IsLavaOrAcid(RoomFxType type) => Of(type).Overlay == RoomFxOverlayMotion.LavaOrAcid;

    /// <summary>The definition of <paramref name="type"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="type"/> is undefined.</exception>
    internal static RoomFxTypeDefinition Of(RoomFxType type) => RoomFxTypeDefinitions.Types[type];
}
