namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>What a post-credits text's typing tells its typewriter to do as it plays (#2488).</summary>
internal enum EndingTypewriterCue
{
    /// <summary>The item-percentage text is typed: it shows the percentage, and with Japanese text the subtitle, for its hold.</summary>
    ShowPercentage,
    /// <summary>The text, and the percentage's hold, are done.</summary>
    Complete,
}
