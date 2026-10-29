using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>Verification access to <see cref="NorfairRidleyColorCatalog"/> members production does not use.</summary>
internal static class NorfairRidleyColorCatalogAccess
{
    /// <summary>Inspects initial and reveal paint sequences held by the installed catalog.</summary>
    extension(NorfairRidleyColorCatalog self)
    {
        /// <summary>Reads a pre-reveal Ridley color.</summary>
        internal RgbColor ResolveInitial(int color) => self.selection.Initial.ColorAt(color);

        /// <summary>Reads a step and color from Ridley's reveal paint sequence.</summary>
        internal RgbColor ResolveReveal(int step, int color) => self.selection.Reveal.ColorAt(step, color);
    }
}
