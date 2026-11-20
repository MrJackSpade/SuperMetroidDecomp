namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Where the Baby Metroid that comes to Samus's rescue in Mother Brain's room starts its entrance (#1946): at its arrival
/// point's whole pixels, with the fractions of a pixel the enemy last in the place it takes left there. Its entrance moves
/// it by speeds in 256ths of a pixel and keeps every carry, so the fractions it starts from decide on which update it
/// reaches each next whole pixel, and with them the update on which it reaches Mother Brain's brain and latches on.
/// </summary>
/// <remarks>
/// Consulted once, when the Baby is set up in the first free enemy place on the update Mother Brain's final beam calls it.
/// It owns no state of its own: the fractions are what the place's previous occupant left, which an enemy deleted without
/// being killed leaves in its place, as the Enemies module's removed-enemy leftovers describe for its species state, so it is
/// a rule over what the place holds rather than a store, as <see cref="BabyMetroidVictim"/> is for the Tourian Baby.
/// Evidence: the enemy spawn routine and the cutscene Baby's setup write the arrival point's whole X and Y only, never
/// the fractions, and a kill is the only thing that clears a place's fractions; the 100% movie loses the Baby's vertical
/// fraction and with it the frame it reaches the brain on when the fractions are cleared (#1269), and the Baby's
/// inherited-fractions check reproduces the start against the retail Mother Brain room.
/// </remarks>
[CompatibilityService("the Baby Metroid that rescues Samus starts its entrance from the fractions of a pixel the enemy last in its place left, which decide the update on which it reaches Mother Brain's brain")]
public static class BabyMetroidArrivalFractions
{
    /// <summary>
    /// The position the Baby starts its entrance from: <paramref name="arrival"/>'s whole pixels with
    /// <paramref name="leftInPlace"/>'s fractions of a pixel.
    /// </summary>
    /// <param name="arrival">The Baby's arrival point, in whole room pixels.</param>
    /// <param name="leftInPlace">
    /// The position the place the Baby takes still holds from its previous occupant; only its fractions are kept.
    /// </param>
    public static FixedPosition<RoomPixels> StartingPosition(
        Position16<RoomPixels> arrival,
        FixedPosition<RoomPixels> leftInPlace) =>
        FixedPosition<RoomPixels>.FromWhole(arrival).WithFractionsOf(leftInPlace);
}
