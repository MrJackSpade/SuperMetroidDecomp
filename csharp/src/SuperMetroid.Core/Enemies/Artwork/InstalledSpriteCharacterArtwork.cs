namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Reaches the installed gameplay artwork through a sprite character source resolver.</summary>
internal static class InstalledSpriteCharacterArtwork
{
    /// <summary>The installed gameplay artwork <paramref name="resolver"/> resolves through.</summary>
    /// <exception cref="InvalidOperationException">The resolver holds no installed gameplay artwork.</exception>
    internal static IInstalledSpriteCharacterArtwork Of(IObjCharacterSourceResolver resolver) =>
        Ensure.NotNull(resolver) as IInstalledSpriteCharacterArtwork ?? throw new InvalidOperationException(
            $"{resolver.GetType().Name} holds no installed gameplay artwork to resolve a named OBJ character source.");
}
