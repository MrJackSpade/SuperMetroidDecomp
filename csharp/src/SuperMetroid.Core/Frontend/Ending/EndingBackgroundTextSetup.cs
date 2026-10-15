using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>What an ending background-text sequence draws and where (#633).</summary>
internal sealed record EndingBackgroundTextSetup
{
    /// <summary>Tilemap.</summary>
    public required ushort[] Tilemap { get; init; }

    /// <summary>Instruction pointer.</summary>
    public required ushort InstructionPointer { get; init; }

    /// <summary>Inventory.</summary>
    public required EndingInventorySnapshot Inventory { get; init; }

    /// <summary>Japanese text.</summary>
    public required bool JapaneseText { get; init; }

    /// <summary>Tilemap destination.</summary>
    public ushort TilemapDestination { get; init; } = EndingCreditsRomData.Rendering.PostCreditsTilemapWord;

    /// <summary>Presentation.</summary>
    public EndingTextPresentation? Presentation { get; init; } = null;

    /// <summary>Installed sequence.</summary>
    public EndingTextSequence? InstalledSequence { get; init; } = null;
}
