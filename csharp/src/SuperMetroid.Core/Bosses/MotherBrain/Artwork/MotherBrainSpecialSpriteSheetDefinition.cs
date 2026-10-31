namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>One contiguous source sheet whose pages Mother Brain's encounter shows its sprites one at a time.</summary>
/// <param name="Sheet">The sheet this is.</param>
/// <param name="FileName">The installed sheet's file name.</param>
/// <param name="PageCount">The sheet's pages.</param>
/// <param name="FirstCharacter">The sprite character the sheet's first page shows from; each later page follows the one before.</param>
public readonly record struct MotherBrainSpecialSpriteSheetDefinition(
    MotherBrainSpritePageSheet Sheet, string FileName, int PageCount, int FirstCharacter)
{
    /// <summary>Every page holds sixteen characters, $200 bytes.</summary>
    public const ushort PageByteCount = 512;

    /// <summary>The sprite characters one page shows.</summary>
    public const int CharactersPerPage = 16;

    /// <summary>Total tile-aligned source length encoded by this sheet.</summary>
    public int ByteCount => checked(PageCount * PageByteCount);
}
