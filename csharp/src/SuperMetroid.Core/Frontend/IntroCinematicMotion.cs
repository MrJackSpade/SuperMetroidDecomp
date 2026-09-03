using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>Exact fixed-point additions shared by translated opening-cinematic actors.</summary>
internal static class IntroCinematicMotion
{
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
