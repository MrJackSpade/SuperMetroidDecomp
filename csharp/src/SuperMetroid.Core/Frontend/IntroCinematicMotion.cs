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
        int velocity = unchecked((short)wholeVelocity) * 0x10000 + fractionalVelocity;
        uint packed = unchecked((uint)(((uint)wholePosition << 16) | subPosition) + (uint)velocity);
        wholePosition = unchecked((ushort)(packed >> 16));
        subPosition = unchecked((ushort)packed);
    }

    public static void AddEightEight(
        ref ushort wholePosition,
        ref ushort subPosition,
        ushort velocity)
    {
        // The 65c816 XBA sequence places the velocity fraction in subposition byte +1.
        // Its eight-bit carry then feeds the sign-extended integer byte.
        int fractionalSum = (subPosition >> 8) + (velocity & 0x00ff);
        subPosition = unchecked((ushort)(
            ((byte)fractionalSum << 8) | (subPosition & 0x00ff)));
        int wholeDelta = unchecked((sbyte)(velocity >> 8)) + (fractionalSum > 0xff ? 1 : 0);
        wholePosition = unchecked((ushort)(wholePosition + wholeDelta));
    }
}
