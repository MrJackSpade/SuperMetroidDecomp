using SuperMetroid.AssetExtraction.Samus;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Verification access to the compositions <see cref="SamusSpritemapSources"/> decoded, which checks compare with the cartridge.</summary>
internal static class SamusSpritemapSourcesAccess
{
    /// <summary>Inspects decoded Samus compositions.</summary>
    extension(SamusSpritemapSources self)
    {
        /// <summary>The composition <paramref name="frame"/> shows, or none for a frame that draws nothing.</summary>
        /// <exception cref="InvalidOperationException">The half frame is not decoded.</exception>
        internal SamusSpritemapDefinition? Definition(SamusBodyHalfFrame frame) =>
            self.halves.TryGetValue(frame, out SamusSpritemapId? composition)
                ? composition is { } shown ? self.definitions[shown] : null
                : throw new InvalidOperationException($"Samus {frame} is not decoded.");

        /// <summary>The composition the suit-explosion sheet frame <paramref name="name"/> shows, or none.</summary>
        internal SamusSpritemapDefinition? ExplosionDefinition(string name) =>
            self.explosion[name] is { } shown ? self.definitions[shown] : null;

        /// <summary>The composition the diving-splash or bubbles sheet frame <paramref name="name"/> shows, or none.</summary>
        internal SamusSpritemapDefinition? AtmosphereDefinition(string name) =>
            self.atmosphere[name] is { } shown ? self.definitions[shown] : null;
    }
}
