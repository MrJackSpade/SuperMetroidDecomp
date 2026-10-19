namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Reaches the installed gameplay artwork through an OBJ character source resolver.</summary>
internal static class InstalledObjCharacterArtwork
{
    /// <summary>The installed gameplay artwork <paramref name="resolver"/> resolves through.</summary>
    /// <exception cref="InvalidOperationException">The resolver holds no installed gameplay artwork.</exception>
    internal static IInstalledObjCharacterArtwork Of(IObjCharacterSourceResolver resolver) =>
        Ensure.NotNull(resolver) as IInstalledObjCharacterArtwork ?? throw new InvalidOperationException(
            $"{resolver.GetType().Name} holds no installed gameplay artwork to resolve a named OBJ character source.");
}
