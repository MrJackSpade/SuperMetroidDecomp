using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// One of the ending's character and map transfers the import executes once to compose the pages its sprite sheets are
/// cut from (#1467); the game selects the composed sheets by scene state and keeps no transfer history. Each transfer is
/// its own case and carries what it loads (#2554).
/// </summary>
internal abstract record EndingTransfer
{
    /// <summary>The escape cloud OBJ sheet.</summary>
    internal sealed record EscapeClouds : EndingTransfer;

    /// <summary>The explosion and flyaway OBJ sheet.</summary>
    internal sealed record ExplosionObjects : EndingTransfer;

    /// <summary>One explosion map, which shares the OBJ page's last characters.</summary>
    /// <param name="Fragment">The explosion map the transfer loads.</param>
    internal sealed record ExplosionMap(EndingExplosionPicture Fragment) : EndingTransfer;

    /// <summary>The ending font's OBJ characters loaded after the explosion sheet.</summary>
    internal sealed record EndingFont : EndingTransfer;

    /// <summary>The waiting, shooting and reward characters with the post-credits maps.</summary>
    internal sealed record CreditsCharacterArt : EndingTransfer;

    /// <summary>One of the post-shot's subtitle and logo parts.</summary>
    /// <param name="Chunk">The post-shot part the transfer makes.</param>
    internal sealed record PostShotPart(int Chunk) : EndingTransfer;
}
