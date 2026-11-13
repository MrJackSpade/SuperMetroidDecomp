using SuperMetroid.Core;
using SuperMetroid.Core.Frontend;
using SuperMetroid.AssetExtraction.Installation;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>Binds one installation's host presentation catalogs to bare <see cref="SuperMetroidGame"/> fixtures.</summary>
internal static class InstalledGameBindings
{
    /// <summary>Loads every game catalog once; the returned action binds them to each new game.</summary>
    internal static Action<SuperMetroidGame> Create(GameInstallation installation)
    {
        GameArtwork artwork = Artwork(installation);
        return game => game.BindArtwork(artwork);
    }

    /// <summary>
    /// Every catalog a host binds from <paramref name="installation"/>: the process's shared catalogs for the repository
    /// installation, otherwise a fresh load. For the repository installation, the map presentation, the opening and the
    /// ending objects are the import's reads of the same documents, which the game reads alike, with the characters the
    /// checks compose memory images from: those are import data no installation holds (#1636).
    /// </summary>
    internal static GameArtwork Artwork(GameInstallation installation)
    {
        Ensure.NotNull(installation);
        return ReferenceEquals(installation, RepositoryInstallation.Installation)
            ? RepositoryInstallation.Artwork with
            {
                MapPresentation = InstalledFixtures.Maps,
                IntroCinematicArt = InstalledFixtures.IntroArtwork.BoundCatalog,
            }
            : installation.LoadGameArtwork(installation.LoadMaps(), installation.LoadProjectiles(), installation.LoadEnemyTiles(), installation.LoadSamusBody());
    }
}
