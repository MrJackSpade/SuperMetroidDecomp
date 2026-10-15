using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>Verification access to <see cref="NorfairRidleyColorCatalog"/> members production does not use.</summary>
internal static class NorfairRidleyColorCatalogAccess
{
    /// <summary>Inspects initial and reveal paint sequences held by the installed catalog.</summary>
    extension(NorfairRidleyColorCatalog self)
    {
        /// <summary>Reads a pre-reveal Ridley color.</summary>
        internal Bgr555 ResolveInitial(int color) => self.initial.ColorAt(color);

        /// <summary>Reads a row and color from Ridley's reveal paint sequence.</summary>
        internal Bgr555 ResolveReveal(int row, int color) => self.reveal.ColorAt(row, color);
    }
}
