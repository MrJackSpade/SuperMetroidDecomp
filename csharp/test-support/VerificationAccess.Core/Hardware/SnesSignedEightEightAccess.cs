using SuperMetroid.Core.Hardware;

/// <summary>Verification access to <see cref="SnesSignedEightEight"/> members production does not use.</summary>
internal static class SnesSignedEightEightAccess
{
    /// <summary>Constructs checked whole-pixel values in the signed 8.8 format.</summary>
    extension(SnesSignedEightEight)
    {
        /// <summary>Creates an exact whole-pixel velocity and rejects values outside 8.8 range.</summary>
        internal static SnesSignedEightEight FromWholePixels(int pixels) =>
            new(unchecked((ushort)SnesEightEight.FromWhole(checked((sbyte)pixels))));
    }

    /// <summary>Inspects or advances a signed 8.8 value with native wrap semantics.</summary>
    extension(SnesSignedEightEight self)
    {
        /// <summary>Signed whole-pixel byte.</summary>
        internal sbyte WholePixels => unchecked((sbyte)(SnesEightEight.WholeOf(self.RawValue)));

        /// <summary>Unsigned 1/256-pixel fraction byte.</summary>
        internal byte Fraction => unchecked((byte)self.RawValue);

        /// <summary>Adds raw 1/256-pixel units with native 16-bit wraparound.</summary>
        internal SnesSignedEightEight AddRawWrapping(int rawDelta) =>
            new(unchecked((ushort)(self.RawValue + rawDelta)));
    }
}
