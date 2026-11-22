using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The post-credits text scene (#1555): the panel it shows, the item-percentage and final-message typed texts, and how
/// far the text has been moved down the screen. What the screen shows is derived from this state each frame, each piece at
/// its screen pixel moved down by <see cref="Drop"/>, without wrapping.
/// </summary>
internal sealed partial class EndingPostCreditsText
{
    /// <summary>The panel shown.</summary>
    internal EndingPostCreditsPanel Panel { get; private set; }

    /// <summary>
    /// The item-percentage text (#2541), the first typed text the screen shows: none until the logo's end.
    /// </summary>
    private EndingTypewriter? ItemPercentage { get; set; }

    /// <summary>
    /// The final message (#2541), from the end of the move down on, below which the item percentage stays shown; none
    /// before. The screen draws it after the item percentage, whatever order they were started in (#2553).
    /// </summary>
    private EndingTypewriter? FinalMessage { get; set; }

    /// <summary>Pixels the text is moved down the screen, zero until the item percentage's move down.</summary>
    internal int Drop { get; set; }

    /// <summary>Shows no text: no panel and no typed text, as the credits leave the screen.</summary>
    internal void Clear()
    {
        Panel = EndingPostCreditsPanel.None;
        ItemPercentage = null;
        FinalMessage = null;
    }

    /// <summary>
    /// Shows the panel that follows the one shown, in the order the ending shows them (#2553): the result panel after none,
    /// the copyright in its place, then none once the white flash clears the copyright.
    /// </summary>
    internal void ShowNextPanel() => Panel = NextPanels[Panel];

    /// <summary>The panel that follows each, in the order the ending shows them.</summary>
    private static IdentityTable<EndingPostCreditsPanel, EndingPostCreditsPanel> NextPanels => field ??= new(
        new Dictionary<EndingPostCreditsPanel, EndingPostCreditsPanel>
        {
            [EndingPostCreditsPanel.None] = EndingPostCreditsPanel.Result,
            [EndingPostCreditsPanel.Result] = EndingPostCreditsPanel.Copyright,
            [EndingPostCreditsPanel.Copyright] = EndingPostCreditsPanel.None,
        });

    /// <summary>Starts the item-percentage text, the first typed text the screen shows.</summary>
    /// <returns>The text started.</returns>
    internal EndingTypewriter StartItemPercentage(EndingTypewriterSetup setup) =>
        ItemPercentage = new EndingTypewriter(setup, EndingTextSequence.ItemPercentage);

    /// <summary>Starts the final message, below which the item-percentage text stays shown.</summary>
    /// <returns>The text started.</returns>
    internal EndingTypewriter StartFinalMessage(EndingTypewriterSetup setup) =>
        FinalMessage = new EndingTypewriter(setup, EndingTextSequence.FinalMessage);

    /// <summary>Binds the typed texts' installed text when the game is bound to its installed content.</summary>
    internal void BindText(EndingTypewriterText value)
    {
        ItemPercentage?.BindText(value);
        FinalMessage?.BindText(value);
    }

    /// <summary>The panel each post-credits panel choice shows: the result panel, the copyright panel, or none (#2553).</summary>
    private static IdentityTable<EndingPostCreditsPanel, IReadOnlyList<EndingShownText>> PanelTexts => field ??= new(
        new Dictionary<EndingPostCreditsPanel, IReadOnlyList<EndingShownText>>
        {
            [EndingPostCreditsPanel.Result] = [new(EndingPictureNames.ResultPanel, EndingResultTextLayout.ResultPanelCorner)],
            [EndingPostCreditsPanel.Copyright] = [new(EndingPictureNames.CopyrightPanel, EndingResultTextLayout.CopyrightPanelCorner)],
            [EndingPostCreditsPanel.None] = [],
        });

    /// <summary>What the screen shows, in drawing order: the panel, the item-percentage text, then the final message.</summary>
    internal IEnumerable<EndingShownText> Shown()
    {
        foreach (EndingShownText panel in PanelTexts[Panel])
            yield return panel;
        foreach (EndingTypewriter? text in (EndingTypewriter?[])[ItemPercentage, FinalMessage])
        {
            foreach (EndingShownText shown in text?.Shown() ?? [])
                yield return shown;
        }
    }
}
