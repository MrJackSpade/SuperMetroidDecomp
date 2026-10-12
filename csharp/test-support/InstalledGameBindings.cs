using SuperMetroid.Core;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;

/// <summary>Binds one installation's host presentation catalogs to bare <see cref="SuperMetroidGame"/> fixtures.</summary>
internal static class InstalledGameBindings
{
    /// <summary>Loads every game catalog once; the returned action binds them to each new game.</summary>
    internal static Action<SuperMetroidGame> Create(GameInstallation installation)
    {
        Ensure.NotNull(installation);
        GameArtwork artwork = installation.LoadGameArtwork(installation.LoadMaps(), installation.LoadProjectiles());
        return game => game.BindArtwork(artwork);
    }
}
