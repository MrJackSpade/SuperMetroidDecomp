namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How far a Bull steps up or left in one update (#1607): its forward step's distance reversed, and, when that distance
/// is whole pixels with no fraction, one 65536th of a pixel further. A Bull charging up or left at a speed and angle whose
/// step has no fraction therefore drifts one 65536th of a pixel further each update than one charging down or right, and
/// its position's fraction, and in time its whole pixels, follow the game's.
/// </summary>
/// <remarks>
/// Consulted for each axis of a Bull's move in every update it flies, after its step's distance is formed from the
/// heading's sine and its speed. Evidence: the original movement reverses a backward step with its own negation, whose
/// path for a step with no fraction leaves it one 65536th of a pixel past a true negation; the compiled Bull movement
/// check moves a Bull at every angle and every speed-word boundary class from two origins and compares each position with
/// that negation over the pinned cartridge's sine words.
/// </remarks>
[CompatibilityService("a Bull's step up or left with no fraction lies one 65536th of a pixel further than its step down or right reversed")]
public static class BullBackwardStep
{
    /// <summary>The finest distance a position tells apart: one 65536th of a pixel.</summary>
    private static readonly SignedSixteenSixteen FinestDistance = SignedSixteenSixteen.FromParts(0, 1);

    /// <summary>The step up or left for a step down or right of <paramref name="forward"/>, zero or more.</summary>
    public static SignedSixteenSixteen Of(SignedSixteenSixteen forward)
    {
        if (forward == SignedSixteenSixteen.Zero)
            return SignedSixteenSixteen.Zero;
        return forward.Fraction == 0 ? -forward - FinestDistance : -forward;
    }
}
