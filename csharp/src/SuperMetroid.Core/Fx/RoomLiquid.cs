namespace SuperMetroid.Core.Fx;

/// <summary>
/// The liquid a room effect fills its room with (#1695): <see cref="RoomWater"/> for water and the Tourian entrance
/// statue's water, <see cref="RoomLavaOrAcid"/> for lava and acid. Every liquid's surface follows its <see cref="Tide"/>.
/// </summary>
/// <param name="Tide">How the surface rises and falls about its base line.</param>
public abstract record RoomLiquid(LiquidTide Tide)
{
    /// <summary>
    /// <paramref name="liquid"/>, which an effect of <paramref name="type"/> fills its room with: water for water and the
    /// statue effect, lava or acid for those, and nothing for every other effect.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="liquid"/> is not the kind of liquid an effect of <paramref name="type"/> has.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="type"/> names no room effect.</exception>
    internal static RoomLiquid? OfEffect(RoomFxType type, RoomLiquid? liquid)
    {
        bool fits = type switch
        {
            RoomFxType.Water or RoomFxType.TourianEntranceStatue => liquid is RoomWater,
            RoomFxType.Lava or RoomFxType.Acid => liquid is RoomLavaOrAcid,
            RoomFxType.None or RoomFxType.Rain or RoomFxType.Fog or RoomFxType.Spores or RoomFxType.ScrollingSky or
                RoomFxType.UnusedScrollingSky or RoomFxType.Fireflea or RoomFxType.CeresRidley or RoomFxType.CeresElevator or
                RoomFxType.CeresHaze => liquid is null,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown room effect."),
        };
        return fits
            ? liquid
            : throw new ArgumentException($"A {type} effect does not fill its room with {liquid?.GetType().Name ?? "no liquid"}.", nameof(liquid));
    }
}
