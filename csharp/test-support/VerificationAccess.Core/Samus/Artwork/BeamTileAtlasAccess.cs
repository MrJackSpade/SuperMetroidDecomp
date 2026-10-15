using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Verification access to <see cref="BeamTileAtlas"/> members production does not use.</summary>
internal static class BeamTileAtlasAccess
{
    /// <summary>Loads an installed beam tile transfer into its native VRAM destination.</summary>
    extension(BeamTileAtlas self)
    {
        /// <summary>Writes the atlas transfer bytes at the beam tile base word.</summary>
        [PpuLayoutBoundary("codec: beam tile transfer at its native VRAM word")]
        internal void LoadTo(SnesVram vram) =>
            vram.LoadBytes(BeamTileAtlasDefinitions.DestinationWord * 2, self.Transfer.Span);
    }
}
