namespace SuperMetroid.Core.Spatial;

/// <summary>Verification access to <see cref="SignedSixteenSixteen"/> members production does not use.</summary>
internal static class SignedSixteenSixteenAccess
{
    /// <summary>Constructs checked whole-pixel values in the signed 16.16 format.</summary>
    extension(SignedSixteenSixteen)
    {
        /// <summary>Creates an exact whole-pixel delta with checked signed-word conversion.</summary>
        internal static SignedSixteenSixteen FromWholePixels(int pixels) =>
            SignedSixteenSixteen.FromParts(checked((short)pixels), 0);
    }
}
