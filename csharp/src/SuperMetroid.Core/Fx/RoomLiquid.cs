namespace SuperMetroid.Core.Fx;

/// <summary>
/// The liquid a room effect fills its room with (#1695): <see cref="RoomWater"/> for water and the Tourian entrance
/// statue's water, <see cref="RoomLavaOrAcid"/> for lava and acid. Every liquid's surface follows its <see cref="Tide"/>.
/// </summary>
/// <param name="Tide">How the surface rises and falls about its base line.</param>
public abstract record RoomLiquid(LiquidTide Tide)
{
    /// <summary>Whether a room's liquid fits each room effect: water for the water effects, lava or acid for theirs, and none for the rest.</summary>
    private static IdentityTable<RoomFxType, Func<RoomLiquid?, bool>> Fits => field ??= new(new Dictionary<RoomFxType, Func<RoomLiquid?, bool>>
    {
        [RoomFxType.Water] = static liquid => liquid is RoomWater,
        [RoomFxType.TourianEntranceStatue] = static liquid => liquid is RoomWater,
        [RoomFxType.Lava] = static liquid => liquid is RoomLavaOrAcid,
        [RoomFxType.Acid] = static liquid => liquid is RoomLavaOrAcid,
        [RoomFxType.None] = static liquid => liquid is null,
        [RoomFxType.Rain] = static liquid => liquid is null,
        [RoomFxType.Fog] = static liquid => liquid is null,
        [RoomFxType.Spores] = static liquid => liquid is null,
        [RoomFxType.ScrollingSky] = static liquid => liquid is null,
        [RoomFxType.Fireflea] = static liquid => liquid is null,
        [RoomFxType.CeresRidley] = static liquid => liquid is null,
        [RoomFxType.CeresElevator] = static liquid => liquid is null,
        [RoomFxType.CeresHaze] = static liquid => liquid is null,
    });

    /// <summary>
    /// <paramref name="liquid"/>, which an effect of <paramref name="type"/> fills its room with: water for water and the
    /// statue effect, lava or acid for those, and nothing for every other effect.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="liquid"/> is not the kind of liquid an effect of <paramref name="type"/> has.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="type"/> names no room effect.</exception>
    internal static RoomLiquid? OfEffect(RoomFxType type, RoomLiquid? liquid)
    {
        bool fits = Fits[type](liquid);
        return fits
            ? liquid
            : throw new ArgumentException($"A {type} effect does not fill its room with {liquid?.GetType().Name ?? "no liquid"}.", nameof(liquid));
    }
}
