using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>Exact fixed-point additions shared by translated opening-cinematic actors.</summary>
internal static class IntroCinematicMotion
{
    /// <summary>Adds a signed 16.16 fixed-point delta with native word carry and wrap semantics.</summary>
    /// <param name="wholePosition">Whole-position word to update in place.</param>
    /// <param name="subPosition">Fractional-position word paired with the whole position.</param>
    /// <param name="wholeVelocity">Signed integer word of the 16.16 delta.</param>
    /// <param name="fractionalVelocity">Fractional word of the 16.16 delta.</param>
    public static void AddSixteenSixteen(
        ref ushort wholePosition,
        ref ushort subPosition,
        ushort wholeVelocity,
        ushort fractionalVelocity)
    {
        // Native 16-bit ADC first adds the fractional words, then propagates its carry into
        // the signed whole-word sum. Packing them produces precisely that wrap/carry model.
        SnesFixedPosition result = SnesSignedSixteenSixteen
            .FromParts(unchecked((short)wholeVelocity), fractionalVelocity)
            .AddTo(wholePosition, subPosition);
        wholePosition = result.Whole;
        subPosition = result.Fraction;
    }

    /// <summary>Adds a signed 8.8 delta to a 16.16 position using the native byte-carry behavior.</summary>
    /// <param name="wholePosition">Whole-position word to update in place.</param>
    /// <param name="subPosition">Fractional-position word paired with the whole position.</param>
    /// <param name="velocity">Signed 8.8 fixed-point delta.</param>
    public static void AddEightEight(
        ref ushort wholePosition,
        ref ushort subPosition,
        ushort velocity)
    {
        // The 65c816 XBA sequence places the velocity fraction in subposition byte +1.
        // Its eight-bit carry then feeds the sign-extended integer byte.
        SnesFixedPosition result = new SnesSignedEightEight(velocity)
            .ToSixteenSixteenDelta()
            .AddTo(wholePosition, subPosition);
        wholePosition = result.Whole;
        subPosition = result.Fraction;
    }
}
