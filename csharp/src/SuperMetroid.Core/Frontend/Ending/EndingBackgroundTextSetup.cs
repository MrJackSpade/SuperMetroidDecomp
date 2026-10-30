using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>What an ending background-text sequence draws and where (#633).</summary>
internal sealed record EndingBackgroundTextSetup
{
    /// <summary>The staging page the sequence writes its text to.</summary>
    public required EndingTextPage Page { get; init; }

    /// <summary>Inventory.</summary>
    public required EndingInventorySnapshot Inventory { get; init; }

    /// <summary>Japanese text.</summary>
    public required bool JapaneseText { get; init; }

    /// <summary>The text page the staging page is shown on after each revealed glyph.</summary>
    public EndingTextTilemap DestinationTilemap { get; init; } = EndingTextTilemap.ResultPanel;

    /// <summary>Presentation.</summary>
    public EndingTypewriterText? Presentation { get; init; } = null;

    /// <summary>Installed sequence.</summary>
    public EndingTextSequence? InstalledSequence { get; init; } = null;
}
