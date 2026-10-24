namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>One contiguous source sheet whose pages Mother Brain's encounter shows its sprites one at a time.</summary>
/// <param name="FileName">The installed sheet's file name.</param>
/// <param name="SourceAddress">Where the sheet's characters begin in the cartridge, which names the installed sheet.</param>
/// <param name="PageCount">The sheet's pages.</param>
/// <param name="FirstCharacter">The sprite character the sheet's first page shows from; each later page follows the one before.</param>
public readonly record struct MotherBrainSpecialSpriteSheetDefinition(
    string FileName, int SourceAddress, int PageCount, int FirstCharacter)
{
    /// <summary>Every page holds sixteen characters, $200 bytes.</summary>
    public const ushort PageByteCount = 0x0200;

    /// <summary>The sprite characters one page shows.</summary>
    public const int CharactersPerPage = 16;

    /// <summary>Total tile-aligned source length encoded by this sheet.</summary>
    public int ByteCount => checked(PageCount * PageByteCount);

    /// <summary>The sprite character page <paramref name="page"/> shows from.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The sheet has no such page.</exception>
    public int PageFirstCharacter(int page) => (uint)page < PageCount
        ? FirstCharacter + page * CharactersPerPage
        : throw new ArgumentOutOfRangeException(nameof(page));
}
