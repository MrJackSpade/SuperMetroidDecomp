using static SuperMetroid.Core.Frontend.Intro.IntroRinkaDefinitions;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Looks up each intro Rinka's physical definition (<see cref="IntroRinkaDefinitions"/>) by parameter.
/// </summary>
internal static class IntroRinkaLaunches
{
    /// <summary>
    /// Returns one complete parameter-selected physical definition. <c>$8B:B896</c>
    /// subtracts eight from the authored Y position; both movement paths use fraction
    /// <c>$8000</c>, so the signed whole component distinguishes +0.5 from -0.5 px/frame.
    /// </summary>
    /// <remarks>
    /// Issues #625 and #986: $8B:B896 indexes four initial X words at
    /// $8B:B8B5+2*p for parameter p=0..3. Pinned NTSC J/U v1.0 ROM and
    /// bank_8B.asm both give $0070, $00C0, $0080, $00E8. The spawn
    /// instructions create p=0/1, then p=2/3, and the selector rejects
    /// invalid parameters. The two pairs have different offsets and spans;
    /// these are authored scene placements, so retain four explicit X
    /// values instead of encoding them as a fitted sequence. The production
    /// verifier runs both waves with physical-table reads forbidden.
    ///
    /// Issues #625 and #987: the separate Y source at $8B:B8BD+2*p has
    /// four words $0050, $0040, $0038, $0058 in pinned NTSC J/U v1.0 ROM
    /// and bank_8B.asm. Initializer $8B:B896 subtracts eight using a 16-bit
    /// SBC before storing Y, yielding $0048, $0038, $0030, $0050.
    /// Both spawn waves use the same bounded p=0..3 selector. The source
    /// heights and final spacing are irregular scene placement, so retain
    /// four explicit final Y values instead of a fitted progression.
    ///
    /// Issues #625 and #988: four signed whole-word X components at
    /// $8B:B985+2*p are $0000, $FFFF, $0000, $FFFF in pinned NTSC J/U v1.0
    /// ROM and bank_8B.asm. For bounded p=0..3 this is exactly -(p&amp;1).
    /// The miss routine indexes the table for p=1..3; the p=0 hit routine
    /// uses the same zero as an immediate. Both first add fractional
    /// velocity $8000, then carry into the signed whole-word addition,
    /// yielding +0.5 px/call for even p and -0.5 for odd p. The shared
    /// selector represents both native paths without an out-of-range read.
    /// </remarks>
    public static IntroRinkaPhysicalDefinition Rinka(int parameter) => Ensure.Element(Rinkas, parameter);
}
