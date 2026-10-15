namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Exact fixed-point additions shared by translated opening-cinematic actors.</summary>
internal static class IntroCinematicMotion
{
    /// <summary>One axis as the native 32-bit whole:fraction value (whole word in the high half).</summary>
    public static uint Packed(SnesFixedPosition position) => new SnesFixedPosition(position.Whole, position.Fraction).RawValue;

    /// <summary>One axis from a native 32-bit whole:fraction value.</summary>
    public static SnesFixedPosition Unpacked(uint value) => new(SnesFixedPosition.WholeOf(unchecked((int)value)), unchecked((ushort)value));

    /// <summary>Adds a native whole/fraction velocity pair to one axis.</summary>
    public static SnesFixedPosition AddSixteenSixteen(
        SnesFixedPosition position,
        ushort wholeVelocity,
        ushort fractionalVelocity) =>
        // Native 16-bit ADC first adds the fractional words, then propagates its carry into
        // the signed whole-word sum. Packing them produces precisely that wrap/carry model.
        SnesSignedSixteenSixteen
            .FromParts(unchecked((short)wholeVelocity), fractionalVelocity)
            .AddTo(position.Whole, position.Fraction);

    /// <summary>Adds a packed signed 16.16 delta (whole word in the high half) to one axis.</summary>
    public static SnesFixedPosition AddSignedSixteenSixteen(SnesFixedPosition position, int fixedDelta) =>
        AddSixteenSixteen(position, SnesFixedPosition.WholeOf(fixedDelta), unchecked((ushort)fixedDelta));

    /// <summary>Adds a signed 8.8 velocity to one axis.</summary>
    public static SnesFixedPosition AddEightEight(
        SnesFixedPosition position,
        SnesSignedEightEight velocity) =>
        // The 65c816 XBA sequence places the velocity fraction in subposition byte +1.
        // Its eight-bit carry then feeds the sign-extended integer byte.
        velocity
            .ToSixteenSixteenDelta()
            .AddTo(position.Whole, position.Fraction);
}
