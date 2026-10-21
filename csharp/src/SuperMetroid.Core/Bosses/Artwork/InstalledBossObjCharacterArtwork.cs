namespace SuperMetroid.Core.Bosses.Artwork;

/// <summary>Finds the installed boss artwork a resolver carries.</summary>
internal static class InstalledBossObjCharacterArtwork
{
    /// <summary>The installed boss artwork <paramref name="resolver"/> resolves through.</summary>
    /// <exception cref="InvalidOperationException">The resolver holds no installed boss artwork.</exception>
    internal static IInstalledBossObjCharacterArtwork Of(IObjCharacterSourceResolver resolver) =>
        Ensure.NotNull(resolver) as IInstalledBossObjCharacterArtwork ?? throw new InvalidOperationException(
            $"{resolver.GetType().Name} holds no installed boss artwork to resolve a named OBJ character source.");
}
