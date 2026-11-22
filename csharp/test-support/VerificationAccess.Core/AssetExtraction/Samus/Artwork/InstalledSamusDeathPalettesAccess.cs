using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>Reads the death step each picture of Samus's death explosion shows.</summary>
internal static class InstalledSamusDeathPalettesAccess
{
    /// <summary>Reads the explosion's steps.</summary>
    extension(InstalledSamusDeathPalettes self)
    {
        /// <summary>The death step the explosion's picture at <paramref name="frame"/> of its animation shows.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The explosion has no such picture.</exception>
        internal int ExplosionPaletteIndex(int frame) => (uint)frame < (uint)SamusDeathSheetDefinitions.Explosion.Count
            ? self.Steps.ExplosionShade(SamusDeathSheetDefinitions.Explosion[frame]).Step
            : throw new ArgumentOutOfRangeException(nameof(frame));
    }
}
