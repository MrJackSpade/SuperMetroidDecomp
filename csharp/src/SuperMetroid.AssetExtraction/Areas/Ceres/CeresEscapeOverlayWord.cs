namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>One word of one Ceres escape overlay page.</summary>
/// <param name="Page">The page's installed name.</param>
/// <param name="Index">The word's position in the page.</param>
internal readonly record struct CeresEscapeOverlayWord(string Page, int Index);
