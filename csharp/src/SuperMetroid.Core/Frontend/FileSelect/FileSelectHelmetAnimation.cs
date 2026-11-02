namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Bounded file-select helmet frame identities and cadence.</summary>
internal static class FileSelectHelmetAnimation
{
    /// <summary>Eight distinct images; the animation compares the incremented frame with eight.</summary>
    internal const int FrameCount = 8;
    /// <summary>The animation reloads the frame timer with eight ticks.</summary>
    internal const int FrameDuration = 8;
}
