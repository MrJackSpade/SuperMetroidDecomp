namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The shinespark's upward acceleration, which the suit pickup's light beam overwrites with its own progress (#1468,
/// #1552). The spark's windup starts the acceleration at its initial value, every vertical or diagonal spark update adds
/// Samus's vertical acceleration to it, and the crash's finish clears it. The suit pickup leaves its stage's place in the
/// stage order as the acceleration's whole pixels and the light beam's progress as its fraction: the lines the beam has
/// grown or been wiped by while it grows, reveals the suit or shrinks, and its two edge columns, the right one scaled by
/// a screen line's width, while it is a column or the centre line. A windup the pickup suspended launches with whatever
/// the light beam left, not with its initial acceleration, so the value is gameplay-observable; how the light is drawn is
/// not.
/// </summary>
/// <remarks>
/// <para>
/// Consulted by the shinespark: started at its windup's setup, advanced on every vertical or diagonal spark update before
/// Samus's speed takes it, and cleared when the crash finishes. Overwritten by the suit pickup at its setup and after every
/// stage call. Lifetime: Samus's; a new Samus starts with no acceleration.
/// </para>
/// <para>
/// Evidence: the original game keeps the spark's vertical acceleration in the two working words the suit pickup keeps its
/// stage and its light beam's progress in, and neither owner reloads them from its own state; the suit pickup carryover
/// check confirms the exact value at every stage of a Varia transformation.
/// </para>
/// </remarks>
[CompatibilityService("a shinespark windup after a suit pickup accelerates by the light beam's last recorded progress")]
[EncodingOwner("suit pickup light beam progress the shinespark reads")]
public sealed class SuitPickupSparkCarryover
{
    /// <summary>The highest line count or edge column the light beam leaves.</summary>
    private const int LastLightBeamPlace = 255;

    /// <summary>
    /// The spark's upward acceleration in 16.16 pixels per update per update, or what the suit pickup's light beam last
    /// left in its place. The spark's additions wrap at 32 bits.
    /// </summary>
    public SignedSixteenSixteen SparkAcceleration { get; private set; }

    /// <summary>Starts a shinespark windup's acceleration.</summary>
    /// <param name="initial">The windup's initial acceleration.</param>
    public void StartSpark(SignedSixteenSixteen initial) => SparkAcceleration = initial;

    /// <summary>Adds one spark update's increment to the acceleration.</summary>
    /// <param name="increment">Samus's vertical acceleration for the update.</param>
    public void Accelerate(SignedSixteenSixteen increment) => SparkAcceleration += increment;

    /// <summary>Clears the acceleration, as a crash's finish does.</summary>
    public void StopSpark() => SparkAcceleration = SignedSixteenSixteen.Zero;

    /// <summary>
    /// The suit pickup leaves its stage and the lines its light beam has grown or been wiped by, 0 through 255.
    /// </summary>
    /// <param name="stage">The stage's place in the stage order, from zero.</param>
    /// <param name="lines">The lines grown or wiped.</param>
    public void LeaveLines(int stage, int lines) =>
        SparkAcceleration = SignedSixteenSixteen.FromParts(stage, Ensure.BetweenInclusive(0, LastLightBeamPlace, lines));

    /// <summary>The suit pickup leaves its stage and its light beam's two edge columns, each 0 through 255.</summary>
    /// <param name="stage">The stage's place in the stage order, from zero.</param>
    /// <param name="left">The beam's left edge column.</param>
    /// <param name="right">The beam's right edge column.</param>
    public void LeaveEdges(int stage, int left, int right) =>
        SparkAcceleration = SignedSixteenSixteen.FromParts(stage,
            Ensure.BetweenInclusive(0, LastLightBeamPlace, left) +
            Ensure.BetweenInclusive(0, LastLightBeamPlace, right) * ScreenGeometry.ScreenWidthPixels);
}
