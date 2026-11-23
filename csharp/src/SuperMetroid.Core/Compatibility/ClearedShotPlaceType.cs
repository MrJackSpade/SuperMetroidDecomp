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
public static partial class ClearedShotPlaceType
{
    /// <summary>
    /// The type a shot strikes with: the type of the shot <paramref name="holder"/> while one holds the place, otherwise
    /// <paramref name="unchargedPowerBeam"/>, which its cleared place reads as.
    /// </summary>
    /// <typeparam name="TShot">The shot holding a place.</typeparam>
    /// <typeparam name="TType">A projectile type.</typeparam>
    /// <param name="holder">The shot holding the place, or null once it ended.</param>
    /// <param name="typeOf">The type of the shot holding the place.</param>
    /// <param name="unchargedPowerBeam">The uncharged Power beam's type.</param>
    public static TType Striking<TShot, TType>(TShot? holder, Func<TShot, TType> typeOf, TType unchargedPowerBeam)
        where TShot : class =>
        holder is null ? unchargedPowerBeam : Ensure.NotNull(typeOf)(holder);
}
