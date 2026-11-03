using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction;

/// <summary>The installed content identities of the artwork bundles the game reads through Core types; only the import pipeline's own bundles carry one (#1502).</summary>
internal static class InstalledArtworkBundleIdentity
{

    /// <summary>The installed content identity of the Ceres flight artwork.</summary>
    extension(CeresFlightArtwork artwork)
    {
        /// <summary>The identity the import pipeline computed for <paramref name="artwork"/>, which it read as a <see cref="CeresFlightCharacters"/>.</summary>
        /// <exception cref="InvalidDataException">The Ceres flight artwork was not read from installed artwork.</exception>
        public string ContentIdentity => artwork is CeresFlightCharacters installed ? installed.ContentIdentity
            : throw new InvalidDataException("The Ceres flight artwork was not read from installed artwork.");
    }

    /// <summary>The installed content identity of the ending floor artwork.</summary>
    extension(EndingFloorArtwork artwork)
    {
        /// <summary>The identity the import pipeline computed for <paramref name="artwork"/>, which it read as a <see cref="EndingFloorCharacters"/>.</summary>
        /// <exception cref="InvalidDataException">The ending floor artwork was not read from installed artwork.</exception>
        public string ContentIdentity => artwork is EndingFloorCharacters installed ? installed.ContentIdentity
            : throw new InvalidDataException("The ending floor artwork was not read from installed artwork.");
    }

    /// <summary>The installed content identity of the ending object artwork.</summary>
    extension(EndingObjectArtwork artwork)
    {
        /// <summary>The identity the import pipeline computed for <paramref name="artwork"/>, which it read as a <see cref="EndingObjectSourceArtwork"/>.</summary>
        /// <exception cref="InvalidDataException">The ending object artwork was not read from installed artwork.</exception>
        public string ContentIdentity => artwork is EndingObjectSourceArtwork installed ? installed.ContentIdentity
            : throw new InvalidDataException("The ending object artwork was not read from installed artwork.");
    }

    /// <summary>The installed content identity of the intro artwork.</summary>
    extension(IntroCinematicArtwork artwork)
    {
        /// <summary>The identity the import pipeline computed for <paramref name="artwork"/>, which it read as a <see cref="IntroCinematicSourceArtwork"/>.</summary>
        /// <exception cref="InvalidDataException">The intro artwork was not read from installed artwork.</exception>
        public string ContentIdentity => artwork is IntroCinematicSourceArtwork installed ? installed.ContentIdentity
            : throw new InvalidDataException("The intro artwork was not read from installed artwork.");
    }

    /// <summary>The installed content identity of the Samus arm-cannon artwork.</summary>
    extension(InstalledSamusArmCannonArtwork artwork)
    {
        /// <summary>The identity the import pipeline computed for <paramref name="artwork"/>, which it read as a <see cref="SamusArmCannonSourceArtwork"/>.</summary>
        /// <exception cref="InvalidDataException">The Samus arm-cannon artwork was not read from installed artwork.</exception>
        public string ContentIdentity => artwork is SamusArmCannonSourceArtwork installed ? installed.ContentIdentity
            : throw new InvalidDataException("The Samus arm-cannon artwork was not read from installed artwork.");
    }

    /// <summary>The installed content identity of the Samus atmospheric artwork.</summary>
    extension(InstalledSamusAtmosphericArtwork artwork)
    {
        /// <summary>The identity the import pipeline computed for <paramref name="artwork"/>, which it read as a <see cref="SamusAtmosphericSourceArtwork"/>.</summary>
        /// <exception cref="InvalidDataException">The Samus atmospheric artwork was not read from installed artwork.</exception>
        public string ContentIdentity => artwork is SamusAtmosphericSourceArtwork installed ? installed.ContentIdentity
            : throw new InvalidDataException("The Samus atmospheric artwork was not read from installed artwork.");
    }

    /// <summary>The installed content identity of the Samus body artwork.</summary>
    extension(InstalledSamusBodyArtwork artwork)
    {
        /// <summary>The identity the import pipeline computed for <paramref name="artwork"/>, which it read as a <see cref="SamusBodySources"/>.</summary>
        /// <exception cref="InvalidDataException">The Samus body artwork was not read from installed artwork.</exception>
        public string ContentIdentity => artwork is SamusBodySources installed ? installed.ContentIdentity
            : throw new InvalidDataException("The Samus body artwork was not read from installed artwork.");
    }

    /// <summary>The installed content identity of the Samus spritemaps.</summary>
    extension(InstalledSamusSpritemaps artwork)
    {
        /// <summary>The identity the import pipeline computed for <paramref name="artwork"/>, which it read as a <see cref="SamusSpritemapSources"/>.</summary>
        /// <exception cref="InvalidDataException">The Samus spritemaps was not read from installed artwork.</exception>
        public string ContentIdentity => artwork is SamusSpritemapSources installed ? installed.ContentIdentity
            : throw new InvalidDataException("The Samus spritemaps was not read from installed artwork.");
    }
}
