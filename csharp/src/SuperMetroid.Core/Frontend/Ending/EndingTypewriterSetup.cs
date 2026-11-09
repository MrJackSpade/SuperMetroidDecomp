using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>What a post-credits typed text types from and counts (#633, #1555).</summary>
internal sealed record EndingTypewriterSetup
{
    /// <summary>Collected ammo and item counts the item percentage counts.</summary>
    public required EndingInventorySnapshot Inventory { get; init; }

    /// <summary>Whether the Japanese subtitle shows during the percentage hold.</summary>
    public required bool JapaneseText { get; init; }

    /// <summary>The installed ending text.</summary>
    public required EndingTypewriterText Text { get; init; }
}
