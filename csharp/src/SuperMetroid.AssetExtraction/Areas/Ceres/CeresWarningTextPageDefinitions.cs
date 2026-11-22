namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>Where the Ceres warning text's page looks come from among the installed Ceres escape characters (#1463).</summary>
internal static partial class CeresWarningTextPageDefinitions
{
    /// <summary>The warning text's pages: four full pages, then a half page.</summary>
    internal const int PageCount = 5;

    /// <summary>The characters one full warning page holds.</summary>
    internal const int PageCharacters = 16;

    /// <summary>The bytes one four-bit character holds.</summary>
    internal const int CharacterBytes = 32;
}
