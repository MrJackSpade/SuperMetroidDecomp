namespace SuperMetroid.Core.Assets;

/// <summary>
/// What shows over a run of background character words (#1392): the artwork bound there and how far into it the run
/// begins, which together name the bytes a pre-coloured picture of those characters was composed from.
/// </summary>
/// <param name="Artwork">The artwork bound over the words.</param>
/// <param name="WordOffset">The words of that artwork before the run's first.</param>
public readonly record struct BgArtworkShown(BgArtworkIdentity Artwork, int WordOffset);
