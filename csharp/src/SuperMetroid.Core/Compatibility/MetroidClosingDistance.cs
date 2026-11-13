namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How far a Metroid closing on Samus reckons it is from its target on each axis (#1620): only the two coordinates'
/// places within a 256-pixel span count, so the distance reads -128 through 127 pixels, and a target 128 pixels or more
/// away one way reads as lying the other way. A Metroid that Samus has outrun by that far while it closes on her
/// therefore flies away from her until the span brings her back within reach.
/// </summary>
/// <remarks>
/// Consulted on every update of a Metroid closing on Samus, for each axis, before its fixed closing speed is set from
/// the distance's sign. Evidence: the closing routine subtracts the coordinates' low bytes and sign-extends the eight-bit
/// difference before shifting and clamping it; a Metroid closes from the touch that starts it at three pixels an update
/// whatever Samus does, so a Speed Booster run or a fall leaves it behind by more than 127 pixels.
/// </remarks>
[CompatibilityService("a Metroid closing on Samus reckons its distance to her within a 256-pixel span, so a target 128 pixels or more away reads as lying the other way")]
public static class MetroidClosingDistance
{
    /// <summary>The span of coordinates the distance tells apart.</summary>
    private const int Span = 256;

    /// <summary>
    /// The distance from <paramref name="current"/> to <paramref name="target"/>, -128 through 127 pixels: their difference's
    /// place within the 256-pixel span, read with its upper half as negative.
    /// </summary>
    /// <param name="target">The coordinate the Metroid closes on, in room pixels.</param>
    /// <param name="current">The Metroid's coordinate, in room pixels.</param>
    public static int Between(int target, int current)
    {
        int place = ((target - current) % Span + Span) % Span;
        return place >= Span / 2 ? place - Span : place;
    }
}
