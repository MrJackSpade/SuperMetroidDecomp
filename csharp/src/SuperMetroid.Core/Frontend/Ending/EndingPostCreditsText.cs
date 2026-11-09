using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The post-credits text scene (#1555): the panel it shows, the item-percentage and final-message typed texts, and how
/// far the text has been moved down the screen. What the screen shows is derived from this state each frame, each piece at
/// its screen pixel moved down by <see cref="Drop"/>, without wrapping.
/// </summary>
internal sealed class EndingPostCreditsText
{
    /// <summary>The panel shown.</summary>
    internal EndingPostCreditsPanel Panel { get; private set; }

    /// <summary>The item-percentage text, from the logo's end on.</summary>
    internal EndingTypewriter? ItemPercentage { get; private set; }

    /// <summary>The final message, from the end of the move down on.</summary>
    internal EndingTypewriter? FinalMessage { get; private set; }

    /// <summary>Pixels the text is moved down the screen, zero until the item percentage's move down.</summary>
    internal int Drop { get; set; }

    /// <summary>The typed text the ending advances: the final message once it starts, otherwise the item percentage.</summary>
    internal EndingTypewriter? Typing => FinalMessage ?? ItemPercentage;

    /// <summary>Shows no text: no panel and no typed text, as the credits leave the screen.</summary>
    internal void Clear()
    {
        Panel = EndingPostCreditsPanel.None;
        ItemPercentage = null;
        FinalMessage = null;
    }

    /// <summary>Shows the result panel.</summary>
    /// <exception cref="InvalidOperationException">A panel is already shown.</exception>
    internal void ShowResultPanel() => Panel = Panel == EndingPostCreditsPanel.None ? EndingPostCreditsPanel.Result
        : throw new InvalidOperationException($"The post-credits text already shows the {Panel} panel.");

    /// <summary>Replaces the result panel with the copyright.</summary>
    /// <exception cref="InvalidOperationException">The result panel is not shown.</exception>
    internal void ReplaceResultWithCopyright() => Panel = Panel == EndingPostCreditsPanel.Result ? EndingPostCreditsPanel.Copyright
        : throw new InvalidOperationException("The post-credits text shows no result panel.");

    /// <summary>Clears the copyright.</summary>
    /// <exception cref="InvalidOperationException">The copyright is not shown.</exception>
    internal void ClearCopyright() => Panel = Panel == EndingPostCreditsPanel.Copyright ? EndingPostCreditsPanel.None
        : throw new InvalidOperationException("The post-credits text shows no copyright.");

    /// <summary>Starts the item-percentage text.</summary>
    internal void StartItemPercentage(EndingTypewriterSetup setup) =>
        ItemPercentage = new EndingTypewriter(setup, EndingTextSequence.ItemPercentage);

    /// <summary>Starts the final message, below which the item-percentage text stays shown.</summary>
    internal void StartFinalMessage(EndingTypewriterSetup setup) =>
        FinalMessage = new EndingTypewriter(setup, EndingTextSequence.FinalMessage);

    /// <summary>Binds the typed texts' installed text when the game is bound to its installed content.</summary>
    internal void BindText(EndingTypewriterText? value)
    {
        ItemPercentage?.BindText(value);
        FinalMessage?.BindText(value);
    }

    /// <summary>What the screen shows, in drawing order: the panel, the item-percentage text, then the final message.</summary>
    /// <exception cref="InvalidOperationException">The panel is undefined.</exception>
    internal IEnumerable<EndingShownText> Shown()
    {
        switch (Panel)
        {
            case EndingPostCreditsPanel.Result:
                yield return new(EndingPictureNames.ResultPanel, EndingResultTextLayout.ResultPanelCorner);
                break;
            case EndingPostCreditsPanel.Copyright:
                yield return new(EndingPictureNames.CopyrightPanel, EndingResultTextLayout.CopyrightPanelCorner);
                break;
            case EndingPostCreditsPanel.None:
                break;
            default:
                throw new InvalidOperationException($"Undefined post-credits panel {Panel}.");
        }
        foreach (EndingShownText text in ItemPercentage?.Shown() ?? [])
            yield return text;
        foreach (EndingShownText text in FinalMessage?.Shown() ?? [])
            yield return text;
    }
}
