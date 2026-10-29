using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The import pipeline's view of the catalogs a Samus body binds (#1463). The import pipeline builds every installed
/// body from its own source catalogs, which hold the compositions, appearances and tiles the sheets are composed
/// from; gameplay holds them as the runtime catalogs. A catalog built elsewhere has no sources and is rejected.
/// </summary>
internal static class SamusBodySourceCatalogs
{
    /// <summary>The pose-half table's compositions.</summary>
    extension(InstalledSamusSpritemaps self)
    {
        /// <summary>The table with the compositions the import pipeline built it from.</summary>
        /// <exception cref="InvalidDataException">The table was not built by the import pipeline.</exception>
        internal SamusSpritemapSources Source => self as SamusSpritemapSources ??
            throw new InvalidDataException("Samus spritemaps were not built from installed compositions.");
    }

    /// <summary>The arm cannon's descriptors, appearances and tiles.</summary>
    extension(InstalledSamusArmCannonArtwork self)
    {
        /// <summary>The artwork with the descriptors, appearances and tiles the import pipeline built it from.</summary>
        /// <exception cref="InvalidDataException">The artwork was not built by the import pipeline.</exception>
        internal SamusArmCannonSourceArtwork Source => self as SamusArmCannonSourceArtwork ??
            throw new InvalidDataException("Arm-cannon artwork was not built from installed descriptors.");
    }

    /// <summary>The atmospheric effects' appearances.</summary>
    extension(InstalledSamusAtmosphericArtwork self)
    {
        /// <summary>The artwork with the appearances the import pipeline built it from.</summary>
        /// <exception cref="InvalidDataException">The artwork was not built by the import pipeline.</exception>
        internal SamusAtmosphericSourceArtwork Source => self as SamusAtmosphericSourceArtwork ??
            throw new InvalidDataException("Atmospheric artwork was not built from installed appearances.");
    }
}
