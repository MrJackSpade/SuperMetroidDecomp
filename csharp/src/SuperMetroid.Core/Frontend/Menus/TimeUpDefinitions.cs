namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>How an expired escape countdown whites out the screen.</summary>
internal static class TimeUpDefinitions
{
    /// <summary>
    /// The shared fade counter's last value at which a step of the whiteout moves the screen toward white: the step after it
    /// completes the fade. The screen whitens in this many parts, one for each of the fade's own steps after its first (#2421).
    /// </summary>
    public const int WhiteOutLastStep = 9;
}
