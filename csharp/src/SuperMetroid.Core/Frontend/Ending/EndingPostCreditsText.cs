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

    /// <summary>
    /// The typed texts the screen shows, in the order they started (#2541): none until the logo's end, then the item
    /// percentage, then, from the end of the move down on, the final message below which the item percentage stays shown.
    /// </summary>
    private readonly List<EndingTypewriter> typed = [];

    /// <summary>Pixels the text is moved down the screen, zero until the item percentage's move down.</summary>
    internal int Drop { get; set; }

    /// <summary>Shows no text: no panel and no typed text, as the credits leave the screen.</summary>
    internal void Clear()
    {
        Panel = EndingPostCreditsPanel.None;
        typed.Clear();
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

    /// <summary>Starts the item-percentage text, the first typed text the screen shows.</summary>
    /// <returns>The text started.</returns>
    /// <exception cref="InvalidOperationException">A typed text has already started.</exception>
    internal EndingTypewriter StartItemPercentage(EndingTypewriterSetup setup) => Start(setup, EndingTextSequence.ItemPercentage, typedBefore: 0);

    /// <summary>Starts the final message, below which the item-percentage text stays shown.</summary>
    /// <returns>The text started.</returns>
    /// <exception cref="InvalidOperationException">The item percentage alone has not started.</exception>
    internal EndingTypewriter StartFinalMessage(EndingTypewriterSetup setup) => Start(setup, EndingTextSequence.FinalMessage, EndingTypedTextDefinitions.TypedBeforeFinalMessage);

    /// <summary>Starts typing <paramref name="sequence"/> after the <paramref name="typedBefore"/> texts that type before it.</summary>
    /// <exception cref="InvalidOperationException">The screen does not show exactly those texts.</exception>
    private EndingTypewriter Start(EndingTypewriterSetup setup, EndingTextSequence sequence, int typedBefore)
    {
        if (typed.Count != typedBefore)
            throw new InvalidOperationException($"The {sequence} text starts after {typedBefore} typed texts, not {typed.Count}.");
        var started = new EndingTypewriter(setup, sequence);
        typed.Add(started);
        return started;
    }

    /// <summary>Binds the typed texts' installed text when the game is bound to its installed content.</summary>
    internal void BindText(EndingTypewriterText value)
    {
        foreach (EndingTypewriter text in typed)
            text.BindText(value);
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
        foreach (EndingTypewriter text in typed)
        {
            foreach (EndingShownText shown in text.Shown())
                yield return shown;
        }
    }
}
