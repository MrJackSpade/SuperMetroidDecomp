namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// Where the ending's character loads sit among each post-credits background layer's character numbers (#1404), which
/// the import reads its maps against when it composes the ending's pictures (#1467).
/// </summary>
internal static class EndingTextLayerDefinitions
{
    /// <summary>The credits font's first character on the post-credits text layer.</summary>
    internal const int FontFirstCharacter = 0;

    /// <summary>The subtitle font's first character on the post-credits text layer, after the credits font.</summary>
    internal const int SubtitleFirstCharacter = 128;

    /// <summary>The waiting backdrop's first character on the post-credits text layer.</summary>
    internal const int TextWaitingFirstCharacter = 256;

    /// <summary>The logo's first character on the post-credits text layer.</summary>
    internal const int TextLogoFirstCharacter = 512;

    /// <summary>The waiting backdrop's first character on the waiting layer.</summary>
    internal const int WaitingFirstCharacter = 0;

    /// <summary>The logo's first character on the logo layer.</summary>
    internal const int LogoFirstCharacter = 0;
}
