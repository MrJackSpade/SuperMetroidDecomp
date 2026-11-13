namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Where Ceres Ridley aims while he breathes fireballs (#1620, was <c>CeresRidleyFlight.JitteredFireballTarget</c>): a
/// jitter of up to seven pixels moves the point he hovers toward diagonally from where the attack began, both coordinates
/// by the jitter, except that a jitter toward the left and up lifts the point one pixel less than it moves it left. So
/// his hover drifts down and right on average while he breathes, and a jitter of one pixel left does not lift him at all.
/// </summary>
/// <remarks>
/// Consulted on every update of the fireball attack, after the jitter is drawn from the random value and before Ridley
/// accelerates toward the point. Evidence: the fireball attack's shooting step (<c>$A6:A7F9</c>) adds the jitter, a negative jitter as
/// its sixteen-bit complement, to the base X and then to the base Y without clearing the carry between them, and enters
/// with the carry clear (the enemy main loop's index addition before the call, or the hover's height comparison it falls
/// through from); so the Y addition takes one more pixel exactly when the X addition passes the end of the word, which a
/// negative jitter does from every X at or past its size and a positive one from none below the word's last pixels. The
/// arena's clamps keep Ridley's X between 40 and 224, so the attack always starts where neither edge case can happen;
/// a base that could reach them is refused. The fireball target check compares every base X and jitter with that chained
/// addition.
/// </remarks>
[CompatibilityService("a leftward jitter of Ceres Ridley's fireball hover point lifts it one pixel less than it moves it left")]
internal static class CeresRidleyFireballTarget
{
    /// <summary>The largest jitter either way, in pixels.</summary>
    internal const int LargestJitter = 7;

    /// <summary>
    /// The point <paramref name="jitter"/> selects around <paramref name="basePosition"/>: moved by the jitter on both
    /// axes, and one pixel down again when the jitter points left.
    /// </summary>
    /// <param name="basePosition">Where the attack began, its X at least <see cref="LargestJitter"/> pixels from either end of the coordinate space.</param>
    /// <param name="jitter">The jitter, -7 through 7 pixels.</param>
    /// <exception cref="ArgumentOutOfRangeException">The jitter or the base's X lies outside its range.</exception>
    internal static Position16<RoomPixels> Around(Position16<RoomPixels> basePosition, int jitter)
    {
        _ = Ensure.BetweenInclusive(LargestJitter, ushort.MaxValue - LargestJitter, basePosition.X);
        int leftwardLift = Ensure.BetweenInclusive(-LargestJitter, LargestJitter, jitter) < 0 ? 1 : 0;
        return basePosition + new Offset16<RoomPixels>(jitter, jitter + leftwardLift);
    }
}
