using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>What an ending background-text sequence draws and on which staging page (#633).</summary>
internal sealed record EndingBackgroundTextSetup
{
    /// <summary>The staging page the sequence writes its text to.</summary>
    public required EndingTextPage Page { get; init; }

    /// <summary>Inventory.</summary>
    public required EndingInventorySnapshot Inventory { get; init; }

    /// <summary>Japanese text.</summary>
    public required bool JapaneseText { get; init; }

    /// <summary>Presentation.</summary>
    public EndingTypewriterText? Presentation { get; init; } = null;

    /// <summary>Installed sequence.</summary>
    public EndingTextSequence? InstalledSequence { get; init; } = null;
}
