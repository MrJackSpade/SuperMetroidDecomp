
namespace SuperMetroid.Core.Spatial;

/// <summary>Verification access to <see cref="SignedEightEight"/> members production does not use.</summary>
internal static class SignedEightEightAccess
{
    /// <summary>Constructs checked whole-pixel values in the signed 8.8 format.</summary>
    extension(SignedEightEight)
    {
        /// <summary>Creates an exact whole-pixel velocity and rejects values outside 8.8 range.</summary>
        internal static SignedEightEight FromWholePixels(int pixels) =>
            new(unchecked((ushort)EightEight.FromWhole(checked((sbyte)pixels))));
    }

    /// <summary>Inspects or advances a signed 8.8 value with native wrap semantics.</summary>
    extension(SignedEightEight self)
    {
        /// <summary>Signed whole-pixel byte.</summary>
        internal sbyte WholePixels => unchecked((sbyte)(EightEight.WholeOf(self.RawValue)));

        /// <summary>Unsigned 1/256-pixel fraction byte.</summary>
        internal byte Fraction => unchecked((byte)self.RawValue);

        /// <summary>Adds raw 1/256-pixel units with native 16-bit wraparound.</summary>
        internal SignedEightEight AddRawWrapping(int rawDelta) =>
            new(unchecked((ushort)(self.RawValue + rawDelta)));
    }
}
