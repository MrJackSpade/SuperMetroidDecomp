namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What a shot cleared during its own update strikes as (#2558): the blocks, doors and Power Bomb boundary its update goes
/// on to reach after it ended still read its place, which the game cleared, as an uncharged Power beam.
/// </summary>
/// <remarks>
/// Consulted where a shot's block span or a Chainsaw fuse's Power Bomb boundary strikes blocks after the shot may have
/// ended in the same update. Evidence: the block and door reactions read the place's projectile type word without
/// testing whether the place still holds a projectile, and a cleared place's word is zero, an uncharged Power beam's.
/// </remarks>
[CompatibilityService("a shot cleared during its own update strikes the blocks its update goes on to reach as an uncharged Power beam")]
public static class ClearedShotPlaceType
{
    /// <summary>
    /// The type a shot strikes with: <paramref name="own"/>'s while it holds its place, otherwise
    /// <paramref name="unchargedPowerBeam"/>, which its cleared place reads as.
    /// </summary>
    /// <typeparam name="TType">A projectile type.</typeparam>
    /// <param name="holdsPlace">Whether the shot still holds its place.</param>
    /// <param name="own">The shot's own type, read only while it holds its place.</param>
    /// <param name="unchargedPowerBeam">The uncharged Power beam's type.</param>
    public static TType Striking<TType>(bool holdsPlace, Func<TType> own, TType unchargedPowerBeam) =>
        holdsPlace ? Ensure.NotNull(own)() : unchargedPowerBeam;
}
