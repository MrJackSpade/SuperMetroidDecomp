namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>How near Samus must come for the Morph Ball eye to open and keep its beam open, and how long it opens and closes (#2156).</summary>
internal static class MorphBallEyeWatchDefinitions
{
    /// <summary>How close horizontally Samus comes, in pixels and exclusive, for the eye to start opening.</summary>
    internal const int OpeningReachX = 128;

    /// <summary>How close horizontally Samus stays, in pixels and exclusive, for an active eye to keep its beam open.</summary>
    internal const int StayOpenReachX = 176;

    /// <summary>How close vertically Samus comes, in pixels and exclusive, for the eye to start opening.</summary>
    internal const int OpeningReachY = 128;

    /// <summary>How close vertically Samus stays, in pixels and exclusive, for an eye tracking her to stay open.</summary>
    internal const int StayOpenReachY = 128;

    /// <summary>How long the eye takes to open or to close, in updates.</summary>
    internal const int TransitionDuration = 32;
}
