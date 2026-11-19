namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>How an expired escape countdown whites out the screen.</summary>
internal static class TimeUpDefinitions
{
    /// <summary>
    /// The parts the whiteout moves the screen toward white in, one for each of its own steps after its first; the step
    /// after the last part completes it (#2421).
    /// </summary>
    public const int WhiteOutParts = 9;
}
