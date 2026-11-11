using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction;

/// <summary>The installed content identities of the artwork bundles the game reads through Core types; only the import pipeline's own bundles carry one (#1502).</summary>
internal static class InstalledArtworkBundleIdentity
{

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
}
