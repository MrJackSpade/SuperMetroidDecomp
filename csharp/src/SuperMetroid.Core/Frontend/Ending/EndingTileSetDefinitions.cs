namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Where the ending's character uploads sit among each tile layer's tile numbers (#1404).</summary>
internal static class EndingTileSetDefinitions
{
    /// <summary>The credits font's first tile on the credits and post-credits text layers.</summary>
    internal const int FontFirstTile = 0;

    /// <summary>The subtitle font's first tile on the post-credits text layer, after the credits font.</summary>
    internal const int SubtitleFirstTile = 128;

    /// <summary>The waiting backdrop's first tile on the post-credits text layer.</summary>
    internal const int TextWaitingFirstTile = 256;

    /// <summary>The logo's first tile on the post-credits text layer.</summary>
    internal const int TextLogoFirstTile = 512;

    /// <summary>The waiting backdrop's first tile on the waiting layer.</summary>
    internal const int WaitingFirstTile = 0;

    /// <summary>The logo's first tile on the waiting layer, after the waiting backdrop.</summary>
    internal const int WaitingLogoFirstTile = 256;

    /// <summary>The logo's first tile on the logo layer.</summary>
    internal const int LogoFirstTile = 0;

    /// <summary>The wavy Samus characters' first tile on the wavy Samus layer.</summary>
    internal const int WavySamusFirstTile = 0;

    /// <summary>The explosion characters' first tile on the explosion layer.</summary>
    internal const int ExplosionFirstTile = 0;
}
