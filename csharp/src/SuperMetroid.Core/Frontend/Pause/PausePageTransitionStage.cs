namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>Where a pause-page change stands (#2560): fading the old page out, loading the new one at black, or fading it in.</summary>
internal enum PausePageTransitionStage
{
    /// <summary>The page being left dims one level every update.</summary>
    FadeOut,
    /// <summary>The screen is black and the new page loads in place of the old.</summary>
    Load,
    /// <summary>The newly loaded page brightens.</summary>
    FadeIn,
}
