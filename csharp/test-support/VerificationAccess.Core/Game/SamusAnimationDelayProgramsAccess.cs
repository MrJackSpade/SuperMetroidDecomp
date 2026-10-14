using SuperMetroid.Core.Game;

/// <summary>Verification access to <see cref="SamusAnimationDelayPrograms"/> members production does not use.</summary>
internal static class SamusAnimationDelayProgramsAccess
{
    /// <summary>Reads compiled pose-animation delay segments in native program order.</summary>
    extension(SamusAnimationDelayPrograms)
    {
        /// <summary>Compiled pose-animation delay segments in their native order.</summary>
        internal static ReadOnlySpan<SamusAnimationSegment> Segments => SamusAnimationDelayPrograms.segments;
    }
}
