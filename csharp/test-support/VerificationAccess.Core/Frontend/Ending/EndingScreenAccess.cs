namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Verification access to <see cref="EndingScreen"/> members production does not use.</summary>
internal static class EndingScreenAccess
{
    /// <summary>Reads the ending screen's recorded transfers.</summary>
    extension(EndingScreen self)
    {
        /// <summary>The transfers since the last full clear, oldest first.</summary>
        internal IReadOnlyList<EndingUpload> Uploads => self.uploads;
    }
}
