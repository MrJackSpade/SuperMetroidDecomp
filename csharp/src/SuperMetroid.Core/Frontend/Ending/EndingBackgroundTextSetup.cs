using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>What an ending background-text sequence draws and where (#633).</summary>
internal sealed record EndingBackgroundTextSetup
{
    /// <summary>Tilemap.</summary>
    public required BackgroundTile[] Tilemap { get; init; }

    /// <summary>Inventory.</summary>
    public required EndingInventorySnapshot Inventory { get; init; }

    /// <summary>Japanese text.</summary>
    public required bool JapaneseText { get; init; }

    /// <summary>The background map the staging tilemap is uploaded into after each revealed glyph.</summary>
    public EndingTextTilemap DestinationTilemap { get; init; } = EndingTextTilemap.ResultPanel;

    /// <summary>Presentation.</summary>
    public EndingTextPresentation? Presentation { get; init; } = null;

    /// <summary>Installed sequence.</summary>
    public EndingTextSequence? InstalledSequence { get; init; } = null;
}
