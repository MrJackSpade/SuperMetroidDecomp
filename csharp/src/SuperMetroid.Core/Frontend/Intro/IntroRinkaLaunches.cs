using static SuperMetroid.Core.Frontend.Intro.IntroRinkaDefinitions;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Looks up each intro Rinka's physical definition (<see cref="IntroRinkaDefinitions"/>) by parameter.
/// </summary>
internal static class IntroRinkaLaunches
{
    /// <summary>
    /// Returns one complete parameter-selected physical definition. The initializer
    /// subtracts eight from the authored Y position; both movement paths use fraction
    /// one half, so the signed whole component distinguishes +0.5 from -0.5 px/frame.
    /// </summary>
    /// <remarks>
    /// Issues #625 and #986: the initializer indexes four initial X values
    /// by parameter p=0..3. The authored data
    /// gives 112, 192, 128, 232. The spawn
    /// cues create p=0/1, then p=2/3, and the selector rejects
    /// invalid parameters. The two pairs have different offsets and spans;
    /// these are authored scene placements, so retain four explicit X
    /// values instead of encoding them as a fitted sequence. The production
    /// verifier runs both waves with physical-table reads forbidden.
    ///
    /// Issues #625 and #987: the separate Y source has
    /// four values 80, 64, 56, 88 in the authored data.
    /// The initializer subtracts eight, wrapping below zero,
    /// before storing Y, yielding 72, 56, 48, 80.
    /// Both spawn waves use the same bounded p=0..3 selector. The source
    /// heights and final spacing are irregular scene placement, so retain
    /// four explicit final Y values instead of a fitted progression.
    ///
    /// Issues #625 and #988: four signed whole-pixel X components
    /// are 0, -1, 0, -1 in the authored
    /// data. For bounded p=0..3 this is exactly -(p&amp;1).
    /// The miss step indexes the table for p=1..3; the p=0 hit step
    /// uses the same zero as an immediate. Both first add fractional
    /// velocity one half, then overflow into the signed whole-pixel addition,
    /// yielding +0.5 px/call for even p and -0.5 for odd p. The shared
    /// selector represents both paths without an out-of-range read.
    /// </remarks>
    public static IntroRinkaPhysicalDefinition Rinka(int parameter) => Ensure.Element(Rinkas, parameter);
}
