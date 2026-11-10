namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The ending's screen (#1358, #1467, #1515): its colours. Which pictures the screen shows follows from the ending's phase
/// and its scenes' state each frame; nothing records which were loaded.
/// </summary>
internal sealed class EndingScreen
{
    /// <summary>What the ending's colours show now: the running scene's loads, fades, flashes and palette effects (#1512, #1597).</summary>
    internal EndingColours Colours { get; } = new();
}
