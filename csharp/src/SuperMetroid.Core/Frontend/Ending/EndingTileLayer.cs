namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The ending's tile layers, by the tiles each reads and the scene colours it shows them in (#1404).</summary>
internal enum EndingTileLayer
{
    /// <summary>The credits roll, in the credits colours.</summary>
    Credits,
    /// <summary>The result, copyright and final text, in the post-credits colours.</summary>
    PostCreditsText,
    /// <summary>The waiting backdrop, in the post-credits colours.</summary>
    Waiting,
    /// <summary>The Super Metroid logo, in the post-credits colours.</summary>
    Logo,
    /// <summary>The wavy Samus backdrop, in the post-credits colours.</summary>
    WavySamus,
    /// <summary>The Zebes explosion, in the explosion colours.</summary>
    Explosion,
}
