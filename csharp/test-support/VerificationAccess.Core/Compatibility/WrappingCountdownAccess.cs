namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Verification access to the stored count of a <see cref="WrappingCountdown"/>, which gameplay reads only through named
/// tests: checks compare it with the original game's timer words, and snapshot codecs carry it in and out.
/// </summary>
internal static class WrappingCountdownAccess
{
    /// <summary>Exposes the countdown's stored count as the original game's timer word.</summary>
    extension(WrappingCountdown self)
    {
        /// <summary>The stored count as the original's timer word, 0 through 65535, a wrapped count kept exactly.</summary>
        internal ushort RawValue => checked((ushort)self.RemainingFrames);
    }
}
