namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How Speed Booster's dash raises a jump (#2116): the dash's extra run speed adds half its whole pixels to the jump's
/// whole speed and its whole fraction to the jump's fraction, each on its own. A fraction sum that passes a whole pixel
/// keeps only its remainder and never raises the whole speed, and an odd whole pixel of the extra speed is dropped rather
/// than halved into the fraction, so a boosted jump can start up to a pixel an update slower than the two speeds' true
/// sum and half would give, which changes how high it rises.
/// </summary>
/// <remarks>
/// Consulted once per jump or wall jump Samus starts with Speed Booster equipped, after the jump's launch speed is chosen
/// and before its gravity is set. A rule with no state of its own. Evidence: the original jump setup
/// (<c>Make_Samus_Jump</c>, <c>$90:98BC</c>) adds the extra run speed's fraction word to the vertical fraction word with
/// no carry into the whole word, and the whole word shifted right once to the vertical whole word; the horizontal-speed
/// check (<c>VerifySamusHorizontalSpeed</c>) starts a boosted jump whose fraction sum passes a whole pixel from an odd
/// whole extra speed.
/// </remarks>
[CompatibilityService("Speed Booster's jump bonus adds half the dash's whole extra speed and its whole fraction to the jump's speed each on its own, a fraction past a pixel losing its carry")]
internal static class SpeedBoosterJumpBonus
{
    /// <summary>
    /// <paramref name="jumpSpeed"/> raised by Speed Booster's dash: half <paramref name="extraRunSpeed"/>'s whole pixels,
    /// the odd pixel dropped, added to its whole speed, and <paramref name="extraRunSpeed"/>'s fraction added to its
    /// fraction, the sum's carry past a whole pixel dropped.
    /// </summary>
    /// <param name="jumpSpeed">The jump's launch speed upward.</param>
    /// <param name="extraRunSpeed">The dash's extra run speed.</param>
    internal static UnsignedSixteenSixteen Boosted(UnsignedSixteenSixteen jumpSpeed, UnsignedSixteenSixteen extraRunSpeed) =>
        jumpSpeed.AddedPartwise(extraRunSpeed.WithWholePixelsHalved());
}
