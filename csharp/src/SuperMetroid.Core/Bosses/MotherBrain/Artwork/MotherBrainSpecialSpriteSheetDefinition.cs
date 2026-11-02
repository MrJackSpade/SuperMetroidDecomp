namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// One contiguous source sheet whose pages Mother Brain's encounter shows its sprites one at a time. Which sheet it is,
/// where its pages show and their byte geometry are the import's (#1502).
/// </summary>
/// <param name="FileName">The installed sheet's file name.</param>
/// <param name="PageCount">The sheet's pages.</param>
public readonly record struct MotherBrainSpecialSpriteSheetDefinition(string FileName, int PageCount);
