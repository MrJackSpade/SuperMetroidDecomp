using System.Runtime.CompilerServices;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Verification access (#1616): the import's source artwork each opening-cinematic and Ceres-flight catalog a fixture binds
/// was built from. The game binds only the catalog, its backdrops and placements; checks that compose the cartridge's memory
/// image of a scene read the pictures' characters and maps from the import's artwork the bound catalog came from.
/// </summary>
internal static class CinematicSourceBindings
{
    /// <summary>The import's opening artwork by the catalog a fixture bound from it.</summary>
    private static readonly ConditionalWeakTable<IntroCinematicArtwork, IntroCinematicSourceArtwork> intros = new();

    /// <summary>The import's Ceres flight artwork by the catalog a fixture bound from it.</summary>
    private static readonly ConditionalWeakTable<CeresFlightArtwork, CeresFlightCharacters> flights = new();

    extension(IntroCinematicSourceArtwork self)
    {
        /// <summary>The catalog the game binds of this artwork, recorded so checks of a scene bound to it can read this artwork.</summary>
        internal IntroCinematicArtwork BoundCatalog
        {
            get
            {
                intros.AddOrUpdate(self.Cinematic, self);
                flights.AddOrUpdate(self.Cinematic.CeresFlight, self.CeresFlight);
                return self.Cinematic;
            }
        }
    }

    extension(CeresFlightCharacters self)
    {
        /// <summary>The catalog the game binds of this artwork, recorded so checks of a scene bound to it can read this artwork.</summary>
        internal CeresFlightArtwork BoundCatalog
        {
            get
            {
                flights.AddOrUpdate(self.Flight, self);
                return self.Flight;
            }
        }
    }

    extension(IntroCinematicArtwork self)
    {
        /// <summary>The import's artwork this bound catalog was built from.</summary>
        /// <exception cref="InvalidOperationException">
        /// No fixture bound the catalog from the import's artwork, and it is not the repository installation's own catalog.
        /// </exception>
        internal IntroCinematicSourceArtwork ImportSource => intros.TryGetValue(self, out IntroCinematicSourceArtwork? source) ? source
            : IsRepositoryArtwork(self) ? InstalledFixtures.IntroArtwork
            : throw new InvalidOperationException("The opening catalog was not bound from the import's artwork by a fixture.");
    }

    extension(CeresFlightArtwork self)
    {
        /// <summary>The import's artwork this bound catalog was built from.</summary>
        /// <exception cref="InvalidOperationException">
        /// No fixture bound the catalog from the import's artwork, and it is not the repository installation's own catalog.
        /// </exception>
        internal CeresFlightCharacters ImportSource => flights.TryGetValue(self, out CeresFlightCharacters? source) ? source
            : IsRepositoryArtwork(self) ? InstalledFixtures.IntroArtwork.CeresFlight
            : throw new InvalidOperationException("The Ceres flight catalog was not bound from the import's artwork by a fixture.");
    }

    /// <summary>
    /// Whether <paramref name="catalog"/> is the repository installation's own opening catalog, or its Ceres flight: the game's
    /// read of the installation whose import artwork is <see cref="InstalledFixtures.IntroArtwork"/>.
    /// </summary>
    private static bool IsRepositoryArtwork(object catalog) => RepositoryInstallation.ArtworkLoader.IsValueCreated &&
        RepositoryInstallation.Artwork.IntroCinematicArt is { } repository &&
        (ReferenceEquals(catalog, repository) || ReferenceEquals(catalog, repository.CeresFlight));
}
