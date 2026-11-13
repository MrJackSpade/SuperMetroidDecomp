using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.AssetExtraction;

/// <summary>The installed content identities of the artwork bundles the game reads through Core types; only the import pipeline's own bundles carry one (#1502).</summary>
internal static class InstalledArtworkBundleIdentity
{

    /// <summary>The installed content identity of the Ceres flight artwork.</summary>
    extension(CeresFlightArtwork artwork)
    {
        /// <summary>The identity the import pipeline computed for <paramref name="artwork"/>, which it read as a <see cref="CeresFlightSourceArtwork"/>.</summary>
        /// <exception cref="InvalidDataException">The Ceres flight artwork was not read from installed artwork.</exception>
        public string ContentIdentity => artwork is CeresFlightSourceArtwork installed ? installed.ContentIdentity
            : throw new InvalidDataException("The Ceres flight artwork was not read from installed artwork.");
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
}
