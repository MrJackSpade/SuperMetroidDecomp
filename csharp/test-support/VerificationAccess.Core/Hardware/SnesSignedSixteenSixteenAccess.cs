namespace SuperMetroid.Core.Hardware;

/// <summary>Verification access to <see cref="SnesSignedSixteenSixteen"/> members production does not use.</summary>
internal static class SnesSignedSixteenSixteenAccess
{
    /// <summary>Constructs checked whole-pixel values in the signed 16.16 format.</summary>
    extension(SnesSignedSixteenSixteen)
    {
        /// <summary>Creates an exact whole-pixel delta with checked signed-word conversion.</summary>
        internal static SnesSignedSixteenSixteen FromWholePixels(int pixels) =>
            SnesSignedSixteenSixteen.FromParts(checked((short)pixels), 0);
    }
}
