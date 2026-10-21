namespace SuperMetroid.Core.Areas;

/// <summary>Finds the installed area artwork a resolver carries.</summary>
internal static class InstalledAreaObjCharacterArtwork
{
    /// <summary>The installed area artwork <paramref name="resolver"/> resolves through.</summary>
    /// <exception cref="InvalidOperationException">The resolver holds no installed area artwork.</exception>
    internal static IInstalledAreaObjCharacterArtwork Of(IObjCharacterSourceResolver resolver) =>
        Ensure.NotNull(resolver) as IInstalledAreaObjCharacterArtwork ?? throw new InvalidOperationException(
            $"{resolver.GetType().Name} holds no installed area artwork to resolve a named OBJ character source.");
}
