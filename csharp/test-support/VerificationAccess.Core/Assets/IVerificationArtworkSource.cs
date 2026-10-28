namespace SuperMetroid.Core.Assets;

/// <summary>
/// A sprite artwork source only verification makes (#1422), which resolves its own characters because the import
/// pipeline's resolution knows only the game's sources.
/// </summary>
internal interface IVerificationArtworkSource
{
    /// <summary>The characters the source names, as <paramref name="resolver"/> holds them.</summary>
    /// <param name="resolver">The installed artwork.</param>
    ReadOnlyMemory<byte> ResolveCharacters(IInstalledArtworkContent resolver);
}
